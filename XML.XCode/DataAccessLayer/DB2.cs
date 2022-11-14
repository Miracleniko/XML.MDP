using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Reflection;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

class DB2 : RemoteDb
{
    #region 属性
    /// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
    public override DatabaseType Type => DatabaseType.DB2;

    /// <summary>创建工厂</summary>
    /// <returns></returns>
    protected override DbProviderFactory CreateFactory() => GetProviderFactory("IBM.Data.DB2.Core.dll", "IBM.Data.DB2.Core.DB2Factory");

    protected override void OnSetConnectionString(ConnectionStringBuilder builder)
    {
        base.OnSetConnectionString(builder);

        // 修正数据源
        if (builder.TryGetAndRemove("Data Source", out var str) && !str.IsNullOrEmpty())
        {
            if (str.Contains("://"))
            {
                var uri = new Uri(str);
                var type = uri.Scheme.IsNullOrEmpty() ? "TCP" : uri.Scheme.ToUpper();
                var port = uri.Port > 0 ? uri.Port : 1521;
                var name = uri.PathAndQuery.TrimStart("/");
                if (name.IsNullOrEmpty()) name = "ORCL";

                str = $"(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL={type})(HOST={uri.Host})(PORT={port})))(CONNECT_DATA=(SERVICE_NAME={name})))";
            }
            builder.TryAdd("Data Source", str);
        }
    }
    #endregion

    #region 方法
    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession() => new DB2Session(this);

    /// <summary>创建元数据对象</summary>
    /// <returns></returns>
    protected override IMetaData OnCreateMetaData() => new DB2Meta();

    public override Boolean Support(String providerName)
    {
        providerName = providerName.ToLower();
        if (providerName.Contains("db2")) return true;

        return false;
    }
    #endregion

    #region 分页
    /// <summary>已重写。获取分页 2012.9.26 HUIYUE修正分页BUG</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">主键列。用于not in分页</param>
    /// <returns></returns>
    public override String PageSplit(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn)
    {
        // 从第一行开始
        if (startRowIndex <= 0)
        {
            if (maximumRows <= 0) return sql;

            if (!sql.ToLower().Contains("order by")) return $"Select * From ({sql}) T0 Where rownum<={maximumRows}";
        }

        //if (maximumRows <= 0)
        //    sql = String.Format("Select * From ({1}) XCode_T0 Where rownum>={0}", startRowIndex + 1, sql);
        //else
        sql = $"Select * From (Select T0.*, rownum as rowNumber From ({sql}) T0) T1 Where rowNumber>{startRowIndex}";
        if (maximumRows > 0) sql += $" And rowNumber<={startRowIndex + maximumRows}";

        return sql;
    }

    /// <summary>构造分页SQL</summary>
    /// <remarks>
    /// 两个构造分页SQL的方法，区别就在于查询生成器能够构造出来更好的分页语句，尽可能的避免子查询。
    /// MS体系的分页精髓就在于唯一键，当唯一键带有Asc/Desc/Unkown等排序结尾时，就采用最大最小值分页，否则使用较次的TopNotIn分页。
    /// TopNotIn分页和MaxMin分页的弊端就在于无法完美的支持GroupBy查询分页，只能查到第一页，往后分页就不行了，因为没有主键。
    /// </remarks>
    /// <param name="builder">查询生成器</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>分页SQL</returns>
    public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        /*
         * DB2的rownum分页，在内层有Order By非主键排序时，外层的rownum会优先生效，
         * 导致排序字段有相同值时无法在多次查询中保持顺序，（DB2算法参数会改变）。
         * 其一，可以在排序字段后加上主键，确保排序内容唯一；
         * 其二，可以在第二层提前取得rownum，然后在第三层外使用；
         * 
         * 原分页算法始于2005年，只有在特殊情况下遇到分页出现重复数据的BUG：
         * 排序、排序字段不包含主键且不唯一、排序字段拥有相同数值的数据行刚好被分到不同页上
         */

        // 从第一行开始，不需要分页
        if (startRowIndex <= 0)
        {
            if (maximumRows <= 0) return builder;

            //// 如果带有排序，需要生成完整语句
            //if (builder.OrderBy.IsNullOrEmpty())
            return builder.AsChild("T0", false).AppendWhereAnd("rownum<={0}", maximumRows);
        }
        else if (maximumRows < 1)
            throw new NotSupportedException();

        builder = builder.AsChild("T0", false).AppendWhereAnd("rownum<={0}", startRowIndex + maximumRows);
        builder.Column = "T0.*, rownum as rowNumber";
        builder = builder.AsChild("T1", false).AppendWhereAnd("rowNumber>{0}", startRowIndex);

        //builder = builder.AsChild("T0", false);
        //builder.Column = "T0.*, rownum as rowNumber";
        //builder = builder.AsChild("T1", false);
        //builder.AppendWhereAnd("rowNumber>{0}", startRowIndex);
        //if (maximumRows > 0) builder.AppendWhereAnd("rowNumber<={0}", startRowIndex + maximumRows);

        return builder;
    }
    #endregion

    #region 数据库特性
    /// <summary>已重载。格式化时间</summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public override String FormatDateTime(DateTime dt)
    {
        if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0) return $"To_Date('{dt:yyyy-MM-dd}', 'YYYY-MM-DD')";

        return $"To_Date('{dt:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')";
    }

    public override String FormatValue(IDataColumn field, Object value)
    {
        var code = System.Type.GetTypeCode(field.DataType);
        var isNullable = field.Nullable;

        if (code == TypeCode.String)
        {
            if (value == null) return isNullable ? "null" : "''";

            if (field.RawType.StartsWithIgnoreCase("n"))
                return "N'" + value.ToString().Replace("'", "''") + "'";
            else
                return "'" + value.ToString().Replace("'", "''") + "'";
        }

        return base.FormatValue(field, value);
    }

    internal protected override String ParamPrefix => ":";

    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public override String StringConcat(String left, String right) => (!String.IsNullOrEmpty(left) ? left : "\'\'") + "||" + (!String.IsNullOrEmpty(right) ? right : "\'\'");

    /// <summary>创建参数</summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public override IDataParameter CreateParameter(String name, Object value, Type type = null)
    {
        //var type = field?.DataType;
        if (type == null)
        {
            type = value?.GetType();
            // 参数可能是数组
            if (type != null && type != typeof(Byte[]) && type.IsArray) type = type.GetElementTypeEx();
        }

        if (type == typeof(Boolean))
        {
            if (value is IEnumerable<Object> list)
                value = list.Select(e => e.ToBoolean() ? 1 : 0).ToArray();
            else if (value is IEnumerable<Boolean> list2)
                value = list2.Select(e => e.ToBoolean() ? 1 : 0).ToArray();
            else
                value = value.ToBoolean() ? 1 : 0;

            //type = typeof(Int32);
            var dp2 = Factory.CreateParameter();
            dp2.ParameterName = FormatParameterName(name);
            dp2.Direction = ParameterDirection.Input;
            dp2.DbType = DbType.Int32;
            dp2.Value = value;
            return dp2;
        }

        var dp = base.CreateParameter(name, value, type);

        // 修正时间映射
        if (type == typeof(DateTime)) dp.DbType = DbType.Date;

        return dp;
    }
    #endregion

    #region 关键字
    protected override String ReservedWordsStr
    {
        get
        {
            return "ALL,ALTER,AND,ANY,AS,ASC,BETWEEN,BY,CHAR,CHECK,CLUSTER,COMPRESS,CONNECT,CREATE,DATE,DECIMAL,DEFAULT,DELETE,DESC,DISTINCT,DROP,ELSE,EXCLUSIVE,EXISTS,FLOAT,FOR,FROM,GRANT,GROUP,HAVING,IDENTIFIED,IN,INDEX,INSERT,INTEGER,INTERSECT,INTO,IS,LIKE,LOCK,LONG,MINUS,MODE,NOCOMPRESS,NOT,NOWAIT,NULL,NUMBER,OF,ON,OPTION,OR,ORDER,PCTFREE,PRIOR,PUBLIC,RAW,RENAME,RESOURCE,REVOKE,SELECT,SET,SHARE,SIZE,SMALLINT,START,SYNONYM,TABLE,THEN,TO,TRIGGER,UNION,UNIQUE,UPDATE,VALUES,VARCHAR,VARCHAR2,VIEW,WHERE,WITH," +
              "Sort,Level,User,Online";
        }
    }

    /// <summary>格式化关键字</summary>
    /// <param name="keyWord">表名</param>
    /// <returns></returns>
    public override String FormatKeyWord(String keyWord)
    {
        if (String.IsNullOrEmpty(keyWord)) return keyWord;

        var pos = keyWord.LastIndexOf(".");

        if (pos < 0) return "\"" + keyWord + "\"";

        var tn = keyWord[(pos + 1)..];
        if (tn.StartsWith("\"")) return keyWord;

        return keyWord[..(pos + 1)] + "\"" + tn + "\"";
    }
    #endregion
}