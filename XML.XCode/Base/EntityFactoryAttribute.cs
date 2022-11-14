using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>指定实体工厂</summary>
public class EntityFactoryAttribute : Attribute
{
    /// <summary>实体工厂类型</summary>
    public Type Type { get; set; }

    /// <summary>指定实体工厂</summary>
    /// <param name="type"></param>
    public EntityFactoryAttribute(Type type) => Type = type;
}