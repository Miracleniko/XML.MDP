using System.Collections;
using System.ComponentModel;

namespace XML.Core;

/// <summary>销毁助手。扩展方法专用</summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class DisposeHelper
{
    /// <summary>尝试销毁对象，如果有<see cref="IDisposable"/>则调用</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Object? TryDispose(this Object? obj)
    {
        if (obj == null) return obj;

        // 列表元素销毁
        if (obj is IEnumerable ems)
        {
            // 对于枚举成员，先考虑添加到列表，再逐个销毁，避免销毁过程中集合改变
            if (obj is not IList list)
            {
                list = new List<Object>();
                foreach (var item in ems)
                {
                    if (item is IDisposable) list.Add(item);
                }
            }
            foreach (var item in list)
            {
                if (item is IDisposable disp)
                {
                    try
                    {
                        //(item as IDisposable).TryDispose();
                        // 只需要释放一层，不需要递归
                        // 因为一般每一个对象负责自己内部成员的释放
                        disp.Dispose();
                    }
                    catch { }
                }
            }
        }
        // 对象销毁
        if (obj is IDisposable disp2)
        {
            try
            {
                disp2.Dispose();
            }
            catch { }
        }

        return obj;
    }
}