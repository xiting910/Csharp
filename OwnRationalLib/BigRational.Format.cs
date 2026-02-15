using System.Globalization;
using System.Numerics;
using System.Text;

namespace OwnRationalLib;

// 大有理数结构体的格式化方法
public partial struct BigRational
{
    /// <summary>
    /// 将大有理数格式化为分数字符串
    /// </summary>
    /// <remarks>如果想得到小数形式的字符串, 请使用 <see cref="ToDecimalString"/> 方法</remarks>
    /// <param name="format">提供格式字符串</param>
    /// <param name="formatProvider">提供格式化程序</param>
    /// <returns>格式化后的字符串</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        // 获取格式化信息
        var nfi = NumberFormatInfo.GetInstance(formatProvider);

        // 处理特殊类型
        switch (Kind)
        {
            case RationalKind.NaN:
                return nfi.NaNSymbol;
            case RationalKind.PositiveInfinity:
                return nfi.PositiveInfinitySymbol;
            case RationalKind.NegativeInfinity:
                return nfi.NegativeInfinitySymbol;
            case RationalKind.Normal:
                break;
            default:
                return nfi.NaNSymbol;
        }

        // 格式化分子
        var numeratorStr = Numerator.ToString(format, formatProvider);

        // 如果分母为1, 直接返回分子
        if (Denominator == BigInteger.One)
        {
            return numeratorStr;
        }

        // 格式化分母
        var denominatorStr = Denominator.ToString(format, formatProvider);

        // 返回"分子/分母"格式
        return $"{numeratorStr}/{denominatorStr}";
    }

    /// <summary>
    /// 尝试格式化大有理数为字符串
    /// </summary>
    /// <param name="destination">目标字符范围</param>
    /// <param name="charsWritten">实际写入的字符数</param>
    /// <param name="format">格式字符串</param>
    /// <param name="provider">格式提供程序</param>
    /// <returns>如果格式化成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        // 初始化输出字符数
        charsWritten = 0;

        // 获取格式化信息
        var nfi = NumberFormatInfo.GetInstance(provider);

        // 处理特殊类型
        switch (Kind)
        {
            case RationalKind.NaN:
                if (nfi.NaNSymbol.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = nfi.NaNSymbol.Length;
                    return true;
                }
                return false;
            case RationalKind.PositiveInfinity:
                if (nfi.PositiveInfinitySymbol.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = nfi.PositiveInfinitySymbol.Length;
                    return true;
                }
                return false;
            case RationalKind.NegativeInfinity:
                if (nfi.NegativeInfinitySymbol.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = nfi.NegativeInfinitySymbol.Length;
                    return true;
                }
                return false;
            case RationalKind.Normal:
                break;
            default:
                if (nfi.NaNSymbol.AsSpan().TryCopyTo(destination))
                {
                    charsWritten = nfi.NaNSymbol.Length;
                    return true;
                }
                return false;
        }

        // 格式化分子
        _ = Numerator.TryFormat([], out var numLen, format, provider);
        var numBuffer = numLen <= 128 ? stackalloc char[128] : new char[numLen];
        if (!Numerator.TryFormat(numBuffer, out numLen, format, provider))
        {
            return false;
        }

        // 格式化分母
        if (Denominator == BigInteger.One)
        {
            if (numLen > destination.Length)
            {
                return false;
            }
            numBuffer[..numLen].CopyTo(destination);
            charsWritten = numLen;
            return true;
        }
        else
        {
            _ = Denominator.TryFormat([], out var denLen, format, provider);
            var denBuffer = denLen <= 128 ? stackalloc char[128] : new char[denLen];
            if (!Denominator.TryFormat(denBuffer, out denLen, format, provider))
            {
                return false;
            }

            var totalLen = numLen + 1 + denLen;
            if (totalLen > destination.Length)
            {
                return false;
            }

            numBuffer[..numLen].CopyTo(destination);
            destination[numLen] = '/';
            denBuffer[..denLen].CopyTo(destination[(numLen + 1)..]);
            charsWritten = totalLen;
            return true;
        }
    }

    /// <summary>
    /// 将一个真分数格式化为字符串, 支持循环小数的识别和表示
    /// </summary>
    /// <remarks><para>
    /// 此方法要求 <paramref name="numerator"/>, <paramref name="denominator"/> 均为正整数, 
    /// 且 <paramref name="numerator"/> 小于 <paramref name="denominator"/>
    /// </para><para>
    /// 此方法只会生成小数部分的字符串, 不包含整数部分和小数点
    /// </para><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 小于 0, 则表示不限制小数位数, 直到余数为 0 或者检测到循环节
    /// </para><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 为 0, 则直接返回空字符串
    /// </para><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 大于 0, 则表示小数部分的最大位数,
    /// 当达到该位数时即使余数不为零也会停止计算, 这可能会导致无法识别循环节和精度丢失
    /// </para><para>
    /// 如果识别到循环小数, 则循环节会被括号括起来, 例如 1/3 会被格式化为 (3)
    /// </para></remarks>
    /// <param name="numerator">真分数的分子, 必须为正整数且小于分母</param>
    /// <param name="denominator">真分数的分母, 必须为正整数</param>
    /// <param name="maxDecimalDigits">小数部分的最大位数, 如果为小于 0 则表示不限制小数位数</param>
    /// <returns>格式化后的字符串</returns>
    private static string FormatProperFraction(BigInteger numerator, BigInteger denominator, int maxDecimalDigits)
    {
        // 如果最大位数为 0, 则直接返回空字符串
        if (maxDecimalDigits == 0)
        {
            return string.Empty;
        }

        // 用于存储小数部分的字符串
        var sb = new StringBuilder();

        // 记录每个余数第一次出现的位置的字典, 用于检测循环节
        var remainderPositions = new Dictionary<BigInteger, int>();

        // 记录每一位数字
        int digit;

        // 迭代计算小数部分
        while (!numerator.IsZero && (maxDecimalDigits < 0 || sb.Length < maxDecimalDigits))
        {
            // 添加当前余数位置
            remainderPositions[numerator] = sb.Length;

            // 余数乘 10, 生成下一位小数
            numerator *= 10;

            // digit 应为 0..9, 可以安全转换为 int
            digit = (int)BigInteger.DivRem(numerator, denominator, out numerator);

            // 转为字符
            _ = sb.Append((char)('0' + digit));

            // 如果出现循环节
            if (remainderPositions.TryGetValue(numerator, out var startIndex))
            {
                return sb.Insert(startIndex, '(').Append(')').ToString();
            }
        }

        // 计算结束, 去除后导零
        while (sb.Length > 0 && sb[^1] == '0')
        {
            sb.Length--;
        }

        // 返回结果
        return sb.ToString();
    }

    /// <summary>
    /// 将大有理数转换为十进制小数且小数部分位数不超过指定小数位数的字符串
    /// </summary>
    /// <remarks><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 为负数, 表示不限制小数位数, 此时会强制保留循环节, 
    /// 且显示不会有精度丢失, 但是在转换高精度大有理数时会大量消耗内存和计算时间, 请谨慎使用
    /// </para><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 为零, 则表示不显示小数部分, 直接返回整数部分
    /// </para><para>
    /// 如果 <paramref name="maxDecimalDigits"/> 为正数, 则表示小数部分的最大位数, 
    /// 当达到该位数时即使余数不为零也会停止计算, 这可能会导致无法识别循环节和精度丢失; 此时可以通过 
    /// <paramref name="keepRepeatingPart"/> 参数来控制是否保留循环节显示, 如果 
    /// <paramref name="keepRepeatingPart"/> 为 <see langword="false"/>, 即使存在循环节也会被截断
    /// </para></remarks>
    /// <param name="maxDecimalDigits">最多显示的小数位数, 负数表示不限制</param>
    /// <param name="keepRepeatingPart">是否保留循环节显示</param>
    /// <param name="formatProvider">提供格式化信息的对象</param>
    /// <returns>转换后的字符串</returns>
    public string ToDecimalString(int maxDecimalDigits = 32, bool keepRepeatingPart = true, IFormatProvider? formatProvider = null)
    {
        // 获取格式化信息
        var nfi = NumberFormatInfo.GetInstance(formatProvider);

        // 处理特殊类型
        switch (Kind)
        {
            case RationalKind.NaN:
                return nfi.NaNSymbol;
            case RationalKind.PositiveInfinity:
                return nfi.PositiveInfinitySymbol;
            case RationalKind.NegativeInfinity:
                return nfi.NegativeInfinitySymbol;
            case RationalKind.Normal:
                break;
            default:
                return nfi.NaNSymbol;
        }

        // 用于构造最终结果的字符串
        var resultBuilder = new StringBuilder();

        // 先提取符号
        _ = resultBuilder.Append(Numerator.Sign < 0 ? nfi.NegativeSign : string.Empty);

        // 对分子取绝对值
        var absNumerator = BigInteger.Abs(Numerator);

        // 分离整数部分和小数部分
        var integerPart = BigInteger.DivRem(absNumerator, Denominator, out var fractionalNumerator);

        // 添加整数部分
        _ = resultBuilder.Append(integerPart.ToString(formatProvider));

        // 如果没有小数部分或者小数位数要求为0, 则直接返回
        if (fractionalNumerator.IsZero || maxDecimalDigits == 0)
        {
            return resultBuilder.ToString();
        }

        // 添加小数点
        _ = resultBuilder.Append(nfi.NumberDecimalSeparator);

        // 格式化小数部分
        var fractionalSpan = FormatProperFraction(fractionalNumerator, Denominator, maxDecimalDigits).AsSpan();

        // 如果小数位数无限制或者要求保留循环节, 则直接返回结果
        if (maxDecimalDigits < 0 || keepRepeatingPart)
        {
            return resultBuilder.Append(fractionalSpan).ToString();
        }

        // 判断小数部分是否存在循环节
        var leftParenIndex = fractionalSpan.IndexOf('(');

        // 如果没有循环节, 则直接返回
        if (leftParenIndex == -1)
        {
            return resultBuilder.Append(fractionalSpan).ToString();
        }

        // 有循环节, 提取非循环部分和循环部分
        var nonRepeatingPart = fractionalSpan[..leftParenIndex];
        var repeatingPart = fractionalSpan[(leftParenIndex + 1)..^1];

        // 非循环部分不够长, 需要添加循环部分
        var remainingDigits = maxDecimalDigits - nonRepeatingPart.Length;
        var (fullRepeats, partialRepeatLength) = int.DivRem(remainingDigits, repeatingPart.Length);

        // 添加非循环部分
        _ = resultBuilder.Append(nonRepeatingPart);

        // 添加完整的循环节
        for (var i = 0; i < fullRepeats; i++)
        {
            _ = resultBuilder.Append(repeatingPart);
        }

        // 添加部分循环节
        if (partialRepeatLength > 0)
        {
            var needAddPart = repeatingPart[..partialRepeatLength];
            _ = resultBuilder.Append(needAddPart);
        }

        // 返回结果
        return resultBuilder.ToString();
    }
}
