using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Common;

/// <summary>数据模拟</summary>
/// <typeparam name="T"></typeparam>
public class DataSimulation<T> : DataSimulation where T : Entity<T>, new()
{
    /// <summary>实例化</summary>
    public DataSimulation() => Factory = Entity<T>.Meta.Factory;
}
