using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>PostgreSQL元数据</summary>
internal class PostgreSQLMetaData : RemoteDbMetaData
{
    public PostgreSQLMetaData() => Types = _DataTypes;

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
            { typeof(Byte[]), new String[] { "bytea" } },
            { typeof(Boolean), new String[] { "boolean" } },
            { typeof(Int16), new String[] { "smallint" } },
            { typeof(Int32), new String[] { "integer" } },
            { typeof(Int64), new String[] { "bigint" } },
            { typeof(Single), new String[] { "float" } },
            { typeof(Double), new String[] { "float8", "double precision" } },
            { typeof(Decimal), new String[] { "decimal" } },
            { typeof(DateTime), new String[] { "timestamp", "timestamp without time zone", "date" } },
            { typeof(String), new String[] { "varchar({0})", "character varying", "text" } },
        };
    #endregion

    protected override void FixTable(IDataTable table, DataRow dr, IDictionary<String, DataTable> data)
    {
        // 注释
        if (TryGetDataRowValue(dr, "TABLE_COMMENT", out String comment)) table.Description = comment;

        base.FixTable(table, dr, data);
    }

    protected override void FixField(IDataColumn field, DataRow dr)
    {
        // 修正原始类型
        if (TryGetDataRowValue(dr, "COLUMN_TYPE", out String rawType)) field.RawType = rawType;

        // 修正自增字段
        if (TryGetDataRowValue(dr, "EXTRA", out String extra) && extra == "auto_increment") field.Identity = true;

        // 修正主键
        if (TryGetDataRowValue(dr, "COLUMN_KEY", out String key)) field.PrimaryKey = key == "PRI";

        // 注释
        if (TryGetDataRowValue(dr, "COLUMN_COMMENT", out String comment)) field.Description = comment;

        // 布尔类型
        if (field.RawType == "enum")
        {
            // PostgreSQL中没有布尔型，这里处理YN枚举作为布尔型
            if (field.RawType is "enum('N','Y')" or "enum('Y','N')")
            {
                field.DataType = typeof(Boolean);
                //// 处理默认值
                //if (!String.IsNullOrEmpty(field.Default))
                //{
                //    if (field.Default == "Y")
                //        field.Default = "true";
                //    else if (field.Default == "N")
                //        field.Default = "false";
                //}
                return;
            }
        }

        base.FixField(field, dr);
    }

    public override String FieldClause(IDataColumn field, Boolean onlyDefine)
    {
        if (field.Identity) return $"{field.Name} serial NOT NULL";

        var sql = base.FieldClause(field, onlyDefine);

        //// 加上注释
        //if (!String.IsNullOrEmpty(field.Description)) sql = $"{sql} COMMENT '{field.Description}'";

        return sql;
    }

    protected override String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        String str = null;
        if (!field.Nullable) str = " NOT NULL";

        if (field.Identity) str = " serial NOT NULL";

        // 默认值
        if (!field.Nullable && !field.Identity)
        {
            str += GetDefault(field, onlyDefine);
        }

        return str;
    }

    #region 架构定义
    //public override object SetSchema(DDLSchema schema, params object[] values)
    //{
    //    if (schema == DDLSchema.DatabaseExist)
    //    {
    //        IDbSession session = Database.CreateSession();

    //        DataTable dt = GetSchema(_.Databases, new String[] { values != null && values.Length > 0 ? (String)values[0] : session.DatabaseName });
    //        if (dt == null || dt.Rows == null || dt.Rows.Count <= 0) return false;
    //        return true;
    //    }

    //    return base.SetSchema(schema, values);
    //}

    protected override Boolean DatabaseExist(String databaseName)
    {
        //return base.DatabaseExist(databaseName);

        var session = Database.CreateSession();
        //var dt = GetSchema(_.Databases, new String[] { databaseName.ToLower() });
        var dt = GetSchema(_.Databases, new String[] { databaseName });
        return dt != null && dt.Rows != null && dt.Rows.Count > 0;
    }

    //public override string CreateDatabaseSQL(string dbname, string file)
    //{
    //    return String.Format("Create Database Binary {0}", FormatKeyWord(dbname));
    //}

    public override String DropDatabaseSQL(String dbname) => $"Drop Database If Exists {Database.FormatName(dbname)}";

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
        if (table.PrimaryKeys.Length > 0) sb.AppendFormat(",\r\n\tPrimary Key ({0})", table.PrimaryKeys.Join(",", FormatName));
        sb.AppendLine();
        sb.Append(')');

        return sb.ToString();
    }

    public override String AddTableDescriptionSQL(IDataTable table) => $"Comment On Table {FormatName(table)} is '{table.Description}'";

    public override String DropTableDescriptionSQL(IDataTable table) => $"Comment On Table {FormatName(table)} is ''";

    public override String AddColumnDescriptionSQL(IDataColumn field) => $"Comment On Column {FormatName(field.Table)}.{FormatName(field)} is '{field.Description}'";

    public override String DropColumnDescriptionSQL(IDataColumn field) => $"Comment On Column {FormatName(field.Table)}.{FormatName(field)} is ''";
    #endregion

    #region 辅助函数

    #endregion
}