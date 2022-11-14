using XML.Core.Data;

namespace XML.Core.Algorithms;

/// <summary>采样接口。负责降采样和插值处理，用于处理时序数据</summary>
public interface ISampling
{
    /// <summary>
    /// 对齐模式。每个桶X轴对齐方式
    /// </summary>
    AlignModes AlignMode { get; set; }

    /// <summary>
    /// 插值填充算法
    /// </summary>
    IInterpolation Interpolation { get; set; }

    /// <summary>
    /// 降采样处理
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="threshold">阈值，采样数</param>
    /// <returns></returns>
    TimePoint[] Down(TimePoint[] data, Int32 threshold);

    /// <summary>
    /// 混合处理，降采样和插值
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="size">桶大小。如60/3600/86400</param>
    /// <param name="offset">偏移量。时间不是对齐零点时使用</param>
    /// <returns></returns>
    TimePoint[] Process(TimePoint[] data, Int32 size, Int32 offset = 0);
}