using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XML.Core.Data;

namespace XML.MDP;

/// <summary>分页模型绑定器提供者</summary>
public class PagerModelBinderProvider : IModelBinderProvider
{
    /// <summary>获取绑定器</summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        if (modelType == typeof(Pager) || modelType == typeof(PageParameter))
        {
            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            foreach (var property in context.Metadata.Properties)
            {
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new PagerModelBinder(propertyBinders, loggerFactory);
        }

        return null;
    }
}