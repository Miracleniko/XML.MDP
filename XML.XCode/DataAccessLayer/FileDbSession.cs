using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>文件型数据库会话</summary>
abstract class FileDbSession : DbSession
{
    #region 属性
    /// <summary>文件</summary>
    public String FileName => (Database as FileDbBase)?.DatabaseName;
    #endregion

    #region 构造函数
    protected FileDbSession(IDatabase db) : base(db)
    {
        if (!String.IsNullOrEmpty(FileName))
        {
            if (!hasChecked.Contains(FileName))
            {
                hasChecked.Add(FileName);
                CreateDatabase();
            }
        }
    }
    #endregion

    #region 方法
    private static readonly List<String> hasChecked = new();

    ///// <summary>已重载。打开数据库连接前创建数据库</summary>
    //public override void Open()
    //{
    //    if (!String.IsNullOrEmpty(FileName))
    //    {
    //        if (!hasChecked.Contains(FileName))
    //        {
    //            hasChecked.Add(FileName);
    //            CreateDatabase();
    //        }
    //    }

    //    base.Open();
    //}

    protected virtual void CreateDatabase()
    {
        if (!File.Exists(FileName)) Database.CreateMetaData().SetSchema(DDLSchema.CreateDatabase, null);
    }
    #endregion

    #region 高级
    /// <summary>清空数据表，标识归零</summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public override Int32 Truncate(String tableName)
    {
        var sql = $"Delete From {Database.FormatName(tableName)}";
        return Execute(sql);
    }
    #endregion
}
