using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;

namespace XML.XCode;

/// <summary>在数据行和实体类之间映射数据的接口</summary>
public interface IDataRowEntityAccessor
{
    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <returns>实体数组</returns>
    IList<T> LoadData<T>(DataTable dt) where T : Entity<T>, new();

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">数据表</param>
    /// <returns>实体数组</returns>
    IList<T> LoadData<T>(DbTable ds) where T : Entity<T>, new();

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dr">数据读取器</param>
    /// <returns>实体数组</returns>
    IList<T> LoadData<T>(IDataReader dr) where T : Entity<T>, new();
}