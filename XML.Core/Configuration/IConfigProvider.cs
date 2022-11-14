namespace XML.Core.Configuration;

/// <summary>配置提供者</summary>
/// <remarks>
/// 建立树状配置数据体系，以分布式配置中心为核心，支持基于key的索引读写，也支持Load/Save/Bind的实体模型转换。
/// key索引支持冒号分隔的多层结构，在配置中心中不同命名空间使用不同提供者实例，在文件配置中不同文件使用不同提供者实例。
/// 
/// 一个配置类，支持从不同持久化提供者读取，可根据需要选择配置持久化策略。
/// 例如，小系统采用ini/xml/json文件配置，分布式系统采用配置中心。
/// 
/// 可通过实现IConfigMapping接口来自定义映射配置到模型实例。
/// </remarks>
public interface IConfigProvider
{
    /// <summary>名称</summary>
    String Name { get; set; }

    /// <summary>根元素</summary>
    IConfigSection Root { get; set; }

    /// <summary>所有键</summary>
    ICollection<String> Keys { get; }

    /// <summary>是否新的配置文件</summary>
    Boolean IsNew { get; set; }

    /// <summary>获取 或 设置 配置值</summary>
    /// <param name="key">配置名，支持冒号分隔的多级名称</param>
    /// <returns></returns>
    String this[String key] { get; set; }

    /// <summary>查找配置项。可得到子级和配置</summary>
    /// <param name="key">配置名</param>
    /// <returns></returns>
    IConfigSection GetSection(String key);

    /// <summary>配置改变事件。执行了某些动作，可能导致配置数据发生改变时触发</summary>
    event EventHandler Changed;

    /// <summary>返回获取配置的委托</summary>
    GetConfigCallback GetConfig { get; }

    /// <summary>从数据源加载数据到配置树</summary>
    Boolean LoadAll();

    /// <summary>保存配置树到数据源</summary>
    Boolean SaveAll();

    /// <summary>加载配置到模型</summary>
    /// <typeparam name="T">模型。可通过实现IConfigMapping接口来自定义映射配置到模型实例</typeparam>
    /// <param name="path">路径。配置树位置，配置中心等多对象混合使用时</param>
    /// <returns></returns>
    T Load<T>(String path = null) where T : new();

    /// <summary>保存模型实例</summary>
    /// <typeparam name="T">模型</typeparam>
    /// <param name="model">模型实例</param>
    /// <param name="path">路径。配置树位置，配置中心等多对象混合使用时</param>
    Boolean Save<T>(T model, String path = null);

    /// <summary>绑定模型，使能热更新，配置存储数据改变时同步修改模型属性</summary>
    /// <typeparam name="T">模型。可通过实现IConfigMapping接口来自定义映射配置到模型实例</typeparam>
    /// <param name="model">模型实例</param>
    /// <param name="autoReload">是否自动更新。默认true</param>
    /// <param name="path">命名空间。配置树位置，配置中心等多对象混合使用时</param>
    void Bind<T>(T model, Boolean autoReload = true, String path = null);

    /// <summary>绑定模型，使能热更新，配置存储数据改变时同步修改模型属性</summary>
    /// <typeparam name="T">模型。可通过实现IConfigMapping接口来自定义映射配置到模型实例</typeparam>
    /// <param name="model">模型实例</param>
    /// <param name="path">命名空间。配置树位置，配置中心等多对象混合使用时</param>
    /// <param name="onChange">配置改变时执行的委托</param>
    void Bind<T>(T model, String path, Action<IConfigSection> onChange);
}