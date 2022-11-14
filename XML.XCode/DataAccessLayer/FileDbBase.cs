using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>文件型数据库</summary>
abstract class FileDbBase : DbBase
{
    #region 属性
    protected override void OnSetConnectionString(ConnectionStringBuilder builder)
    {
        base.OnSetConnectionString(builder);

        //if (!builder.TryGetValue(_.DataSource, out file)) return;
        // 允许空，当作内存数据库处理
        //builder.TryGetValue(_.DataSource, out var file);
        var file = builder["Data Source"];
        file = OnResolveFile(file);
        builder["Data Source"] = file;
        DatabaseName = file;
    }

    protected virtual String OnResolveFile(String file) => ResolveFile(file);
    #endregion
}