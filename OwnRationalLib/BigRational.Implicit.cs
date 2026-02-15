using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的隐式转换
public partial struct BigRational
{
    /// <summary>
    /// 将 <c><see cref="sbyte"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(sbyte value) => new(value);

    /// <summary>
    /// 将 <c><see cref="byte"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(byte value) => new(value);

    /// <summary>
    /// 将 <c><see cref="short"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(short value) => new(value);

    /// <summary>
    /// 将 <c><see cref="ushort"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(ushort value) => new(value);

    /// <summary>
    /// 将 <c><see cref="int"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(int value) => new(value);

    /// <summary>
    /// 将 <c><see cref="uint"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(uint value) => new(value);

    /// <summary>
    /// 将 <c><see cref="long"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(long value) => new(value);

    /// <summary>
    /// 将 <c><see cref="ulong"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(ulong value) => new(value);

    /// <summary>
    /// 将 <c><see cref="BigInteger"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(BigInteger value) => new(value);

    /// <summary>
    /// 将 <c><see cref="float"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(float value)
    {
        // 处理特殊值
        if (float.IsNaN(value))
        {
            return NaN;
        }
        else if (float.IsPositiveInfinity(value))
        {
            return PositiveInfinity;
        }
        else if (float.IsNegativeInfinity(value))
        {
            return NegativeInfinity;
        }

        // 处理零值
        if (value == 0f)
        {
            return Zero;
        }

        // 解析 float 的 IEEE 754 二进制结构
        var bits = BitConverter.SingleToInt32Bits(value);
        var negative = (bits >> 31) != 0;
        var exponent = (bits >> 23) & 0xFF;
        var mantissa = bits & 0x7FFFFF;

        if (exponent == 0)
        {
            // 次正规数
            exponent = 1 - 127;
        }
        else
        {
            // 正规数, 隐含最高位
            mantissa |= 1 << 23;
            exponent -= 127;
        }

        // 计算分子和分母
        var num = (BigInteger)mantissa;
        var den = BigInteger.One << 23;

        // 根据指数调整分子和分母
        if (exponent > 0)
        {
            num <<= exponent;
        }
        else if (exponent < 0)
        {
            den <<= -exponent;
        }

        // 根据符号调整分子
        if (negative)
        {
            num = -num;
        }

        // 返回结果
        return new(num, den);
    }

    /// <summary>
    /// 将 <c><see cref="double"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(double value)
    {
        // 处理特殊值
        if (double.IsNaN(value))
        {
            return NaN;
        }
        else if (double.IsPositiveInfinity(value))
        {
            return PositiveInfinity;
        }
        else if (double.IsNegativeInfinity(value))
        {
            return NegativeInfinity;
        }

        // 处理零值
        if (value == 0d)
        {
            return Zero;
        }

        // 解析 double 的 IEEE 754 二进制结构
        var bits = BitConverter.DoubleToInt64Bits(value);
        var negative = (bits >> 63) != 0;
        var exponent = (int)((bits >> 52) & 0x7FFL);
        var mantissa = bits & 0xFFFFFFFFFFFFFL;

        if (exponent == 0)
        {
            // 次正规数
            exponent = 1 - 1023;
        }
        else
        {
            // 正规数, 隐含最高位
            mantissa |= 1L << 52;
            exponent -= 1023;
        }

        // 计算分子和分母
        var num = (BigInteger)mantissa;
        var den = BigInteger.One << 52;

        // 根据指数调整分子和分母
        if (exponent > 0)
        {
            num <<= exponent;
        }
        else if (exponent < 0)
        {
            den <<= -exponent;
        }

        // 根据符号调整分子
        if (negative)
        {
            num = -num;
        }

        // 返回结果
        return new(num, den);
    }

    /// <summary>
    /// 将 <c><see cref="decimal"/></c> 隐式转换为大有理数
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的大有理数</returns>
    public static implicit operator BigRational(decimal value)
    {
        // 处理特殊值
        if (value == 0m)
        {
            return Zero;
        }

        // 解析 decimal 的二进制结构
        var bits = decimal.GetBits(value);

        // bits[0], bits[1], bits[2] 组成96位整数
        var num = ((BigInteger)(uint)bits[2] << 64) + ((BigInteger)(uint)bits[1] << 32) + (uint)bits[0];
        var negative = (bits[3] & 0x80000000) != 0;
        var scale = (bits[3] >> 16) & 0xFF;

        // 根据符号调整分子
        if (negative)
        {
            num = -num;
        }

        // 分母是 10 的 scale 次方
        var den = BigInteger.Pow(10, scale);

        // 返回结果
        return new(num, den);
    }
}
