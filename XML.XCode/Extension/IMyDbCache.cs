using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Extension;

/// <summary>数据缓存接口</summary>
public partial interface IMyDbCache
{
    #region 属性
    /// <summary>名称</summary>
    String Name { get; set; }

    /// <summary>键值</summary>
    String Value { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>过期时间</summary>
    DateTime ExpiredTime { get; set; }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    Object this[String name] { get; set; }
    #endregion
}