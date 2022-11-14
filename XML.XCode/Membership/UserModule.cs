using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using XML.Core;

namespace XML.XCode.Membership;

/// <summary>用户模型</summary>
public class UserModule : EntityModule
{
    #region 静态引用
    /// <summary>字段名</summary>
    public class __
    {
        /// <summary>创建人</summary>
        public static String CreateUserID = nameof(CreateUserID);

        /// <summary>创建人</summary>
        public static String CreateUser = nameof(CreateUser);

        /// <summary>更新人</summary>
        public static String UpdateUserID = nameof(UpdateUserID);

        /// <summary>更新人</summary>
        public static String UpdateUser = nameof(UpdateUser);
    }
    #endregion

    #region 提供者
    /// <summary>当前用户提供者</summary>
    public IManageProvider Provider { get; set; }
    #endregion

    #region 构造函数
    /// <summary>实例化</summary>
    public UserModule() : this(null) { }

    /// <summary>实例化</summary>
    /// <param name="provider"></param>
    public UserModule(IManageProvider provider) => Provider = provider;
    #endregion

    /// <summary>初始化。检查是否匹配</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    protected override Boolean OnInit(Type entityType)
    {
        var fs = GetFields(entityType);
        foreach (var fi in fs)
        {
            if (fi.Type == typeof(Int32) || fi.Type == typeof(Int64))
            {
                if (fi.Name.EqualIgnoreCase(__.CreateUserID, __.UpdateUserID)) return true;
            }
            else if (fi.Type == typeof(String))
            {
                if (fi.Name.EqualIgnoreCase(__.CreateUser, __.UpdateUser)) return true;
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

        // 当前登录用户
        var prv = Provider ?? ManageProvider.Provider;
        //var user = prv?.Current ?? HttpContext.Current?.User?.Identity as IManageUser;
        var user = prv?.Current;
        if (user != null)
        {
            if (isNew)
            {
                SetItem(fs, entity, __.CreateUserID, user.ID);
                SetItem(fs, entity, __.CreateUser, user + "");
                SetItem(fs, entity, __.UpdateUserID, user.ID);
                SetItem(fs, entity, __.UpdateUser, user + "");
            }
            else
            {
                SetNoDirtyItem(fs, entity, __.UpdateUserID, user.ID);
                SetNoDirtyItem(fs, entity, __.UpdateUser, user + "");
            }
        }
        else
        {
            // 在没有当前登录用户的场合，把更新者清零
            SetNoDirtyItem(fs, entity, __.UpdateUserID, 0);
            SetNoDirtyItem(fs, entity, __.UpdateUser, "");
        }

        return true;
    }
}