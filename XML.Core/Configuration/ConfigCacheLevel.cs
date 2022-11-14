﻿namespace XML.Core.Configuration;

/// <summary>配置数据缓存等级</summary>
public enum ConfigCacheLevel
{
    /// <summary>不缓存</summary>
    NoCache,

    /// <summary>Json格式缓存</summary>
    Json,

    /// <summary>加密缓存</summary>
    Encrypted,
}