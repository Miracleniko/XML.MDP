namespace XML.Core.Configuration;

/// <summary>配置对象</summary>
public interface IConfigSection
{
    /// <summary>配置名</summary>
    String Key { get; set; }

    /// <summary>配置值</summary>
    String Value { get; set; }

    /// <summary>注释</summary>
    String Comment { get; set; }

    /// <summary>子级</summary>
    IList<IConfigSection> Childs { get; set; }

    /// <summary>获取 或 设置 配置值</summary>
    /// <param name="key">配置名，支持冒号分隔的多级名称</param>
    /// <returns></returns>
    String this[String key] { get; set; }
}