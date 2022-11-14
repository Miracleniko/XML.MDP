namespace XML.Core.Security;

/// <summary>MD5密码提供者</summary>
public class MD5PasswordProvider : IPasswordProvider
{
    /// <summary>对密码进行散列处理，此处可以加盐，结果保存在数据库</summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public String Hash(String password) => password.MD5();

    /// <summary>验证密码散列，包括加盐判断</summary>
    /// <param name="password"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public Boolean Verify(String password, String hash) => password.MD5().EqualIgnoreCase(hash);
}