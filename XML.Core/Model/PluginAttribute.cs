namespace XML.Core.Model;

/// <summary>插件特性。用于判断某个插件实现类是否支持某个宿主</summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginAttribute : Attribute
{
    /// <summary>插件宿主标识</summary>
    public String Identity { get; set; }

    /// <summary>实例化</summary>
    /// <param name="identity"></param>
    public PluginAttribute(String identity) => Identity = identity;
}