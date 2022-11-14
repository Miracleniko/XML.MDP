using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XML.XCode.Transform;

/// <summary>抽取参数帮助类</summary>
public static class ExtractSettingHelper
{
    /// <summary>拷贝设置参数</summary>
    /// <param name="src"></param>
    /// <param name="set"></param>
    public static IExtractSetting Copy(this IExtractSetting src, IExtractSetting set)
    {
        if (src == null | set == null) return src;

        src.Start = set.Start;
        src.End = set.End;
        src.Row = set.Row;
        src.Step = set.Step;
        src.BatchSize = set.BatchSize;
        //src.Enable = set.Enable;

        return src;
    }

    /// <summary>克隆一份设置参数</summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static IExtractSetting Clone(this IExtractSetting src)
    {
        var set = new ExtractSetting();
        set.Copy(src);

        return set;
    }
}