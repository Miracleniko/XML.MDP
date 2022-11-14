using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XML.Core;
using XML.XCode.Configuration;

namespace XML.XCode.Membership;

/// <summary>IP地址模型</summary>
public class IPModule : EntityModule
{
    #region 静态引用
    /// <summary>字段名</summary>
    public class __
    {
        /// <summary>创建人</summary>
        public static String CreateIP = nameof(CreateIP);

        /// <summary>更新人</summary>
        public static String UpdateIP = nameof(UpdateIP);
    }
    #endregion

    /// <summary>初始化。检查是否匹配</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    protected override Boolean OnInit(Type entityType)
    {
        var fs = GetFields(entityType);
        foreach (var fi in fs)
        {
            if (fi.Type == typeof(String))
            {
                if (fi.Name.EqualIgnoreCase(__.CreateIP, __.UpdateIP)) return true;
            }
        }

        var fs2 = GetIPFieldNames(entityType);
        return fs2 != null && fs2.Length > 0;
    }

    /// <summary>验证数据，自动加上创建和更新的信息</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    protected override Boolean OnValid(IEntity entity, Boolean isNew)
    {
        if (!isNew && !entity.HasDirty) return true;

        var ip = ManageProvider.UserHost;
        if (!ip.IsNullOrEmpty())
        {
            // 如果不是IPv6，去掉后面端口
            if (ip.Contains("://")) ip = ip.Substring("://", null);
            //if (ip.Contains(":") && !ip.Contains("::")) ip = ip.Substring(null, ":");

            var fs = GetFields(entity.GetType());

            if (isNew)
            {
                //SetItem(fs, entity, __.CreateIP, ip);

                var fs2 = GetIPFieldNames(entity.GetType());
                if (fs2 != null)
                {
                    foreach (var item in fs2)
                    {
                        SetItem(fs2, entity, item, ip);
                    }
                }

                //SetItem(fs, entity, __.UpdateIP, ip);
            }
            else
            {
                // 不管新建还是更新，都改变更新
                SetNoDirtyItem(fs, entity, __.UpdateIP, ip);
            }
        }

        return true;
    }

    private static readonly ConcurrentDictionary<Type, FieldItem[]> _ipFieldNames = new();
    /// <summary>获取实体类的字段名。带缓存</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    protected static FieldItem[] GetIPFieldNames(Type entityType)
    {
        return _ipFieldNames.GetOrAdd(entityType, t => GetFields(t)?.Where(e => e.Name.EqualIgnoreCase("CreateIP", "UpdateIP")).ToArray());
    }
}