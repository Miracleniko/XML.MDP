using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Reflection;

namespace XML.XCode.Statistics;

/// <summary>统计模型</summary>
/// <typeparam name="T"></typeparam>
public class StatModel<T> : StatModel/*, IEqualityComparer<T>*/ where T : StatModel<T>, new()
{
    #region 方法
    /// <summary>拷贝</summary>
    /// <param name="model"></param>
    public virtual void Copy(T model)
    {
        Time = model.Time;
        Level = model.Level;
    }

    /// <summary>克隆到目标类型</summary>
    /// <returns></returns>
    public virtual T Clone()
    {
        var model = GetType().CreateInstance() as T;
        model.Copy(this);
        // 克隆不能格式化时间，否则会丢失时间精度
        //Time = GetDate(model.Level);

        return model;
    }

    /// <summary>分割为多个层级</summary>
    /// <param name="levels"></param>
    /// <returns></returns>
    public virtual List<T> Split(params StatLevels[] levels)
    {
        var list = new List<T>();
        foreach (var item in levels)
        {
            var st = Clone();
            st.Level = item;
            st.Time = st.GetDate(item);

            list.Add(st);
        }

        return list;
    }
    #endregion

    #region 相等比较
    ///// <summary>相等</summary>
    ///// <param name="x"></param>
    ///// <param name="y"></param>
    ///// <returns></returns>
    //public virtual Boolean Equals(T x, T y)
    //{
    //    if (x == null) return y == null;
    //    if (y != null) return false;

    //    return x.Level == y.Level && x.Time == y.Time;
    //}

    ///// <summary>获取哈希</summary>
    ///// <param name="obj"></param>
    ///// <returns></returns>
    //public virtual Int32 GetHashCode(T obj)
    //{
    //    return Level.GetHashCode() ^ Time.GetHashCode();
    //}
    #endregion
}