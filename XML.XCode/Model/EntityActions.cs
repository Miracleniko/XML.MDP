using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Model;

/// <summary>实体动作</summary>
public enum EntityActions
{
    /// <summary>保存</summary>
    Save = 0,

    /// <summary>插入</summary>
    Insert = 1,

    /// <summary>更新</summary>
    Update = 2,

    /// <summary>插入或更新</summary>
    Upsert = 3,

    /// <summary>删除</summary>
    Delete = 4,
}