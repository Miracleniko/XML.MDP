namespace XML.Core.Model;

/// <summary>认证用户接口，具有登录验证、注册、在线等基本信息</summary>
public interface IAuthUser : IManageUser
{
    #region 属性
    /// <summary>密码</summary>
    String Password { get; set; }

    ///// <summary>在线</summary>
    //Boolean Online { get; set; }

    /// <summary>登录次数</summary>
    Int32 Logins { get; set; }

    /// <summary>最后登录</summary>
    DateTime LastLogin { get; set; }

    /// <summary>最后登录IP</summary>
    String LastLoginIP { get; set; }

    /// <summary>注册时间</summary>
    DateTime RegisterTime { get; set; }

    /// <summary>注册IP</summary>
    String RegisterIP { get; set; }
    #endregion

    /// <summary>保存</summary>
    /// <returns></returns>
    Int32 Save();
}