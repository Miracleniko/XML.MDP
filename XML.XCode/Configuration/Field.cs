using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XML.XCode.Configuration;

/// <summary>继承FieldItem，仅仅为了重载==和!=运算符</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class Field : FieldItem
{
    #region 构造
    /// <summary>构造函数</summary>
    /// <param name="table"></param>
    /// <param name="property">属性</param>
    public Field(TableItem table, PropertyInfo property) : base(table, property) { }

    internal Field(TableItem table, String name, Type type, String description, Int32 length)
    {
        Table = table;

        Name = name;
        ColumnName = name;
        Type = type;
        Description = description;
        Length = length;

        IsNullable = true;
    }
    #endregion

    /// <summary>等于</summary>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static Expression operator ==(Field field, Object value) => field.Equal(value);

    /// <summary>不等于</summary>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public static Expression operator !=(Field field, Object value) => field.NotEqual(value);

    /// <summary>重写一下</summary>
    /// <returns></returns>
    public override Int32 GetHashCode() => base.GetHashCode();

    /// <summary>重写一下</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override Boolean Equals(Object obj) => base.Equals(obj);

    #region 类型转换
    /// <summary>类型转换</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static implicit operator String(Field obj) => !Equals(obj, null) && !obj.Equals(null) ? obj.ColumnName : null;
    #endregion
}