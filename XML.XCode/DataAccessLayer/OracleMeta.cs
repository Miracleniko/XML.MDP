using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core;
using XML.XCode.Configuration;
using XML.XCode.Common;

namespace XML.XCode.DataAccessLayer;

/// <summary>Oracle元数据</summary>
class OracleMeta : RemoteDbMetaData
{
    public OracleMeta() => Types = _DataTypes;

    /// <summary>拥有者</summary>
    public String Owner
    {
        get
        {
            var owner = Database.Owner;
            if (owner.IsNullOrEmpty()) owner = (Database as Oracle).User;

            return owner.ToUpper();
        }
    }

    /// <summary>用户名</summary>
    public String UserID => (Database as Oracle).User.ToUpper();

    /// <summary>取得所有表构架</summary>
    /// <returns></returns>
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        DataTable dt = null;

        // 不缺分大小写，并且不是保留字，才转大写
        if (names != null)
        {
            var db = Database as Oracle;
            /*if (db.IgnoreCase)*/
            names = names.Select(e => db.IsReservedWord(e) ? e : e.ToUpper()).ToArray();
        }

        // 采用集合过滤，提高效率
        String tableName = null;
        if (names != null && names.Length == 1) tableName = names.FirstOrDefault();
        if (tableName.IsNullOrEmpty()) tableName = null;

        var owner = Owner;
        //if (owner.IsNullOrEmpty()) owner = UserID;

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
        //data["Columns"] = GetSchema(_.Columns, new String[] { owner, tableName, null });
        //data["Indexes"] = GetSchema(_.Indexes, new String[] { owner, null, owner, tableName });
        //data["IndexColumns"] = GetSchema(_.IndexColumns, new String[] { owner, null, owner, tableName, null });

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
        if (MetaDataCollections.Contains(_.PrimaryKeys)) data["PrimaryKeys"] = GetSchema(_.PrimaryKeys, new String[] { owner, tableName, null });

        // 序列
        data["Sequences"] = Get("all_sequences", owner, null, null, "Sequence_Owner");

        // 表注释
        data["TableComment"] = Get("all_tab_comments", owner, tableName, mulTable);

        // 列注释
        data["ColumnComment"] = Get("all_col_comments", owner, tableName, mulTable);

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

        //// 检查该表是否有序列
        //if (CheckSeqExists("SEQ_{0}".F(table.TableName), data))
        //{
        //    // 不好判断自增列表，只能硬编码
        //    var dc = table.GetColumn("ID");
        //    if (dc == null) dc = table.Columns.FirstOrDefault(e => e.PrimaryKey && e.DataType.IsInt());
        //    if (dc != null && dc.DataType.IsInt()) dc.Identity = true;
        //}
    }

    ///// <summary>序列</summary>
    ///// <summary>检查序列是否存在</summary>
    ///// <param name="name">名称</param>
    ///// <param name="data"></param>
    ///// <returns></returns>
    //Boolean CheckSeqExists(String name, IDictionary<String, DataTable> data)
    //{
    //    // 序列名一定不是关键字，全部大写
    //    name = name.ToUpper();

    //    var dt = data?["Sequences"];
    //    if (dt?.Rows == null) dt = Database.CreateSession().Query("Select * From ALL_SEQUENCES Where SEQUENCE_OWNER='{0}' And SEQUENCE_NAME='{1}'".F(Owner, name)).Tables[0];
    //    if (dt?.Rows == null || dt.Rows.Count <= 0) return false;

    //    var where = String.Format("SEQUENCE_NAME='{0}'", name);
    //    var drs = dt.Select(where);
    //    return drs != null && drs.Length > 0;
    //}

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

            // 处理数字类型
            if (field.RawType.StartsWithIgnoreCase("NUMBER"))
            {
                var prec = fi.Precision;
                Type type = null;
                if (fi.Scale == 0)
                {
                    // 0表示长度不限制，为了方便使用，转为最常见的Int32
                    if (prec == 0)
                        type = typeof(Int32);
                    else if (prec == 1)
                        type = typeof(Boolean);
                    else if (prec <= 5)
                        type = typeof(Int16);
                    else if (prec <= 10)
                        type = typeof(Int32);
                    else
                        type = typeof(Int64);
                }
                else
                {
                    if (prec == 0)
                        type = typeof(Decimal);
                    else if (prec <= 5)
                        type = typeof(Single);
                    else if (prec <= 10)
                        type = typeof(Double);
                    else
                        type = typeof(Decimal);
                }
                field.DataType = type;
                if (prec > 0 && field.RawType.EqualIgnoreCase("NUMBER")) field.RawType += $"({prec},{fi.Scale})";
            }
        }

        // 长度
        if (TryGetDataRowValue(drColumn, "LENGTHINCHARS", out Int32 len) && len > 0) field.Length = len;

        base.FixField(field, drColumn);
    }

    protected override String GetFieldType(IDataColumn field)
    {
        var precision = 0;
        var scale = 0;

        if (field is XField fi)
        {
            precision = fi.Precision;
            scale = fi.Scale;
        }

        switch (Type.GetTypeCode(field.DataType))
        {
            case TypeCode.Boolean:
                return "NUMBER(1, 0)";
            case TypeCode.Int16:
            case TypeCode.UInt16:
                if (precision <= 0) precision = 5;
                return $"NUMBER({precision}, 0)";
            case TypeCode.Int32:
            case TypeCode.UInt32:
                //if (precision <= 0) precision = 10;
                if (precision <= 0) return "NUMBER";
                return $"NUMBER({precision}, 0)";
            case TypeCode.Int64:
            case TypeCode.UInt64:
                if (precision <= 0) precision = 20;
                return $"NUMBER({precision}, 0)";
            case TypeCode.Single:
                if (precision <= 0) precision = 5;
                if (scale <= 0) scale = 1;
                return $"NUMBER({precision}, {scale})";
            case TypeCode.Double:
                if (precision <= 0) precision = 10;
                if (scale <= 0) scale = 2;
                return $"NUMBER({precision}, {scale})";
            case TypeCode.Decimal:
                if (precision <= 0) precision = 20;
                if (scale <= 0) scale = 4;
                return $"NUMBER({precision}, {scale})";
            default:
                break;
        }

        return base.GetFieldType(field);
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
            { typeof(Byte[]), new String[] { "RAW({0})", "BFILE", "BLOB", "LONG RAW" } },
            { typeof(Boolean), new String[] { "NUMBER(1,0)" } },
            { typeof(Byte), new String[] { "NUMBER(1,0)" } },
            { typeof(Int16), new String[] { "NUMBER(5,0)" } },
            { typeof(Int32), new String[] { "NUMBER(10,0)" } },
            { typeof(Int64), new String[] { "NUMBER(20,0)" } },
            { typeof(Single), new String[] { "BINARY_FLOAT" } },
            { typeof(Double), new String[] { "BINARY_DOUBLE" } },
            { typeof(Decimal), new String[] { "NUMBER({0}, {1})", "FLOAT({0})" } },
            { typeof(DateTime), new String[] { "DATE", "TIMESTAMP({0})", "TIMESTAMP({0} WITH LOCAL TIME ZONE)", "TIMESTAMP({0} WITH TIME ZONE)" } },
            { typeof(String), new String[] { "VARCHAR2({0})", "NVARCHAR2({0})", "LONG", "CHAR({0})", "CLOB", "NCHAR({0})", "NCLOB", "XMLTYPE", "ROWID" } }
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
                // Oracle不支持判断数据库是否存在
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

        // 默认值
        if (!field.Nullable && !field.Identity)
        {
            str = GetDefault(field, onlyDefine) + str;
        }

        return str;
    }

    /// <summary>默认值</summary>
    /// <param name="field"></param>
    /// <param name="onlyDefine"></param>
    /// <returns></returns>
    protected override String GetDefault(IDataColumn field, Boolean onlyDefine)
    {
        if (field.DataType == typeof(DateTime)) return " DEFAULT To_Date('0001-01-01','yyyy-mm-dd')";

        return base.GetDefault(field, onlyDefine);
    }

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
        if (pks != null && pks.Length > 0)
        {
            sb.AppendFormat(",\r\n\tconstraint pk_{0} primary key ({1})", table.TableName, pks.Join(",", FormatName));
        }

        sb.AppendLine();
        sb.Append(')');

        //// 处理延迟段执行
        //if (Database is Oracle db)
        //{
        //    var vs = db.ServerVersion.SplitAsInt(".");
        //    if (vs.Length >= 4)
        //    {
        //        var ver = new Version(vs[0], vs[1], vs[2], vs[3]);
        //        if (ver >= new Version(11, 2, 0, 1)) sb.Append(" SEGMENT CREATION IMMEDIATE");
        //    }
        //}

        var sql = sb.ToString();
        if (sql.IsNullOrEmpty()) return sql;

        //// 有些表没有自增字段
        //var id = table.Columns.FirstOrDefault(e => e.Identity);
        //if (id != null)
        //{
        //    // 如果序列已存在，需要先删除
        //    if (CheckSeqExists("SEQ_{0}".F(table.TableName), null)) sb.AppendFormat(";\r\nDrop Sequence SEQ_{0}", table.TableName);

        //    // 感谢@晴天（412684802）和@老徐（gregorius 279504479），这里的最小值开始必须是0，插入的时候有++i的效果，才会得到从1开始的编号
        //    // @大石头 在PLSQL里面，创建序列从1开始时，nextval得到从1开始，而ADO.Net这里从1开始时，nextval只会得到2
        //    //sb.AppendFormat(";\r\nCreate Sequence SEQ_{0} Minvalue 0 Maxvalue 9999999999 Start With 0 Increment By 1 Cache 20", table.TableName);

        //    /*
        //     * Oracle从 11.2.0.1 版本开始，提供了一个“延迟段创建”特性：
        //     * 当我们创建了新的表(table)和序列(sequence)，在插入(insert)语句时，序列会跳过第一个值(1)。
        //     * 所以结果是插入的序列值从 2(序列的第二个值) 开始， 而不是 1开始。
        //     * 
        //     * 更改数据库的“延迟段创建”特性为false（需要有相应的权限）
        //     * ALTER SYSTEM SET deferred_segment_creation=FALSE; 
        //     * 
        //     * 第二种解决办法
        //     * 创建表时让seqment立即执行，如： 
        //     * CREATE TABLE tbl_test(
        //     *   test_id NUMBER PRIMARY KEY, 
        //     *   test_name VARCHAR2(20)
        //     * )
        //     * SEGMENT CREATION IMMEDIATE;
        //     */
        //    sb.AppendFormat(";\r\nCreate Sequence SEQ_{0} Minvalue 1 Maxvalue 9999999999 Start With 1 Increment By 1", table.TableName);
        //}

        // 去掉分号后的空格，Oracle不支持同时执行多个语句
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