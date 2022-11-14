namespace XML.Core.Data;

/// <summary>具有可读写的扩展数据</summary>
public interface IExtend
{
    /// <summary>设置 或 获取 数据项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Object this[String key] { get; set; }
}