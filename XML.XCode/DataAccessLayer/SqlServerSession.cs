using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XML.Core;
using XML.Core.Collections;
using XML.Core.Data;
using XML.Core.Reflection;

namespace XML.XCode.DataAccessLayer;

/// <summary>SqlServer数据库</summary>
internal class SqlServerSession : RemoteDbSession
{
    #region 构造函数
    public SqlServerSession(IDatabase db) : base(db) { }
    #endregion

    #region 查询
    protected override DbTable OnFill(DbDataReader dr)
    {
        var dt = new DbTable();
        dt.ReadHeader(dr);
        dt.ReadData(dr, GetFields(dt, dr));

        return dt;
    }

    private Int32[] GetFields(DbTable dt, DbDataReader dr)
    {
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
            return fs.ToArray();
        }

        return null;
    }

    /// <summary>快速查询单表记录数，稍有偏差</summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public override Int64 QueryCountFast(String tableName)
    {
        tableName = tableName.Trim().Trim('[', ']').Trim();

        var sql = $"select rows from sysindexes where id = object_id('{tableName}') and indid in (0,1)";
        return ExecuteScalar<Int64>(sql);
    }

    /// <summary>执行插入语句并返回新增行的自动编号</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="type">命令类型，默认SQL文本</param>
    /// <param name="ps">命令参数</param>
    /// <returns>新增行的自动编号</returns>
    public override Int64 InsertAndGetIdentity(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql = "SET NOCOUNT ON;" + sql + ";Select SCOPE_IDENTITY()";
        return base.InsertAndGetIdentity(sql, type, ps);
    }

    public override Task<Int64> InsertAndGetIdentityAsync(String sql, CommandType type = CommandType.Text, params IDataParameter[] ps)
    {
        sql = "SET NOCOUNT ON;" + sql + ";Select SCOPE_IDENTITY()";
        return base.InsertAndGetIdentityAsync(sql, type, ps);
    }
    #endregion

    #region 批量操作
    public override Int32 Insert(IDataTable table, IDataColumn[] columns, IEnumerable<IExtend> list)
    {
        var ps = new HashSet<String>();
        var sql = GetInsertSql(table, columns, ps);
        var dpsList = GetParametersList(columns, ps, list);

        return BatchExecute(sql, dpsList);
    }

    private String GetInsertSql(IDataTable table, IDataColumn[] columns, ICollection<String> ps)
    {
        var sb = Pool.StringBuilder.Get();
        var db = Database as DbBase;

        // 字段列表
        sb.AppendFormat("Insert Into {0}(", db.FormatName(table));
        foreach (var dc in columns)
        {
            if (dc.Identity) continue;

            sb.Append(db.FormatName(dc));
            sb.Append(',');
        }
        sb.Length--;
        sb.Append(')');

        // 值列表
        sb.Append(" Values(");
        foreach (var dc in columns)
        {
            if (dc.Identity) continue;

            sb.Append(db.FormatParameterName(dc.Name));
            sb.Append(',');

            if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
        }
        sb.Length--;
        sb.Append(')');

        return sb.Put(true);
    }

    public override Int32 Upsert(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, IEnumerable<IExtend> list)
    {
        var ps = new HashSet<String>();
        var insert = GetInsertSql(table, columns, ps);
        var update = GetUpdateSql(table, columns, updateColumns, addColumns, ps);

        // 先更新，根据更新结果影响的条目数判断是否需要插入
        var sb = Pool.StringBuilder.Get();
        sb.Append(update);
        sb.AppendLine(";");
        sb.AppendLine("IF(@@ROWCOUNT = 0)");
        sb.AppendLine("BEGIN");
        sb.Append(insert);
        sb.AppendLine(";");
        sb.AppendLine("END;");
        var sql = sb.Put(true);

        var dpsList = GetParametersList(columns, ps, list, true);
        return BatchExecute(sql, dpsList);
    }

    private String GetUpdateSql(IDataTable table, IDataColumn[] columns, ICollection<String> updateColumns, ICollection<String> addColumns, ICollection<String> ps)
    {
        var sb = Pool.StringBuilder.Get();
        var db = Database as DbBase;

        // 字段列表
        sb.AppendFormat("Update {0} Set ", db.FormatName(table));
        foreach (var dc in columns)
        {
            if (dc.Identity || dc.PrimaryKey) continue;

            // 修复当columns看存在updateColumns不存在列时构造出来的Sql语句会出现连续逗号的问题
            if (updateColumns != null && updateColumns.Contains(dc.Name) && (addColumns == null || !addColumns.Contains(dc.Name)))
            {
                sb.AppendFormat("{0}={1},", db.FormatName(dc), db.FormatParameterName(dc.Name));

                if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
            }
            else if (addColumns != null && addColumns.Contains(dc.Name))
            {
                sb.AppendFormat("{0}={0}+{1},", db.FormatName(dc), db.FormatParameterName(dc.Name));

                if (!ps.Contains(dc.Name)) ps.Add(dc.Name);
            }
            //sb.Append(",");
        }
        sb.Length--;
        //sb.Append(")");

        // 条件
        var pks = columns.Where(e => e.PrimaryKey).ToArray();
        if (pks == null || pks.Length == 0) throw new InvalidOperationException("未指定用于更新的主键");

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
    #endregion

    #region 修复实现SqlServer批量操作增添方法
    private Int32 BatchExecute(String sql, List<IDataParameter[]> psList)
    {
        return Process(conn =>
        {
            ////获取连接对象
            //var conn = Database.Pool.Get();

            // 准备
            var mBatcher = new SqlBatcher(Database.Factory);
            mBatcher.StartBatch(conn);

            // 创建并添加Command
            foreach (var dps in psList)
            {
                if (dps != null)
                {
                    var cmd = OnCreateCommand(sql, CommandType.Text, dps);
                    mBatcher.AddToBatch(cmd);
                }
            }

            // 执行批量操作
            try
            {
                BeginTrace();
                var ret = mBatcher.ExecuteBatch();
                mBatcher.EndBatch();
                return ret;
            }
            catch (DbException ex)
            {
                throw OnException(ex);
            }
            finally
            {
                //if (conn != null) Database.Pool.Put(conn);
                EndTrace(OnCreateCommand(sql, CommandType.Text));
            }
        });
    }

    private List<IDataParameter[]> GetParametersList(IDataColumn[] columns, ICollection<String> ps, IEnumerable<IExtend> list, Boolean isInsertOrUpdate = false)
    {
        var db = Database;
        var dpsList = new List<IDataParameter[]>();

        foreach (var entity in list)
        {
            var dps = new List<IDataParameter>();
            foreach (var dc in columns)
            {
                if (isInsertOrUpdate)
                {
                    if (dc.Identity || dc.PrimaryKey)
                    {
                        //更新时添加主键做为查询条件
                        dps.Add(db.CreateParameter(dc.Name, entity[dc.Name], dc));
                        continue;
                    }
                }
                else
                {
                    if (dc.Identity) continue;
                }
                if (!ps.Contains(dc.Name)) continue;

                // 用于参数化的字符串不能为null
                var val = entity[dc.Name];
                if (dc.DataType == typeof(String))
                    val += "";
                else if (dc.DataType == typeof(DateTime))
                {
                    var dt = val.ToDateTime();
                    if (dt.Year < 1970) val = new DateTime(1970, 1, 1);
                }

                // 逐列创建参数对象
                dps.Add(db.CreateParameter(dc.Name, val, dc));
            }

            dpsList.Add(dps.ToArray());
        }

        return dpsList;
    }

    /// <summary>
    /// 批量操作帮助类
    /// </summary>
    private class SqlBatcher
    {
        private DataAdapter mAdapter;
        private readonly DbProviderFactory _factory;

        /// <summary>获得批处理是否正在批处理状态。</summary>
        public Boolean IsStarted { get; private set; }

        static MethodInfo _init;
        static MethodInfo _add;
        static MethodInfo _exe;
        static MethodInfo _clear;
        Func<IDbCommand, Int32> _addToBatch;
        Func<Int32> _executeBatch;
        Action _clearBatch;

        public SqlBatcher(DbProviderFactory factory)
        {
            _factory = factory;

            if (_init == null)
            {
                using var adapter = factory.CreateDataAdapter();
                var type = adapter.GetType();

                _add = type.GetMethodEx("AddToBatch");
                _exe = type.GetMethodEx("ExecuteBatch");
                _clear = type.GetMethodEx("ClearBatch");
                _init = type.GetMethodEx("InitializeBatching");
            }
        }

        /// <summary>开始批处理</summary>
        /// <param name="connection">连接。</param>
        public void StartBatch(DbConnection connection)
        {
            if (IsStarted) return;

            var cmd = _factory.CreateCommand();
            cmd.Connection = connection;

            var adapter = _factory.CreateDataAdapter();
            adapter.InsertCommand = cmd;
            //adapter.Invoke("InitializeBatching");
            _init.As<Action>(adapter)();

            _addToBatch = _add.As<Func<IDbCommand, Int32>>(adapter);
            _executeBatch = _exe.As<Func<Int32>>(adapter);
            _clearBatch = _clear.As<Action>(adapter);

            mAdapter = adapter;

            IsStarted = true;
        }

        /// <summary>
        /// 添加批命令。
        /// </summary>
        /// <param name="command">命令</param>
        public void AddToBatch(IDbCommand command)
        {
            if (!IsStarted) throw new InvalidOperationException();

            //mAdapter.Invoke("AddToBatch", new Object[] { command });
            _addToBatch(command);
        }

        /// <summary>
        /// 执行批处理。
        /// </summary>
        /// <returns>影响的数据行数。</returns>
        public Int32 ExecuteBatch()
        {
            if (!IsStarted) throw new InvalidOperationException();

            //return (Int32)mAdapter.Invoke("ExecuteBatch");
            return _executeBatch();
        }

        /// <summary>
        /// 结束批处理。
        /// </summary>
        public void EndBatch()
        {
            if (IsStarted)
            {
                ClearBatch();
                mAdapter.Dispose();
                mAdapter = null;
                IsStarted = false;
            }
        }

        /// <summary>
        /// 清空保存的批命令。
        /// </summary>
        public void ClearBatch()
        {
            if (!IsStarted) throw new InvalidOperationException();

            //mAdapter.Invoke("ClearBatch");
            _clearBatch();
        }
    }
    #endregion
}