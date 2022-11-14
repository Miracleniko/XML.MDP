using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Net;
using XML.Remoting.Http;

namespace XML.Remoting;

class ApiHttpServer : ApiNetServer
{
    #region 属性
    //private String RawUrl;
    #endregion

    public ApiHttpServer()
    {
        Name = "Http";

        ProtocolType = NetType.Http;
    }

    /// <summary>初始化</summary>
    /// <param name="config"></param>
    /// <param name="host"></param>
    /// <returns></returns>
    public override Boolean Init(Object config, IApiHost host)
    {
        Host = host;

        var uri = config as NetUri;
        Port = uri.Port;

        //RawUrl = uri + "";

        // Http封包协议
        //Add<HttpCodec>();
        Add(new HttpCodec { AllowParseHeader = true });

        //host.Handler = new ApiHttpHandler { Host = host };
        host.Encoder = new HttpEncoder();

        return true;
    }
}