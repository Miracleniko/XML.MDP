using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>远程数据库会话</summary>
abstract class RemoteDbSession : DbSession
{
    #region 属性
    /// <summary>系统数据库名</summary>
    public String SystemDatabaseName => (Database as RemoteDb)?.SystemDatabaseName;
    #endregion

    #region 构造函数
    public RemoteDbSession(IDatabase db) : base(db) { }
    #endregion

    #region 架构
    public override DataTable GetSchema(DbConnection conn, String collectionName, String[] restrictionValues)
    {
        try
        {
            return base.GetSchema(conn, collectionName, restrictionValues);
        }
        catch (Exception ex)
        {
            DAL.WriteLog("[{2}]GetSchema({0})异常重试！{1}", collectionName, ex.Message, Database.ConnName);

            // 如果没有数据库，登录会失败，需要切换到系统数据库再试试
            return ProcessWithSystem((s, c) => base.GetSchema(c, collectionName, restrictionValues)) as DataTable;
        }
    }
    #endregion

    #region 系统权限处理
    public Object ProcessWithSystem(Func<IDbSession, DbConnection, Object> callback)
    {
        var dbname = Database.DatabaseName;
        var sysdbname = SystemDatabaseName;

        // 如果指定了数据库名，并且不是master，则切换到master
        if (!dbname.IsNullOrEmpty() && !dbname.EqualIgnoreCase(sysdbname))
        {
            if (DAL.Debug) WriteLog("切换到系统库[{0}]", sysdbname);
            using var conn = Database.Factory.CreateConnection();
            try
            {
                //conn.ConnectionString = Database.ConnectionString;

                OpenDatabase(conn, Database.ConnectionString, sysdbname);

                return callback(this, conn);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                throw;
            }
            finally
            {
                if (DAL.Debug) WriteLog("退出系统库[{0}]，回到[{1}]", sysdbname, dbname);
            }
        }
        else
        {
            using var conn = Database.OpenConnection();
            return callback(this, conn);
        }
    }

    private static void OpenDatabase(IDbConnection conn, String connStr, String dbName)
    {
        // 如果没有打开，则改变链接字符串
        var builder = new ConnectionStringBuilder(connStr);
        var flag = false;
        if (builder["Database"] != null)
        {
            builder["Database"] = dbName;
            flag = true;
        }
        else if (builder["Initial Catalog"] != null)
        {
            builder["Initial Catalog"] = dbName;
            flag = true;
        }
        if (flag)
        {
            connStr = builder.ToString();
            //WriteLog("系统级：{0}", connStr);
        }

        conn.ConnectionString = connStr;
        conn.Open();
    }
    #endregion
}
