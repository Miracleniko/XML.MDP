
using System.Reflection;
using System.Text;
using XML.Core.Data;
using XML.Core.Log;

namespace XML.Core.Serialization;

/// <summary>序列化接口</summary>
public abstract class FormatterBase //: IFormatterX
{
    #region 属性
    /// <summary>数据流。默认实例化一个内存数据流</summary>
    public virtual Stream Stream { get; set; } = new MemoryStream();

    /// <summary>主对象</summary>
    public Stack<Object> Hosts { get; private set; } = new Stack<Object>();

    /// <summary>成员</summary>
    public MemberInfo Member { get; set; }

    /// <summary>字符串编码，默认utf-8</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>序列化属性而不是字段。默认true</summary>
    public Boolean UseProperty { get; set; } = true;

    /// <summary>用户对象。存放序列化过程中使用的用户自定义对象</summary>
    public Object UserState { get; set; }
    #endregion

    #region 方法
    /// <summary>获取流里面的数据</summary>
    /// <returns></returns>
    public Byte[] GetBytes()
    {
        var ms = Stream;
        var pos = ms.Position;
        var start = 0;
        if (pos == 0 || pos == start) return Array.Empty<Byte>();

        if (ms is MemoryStream ms2 && pos == ms.Length && start == 0)
            return ms2.ToArray();

        ms.Position = start;

        var buf = new Byte[pos - start];
        ms.Read(buf, 0, buf.Length);
        return buf;
    }

    /// <summary>获取流里面的数据包</summary>
    /// <returns></returns>
    public Packet GetPacket()
    {
        Stream.Position = 0;
        return new(Stream);
    }
    #endregion

    #region 跟踪日志
    /// <summary>日志提供者</summary>
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>输出日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public virtual void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}