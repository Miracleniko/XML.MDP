using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace XML.Core.Reflection;

/// <summary>反射接口</summary>
/// <remarks>该接口仅用于扩展，不建议外部使用</remarks>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IReflect
{
    #region 反射获取
    /// <summary>根据名称获取类型</summary>
    /// <param name="typeName">类型名</param>
    /// <param name="isLoadAssembly">是否从未加载程序集中获取类型。使用仅反射的方法检查目标类型，如果存在，则进行常规加载</param>
    /// <returns></returns>
    Type GetType(String typeName, Boolean isLoadAssembly);

    /// <summary>获取方法</summary>
    /// <remarks>用于具有多个签名的同名方法的场合，不确定是否存在性能问题，不建议普通场合使用</remarks>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="paramTypes">参数类型数组</param>
    /// <returns></returns>
    MethodInfo GetMethod(Type type, String name, params Type[] paramTypes);

    /// <summary>获取指定名称的方法集合，支持指定参数个数来匹配过滤</summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="paramCount">参数个数，-1表示不过滤参数个数</param>
    /// <returns></returns>
    MethodInfo[] GetMethods(Type type, String name, Int32 paramCount = -1);

    /// <summary>获取属性</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    PropertyInfo GetProperty(Type type, String name, Boolean ignoreCase);

    /// <summary>获取字段</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    FieldInfo GetField(Type type, String name, Boolean ignoreCase);

    /// <summary>获取成员</summary>
    /// <param name="type">类型</param>
    /// <param name="name">名称</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <returns></returns>
    MemberInfo GetMember(Type type, String name, Boolean ignoreCase);

    /// <summary>获取字段</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    IList<FieldInfo> GetFields(Type type, Boolean baseFirst = true);

    /// <summary>获取属性</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    IList<PropertyInfo> GetProperties(Type type, Boolean baseFirst = true);
    #endregion

    #region 反射调用
    /// <summary>反射创建指定类型的实例</summary>
    /// <param name="type">类型</param>
    /// <param name="parameters">参数数组</param>
    /// <returns></returns>
    Object CreateInstance(Type type, params Object[] parameters);

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数</param>
    /// <returns></returns>
    Object Invoke(Object target, MethodBase method, params Object[] parameters);

    /// <summary>反射调用指定对象的方法</summary>
    /// <param name="target">要调用其方法的对象，如果要调用静态方法，则target是类型</param>
    /// <param name="method">方法</param>
    /// <param name="parameters">方法参数字典</param>
    /// <returns></returns>
    Object InvokeWithParams(Object target, MethodBase method, IDictionary parameters);

    /// <summary>获取目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <returns></returns>
    Object GetValue(Object target, PropertyInfo property);

    /// <summary>获取目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <returns></returns>
    Object GetValue(Object target, FieldInfo field);

    /// <summary>设置目标对象的属性值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="property">属性</param>
    /// <param name="value">数值</param>
    void SetValue(Object target, PropertyInfo property, Object value);

    /// <summary>设置目标对象的字段值</summary>
    /// <param name="target">目标对象</param>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    void SetValue(Object target, FieldInfo field, Object value);

    /// <summary>从源对象拷贝数据到目标对象</summary>
    /// <param name="target">目标对象</param>
    /// <param name="src">源对象</param>
    /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
    /// <param name="excludes">要忽略的成员</param>
    void Copy(Object target, Object src, Boolean deep = false, params String[] excludes);

    /// <summary>从源字典拷贝数据到目标对象</summary>
    /// <param name="target">目标对象</param>
    /// <param name="dic">源字典</param>
    /// <param name="deep">递归深度拷贝，直接拷贝成员值而不是引用</param>
    void Copy(Object target, IDictionary<String, Object> dic, Boolean deep = false);
    #endregion

    #region 类型辅助
    /// <summary>获取一个类型的元素类型</summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    Type GetElementType(Type type);

    /// <summary>类型转换</summary>
    /// <param name="value">数值</param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    Object ChangeType(Object value, Type conversionType);

    /// <summary>获取类型的友好名称</summary>
    /// <param name="type">指定类型</param>
    /// <param name="isfull">是否全名，包含命名空间</param>
    /// <returns></returns>
    String GetName(Type type, Boolean isfull);
    #endregion

    #region 插件
    /// <summary>是否能够转为指定基类</summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    Boolean As(Type type, Type baseType);

    /// <summary>在指定程序集中查找指定基类或接口的所有子类实现</summary>
    /// <param name="asm">指定程序集</param>
    /// <param name="baseType">基类或接口，为空时返回所有类型</param>
    /// <returns></returns>
    IEnumerable<Type> GetSubclasses(Assembly asm, Type baseType);

    /// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
    /// <param name="baseType">基类或接口</param>
    /// <returns></returns>
    IEnumerable<Type> GetAllSubclasses(Type baseType);

    ///// <summary>在所有程序集中查找指定基类或接口的子类实现</summary>
    ///// <param name="baseType">基类或接口</param>
    ///// <param name="isLoadAssembly">是否加载为加载程序集</param>
    ///// <returns></returns>
    //IEnumerable<Type> GetAllSubclasses(Type baseType, Boolean isLoadAssembly);
    #endregion
}