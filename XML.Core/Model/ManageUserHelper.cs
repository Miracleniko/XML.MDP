using XML.Core.Net;

namespace XML.Core.Model;

/// <summary>用户接口工具类</summary>
public static class ManageUserHelper
{
    /// <summary>比较密码相等</summary>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public static Boolean CheckEqual(this IAuthUser user, String pass)
    {
        // 验证密码
        if (user.Password != pass) throw new Exception(user + " 密码错误");

        return true;
    }

    /// <summary>比较密码MD5</summary>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public static Boolean CheckMD5(this IAuthUser user, String pass)
    {
        // 验证密码
        if (user.Password != pass.MD5()) throw new Exception(user + " 密码错误");

        return true;
    }

    /// <summary>比较密码RC4</summary>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <returns></returns>
    public static Boolean CheckRC4(this IAuthUser user, String pass)
    {
        // 密码有盐值和密文两部分组成
        var p = pass.Length / 2;
        var salt = pass[..p].ToHex();
        pass = pass[p..];

        // 验证密码
        var tpass = user.Password.GetBytes();
        if (salt.RC4(tpass).ToHex() != pass) throw new Exception(user + " 密码错误");

        return true;
    }

    /// <summary>保存登录信息</summary>
    /// <param name="user"></param>
    /// <param name="session"></param>
    public static void SaveLogin(this IAuthUser user, INetSession session)
    {
        user.Logins++;
        user.LastLogin = DateTime.Now;

        if (session != null)
        {
            user.LastLoginIP = session.Remote?.Address + "";
            //// 销毁时
            //session.OnDisposed += (s, e) =>
            //{
            //    user.Online = false;
            //    user.Save();
            //};
        }
        //else
        //    user.LastLoginIP = WebHelper.UserHost;

        //user.Online = true;
        user.Save();
    }

    /// <summary>保存注册信息</summary>
    /// <param name="user"></param>
    /// <param name="session"></param>
    public static void SaveRegister(this IAuthUser user, INetSession session)
    {
        //user.Registers++;
        user.RegisterTime = DateTime.Now;
        //user.RegisterIP = ns.Remote.EndPoint.Address + "";

        if (session != null)
        {
            user.RegisterIP = session.Remote?.Address + "";
            //// 销毁时
            //session.OnDisposed += (s, e) =>
            //{
            //    user.Online = false;
            //    user.Save();
            //};
        }

        //user.Online = true;
        user.Save();
    }
}