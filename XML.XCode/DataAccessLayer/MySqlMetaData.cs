using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core;
using XML.XCode.Configuration;

namespace XML.XCode.DataAccessLayer;

/// <summary>MySql元数据</summary>
internal class MySqlMetaData : RemoteDbMetaData
{
    public MySqlMetaData() => Types = _DataTypes;

    #region 数据类型
    protected override List<KeyValuePair<Type, Type>> FieldTypeMaps
    {
        get
        {
            if (_FieldTypeMaps == null)
            {
                var list = base.FieldTypeMaps;
                if (!list.Any(e => e.Key == typeof(Byte) && e.Value == typeof(Boolean)))
                    list.Add(new KeyValuePair<Type, Type>(typeof(Byte), typeof(Boolean)));
            }
            return base.FieldTypeMaps;
        }
    }

    /// <summary>数据类型映射</summary>
    private static readonly Dictionary<Type, String[]> _DataTypes = new()
        {
            { typeof(Byte[]), new String[] { "BLOB", "TINYBLOB", "MEDIUMBLOB", "LONGBLOB", "binary({0})", "varbinary({0})" } },
            //{ typeof(TimeSpan), new String[] { "TIME" } },
            //{ typeof(SByte), new String[] { "TINYINT" } },
            { typeof(Byte), new String[] { "TINYINT", "TINYINT UNSIGNED" } },
            { typeof(Int16), new String[] { "SMALLINT", "SMALLINT UNSIGNED" } },
            //{ typeof(UInt16), new String[] { "SMALLINT UNSIGNED" } },
            { typeof(Int32), new String[] { "INT", "YEAR", "MEDIUMINT", "MEDIUMINT UNSIGNED", "INT UNSIGNED" } },
            //{ typeof(UInt32), new String[] { "MEDIUMINT UNSIGNED", "INT UNSIGNED" } },
            { typeof(Int64), new String[] { "BIGINT", "BIT", "BIGINT UNSIGNED" } },
            //{ typeof(UInt64), new String[] { "BIT", "BIGINT UNSIGNED" } },
            { typeof(Single), new String[] { "FLOAT" } },
            { typeof(Double), new String[] { "DOUBLE" } },
            { typeof(Decimal), new String[] { "DECIMAL({0}, {1})" } },
            { typeof(DateTime), new String[] { "DATETIME", "DATE", "TIMESTAMP", "TIME" } },
            // mysql中nvarchar会变成utf8字符集的varchar，而不会取数据库的utf8mb4
            { typeof(String), new String[] { "VARCHAR({0})", "LONGTEXT", "TEXT", "CHAR({0})", "NCHAR({0})", "NVARCHAR({0})", "SET", "ENUM", "TINYTEXT", "TEXT", "MEDIUMTEXT" } },
            { typeof(Boolean), new String[] { "TINYINT" } },
        };
    #endregion

    #region 架构
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        var ss = Database.CreateSession();
        var db = Database.DatabaseName;
        var list = new List<IDataTable>();

        var old = ss.ShowSQL;
        ss.ShowSQL = false;
        try
        {
            var sql = $"SHOW TABLE STATUS FROM `{db}`";
            var dt = ss.Query(sql, null);
            if (dt.Rows.Count == 0) return null;

            var hs = new HashSet<String>(names ?? new String[0], StringComparer.OrdinalIgnoreCase);

            // 所有表
            foreach (var dr in dt)
            {
                var name = dr["Name"] + "";
                if (name.IsNullOrEmpty() || hs.Count > 0 && !hs.Contains(name)) continue;

                var table = DAL.CreateTable();
                table.TableName = name;
                table.Description = dr["Comment"] + "";
                table.DbType = Database.Type;

                #region 字段
                sql = $"SHOW FULL COLUMNS FROM `{db}`.`{name}`";
                var dcs = ss.Query(sql, null);
                foreach (var dc in dcs)
                {
                    var field = table.CreateColumn();

                    field.ColumnName = dc["Field"] + "";
                    field.RawType = dc["Type"] + "";
                    field.DataType = GetDataType(field.RawType);
                    field.Description = dc["Comment"] + "";

                    if (dc["Extra"] + "" == "auto_increment") field.Identity = true;
                    if (dc["Key"] + "" == "PRI") field.PrimaryKey = true;
                    if (dc["Null"] + "" == "YES") field.Nullable = true;

                    field.Length = field.RawType.Substring("(", ")").ToInt();

                    if (field.DataType == null)
                    {
                        if (field.RawType.StartsWithIgnoreCase("varchar", "nvarchar")) field.DataType = typeof(String);
                    }

                    // MySql中没有布尔型，这里处理YN枚举作为布尔型
                    if (field.RawType is "enum('N','Y')" or "enum('Y','N')") field.DataType = typeof(Boolean);

                    field.Fix();

                    table.Columns.Add(field);
                }
                #endregion

                #region 索引
                sql = $"SHOW INDEX FROM `{db}`.`{name}`";
                var dis = ss.Query(sql, null);
                foreach (var dr2 in dis)
                {
                    var dname = dr2["Key_name"] + "";
                    var di = table.Indexes.FirstOrDefault(e => e.Name == dname) ?? table.CreateIndex();
                    di.Name = dname;
                    di.Unique = dr2.Get<Int32>("Non_unique") == 0;

                    var cname = dr2.Get<String>("Column_name");
                    var cs = new List<String>();
                    if (di.Columns != null && di.Columns.Length > 0) cs.AddRange(di.Columns);
                    cs.Add(cname);
                    di.Columns = cs.ToArray();

                    table.Indexes.Add(di);
                }
                #endregion

                // 修正关系数据
                table.Fix();

                list.Add(table);
            }
        }
        finally
        {
            ss.ShowSQL = old;
        }

        // 找到使用枚举作为布尔型的旧表
        var es = (Database as MySql).EnumTables;
        foreach (var table in list)
        {
            if (!es.Contains(table.TableName))
            {
                var dc = table.Columns.FirstOrDefault(c => c.DataType == typeof(Boolean)
                  && c.RawType.EqualIgnoreCase("enum('N','Y')", "enum('Y','N')"));
                if (dc != null)
                {
                    es.Add(table.TableName);

                    WriteLog("发现MySql中旧格式的布尔型字段 {0} {1}", table.TableName, dc);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 快速取得所有表名
    /// </summary>
    /// <returns></returns>
    public override IList<String> GetTableNames()
    {
        var list = new List<String>();

        var sql = $"SHOW TABLE STATUS FROM `{Database.DatabaseName}`";
        var dt = base.Database.CreateSession().Query(sql, null);
        if (dt.Rows.Count == 0) return list;

        // 所有表
        foreach (var dr in dt)
        {
            var name = dr["Name"] + "";
            if (!name.IsNullOrEmpty()) list.Add(name);
        }

        return list;
    }

    protected override String GetFieldType(IDataColumn field)
    {
        //field.Length = field.Length > 255 ? 255 : field.Length;
        if (field.DataType == null && field.RawType == "datetimeoffset")
        {
            field.DataType = typeof(DateTime);
        }
        if (field.DataType == typeof(Decimal) && field is XField fi)
        {
            // 精度 与 位数
            field.Precision = field.Length <= 0 ? field.Precision : field.Length > 255 ? 255 : field.Length;
            //field.Precision = field.Length > 255 ? 255 : field.Length <= 0 ? field.Precision : field.Length;
        }
        return base.GetFieldType(field);
    }

    public override String FieldClause(IDataColumn field, Boolean onlyDefine)
    {
        var sql = base.FieldClause(field, onlyDefine);

        // 加上注释
        if (!String.IsNullOrEmpty(field.Description)) sql = $"{sql} COMMENT '{field.Description}'";

        return sql;
    }

    protected override String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        String str = null;
        if (!field.Nullable) str = " NOT NULL";

        // 默认值
        if (!field.Nullable && !field.Identity)
        {
            str += GetDefault(field, onlyDefine);
        }

        if (field.Identity) str += " AUTO_INCREMENT";

        return str;
    }
    #endregion

    #region 反向工程
    protected override Boolean DatabaseExist(String databaseName)
    {
        var dt = GetSchema(_.Databases, new String[] { databaseName });
        return dt != null && dt.Rows != null && dt.Rows.Count > 0;
    }

    public override String CreateDatabaseSQL(String dbname, String file) => base.CreateDatabaseSQL(dbname, file) + " DEFAULT CHARACTER SET utf8mb4";

    public override String DropDatabaseSQL(String dbname) => $"Drop Database If Exists {Database.FormatName(dbname)}";

    public override String CreateTableSQL(IDataTable table)
    {
        var fs = new List<IDataColumn>(table.Columns);

        //var sb = new StringBuilder(32 + fs.Count * 20);
        var sb = Pool.StringBuilder.Get();

        sb.AppendFormat("Create Table If Not Exists {0}(", FormatName(table));
        for (var i = 0; i < fs.Count; i++)
        {
            sb.AppendLine();
            sb.Append('\t');
            sb.Append(FieldClause(fs[i], true));
            if (i < fs.Count - 1) sb.Append(',');
        }
        if (table.PrimaryKeys.Length > 0) sb.AppendFormat(",\r\n\tPrimary Key ({0})", table.PrimaryKeys.Join(",", FormatName));
        sb.AppendLine();
        sb.Append(')');

        // 引擎和编码
        //sb.Append(" ENGINE=InnoDB");
        sb.Append(" DEFAULT CHARSET=utf8mb4");
        sb.Append(';');

        return sb.Put(true);
    }

    public override String AddTableDescriptionSQL(IDataTable table)
    {
        if (String.IsNullOrEmpty(table.Description)) return null;

        return $"Alter Table {FormatName(table)} Comment '{table.Description}'";
    }

    public override String AlterColumnSQL(IDataColumn field, IDataColumn oldfield) => $"Alter Table {FormatName(field.Table)} Modify Column {FieldClause(field, false)}";

    public override String AddColumnDescriptionSQL(IDataColumn field) =>
        // 返回String.Empty表示已经在别的SQL中处理
        String.Empty;
    #endregion
}