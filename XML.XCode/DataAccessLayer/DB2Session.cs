using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XML.Core.Collections;
using XML.Core.Data;
using XML.Core;
using XML.Core.Reflection;

namespace XML.XCode.DataAccessLayer;

/// <summary>DB2数据库</summary>
internal class DB2Session : RemoteDbSession
{
    #region 构造函数
    public DB2Session(IDatabase db) : base(db) { }
    #endregion

    #region 基本方法 查询/执行
    protected override DbTable OnFill(DbDataReader dr)
    {
        var dt = new DbTable();
        dt.ReadHeader(dr);

        Int32[] fields = null;

        // 干掉rowNumber
        var idx = Array.FindIndex(dt.Columns, c => c.EqualIgnoreCase("rowNumber"));
        if (idx >= 0)
        {
            var cs = dt.Columns.ToList();
            var ts = dt.Types.ToList();
            var fs = Enumerable.Range(0, cs.Count).ToList();

            cs.RemoveAt(idx);
            ts.RemoveAt(idx);
            fs.RemoveAt(idx);

            dt.Columns = cs.ToArray();
            dt.Types = ts.ToArray();
            fields = fs.ToArray();
        }

        dt.ReadData(dr, fields);

        return dt;
    }

    /// <summary>快速查询单表记录数，稍有偏差</summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public override Int64 QueryCountFast(String tableName)
    {
        if (String.IsNullOrEmpty(tableName)) return 0;

        var p = tableName.LastIndexOf(".");
        if (p >= 0 && p < tableName.Length - 1) tableName = tableName[(p + 1)..];
        tableName = tableName.ToUpper();

        var owner = (Database as DB2).Owner;
        if (owner.IsNullOrEmpty()) owner = (Database as DB2).User;
        //var owner = (Database as DB2).Owner.ToUpper();
        owner = owner.ToUpper();

        // 某些表没有聚集索引，导致查出来的函数为零
        var sql = $"select NUM_ROWS from all_tables where OWNER='{owner}' and TABLE_NAME='{tableName}'";
        return ExecuteScalar<Int64>(sql);
    }

    static readonly Regex reg_SEQ = new(@"\b(\w+)\.nextval\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public override Int64 InsertAndGetIdentity(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        BeginTransaction(IsolationLevel.Serializable);
        try
        {
            Int64 rs = Execute(sql, type, ps);
            if (rs > 0)
            {
                var m = reg_SEQ.Match(sql);
                if (m != null && m.Success && m.Groups != null && m.Groups.Count > 0)
                    rs = ExecuteScalar<Int64>($"Select {m.Groups[1].Value}.currval From dual");
            }
            Commit();
            return rs;
        }
        catch { Rollback(true); throw; }
    }

    public override Task<Int64> QueryCountFastAsync(String tableName)
    {
        if (String.IsNullOrEmpty(tableName)) return Task.FromResult(0L);

        var p = tableName.LastIndexOf(".");
        if (p >= 0 && p < tableName.Length - 1) tableName = tableName[(p + 1)..];
        tableName = tableName.ToUpper();

        var owner = (Database as DB2).Owner;
        if (owner.IsNullOrEmpty()) owner = (Database as DB2).User;
        //var owner = (Database as DB2).Owner.ToUpper();
        owner = owner.ToUpper();

        // 某些表没有聚集索引，导致查出来的函数为零
        var sql = $"select NUM_ROWS from all_tables where OWNER='{owner}' and TABLE_NAME='{tableName}'";
        return ExecuteScalarAsync<Int64>(sql);
    }

    public override async Task<Int64> InsertAndGetIdentityAsync(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        BeginTransaction(IsolationLevel.Serializable);
        try
        {
            Int64 rs = await ExecuteAsync(sql, type, ps);
            if (rs > 0)
            {
                var m = reg_SEQ.Match(sql);
                if (m != null && m.Success && m.Groups != null && m.Groups.Count > 0)
                    rs = await ExecuteScalarAsync<Int64>($"Select {m.Groups[1].Value}.currval From dual");
            }
            Commit();
            return rs;
        }
        catch { Rollback(true); throw; }
    }

    /// <summary>重载支持批量操作</summary>
    /// <param name="sql"></param>
    /// <param name="type"></param>
    /// <param name="ps"></param>
    /// <returns></returns>
    protected override DbCommand OnCreateCommand(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        var cmd = base.OnCreateCommand(sql, type, ps);
        if (cmd == null) return null;

        // 如果参数Value都是数组，那么就是批量操作
        if (ps != null && ps.Length > 0 && ps.All(p => p.Value is IList))
        {
            var arr = ps.First().Value as IList;
            cmd.SetValue("ArrayBindCount", arr.Count);
            cmd.SetValue("BindByName", true);

            // 超时时间放大10倍
            if (cmd.CommandTimeout > 0)
                cmd.CommandTimeout *= 10;
            else
                cmd.CommandTimeout = 120;
        }

        return cmd;
    }
    #endregion

    #region 批量操作
    public override Int32 Insert(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var ps = new HashSet<String>();
        var sql = GetInsertSql(table, columns, ps);
        var dps = GetParameters(columns, ps, list);
        return Execute(sql, CommandType.Text, dps);
    }

    private String GetInsertSql(IDataTable table, IDataColumn[] columns, ICollection<String> ps)
    {
        var sb = Pool.StringBuilder.Get();
        var db = Database as DbBase;

        // 字段列表
        sb.AppendFormat("Insert Into {0}(", db.FormatName(table));
        foreach (var dc in columns)
        {
            //if (dc.Identity) continue;

            sb.Append(db.FormatName(dc));
            sb.Append(',');
        }
        sb.Length--;
        sb.Append(')');

        // 值列表
        sb.Append(" Values(");
        foreach (var dc in columns)
        {
            //if (dc.Identity) continue;

            sb.Append(db.FormatParameterName(dc.Name));
            sb.Append(',');

            if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
        }
        sb.Length--;
        sb.Append(')');

        return sb.Put(true);
    }

    private IDataParameter[] GetParameters(IDataColumn[] columns, ICollection<String> ps, IEnumerable<IExtend> list)
    {
        var db = Database;
        var dps = new List<IDataParameter>();
        foreach (var dc in columns)
        {
            //if (dc.Identity) continue;
            if (!ps.Contains(dc.Name)) continue;

            //var vs = new List<Object>();
            var type = dc.DataType;
            if (!type.IsInt() && type.IsEnum) type = typeof(Int32);
            var arr = Array.CreateInstance(type, list.Count());
            var k = 0;
            foreach (var entity in list)
            {
                //vs.Add(entity[dc.Name]);
                arr.SetValue(entity[dc.Name], k++);
            }
            var dp = db.CreateParameter(dc.Name, arr, dc);
            dps.Add(dp);
        }
        return dps.ToArray();
    }

    public override Int32 Upsert(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var ps = new HashSet<String>();
        var insert = GetInsertSql(table, columns, ps);
        var update = GetUpdateSql(table, columns, updateColumns, addColumns, ps);

        var sb = Pool.StringBuilder.Get();
        sb.AppendLine("BEGIN");
        sb.AppendLine(insert + ";");
        sb.AppendLine("EXCEPTION");
        // 没有更新时，直接返回，可用于批量插入且其中部分有冲突需要忽略的场景
        if (!update.IsNullOrEmpty())
        {
            sb.AppendLine("WHEN DUP_VAL_ON_INDEX THEN");
            sb.AppendLine(update + ";");
        }
        else
        {
            //sb.AppendLine("WHEN OTHERS THEN");
            sb.AppendLine("WHEN DUP_VAL_ON_INDEX THEN");
            sb.AppendLine("RETURN;");
        }
        sb.AppendLine("END;");
        var sql = sb.Put(true);
        var dps = GetParameters(columns, ps, list);
        return Execute(sql, CommandType.Text, dps);
    }

    private String GetUpdateSql(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, ICollection<String> ps)
    {
        if ((updateColumns == null || updateColumns.Count == 0)
            && (addColumns == null || addColumns.Count == 0)) return null;

        var sb = Pool.StringBuilder.Get();
        var db = Database as DbBase;

        // 字段列表
        sb.AppendFormat("Update {0} Set ", db.FormatName(table));
        foreach (var dc in columns)
        {
            if (dc.Identity || dc.PrimaryKey) continue;

            if (addColumns != null && addColumns.Contains(dc.Name))
            {
                sb.AppendFormat("{0}={0}+{1},", db.FormatName(dc), db.FormatParameterName(dc.Name));

                if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
            }
            else if (updateColumns != null && updateColumns.Contains(dc.Name))
            {
                sb.AppendFormat("{0}={1},", db.FormatName(dc), db.FormatParameterName(dc.Name));

                if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
            }
        }
        sb.Length--;

        // 条件
        sb.Append(" Where ");
        foreach (var dc in columns)
        {
            if (!dc.PrimaryKey) continue;

            sb.AppendFormat("{0}={1}", db.FormatName(dc), db.FormatParameterName(dc.Name));
            sb.Append(" And ");
            if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
        }
        sb.Length -= " And ".Length;
        return sb.Put(true);
    }

    public override Int32 Update(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var ps = new HashSet<String>();
        var sql = GetUpdateSql(table, columns, updateColumns, addColumns, ps);
        var dps = GetParameters(columns, ps, list);

        return Execute(sql, CommandType.Text, dps);
    }
    #endregion
}