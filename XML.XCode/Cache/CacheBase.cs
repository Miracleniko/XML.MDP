using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core;
using XML.Core.Threading;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Cache;

/// <summary>缓存基类</summary>
public abstract class CacheBase : DisposeBase
{
    #region 设置
    /// <summary>是否调试缓存模块</summary>
    public static Boolean Debug { get; set; }

    /// <summary>显示统计信息的周期。默认60*60s，DAL.Debug=true时10*60s，Debug=true时60s</summary>
    public static Int32 Period { get; set; }

    /// <summary>日志前缀</summary>
    protected String LogPrefix { get; set; }
    #endregion

    static CacheBase()
    {
#if DEBUG
        Debug = true;
#endif
    }

    internal void WriteLog(String format, params Object[] args)
    {
        if (Debug) XTrace.WriteLine(LogPrefix + format, args);
    }

    /// <summary>检查并显示统计信息</summary>
    /// <param name="total"></param>
    /// <param name="show"></param>
    internal static void CheckShowStatics(ref Int32 total, Action show)
    {
        Interlocked.Increment(ref total);

        NextShow = true;

        // 加入列表
        if (total < 10)
        {
            var key = show?.Target?.GetType().FullName;
            if (key != null && !_dic.ContainsKey(key))
            {
                _dic.TryAdd(key, show);
            }
        }

        // 启动定时器
        if (_timer == null)
        {
            lock (typeof(CacheBase))
            {
                if (_timer == null)
                {
                    var ms = Period * 1000;
                    if (ms == 0)
                    {
                        ms = 60 * 60 * 1000;
                        if (DAL.Debug) ms = 10 * 60 * 1000;
                        if (Debug) ms = 1 * 60 * 1000;
                    }
                    _timer = new TimerEV(Check, null, 10000, ms);
                }
            }
        }
    }

    private static TimerEV _timer;
    private static readonly ConcurrentDictionary<String, Action> _dic = new();
    private static Boolean NextShow;

    private static void Check(Object state)
    {
        if (!NextShow) return;

        NextShow = false;

        foreach (var item in _dic)
        {
            item.Value();
        }
    }
}