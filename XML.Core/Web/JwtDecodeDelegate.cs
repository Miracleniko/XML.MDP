namespace XML.Core.Web;

/// <summary>Jwt解码委托</summary>
/// <param name="data"></param>
/// <param name="secrect"></param>
/// <param name="signature"></param>
/// <returns></returns>
public delegate Boolean JwtDecodeDelegate(Byte[] data, String secrect, Byte[] signature);
