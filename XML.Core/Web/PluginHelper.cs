using System.Reflection;
using XML.Core.Configuration;
using XML.Core.Log;
using System.IO;

namespace XML.Core.Web;

public static class PluginHelper
{
    /// <summary>加载插件</summary>
    /// <param name="typeName"></param>
    /// <param name="disname"></param>
    /// <param name="dll"></param>
    /// <param name="linkName"></param>
    /// <param name="urls">提供下载地址的多个目标页面</param>
    /// <returns></returns>
    public static Type LoadPlugin(
      string typeName,
      string disname,
      string dll,
      string linkName,
      string urls = null)
    {
        Type type1 = Type.GetType(typeName);
        if (type1 != (Type)null)
            return type1;
        if (dll.IsNullOrEmpty())
            return (Type)null;
        Setting current = Config<Setting>.Current;
        string str = "";
        if (!dll.IsNullOrEmpty())
        {
            str = dll.GetFullPath();
            if (!File.Exists(str))
                str = dll.GetBasePath();
            if (!File.Exists(str))
                str = current.PluginPath.CombinePath(dll).GetFullPath();
            if (!File.Exists(str))
                str = current.PluginPath.CombinePath(dll).GetBasePath();
        }
        if (File.Exists(str))
        {
            try
            {
                Type type2 = Assembly.LoadFrom(str).GetType(typeName);
                if (type2 != (Type)null)
                    return type2;
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
        }
        if (linkName.IsNullOrEmpty())
            return (Type)null;
        lock (typeName)
        {
            if (urls.IsNullOrEmpty())
                urls = current.PluginServer;
            if (!File.Exists(str))
            {
                XTrace.WriteLine("{0}不存在或平台版本不正确，准备联网获取 {1}", (object)(disname ?? dll), (object)urls);
                WebClientX webClientX = new WebClientX();
                webClientX.Log = XTrace.Log;
                string directoryName = Path.GetDirectoryName(str);
                webClientX.DownloadLinkAndExtract(urls, linkName, directoryName);
                webClientX.TryDispose();
            }
            if (!File.Exists(str))
            {
                XTrace.WriteLine("未找到 {0} {1}", (object)disname, (object)dll);
                return (Type)null;
            }
            Type type3 = Type.GetType(typeName);
            if (type3 != (Type)null)
                return type3;
            if (File.Exists(str))
            {
                try
                {
                    Type type4 = Assembly.LoadFrom(str).GetType(typeName);
                    if (type4 != (Type)null)
                        return type4;
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                }
            }
            return (Type)null;
        }
    }
}