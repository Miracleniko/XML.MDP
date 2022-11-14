using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>反向工程</summary>
public enum Migration
{
    /// <summary>关闭</summary>
    Off = 0,

    /// <summary>只读。异步检查差异，不执行</summary>
    ReadOnly = 1,

    /// <summary>默认。新建表结构</summary>
    On = 2,

    /// <summary>完全。新建、修改、删除</summary>
    Full = 3
}