using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Web;
using XML.Core;
using XML.XCode.TDengine;

namespace XML.XCode.DataAccessLayer;

class TDengine : RemoteDb
{
    #region 属性
    /// <summary>返回数据库类型。</summary>
    public override DatabaseType Type => DatabaseType.TDengine;

    /// <summary>创建工厂</summary>
    /// <returns></returns>
    protected override DbProviderFactory CreateFactory()
    {
        var links = GetLinkNames("XML.Core.TDengine.dll", true);
        _ = PluginHelper.LoadPlugin("XML.Core.TDengine.TDengineFactory", null, "taos.dll", links.Join(","));

        return TDengineFactory.Instance;
    }

    const String Server_Key = "Server";
    protected override void OnSetConnectionString(ConnectionStringBuilder builder)
    {
        base.OnSetConnectionString(builder);

        var key = builder[Server_Key];
        if (key.EqualIgnoreCase(".", "localhost")) builder[Server_Key] = IPAddress.Loopback.ToString();
    }
    #endregion

    #region 方法
    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession() => new TDengineSession(this);

    /// <summary>创建元数据对象</summary>
    /// <returns></returns>
    protected override IMetaData OnCreateMetaData() => new TDengineMetaData();

    public override Boolean Support(String providerName)
    {
        providerName = providerName.ToLower();
        if (providerName.EqualIgnoreCase("TD", "TDengine")) return true;

        return false;
    }
    #endregion

    #region 数据库特性
    protected override String ReservedWordsStr
    {
        get
        {
            return "ACCESSIBLE,ADD,ALL,ALTER,ANALYZE,AND,AS,ASC,ASENSITIVE,BEFORE,BETWEEN,BIGINT,BINARY,BLOB,BOTH,BY,CALL,CASCADE,CASE,CHANGE,CHAR,CHARACTER,CHECK,COLLATE,COLUMN,CONDITION,CONNECTION,CONSTRAINT,CONTINUE,CONTRIBUTORS,CONVERT,CREATE,CROSS,CURRENT_DATE,CURRENT_TIME,CURRENT_TIMESTAMP,CURRENT_USER,CURSOR,DATABASE,DATABASES,DAY_HOUR,DAY_MICROSECOND,DAY_MINUTE,DAY_SECOND,DEC,DECIMAL,DECLARE,DEFAULT,DELAYED,DELETE,DESC,DESCRIBE,DETERMINISTIC,DISTINCT,DISTINCTROW,DIV,DOUBLE,DROP,DUAL,EACH,ELSE,ELSEIF,ENCLOSED,ESCAPED,EXISTS,EXIT,EXPLAIN,FALSE,FETCH,FLOAT,FLOAT4,FLOAT8,FOR,FORCE,FOREIGN,FROM,FULLTEXT,GRANT,GROUP,HAVING,HIGH_PRIORITY,HOUR_MICROSECOND,HOUR_MINUTE,HOUR_SECOND,IF,IGNORE,IN,INDEX,INFILE,INNER,INOUT,INSENSITIVE,INSERT,INT,INT1,INT2,INT3,INT4,INT8,INTEGER,INTERVAL,INTO,IS,ITERATE,JOIN,KEY,KEYS,KILL,LEADING,LEAVE,LEFT,LIKE,LIMIT,LINEAR,LINES,LOAD,LOCALTIME,LOCALTIMESTAMP,LOCK,LONG,LONGBLOB,LONGTEXT,LOOP,LOW_PRIORITY,MATCH,MEDIUMBLOB,MEDIUMINT,MEDIUMTEXT,MIDDLEINT,MINUTE_MICROSECOND,MINUTE_SECOND,MOD,MODIFIES,NATURAL,NOT,NO_WRITE_TO_BINLOG,NULL,NUMERIC,ON,OPTIMIZE,OPTION,OPTIONALLY,OR,ORDER,OUT,OUTER,OUTFILE,PRECISION,PRIMARY,PROCEDURE,PURGE,RANGE,READ,READS,READ_ONLY,READ_WRITE,REAL,REFERENCES,REGEXP,RELEASE,RENAME,REPEAT,REPLACE,REQUIRE,RESTRICT,RETURN,REVOKE,RIGHT,RLIKE,SCHEMA,SCHEMAS,SECOND_MICROSECOND,SELECT,SENSITIVE,SEPARATOR,SET,SHOW,SMALLINT,SPATIAL,SPECIFIC,SQL,SQLEXCEPTION,SQLSTATE,SQLWARNING,SQL_BIG_RESULT,SQL_CALC_FOUND_ROWS,SQL_SMALL_RESULT,SSL,STARTING,STRAIGHT_JOIN,TABLE,TERMINATED,THEN,TINYBLOB,TINYINT,TINYTEXT,TO,TRAILING,TRIGGER,TRUE,UNDO,UNION,UNIQUE,UNLOCK,UNSIGNED,UPDATE,UPGRADE,USAGE,USE,USING,UTC_DATE,UTC_TIME,UTC_TIMESTAMP,VALUES,VARBINARY,VARCHAR,VARCHARACTER,VARYING,WHEN,WHERE,WHILE,WITH,WRITE,X509,XOR,YEAR_MONTH,ZEROFILL," +
                "TBName," +
                "LOG,User,Role,Admin,Rank,Member";
        }
    }

    /// <summary>格式化关键字</summary>
    /// <param name="keyWord">关键字</param>
    /// <returns></returns>
    public override String FormatKeyWord(String keyWord)
    {
        //if (String.IsNullOrEmpty(keyWord)) throw new ArgumentNullException("keyWord");
        if (keyWord.IsNullOrEmpty()) return keyWord;

        if (keyWord.StartsWith("`") && keyWord.EndsWith("`")) return keyWord;

        return $"`{keyWord}`";
    }

    /// <summary>格式化数据为SQL数据</summary>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public override String FormatValue(IDataColumn field, Object value)
    {
        var code = System.Type.GetTypeCode(field.DataType);
        if (code == TypeCode.String)
        {
            if (value == null)
                return field.Nullable ? "null" : "''";

            return "'" + value.ToString()
                .Replace("\\", "\\\\")//反斜杠需要这样才能插入到数据库
                .Replace("'", @"\'") + "'";
        }
        else if (code == TypeCode.Boolean)
        {
            return value.ToBoolean() ? "1" : "0";
        }

        return base.FormatValue(field, value);
    }

    /// <summary>格式化时间为SQL字符串</summary>
    /// <remarks>
    /// 优化DateTime转为全字符串，平均耗时从25.76ns降为15.07。
    /// 调用非常频繁，每分钟都有数百万次调用。
    /// </remarks>
    /// <param name="dateTime">时间值</param>
    /// <returns></returns>
    public override String FormatDateTime(DateTime dateTime) => $"'{dateTime:yyyy-MM-dd HH:mm:ss.fff}'";

    /// <summary>长文本长度</summary>
    public override Int32 LongTextLength => 4000;

    internal protected override String ParamPrefix => "?";

    /// <summary>创建参数</summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public override IDataParameter CreateParameter(String name, Object value, Type type = null)
    {
        var dp = base.CreateParameter(name, value, type);

        //var type = field?.DataType;
        if (type == null) type = value?.GetType();

        // TDengine的枚举要用 DbType.String
        if (type == typeof(Boolean))
        {
            var v = value.ToBoolean();
            dp.DbType = DbType.Int16;
            dp.Value = v ? 1 : 0;
        }

        return dp;
    }

    /// <summary>系统数据库名</summary>
    public override String SystemDatabaseName => "db";

    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public override String StringConcat(String left, String right) => $"concat({(!String.IsNullOrEmpty(left) ? left : "\'\'")},{(!String.IsNullOrEmpty(right) ? right : "\'\'")})";
    #endregion
}