using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XML.Core;
using XML.Core.Threading;

namespace XML.XCode.Membership;

/// <summary>时间模型</summary>
public class TimeModule : EntityModule
{
    #region 静态引用
    /// <summary>字段名</summary>
    public class __
    {
        /// <summary>创建时间</summary>
        public static String CreateTime = nameof(CreateTime);

        /// <summary>更新时间</summary>
        public static String UpdateTime = nameof(UpdateTime);
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
            if (fi.Type == typeof(DateTime))
            {
                if (fi.Name.EqualIgnoreCase(__.CreateTime, __.UpdateTime)) return true;
            }
        }

        return false;
    }

    /// <summary>创建实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="forEdit"></param>
    protected override void OnCreate(IEntity entity, Boolean forEdit)
    {
        if (forEdit) OnValid(entity, true);
    }

    /// <summary>验证数据，自动加上创建和更新的信息</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    protected override Boolean OnValid(IEntity entity, Boolean isNew)
    {
        if (!isNew && !entity.HasDirty) return true;

        var fs = GetFields(entity.GetType());

        if (isNew)
        {
            SetItem(fs, entity, __.CreateTime, TimerEV.Now);
            SetItem(fs, entity, __.UpdateTime, TimerEV.Now);
        }
        else
        {
            // 不管新建还是更新，都改变更新时间
            SetNoDirtyItem(fs, entity, __.UpdateTime, TimerEV.Now);
        }

        return true;
    }
}