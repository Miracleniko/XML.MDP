namespace XML.Core.Model;

/// <summary>生命周期</summary>
public enum ObjectLifetime
{
    /// <summary>单实例</summary>
    Singleton,

    ///// <summary>容器内单实例</summary>
    //Scoped,

    /// <summary>每次一个实例</summary>
    Transient
}