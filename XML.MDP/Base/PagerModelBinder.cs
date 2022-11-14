namespace XML.MDP;

/// <summary>分页模型绑定器</summary>
public class PagerModelBinder : IModelBinder
{
    private readonly
#nullable disable
    IDictionary<ModelMetadata, IModelBinder> _propertyBinders;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>实例化分页模型绑定器</summary>
    /// <param name="propertyBinders"></param>
    /// <param name="loggerFactory"></param>
    public PagerModelBinder(
      IDictionary<ModelMetadata, IModelBinder> propertyBinders,
      ILoggerFactory loggerFactory)
    {
        this._propertyBinders = propertyBinders ?? throw new ArgumentNullException(nameof(propertyBinders));
        this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>创建模型。对于有Key的请求，使用FindByKeyForEdit方法先查出来数据，而不是直接反射实例化实体对象</summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        Type modelType = bindingContext.ModelType;
        if (!(modelType == typeof(Pager)) && !(modelType == typeof(PageParameter)))
            return;
        Pager model = new Pager()
        {
            Params = WebHelper.Params
        };
        RouteValueDictionary values = bindingContext.ActionContext.RouteData.Values;
        object obj;
        if (!model.Params.ContainsKey("id") && values.TryGetValue("id", out obj))
            model.Params["id"] = obj?.ToString() ?? "";
        if (bindingContext.HttpContext.Request.GetRequestBody<object>() is NullableDictionary<string, object> requestBody)
        {
            foreach ((ModelMetadata key, IModelBinder _) in (IEnumerable<KeyValuePair<ModelMetadata, IModelBinder>>)this._propertyBinders)
            {
                string str = requestBody[key.Name]?.ToString();
                if (!StringHelper.IsNullOrWhiteSpace(str))
                    model[key.Name] = str;
            }
            bindingContext.Result = ModelBindingResult.Success((object)model);
        }
        else
        {
            ComplexTypeModelBinder complexTypeModelBinder = new ComplexTypeModelBinder(this._propertyBinders, this._loggerFactory);
            bindingContext.Model = (object)model;
            await complexTypeModelBinder.BindModelAsync(bindingContext);
        }
    }
}