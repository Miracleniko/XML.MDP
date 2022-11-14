using System.Collections;

namespace XML.Core.Data;

internal class ExtendDictionary2 : IDictionary<String, Object>
{
    public IExtend Data { get; set; }

    public ICollection<String> Keys { get; set; }

    public Object this[String key] { get => Data[key]; set => Data[key] = value; }

    public ICollection<Object> Values => Keys.Select(e => Data[e]).ToList();

    public Int32 Count => Keys.Count;

    public Boolean IsReadOnly => false;

    public void Add(String key, Object value) => throw new NotImplementedException();

    public void Add(KeyValuePair<String, Object> item) => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();

    public Boolean Contains(KeyValuePair<String, Object> item) => Keys.Contains(item.Key);

    public Boolean ContainsKey(String key) => Keys.Contains(key);

    public void CopyTo(KeyValuePair<String, Object>[] array, Int32 arrayIndex) => throw new NotImplementedException();

    public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
    {
        foreach (var item in Keys)
        {
            yield return new KeyValuePair<String, Object>(item, Data[item]);
        }
    }

    public Boolean Remove(String key) => throw new NotImplementedException();

    public Boolean Remove(KeyValuePair<String, Object> item) => throw new NotImplementedException();

    public Boolean TryGetValue(String key, out Object value)
    {
        if (Keys.Contains(key))
        {
            value = Data[key];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}