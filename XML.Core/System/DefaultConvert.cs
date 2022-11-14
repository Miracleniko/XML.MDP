using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using XML.Core;

namespace System;

/// <summary>默认转换</summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class DefaultConvert
{
    private static readonly DateTime _dt1970 = new DateTime(1970, 1, 1);
    private static readonly DateTimeOffset _dto1970 = new DateTimeOffset(new DateTime(1970, 1, 1));
    private static readonly long _maxSeconds = (long)(DateTime.MaxValue - DateTime.MinValue).TotalSeconds;
    private static readonly long _maxMilliseconds = (long)(DateTime.MaxValue - DateTime.MinValue).TotalMilliseconds;
    /// <summary>转为整数，转换失败时返回默认值。支持字符串、全角、字节数组（小端）、时间（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual int ToInt(object value, int defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string s = DefaultConvert.ToDBC(str.Replace(",", (string)null)).Trim();
                int result;
                return s.IsNullOrEmpty() || !int.TryParse(s, out result) ? defaultValue : result;
            case DateTime dateTime:
                if (dateTime == DateTime.MinValue)
                    return 0;
                if (dateTime == DateTime.MaxValue)
                    return -1;
                double totalSeconds1 = (dateTime - DefaultConvert._dt1970).TotalSeconds;
                return totalSeconds1 < (double)int.MaxValue ? (int)totalSeconds1 : throw new InvalidDataException("时间过大，数值超过Int32.MaxValue");
            case DateTimeOffset dateTimeOffset:
                if (dateTimeOffset == DateTimeOffset.MinValue)
                    return 0;
                double totalSeconds2 = (dateTimeOffset - DefaultConvert._dto1970).TotalSeconds;
                return totalSeconds2 < (double)int.MaxValue ? (int)totalSeconds2 : throw new InvalidDataException("时间过大，数值超过Int32.MaxValue");
            case byte[] numArray:
                if (numArray == null || numArray.Length == 0)
                    return defaultValue;
                switch (numArray.Length)
                {
                    case 1:
                        return (int)numArray[0];
                    case 2:
                        return (int)BitConverter.ToInt16(numArray, 0);
                    case 3:
                        return BitConverter.ToInt32(new byte[4]
                        {
                numArray[0],
                numArray[1],
                numArray[2],
                (byte) 0
                        }, 0);
                    case 4:
                        return BitConverter.ToInt32(numArray, 0);
                }
                break;
        }
        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>转为长整数。支持字符串、全角、字节数组（小端）、时间（Unix毫秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual long ToLong(object value, long defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string s = DefaultConvert.ToDBC(str.Replace(",", (string)null)).Trim();
                long result;
                return s.IsNullOrEmpty() || !long.TryParse(s, out result) ? defaultValue : result;
            case DateTime dateTime:
                return dateTime == DateTime.MinValue ? 0L : (long)(dateTime - DefaultConvert._dt1970).TotalMilliseconds;
            case DateTimeOffset dateTimeOffset:
                return dateTimeOffset == DateTimeOffset.MinValue ? 0L : (long)(dateTimeOffset - DefaultConvert._dto1970).TotalMilliseconds;
            case byte[] numArray:
                if (numArray == null || numArray.Length == 0)
                    return defaultValue;
                switch (numArray.Length)
                {
                    case 1:
                        return (long)numArray[0];
                    case 2:
                        return (long)BitConverter.ToInt16(numArray, 0);
                    case 3:
                        return (long)BitConverter.ToInt32(new byte[4]
                        {
                numArray[0],
                numArray[1],
                numArray[2],
                (byte) 0
                        }, 0);
                    case 4:
                        return (long)BitConverter.ToInt32(numArray, 0);
                    case 8:
                        return BitConverter.ToInt64(numArray, 0);
                }
                break;
        }
        try
        {
            return Convert.ToInt64(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>转为浮点数</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual double ToDouble(object value, double defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string s = DefaultConvert.ToDBC(str).Trim();
                double result;
                return s.IsNullOrEmpty() || !double.TryParse(s, out result) ? defaultValue : result;
            case byte[] numArray when numArray.Length <= 8:
                return BitConverter.ToDouble(numArray, 0);
            default:
                try
                {
                    return Convert.ToDouble(value);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }
    /// <summary>转为高精度浮点数</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual Decimal ToDecimal(object value, Decimal defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string s = DefaultConvert.ToDBC(str).Trim();
                Decimal result;
                return s.IsNullOrEmpty() || !Decimal.TryParse(s, out result) ? defaultValue : result;
            case byte[] src:
                if (src == null || src.Length == 0)
                    return defaultValue;
                switch (src.Length)
                {
                    case 1:
                        return (Decimal)src[0];
                    case 2:
                        return (Decimal)BitConverter.ToInt16(src, 0);
                    case 3:
                        return (Decimal)BitConverter.ToInt32(new byte[4]
                        {
                src[0],
                src[1],
                src[2],
                (byte) 0
                        }, 0);
                    case 4:
                        return (Decimal)BitConverter.ToInt32(src, 0);
                    default:
                        if (src.Length < 8)
                        {
                            byte[] dst = new byte[8];
                            Buffer.BlockCopy((Array)src, 0, (Array)dst, 0, src.Length);
                            src = dst;
                        }
                        return BitConverter.ToDouble(src, 0).ToDecimal();
                }
            default:
                try
                {
                    return Convert.ToDecimal(value);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }
    /// <summary>转为布尔型。支持大小写True/False、0和非零</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual bool ToBoolean(object value, bool defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        if (value is string str1)
        {
            string str = str1.Trim();
            if (str.IsNullOrEmpty())
                return defaultValue;
            bool result1;
            if (bool.TryParse(str, out result1))
                return result1;
            if (string.Equals(str, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(str, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return false;
            int result2;
            return int.TryParse(DefaultConvert.ToDBC(str), out result2) ? result2 > 0 : defaultValue;
        }
        try
        {
            return Convert.ToBoolean(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual DateTime ToDateTime(object value, DateTime defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str1:
                string s = str1.Trim();
                if (s.IsNullOrEmpty())
                    return defaultValue;
                bool flag = false;
                if (s.EndsWithIgnoreCase(" UTC"))
                {
                    flag = true;
                    string str = s;
                    s = str.Substring(0, str.Length - 4);
                }
                DateTime result;
                if (!DateTime.TryParse(s, out result) && (!s.Contains('-') || !DateTime.TryParseExact(s, "yyyy-M-d", (IFormatProvider)null, DateTimeStyles.None, out result)) && (!s.Contains('/') || !DateTime.TryParseExact(s, "yyyy/M/d", (IFormatProvider)null, DateTimeStyles.None, out result)) && !DateTime.TryParseExact(s, "yyyyMMddHHmmss", (IFormatProvider)null, DateTimeStyles.None, out result) && !DateTime.TryParseExact(s, "yyyyMMdd", (IFormatProvider)null, DateTimeStyles.None, out result) && !DateTime.TryParse(s, out result))
                    result = defaultValue;
                if (flag)
                    result = new DateTime(result.Ticks, DateTimeKind.Utc);
                return result;
            case int num1:
                return (long)num1 >= DefaultConvert._maxSeconds || (long)num1 <= -DefaultConvert._maxSeconds ? defaultValue : DefaultConvert._dt1970.AddSeconds((double)num1);
            case long num2:
                if (num2 >= DefaultConvert._maxMilliseconds || num2 <= -DefaultConvert._maxMilliseconds)
                    return defaultValue;
                return num2 > 3153600000L ? DefaultConvert._dt1970.AddMilliseconds((double)num2) : DefaultConvert._dt1970.AddSeconds((double)num2);
            default:
                try
                {
                    return Convert.ToDateTime(value);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }
    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public virtual DateTimeOffset ToDateTimeOffset(
      object value,
      DateTimeOffset defaultValue)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string input = str.Trim();
                DateTimeOffset result;
                return input.IsNullOrEmpty() || !DateTimeOffset.TryParse(input, out result) && (!input.Contains('-') || !DateTimeOffset.TryParseExact(input, "yyyy-M-d", (IFormatProvider)null, DateTimeStyles.None, out result)) && (!input.Contains('/') || !DateTimeOffset.TryParseExact(input, "yyyy/M/d", (IFormatProvider)null, DateTimeStyles.None, out result)) && !DateTimeOffset.TryParseExact(input, "yyyyMMddHHmmss", (IFormatProvider)null, DateTimeStyles.None, out result) && !DateTimeOffset.TryParseExact(input, "yyyyMMdd", (IFormatProvider)null, DateTimeStyles.None, out result) ? defaultValue : result;
            case int seconds:
                return (long)seconds >= DefaultConvert._maxSeconds || (long)seconds <= -DefaultConvert._maxSeconds ? defaultValue : DefaultConvert._dto1970.AddSeconds((double)seconds);
            case long num:
                if (num >= DefaultConvert._maxMilliseconds || num <= -DefaultConvert._maxMilliseconds)
                    return defaultValue;
                return num > 3153600000L ? DefaultConvert._dto1970.AddMilliseconds((double)num) : DefaultConvert._dto1970.AddSeconds((double)num);
            default:
                try
                {
                    return (DateTimeOffset)Convert.ToDateTime(value);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }
    /// <summary>全角为半角</summary>
    /// <remarks>全角半角的关系是相差0xFEE0</remarks>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string ToDBC(string str)
    {
        char[] charArray = str.ToCharArray();
        for (int index = 0; index < charArray.Length; ++index)
        {
            if (charArray[index] == '　')
                charArray[index] = ' ';
            else if (charArray[index] > '\uFF00' && charArray[index] < '｟')
                charArray[index] = (char)((uint)charArray[index] - 65248U);
        }
        return new string(charArray);
    }
    /// <summary>去掉时间日期秒后面部分，可指定毫秒ms、分m、小时h</summary>
    /// <param name="value">时间日期</param>
    /// <param name="format">格式字符串，默认s格式化到秒，ms格式化到毫秒</param>
    /// <returns></returns>
    public virtual DateTime Trim(DateTime value, string format) => format == "ms" ? new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind) : (format == "s" ? new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind) : (format == "m" ? new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind) : (format == "h" ? new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind) : value)));
    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="useMillisecond">是否使用毫秒</param>
    /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public virtual string ToFullString(DateTime value, bool useMillisecond, string emptyValue = null)
    {
        if (emptyValue != null && value <= DateTime.MinValue)
            return emptyValue;
        char[] chArray1 = useMillisecond ? "yyyy-MM-dd HH:mm:ss.fff".ToCharArray() : "yyyy-MM-dd HH:mm:ss".ToCharArray();
        int num1 = 0;
        int year = value.Year;
        char[] chArray2 = chArray1;
        int index1 = num1;
        int num2 = index1 + 1;
        int num3 = (int)(ushort)(48 + year / 1000);
        chArray2[index1] = (char)num3;
        int num4 = year % 1000;
        char[] chArray3 = chArray1;
        int index2 = num2;
        int num5 = index2 + 1;
        int num6 = (int)(ushort)(48 + num4 / 100);
        chArray3[index2] = (char)num6;
        int num7 = num4 % 100;
        char[] chArray4 = chArray1;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = (int)(ushort)(48 + num7 / 10);
        chArray4[index3] = (char)num9;
        int num10 = num7 % 10;
        char[] chArray5 = chArray1;
        int index4 = num8;
        int num11 = index4 + 1;
        int num12 = (int)(ushort)(48 + num10);
        chArray5[index4] = (char)num12;
        int num13 = num11 + 1;
        int month = value.Month;
        char[] chArray6 = chArray1;
        int index5 = num13;
        int num14 = index5 + 1;
        int num15 = (int)(ushort)(48 + month / 10);
        chArray6[index5] = (char)num15;
        char[] chArray7 = chArray1;
        int index6 = num14;
        int num16 = index6 + 1;
        int num17 = (int)(ushort)(48 + month % 10);
        chArray7[index6] = (char)num17;
        int num18 = num16 + 1;
        int day = value.Day;
        char[] chArray8 = chArray1;
        int index7 = num18;
        int num19 = index7 + 1;
        int num20 = (int)(ushort)(48 + day / 10);
        chArray8[index7] = (char)num20;
        char[] chArray9 = chArray1;
        int index8 = num19;
        int num21 = index8 + 1;
        int num22 = (int)(ushort)(48 + day % 10);
        chArray9[index8] = (char)num22;
        int num23 = num21 + 1;
        int hour = value.Hour;
        char[] chArray10 = chArray1;
        int index9 = num23;
        int num24 = index9 + 1;
        int num25 = (int)(ushort)(48 + hour / 10);
        chArray10[index9] = (char)num25;
        char[] chArray11 = chArray1;
        int index10 = num24;
        int num26 = index10 + 1;
        int num27 = (int)(ushort)(48 + hour % 10);
        chArray11[index10] = (char)num27;
        int num28 = num26 + 1;
        int minute = value.Minute;
        char[] chArray12 = chArray1;
        int index11 = num28;
        int num29 = index11 + 1;
        int num30 = (int)(ushort)(48 + minute / 10);
        chArray12[index11] = (char)num30;
        char[] chArray13 = chArray1;
        int index12 = num29;
        int num31 = index12 + 1;
        int num32 = (int)(ushort)(48 + minute % 10);
        chArray13[index12] = (char)num32;
        int num33 = num31 + 1;
        int second = value.Second;
        char[] chArray14 = chArray1;
        int index13 = num33;
        int num34 = index13 + 1;
        int num35 = (int)(ushort)(48 + second / 10);
        chArray14[index13] = (char)num35;
        char[] chArray15 = chArray1;
        int index14 = num34;
        int num36 = index14 + 1;
        int num37 = (int)(ushort)(48 + second % 10);
        chArray15[index14] = (char)num37;
        if (useMillisecond)
        {
            int num38 = num36 + 1;
            int millisecond = value.Millisecond;
            char[] chArray16 = chArray1;
            int index15 = num38;
            int num39 = index15 + 1;
            int num40 = (int)(ushort)(48 + millisecond / 100);
            chArray16[index15] = (char)num40;
            char[] chArray17 = chArray1;
            int index16 = num39;
            int num41 = index16 + 1;
            int num42 = (int)(ushort)(48 + millisecond % 100 / 10);
            chArray17[index16] = (char)num42;
            char[] chArray18 = chArray1;
            int index17 = num41;
            int num43 = index17 + 1;
            int num44 = (int)(ushort)(48 + millisecond % 10);
            chArray18[index17] = (char)num44;
        }
        return new string(chArray1);
    }
    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="useMillisecond">是否使用毫秒</param>
    /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public virtual string ToFullString(
      DateTimeOffset value,
      bool useMillisecond,
      string emptyValue = null)
    {
        if (emptyValue != null && value <= DateTimeOffset.MinValue)
            return emptyValue;
        char[] chArray1 = useMillisecond ? "yyyy-MM-dd HH:mm:ss.fff +08:00".ToCharArray() : "yyyy-MM-dd HH:mm:ss +08:00".ToCharArray();
        int num1 = 0;
        int year = value.Year;
        char[] chArray2 = chArray1;
        int index1 = num1;
        int num2 = index1 + 1;
        int num3 = (int)(ushort)(48 + year / 1000);
        chArray2[index1] = (char)num3;
        int num4 = year % 1000;
        char[] chArray3 = chArray1;
        int index2 = num2;
        int num5 = index2 + 1;
        int num6 = (int)(ushort)(48 + num4 / 100);
        chArray3[index2] = (char)num6;
        int num7 = num4 % 100;
        char[] chArray4 = chArray1;
        int index3 = num5;
        int num8 = index3 + 1;
        int num9 = (int)(ushort)(48 + num7 / 10);
        chArray4[index3] = (char)num9;
        int num10 = num7 % 10;
        char[] chArray5 = chArray1;
        int index4 = num8;
        int num11 = index4 + 1;
        int num12 = (int)(ushort)(48 + num10);
        chArray5[index4] = (char)num12;
        int num13 = num11 + 1;
        int month = value.Month;
        char[] chArray6 = chArray1;
        int index5 = num13;
        int num14 = index5 + 1;
        int num15 = (int)(ushort)(48 + month / 10);
        chArray6[index5] = (char)num15;
        char[] chArray7 = chArray1;
        int index6 = num14;
        int num16 = index6 + 1;
        int num17 = (int)(ushort)(48 + month % 10);
        chArray7[index6] = (char)num17;
        int num18 = num16 + 1;
        int day = value.Day;
        char[] chArray8 = chArray1;
        int index7 = num18;
        int num19 = index7 + 1;
        int num20 = (int)(ushort)(48 + day / 10);
        chArray8[index7] = (char)num20;
        char[] chArray9 = chArray1;
        int index8 = num19;
        int num21 = index8 + 1;
        int num22 = (int)(ushort)(48 + day % 10);
        chArray9[index8] = (char)num22;
        int num23 = num21 + 1;
        int hour = value.Hour;
        char[] chArray10 = chArray1;
        int index9 = num23;
        int num24 = index9 + 1;
        int num25 = (int)(ushort)(48 + hour / 10);
        chArray10[index9] = (char)num25;
        char[] chArray11 = chArray1;
        int index10 = num24;
        int num26 = index10 + 1;
        int num27 = (int)(ushort)(48 + hour % 10);
        chArray11[index10] = (char)num27;
        int num28 = num26 + 1;
        int minute = value.Minute;
        char[] chArray12 = chArray1;
        int index11 = num28;
        int num29 = index11 + 1;
        int num30 = (int)(ushort)(48 + minute / 10);
        chArray12[index11] = (char)num30;
        char[] chArray13 = chArray1;
        int index12 = num29;
        int num31 = index12 + 1;
        int num32 = (int)(ushort)(48 + minute % 10);
        chArray13[index12] = (char)num32;
        int num33 = num31 + 1;
        int second = value.Second;
        char[] chArray14 = chArray1;
        int index13 = num33;
        int num34 = index13 + 1;
        int num35 = (int)(ushort)(48 + second / 10);
        chArray14[index13] = (char)num35;
        char[] chArray15 = chArray1;
        int index14 = num34;
        int num36 = index14 + 1;
        int num37 = (int)(ushort)(48 + second % 10);
        chArray15[index14] = (char)num37;
        int num38 = num36 + 1;
        if (useMillisecond)
        {
            int millisecond = value.Millisecond;
            char[] chArray16 = chArray1;
            int index15 = num38;
            int num39 = index15 + 1;
            int num40 = (int)(ushort)(48 + millisecond / 100);
            chArray16[index15] = (char)num40;
            char[] chArray17 = chArray1;
            int index16 = num39;
            int num41 = index16 + 1;
            int num42 = (int)(ushort)(48 + millisecond % 100 / 10);
            chArray17[index16] = (char)num42;
            char[] chArray18 = chArray1;
            int index17 = num41;
            int num43 = index17 + 1;
            int num44 = (int)(ushort)(48 + millisecond % 10);
            chArray18[index17] = (char)num44;
            num38 = num43 + 1;
        }
        TimeSpan offset = value.Offset;
        char[] chArray19 = chArray1;
        int index18 = num38;
        int num45 = index18 + 1;
        int num46 = offset.TotalSeconds >= 0.0 ? 43 : 45;
        chArray19[index18] = (char)num46;
        int num47 = Math.Abs(offset.Hours);
        char[] chArray20 = chArray1;
        int index19 = num45;
        int num48 = index19 + 1;
        int num49 = (int)(ushort)(48 + num47 / 10);
        chArray20[index19] = (char)num49;
        char[] chArray21 = chArray1;
        int index20 = num48;
        int num50 = index20 + 1;
        int num51 = (int)(ushort)(48 + num47 % 10);
        chArray21[index20] = (char)num51;
        int num52 = num50 + 1;
        int num53 = Math.Abs(offset.Minutes);
        char[] chArray22 = chArray1;
        int index21 = num52;
        int num54 = index21 + 1;
        int num55 = (int)(ushort)(48 + num53 / 10);
        chArray22[index21] = (char)num55;
        char[] chArray23 = chArray1;
        int index22 = num54;
        int num56 = index22 + 1;
        int num57 = (int)(ushort)(48 + num53 % 10);
        chArray23[index22] = (char)num57;
        return new string(chArray1);
    }
    /// <summary>时间日期转为指定格式字符串</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="format">格式化字符串</param>
    /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public virtual string ToString(DateTime value, string format, string emptyValue)
    {
        if (emptyValue != null && value <= DateTime.MinValue)
            return emptyValue;
        return format.IsNullOrEmpty() || format == "yyyy-MM-dd HH:mm:ss" ? this.ToFullString(value, false, emptyValue) : value.ToString(format);
    }
    /// <summary>获取内部真实异常</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public virtual Exception GetTrue(Exception ex)
    {
        switch (ex)
        {
            case null:
                return (Exception)null;
            case AggregateException _:
                return this.GetTrue((ex as AggregateException).Flatten().InnerException);
            case TargetInvocationException _:
                return this.GetTrue((ex as TargetInvocationException).InnerException);
            case TypeInitializationException _:
                return this.GetTrue((ex as TypeInitializationException).InnerException);
            default:
                return ex.GetBaseException() ?? ex;
        }
    }
    /// <summary>获取异常消息</summary>
    /// <param name="ex">异常</param>
    /// <returns></returns>
    public virtual string GetMessage(Exception ex)
    {
        string str = ex?.ToString() ?? "";
        return str.IsNullOrEmpty() ? (string)null : ((IEnumerable<string>)str.Split(Environment.NewLine)).Where<string>((Func<string, bool>)(e => !e.StartsWith("---") && !e.Contains("System.Runtime.ExceptionServices") && !e.Contains("System.Runtime.CompilerServices"))).Join<string>(Environment.NewLine);
    }
    /// <summary>字节单位字符串</summary>
    /// <param name="value">数值</param>
    /// <param name="format">格式化字符串</param>
    /// <returns></returns>
    public virtual string ToGMK(ulong value, string format = null)
    {
        if (value < 1024UL)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
            interpolatedStringHandler.AppendFormatted<ulong>(value, "n0");
            return interpolatedStringHandler.ToStringAndClear();
        }
        if (format.IsNullOrEmpty())
            format = "n2";
        double num1 = (double)value / 1024.0;
        if (num1 < 1024.0)
            return num1.ToString(format) + "K";
        double num2 = num1 / 1024.0;
        if (num2 < 1024.0)
            return num2.ToString(format) + "M";
        double num3 = num2 / 1024.0;
        if (num3 < 1024.0)
            return num3.ToString(format) + "G";
        double num4 = num3 / 1024.0;
        if (num4 < 1024.0)
            return num4.ToString(format) + "T";
        double num5 = num4 / 1024.0;
        return num5 < 1024.0 ? num5.ToString(format) + "P" : (num5 / 1024.0).ToString(format) + "E";
    }
}