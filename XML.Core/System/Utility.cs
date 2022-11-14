namespace System;

/// <summary>工具类</summary>
public static class Utility
{
    #region 类型转换
    /// <summary>类型转换提供者</summary>
    /// <remarks>重载默认提供者<seealso cref="DefaultConvert"/>并赋值给<see cref="Convert"/>可改变所有类型转换的行为</remarks>
    public static DefaultConvert Convert { get; set; } = new DefaultConvert();

    /// <summary>转为整数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）、时间（Unix秒不转UTC）</summary>
    /// <remarks>Int16/UInt32/Int64等，可以先转为最常用的Int32后再二次处理</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Int32 ToInt(this Object value, Int32 defaultValue = 0) => Convert.ToInt(value, defaultValue);

    /// <summary>转为长整数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）、时间（Unix毫秒不转UTC）</summary>
    /// <remarks></remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Int64 ToLong(this Object value, Int64 defaultValue = 0) => Convert.ToLong(value, defaultValue);

    /// <summary>转为浮点数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）</summary>
    /// <remarks>Single可以先转为最常用的Double后再二次处理</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Double ToDouble(this Object value, Double defaultValue = 0) => Convert.ToDouble(value, defaultValue);

    /// <summary>转为高精度浮点数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）</summary>
    /// <remarks>Single可以先转为最常用的Double后再二次处理</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Decimal ToDecimal(this Object value, Decimal defaultValue = 0) => Convert.ToDecimal(value, defaultValue);

    /// <summary>转为布尔型，转换失败时返回默认值。支持大小写True/False、0和非零</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Boolean ToBoolean(this Object value, Boolean defaultValue = false) => Convert.ToBoolean(value, defaultValue);

    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒不考虑UTC转本地）</summary>
    /// <param name="value">待转换对象</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this Object value) => Convert.ToDateTime(value, DateTime.MinValue);

    /// <summary>转为时间日期，转换失败时返回默认值</summary>
    /// <remarks><see cref="DateTime.MinValue"/>不是常量无法做默认值</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this Object value, DateTime defaultValue) => Convert.ToDateTime(value, defaultValue);

    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this Object value) => Convert.ToDateTimeOffset(value, DateTimeOffset.MinValue);

    /// <summary>转为时间日期，转换失败时返回默认值</summary>
    /// <remarks><see cref="DateTimeOffset.MinValue"/>不是常量无法做默认值</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this Object value, DateTimeOffset defaultValue) => Convert.ToDateTimeOffset(value, defaultValue);

    /// <summary>去掉时间日期秒后面部分，可指定毫秒ms、分m、小时h</summary>
    /// <param name="value">时间日期</param>
    /// <param name="format">格式字符串，默认s格式化到秒，ms格式化到毫秒</param>
    /// <returns></returns>
    public static DateTime Trim(this DateTime value, String format = "s") => Convert.Trim(value, format);

    /// <summary>去掉时间日期秒后面部分，可指定毫秒</summary>
    /// <param name="value">时间日期</param>
    /// <param name="format">格式字符串，默认s格式化到秒，ms格式化到毫秒</param>
    /// <returns></returns>
    public static DateTimeOffset Trim(this DateTimeOffset value, String format = "s") => new(Convert.Trim(value.DateTime, format), value.Offset);

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串，对UTC时间加后缀</summary>
    /// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
    /// <param name="value">待转换对象</param>
    /// <returns></returns>
    public static String ToFullString(this DateTime value) => Convert.ToFullString(value, false);

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串，支持指定最小时间的字符串</summary>
    /// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="emptyValue">字符串空值时（DateTime.MinValue）显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToFullString(this DateTime value, String emptyValue = null) => Convert.ToFullString(value, false, emptyValue);

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss.fff完整字符串，支持指定最小时间的字符串</summary>
    /// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="useMillisecond">是否使用毫秒</param>
    /// <param name="emptyValue">字符串空值时（DateTime.MinValue）显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToFullString(this DateTime value, Boolean useMillisecond, String emptyValue = null) => Convert.ToFullString(value, useMillisecond, emptyValue);

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss +08:00完整字符串，支持指定最小时间的字符串</summary>
    /// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="emptyValue">字符串空值时（DateTimeOffset.MinValue）显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToFullString(this DateTimeOffset value, String emptyValue = null) => Convert.ToFullString(value, false, emptyValue);

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss.fff +08:00完整字符串，支持指定最小时间的字符串</summary>
    /// <remarks>最常用的时间日期格式，可以无视各平台以及系统自定义的时间格式</remarks>
    /// <param name="value">待转换对象</param>
    /// <param name="useMillisecond">是否使用毫秒</param>
    /// <param name="emptyValue">字符串空值时（DateTimeOffset.MinValue）显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToFullString(this DateTimeOffset value, Boolean useMillisecond, String emptyValue = null) => Convert.ToFullString(value, useMillisecond, emptyValue);

    /// <summary>时间日期转为指定格式字符串</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToString(this DateTime value, String format, String emptyValue) => Convert.ToString(value, format, emptyValue);

    /// <summary>字节单位字符串</summary>
    /// <param name="value">数值</param>
    /// <param name="format">格式化字符串</param>
    /// <returns></returns>
    public static String ToGMK(this UInt64 value, String format = null) => Convert.ToGMK(value, format);

    /// <summary>字节单位字符串</summary>
    /// <param name="value">数值</param>
    /// <param name="format">格式化字符串</param>
    /// <returns></returns>
    public static String ToGMK(this Int64 value, String format = null) => value < 0 ? value + "" : Convert.ToGMK((UInt64)value, format);
    #endregion

    #region 异常处理
    /// <summary>获取内部真实异常</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Exception GetTrue(this Exception ex) => Convert.GetTrue(ex);

    /// <summary>获取异常消息</summary>
    /// <param name="ex">异常</param>
    /// <returns></returns>
    public static String GetMessage(this Exception ex) => Convert.GetMessage(ex);
    #endregion
}