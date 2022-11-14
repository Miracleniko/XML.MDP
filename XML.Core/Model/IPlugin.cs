namespace XML.Core.Model;

/// <summary>通用插件接口</summary>
/// <remarks>
/// 为了方便构建一个简单通用的插件系统，先规定如下：
/// 1，负责加载插件的宿主，在加载插件后会进行插件实例化，此时可在插件构造函数中做一些事情，但不应该开始业务处理，因为宿主的准备工作可能尚未完成
/// 2，宿主一切准备就绪后，会顺序调用插件的Init方法，并将宿主标识传入，插件通过标识区分是否自己的目标宿主。插件的Init应尽快完成。
/// 3，如果插件实现了<see cref="T:System.IDisposable" />接口，宿主最后会清理资源。
/// </remarks>
public interface IPlugin
{
    /// <summary>初始化</summary>
    /// <param name="identity">插件宿主标识</param>
    /// <param name="provider">服务提供者</param>
    /// <returns>返回初始化是否成功。如果当前宿主不是所期待的宿主，这里返回false</returns>
    Boolean Init(String identity, IServiceProvider provider);
}