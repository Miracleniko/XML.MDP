namespace XML.Core.Security;

/// <summary>CRC16校验</summary>
public sealed class Crc16
{
    /// <summary>CRC16表</summary>
    private static readonly ushort[] CrcTable = new ushort[256]
    {
      (ushort) 0,
      (ushort) 4129,
      (ushort) 8258,
      (ushort) 12387,
      (ushort) 16516,
      (ushort) 20645,
      (ushort) 24774,
      (ushort) 28903,
      (ushort) 33032,
      (ushort) 37161,
      (ushort) 41290,
      (ushort) 45419,
      (ushort) 49548,
      (ushort) 53677,
      (ushort) 57806,
      (ushort) 61935,
      (ushort) 4657,
      (ushort) 528,
      (ushort) 12915,
      (ushort) 8786,
      (ushort) 21173,
      (ushort) 17044,
      (ushort) 29431,
      (ushort) 25302,
      (ushort) 37689,
      (ushort) 33560,
      (ushort) 45947,
      (ushort) 41818,
      (ushort) 54205,
      (ushort) 50076,
      (ushort) 62463,
      (ushort) 58334,
      (ushort) 9314,
      (ushort) 13379,
      (ushort) 1056,
      (ushort) 5121,
      (ushort) 25830,
      (ushort) 29895,
      (ushort) 17572,
      (ushort) 21637,
      (ushort) 42346,
      (ushort) 46411,
      (ushort) 34088,
      (ushort) 38153,
      (ushort) 58862,
      (ushort) 62927,
      (ushort) 50604,
      (ushort) 54669,
      (ushort) 13907,
      (ushort) 9842,
      (ushort) 5649,
      (ushort) 1584,
      (ushort) 30423,
      (ushort) 26358,
      (ushort) 22165,
      (ushort) 18100,
      (ushort) 46939,
      (ushort) 42874,
      (ushort) 38681,
      (ushort) 34616,
      (ushort) 63455,
      (ushort) 59390,
      (ushort) 55197,
      (ushort) 51132,
      (ushort) 18628,
      (ushort) 22757,
      (ushort) 26758,
      (ushort) 30887,
      (ushort) 2112,
      (ushort) 6241,
      (ushort) 10242,
      (ushort) 14371,
      (ushort) 51660,
      (ushort) 55789,
      (ushort) 59790,
      (ushort) 63919,
      (ushort) 35144,
      (ushort) 39273,
      (ushort) 43274,
      (ushort) 47403,
      (ushort) 23285,
      (ushort) 19156,
      (ushort) 31415,
      (ushort) 27286,
      (ushort) 6769,
      (ushort) 2640,
      (ushort) 14899,
      (ushort) 10770,
      (ushort) 56317,
      (ushort) 52188,
      (ushort) 64447,
      (ushort) 60318,
      (ushort) 39801,
      (ushort) 35672,
      (ushort) 47931,
      (ushort) 43802,
      (ushort) 27814,
      (ushort) 31879,
      (ushort) 19684,
      (ushort) 23749,
      (ushort) 11298,
      (ushort) 15363,
      (ushort) 3168,
      (ushort) 7233,
      (ushort) 60846,
      (ushort) 64911,
      (ushort) 52716,
      (ushort) 56781,
      (ushort) 44330,
      (ushort) 48395,
      (ushort) 36200,
      (ushort) 40265,
      (ushort) 32407,
      (ushort) 28342,
      (ushort) 24277,
      (ushort) 20212,
      (ushort) 15891,
      (ushort) 11826,
      (ushort) 7761,
      (ushort) 3696,
      (ushort) 65439,
      (ushort) 61374,
      (ushort) 57309,
      (ushort) 53244,
      (ushort) 48923,
      (ushort) 44858,
      (ushort) 40793,
      (ushort) 36728,
      (ushort) 37256,
      (ushort) 33193,
      (ushort) 45514,
      (ushort) 41451,
      (ushort) 53516,
      (ushort) 49453,
      (ushort) 61774,
      (ushort) 57711,
      (ushort) 4224,
      (ushort) 161,
      (ushort) 12482,
      (ushort) 8419,
      (ushort) 20484,
      (ushort) 16421,
      (ushort) 28742,
      (ushort) 24679,
      (ushort) 33721,
      (ushort) 37784,
      (ushort) 41979,
      (ushort) 46042,
      (ushort) 49981,
      (ushort) 54044,
      (ushort) 58239,
      (ushort) 62302,
      (ushort) 689,
      (ushort) 4752,
      (ushort) 8947,
      (ushort) 13010,
      (ushort) 16949,
      (ushort) 21012,
      (ushort) 25207,
      (ushort) 29270,
      (ushort) 46570,
      (ushort) 42443,
      (ushort) 38312,
      (ushort) 34185,
      (ushort) 62830,
      (ushort) 58703,
      (ushort) 54572,
      (ushort) 50445,
      (ushort) 13538,
      (ushort) 9411,
      (ushort) 5280,
      (ushort) 1153,
      (ushort) 29798,
      (ushort) 25671,
      (ushort) 21540,
      (ushort) 17413,
      (ushort) 42971,
      (ushort) 47098,
      (ushort) 34713,
      (ushort) 38840,
      (ushort) 59231,
      (ushort) 63358,
      (ushort) 50973,
      (ushort) 55100,
      (ushort) 9939,
      (ushort) 14066,
      (ushort) 1681,
      (ushort) 5808,
      (ushort) 26199,
      (ushort) 30326,
      (ushort) 17941,
      (ushort) 22068,
      (ushort) 55628,
      (ushort) 51565,
      (ushort) 63758,
      (ushort) 59695,
      (ushort) 39368,
      (ushort) 35305,
      (ushort) 47498,
      (ushort) 43435,
      (ushort) 22596,
      (ushort) 18533,
      (ushort) 30726,
      (ushort) 26663,
      (ushort) 6336,
      (ushort) 2273,
      (ushort) 14466,
      (ushort) 10403,
      (ushort) 52093,
      (ushort) 56156,
      (ushort) 60223,
      (ushort) 64286,
      (ushort) 35833,
      (ushort) 39896,
      (ushort) 43963,
      (ushort) 48026,
      (ushort) 19061,
      (ushort) 23124,
      (ushort) 27191,
      (ushort) 31254,
      (ushort) 2801,
      (ushort) 6864,
      (ushort) 10931,
      (ushort) 14994,
      (ushort) 64814,
      (ushort) 60687,
      (ushort) 56684,
      (ushort) 52557,
      (ushort) 48554,
      (ushort) 44427,
      (ushort) 40424,
      (ushort) 36297,
      (ushort) 31782,
      (ushort) 27655,
      (ushort) 23652,
      (ushort) 19525,
      (ushort) 15522,
      (ushort) 11395,
      (ushort) 7392,
      (ushort) 3265,
      (ushort) 61215,
      (ushort) 65342,
      (ushort) 53085,
      (ushort) 57212,
      (ushort) 44955,
      (ushort) 49082,
      (ushort) 36825,
      (ushort) 40952,
      (ushort) 28183,
      (ushort) 32310,
      (ushort) 20053,
      (ushort) 24180,
      (ushort) 11923,
      (ushort) 16050,
      (ushort) 3793,
      (ushort) 7920
    };
    private static readonly ushort[] crc_ta = new ushort[16]
    {
      (ushort) 0,
      (ushort) 52225,
      (ushort) 55297,
      (ushort) 5120,
      (ushort) 61441,
      (ushort) 15360,
      (ushort) 10240,
      (ushort) 58369,
      (ushort) 40961,
      (ushort) 27648,
      (ushort) 30720,
      (ushort) 46081,
      (ushort) 20480,
      (ushort) 39937,
      (ushort) 34817,
      (ushort) 17408
    };

    /// <summary>校验值</summary>
    public ushort Value { get; set; } = ushort.MaxValue;

    /// <summary>重置清零</summary>
    public Crc16 Reset()
    {
        this.Value = ushort.MaxValue;
        return this;
    }

    /// <summary>添加整数进行校验</summary>
    /// <param name="value">
    /// the byte is taken as the lower 8 bits of value
    /// </param>
    public Crc16 Update(short value)
    {
        this.Value = (ushort)((uint)this.Value << 8 ^ (uint)Crc16.CrcTable[(int)this.Value >> 8 ^ (int)value]);
        return this;
    }

    /// <summary>添加字节数组进行校验，查表计算  CRC16-CCITT x16+x12+x5+1 1021  ISO HDLC, ITU X.25, V.34/V.41/V.42, PPP-FCS</summary>
    /// <remarks>字符串123456789的Crc16是31C3</remarks>
    /// <param name="buffer">数据缓冲区</param>
    /// <param name="offset">偏移量</param>
    /// <param name="count">字节个数</param>
    public Crc16 Update(byte[] buffer, int offset = 0, int count = -1)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (count <= 0)
            count = buffer.Length;
        if (offset < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        ushort num1 = this.Value;
        ushort num2 = (ushort)((uint)num1 ^ (uint)num1);
        for (int index = 0; index < count; ++index)
            num2 = (ushort)((uint)num2 << 8 ^ (uint)Crc16.CrcTable[((int)num2 >> 8 ^ (int)buffer[offset + index]) & (int)byte.MaxValue]);
        this.Value = num2;
        return this;
    }

    /// <summary>添加数据流进行校验，不查表计算  CRC-16 x16+x15+x2+1 8005 IBM SDLC</summary>
    /// <param name="stream"></param>
    /// <param name="count">数量</param>
    public Crc16 Update(Stream stream, long count = -1)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (count <= 0L)
            count = long.MaxValue;
        ushort num1 = this.Value;
        while (--count >= 0L)
        {
            int num2 = stream.ReadByte();
            if (num2 != -1)
            {
                num1 ^= (ushort)(byte)num2;
                for (int index = 0; index < 8; ++index)
                {
                    if (((int)num1 & 1) != 0)
                        num1 = (ushort)((int)num1 >> 1 ^ 40961);
                    else
                        num1 >>= 1;
                }
            }
            else
                break;
        }
        this.Value = num1;
        return this;
    }

    /// <summary>计算校验码</summary>
    /// <param name="buf"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static ushort Compute(byte[] buf, int offset = 0, int count = -1)
    {
        Crc16 crc16 = new Crc16();
        crc16.Update(buf, offset, count);
        return crc16.Value;
    }

    /// <summary>计算数据流校验码</summary>
    /// <param name="stream"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static ushort Compute(Stream stream, int count = -1)
    {
        Crc16 crc16 = new Crc16();
        crc16.Update(stream, (long)count);
        return crc16.Value;
    }

    /// <summary>计算数据流校验码，指定起始位置和字节数偏移量</summary>
    /// <remarks>
    /// 一般用于计算数据包校验码，需要回过头去开始校验，并且可能需要跳过最后的校验码长度。
    /// position小于0时，数据流从当前位置开始计算校验；
    /// position大于等于0时，数据流移到该位置开始计算校验，最后由count决定可能差几个字节不参与计算；
    /// </remarks>
    /// <param name="stream"></param>
    /// <param name="position">如果大于等于0，则表示从该位置开始计算</param>
    /// <param name="count">字节数偏移量，一般用负数表示</param>
    /// <returns></returns>
    public static ushort Compute(Stream stream, long position = -1, int count = -1)
    {
        if (position >= 0L)
        {
            if (count > 0)
                count = -count;
            count += (int)(stream.Position - position);
            stream.Position = position;
        }
        Crc16 crc16 = new Crc16();
        crc16.Update(stream, (long)count);
        return crc16.Value;
    }

    /// <summary>Modbus版Crc校验</summary>
    /// <param name="data"></param>
    /// <param name="offset">偏移</param>
    /// <param name="count">数量</param>
    /// <returns></returns>
    public static ushort ComputeModbus(byte[] data, int offset, int count = -1)
    {
        if (data == null || data.Length < 1)
            return 0;
        ushort modbus = ushort.MaxValue;
        if (count == 0)
            count = data.Length - offset;
        for (int index = offset; index < count; ++index)
        {
            byte num1 = data[index];
            ushort num2 = (ushort)((uint)Crc16.crc_ta[((int)num1 ^ (int)modbus) & 15] ^ (uint)modbus >> 4);
            modbus = (ushort)((uint)Crc16.crc_ta[((int)num1 >> 4 ^ (int)num2) & 15] ^ (uint)num2 >> 4);
        }
        return modbus;
    }

    /// <summary>Modbus版Crc校验</summary>
    /// <param name="stream">数据流</param>
    /// <param name="position">回到该位置开始</param>
    /// <returns></returns>
    public static ushort ComputeModbus(Stream stream, long position = -1)
    {
        if (position >= 0L)
            stream.Position = position;
        ushort modbus = ushort.MaxValue;
        while (stream.Position < stream.Length)
        {
            int num1 = stream.ReadByte();
            ushort num2 = (ushort)((uint)Crc16.crc_ta[(num1 ^ (int)modbus) & 15] ^ (uint)modbus >> 4);
            modbus = (ushort)((uint)Crc16.crc_ta[(num1 >> 4 ^ (int)num2) & 15] ^ (uint)num2 >> 4);
        }
        return modbus;
    }
}