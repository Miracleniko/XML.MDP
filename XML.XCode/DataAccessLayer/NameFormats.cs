using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>名称格式化</summary>
public enum NameFormats
{
    /// <summary>原样</summary>
    Default = 0,

    /// <summary>全大写</summary>
    Upper,

    /// <summary>全小写</summary>
    Lower,

    /// <summary>下划线</summary>
    Underline,
}