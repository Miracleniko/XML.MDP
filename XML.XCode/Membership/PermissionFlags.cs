using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Membership;

/// <summary>操作权限</summary>
[Flags]
[Description("操作权限")]
public enum PermissionFlags
{
    /// <summary>无权限</summary>
    [Description("无权限")]
    None = 0,

    /// <summary>查看权限</summary>
    [Description("查看")]
    Detail = 1,

    /// <summary>添加权限</summary>
    [Description("添加")]
    Insert = 2,

    /// <summary>修改权限</summary>
    [Description("修改")]
    Update = 4,

    /// <summary>删除权限</summary>
    [Description("删除")]
    Delete = 8,

    /// <summary>所有权限</summary>
    [Description("所有")]
    All = 0xFF,
}