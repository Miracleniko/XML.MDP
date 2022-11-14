namespace XML.Core.Model;

internal class ServiceProvider : IServiceProvider
{
    private readonly IObjectContainer _container;
    /// <summary>容器</summary>
    public IObjectContainer Container => _container;

    public ServiceProvider(IObjectContainer container) => _container = container;

    public Object GetService(Type serviceType)
    {
        if (serviceType == typeof(IObjectContainer)) return _container;
        if (serviceType == typeof(IServiceProvider)) return this;

        return _container.Resolve(serviceType);
    }
}