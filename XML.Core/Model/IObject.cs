namespace XML.Core.Model;

/// <summary>对象映射接口</summary>
public interface IObject
{
    /// <summary>服务类型</summary>
    Type ServiceType { get; }

    /// <summary>实现类型</summary>
    Type ImplementationType { get; }

    /// <summary>生命周期</summary>
    ObjectLifetime Lifttime { get; }
}