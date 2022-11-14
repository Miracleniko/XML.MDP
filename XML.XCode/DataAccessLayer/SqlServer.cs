using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core.Web;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

internal class SqlServer : RemoteDb
{
    #region 属性
    /// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
    public override DatabaseType Type => DatabaseType.SqlServer;

    /// <summary>创建工厂</summary>
    /// <returns></returns>
    protected override DbProviderFactory CreateFactory()
    {
        // Microsoft 是最新的跨平台版本，优先使用
        //if (_Factory == null) _Factory = GetProviderFactory("Microsoft.Data.SqlClient.dll", "Microsoft.Data.SqlClient.SqlClientFactory", false, true);

        // 根据提供者加载已有驱动
        if (!Provider.IsNullOrEmpty() && Provider.Contains("Microsoft"))
        {
            var type = PluginHelper.LoadPlugin("Microsoft.Data.SqlClient.SqlClientFactory", null, "Microsoft.Data.SqlClient.dll", null);
            var factory = GetProviderFactory(type);
            if (factory != null) return factory;
        }

        // 找不到驱动时，再到线上下载
        {
            var factory = GetProviderFactory("System.Data.SqlClient.dll", "System.Data.SqlClient.SqlClientFactory");

            return factory;
        }
    }

    /// <summary>是否SQL2012及以上</summary>
    public Boolean IsSQL2012 => Version.Major > 11;

    private Version _Version;
    /// <summary>数据库版本</summary>
    public Version Version
    {
        get
        {
            if (_Version == null)
            {
                //_Version = new Version(ServerVersion);
                if (Version.TryParse(ServerVersion, out var v))
                    _Version = v;
                else
                {
                    var ns = ServerVersion.SplitAsInt(".");
                    if (ns.Length >= 4)
                        _Version = new Version(ns[0], ns[1], ns[2], ns[3]);
                    else if (ns.Length >= 3)
                        _Version = new Version(ns[0], ns[1], ns[2]);
                    else if (ns.Length >= 2)
                        _Version = new Version(ns[0], ns[1]);
                    else
                        _Version = new Version();
                }
            }
            return _Version;
        }
    }

    /// <summary>数据目录</summary>
    public String DataPath { get; set; }

    private const String Application_Name = "Application Name";
    protected override void OnSetConnectionString(ConnectionStringBuilder builder)
    {
        // 获取数据目录，用于反向工程创建数据库
        if (builder.TryGetAndRemove("DataPath", out var str) && !str.IsNullOrEmpty()) DataPath = str;

        base.OnSetConnectionString(builder);

        if (builder[Application_Name] == null)
        {
            var name = AppDomain.CurrentDomain.FriendlyName;
            builder[Application_Name] = $"XCode_{name}_{ConnName}";
        }
    }
    #endregion

    #region 方法
    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession() => new SqlServerSession(this);

    /// <summary>创建元数据对象</summary>
    /// <returns></returns>
    protected override IMetaData OnCreateMetaData() => new SqlServerMetaData();

    public override Boolean Support(String providerName)
    {
        providerName = providerName.ToLower();
        if (providerName.Contains("system.data.sqlclient")) return true;
        if (providerName.Contains("sql2012")) return true;
        if (providerName.Contains("sql2008")) return true;
        if (providerName.Contains("sql2005")) return true;
        if (providerName.Contains("sql2000")) return true;
        if (providerName == "sqlclient") return true;
        if (providerName.Contains("mssql")) return true;
        if (providerName.Contains("sqlserver")) return true;
        if (providerName.Contains("microsoft.data.sqlclient")) return true;

        return false;
    }
    #endregion

    #region 分页
    /// <summary>构造分页SQL</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">唯一键。用于not in分页</param>
    /// <returns>分页SQL</returns>
    public override String PageSplit(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn)
    {
        // 从第一行开始，不需要分页
        if (startRowIndex <= 0 && maximumRows < 1) return sql;

        // 先用字符串判断，命中率高，这样可以提高处理效率
        var hasOrderBy = false;
        if (sql.Contains(" Order ") && sql.ToLower().Contains(" order "))
            hasOrderBy = true;

        // 使用MS SQL 2012特有的分页算法
        if (hasOrderBy && IsSQL2012) return PageSplitFor2012(sql, startRowIndex, maximumRows);

        var builder = new SelectBuilder();
        builder.Parse(sql);

        return PageSplit(builder, startRowIndex, maximumRows).ToString();
    }

    public static String PageSplitFor2012(String sql, Int64 startRowIndex, Int64 maximumRows)
    {
        // 从第一行开始，不需要分页
        if (startRowIndex <= 0)
        {
            if (maximumRows < 1) return sql;

            return $"{sql} offset 1 rows fetch next {maximumRows} rows only";
        }
        if (maximumRows < 1) throw new NotSupportedException("不支持取第几条数据之后的所有数据！");

        return $"{sql} offset {startRowIndex} rows fetch next {maximumRows} rows only";
    }

    /// <summary>
    /// 格式化SQL SERVER 2012分页前半部分SQL语句
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    private String FormatSqlserver2012SQL(String sql)
    {
        var builder = new SelectBuilder();
        builder.Parse(sql);

        var sb = Pool.StringBuilder.Get();
        sb.Append("Select ");
        sb.Append(builder.Column.IsNullOrEmpty() ? "*" : builder.Column);
        sb.Append(" From ");
        sb.Append(builder.Table);
        if (!String.IsNullOrEmpty(builder.Where))
        {
            sb.Append(" Where type='p' and " + builder.Where);
        }
        else
        {
            sb.Append(" Where type='p' ");
        }
        if (!String.IsNullOrEmpty(builder.GroupBy)) sb.Append(" Group By " + builder.GroupBy);
        if (!String.IsNullOrEmpty(builder.Having)) sb.Append(" Having " + builder.Having);
        if (!String.IsNullOrEmpty(builder.OrderBy)) sb.Append(" Order By " + builder.OrderBy);

        return sb.Put(true);
    }

    public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        // 首页处理
        if (startRowIndex <= 0)
        {
            if (maximumRows < 1) return builder;

            return builder.Clone().Top(maximumRows);
        }

        // 修复无主键分页报错的情况
        if (builder.Key.IsNullOrEmpty() && builder.OrderBy.IsNullOrEmpty()) throw new XCodeException("分页算法要求指定排序列！" + builder.ToString());

        // Sql2012，非首页
        if (IsSQL2012 && !builder.OrderBy.IsNullOrEmpty())
        {
            builder = builder.Clone();
            builder.Limit = $"offset {startRowIndex} rows fetch next {maximumRows} rows only";
            return builder;
        }

        // 如果包含分组，则必须作为子查询
        var builder1 = builder.CloneWithGroupBy("XCode_T0", true);
        // 不必追求极致，把所有列放出来
        builder1.Column = $"*, row_number() over(Order By {builder.OrderBy ?? builder.Key}) as rowNumber";

        var builder2 = builder1.AsChild("XCode_T1", true);
        // 结果列处理
        //builder2.Column = builder.Column;
        //// 如果结果列包含有“.”，即有形如tab1.id、tab2.name之类的列时设为获取子查询的全部列
        //if ((!string.IsNullOrEmpty(builder2.Column)) && builder2.Column.Contains("."))
        //{
        //    builder2.Column = "*";
        //}
        // 不必追求极致，把所有列放出来
        builder2.Column = "*";

        // row_number()直接影响了排序，这里不再需要
        builder2.OrderBy = null;
        if (maximumRows < 1)
            builder2.Where = $"rowNumber>={startRowIndex + 1}";
        else
            builder2.Where = $"rowNumber Between {startRowIndex + 1} And {startRowIndex + maximumRows}";

        return builder2;
    }

    /// <summary>按top not in构造分页SQL</summary>
    /// <remarks>
    /// 两个构造分页SQL的方法，区别就在于查询生成器能够构造出来更好的分页语句，尽可能的避免子查询。
    /// MS体系的分页精髓就在于唯一键，当唯一键带有Asc/Desc/Unkown等排序结尾时，就采用最大最小值分页，否则使用较次的TopNotIn分页。
    /// TopNotIn分页和MaxMin分页的弊端就在于无法完美的支持GroupBy查询分页，只能查到第一页，往后分页就不行了，因为没有主键。
    /// </remarks>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">唯一键。用于not in分页</param>
    /// <returns>分页SQL</returns>
    public virtual String PageSplitByTopNotIn(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn)
    {
        // 从第一行开始，不需要分页
        if (startRowIndex <= 0 && maximumRows < 1) return sql;

        #region Max/Min分页
        // 如果要使用max/min分页法，首先keyColumn必须有asc或者desc
        if (!String.IsNullOrEmpty(keyColumn))
        {
            var kc = keyColumn.ToLower();
            if (kc.EndsWith(" desc") || kc.EndsWith(" asc") || kc.EndsWith(" unknown"))
            {
                var str = PageSplitMaxMin(sql, startRowIndex, maximumRows, keyColumn);
                if (!String.IsNullOrEmpty(str)) return str;

                // 如果不能使用最大最小值分页，则砍掉排序，为TopNotIn分页做准备
                var p = keyColumn.IndexOf(' ');
                if (p > 0) keyColumn = keyColumn[..p];
            }
        }
        #endregion

        //检查简单SQL。为了让生成分页SQL更短
        var tablename = CheckSimpleSQL(sql);
        if (tablename != sql)
            sql = tablename;
        else
            sql = $"({sql}) XCode_Temp_a";

        // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
        if (startRowIndex <= 0 && maximumRows > 0)
            return $"Select Top {maximumRows} * From {sql}";

        if (String.IsNullOrEmpty(keyColumn)) throw new ArgumentNullException(nameof(keyColumn), "这里用的not in分页算法要求指定主键列！");

        if (maximumRows < 1)
            sql = $"Select * From {sql} Where {keyColumn} Not In(Select Top {startRowIndex} {keyColumn} From {sql})";
        else
            sql = $"Select Top {maximumRows} * From {sql} Where {keyColumn} Not In(Select Top {startRowIndex} {keyColumn} From {sql})";
        return sql;
    }

    private static readonly Regex reg_Order = new(@"\border\s*by\b([^)]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    /// <summary>按唯一数字最大最小分析</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">唯一键。用于not in分页</param>
    /// <returns>分页SQL</returns>
    public static String PageSplitMaxMin(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn)
    {
        // 唯一键的顺序。默认为Empty，可以为asc或desc，如果有，则表明主键列是数字唯一列，可以使用max/min分页法
        var isAscOrder = keyColumn.ToLower().EndsWith(" asc");
        // 是否使用max/min分页法
        var canMaxMin = false;

        // 如果sql最外层有排序，且唯一的一个排序字段就是keyColumn时，可用max/min分页法
        // 如果sql最外层没有排序，其排序不是unknown，可用max/min分页法
        var ms = reg_Order.Matches(sql);
        if (ms != null && ms.Count > 0 && ms[0].Index > 0)
        {
            #region 有OrderBy
            // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
            if (startRowIndex <= 0 && maximumRows > 0)
                return $"Select Top {maximumRows} * From {CheckSimpleSQL(sql)}";

            var p = keyColumn.IndexOf(' ');
            if (p > 0) keyColumn = keyColumn[..p];
            sql = sql[..ms[0].Index];

            var strOrderBy = ms[0].Groups[1].Value.Trim();
            // 只有一个排序字段
            if (!String.IsNullOrEmpty(strOrderBy) && !strOrderBy.Contains(","))
            {
                // 有asc或者desc。没有时，默认为asc
                if (strOrderBy.ToLower().EndsWith(" desc"))
                {
                    var str = strOrderBy[..^" desc".Length].Trim();
                    // 排序字段等于keyColumn
                    if (str.ToLower() == keyColumn.ToLower())
                    {
                        isAscOrder = false;
                        canMaxMin = true;
                    }
                }
                else if (strOrderBy.ToLower().EndsWith(" asc"))
                {
                    var str = strOrderBy[..^" asc".Length].Trim();
                    // 排序字段等于keyColumn
                    if (str.ToLower() == keyColumn.ToLower())
                    {
                        isAscOrder = true;
                        canMaxMin = true;
                    }
                }
                else if (!strOrderBy.Contains(" ")) // 不含空格，是唯一排序字段
                {
                    // 排序字段等于keyColumn
                    if (strOrderBy.ToLower() == keyColumn.ToLower())
                    {
                        isAscOrder = true;
                        canMaxMin = true;
                    }
                }
            }
            #endregion
        }
        else
        {
            // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
            if (startRowIndex <= 0 && maximumRows > 0)
            {
                //数字分页中，业务上一般使用降序，Entity类会给keyColumn指定降序的
                //但是，在第一页的时候，没有用到keyColumn，而数据库一般默认是升序
                //这时候就会出现第一页是升序，后面页是降序的情况了。这里改正这个BUG
                if (keyColumn.ToLower().EndsWith(" desc") || keyColumn.ToLower().EndsWith(" asc"))
                    return $"Select Top {maximumRows} * From {CheckSimpleSQL(sql)} Order By {keyColumn}";
                else
                    return $"Select Top {maximumRows} * From {CheckSimpleSQL(sql)}";
            }

            if (!keyColumn.ToLower().EndsWith(" unknown")) canMaxMin = true;

            var p = keyColumn.IndexOf(' ');
            if (p > 0) keyColumn = keyColumn[..p];
        }

        if (canMaxMin)
        {
            if (maximumRows < 1)
                sql = $"Select * From {CheckSimpleSQL(sql)} Where {keyColumn}{(isAscOrder ? ">" : "<")}(Select {(isAscOrder ? "max" : "min")}({keyColumn}) From (Select Top {startRowIndex} {keyColumn} From {CheckSimpleSQL(sql)} Order By {keyColumn} {(isAscOrder ? "Asc" : "Desc")}) XCode_Temp_a) Order By {keyColumn} {(isAscOrder ? "Asc" : "Desc")}";
            else
                sql = $"Select Top {maximumRows} * From {CheckSimpleSQL(sql)} Where {keyColumn}{(isAscOrder ? ">" : "<")}(Select {(isAscOrder ? "max" : "min")}({keyColumn}) From (Select Top {startRowIndex} {keyColumn} From {CheckSimpleSQL(sql)} Order By {keyColumn} {(isAscOrder ? "Asc" : "Desc")}) XCode_Temp_a) Order By {keyColumn} {(isAscOrder ? "Asc" : "Desc")}";
            return sql;
        }
        return null;
    }
    #endregion

    #region 数据库特性
    protected override String ReservedWordsStr => "ADD,EXCEPT,PERCENT,ALL,EXEC,PLAN,ALTER,EXECUTE,PRECISION,AND,EXISTS,PRIMARY,ANY,EXIT,PRINT,AS,FETCH,PROC,ASC,FILE,PROCEDURE,AUTHORIZATION,FILLFACTOR,PUBLIC,BACKUP,FOR,RAISERROR,BEGIN,FOREIGN,READ,BETWEEN,FREETEXT,READTEXT,BREAK,FREETEXTTABLE,RECONFIGURE,BROWSE,FROM,REFERENCES,BULK,FULL,REPLICATION,BY,FUNCTION,RESTORE,CASCADE,GOTO,RESTRICT,CASE,GRANT,RETURN,CHECK,GROUP,REVOKE,CHECKPOINT,HAVING,RIGHT,CLOSE,HOLDLOCK,ROLLBACK,CLUSTERED,IDENTITY,ROWCOUNT,COALESCE,IDENTITY_INSERT,ROWGUIDCOL,COLLATE,IDENTITYCOL,RULE,COLUMN,IF,SAVE,COMMIT,IN,SCHEMA,COMPUTE,INDEX,SELECT,CONSTRAINT,INNER,SESSION_USER,CONTAINS,INSERT,SET,CONTAINSTABLE,INTERSECT,SETUSER,CONTINUE,INTO,SHUTDOWN,CONVERT,IS,SOME,CREATE,JOIN,STATISTICS,CROSS,KEY,SYSTEM_USER,CURRENT,KILL,TABLE,CURRENT_DATE,LEFT,TEXTSIZE,CURRENT_TIME,LIKE,THEN,CURRENT_TIMESTAMP,LINENO,TO,CURRENT_USER,LOAD,TOP,CURSOR,NATIONAL ,TRAN,DATABASE,NOCHECK,TRANSACTION,DBCC,NONCLUSTERED,TRIGGER,DEALLOCATE,NOT,TRUNCATE,DECLARE,NULL,TSEQUAL,DEFAULT,NULLIF,UNION,DELETE,OF,UNIQUE,DENY,OFF,UPDATE,DESC,OFFSETS,UPDATETEXT,DISK,ON,USE,DISTINCT,OPEN,USER,DISTRIBUTED,OPENDATASOURCE,VALUES,DOUBLE,OPENQUERY,VARYING,DROP,OPENROWSET,VIEW,DUMMY,OPENXML,WAITFOR,DUMP,OPTION,WHEN,ELSE,OR,WHERE,END,ORDER,WHILE,ERRLVL,OUTER,WITH,ESCAPE,OVER,WRITETEXT,ABSOLUTE,FOUND,PRESERVE,ACTION,FREE,PRIOR,ADMIN,GENERAL,PRIVILEGES,AFTER,GET,READS,AGGREGATE,GLOBAL,REAL,ALIAS,GO,RECURSIVE,ALLOCATE,GROUPING,REF,ARE,HOST,REFERENCING,ARRAY,HOUR,RELATIVE,ASSERTION,IGNORE,RESULT,AT,IMMEDIATE,RETURNS,BEFORE,INDICATOR,ROLE,BINARY,INITIALIZE,ROLLUP,BIT,INITIALLY,ROUTINE,BLOB,INOUT,ROW,BOOLEAN,INPUT,ROWS,BOTH,INT,SAVEPOINT,BREADTH,INTEGER,SCROLL,CALL,INTERVAL,SCOPE,CASCADED,ISOLATION,SEARCH,CAST,ITERATE,SECOND,CATALOG,LANGUAGE,SECTION,CHAR,LARGE,SEQUENCE,CHARACTER,LAST,SESSION,CLASS,LATERAL,SETS,CLOB,LEADING,SIZE,COLLATION,LESS,SMALLINT,COMPLETION,LEVEL,SPACE,CONNECT,LIMIT,SPECIFIC,CONNECTION,LOCAL,SPECIFICTYPE,CONSTRAINTS,LOCALTIME,SQL,CONSTRUCTOR,LOCALTIMESTAMP,SQLEXCEPTION,CORRESPONDING,LOCATOR,SQLSTATE,CUBE,MAP,SQLWARNING,CURRENT_PATH,MATCH,START,CURRENT_ROLE,MINUTE,STATE,CYCLE,MODIFIES,STATEMENT,DATA,MODIFY,STATIC,DATE,MODULE,STRUCTURE,DAY,MONTH,TEMPORARY,DEC,NAMES,TERMINATE,DECIMAL,NATURAL,THAN,DEFERRABLE,NCHAR,TIME,DEFERRED,NCLOB,TIMESTAMP,DEPTH,NEW,TIMEZONE_HOUR,DEREF,NEXT,TIMEZONE_MINUTE,DESCRIBE,NO,TRAILING,DESCRIPTOR,NONE,TRANSLATION,DESTROY,NUMERIC,TREAT,DESTRUCTOR,OBJECT,TRUE,DETERMINISTIC,OLD,UNDER,DICTIONARY,ONLY,UNKNOWN,DIAGNOSTICS,OPERATION,UNNEST,DISCONNECT,ORDINALITY,USAGE,DOMAIN,OUT,USING,DYNAMIC,OUTPUT,VALUE,EACH,PAD,VARCHAR,END-EXEC,PARAMETER,VARIABLE,EQUALS,PARAMETERS,WHENEVER,EVERY,PARTIAL,WITHOUT,EXCEPTION,PATH,WORK,EXTERNAL,POSTFIX,WRITE,FALSE,PREFIX,YEAR,FIRST,PREORDER,ZONE,FLOAT,PREPARE,ADA,AVG,BIT_LENGTH,CHAR_LENGTH,CHARACTER_LENGTH,COUNT,EXTRACT,FORTRAN,INCLUDE,INSENSITIVE,LOWER,MAX,MIN,OCTET_LENGTH,OVERLAPS,PASCAL,POSITION,SQLCA,SQLCODE,SQLERROR,SUBSTRING,SUM,TRANSLATE,TRIM,UPPER," +
              "Sort,Level,User,Online";

    /// <summary>长文本长度</summary>
    public override Int32 LongTextLength => 4000;

    /// <summary>格式化时间为SQL字符串</summary>
    /// <param name="dateTime">时间值</param>
    /// <returns></returns>
    public override String FormatDateTime(DateTime dateTime) => "{ts'" + dateTime.ToFullString() + "'}";

    /// <summary>格式化名称，如果是关键字，则格式化后返回，否则原样返回</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public override String FormatName(String name)
    {
        if (name.IsNullOrEmpty()) return name;

        // SqlServer数据库名和表名可以用横线。。。
        if (name.Contains("-")) return $"[{name}]";

        return base.FormatName(name);
    }

    /// <summary>格式化关键字</summary>
    /// <param name="keyWord">关键字</param>
    /// <returns></returns>
    public override String FormatKeyWord(String keyWord)
    {
        //if (String.IsNullOrEmpty(keyWord)) throw new ArgumentNullException("keyWord");
        if (String.IsNullOrEmpty(keyWord)) return keyWord;

        if (keyWord.StartsWith("[") && keyWord.EndsWith("]")) return keyWord;

        return $"[{keyWord}]";
    }

    /// <summary>系统数据库名</summary>
    public override String SystemDatabaseName => "master";

    public override String FormatValue(IDataColumn field, Object value)
    {
        var isNullable = true;
        Type type = null;
        if (field != null)
        {
            type = field.DataType;
            isNullable = field.Nullable;
        }
        else if (value != null)
            type = value.GetType();

        if (type == typeof(String))
        {
            // 热心网友 Hannibal 在处理日文网站时发现插入的日文为乱码，这里加上N前缀
            if (value == null) return isNullable ? "null" : "''";

            // 为了兼容旧版本实体类
            if (field.RawType.StartsWithIgnoreCase("n"))
                return "N'" + value.ToString().Replace("'", "''") + "'";
            else
                return "'" + value.ToString().Replace("'", "''") + "'";
        }
        else if (type == typeof(DateTime))
        {
            if (value == null) return isNullable ? "null" : "''";
            var dt = Convert.ToDateTime(value);

            if (dt <= DateTime.MinValue || dt >= DateTime.MaxValue) return isNullable ? "null" : "''";

            if (isNullable && (dt <= DateTime.MinValue || dt >= DateTime.MaxValue)) return "null";

            return FormatDateTime(dt);
        }

        return base.FormatValue(field, value);
    }

    private static readonly Char[] _likeKeys = new[] { '[', ']', '%', '_' };
    /// <summary>格式化模糊搜索的字符串。处理转义字符</summary>
    /// <param name="column">字段</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public override String FormatLike(IDataColumn column, String format, String value)
    {
        if (value.IsNullOrEmpty()) return value;

        if (value.IndexOfAny(_likeKeys) >= 0)
            value = value
                .Replace("[", "[[]")
                .Replace("]", "[]]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");

        return base.FormatLike(column, format, value);
    }
    #endregion
}