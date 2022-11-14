using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core.Data;
using XML.Core.Log;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>SQLite数据库</summary>
internal class SQLiteSession : FileDbSession
{
    #region 构造函数
    public SQLiteSession(IDatabase db) : base(db) { }
    #endregion

    #region 方法
    protected override void CreateDatabase()
    {
        // 内存数据库不需要创建
        if ((Database as SQLite).IsMemoryDatabase) return;

        base.CreateDatabase();

        // 打开自动清理数据库模式，此条命令必须放在创建表之前使用
        // 当从SQLite中删除数据时，数据文件大小不会减小，当重新插入数据时，
        // 将使用那块“空白”空间，打开自动清理后，删除数据后，会自动清理“空白”空间
        if ((Database as SQLite).AutoVacuum) Execute("PRAGMA auto_vacuum = 1");
    }
    #endregion

    #region 基本方法 查询/执行
    //protected override DbTable OnFill(DbDataReader dr)
    //{
    //    var dt = new DbTable();
    //    dt.ReadHeader(dr);

    //    var count = dr.FieldCount;
    //    var md = Database.CreateMetaData() as DbMetaData;

    //    // 字段
    //    var ts = new Type[count];
    //    var tns = new String[count];
    //    for (var i = 0; i < count; i++)
    //    {
    //        tns[i] = dr.GetDataTypeName(i);
    //        ts[i] = md.GetDataType(tns[i]);
    //    }
    //    dt.Types = ts;

    //    dt.ReadData(dr);

    //    return dt;
    //}

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public override Int64 InsertAndGetIdentity(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql += ";Select last_insert_rowid() newid";
        return base.InsertAndGetIdentity(sql, type, ps);
    }

    public override Task<Int64> InsertAndGetIdentityAsync(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql += ";Select last_insert_rowid() newid";
        return base.InsertAndGetIdentityAsync(sql, type, ps);
    }
    #endregion

    #region 高级
    /// <summary>清空数据表，标识归零</summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public override Int32 Truncate(String tableName)
    {
        // 先删除数据再收缩
        var sql = $"Delete From {Database.FormatName(tableName)}";
        var rs = Execute(sql);

        // 该数据库没有任何表用到自增时，序列表不存在
        try
        {
            Execute($"Update sqlite_sequence Set seq=0 where name='{tableName}'");
        }
        catch (Exception ex) { XTrace.WriteException(ex); }

        try
        {
            //rs += Execute("PRAGMA auto_vacuum = 1");
            rs += Execute("VACUUM");
        }
        catch (Exception ex) { XTrace.WriteException(ex); }

        return rs;
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
        if (updateColumns != null || addColumns != null)
        {
            sb.Append(" On Conflict");

            // 先找唯一索引，再用主键
            //var table = columns.FirstOrDefault()?.Table;
            var di = table.Indexes?.FirstOrDefault(e => e.Unique);
            if (di != null && di.Columns != null && di.Columns.Length > 0)
            {
                var dcs = table.GetColumns(di.Columns);
                sb.AppendFormat("({0})", dcs.Join(",", e => db.FormatName(e)));
            }
            else
            {
                var pks = table.PrimaryKeys;
                if (pks != null && pks.Length > 0)
                    sb.AppendFormat("({0})", pks.Join(",", e => db.FormatName(e)));
            }

            sb.Append(" Do Update Set ");
            if (updateColumns != null)
            {
                foreach (var dc in columns)
                {
                    if (dc.Identity || dc.PrimaryKey) continue;

                    if (updateColumns.Contains(dc.Name) && (addColumns == null || !addColumns.Contains(dc.Name)))
                        sb.AppendFormat("{0}=excluded.{0},", db.FormatName(dc));
                }
                sb.Length--;
            }
            if (addColumns != null)
            {
                sb.Append(',');
                foreach (var dc in columns)
                {
                    if (dc.Identity || dc.PrimaryKey) continue;

                    if (addColumns.Contains(dc.Name))
                        sb.AppendFormat("{0}={0}+excluded.{0},", db.FormatName(dc));
                }
                sb.Length--;
            }
        }
        return sb.Put(true);
    }

    public override Int32 Insert(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Into", table, columns, null, null, list);
        return Execute(sql);
    }

    public override Int32 InsertIgnore(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Or Ignore Into", table, columns, null, null, list);
        return Execute(sql);
    }

    public override Int32 Replace(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Or Replace Into", table, columns, null, null, list);
        return Execute(sql);
    }

    public override Int32 Upsert(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var sql = GetBatchSql("Insert Into", table, columns, updateColumns, addColumns, list);
        return Execute(sql);
    }
    #endregion
}