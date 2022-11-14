using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Model;
using XML.Core.Security;

namespace XML.XCode.Membership;

/// <summary>管理提供者接口</summary>
/// <remarks>
/// 管理提供者接口主要提供（或统一规范）用户提供者定位、用户查找登录等功能。
/// 只需要一个实现IManageUser接口的用户类即可实现IManageProvider接口。
/// IManageProvider足够精简，使得大多数用户可以自定义实现；
/// 也因为其简单稳定，大多数需要涉及用户与权限功能的操作，均可以直接使用该接口。
/// </remarks>
public interface IManageProvider : IServiceProvider
{
    /// <summary>当前登录用户，设为空则注销登录</summary>
    IManageUser Current { get; set; }

    /// <summary>密码提供者</summary>
    IPasswordProvider PasswordProvider { get; set; }

    /// <summary>获取当前用户</summary>
    /// <param name="context"></param>
    /// <returns></returns>
    IManageUser GetCurrent(IServiceProvider context);

    /// <summary>设置当前用户</summary>
    /// <param name="user"></param>
    /// <param name="context"></param>
    void SetCurrent(IManageUser user, IServiceProvider context);

    /// <summary>根据用户编号查找</summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    IManageUser FindByID(Object userid);

    /// <summary>根据用户帐号查找</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IManageUser FindByName(String name);

    /// <summary>登录</summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="rememberme">是否记住密码</param>
    /// <returns></returns>
    IManageUser Login(String username, String password, Boolean rememberme = false);

    /// <summary>注销</summary>
    void Logout();

    /// <summary>注册用户</summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <param name="roleid">角色</param>
    /// <param name="enable">是否启用</param>
    /// <returns></returns>
    IManageUser Register(String username, String password, Int32 roleid = 0, Boolean enable = false);

    /// <summary>修改密码</summary>
    /// <param name="username">用户名</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="oldPassword">旧密码，如果未指定，则不校验</param>
    /// <returns></returns>
    IManageUser ChangePassword(String username, String newPassword, String oldPassword);

    /// <summary>获取服务</summary>
    /// <remarks>
    /// 其实IServiceProvider有该扩展方法，但是在FX2里面不方面使用，所以这里保留
    /// </remarks>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService GetService<TService>();
}