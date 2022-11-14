using Microsoft.AspNetCore.Mvc.ModelBinding;
using XML.Core.Configuration;
using XML.Core.Reflection;

namespace XML.MDP;

/// <summary>Json模型绑定器提供者</summary>
public class JsonModelBinderProvider : IModelBinderProvider
{
    /// <summary>获取绑定器</summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        var isGenericType = false;
        if (modelType.BaseType?.FullName != null && modelType.BaseType.FullName.StartsWith("NewLife.Configuration.Config`1["))
        {
            var genericType = typeof(Config<>).MakeGenericType(modelType);
            isGenericType = genericType.FullName != null && modelType.As(genericType);
        }

        if (modelType.As<ICubeModel>() || isGenericType)
        {
            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            foreach (var property in context.Metadata.Properties)
            {
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new JsonModelBinder(propertyBinders, loggerFactory);
        }

        return null;
    }
}