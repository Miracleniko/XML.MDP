namespace XML.Core.Security;

/// <summary>密码提供者</summary>
public interface IPasswordProvider
{
    /// <summary>对密码进行散列处理，此处可以加盐，结果保存在数据库</summary>
    /// <param name="password"></param>
    /// <returns></returns>
    String Hash(String password);

    /// <summary>验证密码散列，包括加盐判断</summary>
    /// <param name="password"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    Boolean Verify(String password, String hash);
}