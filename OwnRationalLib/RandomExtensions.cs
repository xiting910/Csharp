using System.Numerics;

namespace OwnRationalLib;

/// <summary>
/// 提供 <see cref="Random"/> 类的扩展方法
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// 返回一个范围在 [0, <paramref name="maxValue"/>) 之间的随机 <see cref="BigInteger"/>
    /// </summary>
    /// <remarks>如果 <paramref name="maxValue"/> 等于 0, 则返回 0</remarks>
    /// <param name="random"></param>
    /// <param name="maxValue">指定返回值的上限</param>
    /// <returns>随机生成的 <see cref="BigInteger"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> 小于 0</exception>
    public static BigInteger NextBigInteger(this Random random, BigInteger maxValue)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue, nameof(maxValue));

        // 特例处理
        if (maxValue.IsZero || maxValue.IsOne)
        {
            return BigInteger.Zero;
        }

        // 获取无符号字节数
        var byteCount = maxValue.GetByteCount(true);

        // 额外一个 0 字节, 确保构造出的 BigInteger 为非负
        var tmp = new byte[byteCount + 1];

        // 最终的结果
        BigInteger result;

        // 随机生成, 直到落在范围内
        while (true)
        {
            random.NextBytes(tmp.AsSpan(0, byteCount));
            result = new(tmp);
            if (result < maxValue)
            {
                return result;
            }
        }
    }

    /// <summary>
    /// 返回一个范围在 [<paramref name="minValue"/>, <paramref name="maxValue"/>) 之间的随机 <see cref="BigInteger"/>
    /// </summary>
    /// <remarks>如果 <paramref name="minValue"/> 等于 <paramref name="maxValue"/>, 则返回 <paramref name="minValue"/></remarks>
    /// <param name="random"></param>
    /// <param name="minValue">指定返回值的下限</param>
    /// <param name="maxValue">指定返回值的上限</param>
    /// <returns>随机生成的 <see cref="BigInteger"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> 大于 <paramref name="maxValue"/></exception>
    public static BigInteger NextBigInteger(this Random random, BigInteger minValue, BigInteger maxValue)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue, nameof(minValue));

        // 特例处理
        if (minValue == maxValue)
        {
            return minValue;
        }

        // 计算范围
        var range = maxValue - minValue;

        // 获取随机值并偏移
        return random.NextBigInteger(range) + minValue;
    }

    /// <summary>
    /// 返回一个范围在 [0, 1) 之间的随机 <see cref="BigRational"/>
    /// </summary>
    /// <remarks><para>
    /// 生成的分母的最大位长为 <paramref name="maxDenominatorBits"/>
    /// <para></para>
    /// 由于采用的是按位长均匀分布的方式生成分母, 因此最终结果小位长的分母出现概率略大
    /// </para></remarks>
    /// <param name="random"></param>
    /// <param name="maxDenominatorBits">指定分母的最大位长</param>
    /// <returns>随机生成的 <c><see cref="BigRational"/></c></returns>
    public static BigRational NextBigRational(this Random random, int maxDenominatorBits = 32)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDenominatorBits, 1, nameof(maxDenominatorBits));

        // 随机选择分母的位长(1 到 maxDenominatorBits)
        var bits = random.Next(1, maxDenominatorBits + 1);

        // 分母范围 [2^(bits-1), 2^bits)
        var minDen = BigInteger.One << (bits - 1);
        var maxDen = BigInteger.One << bits;

        // 生成分母和分子
        var den = random.NextBigInteger(minDen, maxDen);
        var num = random.NextBigInteger(den);

        // 返回结果
        return new(num, den);
    }

    /// <summary>
    /// 返回一个范围在 [0, <paramref name="maxValue"/>) 之间的随机 <see cref="BigRational"/>
    /// </summary>
    /// <remarks><para>
    /// 生成的分母的最大位长为 <paramref name="maxDenominatorBits"/>
    /// <para></para>
    /// 由于采用的是按位长均匀分布的方式生成分母, 因此最终结果小位长的分母出现概率略大
    /// </para></remarks>
    /// <param name="random"></param>
    /// <param name="maxValue">指定返回值的上限</param>
    /// <param name="maxDenominatorBits">指定分母的最大位长</param>
    /// <returns>随机生成的 <c><see cref="BigRational"/></c></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> 小于 0</exception>
    public static BigRational NextBigRational(this Random random, BigRational maxValue, int maxDenominatorBits = 32)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue, nameof(maxValue));

        // 特例处理
        if (BigRational.IsZero(maxValue))
        {
            return BigRational.Zero;
        }

        // 获取随机值并缩放
        var r = random.NextBigRational(maxDenominatorBits);
        return r * maxValue;
    }

    /// <summary>
    /// 返回一个范围在 [<paramref name="minValue"/>, <paramref name="maxValue"/>) 之间的随机 <see cref="BigRational"/>
    /// </summary>
    /// <remarks><para>
    /// 生成的分母的最大位长为 <paramref name="maxDenominatorBits"/>
    /// </para><para>
    /// 由于采用的是按位长均匀分布的方式生成分母, 因此最终结果小位长的分母出现概率略大
    /// </para></remarks>
    /// <param name="random"></param>
    /// <param name="minValue">指定返回值的下限</param>
    /// <param name="maxValue">指定返回值的上限</param>
    /// <param name="maxDenominatorBits">指定分母的最大位长</param>
    /// <returns>随机生成的 <see cref="BigRational"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> 大于 <paramref name="maxValue"/></exception>
    public static BigRational NextBigRational(this Random random, BigRational minValue, BigRational maxValue, int maxDenominatorBits = 32)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue, nameof(minValue));

        // 特例处理
        if (minValue == maxValue)
        {
            return minValue;
        }

        // 计算范围
        var range = maxValue - minValue;

        // 获取随机值并偏移
        return random.NextBigRational(maxDenominatorBits) * range + minValue;
    }
}
