using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>实体处理模块</summary>
public interface IEntityModule
{
    /// <summary>为指定实体类初始化模块，返回是否支持</summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    Boolean Init(Type entityType);

    /// <summary>创建实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="forEdit"></param>
    void Create(IEntity entity, Boolean forEdit);

    /// <summary>验证实体对象</summary>
    /// <param name="entity"></param>
    /// <param name="isNew"></param>
    /// <returns></returns>
    Boolean Valid(IEntity entity, Boolean isNew);

    /// <summary>删除实体对象</summary>
    /// <param name="entity"></param>
    Boolean Delete(IEntity entity);
}