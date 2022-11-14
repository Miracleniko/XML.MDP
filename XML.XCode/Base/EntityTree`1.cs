using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode;

/// <summary>主键为整型的实体树基类</summary>
/// <typeparam name="TEntity"></typeparam>
[Serializable]
public class EntityTree<TEntity> : EntityTree<Int32, TEntity> where TEntity : EntityTree<TEntity>, new()
{ }