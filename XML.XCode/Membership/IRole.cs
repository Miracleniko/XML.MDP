using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Membership;

/// <summary>角色</summary>
public partial interface IRole
{
    #region 属性
    /// <summary>编号</summary>
    Int32 ID { get; set; }

    /// <summary>名称</summary>
    String Name { get; set; }

    /// <summary>启用</summary>
    Boolean Enable { get; set; }

    /// <summary>系统。用于业务系统开发使用，不受数据权限约束，禁止修改名称或删除</summary>
    Boolean IsSystem { get; set; }

    /// <summary>权限。对不同资源的权限，逗号分隔，每个资源的权限子项竖线分隔</summary>
    String Permission { get; set; }

    /// <summary>扩展1</summary>
    Int32 Ex1 { get; set; }

    /// <summary>扩展2</summary>
    Int32 Ex2 { get; set; }

    /// <summary>扩展3</summary>
    Double Ex3 { get; set; }

    /// <summary>扩展4</summary>
    String Ex4 { get; set; }

    /// <summary>扩展5</summary>
    String Ex5 { get; set; }

    /// <summary>扩展6</summary>
    String Ex6 { get; set; }

    /// <summary>创建者</summary>
    String CreateUser { get; set; }

    /// <summary>创建用户</summary>
    Int32 CreateUserID { get; set; }

    /// <summary>创建地址</summary>
    String CreateIP { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>更新者</summary>
    String UpdateUser { get; set; }

    /// <summary>更新用户</summary>
    Int32 UpdateUserID { get; set; }

    /// <summary>更新地址</summary>
    String UpdateIP { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }

    /// <summary>备注</summary>
    String Remark { get; set; }
    #endregion
}

/// <summary>角色</summary>
public partial interface IRole
{
    /// <summary>本角色权限集合</summary>
    IDictionary<Int32, PermissionFlags> Permissions { get; }

    /// <summary>是否拥有指定资源的指定权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    Boolean Has(Int32 resid, PermissionFlags flag = PermissionFlags.None);

    /// <summary>获取权限</summary>
    /// <param name="resid"></param>
    /// <returns></returns>
    PermissionFlags Get(Int32 resid);

    /// <summary>设置该角色拥有指定资源的指定权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    void Set(Int32 resid, PermissionFlags flag = PermissionFlags.Detail);

    /// <summary>重置该角色指定的权限</summary>
    /// <param name="resid"></param>
    /// <param name="flag"></param>
    void Reset(Int32 resid, PermissionFlags flag);

    /// <summary>当前角色拥有的资源</summary>
    Int32[] Resources { get; }

    /// <summary>根据编号查找角色</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IRole FindByID(Int32 id);

    /// <summary>根据名称查找角色，若不存在则创建</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IRole GetOrAdd(String name);

    /// <summary>保存</summary>
    /// <returns></returns>
    Int32 Save();
}