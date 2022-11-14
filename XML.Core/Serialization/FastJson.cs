namespace XML.Core.Serialization;

class FastJson : IJsonHost
{
    #region IJsonHost 成员
    public String Write(Object value, Boolean indented, Boolean nullValue, Boolean camelCase) => JsonWriter.ToJson(value, indented, nullValue, camelCase);

    public Object Read(String json, Type type) => new JsonReader().Read(json, type);

    public Object Convert(Object obj, Type targetType) => new JsonReader().ToObject(obj, targetType, null);
    #endregion
}