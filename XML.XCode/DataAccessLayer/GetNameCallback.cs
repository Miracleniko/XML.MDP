using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.DataAccessLayer;

/// <summary>根据实体类获取表名或主键名的委托</summary>
/// <param name="entityType">实体类</param>
/// <returns></returns>
public delegate String GetNameCallback(Type entityType);