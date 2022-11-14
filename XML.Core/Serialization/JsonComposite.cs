
using System.Reflection;
using System.Runtime.CompilerServices;
using XML.Core.Reflection;
using XML.Core.Serialization.Interface;

namespace XML.Core.Serialization;

/// <summary>复合对象处理器</summary>
public class JsonComposite : JsonHandlerBase
{
    /// <summary>要忽略的成员</summary>
    public ICollection<string> IgnoreMembers { get; set; }

    /// <summary>实例化</summary>
    public JsonComposite()
    {
        this.Priority = 100;
        this.IgnoreMembers = (ICollection<string>)new HashSet<string>();
    }

    /// <summary>获取对象的Json字符串表示形式。</summary>
    /// <param name="value"></param>
    /// <returns>返回null表示不支持</returns>
    public override string GetString(object value)
    {
        if (value == null)
            return string.Empty;
        Type type = value.GetType();
        if (type == typeof(Guid))
            return ((Guid)value).ToString();
        if (type == typeof(byte[]))
            return Convert.ToBase64String((byte[])value);
        if (type == typeof(char[]))
            return new string((char[])value);
        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.Empty:
            case TypeCode.DBNull:
                return string.Empty;
            case TypeCode.Boolean:
                return value?.ToString() ?? "";
            case TypeCode.Char:
            case TypeCode.SByte:
            case TypeCode.Byte:
                return value?.ToString() ?? "";
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return value?.ToString() ?? "";
            case TypeCode.Single:
            case TypeCode.Double:
                return value?.ToString() ?? "";
            case TypeCode.Decimal:
                return value?.ToString() ?? "";
            case TypeCode.DateTime:
                return value?.ToString() ?? "";
            case TypeCode.String:
                if (((string)value).IsNullOrEmpty())
                    return string.Empty;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
                interpolatedStringHandler.AppendLiteral("\"");
                interpolatedStringHandler.AppendFormatted<object>(value);
                interpolatedStringHandler.AppendLiteral("\"");
                return interpolatedStringHandler.ToStringAndClear();
            default:
                return (string)null;
        }
    }

    /// <summary>写入对象</summary>
    /// <param name="value">目标对象</param>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public override bool Write(object value, Type type)
    {
        if (value == null || Type.GetTypeCode(type) != TypeCode.Object)
            return false;
        List<MemberInfo> members = this.GetMembers(type);
        this.WriteLog("JsonWrite类{0} 共有成员{1}个", (object)type.Name, (object)members.Count);
        this.Host.Hosts.Push(value);
        AccessorContext context = new AccessorContext()
        {
            Host = (IFormatterX)this.Host,
            Type = type,
            Value = value,
            UserState = this.Host.UserState
        };
        foreach (MemberInfo member in members)
        {
            if (this.IgnoreMembers == null || !this.IgnoreMembers.Contains(member.Name))
            {
                Type memberType = JsonComposite.GetMemberType(member);
                context.Member = this.Host.Member = member;
                object obj = value.GetValue(member);
                this.WriteLog("    {0}.{1} {2}", (object)type.Name, (object)member.Name, obj);
                if ((!(value is IMemberAccessor memberAccessor) || !memberAccessor.Read((IFormatterX)this.Host, context)) && !this.Host.Write(obj, memberType))
                {
                    this.Host.Hosts.Pop();
                    return false;
                }
            }
        }
        this.Host.Hosts.Pop();
        return true;
    }

    /// <summary>尝试读取指定类型对象</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool TryRead(Type type, ref object value)
    {
        if (type == (Type)null)
        {
            if (value == null)
                return false;
            type = value.GetType();
        }
        if (Type.GetTypeCode(type) != TypeCode.Object || !type.As<object>())
            return false;
        List<MemberInfo> members = this.GetMembers(type);
        this.WriteLog("JsonRead类{0} 共有成员{1}个", (object)type.Name, (object)members.Count);
        if (value == null)
            value = type.CreateInstance();
        this.Host.Hosts.Push(value);
        AccessorContext context = new AccessorContext()
        {
            Host = (IFormatterX)this.Host,
            Type = type,
            Value = value,
            UserState = this.Host.UserState
        };
        for (int index = 0; index < members.Count; ++index)
        {
            MemberInfo member = members[index];
            if (this.IgnoreMembers == null || !this.IgnoreMembers.Contains(member.Name))
            {
                Type memberType = JsonComposite.GetMemberType(member);
                context.Member = this.Host.Member = member;
                this.WriteLog("    {0}.{1}", (object)member.DeclaringType.Name, (object)member.Name);
                if (!(value is IMemberAccessor memberAccessor) || !memberAccessor.Read((IFormatterX)this.Host, context))
                {
                    object obj = (object)null;
                    if (!this.Host.TryRead(memberType, ref obj))
                    {
                        this.Host.Hosts.Pop();
                        return false;
                    }
                    value.SetValue(member, obj);
                }
            }
        }
        this.Host.Hosts.Pop();
        return true;
    }

    /// <summary>获取成员</summary>
    /// <param name="type"></param>
    /// <param name="baseFirst"></param>
    /// <returns></returns>
    protected virtual List<MemberInfo> GetMembers(Type type, bool baseFirst = true) => this.Host.UseProperty ? type.GetProperties(baseFirst).Cast<MemberInfo>().ToList<MemberInfo>() : type.GetFields(baseFirst).Cast<MemberInfo>().ToList<MemberInfo>();

    private static Type GetMemberType(MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                return (member as FieldInfo).FieldType;
            case MemberTypes.Property:
                return (member as PropertyInfo).PropertyType;
            default:
                throw new NotSupportedException();
        }
    }
}