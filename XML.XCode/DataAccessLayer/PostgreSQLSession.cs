using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core.Data;

namespace XML.XCode.DataAccessLayer;

/// <summary>PostgreSQL数据库</summary>
internal class PostgreSQLSession : RemoteDbSession
{
    #region 构造函数
    public PostgreSQLSession(IDatabase db) : base(db) { }
    #endregion

    #region 基本方法 查询/执行
    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public override Int64 InsertAndGetIdentity(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql += " RETURNING id";
        return base.InsertAndGetIdentity(sql, type, ps);
    }

    public override Task<Int64> InsertAndGetIdentityAsync(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql += " RETURNING id";
        return base.InsertAndGetIdentityAsync(sql, type, ps);
    }
    #endregion

    #region 批量操作
    /*
    insert into stat (siteid,statdate,`count`,cost,createtime,updatetime) values 
    (1,'2018-08-11 09:34:00',1,123,now(),now()),
    (2,'2018-08-11 09:34:00',1,456,now(),now()),
    (3,'2018-08-11 09:34:00',1,789,now(),now()),
    (2,'2018-08-11 09:34:00',1,456,now(),now())
    on duplicate key update 
    `count`=`count`+values(`count`),cost=cost+values(cost),
    updatetime=values(updatetime);
     */

    private String GetBatchSql(String action, IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var sb = Pool.StringBuilder.Get();
        var db = Database as DbBase;

        // 字段列表
        if (columns == null) columns = table.Columns.ToArray();
        BuildInsert(sb, db, action, table, columns);

        // 值列表
        sb.Append(" Values");
        BuildBatchValues(sb, db, action, table, columns, list);

        // 重复键执行update
        BuildDuplicateKey(sb, db, columns, updateColumns, addColumns);

        return sb.Put(true);
    }

    public override Int32 Insert(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Into", table, columns, null, null, list);
        return Execute(sql);
    }

    public override Int32 Upsert(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Into", table, columns, updateColumns, addColumns, list);
        return Execute(sql);
    }
    #endregion
}