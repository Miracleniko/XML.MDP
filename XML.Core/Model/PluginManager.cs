using System.Reflection;
using XML.Core.Log;
using XML.Core.Reflection;

namespace XML.Core.Model;

/// <summary>插件管理器</summary>
public class PluginManager : DisposeBase, IServiceProvider
{
    #region 属性
    /// <summary>宿主标识，用于供插件区分不同宿主</summary>
    public String Identity { get; set; }

    /// <summary>宿主服务提供者</summary>
    public IServiceProvider Provider { get; set; }

    /// <summary>插件集合</summary>
    public IPlugin[] Plugins { get; set; }

    /// <summary>日志提供者</summary>
    public ILog Log { get; set; } = XTrace.Log;
    #endregion

    #region 构造
    /// <summary>实例化一个插件管理器</summary>
    public PluginManager() { }

    ///// <summary>使用宿主对象实例化一个插件管理器</summary>
    ///// <param name="host"></param>
    //public PluginManager(Object host)
    //{
    //    if (host != null)
    //    {
    //        Identity = host.ToString();
    //        Provider = host as IServiceProvider;
    //    }
    //}

    /// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
    /// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
    /// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            Plugins.TryDispose();
            Plugins = null;
        }
    }
    #endregion

    #region 方法
    /// <summary>加载插件。此时是加载所有插件，无法识别哪些是需要的</summary>
    public void Load()
    {
        var list = new List<IPlugin>();
        // 此时是加载所有插件，无法识别哪些是需要的
        foreach (var item in LoadPlugins())
        {
            if (item != null)
            {
                try
                {
                    if (item.CreateInstance() is IPlugin plugin) list.Add(plugin);
                }
                catch (Exception ex)
                {
                    Log?.Debug(null, ex);
                }
            }
        }
        Plugins = list.ToArray();
    }

    IEnumerable<Type> LoadPlugins()
    {
        // 此时是加载所有插件，无法识别哪些是需要的
        foreach (var item in AssemblyX.FindAllPlugins(typeof(IPlugin), true))
        {
            if (item != null)
            {
                // 如果有插件特性，并且所有特性都不支持当前宿主，则跳过
                var atts = item.GetCustomAttributes<PluginAttribute>(true);
                if (atts != null && atts.Any(a => a.Identity != Identity)) continue;

                yield return item;
            }
        }
    }

    /// <summary>开始初始化。初始化之后，不属于当前宿主的插件将会被过滤掉</summary>
    public void Init()
    {
        var ps = Plugins;
        if (ps == null || ps.Length <= 0) return;

        var list = new List<IPlugin>();
        foreach (var item in ps)
        {
            try
            {
                if (item.Init(Identity, this)) list.Add(item);
            }
            catch (Exception ex)
            {
                Log?.Debug(null, ex);
            }
        }

        Plugins = list.ToArray();
    }
    #endregion

    #region IServiceProvider 成员
    Object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(PluginManager)) return this;

        return Provider?.GetService(serviceType);
    }
    #endregion
}