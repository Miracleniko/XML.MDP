using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using XML.Core.Collections;
using XML.Core;
using XML.XCode.Configuration;
using XML.XCode.Common;

namespace XML.XCode.DataAccessLayer;

/// <summary>DaMeng元数据</summary>
class DaMengMeta : RemoteDbMetaData
{
    public DaMengMeta() => Types = _DataTypes;

    /// <summary>拥有者</summary>
    public String Owner
    {
        get
        {
            var owner = Database.Owner;
            if (owner.IsNullOrEmpty()) owner = (Database as DaMeng).User;

            return owner;
        }
    }

    /// <summary>用户名</summary>
    public String UserID => (Database as DaMeng).User.ToUpper();

    /// <summary>取得所有表构架</summary>
    /// <returns></returns>
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        DataTable dt = null;

        // 不缺分大小写，并且不是保留字，才转大写
        if (names != null)
        {
            var db = Database as DaMeng;
            /*if (db.IgnoreCase)*/
            names = names.Select(e => db.IsReservedWord(e) ? e : e.ToUpper()).ToArray();
        }

        // 采用集合过滤，提高效率
        String tableName = null;
        if (names != null && names.Length == 1) tableName = names.FirstOrDefault();
        if (tableName.IsNullOrEmpty()) tableName = null;

        var owner = Owner;
        //if (owner.IsNullOrEmpty()) owner = UserID;

        //dt = Get("all_tables", owner, tableName);
        dt = GetSchema(_.Tables, new String[] { owner, tableName });
        if (!dt.Columns.Contains("TABLE_TYPE"))
        {
            dt.Columns.Add("TABLE_TYPE", typeof(String));
            foreach (var dr in dt.Rows?.ToArray())
            {
                dr["TABLE_TYPE"] = "Table";
            }
        }
        var dtView = GetSchema(_.Views, new String[] { owner, tableName });
        if (dtView != null && dtView.Rows.Count != 0)
        {
            foreach (var dr in dtView.Rows?.ToArray())
            {
                var drNew = dt.NewRow();
                drNew["OWNER"] = dr["OWNER"];
                drNew["TABLE_NAME"] = dr["VIEW_NAME"];
                drNew["TABLE_TYPE"] = "View";
                dt.Rows.Add(drNew);
            }
        }

        var data = new NullableDictionary<String, DataTable>(StringComparer.OrdinalIgnoreCase);

        // 如果表太多，则只要目标表数据
        var mulTable = "";
        if (dt.Rows.Count > 10 && names != null && names.Length > 0)
        {
            //var tablenames = dt.Rows.ToArray().Select(e => "'{0}'".F(e["TABLE_NAME"]));
            //mulTable = " And TABLE_NAME in ({0})".F(tablenames.Join(","));
            mulTable = $" And TABLE_NAME in ({names.Select(e => $"'{e}'").Join(",")})";
        }

        // 列和索引
        data["Columns"] = Get("all_tab_columns", owner, tableName, mulTable);
        data["Indexes"] = Get("all_indexes", owner, tableName, mulTable);
        data["IndexColumns"] = Get("all_ind_columns", owner, tableName, mulTable, "Table_Owner");

        // 主键
        if (MetaDataCollections.Contains(_.PrimaryKeys) && !tableName.IsNullOrEmpty()) data["PrimaryKeys"] = GetSchema(_.PrimaryKeys, new String[] { owner, tableName, null });

        // 序列
        data["Sequences"] = Get("all_sequences", owner, null, null, "Sequence_Owner");

        // 表注释
        data["TableComment"] = Get("all_tab_comments", owner, tableName, mulTable);

        // 列注释
        data["ColumnComment"] = Get("all_col_comments", owner, tableName, mulTable, "SCHEMA_NAME");

        var list = GetTables(dt.Rows.ToArray(), names, data);

        return list;
    }

    /// <summary>
    /// 快速取得所有表名
    /// </summary>
    /// <returns></returns>
    public override IList<String> GetTableNames()
    {
        var list = new List<String>();

        var dt = GetSchema(_.Tables, new String[] { Owner, null });
        if (dt?.Rows == null || dt.Rows.Count <= 0) return list;

        foreach (DataRow dr in dt.Rows)
        {
            list.Add(GetDataRowValue<String>(dr, _.TalbeName));
        }

        return list;
    }

    private DataTable Get(String name, String owner, String tableName, String mulTable = null, String ownerName = null)
    {
        if (ownerName.IsNullOrEmpty()) ownerName = "Owner";
        var sql = $"Select * From {name} Where {ownerName}='{owner}'";
        if (!tableName.IsNullOrEmpty())
            sql += $" And TABLE_NAME='{tableName}'";
        else if (!mulTable.IsNullOrEmpty())
            sql += mulTable;

        return Database.CreateSession().Query(sql).Tables[0];
    }

    protected override void FixTable(IDataTable table, DataRow dr, IDictionary<String, DataTable> data)
    {
        base.FixTable(table, dr, data);

        // 主键
        var dt = data?["PrimaryKeys"];
        if (dt != null && dt.Rows.Count > 0)
        {
            var drs = dt.Select($"{_.TalbeName}='{table.TableName}'");
            if (drs != null && drs.Length > 0)
            {
                // 找到主键所在索引，这个索引的列才是主键
                if (TryGetDataRowValue(drs[0], _.IndexName, out String name) && !String.IsNullOrEmpty(name))
                {
                    var di = table.Indexes.FirstOrDefault(i => i.Name == name);
                    if (di != null)
                    {
                        di.PrimaryKey = true;
                        foreach (var dc in table.Columns)
                        {
                            dc.PrimaryKey = di.Columns.Contains(dc.ColumnName);
                        }
                    }
                }
            }
        }

        // 表注释 USER_TAB_COMMENTS
        table.Description = GetTableComment(table.TableName, data);

        if (table?.Columns == null || table.Columns.Count == 0) return;
    }

    String GetTableComment(String name, IDictionary<String, DataTable> data)
    {
        var dt = data?["TableComment"];
        if (dt?.Rows == null || dt.Rows.Count <= 0) return null;

        var where = $"TABLE_NAME='{name}'";
        var drs = dt.Select(where);
        if (drs != null && drs.Length > 0) return Convert.ToString(drs[0]["COMMENTS"]);

        return null;
    }

    /// <summary>取得指定表的所有列构架</summary>
    /// <param name="table"></param>
    /// <param name="columns">列</param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected override List<IDataColumn> GetFields(IDataTable table, DataTable columns, IDictionary<String, DataTable> data)
    {
        var list = base.GetFields(table, columns, data);
        if (list == null || list.Count <= 0) return null;

        // 字段注释
        if (list != null && list.Count > 0)
        {
            foreach (var field in list)
            {
                field.Description = GetColumnComment(table.TableName, field.ColumnName, data);
            }
        }
        return list;
    }
    const String KEY_OWNER = "OWNER";

    protected override List<IDataColumn> GetFields(IDataTable table, DataRow[] rows)
    {
        if (rows == null || rows.Length <= 0) return null;

        var owner = Owner;
        if (owner.IsNullOrEmpty() || !rows[0].Table.Columns.Contains(KEY_OWNER)) return base.GetFields(table, rows);

        var list = new List<DataRow>();
        foreach (var dr in rows)
        {
            if (TryGetDataRowValue(dr, KEY_OWNER, out String str) && owner.EqualIgnoreCase(str)) list.Add(dr);
        }

        return base.GetFields(table, list.ToArray());
    }

    String GetColumnComment(String tableName, String columnName, IDictionary<String, DataTable> data)
    {
        var dt = data?["ColumnComment"];
        if (dt?.Rows == null || dt.Rows.Count <= 0) return null;

        var where = $"{_.TalbeName}='{tableName}' AND {_.ColumnName}='{columnName}'";
        var drs = dt.Select(where);
        if (drs != null && drs.Length > 0) return Convert.ToString(drs[0]["COMMENTS"]);
        return null;
    }

    protected override void FixField(IDataColumn field, DataRow drColumn)
    {
        var dr = drColumn;

        // 长度
        //field.Length = GetDataRowValue<Int32>(dr, "CHAR_LENGTH", "DATA_LENGTH");
        field.Length = GetDataRowValue<Int32>(dr, "DATA_LENGTH");

        if (field is XField fi)
        {
            // 精度 与 位数
            fi.Precision = GetDataRowValue<Int32>(dr, "DATA_PRECISION");
            fi.Scale = GetDataRowValue<Int32>(dr, "DATA_SCALE");
            if (field.Length == 0) field.Length = fi.Precision;
        }

        // 长度
        if (TryGetDataRowValue(drColumn, "LENGTHINCHARS", out Int32 len) && len > 0) field.Length = len;

        base.FixField(field, drColumn);
    }

    protected override void FixIndex(IDataIndex index, DataRow dr)
    {
        if (TryGetDataRowValue(dr, "UNIQUENESS", out String str))
            index.Unique = str == "UNIQUE";

        base.FixIndex(index, dr);
    }

    /// <summary>数据类型映射</summary>
    private static readonly Dictionary<Type, String[]> _DataTypes = new()
        {
            { typeof(Byte[]), new String[] { "BLOB", "BINARY", "VARBINARY" } },
            { typeof(Boolean), new String[] { "BIT" } },
            { typeof(Byte), new String[] { "TINYINT" } },
            { typeof(Int16), new String[] { "SMALLINT" } },
            { typeof(Int32), new String[] { "INT" } },
            { typeof(Int64), new String[] { "BIGINT" } },
            { typeof(Single), new String[] { "REAL" } },
            { typeof(Double), new String[] { "DOUBLE" } },
            { typeof(Decimal), new String[] { "DEC" } },
            { typeof(DateTime), new String[] { "DATETIME", "TIME", "DATE", "TIMESTAMP" } },
            { typeof(String), new String[] { "VARCHAR({0})", "TEXT", "CHAR", "CLOB" } }
        };

    #region 架构定义
    public override Object SetSchema(DDLSchema schema, params Object[] values)
    {
        var session = Database.CreateSession();

        var dbname = String.Empty;
        var databaseName = String.Empty;
        switch (schema)
        {
            case DDLSchema.DatabaseExist:
                // DaMeng不支持判断数据库是否存在
                return true;

            default:
                break;
        }
        return base.SetSchema(schema, values);
    }

    public override String DatabaseExistSQL(String dbname) => String.Empty;

    protected override String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        var str = field.Nullable ? " NULL" : " NOT NULL";

        if (field.Identity) str = " IDENTITY NOT NULL";

        // 默认值
        if (!field.Nullable && !field.Identity)
        {
            str += GetDefault(field, onlyDefine);
        }

        return str;
    }

    //protected override String GetDefault(IDataColumn field, Boolean onlyDefine)
    //{
    //    if (field.DataType == typeof(DateTime)) return " DEFAULT To_Date('0001-01-01','yyyy-mm-dd')";

    //    return base.GetDefault(field, onlyDefine);
    //}

    public override String CreateTableSQL(IDataTable table)
    {
        var fs = new List<IDataColumn>(table.Columns);

        var sb = new StringBuilder(32 + fs.Count * 20);

        sb.AppendFormat("Create Table {0}(", FormatName(table));
        for (var i = 0; i < fs.Count; i++)
        {
            sb.AppendLine();
            sb.Append('\t');
            sb.Append(FieldClause(fs[i], true));
            if (i < fs.Count - 1) sb.Append(',');
        }

        // 主键
        var pks = table.PrimaryKeys;
        if (pks != null && pks.Length > 0) sb.AppendFormat(",\r\n\tprimary key ({0})", pks.Join(",", FormatName));

        sb.AppendLine();
        sb.Append(')');

        var sql = sb.ToString();
        if (sql.IsNullOrEmpty()) return sql;

        // 去掉分号后的空格，DaMeng不支持同时执行多个语句
        return sb.ToString();
    }

    //public override String DropTableSQL(String tableName)
    //{
    //    var sql = base.DropTableSQL(tableName);
    //    if (String.IsNullOrEmpty(sql)) return sql;

    //    var sqlSeq = String.Format("Drop Sequence SEQ_{0}", tableName);
    //    return sql + "; " + Environment.NewLine + sqlSeq;
    //}

    public override String AddColumnSQL(IDataColumn field) => $"Alter Table {FormatName(field.Table)} Add {FieldClause(field, true)}";
    public override String AlterColumnSQL(IDataColumn field, IDataColumn oldfield) => $"Alter Table {FormatName(field.Table)} Modify {FieldClause(field, false)}";

    public override String DropColumnSQL(IDataColumn field) => $"Alter Table {FormatName(field.Table)} Drop Column {field}";

    public override String AddTableDescriptionSQL(IDataTable table) => $"Comment On Table {FormatName(table)} is '{table.Description}'";

    public override String DropTableDescriptionSQL(IDataTable table) => $"Comment On Table {FormatName(table)} is ''";

    public override String AddColumnDescriptionSQL(IDataColumn field) => $"Comment On Column {FormatName(field.Table)}.{FormatName(field)} is '{field.Description}'";

    public override String DropColumnDescriptionSQL(IDataColumn field) => $"Comment On Column {FormatName(field.Table)}.{FormatName(field)} is ''";
    #endregion
}