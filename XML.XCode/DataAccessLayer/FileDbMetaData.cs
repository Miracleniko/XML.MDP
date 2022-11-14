using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>文件型数据库元数据</summary>
abstract class FileDbMetaData : DbMetaData
{
    #region 属性
    /// <summary>文件</summary>
    public String FileName => (Database as FileDbBase).DatabaseName;
    #endregion

    #region 数据定义
    /// <summary>设置数据定义模式</summary>
    /// <param name="schema"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public override Object SetSchema(DDLSchema schema, Object[] values)
    {
        //Object obj = null;
        switch (schema)
        {
            case DDLSchema.CreateDatabase:
                CreateDatabase();
                return null;
            case DDLSchema.DropDatabase:
                DropDatabase();
                return null;
            case DDLSchema.DatabaseExist:
                return File.Exists(FileName);
            default:
                break;
        }
        return base.SetSchema(schema, values);
    }

    /// <summary>创建数据库</summary>
    protected virtual void CreateDatabase()
    {
        if (String.IsNullOrEmpty(FileName)) return;

        // 提前创建目录
        var dir = Path.GetDirectoryName(FileName);
        if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        if (!File.Exists(FileName))
        {
            DAL.WriteLog("创建数据库：{0}", FileName);

            File.Create(FileName).Dispose();
        }
    }

    protected virtual void DropDatabase()
    {
        //首先关闭数据库
        if (Database is DbBase db)
            db.ReleaseSession();
        else
            Database.CreateSession().Dispose();

        //OleDbConnection.ReleaseObjectPool();
        GC.Collect();

        if (File.Exists(FileName)) File.Delete(FileName);
    }
    #endregion
}