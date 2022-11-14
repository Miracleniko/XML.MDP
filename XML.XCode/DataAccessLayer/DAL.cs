using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XML.Core.Caching;
using XML.Core.Collections;
using XML.Core.Configuration;
using XML.Core.Log;
using XML.Core.Model;
using XML.Core.Reflection;
using XML.Core.Serialization;
using XML.Core;
using System.Text.RegularExpressions;
using XML.Core.Data;
using System.Data.Common;
using System.Reflection;
using XML.Core.Threading;

namespace XML.XCode.DataAccessLayer;

/// <summary>数据访问层</summary>
/// <remarks>
/// 主要用于选择不同的数据库，不同的数据库的操作有所差别。
/// 每一个数据库链接字符串，对应唯一的一个DAL实例。
/// 数据库链接字符串可以写在配置文件中，然后在Create时指定名字；
/// 也可以直接把链接字符串作为AddConnStr的参数传入。
/// </remarks>
public partial class DAL
{
    #region 属性
    /// <summary>连接名</summary>
    public String ConnName { get; }

    /// <summary>实现了IDatabase接口的数据库类型</summary>
    public Type ProviderType { get; private set; }

    /// <summary>数据库类型</summary>
    public DatabaseType DbType { get; private set; }

    /// <summary>连接字符串</summary>
    /// <remarks>
    /// 修改连接字符串将会清空<see cref="Db"/>
    /// </remarks>
    public String ConnStr { get; private set; }

    private IDatabase _Db;
    /// <summary>数据库。所有数据库操作在此统一管理，强烈建议不要直接使用该属性，在不同版本中IDatabase可能有较大改变</summary>
    public IDatabase Db
    {
        get
        {
            if (_Db != null) return _Db;
            lock (this)
            {
                if (_Db != null) return _Db;

                var type = ProviderType;
                if (type == null) throw new XCodeException("无法识别{0}的数据提供者！", ConnName);

                //!!! 重量级更新：经常出现链接字符串为127/master的连接错误，非常有可能是因为这里线程冲突，A线程创建了实例但未来得及赋值连接字符串，就被B线程使用了
                var db = type.CreateInstance() as IDatabase;
                if (!ConnName.IsNullOrEmpty()) db.ConnName = ConnName;
                if (_infos.TryGetValue(ConnName, out var info)) db.Provider = info.Provider;

                // 设置连接字符串时，可能触发内部的一系列动作，因此放在最后
                if (!ConnStr.IsNullOrEmpty()) db.ConnectionString = DecodeConnStr(ConnStr);

                _Db = db;

                return _Db;
            }
        }
    }

    /// <summary>数据库会话</summary>
    public IDbSession Session => Db.CreateSession();

    private String _mapTo;
    private readonly ICache _cache = new MemoryCache();
    #endregion

    #region 创建函数
    /// <summary>构造函数</summary>
    /// <param name="connName">配置名</param>
    private DAL(String connName) => ConnName = connName;

    private Boolean _inited;
    private void Init()
    {
        if (_inited) return;
        lock (this)
        {
            if (_inited) return;

            var connName = ConnName;
            var css = ConnStrs;
            //if (!css.ContainsKey(connName)) throw new XCodeException("请在使用数据库前设置[" + connName + "]连接字符串");
            if (!css.ContainsKey(connName)) GetFromConfigCenter(connName);
            if (!css.ContainsKey(connName)) OnResolve?.Invoke(this, new ResolveEventArgs(connName));
            if (!css.ContainsKey(connName))
            {
                var cfg = XML.Core.Setting.Current;
                var set = Setting.Current;
                var connstr = "Data Source=" + cfg.DataPath.CombinePath(connName + ".db");
                if (set.Migration <= Migration.On) connstr += ";Migration=On";
                WriteLog("自动为[{0}]设置SQLite连接字符串：{1}", connName, connstr);
                AddConnStr(connName, connstr, null, "SQLite");
            }

            ConnStr = css[connName];
            if (ConnStr.IsNullOrEmpty()) throw new XCodeException("请在使用数据库前设置[" + connName + "]连接字符串");

            // 连接映射
            var vs = ConnStr.SplitAsDictionary("=", ",", true);
            if (vs.TryGetValue("MapTo", out var map) && !map.IsNullOrEmpty()) _mapTo = map;

            if (_infos.TryGetValue(connName, out var t))
            {
                ProviderType = t.Type;
                DbType = DbFactory.GetDefault(t.Type)?.Type ?? DatabaseType.None;
            }

            // 读写分离
            if (!connName.EndsWithIgnoreCase(".readonly"))
            {
                var connName2 = connName + ".readonly";
                if (css.ContainsKey(connName2)) ReadOnly = Create(connName2);
            }

            _inited = true;
        }
    }
    #endregion

    #region 静态管理
    private static readonly ConcurrentDictionary<String, DAL> _dals = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>创建一个数据访问层对象。</summary>
    /// <param name="connName">配置名</param>
    /// <returns>对应于指定链接的全局唯一的数据访问层对象</returns>
    public static DAL Create(String connName)
    {
        if (String.IsNullOrEmpty(connName)) throw new ArgumentNullException(nameof(connName));

        // Dictionary.TryGetValue 在多线程高并发下有可能抛出空异常
        var dal = _dals.GetOrAdd(connName, k => new DAL(k));

        // 创建完成对象后，初始化时单独锁这个对象，避免整体加锁
        dal.Init();

        // 映射到另一个连接
        if (!dal._mapTo.IsNullOrEmpty()) dal = _dals.GetOrAdd(dal._mapTo, Create);

        return dal;
    }

    private void Reset()
    {
        _Db.TryDispose();

        _Db = null;
        _Tables = null;
        _hasCheck = false;
        HasCheckTables.Clear();
        _mapTo = null;

        GC.Collect(2);

        _inited = false;
        Init();
    }

    private static ConcurrentDictionary<String, DbInfo> _infos;
    private static void InitConnections()
    {
        var ds = new ConcurrentDictionary<String, DbInfo>(StringComparer.OrdinalIgnoreCase);

        try
        {
            LoadConfig(ds);
            //LoadAppSettings(cs, ts);
        }
        catch (Exception ex)
        {
            WriteLog("LoadConfig 失败。{0}", ex.Message);
        }

        // 联合使用 appsettings.json
        try
        {
            LoadAppSettings("appsettings.json", ds);
        }
        catch (Exception ex)
        {
            WriteLog("LoadAppSettings 失败。{0}", ex.Message);
        }
        // 读取环境变量:ASPNETCORE_ENVIRONMENT=Development
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (String.IsNullOrWhiteSpace(env)) env = "Production";
        try
        {
            LoadAppSettings($"appsettings.{env.Trim()}.json", ds);
        }
        catch (Exception ex)
        {
            WriteLog("LoadAppSettings 失败。{0}", ex.Message);
        }

        // 从环境变量加载连接字符串，优先级最高
        try
        {
            LoadEnvironmentVariable(ds, Environment.GetEnvironmentVariables());
        }
        catch (Exception ex)
        {
            WriteLog("LoadEnvironmentVariable 失败。{0}", ex.Message);
        }

        var cs = new ConcurrentDictionary<String, String>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in ds)
        {
            cs.TryAdd(item.Key, item.Value.ConnectionString);
        }

        ConnStrs = cs;
        _infos = ds;
    }

    /// <summary>链接字符串集合</summary>
    /// <remarks>
    /// 如果需要添加连接字符串，应该使用AddConnStr，MapTo连接字符串除外（可以直接ConnStrs.TryAdd添加）；
    /// 如果需要修改一个DAL的连接字符串，不应该修改这里，而是修改DAL实例的<see cref="ConnStr"/>属性。
    /// </remarks>
    public static ConcurrentDictionary<String, String> ConnStrs { get; private set; }

    internal static void LoadConfig(IDictionary<String, DbInfo> ds)
    {
        var file = "web.config".GetFullPath();
        var fname = AppDomain.CurrentDomain.FriendlyName;
        // 2020-10-22 阴 fname可能是特殊情况，要特殊处理 "TestSourceHost: Enumerating source (E:\projects\bin\Debug\project.dll)"
        if (!File.Exists(fname) && fname.StartsWith("TestSourceHost: Enumerating"))
        {
            XTrace.WriteLine($"AppDomain.CurrentDomain.FriendlyName不太友好，处理一下：{fname}");
            fname = fname[fname.IndexOf(AppDomain.CurrentDomain.BaseDirectory, StringComparison.Ordinal)..].TrimEnd(')');
        }
        if (!File.Exists(file)) file = "app.config".GetFullPath();
        if (!File.Exists(file)) file = $"{fname}.config".GetFullPath();
        if (!File.Exists(file)) file = $"{fname}.exe.config".GetFullPath();
        if (!File.Exists(file)) file = $"{fname}.dll.config".GetFullPath();

        if (File.Exists(file))
        {
            // 读取配置文件
            var doc = new XmlDocument();
            doc.Load(file);

            var nodes = doc.SelectNodes("/configuration/connectionStrings/add");
            if (nodes != null)
            {
                foreach (XmlNode item in nodes)
                {
                    var name = item.Attributes["name"]?.Value;
                    var connstr = item.Attributes["connectionString"]?.Value;
                    var provider = item.Attributes["providerName"]?.Value;
                    if (name.IsNullOrEmpty() || connstr.IsNullOrWhiteSpace()) continue;

                    var type = DbFactory.GetProviderType(connstr, provider);
                    if (type == null) XTrace.WriteLine("无法识别{0}的提供者{1}！", name, provider);

                    ds[name] = new DbInfo
                    {
                        Name = name,
                        ConnectionString = connstr,
                        Type = type,
                        Provider = provider,
                    };
                }
            }
        }
    }

    internal static void LoadAppSettings(String fileName, IDictionary<String, DbInfo> ds)
    {
        // Asp.Net Core的Debug模式下配置文件位于项目目录而不是输出目录
        var file = fileName.GetBasePath();
        if (!File.Exists(file)) file = fileName.GetFullPath();
        if (!File.Exists(file)) file = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (File.Exists(file))
        {
            var text = File.ReadAllText(file);

            // 预处理注释
            text = JsonConfigProvider.TrimComment(text);

            var dic = JsonParser.Decode(text);
            dic = dic?["ConnectionStrings"] as IDictionary<String, Object>;
            if (dic != null && dic.Count > 0)
            {
                foreach (var item in dic)
                {
                    var name = item.Key;
                    if (name.IsNullOrEmpty()) continue;
                    if (item.Value is IDictionary<String, Object> cfgs)
                    {
                        var connstr = cfgs["connectionString"] + "";
                        var provider = cfgs["providerName"] + "";
                        if (connstr.IsNullOrWhiteSpace()) continue;

                        var type = DbFactory.GetProviderType(connstr, provider);
                        if (type == null) XTrace.WriteLine("无法识别{0}的提供者{1}！", name, provider);

                        ds[name] = new DbInfo
                        {
                            Name = name,
                            ConnectionString = connstr,
                            Type = type,
                            Provider = provider,
                        };
                    }
                    else if (item.Value is String connstr)
                    {
                        //var connstr = cfgs["connectionString"] + "";
                        if (connstr.IsNullOrWhiteSpace()) continue;

                        var builder = new ConnectionStringBuilder(connstr);
                        var provider = builder.TryGetValue("provider", out var prv) ? prv : null;

                        var type = DbFactory.GetProviderType(connstr, provider);
                        if (type == null) XTrace.WriteLine("无法识别{0}的提供者{1}！", name, provider);

                        ds[name] = new DbInfo
                        {
                            Name = name,
                            ConnectionString = connstr,
                            Type = type,
                            Provider = provider,
                        };
                    }
                }
            }
        }
    }

    internal static void LoadEnvironmentVariable(IDictionary<String, DbInfo> ds, IDictionary envs)
    {
        foreach (DictionaryEntry item in envs)
        {
            if (item.Key is String key && item.Value is String value && key.StartsWithIgnoreCase("XCode_"))
            {
                var connName = key["XCode_".Length..];

                var type = DbFactory.GetProviderType(value, null);
                if (type == null)
                {
                    WriteLog("环境变量[{0}]设置连接[{1}]时，未通过provider指定数据库类型，使用默认类型SQLite", key, connName);
                    type = DbFactory.Create(DatabaseType.SQLite).GetType();
                }

                var dic = value.SplitAsDictionary("=", ";");
                var provider = dic["provider"];

                // 允许后来者覆盖前面设置过了的
                ds[connName] = new DbInfo
                {
                    Name = connName,
                    ConnectionString = value,
                    Type = type,
                    Provider = provider,
                };
            }
        }
    }

    /// <summary>添加连接字符串</summary>
    /// <param name="connName">连接名</param>
    /// <param name="connStr">连接字符串</param>
    /// <param name="type">实现了IDatabase接口的数据库类型</param>
    /// <param name="provider">数据库提供者，如果没有指定数据库类型，则有提供者判断使用哪一种内置类型</param>
    public static void AddConnStr(String connName, String connStr, Type type, String provider)
    {
        if (connName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(connName));
        if (connStr.IsNullOrEmpty()) return;

        //2016.01.04 @宁波-小董，加锁解决大量分表分库多线程带来的提供者无法识别错误
        lock (_infos)
        {
            if (!ConnStrs.TryGetValue(connName, out var oldConnStr)) oldConnStr = null;

            // 从连接字符串中取得提供者
            if (provider.IsNullOrEmpty())
            {
                var dic = connStr.SplitAsDictionary("=", ";");
                provider = dic["provider"];
            }

            if (type == null) type = DbFactory.GetProviderType(connStr, provider);
            if (type == null) throw new XCodeException("无法识别{0}的提供者{1}！", connName, provider);

            // 允许后来者覆盖前面设置过了的
            //var set = new ConnectionStringSettings(connName, connStr, provider);
            ConnStrs[connName] = connStr;

            var inf = _infos.GetOrAdd(connName, k => new DbInfo { Name = k });
            inf.Name = connName;
            inf.ConnectionString = connStr;
            if (type != null) inf.Type = type;
            if (!provider.IsNullOrEmpty()) inf.Provider = provider;

            // 如果连接字符串改变，则重置所有
            if (!oldConnStr.IsNullOrEmpty() && !oldConnStr.EqualIgnoreCase(connStr))
            {
                WriteLog("[{0}]的连接字符串改变，准备重置！", connName);

                var dal = _dals.GetOrAdd(connName, k => new DAL(k));
                dal.ConnStr = connStr;
                dal.Reset();
            }
        }
    }

    /// <summary>找不到连接名时调用。支持用户自定义默认连接</summary>
    [Obsolete]
    public static event EventHandler<ResolveEventArgs> OnResolve;

    /// <summary>获取连接字符串的委托。可以二次包装在连接名前后加上标识，存放在配置中心</summary>
    public static GetConfigCallback GetConfig { get; set; }

    private static IConfigProvider _configProvider;
    /// <summary>设置配置提供者。可对接配置中心，DAL内部自动从内置对象容器中取得配置提供者</summary>
    /// <param name="configProvider"></param>
    public static void SetConfig(IConfigProvider configProvider)
    {
        WriteLog("DAL绑定配置提供者 {0}", configProvider);

        configProvider.Bind(new MyDAL());
        _configProvider = configProvider;
    }

    private static readonly ConcurrentHashSet<String> _conns = new();
    private static TimerEV _timerGetConfig;
    /// <summary>从配置中心加载连接字符串，并支持定时刷新</summary>
    /// <param name="connName"></param>
    /// <returns></returns>
    private static Boolean GetFromConfigCenter(String connName)
    {
        var getConfig = GetConfig;

        // 自动从对象容器里取得配置提供者
        if (getConfig == null && _configProvider == null)
        {
            var prv = ObjectContainer.Provider.GetService<IConfigProvider>();
            if (prv != null)
            {
                WriteLog("DAL自动绑定配置提供者 {0}", prv);

                prv.Bind(new MyDAL());
                _configProvider = prv;
            }
        }

        if (getConfig == null) getConfig = _configProvider?.GetConfig;
        {
            var str = getConfig?.Invoke("db:" + connName);
            if (str.IsNullOrEmpty()) return false;

            AddConnStr(connName, str, null, null);

            // 加入集合，定时更新
            if (!_conns.Contains(connName)) _conns.TryAdd(connName);
        }

        // 读写分离
        if (!connName.EndsWithIgnoreCase(".readonly"))
        {
            var connName2 = connName + ".readonly";
            var str = getConfig?.Invoke("db:" + connName2);
            if (!str.IsNullOrEmpty()) AddConnStr(connName2, str, null, null);

            // 加入集合，定时更新
            if (!_conns.Contains(connName2)) _conns.TryAdd(connName2);
        }

        if (_timerGetConfig == null && GetConfig != null) _timerGetConfig = new TimerEV(DoGetConfig, null, 5_000, 60_000) { Async = true };

        return true;
    }

    private static void DoGetConfig(Object state)
    {
        foreach (var item in _conns)
        {
            var str = GetConfig?.Invoke("db:" + item);
            if (!str.IsNullOrEmpty()) AddConnStr(item, str, null, null);
        }
    }

    private class MyDAL : IConfigMapping
    {
        public void MapConfig(IConfigProvider provider, IConfigSection section)
        {
            foreach (var item in _conns)
            {
                var str = section["db:" + item];
                if (!str.IsNullOrEmpty()) AddConnStr(item, str, null, null);
            }
        }
    }
    #endregion

    #region 连接字符串编码解码
    /// <summary>连接字符串编码</summary>
    /// <remarks>明文=>UTF8字节=>Base64</remarks>
    /// <param name="connstr"></param>
    /// <returns></returns>
    public static String EncodeConnStr(String connstr)
    {
        if (String.IsNullOrEmpty(connstr)) return connstr;

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(connstr));
    }

    /// <summary>连接字符串解码</summary>
    /// <remarks>Base64=>UTF8字节=>明文</remarks>
    /// <param name="connstr"></param>
    /// <returns></returns>
    private static String DecodeConnStr(String connstr)
    {
        if (String.IsNullOrEmpty(connstr)) return connstr;

        // 如果包含任何非Base64编码字符，直接返回
        foreach (var c in connstr)
        {
            if (!(c >= 'a' && c <= 'z' ||
                c >= 'A' && c <= 'Z' ||
                c >= '0' && c <= '9' ||
                c == '+' || c == '/' || c == '=')) return connstr;
        }

        Byte[] bts = null;
        try
        {
            // 尝试Base64解码，如果解码失败，估计就是连接字符串，直接返回
            bts = Convert.FromBase64String(connstr);
        }
        catch { return connstr; }

        return Encoding.UTF8.GetString(bts);
    }
    #endregion

    #region 正向工程
    private IList<IDataTable> _Tables;
    /// <summary>取得所有表和视图的构架信息（异步缓存延迟1秒）。设为null可清除缓存</summary>
    /// <remarks>
    /// 如果不存在缓存，则获取后返回；否则使用线程池线程获取，而主线程返回缓存。
    /// </remarks>
    /// <returns></returns>
    public IList<IDataTable> Tables
    {
        get
        {
            // 如果不存在缓存，则获取后返回；否则使用线程池线程获取，而主线程返回缓存
            if (_Tables == null)
                _Tables = GetTables();
            else
                Task.Factory.StartNew(() => { _Tables = GetTables(); });

            return _Tables;
        }
        set =>
            //设为null可清除缓存
            _Tables = null;
    }

    private IList<IDataTable> GetTables()
    {
        if (Db is DbBase db2 && !db2.SupportSchema) return new List<IDataTable>();

        var tracer = Tracer ?? GlobalTracer;
        using var span = tracer?.NewSpan($"db:{ConnName}:GetTables", ConnName);
        try
        {
            //CheckDatabase();
            var tables = Db.CreateMetaData().GetTables();

            if (span != null) span.Tag += ": " + tables.Join(",");

            return tables;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>
    /// 获取所有表名，带缓存，不区分大小写
    /// </summary>
    public ICollection<String> TableNames => _cache.GetOrAdd("tableNames", k => new HashSet<String>(GetTableNames(), StringComparer.OrdinalIgnoreCase), 60);

    /// <summary>
    /// 快速获取所有表名，无缓存，区分大小写
    /// </summary>
    /// <returns></returns>
    public IList<String> GetTableNames()
    {
        var tracer = Tracer ?? GlobalTracer;
        using var span = tracer?.NewSpan($"db:{ConnName}:GetTableNames", ConnName);
        try
        {
            var tables = Db.CreateMetaData().GetTableNames();

            if (span != null) span.Tag += ": " + tables.Join(",");

            return tables;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>导出模型</summary>
    /// <returns></returns>
    public String Export()
    {
        var list = Tables;

        if (list == null || list.Count <= 0) return null;

        return Export(list);
    }

    /// <summary>导出模型</summary>
    /// <param name="tables"></param>
    /// <returns></returns>
    public static String Export(IEnumerable<IDataTable> tables) => ModelHelper.ToXml(tables);

    /// <summary>导入模型</summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static List<IDataTable> Import(String xml)
    {
        if (String.IsNullOrEmpty(xml)) return null;

        return ModelHelper.FromXml(xml, CreateTable);
    }

    /// <summary>导入模型文件</summary>
    /// <param name="xmlFile"></param>
    /// <returns></returns>
    public static List<IDataTable> ImportFrom(String xmlFile)
    {
        if (xmlFile.IsNullOrEmpty()) return null;

        xmlFile = xmlFile.GetFullPath();
        if (!File.Exists(xmlFile)) return null;

        return ModelHelper.FromXml(File.ReadAllText(xmlFile), CreateTable);
    }
    #endregion

    #region 反向工程
    private Boolean _hasCheck;
    /// <summary>检查数据库，建库建表加字段</summary>
    /// <remarks>不阻塞，可能第一个线程正在检查表架构，别的线程已经开始使用数据库了</remarks>
    public void CheckDatabase()
    {
        if (_hasCheck) return;
        lock (this)
        {
            if (_hasCheck) return;
            _hasCheck = true;

            try
            {
                switch (Db.Migration)
                {
                    case Migration.Off:
                        break;
                    case Migration.ReadOnly:
                        Task.Factory.StartNew(CheckTables);
                        break;
                    case Migration.On:
                    case Migration.Full:
                        CheckTables();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (Debug) WriteLog(ex.GetMessage());
            }
        }
    }

    internal List<String> HasCheckTables = new();
    /// <summary>检查是否已存在，如果不存在则添加</summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    internal Boolean CheckAndAdd(String tableName)
    {
        var tbs = HasCheckTables;
        if (tbs.Contains(tableName)) return true;
        lock (tbs)
        {
            if (tbs.Contains(tableName)) return true;

            tbs.Add(tableName);
        }

        return false;
    }

    /// <summary>检查所有数据表，建表加字段</summary>
    public void CheckTables()
    {
        var name = ConnName;
        WriteLog("开始检查连接[{0}/{1}]的数据库架构……", name, DbType);

        var sw = Stopwatch.StartNew();

        try
        {
            var list = EntityFactory.GetTables(name, true);
            if (list != null && list.Count > 0)
            {
                // 移除所有已初始化的
                list.RemoveAll(dt => CheckAndAdd(dt.TableName));

                // 过滤掉视图
                list.RemoveAll(dt => dt.IsView);

                if (list != null && list.Count > 0)
                {
                    WriteLog("[{0}]待检查表架构的实体个数：{1}", name, list.Count);

                    SetTables(list.ToArray());
                }
            }
        }
        finally
        {
            sw.Stop();

            WriteLog("检查连接[{0}/{1}]的数据库架构耗时{2:n0}ms", name, DbType, sw.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>检查指定数据表，建表加字段</summary>
    /// <param name="tables"></param>
    public void SetTables(params IDataTable[] tables)
    {
        if (Db is DbBase db2 && !db2.SupportSchema) return;

        var tracer = Tracer ?? GlobalTracer;
        using var span = tracer?.NewSpan($"db:{ConnName}:SetTables", tables.Join());
        try
        {
            //// 构建DataTable时也要注意表前缀，避免反向工程用错
            //var pf = Db.TablePrefix;
            //if (!pf.IsNullOrEmpty())
            //{
            //    foreach (var tbl in tables)
            //    {
            //        if (!tbl.TableName.StartsWithIgnoreCase(pf)) tbl.TableName = pf + tbl.TableName;
            //    }
            //}

            foreach (var item in tables)
            {
                FixIndexName(item);
            }

            Db.CreateMetaData().SetTables(Db.Migration, tables);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    private void FixIndexName(IDataTable table)
    {
        // 修改一下索引名，否则，可能因为同一个表里面不同的索引冲突
        if (table.Indexes != null)
        {
            var pf = Db.TablePrefix;
            foreach (var di in table.Indexes)
            {
                if (!di.Name.IsNullOrEmpty() && pf.IsNullOrEmpty()) continue;

                var sb = Pool.StringBuilder.Get();
                sb.AppendFormat("IX_{0}", Db.FormatName(table, false));
                foreach (var item in di.Columns)
                {
                    sb.Append('_');
                    sb.Append(item);
                }

                di.Name = sb.Put(true);
            }
        }
    }
    #endregion
}

partial class DAL
{
    #region 属性
    [ThreadStatic]
    private static Int32 _QueryTimes;
    /// <summary>查询次数</summary>
    public static Int32 QueryTimes => _QueryTimes;

    [ThreadStatic]
    private static Int32 _ExecuteTimes;
    /// <summary>执行次数</summary>
    public static Int32 ExecuteTimes => _ExecuteTimes;

    /// <summary>只读实例。读写分离时，读取操作分走</summary>
    public DAL ReadOnly { get; set; }

    /// <summary>读写分离策略。忽略时间区间和表名</summary>
    public ReadWriteStrategy Strategy { get; set; } = new ReadWriteStrategy();
    #endregion

    #region 数据操作方法
    /// <summary>根据条件把普通查询SQL格式化为分页SQL。</summary>
    /// <param name="builder">查询生成器</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>分页SQL</returns>
    public SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        if (startRowIndex <= 0 && maximumRows <= 0) return builder;

        // 2016年7月2日 HUIYUE 取消分页SQL缓存，此部分缓存提升性能不多，但有可能会造成分页数据不准确，感觉得不偿失
        return Db.PageSplit(builder.Clone(), startRowIndex, maximumRows);
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public DataSet Select(String sql)
    {
        return QueryByCache(sql, "", "", (s, k2, k3) => Session.Query(s), nameof(Select));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="builder">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns></returns>
    public DataSet Select(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        return QueryByCache(builder, startRowIndex, maximumRows, (sb, start, max) =>
        {
            sb = PageSplit(sb, start, max);
            return Session.Query(sb.ToString(), CommandType.Text, sb.Parameters.ToArray());
        }, nameof(Select));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="builder">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns></returns>
    public DbTable Query(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        return QueryByCache(builder, startRowIndex, maximumRows, (sb, start, max) =>
        {
            sb = PageSplit(sb, start, max);
            return Session.Query(sb.ToString(), sb.Parameters.ToArray());
        }, nameof(Query));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public DbTable Query(String sql, IDictionary<String, Object> ps = null)
    {
        return QueryByCache(sql, ps, "", (s, p, k3) => Session.Query(s, Db.CreateParameters(p)), nameof(Query));
    }

    /// <summary>执行SQL查询，返回总记录数</summary>
    /// <param name="sb">查询生成器</param>
    /// <returns></returns>
    public Int32 SelectCount(SelectBuilder sb)
    {
        return (Int32)QueryByCache(sb, "", "", (s, k2, k3) => Session.QueryCount(s), nameof(SelectCount));
    }

    /// <summary>执行SQL查询，返回总记录数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Int32 SelectCount(String sql, CommandType type, params IDataParameter[] ps)
    {
        return (Int32)QueryByCache(sql, type, ps, (s, t, p) => Session.QueryCount(s, t, p), nameof(SelectCount));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public Int32 Execute(String sql)
    {
        return ExecuteByCache(sql, "", "", (s, t, p) => Session.Execute(s));
    }

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql"></param>
    /// <returns>新增行的自动编号</returns>
    public Int64 InsertAndGetIdentity(String sql)
    {
        return ExecuteByCache(sql, "", "", (s, t, p) => Session.InsertAndGetIdentity(s));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public DataSet Select(String sql, CommandType type, params IDataParameter[] ps)
    {
        return QueryByCache(sql, type, ps, (s, t, p) => Session.Query(s, t, p), nameof(Select));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Int32 Execute(String sql, CommandType type, params IDataParameter[] ps)
    {
        return ExecuteByCache(sql, type, ps, (s, t, p) => Session.Execute(s, t, p));
    }

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql"></param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public Int64 InsertAndGetIdentity(String sql, CommandType type, params IDataParameter[] ps)
    {
        return ExecuteByCache(sql, type, ps, (s, t, p) => Session.InsertAndGetIdentity(s, t, p));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public DataSet Select(String sql, CommandType type, IDictionary<String, Object> ps)
    {
        return QueryByCache(sql, type, ps, (s, t, p) => Session.Query(s, t, Db.CreateParameters(p)), nameof(Select));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Int32 Execute(String sql, CommandType type, IDictionary<String, Object> ps)
    {
        return ExecuteByCache(sql, type, ps, (s, t, p) => Session.Execute(s, t, Db.CreateParameters(p)));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="commandTimeout">命令超时时间，一般用于需要长时间执行的命令</param>
    /// <returns></returns>
    public Int32 Execute(String sql, Int32 commandTimeout)
    {
        return ExecuteByCache(sql, commandTimeout, "", (s, t, p) =>
        {
            using var cmd = Session.CreateCommand(s);
            if (t > 0) cmd.CommandTimeout = t;
            return Session.Execute(cmd);
        });
    }

    /// <summary>执行SQL语句，返回结果中的第一行第一列</summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public T ExecuteScalar<T>(String sql, CommandType type, IDictionary<String, Object> ps)
    {
        return ExecuteByCache(sql, type, ps, (s, t, p) => Session.ExecuteScalar<T>(s, t, Db.CreateParameters(p)));
    }
    #endregion

    #region 异步操作
    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="builder">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns></returns>
    public Task<DbTable> QueryAsync(SelectBuilder builder, Int64 startRowIndex, Int64 maximumRows)
    {
        return QueryByCacheAsync(builder, startRowIndex, maximumRows, (sb, start, max) =>
        {
            sb = PageSplit(sb, start, max);
            return Session.QueryAsync(sb.ToString(), sb.Parameters.ToArray());
        }, nameof(QueryAsync));
    }

    /// <summary>执行SQL查询，返回记录集</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Task<DbTable> QueryAsync(String sql, IDictionary<String, Object> ps = null)
    {
        return QueryByCacheAsync(sql, ps, "", (s, p, k3) => Session.QueryAsync(s, Db.CreateParameters(p)), nameof(QueryAsync));
    }

    /// <summary>执行SQL查询，返回总记录数</summary>
    /// <param name="sb">查询生成器</param>
    /// <returns></returns>
    public Task<Int64> SelectCountAsync(SelectBuilder sb)
    {
        return QueryByCacheAsync(sb, "", "", (s, k2, k3) => Session.QueryCountAsync(s), nameof(SelectCountAsync));
    }

    /// <summary>执行SQL查询，返回总记录数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Task<Int64> SelectCountAsync(String sql, CommandType type, params IDataParameter[] ps)
    {
        return QueryByCacheAsync(sql, type, ps, (s, t, p) => Session.QueryCountAsync(s, t, p), nameof(SelectCountAsync));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public Task<Int32> ExecuteAsync(String sql)
    {
        return ExecuteByCacheAsync(sql, "", "", (s, t, p) => Session.ExecuteAsync(s));
    }

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql"></param>
    /// <returns>新增行的自动编号</returns>
    public Task<Int64> InsertAndGetIdentityAsync(String sql)
    {
        return ExecuteByCacheAsync(sql, "", "", (s, t, p) => Session.InsertAndGetIdentityAsync(s));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Task<Int32> ExecuteAsync(String sql, CommandType type, params IDataParameter[] ps)
    {
        return ExecuteByCacheAsync(sql, type, ps, (s, t, p) => Session.ExecuteAsync(s, t, p));
    }

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql"></param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public Task<Int64> InsertAndGetIdentityAsync(String sql, CommandType type, params IDataParameter[] ps)
    {
        return ExecuteByCacheAsync(sql, type, ps, (s, t, p) => Session.InsertAndGetIdentityAsync(s, t, p));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Task<Int32> ExecuteAsync(String sql, CommandType type, IDictionary<String, Object> ps)
    {
        return ExecuteByCacheAsync(sql, type, ps, (s, t, p) => Session.ExecuteAsync(s, t, Db.CreateParameters(p)));
    }

    /// <summary>执行SQL语句，返回受影响的行数</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="commandTimeout">命令超时时间，一般用于需要长时间执行的命令</param>
    /// <returns></returns>
    public Task<Int32> ExecuteAsync(String sql, Int32 commandTimeout)
    {
        return ExecuteByCacheAsync(sql, commandTimeout, "", (s, t, p) =>
        {
            using var cmd = Session.CreateCommand(s);
            if (t > 0) cmd.CommandTimeout = t;
            return Session.ExecuteAsync(cmd);
        });
    }

    /// <summary>执行SQL语句，返回结果中的第一行第一列</summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns></returns>
    public Task<T> ExecuteScalarAsync<T>(String sql, CommandType type, IDictionary<String, Object> ps)
    {
        return ExecuteByCacheAsync(sql, type, ps, (s, t, p) => Session.ExecuteScalarAsync<T>(s, t, Db.CreateParameters(p)));
    }
    #endregion

    #region 事务
    /// <summary>开始事务</summary>
    /// <remarks>
    /// Read Uncommitted: 允许读取脏数据，一个事务能看到另一个事务还没有提交的数据。（不会阻止其它操作）
    /// Read Committed: 确保事务读取的数据都必须是已经提交的数据。它限制了读取中间的，没有提交的，脏的数据。
    /// 但是它不能确保当事务重新去读取的时候，读的数据跟上次读的数据是一样的，也就是说当事务第一次读取完数据后，
    /// 该数据是可能被其他事务修改的，当它再去读取的时候，数据可能是不一样的。（数据隐藏，不阻止）
    /// Repeatable Read: 是一个更高级别的隔离级别，如果事务再去读取同样的数据，先前的数据是没有被修改过的。（阻止其它修改）
    /// Serializable: 它做出了最有力的保证，除了每次读取的数据是一样的，它还确保每次读取没有新的数据。（阻止其它添删改）
    /// </remarks>
    /// <param name="level">事务隔离等级</param>
    /// <returns>剩下的事务计数</returns>
    public Int32 BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
    {
        //CheckDatabase();

        return Session.BeginTransaction(level);
    }

    /// <summary>提交事务</summary>
    /// <returns>剩下的事务计数</returns>
    public Int32 Commit() => Session.Commit();

    /// <summary>回滚事务，忽略异常</summary>
    /// <returns>剩下的事务计数</returns>
    public Int32 Rollback() => Session.Rollback();
    #endregion

    #region 缓存
    /// <summary>缓存存储</summary>
    public ICache Store { get; set; }

    /// <summary>数据层缓存。默认10秒</summary>
    public Int32 Expire { get; set; }

    private static readonly AsyncLocal<String> _SpanTag = new();

    /// <summary>埋点上下文信息。用于附加在埋点标签后的上下文信息</summary>
    public static void SetSpanTag(String value) => _SpanTag.Value = value;

    private ICache GetCache()
    {
        var st = Store;
        if (st != null) return st;

        var exp = Expire;
        if (exp == 0) exp = Db.DataCache;
        if (exp == 0) exp = Setting.Current.DataCacheExpire;
        if (exp <= 0) return null;

        Expire = exp;

        lock (this)
        {
            if (Store == null)
            {
                var p = exp / 2;
                if (p < 30) p = 30;

                st = Store = new MemoryCache { Period = p, Expire = exp };
            }
        }

        return st;
    }

    private TResult QueryByCache<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, TResult> callback, String action)
    {
        // 读写分离
        if (Strategy != null)
        {
            if (Strategy.Validate(this, k1 + "", action)) return ReadOnly.QueryByCache(k1, k2, k3, callback, action);
        }

        //CheckDatabase();

        // 读缓存
        var cache = GetCache();
        var key = "";
        if (cache != null)
        {
            var sb = Pool.StringBuilder.Get();
            if (!action.IsNullOrEmpty())
            {
                sb.Append(action);
                sb.Append('#');
            }
            Append(sb, k1);
            Append(sb, k2);
            Append(sb, k3);
            key = sb.Put(true);

            if (cache.TryGetValue<TResult>(key, out var value)) return value;
        }

        Interlocked.Increment(ref _QueryTimes);
        var rs = Invoke(k1, k2, k3, callback, action);

        cache?.Set(key, rs, Expire);

        return rs;
    }

    private TResult ExecuteByCache<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, TResult> callback)
    {
        if (Db.Readonly) throw new InvalidOperationException($"数据连接[{ConnName}]只读，禁止执行{k1}");

        //CheckDatabase();

        var rs = Invoke(k1, k2, k3, callback, "Execute");

        GetCache()?.Clear();

        Interlocked.Increment(ref _ExecuteTimes);

        return rs;
    }

    private TResult Invoke<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, TResult> callback, String action)
    {
        var tracer = Tracer ?? GlobalTracer;
        var traceName = "";
        var sql = "";

        // 从sql解析表名，作为跟踪名一部分。正则避免from前后换行的情况
        if (tracer != null)
        {
            sql = (k1 + "").Trim();
            if (action == "Execute")
            {
                // 使用 Insert/Update/Delete 作为埋点操作名
                var p = sql.IndexOf(' ');
                if (p > 0) action = sql[..p];
            }
            else if (action.EqualIgnoreCase("Query", "Select"))
            {
                // 查询数据时，Group作为独立埋点操作名
                if (sql.ToLower().Contains("group by"))
                    action = "Group";
            }

            traceName = $"db:{ConnName}:{action}";

            var tables = GetTables(sql);
            if (tables.Length > 0) traceName += ":" + tables.Join("-");
        }

        // 使用k1参数作为tag，一般是sql
        var span = tracer?.NewSpan(traceName, sql);
        try
        {
            var rs = callback(k1, k2, k3);

            if (span != null)
            {
                if (rs is DbTable dt)
                    span.Tag = $"{sql} [rows={dt.Rows?.Count}]";
                else if (rs is DataSet ds && ds.Tables.Count > 0)
                    span.Tag = $"{sql} [rows={ds.Tables[0].Rows.Count}]";
                else
                    span.Tag = $"{sql} [result={rs}]";

                var stag = _SpanTag.Value;
                if (!stag.IsNullOrEmpty()) span.Tag += " " + stag;
            }

            return rs;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
        finally
        {
            span?.Dispose();
        }
    }

    private async Task<TResult> QueryByCacheAsync<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, Task<TResult>> callback, String action)
    {
        // 读写分离
        if (Strategy != null)
        {
            if (Strategy.Validate(this, k1 + "", action)) return await ReadOnly.QueryByCacheAsync(k1, k2, k3, callback, action);
        }

        //CheckDatabase();

        // 读缓存
        var cache = GetCache();
        var key = "";
        if (cache != null)
        {
            var sb = Pool.StringBuilder.Get();
            if (!action.IsNullOrEmpty())
            {
                sb.Append(action);
                sb.Append('#');
            }
            Append(sb, k1);
            Append(sb, k2);
            Append(sb, k3);
            key = sb.Put(true);

            if (cache.TryGetValue<TResult>(key, out var value)) return value;
        }

        Interlocked.Increment(ref _QueryTimes);
        var rs = await InvokeAsync(k1, k2, k3, callback, action);

        cache?.Set(key, rs, Expire);

        return rs;
    }

    private async Task<TResult> ExecuteByCacheAsync<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, Task<TResult>> callback)
    {
        if (Db.Readonly) throw new InvalidOperationException($"数据连接[{ConnName}]只读，禁止执行{k1}");

        //CheckDatabase();

        var rs = await InvokeAsync(k1, k2, k3, callback, "Execute");

        GetCache()?.Clear();

        Interlocked.Increment(ref _ExecuteTimes);

        return rs;
    }

    private async Task<TResult> InvokeAsync<T1, T2, T3, TResult>(T1 k1, T2 k2, T3 k3, Func<T1, T2, T3, Task<TResult>> callback, String action)
    {
        var tracer = Tracer ?? GlobalTracer;
        var traceName = "";
        var sql = "";

        // 从sql解析表名，作为跟踪名一部分。正则避免from前后换行的情况
        if (tracer != null)
        {
            sql = (k1 + "").Trim();
            if (action == "Execute")
            {
                // 使用 Insert/Update/Delete 作为埋点操作名
                var p = sql.IndexOf(' ');
                if (p > 0) action = sql[..p];
            }

            traceName = $"db:{ConnName}:{action}";

            var tables = GetTables(sql);
            if (tables.Length > 0) traceName += ":" + tables.Join("-");
        }

        // 使用k1参数作为tag，一般是sql
        var span = tracer?.NewSpan(traceName, sql);
        try
        {
            var rs = await callback(k1, k2, k3);

            if (span != null)
            {
                if (rs is DbTable dt)
                    span.Tag = $"{sql} [rows={dt.Rows?.Count}]";
                else if (rs is DataSet ds && ds.Tables.Count > 0)
                    span.Tag = $"{sql} [rows={ds.Tables[0].Rows.Count}]";
                else
                    span.Tag = $"{sql} [result={rs}]";

                var stag = _SpanTag.Value;
                if (!stag.IsNullOrEmpty()) span.Tag += " " + stag;
            }

            return rs;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
        finally
        {
            span?.Dispose();
        }
    }

    private static readonly Regex reg_table = new("(?:\\s+from|insert\\s+into|update|\\s+join|drop\\s+table|truncate\\s+table)\\s+[`'\"\\[]?([\\w]+)[`'\"\\[]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    /// <summary>从Sql语句中截取表名</summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static String[] GetTables(String sql)
    {
        var list = new List<String>();
        var ms = reg_table.Matches(sql);
        foreach (Match item in ms)
        {
            list.Add(item.Groups[1].Value);
        }
        return list.ToArray();
    }

    private static void Append(StringBuilder sb, Object value)
    {
        if (value == null) return;

        if (value is SelectBuilder builder)
        {
            sb.Append(builder);
            foreach (var item in builder.Parameters)
            {
                sb.Append('#');
                sb.Append(item.ParameterName);
                sb.Append('#');
                sb.Append(item.Value);
            }
        }
        else if (value is IDataParameter[] ps)
        {
            foreach (var item in ps)
            {
                sb.Append('#');
                sb.Append(item.ParameterName);
                sb.Append('#');
                sb.Append(item.Value);
            }
        }
        else if (value is IDictionary<String, Object> dic)
        {
            foreach (var item in dic)
            {
                sb.Append('#');
                sb.Append(item.Key);
                sb.Append('#');
                sb.Append(item.Value);
            }
        }
        else
        {
            sb.Append('#');
            sb.Append(value);
        }
    }
    #endregion
}

public partial class DAL
{
    /// <summary>根据实体类获取表名的委托，用于Mapper的Insert/Update</summary>
    public static GetNameCallback GetTableName { get; set; }

    /// <summary>根据实体类获取主键名的委托，用于Mapper的Update</summary>
    public static GetNameCallback GetKeyName { get; set; }

    #region 添删改查
    /// <summary>查询Sql并映射为结果集</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public IEnumerable<T> Query<T>(String sql, Object param = null)
    {
        if (IsValueTuple(typeof(T))) throw new InvalidOperationException($"不支持ValueTuple类型[{typeof(T).FullName}]");

        //var ps = param?.ToDictionary();
        var dt = QueryByCache(sql, param, "", (s, p, k3) => Session.Query(s, Db.CreateParameters(p)), nameof(Query));

        // 优先特殊处理基础类型，选择第一字段
        var type = typeof(T);
        var utype = Nullable.GetUnderlyingType(type);
        if (utype != null) type = utype;
        if (type.GetTypeCode() != TypeCode.Object) return dt.Rows.Select(e => e[0].ChangeType<T>());

        return dt.ReadModels<T>();
    }

    /// <summary>查询Sql并映射为结果集，支持分页</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns></returns>
    public IEnumerable<T> Query<T>(String sql, Object param, Int64 startRowIndex, Int64 maximumRows)
    {
        if (IsValueTuple(typeof(T))) throw new InvalidOperationException($"不支持ValueTuple类型[{typeof(T).FullName}]");

        // SqlServer的分页需要知道主键
        var sql2 =
            DbType == DatabaseType.SqlServer ?
            Db.PageSplit(sql, startRowIndex, maximumRows, new SelectBuilder(sql).Key) :
            Db.PageSplit(sql, startRowIndex, maximumRows, null);

        return Query<T>(sql2, param);
    }

    /// <summary>查询Sql并映射为结果集，支持分页</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <param name="page">分页参数</param>
    /// <returns></returns>
    public IEnumerable<T> Query<T>(String sql, Object param, PageParameter page)
    {
        if (IsValueTuple(typeof(T))) throw new InvalidOperationException($"不支持ValueTuple类型[{typeof(T).FullName}]");

        // 查询总行数
        if (page.RetrieveTotalCount)
        {
            page.TotalCount = SelectCount(sql, CommandType.Text);
        }

        var start = (page.PageIndex - 1) * page.PageSize;
        var max = page.PageSize;

        if (!page.OrderBy.IsNullOrEmpty()) sql += " order by " + page.OrderBy;

        return Query<T>(sql, param, start, max);
    }

    /// <summary>查询Sql并返回单个结果</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public T QuerySingle<T>(String sql, Object param = null) => Query<T>(sql, param).FirstOrDefault();

    /// <summary>查询Sql并映射为结果集</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> QueryAsync<T>(String sql, Object param = null)
    {
        if (IsValueTuple(typeof(T))) throw new InvalidOperationException($"不支持ValueTuple类型[{typeof(T).FullName}]");

        var dt = await QueryByCacheAsync(sql, param, "", (s, p, k3) => Session.QueryAsync(s, Db.CreateParameters(p)), nameof(QueryAsync));

        // 优先特殊处理基础类型，选择第一字段
        var type = typeof(T);
        var utype = Nullable.GetUnderlyingType(type);
        if (utype != null) type = utype;
        if (type.GetTypeCode() != TypeCode.Object) return dt.Rows.Select(e => e[0].ChangeType<T>());

        return dt.ReadModels<T>();
    }

    /// <summary>查询Sql并返回单个结果</summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public async Task<T> QuerySingleAsync<T>(String sql, Object param = null) => (await QueryAsync<T>(sql, param)).FirstOrDefault();

    private static Boolean IsValueTuple(Type type)
    {
        if ((Object)type != null && type.IsValueType)
        {
            return type.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);
        }
        return false;
    }

    /// <summary>执行Sql</summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public Int32 Execute(String sql, Object param = null) =>
        //var ps = param?.ToDictionary();
        ExecuteByCache(sql, "", param, (s, t, p) => Session.Execute(s, CommandType.Text, Db.CreateParameters(p)));

    /// <summary>执行Sql并返回数据读取器</summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public IDataReader ExecuteReader(String sql, Object param = null)
    {
        var traceName = $"db:{ConnName}:ExecuteReader";
        if (Tracer != null)
        {
            var tables = GetTables(sql);
            if (tables.Length > 0) traceName += ":" + tables.Join("-");
        }
        using var span = Tracer?.NewSpan(traceName, sql);
        try
        {
            //var ps = param?.ToDictionary();
            var cmd = Session.CreateCommand(sql, CommandType.Text, Db.CreateParameters(param));
            cmd.Connection = Db.OpenConnection();

            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>执行SQL语句，返回结果中的第一行第一列</summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public T ExecuteScalar<T>(String sql, Object param = null) =>
        QueryByCache(sql, param, "", (s, p, k3) => Session.ExecuteScalar<T>(s, CommandType.Text, Db.CreateParameters(p)), nameof(ExecuteScalar));

    /// <summary>执行Sql</summary>
    /// <param name="sql">Sql语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public Task<Int32> ExecuteAsync(String sql, Object param = null) =>
        ExecuteByCacheAsync(sql, "", param, (s, t, p) => Session.ExecuteAsync(s, CommandType.Text, Db.CreateParameters(p)));

    /// <summary>执行Sql并返回数据读取器</summary>
    /// <param name="sql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public Task<DbDataReader> ExecuteReaderAsync(String sql, Object param = null)
    {
        var traceName = $"db:{ConnName}:ExecuteReaderAsync";
        if (Tracer != null)
        {
            var tables = GetTables(sql);
            if (tables.Length > 0) traceName += ":" + tables.Join("-");
        }
        using var span = Tracer?.NewSpan(traceName, sql);
        try
        {
            var cmd = Session.CreateCommand(sql, CommandType.Text, Db.CreateParameters(param));
            cmd.Connection = Db.OpenConnection();

            return cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            throw;
        }
    }

    /// <summary>执行SQL语句，返回结果中的第一行第一列</summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="param">参数对象</param>
    /// <returns></returns>
    public Task<T> ExecuteScalarAsync<T>(String sql, Object param = null) =>
        QueryByCacheAsync(sql, param, "", (s, p, k3) => Session.ExecuteScalarAsync<T>(s, CommandType.Text, Db.CreateParameters(p)), nameof(ExecuteScalarAsync));

    private ConcurrentDictionary<Type, String> _tableMaps = new();
    private String OnGetTableName(Type type)
    {
        if (GetTableName == null) return null;

        return _tableMaps.GetOrAdd(type, t => GetTableName(t));
    }

    private ConcurrentDictionary<Type, String> _keyMaps = new();
    private String OnGetKeyName(Type type)
    {
        if (GetKeyName == null) return null;

        return _keyMaps.GetOrAdd(type, t => GetKeyName(t));
    }

    /// <summary>插入数据</summary>
    /// <param name="data">实体对象</param>
    /// <param name="tableName">表名</param>
    /// <returns></returns>
    public Int32 Insert(Object data, String tableName = null)
    {
        if (tableName.IsNullOrEmpty() && GetTableName != null) tableName = OnGetTableName(data.GetType());
        if (tableName.IsNullOrEmpty()) tableName = data.GetType().Name;

        var pis = data.ToDictionary();
        var dps = Db.CreateParameters(data);
        var ns = pis.Join(",", e => e.Key);
        var vs = dps.Join(",", e => e.ParameterName);
        var sql = $"Insert Into {tableName}({ns}) Values({vs})";

        return ExecuteByCache(sql, "", dps, (s, t, p) => Session.Execute(s, CommandType.Text, p));
    }

    /// <summary>插入数据表。多行数据循环插入，非批量</summary>
    /// <param name="table">表定义</param>
    /// <param name="columns">字段列表，为空表示所有字段</param>
    /// <param name="data">数据对象</param>
    /// <param name="mode">保存模式，默认Insert</param>
    /// <returns></returns>
    public Int32 Insert(DbTable data, IDataTable table, IDataColumn[] columns = null, SaveModes mode = SaveModes.Insert)
    {
        var rs = 0;
        foreach (var row in data)
        {
            rs += Insert(row, table, columns, mode);
        }
        return rs;
    }

    /// <summary>插入数据行</summary>
    /// <param name="table">表定义</param>
    /// <param name="columns">字段列表，为空表示所有字段</param>
    /// <param name="data">数据对象</param>
    /// <param name="mode">保存模式，默认Insert</param>
    /// <returns></returns>
    public Int32 Insert(IExtend data, IDataTable table, IDataColumn[] columns = null, SaveModes mode = SaveModes.Insert)
    {
        var builder = new InsertBuilder
        {
            Mode = mode,
            UseParameter = true
        };
        var sql = builder.GetSql(Db, table, columns, data);

        return ExecuteByCache(sql, "", builder.Parameters, (s, t, p) => Session.Execute(s, CommandType.Text, p));
    }

    /// <summary>更新数据。不支持自动识别主键</summary>
    /// <param name="data">实体对象</param>
    /// <param name="where">查询条件。默认使用Id字段</param>
    /// <param name="tableName">表名</param>
    /// <returns></returns>
    public Int32 Update(Object data, Object where, String tableName = null)
    {
        if (tableName.IsNullOrEmpty() && GetTableName != null) tableName = OnGetTableName(data.GetType());
        if (tableName.IsNullOrEmpty()) tableName = data.GetType().Name;

        var sb = Pool.StringBuilder.Get();
        sb.Append("Update ");
        sb.Append(tableName);

        var dps = new List<IDataParameter>();
        // Set参数
        {
            sb.Append(" Set ");
            var i = 0;
            foreach (var pi in data.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append(',');

                var p = Db.CreateParameter(pi.Name, pi.GetValue(data, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        // Where条件
        if (where != null)
        {
            sb.Append(" Where ");
            var i = 0;
            foreach (var pi in where.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append(" And ");

                var p = Db.CreateParameter(pi.Name, pi.GetValue(where, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        else
        {
            var name = OnGetKeyName(data.GetType());
            if (name.IsNullOrEmpty()) name = "Id";

            var pi = data.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null) throw new XCodeException($"更新实体对象时未标记主键且未设置where");

            sb.Append(" Where ");

            var p = Db.CreateParameter(pi.Name, pi.GetValue(data, null), pi.PropertyType);
            dps.Add(p);
            sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
        }

        var sql = sb.Put(true);

        return ExecuteByCache(sql, "", dps.ToArray(), (s, t, p) => Session.Execute(s, CommandType.Text, p));
    }

    /// <summary>删除数据</summary>
    /// <param name="tableName">表名</param>
    /// <param name="where">查询条件</param>
    /// <returns></returns>
    public Int32 Delete(String tableName, Object where)
    {
        if (tableName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(tableName));

        var sb = Pool.StringBuilder.Get();
        sb.Append("Delete From ");
        sb.Append(tableName);

        // 带上参数化的Where条件
        var dps = new List<IDataParameter>();
        if (where != null)
        {
            sb.Append(" Where ");
            var i = 0;
            foreach (var pi in where.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append("And ");

                var p = Db.CreateParameter(pi.Name, pi.GetValue(where, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        var sql = sb.Put(true);

        return ExecuteByCache(sql, "", dps.ToArray(), (s, t, p) => Session.Execute(s, CommandType.Text, p));
    }

    /// <summary>插入数据</summary>
    /// <param name="data">实体对象</param>
    /// <param name="tableName">表名</param>
    /// <returns></returns>
    public Task<Int32> InsertAsync(Object data, String tableName = null)
    {
        if (tableName.IsNullOrEmpty() && GetTableName != null) tableName = OnGetTableName(data.GetType());
        if (tableName.IsNullOrEmpty()) tableName = data.GetType().Name;

        var pis = data.ToDictionary();
        var dps = Db.CreateParameters(data);
        var ns = pis.Join(",", e => e.Key);
        var vs = dps.Join(",", e => e.ParameterName);
        var sql = $"Insert Into {tableName}({ns}) Values({vs})";

        return ExecuteByCacheAsync(sql, "", dps, (s, t, p) => Session.ExecuteAsync(s, CommandType.Text, p));
    }

    /// <summary>更新数据。不支持自动识别主键</summary>
    /// <param name="data">实体对象</param>
    /// <param name="where">查询条件。默认使用Id字段</param>
    /// <param name="tableName">表名</param>
    /// <returns></returns>
    public Task<Int32> UpdateAsync(Object data, Object where, String tableName = null)
    {
        if (tableName.IsNullOrEmpty() && GetTableName != null) tableName = OnGetTableName(data.GetType());
        if (tableName.IsNullOrEmpty()) tableName = data.GetType().Name;

        var sb = Pool.StringBuilder.Get();
        sb.Append("Update ");
        sb.Append(tableName);

        var dps = new List<IDataParameter>();
        // Set参数
        {
            sb.Append(" Set ");
            var i = 0;
            foreach (var pi in data.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append(',');

                var p = Db.CreateParameter(pi.Name, pi.GetValue(data, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        // Where条件
        if (where != null)
        {
            sb.Append(" Where ");
            var i = 0;
            foreach (var pi in where.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append(" And ");

                var p = Db.CreateParameter(pi.Name, pi.GetValue(where, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        else
        {
            var name = OnGetKeyName(data.GetType());
            if (name.IsNullOrEmpty()) name = "Id";

            var pi = data.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null) throw new XCodeException($"更新实体对象时未标记主键且未设置where");

            sb.Append(" Where ");

            var p = Db.CreateParameter(pi.Name, pi.GetValue(data, null), pi.PropertyType);
            dps.Add(p);
            sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
        }

        var sql = sb.Put(true);

        return ExecuteByCacheAsync(sql, "", dps.ToArray(), (s, t, p) => Session.ExecuteAsync(s, CommandType.Text, p));
    }

    /// <summary>删除数据</summary>
    /// <param name="tableName">表名</param>
    /// <param name="where">查询条件</param>
    /// <returns></returns>
    public Task<Int32> DeleteAsync(String tableName, Object where)
    {
        if (tableName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(tableName));

        var sb = Pool.StringBuilder.Get();
        sb.Append("Delete From ");
        sb.Append(tableName);

        // 带上参数化的Where条件
        var dps = new List<IDataParameter>();
        if (where != null)
        {
            sb.Append(" Where ");
            var i = 0;
            foreach (var pi in where.GetType().GetProperties(true))
            {
                if (i++ > 0) sb.Append("And ");

                var p = Db.CreateParameter(pi.Name, pi.GetValue(where, null), pi.PropertyType);
                dps.Add(p);
                sb.AppendFormat("{0}={1}", pi.Name, p.ParameterName);
            }
        }
        var sql = sb.Put(true);

        return ExecuteByCacheAsync(sql, "", dps.ToArray(), (s, t, p) => Session.ExecuteAsync(s, CommandType.Text, p));
    }

    /// <summary>插入数据</summary>
    /// <param name="tableName"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    [Obsolete]
    public Int32 Insert(String tableName, Object data) => Insert(data, tableName);

    /// <summary>更新数据</summary>
    /// <param name="tableName"></param>
    /// <param name="data"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    [Obsolete]
    public Int32 Update(String tableName, Object data, Object where) => Update(data, where, tableName);
    #endregion
}

partial class DAL
{
    static DAL()
    {
        var ioc = ObjectContainer.Current;
        ioc.AddTransient<IDataTable, XTable>();

        InitLog();
        InitConnections();
    }

    #region Sql日志输出
    /// <summary>是否调试</summary>
    public static Boolean Debug { get; set; } = Setting.Current.Debug;

    /// <summary>输出日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public static void WriteLog(String format, params Object[] args)
    {
        if (!Debug) return;

        //InitLog();
        XTrace.WriteLine(format, args);
    }

    /// <summary>输出日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    [Conditional("DEBUG")]
    public static void WriteDebugLog(String format, params Object[] args)
    {
        if (!Debug) return;

        //InitLog();
        XTrace.WriteLine(format, args);
    }

    static Int32 hasInitLog = 0;
    internal static void InitLog()
    {
        if (Interlocked.CompareExchange(ref hasInitLog, 1, 0) > 0) return;

        // 输出当前版本
        System.Reflection.Assembly.GetExecutingAssembly().WriteVersion();

        if (Setting.Current.ShowSQL)
            XTrace.WriteLine("当前配置为输出SQL日志，如果觉得日志过多，可以修改配置关闭[Config/XCode.config:ShowSQL=false]。");
    }
    #endregion

    #region SQL拦截器
    private static readonly ThreadLocal<Action<String>> _filter = new();
    /// <summary>本地过滤器（本线程SQL拦截）</summary>
    public static Action<String> LocalFilter { get => _filter.Value; set => _filter.Value = value; }

    /// <summary>APM跟踪器</summary>
    public ITracer Tracer { get; set; } = GlobalTracer;

    /// <summary>全局APM跟踪器</summary>
    public static ITracer GlobalTracer { get; set; } = DefaultTracer.Instance;
    #endregion

    #region 辅助函数
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Db.ToString();

    /// <summary>建立数据表对象</summary>
    /// <returns></returns>
    internal static IDataTable CreateTable() => ObjectContainer.Current.Resolve<IDataTable>();

    /// <summary>是否支持批操作</summary>
    /// <returns></returns>
    public Boolean SupportBatch
    {
        get
        {
            if (DbType is DatabaseType.MySql or DatabaseType.Oracle or DatabaseType.SQLite or DatabaseType.PostgreSQL or DatabaseType.PostgreSQL) return true;

            // SqlServer对批处理有BUG，将在3.0中修复
            // https://github.com/dotnet/corefx/issues/29391
            if (DbType == DatabaseType.SqlServer) return true;

            return false;
        }
    }
    #endregion
}

public partial class DAL
{
    #region 备份
    /// <summary>备份单表数据</summary>
    /// <remarks>
    /// 最大支持21亿行
    /// </remarks>
    /// <param name="table">数据表</param>
    /// <param name="stream">目标数据流</param>
    /// <returns></returns>
    public Int32 Backup(IDataTable table, Stream stream)
    {
        var dpk = new DbPackage { Dal = this, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.Backup(table, stream);
    }

    /// <summary>备份单表数据到文件</summary>
    /// <param name="table">数据表</param>
    /// <param name="file">文件。.gz后缀时采用压缩</param>
    /// <returns></returns>
    public Int32 Backup(IDataTable table, String file = null)
    {
        var dpk = new DbPackage { Dal = this, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.Backup(table, file);
    }

    /// <summary>备份一批表到指定压缩文件</summary>
    /// <param name="tables">数据表集合</param>
    /// <param name="file">zip压缩文件</param>
    /// <param name="backupSchema">备份架构</param>
    /// <param name="ignoreError">忽略错误，继续恢复下一张表</param>
    /// <returns></returns>
    public Int32 BackupAll(IList<IDataTable> tables, String file, Boolean backupSchema = true, Boolean ignoreError = true)
    {
        var dpk = new DbPackage { Dal = this, IgnoreError = ignoreError, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.BackupAll(tables, file, backupSchema);
    }
    #endregion

    #region 恢复
    /// <summary>从数据流恢复数据</summary>
    /// <param name="stream">数据流</param>
    /// <param name="table">数据表</param>
    /// <returns></returns>
    public Int32 Restore(Stream stream, IDataTable table)
    {
        var dpk = new DbPackage { Dal = this, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.Restore(stream, table);
    }

    /// <summary>从文件恢复数据</summary>
    /// <param name="file">zip压缩文件</param>
    /// <param name="table">数据表</param>
    /// <param name="setSchema">是否设置数据表模型，自动建表</param>
    /// <returns></returns>
    public Int64 Restore(String file, IDataTable table, Boolean setSchema = true)
    {
        var dpk = new DbPackage { Dal = this, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.Restore(file, table, setSchema);
    }

    /// <summary>从指定压缩文件恢复一批数据到目标库</summary>
    /// <param name="file">zip压缩文件</param>
    /// <param name="tables">数据表。为空时从压缩包读取xml模型文件</param>
    /// <param name="setSchema">是否设置数据表模型，自动建表</param>
    /// <param name="ignoreError">忽略错误，继续下一张表</param>
    /// <returns></returns>
    public IDataTable[] RestoreAll(String file, IDataTable[] tables = null, Boolean setSchema = true, Boolean ignoreError = true)
    {
        var dpk = new DbPackage { Dal = this, IgnoreError = ignoreError, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.RestoreAll(file, tables, setSchema);
    }
    #endregion

    #region 同步
    /// <summary>同步单表数据</summary>
    /// <remarks>
    /// 把数据同一张表同步到另一个库
    /// </remarks>
    /// <param name="table">数据表</param>
    /// <param name="connName">目标连接名</param>
    /// <param name="syncSchema">同步架构</param>
    /// <returns></returns>
    public Int32 Sync(IDataTable table, String connName, Boolean syncSchema = true)
    {
        var dpk = new DbPackage { Dal = this, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.Sync(table, connName, syncSchema);
    }

    /// <summary>备份一批表到另一个库</summary>
    /// <param name="tables">表名集合</param>
    /// <param name="connName">目标连接名</param>
    /// <param name="syncSchema">同步架构</param>
    /// <param name="ignoreError">忽略错误，继续下一张表</param>
    /// <returns></returns>
    public IDictionary<String, Int32> SyncAll(IDataTable[] tables, String connName, Boolean syncSchema = true, Boolean ignoreError = true)
    {
        var dpk = new DbPackage { Dal = this, IgnoreError = ignoreError, Tracer = Tracer ?? GlobalTracer, Log = XTrace.Log };
        return dpk.SyncAll(tables, connName, syncSchema);
    }
    #endregion
}