using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>模型解析器接口。解决名称大小写、去前缀、关键字等多个问题</summary>
public interface IModelResolver
{
    #region 名称处理
    /// <summary>获取别名。过滤特殊符号，过滤_之类的前缀</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    String GetName(String name);

    /// <summary>获取数据库名字。可以加上下划线</summary>
    /// <param name="name">名称</param>
    /// <param name="format">格式风格</param>
    /// <returns></returns>
    String GetDbName(String name, NameFormats format);

    /// <summary>根据字段名等信息计算索引的名称</summary>
    /// <param name="di"></param>
    /// <returns></returns>
    String GetName(IDataIndex di);

    /// <summary>获取显示名，如果描述不存在，则使用名称，否则使用描述前面部分，句号（中英文皆可）、换行分隔</summary>
    /// <param name="name">名称</param>
    /// <param name="description"></param>
    /// <returns></returns>
    String GetDisplayName(String name, String description);
    #endregion

    #region 模型处理
    /// <summary>修正数据</summary>
    /// <param name="table"></param>
    IDataTable Fix(IDataTable table);

    /// <summary>修正数据列</summary>
    /// <param name="column"></param>
    IDataColumn Fix(IDataColumn column);
    #endregion
}