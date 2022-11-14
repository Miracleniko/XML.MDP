using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>模型字段排序模式。其实不是很重要，仅仅影响数据字段在数据表中的先后顺序而已</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ModelSortModeAttribute : Attribute
{
    /// <summary>模式</summary>
    public ModelSortModes Mode { get; set; }

    /// <summary>指定实体类的模型字段排序模式</summary>
    /// <param name="mode"></param>
    public ModelSortModeAttribute(ModelSortModes mode) { Mode = mode; }
}