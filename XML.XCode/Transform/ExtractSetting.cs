using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XML.Core.Reflection;

namespace XML.XCode.Transform;

/// <summary>数据抽取参数</summary>
public class ExtractSetting : IExtractSetting
{
    #region 属性
    /// <summary>开始。大于等于</summary>
    [XmlIgnore, IgnoreDataMember]
    public DateTime Start { get; set; }

    /// <summary>结束。小于</summary>
    [XmlIgnore, IgnoreDataMember]
    public DateTime End { get; set; }

    /// <summary>时间偏移。距离实时时间的秒数，部分业务不能跑到实时</summary>
    [XmlIgnore, IgnoreDataMember]
    public Int32 Offset { get; set; }

    /// <summary>开始行。分页</summary>
    [XmlIgnore, IgnoreDataMember]
    public Int32 Row { get; set; }

    /// <summary>步进。最大区间大小，秒</summary>
    [XmlIgnore, IgnoreDataMember]
    public Int32 Step { get; set; }

    /// <summary>批大小</summary>
    [XmlIgnore, IgnoreDataMember]
    public Int32 BatchSize { get; set; } = 5000;

    ///// <summary>启用</summary>
    //public Boolean Enable { get; set; } = true;
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public ExtractSetting() { }

    /// <summary>实例化</summary>
    /// <param name="set"></param>
    public ExtractSetting(IExtractSetting set)
    {
        this.Copy(set);
    }
    #endregion
}