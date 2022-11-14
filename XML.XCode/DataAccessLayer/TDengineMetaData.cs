using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>TDengine元数据</summary>
class TDengineMetaData : RemoteDbMetaData
{
    public TDengineMetaData() => Types = _DataTypes;

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
            { typeof(Byte), new String[] { "TINYINT" } },
            { typeof(Int16), new String[] { "SMALLINT" } },
            { typeof(Int32), new String[] { "INT" } },
            { typeof(Int64), new String[] { "BIGINT" } },
            { typeof(Single), new String[] { "FLOAT" } },
            { typeof(Double), new String[] { "DOUBLE" } },
            { typeof(Decimal), new String[] { "DOUBLE" } },
            { typeof(DateTime), new String[] { "TIMESTAMP" } },
            { typeof(String), new String[] { "NCHAR({0})", "BINARY({0})" } },
            { typeof(Boolean), new String[] { "BOOL" } },
            //{ typeof(Byte[]), new String[] { "BINARY" } },
        };
    #endregion

    #region 架构
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        var ss = Database.CreateSession();
        var list = new List<IDataTable>();

        var old = ss.ShowSQL;
        ss.ShowSQL = false;
        try
        {
            var sql = "SHOW TABLES";
            var dt = ss.Query(sql, null);
            if (dt.Rows.Count == 0) return null;

            var hs = new HashSet<String>(names ?? new String[0], StringComparer.OrdinalIgnoreCase);

            // 所有表
            foreach (var dr in dt)
            {
                var name = dr["table_name"] + "";
                if (name.IsNullOrEmpty() || hs.Count > 0 && !hs.Contains(name)) continue;

                var table = DAL.CreateTable();
                table.TableName = name;
                table.Owner = dr["stable_name"] as String;
                table.DbType = Database.Type;

                #region 字段
                sql = $"DESCRIBE {name}";
                var dcs = ss.Query(sql, null);
                foreach (var dc in dcs)
                {
                    var field = table.CreateColumn();

                    field.ColumnName = dc["Field"] + "";
                    field.RawType = dc["Type"] + "";
                    field.DataType = GetDataType(field.RawType);
                    field.Length = dc["Length"].ToInt();
                    field.Master = dc["Note"] as String == "TAGS";

                    field.Fix();

                    table.Columns.Add(field);
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

        return list;
    }

    /// <summary>
    /// 快速取得所有表名
    /// </summary>
    /// <returns></returns>
    public override IList<String> GetTableNames()
    {
        var list = new List<String>();

        var dt = base.Database.CreateSession().Query("SHOW TABLES", null);
        if (dt.Rows.Count == 0) return list;

        // 所有表
        foreach (var dr in dt)
        {
            var name = dr["table_name"] + "";
            if (!name.IsNullOrEmpty()) list.Add(name);
        }

        return list;
    }

    public override String FieldClause(IDataColumn field, Boolean onlyDefine)
    {
        var sb = new StringBuilder();

        // 字段名
        sb.AppendFormat("{0} ", FormatName(field));

        String typeName = null;
        // 每种数据库的自增差异太大，理应由各自处理，而不采用原始值
        if (Database.Type == field.Table.DbType && !field.Identity) typeName = field.RawType;

        if (String.IsNullOrEmpty(typeName)) typeName = GetFieldType(field);

        sb.Append(typeName);

        return sb.ToString();
    }
    #endregion

    #region 反向工程
    protected override Boolean DatabaseExist(String databaseName)
    {
        var ss = Database.CreateSession();
        var sql = $"SHOW DATABASES";
        var dt = ss.Query(sql, null);
        return dt != null && dt.Rows != null && dt.Rows.Any(e => e[0] as String == databaseName);
    }

    public override String CreateDatabaseSQL(String dbname, String file) => $"Create Database If Not Exists {Database.FormatName(dbname)}";
    //public override String CreateDatabaseSQL(String dbname, String file) => $"Create Database If Not Exists {Database.FormatName(dbname)} KEEP 365 DAYS 10 BLOCKS 6 UPDATE 1;";

    public override String DropDatabaseSQL(String dbname) => $"Drop Database If Exists {Database.FormatName(dbname)}";

    public override String CreateTableSQL(IDataTable table)
    {
        var fs = new List<IDataColumn>(table.Columns);

        var sb = Pool.StringBuilder.Get();

        sb.AppendFormat("Create Table If Not Exists {0}(", FormatName(table));
        var ss = fs.Where(e => !e.Master).ToList();
        var ms = fs.Where(e => e.Master).ToList();
        sb.Append(ss.Join(",", e => FieldClause(e, true)));
        sb.Append(')');

        if (ms.Count > 0)
        {
            sb.Append(" TAGS (");
            sb.Append(ms.Join(",", e => FieldClause(e, true)));
            sb.Append(')');
        }
        sb.Append(';');

        return sb.Put(true);
    }

    public override String AddTableDescriptionSQL(IDataTable table) =>
        // 返回String.Empty表示已经在别的SQL中处理
        String.Empty;

    public override String AlterColumnSQL(IDataColumn field, IDataColumn oldfield) => $"Alter Table {FormatName(field.Table)} Modify Column {FieldClause(field, false)}";

    public override String AddColumnDescriptionSQL(IDataColumn field) =>
        // 返回String.Empty表示已经在别的SQL中处理
        String.Empty;
    #endregion
}