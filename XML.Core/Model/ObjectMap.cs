using System.Runtime.CompilerServices;

namespace XML.Core.Model;

/// <summary>对象映射</summary>
public class ObjectMap : IObject
{
    #region 属性
    /// <summary>服务类型</summary>
    public Type ServiceType { get; set; }

    /// <summary>实现类型</summary>
    public Type ImplementationType { get; set; }

    /// <summary>生命周期</summary>
    public ObjectLifetime Lifttime { get; set; }

    /// <summary>实例</summary>
    public Object Instance { get; set; }

    /// <summary>对象工厂</summary>
    public Func<IServiceProvider, Object> Factory { get; set; }
    #endregion

    #region 方法
    /// <summary>显示友好名称</summary>
    /// <returns></returns>
    public override String ToString() => $"[{ServiceType?.Name},{ImplementationType?.Name}]";
    #endregion
}