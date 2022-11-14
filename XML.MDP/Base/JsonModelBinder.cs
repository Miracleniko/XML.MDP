using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace XML.MDP;

/// <summary>Json模型绑定器</summary>
public class JsonModelBinder : IModelBinder
{
    private readonly IDictionary<ModelMetadata, IModelBinder> _propertyBinders;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>实例化分页模型绑定器</summary>
    /// <param name="propertyBinders"></param>
    /// <param name="loggerFactory"></param>
    public JsonModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory)
    {
        _propertyBinders = propertyBinders ?? throw new ArgumentNullException(nameof(propertyBinders));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>对于Json请求，从body中读取参数</summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var req = bindingContext.HttpContext.Request;

        var modelType = bindingContext.ModelType;

        var entityBody = req.GetRequestBody(modelType);

        if (entityBody != null)
        {
            bindingContext.Result = ModelBindingResult.Success(entityBody);
        }
        else
        {
            var modelBinder = new ComplexTypeModelBinder(_propertyBinders, _loggerFactory);
            await modelBinder.BindModelAsync(bindingContext);
        }
    }
}