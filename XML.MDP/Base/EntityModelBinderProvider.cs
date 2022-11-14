using Microsoft.AspNetCore.Mvc.ModelBinding;
using XML.Core.Log;
using XML.Core.Reflection;
using XML.XCode;

namespace XML.MDP;

/// <summary>实体模型绑定器提供者，为所有XCode实体类提供实体模型绑定器</summary>
public class EntityModelBinderProvider : IModelBinderProvider
{
    /// <summary>获取绑定器</summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!context.Metadata.ModelType.As<IEntity>()) return null;

        var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
        var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
        foreach (var property in context.Metadata.Properties)
        {
            propertyBinders.Add(property, context.CreateBinder(property));
        }

        return new EntityModelBinder(propertyBinders, loggerFactory);
    }

    /// <summary>实例化</summary>
    public EntityModelBinderProvider() => XTrace.WriteLine("注册实体模型绑定器：{0}", typeof(EntityModelBinderProvider).FullName);
}