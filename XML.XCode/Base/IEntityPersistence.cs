using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>实体持久化接口。可通过实现该接口来自定义实体类持久化行为。</summary>
public interface IEntityPersistence
{
    #region 属性
    /// <summary>实体工厂</summary>
    IEntityFactory Factory { get; }
    #endregion

    #region 添删改方法
    /// <summary>插入</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Int32 Insert(IEntitySession session, IEntity entity);

    /// <summary>更新</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Int32 Update(IEntitySession session, IEntity entity);

    /// <summary>删除</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Int32 Delete(IEntitySession session, IEntity entity);

    /// <summary>插入</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Task<Int32> InsertAsync(IEntitySession session, IEntity entity);

    /// <summary>更新</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Task<Int32> UpdateAsync(IEntitySession session, IEntity entity);

    /// <summary>删除</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体</param>
    /// <returns></returns>
    Task<Int32> DeleteAsync(IEntitySession session, IEntity entity);

    /// <summary>把一个实体对象持久化到数据库</summary>
    /// <param name="session">实体会话</param>
    /// <param name="names">更新属性列表</param>
    /// <param name="values">更新值列表</param>
    /// <returns>返回受影响的行数</returns>
    Int32 Insert(IEntitySession session, String[] names, Object[] values);

    /// <summary>更新一批实体数据</summary>
    /// <param name="session">实体会话</param>
    /// <param name="setClause">要更新的项和数据</param>
    /// <param name="whereClause">指定要更新的实体</param>
    /// <returns></returns>
    Int32 Update(IEntitySession session, String setClause, String whereClause);

    /// <summary>更新一批实体数据</summary>
    /// <param name="session">实体会话</param>
    /// <param name="setNames">更新属性列表</param>
    /// <param name="setValues">更新值列表</param>
    /// <param name="whereNames">条件属性列表</param>
    /// <param name="whereValues">条件值列表</param>
    /// <returns>返回受影响的行数</returns>
    Int32 Update(IEntitySession session, String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues);

    /// <summary>从数据库中删除指定条件的实体对象。</summary>
    /// <param name="session">实体会话</param>
    /// <param name="whereClause">限制条件</param>
    /// <returns></returns>
    Int32 Delete(IEntitySession session, String whereClause);

    /// <summary>从数据库中删除指定属性列表和值列表所限定的实体对象。</summary>
    /// <param name="session">实体会话</param>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns></returns>
    Int32 Delete(IEntitySession session, String[] names, Object[] values);
    #endregion

    #region 获取语句
    /// <summary>获取主键条件</summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    WhereExpression GetPrimaryCondition(IEntity entity);

    /// <summary>把SQL模版格式化为SQL语句</summary>
    /// <param name="session">实体会话</param>
    /// <param name="entity">实体对象</param>
    /// <param name="methodType"></param>
    /// <returns>SQL字符串</returns>
    String GetSql(IEntitySession session, IEntity entity, DataObjectMethodType methodType);
    #endregion

    #region 参数化
    /// <summary>插入语句</summary>
    /// <param name="session">实体会话</param>
    /// <returns></returns>
    String InsertSQL(IEntitySession session);
    #endregion
}
