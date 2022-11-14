using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.Models;

/// <summary>节点登录信息</summary>
public class LoginInfo
{
    #region 属性
    /// <summary>节点编码</summary>
    public String Code { get; set; }

    /// <summary>节点密钥</summary>
    public String Secret { get; set; }

    /// <summary>产品编码</summary>
    public String ProductCode { get; set; }

    /// <summary>节点信息</summary>
    public NodeInfo Node { get; set; }
    #endregion
}