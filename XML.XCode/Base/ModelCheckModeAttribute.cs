using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>模型检查模式</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ModelCheckModeAttribute : Attribute
{
    /// <summary>模式</summary>
    public ModelCheckModes Mode { get; set; }

    /// <summary>指定实体类的模型检查模式</summary>
    /// <param name="mode"></param>
    public ModelCheckModeAttribute(ModelCheckModes mode) { Mode = mode; }
}