using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>实体累加接口。实现Count=Count+123的效果</summary>
public interface IEntityAddition
{
    #region 属性
    /// <summary>实体对象</summary>
    IEntity Entity { get; set; }
    #endregion

    #region 累加
    /// <summary>设置累加字段</summary>
    /// <param name="names">字段集合</param>
    void Set(IEnumerable<String> names);

    /// <summary>获取快照</summary>
    /// <returns></returns>
    IDictionary<String, Object[]> Get();

    /// <summary>使用快照重置</summary>
    /// <param name="value"></param>
    void Reset(IDictionary<String, Object[]> value);
    #endregion
}