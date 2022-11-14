using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core.Reflection;
using XML.Core.Web;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

internal class SQLite : FileDbBase
{
    #region 属性
    /// <summary>返回数据库类型。</summary>
    public override DatabaseType Type => DatabaseType.SQLite;

    /// <summary>创建工厂</summary>
    /// <returns></returns>
    protected override DbProviderFactory CreateFactory()
    {
        // Mono有自己的驱动，因为SQLite是混合编译，里面的C++代码与平台相关，不能通用;注意大小写问题
        if (Runtime.Mono)
            return GetProviderFactory("Mono.Data.Sqlite.dll", "System.Data.SqliteFactory");

        var type =
            PluginHelper.LoadPlugin("Microsoft.Data.Sqlite.SqliteFactory", null, "Microsoft.Data.Sqlite.dll", null) ??
            PluginHelper.LoadPlugin("System.Data.SQLite.SQLiteFactory", null, "System.Data.SQLite.dll", null);

        return GetProviderFactory(type) ??
            GetProviderFactory("Microsoft.Data.Sqlite.dll", "Microsoft.Data.Sqlite.SqliteFactory", true, true) ??
            GetProviderFactory("System.Data.SQLite.dll", "System.Data.SQLite.SQLiteFactory", false, false);
    }

    /// <summary>是否内存数据库</summary>
    public Boolean IsMemoryDatabase => DatabaseName.EqualIgnoreCase(MemoryDatabase);

    /// <summary>自动收缩数据库</summary>
    /// <remarks>
    /// 当一个事务从数据库中删除了数据并提交后，数据库文件的大小保持不变。
    /// 即使整页的数据都被删除，该页也会变成“空闲页”等待再次被使用，而不会实际地被从数据库文件中删除。
    /// 执行vacuum操作，可以通过重建数据库文件来清除数据库内所有的未用空间，使数据库文件变小。
    /// 但是，如果一个数据库在创建时被指定为auto_vacuum数据库，当删除事务提交时，数据库文件会自动缩小。
    /// 使用auto_vacuum数据库可以节省空间，但却会增加数据库操作的时间。
    /// </remarks>
    public Boolean AutoVacuum { get; set; }

    private static readonly String MemoryDatabase = ":memory:";

    protected override String OnResolveFile(String file)
    {
        if (String.IsNullOrEmpty(file) || file.EqualIgnoreCase(MemoryDatabase)) return MemoryDatabase;

        return base.OnResolveFile(file);
    }

    protected override void OnGetConnectionString(ConnectionStringBuilder builder)
    {
        base.OnGetConnectionString(builder);

        var factory = GetFactory(true);
        var provider = factory?.GetType().FullName ?? Provider;
        if (provider.StartsWithIgnoreCase("System.Data"))
        {
            //// 正常情况下INSERT, UPDATE和DELETE语句不返回数据。 当开启count-changes，以上语句返回一行含一个整数值的数据——该语句插入，修改或删除的行数。
            //if (!builder.ContainsKey("count_changes")) builder["count_changes"] = "1";

            // 优化SQLite，如果原始字符串里面没有这些参数，就设置这些参数
            //builder.TryAdd("Pooling", "true");
            //if (!builder.ContainsKey("Cache Size")) builder["Cache Size"] = "5000";
            builder.TryAdd("Cache Size", (512 * 1024 * 1024 / -1024) + "");
            // 加大Page Size会导致磁盘IO大大加大，性能反而有所下降
            //if (!builder.ContainsKey("Page Size")) builder["Page Size"] = "32768";
            // 这两个设置可以让SQLite拥有数十倍的极限性能，但同时又加大了风险，如果系统遭遇突然断电，数据库会出错，而导致系统无法自动恢复
            if (!Readonly) builder.TryAdd("Synchronous", "Off");
            // Journal Mode的内存设置太激进了，容易出事，关闭
            //if (!builder.ContainsKey("Journal Mode")) builder["Journal Mode"] = "Memory";
            // 数据库中一种高效的日志算法，对于非内存数据库而言，磁盘I/O操作是数据库效率的一大瓶颈。
            // 在相同的数据量下，采用WAL日志的数据库系统在事务提交时，磁盘写操作只有传统的回滚日志的一半左右，大大提高了数据库磁盘I/O操作的效率，从而提高了数据库的性能。
            if (!Readonly) builder.TryAdd("Journal Mode", "WAL");
            // 绝大多数情况下，都是小型应用，发生数据损坏的几率微乎其微，而多出来的问题让人觉得很烦，所以还是采用内存设置
            // 将来可以增加自动恢复数据的功能
            //if (!builder.ContainsKey("Journal Mode")) builder["Journal Mode"] = "Memory";

            if (Readonly) builder.TryAdd("Read Only", "true");

            // 自动清理数据
            if (builder.TryGetAndRemove("autoVacuum", out var vac)) AutoVacuum = vac.ToBoolean();
        }
        //else
        //    SupportSchema = false;

        // 默认超时时间
        //if (!builder.ContainsKey("Default Timeout")) builder["Default Timeout"] = 5 + "";

        // 繁忙超时
        //var busy = Setting.Current.CommandTimeout;
        //if (busy > 0)
        {
            // SQLite内部和.Net驱动都有Busy重试机制，多次重试仍然失败，则会出现dabase is locked。通过加大重试次数，减少高峰期出现locked的几率
            // 繁忙超时时间。出现Busy时，SQLite内部会在该超时时间内多次尝试
            //if (!builder.ContainsKey("BusyTimeout")) builder["BusyTimeout"] = 50 + "";
            // 重试次数。SQLite.Net驱动在遇到Busy时会多次尝试，每次随机等待1~150ms
            //if (!builder.ContainsKey("PrepareRetries")) builder["PrepareRetries"] = 10 + "";
        }

        DAL.WriteLog(builder.ToString());
    }
    #endregion

    #region 构造
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        // 不用Factory属性，为了避免触发加载SQLite驱动
        var factory = GetFactory(false);
        if (factory != null)
        {
            try
            {
                // 清空连接池
                var type = factory.CreateConnection().GetType();
                type.Invoke("ClearAllPools");
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { XTrace.WriteException(ex); }
        }
    }
    #endregion

    #region 方法
    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession() => new SQLiteSession(this);

    /// <summary>创建元数据对象</summary>
    /// <returns></returns>
    protected override IMetaData OnCreateMetaData() => new SQLiteMetaData();

    public override Boolean Support(String providerName)
    {
        providerName = providerName.ToLower();
        if (providerName.Contains("sqlite")) return true;

        return false;
    }

    //private void SetThreadSafe()
    //{
    //    var asm = _providerFactory?.GetType().Assembly;
    //    if (asm == null) return;

    //    var type = asm.GetTypes().FirstOrDefault(e => e.Name == "UnsafeNativeMethods");
    //    var mi = type?.GetMethod("sqlite3_config_none", BindingFlags.Static | BindingFlags.NonPublic);
    //    if (mi == null) return;

    //    /*
    //     * SQLiteConfigOpsEnum
    //     * 	SQLITE_CONFIG_SINGLETHREAD = 1,
    //     * 	SQLITE_CONFIG_MULTITHREAD = 2,
    //     * 	SQLITE_CONFIG_SERIALIZED = 3,
    //     */

    //    var rs = mi.Invoke(this, new Object[] { 2 });
    //    XTrace.WriteLine("sqlite3_config_none(SQLITE_CONFIG_MULTITHREAD) = {0}", rs);
    //}
    #endregion

    #region 分页
    /// <summary>已重写。获取分页</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">主键列。用于not in分页</param>
    /// <returns></returns>
    public override String PageSplit(String sql, Int64 startRowIndex, Int64 maximumRows, String keyColumn) => MySql.PageSplitByLimit(sql, startRowIndex, maximumRows);

    /// <summary>构造分页SQL</summary>
    /// <param name="builder">查询生成器</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>分页SQL</returns>
    public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows) => MySql.PageSplitByLimit(builder, startRowIndex, maximumRows);
    #endregion

    #region 数据库特性
    protected override String ReservedWordsStr => "ABORT,ACTION,ADD,AFTER,ALL,ALTER,ANALYZE,AND,AS,ASC,ATTACH,AUTOINCREMENT,BEFORE,BEGIN,BETWEEN,BY,CASCADE,CASE,CAST,CHECK,COLLATE,COLUMN,COMMIT,CONFLICT,CONSTRAINT,CREATE,CROSS,CURRENT_DATE,CURRENT_TIME,CURRENT_TIMESTAMP,DATABASE,DEFAULT,DEFERRABLE,DEFERRED,DELETE,DESC,DETACH,DISTINCT,DROP,EACH,ELSE,END,ESCAPE,EXCEPT,EXCLUSIVE,EXISTS,EXPLAIN,FAIL,FOR,FOREIGN,FROM,FULL,GLOB,GROUP,HAVING,IF,IGNORE,IMMEDIATE,IN,INDEX,INDEXED,INITIALLY,INNER,INSERT,INSTEAD,INTERSECT,INTO,IS,ISNULL,JOIN,KEY,LEFT,LIKE,LIMIT,MATCH,NATURAL,NO,NOT,NOTNULL,NULL,OF,OFFSET,ON,OR,ORDER,OUTER,PLAN,PRAGMA,PRIMARY,QUERY,RAISE,RECURSIVE,REFERENCES,REGEXP,REINDEX,RELEASE,RENAME,REPLACE,RESTRICT,RIGHT,ROLLBACK,ROW,SAVEPOINT,SELECT,SET,TABLE,TEMP,TEMPORARY,THEN,TO,TRANSACTION,TRIGGER,UNION,UNIQUE,UPDATE,USING,VACUUM,VALUES,VIEW,VIRTUAL,WHEN,WHERE,WITH,WITHOUT";

    /// <summary>格式化关键字</summary>
    /// <param name="keyWord">关键字</param>
    /// <returns></returns>
    public override String FormatKeyWord(String keyWord)
    {
        //if (String.IsNullOrEmpty(keyWord)) throw new ArgumentNullException("keyWord");
        if (String.IsNullOrEmpty(keyWord)) return keyWord;

        if (keyWord.StartsWith("[") && keyWord.EndsWith("]")) return keyWord;

        return $"[{keyWord}]";
        //return keyWord;
    }

    public override String FormatValue(IDataColumn field, Object value)
    {
        if (field.DataType == typeof(Byte[]))
        {
            var bts = (Byte[])value;
            if (bts == null || bts.Length <= 0) return "0x0";

            return "X'" + BitConverter.ToString(bts).Replace("-", null) + "'";
        }

        return base.FormatValue(field, value);
    }

    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public override String StringConcat(String left, String right) => (!left.IsNullOrEmpty() ? left : "\'\'") + "||" + (!right.IsNullOrEmpty() ? right : "\'\'");
    #endregion
}