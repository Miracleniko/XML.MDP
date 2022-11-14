using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core;
using XML.XCode.Configuration;

namespace XML.XCode;

/// <summary>映射提供者</summary>
public class MapProvider
{
    #region 属性
    /// <summary>实体类型</summary>
    public Type EntityType { get; set; }

    /// <summary>关联键</summary>
    public String Key { get; set; }
    #endregion

    #region 方法
    /// <summary>获取数据源</summary>
    /// <returns></returns>
    public virtual IDictionary<Object, String> GetDataSource()
    {
        var fact = EntityType.AsFactory();

        var key = Key;
        var mst = fact.Master?.Name;

        if (key.IsNullOrEmpty()) key = fact.Unique?.Name;
        if (key.IsNullOrEmpty()) throw new ArgumentNullException("没有设置关联键", nameof(Key));
        if (mst.IsNullOrEmpty()) throw new ArgumentNullException("没有设置主要字段");

        // 修正字段大小写，用户书写Map特性时，可能把字段名大小写写错
        if (fact.Table.FindByName(key) is FieldItem fi)
        {
            key = fi.Name;
        }

        // 数据较少时，从缓存读取
        var list = fact.Session.Count < 1000 ? fact.FindAllWithCache() : fact.FindAll("", null, null, 0, 100);

        return list.Where(e => e[key] != null).ToDictionary(e => e[key], e => e[mst] + "");
    }
    #endregion
}