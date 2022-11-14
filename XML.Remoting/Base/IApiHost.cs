﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core.Model;

namespace XML.Remoting;

/// <summary>Api主机</summary>
public interface IApiHost
{
    /// <summary>编码器</summary>
    IEncoder Encoder { get; set; }

    /// <summary>获取消息编码器。重载以指定不同的封包协议</summary>
    /// <returns></returns>
    IHandler GetMessageCodec();

    /// <summary>日志</summary>
    ILog Log { get; set; }

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    void WriteLog(String format, params Object[] args);
}