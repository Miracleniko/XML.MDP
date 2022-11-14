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

class DaMeng : RemoteDb
{
    #region 属性
    /// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
    public override DatabaseType Type => DatabaseType.DaMeng;

    /// <summary>创建工厂</summary>
    /// <returns></returns>
    protected override DbProviderFactory CreateFactory() => GetProviderFactory("DmProvider.dll", "Dm.DmClientFactory");

    const String Server_Key = "Server";
    protected override void OnSetConnectionString(ConnectionStringBuilder builder)
    {
        base.OnSetConnectionString(builder);

        var key = builder[Server_Key];
        if (key.EqualIgnoreCase(".", "localhost"))
        {
            builder[Server_Key] = "localhost";
        }
    }
    #endregion

    #region 方法
    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession() => new DaMengSession(this);

    /// <summary>创建元数据对象</summary>
    /// <returns></returns>
    protected override IMetaData OnCreateMetaData() => new DaMengMeta();

    public override Boolean Support(String providerName)
    {
        providerName = providerName.ToLower();
        if (providerName.Contains("dameng")) return true;
        if (providerName == "dm") return true;

        return false;
    }
    #endregion

    #region 数据库特性
    ///// <summary>已重载。格式化时间</summary>
    ///// <param name="dt"></param>
    ///// <returns></returns>
    //public override String FormatDateTime(DateTime dt)
    //{
    //    if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0) return "To_Date('{0:yyyy-MM-dd}', 'YYYY-MM-DD')".F(dt);

    //    return "To_Date('{0:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')".F(dt);
    //}

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

    ///// <summary>格式化标识列，返回插入数据时所用的表达式，如果字段本身支持自增，则返回空</summary>
    ///// <param name="field">字段</param>
    ///// <param name="value">数值</param>
    ///// <returns></returns>
    //public override String FormatIdentity(IDataColumn field, Object value) => String.Format("SEQ_{0}.nextval", field.Table.TableName);

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
        //return String.Format("\"{0}\"", keyWord);

        //if (String.IsNullOrEmpty(keyWord)) throw new ArgumentNullException("keyWord");
        if (String.IsNullOrEmpty(keyWord)) return keyWord;

        var pos = keyWord.LastIndexOf(".");

        if (pos < 0) return "\"" + keyWord + "\"";

        var tn = keyWord[(pos + 1)..];
        if (tn.StartsWith("\"")) return keyWord;

        return keyWord[..(pos + 1)] + "\"" + tn + "\"";
    }
    #endregion
}