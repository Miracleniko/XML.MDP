using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Transform;

/// <summary>数据抽取接口</summary>
/// <typeparam name="T"></typeparam>
public interface IExtracter<T>
{
    #region 属性
    /// <summary>开始行。分页时表示偏移行数，自增时表示下一个编号，默认0</summary>
    Int64 Row { get; set; }
    #endregion

    #region 抽取数据
    /// <summary>迭代抽取数据</summary>
    /// <returns></returns>
    IEnumerable<T> Fetch();
    #endregion
}