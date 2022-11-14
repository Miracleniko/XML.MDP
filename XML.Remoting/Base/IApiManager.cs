﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.Remoting;

/// <summary>接口管理器</summary>
public interface IApiManager
{
    /// <summary>可提供服务的方法</summary>
    IDictionary<String, ApiAction> Services { get; }

    /// <summary>注册服务提供类。该类的所有公开方法将直接暴露</summary>
    /// <typeparam name="TService"></typeparam>
    void Register<TService>();

    /// <summary>注册服务</summary>
    /// <param name="controller">控制器对象</param>
    /// <param name="method">动作名称。为空时遍历控制器所有公有成员方法</param>
    void Register(Object controller, String method);

    /// <summary>查找服务</summary>
    /// <param name="action"></param>
    /// <returns></returns>
    ApiAction Find(String action);
}