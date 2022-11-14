namespace XML.Core.Web;

/// <summary>Jwt编码委托</summary>
/// <param name="data"></param>
/// <param name="secrect"></param>
/// <returns></returns>
public delegate Byte[] JwtEncodeDelegate(Byte[] data, String secrect);