using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的 Parse 方法
public partial struct BigRational
{
    /// <summary>
    /// 默认数字样式
    /// </summary>
    public static NumberStyles DefaultNumberStyle => NumberStyles.Integer | NumberStyles.AllowThousands;

    /// <summary>
    /// 整数部分的默认数字样式
    /// </summary>
    public static NumberStyles IntegerDefaultNumberStyle => NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowThousands;

    /// <summary>
    /// 小数部分的默认数字样式
    /// </summary>
    public static NumberStyles FractionDefaultNumberStyle => NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="style">数字样式</param>
    /// <param name="provider">格式提供程序</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BigRational result)
    {
        // 初始化 result
        result = NaN;

        // 去除 s 的前后空白
        s = s.Trim();

        // 判断 s 是否为空
        if (s.IsEmpty)
        {
            return false;
        }

        // '/' 的位置
        var index1 = s.IndexOf('/');
        if (index1 != -1)
        {
            // '/' 的左右部分
            var left = s[..index1].Trim();
            var right = s[(index1 + 1)..].Trim();

            // 如果右侧还包含 '/', 则格式错误
            if (right.IndexOf('/') != -1)
            {
                return false;
            }

            // 递归解析分子和分母为大有理数, 然后相除
            if (TryParse(left, style, provider, out var numerator) && TryParse(right, style, provider, out var denominator))
            {
                result = numerator / denominator;
                return true;
            }

            // 解析失败
            return false;
        }

        // '.' 的位置
        var nfi = NumberFormatInfo.GetInstance(provider);
        var sep = nfi.NumberDecimalSeparator.AsSpan();
        var index2 = s.IndexOf(sep);
        if (index2 != -1)
        {
            // '.' 的左右部分
            var left = s[..index2].Trim();
            var right = s[(index2 + 1)..].Trim();

            // 提取字符串开头的符号
            var sign = 0;
            if (!left.IsEmpty)
            {
                var positiveSign = nfi.PositiveSign.AsSpan();
                var negativeSign = nfi.NegativeSign.AsSpan();

                if (left.StartsWith(positiveSign, StringComparison.Ordinal))
                {
                    sign = 1;
                    left = left[positiveSign.Length..].TrimStart();
                }
                else if (left.StartsWith(negativeSign, StringComparison.Ordinal))
                {
                    sign = -1;
                    left = left[negativeSign.Length..].TrimStart();
                }
            }

            // 如果小数点两边都为空, 则格式错误
            if (left.IsEmpty && right.IsEmpty)
            {
                return false;
            }

            // 如果右侧还包含 '.', 则格式错误
            if (right.IndexOf(sep) != -1)
            {
                return false;
            }

            // 如果左侧为空, 则视为0
            if (left.IsEmpty)
            {
                left = "0".AsSpan();
            }

            // 解析整数部分(由于已经去除符号, 因此这里不允许有符号且结果为非负)
            if (!BigInteger.TryParse(left, IntegerDefaultNumberStyle, provider, out var intPart))
            {
                return false;
            }

            // 如果右侧为空, 则直接返回整数部分
            if (right.IsEmpty)
            {
                result = sign < 0 ? new(-intPart) : new(intPart);
                return true;
            }

            // 如果小数部分包含循环节, 进行处理
            var leftParenIndex = right.IndexOf('(');
            if (leftParenIndex != -1)
            {
                // 有左括号但没有右括号, 格式错误
                var rightParenIndex = right.IndexOf(')');
                if (rightParenIndex == -1)
                {
                    return false;
                }

                // 右括号不在末尾, 格式错误
                if (rightParenIndex != right.Length - 1)
                {
                    return false;
                }

                // 提取非循环部分和循环部分
                var nonRepeatingPart = right[..leftParenIndex].Trim();
                var repeatingPart = right[(leftParenIndex + 1)..rightParenIndex].Trim();

                // 如果非循环部分为空, 则视为0
                var strToParse = nonRepeatingPart.IsEmpty ? "0".AsSpan() : nonRepeatingPart;

                // 解析非循环部分
                if (!BigInteger.TryParse(strToParse, FractionDefaultNumberStyle, provider, out var nonRepeating) || nonRepeating.Sign < 0)
                {
                    return false;
                }

                // 解析循环部分
                if (!BigInteger.TryParse(repeatingPart, FractionDefaultNumberStyle, provider, out var repeating) || repeating.Sign < 0)
                {
                    return false;
                }

                // 如果循环部分为0, 直接以非循环部分解析
                if (repeating.IsZero)
                {
                    // 如果非循环部分为0, 直接取整数部分
                    if (nonRepeating.IsZero)
                    {
                        result = new(intPart);
                    }
                    else
                    {
                        // 计算小数部分的分母
                        var fracPartDenominator = BigInteger.Pow(10, nonRepeatingPart.Length);

                        // 合并整数部分和小数部分
                        result = new(intPart * fracPartDenominator + nonRepeating, fracPartDenominator);
                    }
                }
                else
                {
                    // 计算 10^m 和 (10^n - 1)(m 为非循环部分长度, n 为循环部分长度)
                    var a = BigInteger.Pow(10, nonRepeatingPart.Length);
                    var b = BigInteger.Pow(10, repeatingPart.Length) - 1;

                    // 计算分母
                    var denominator = a * b;

                    // 计算分子
                    var numerator = intPart * denominator + nonRepeating * b + repeating;

                    // 构造结果
                    result = new(numerator, denominator);
                }

                // 恢复符号
                if (sign < 0)
                {
                    result = -result;
                }
                return true;
            }

            // 不存在循环节, 直接解析小数部分
            if (BigInteger.TryParse(right, FractionDefaultNumberStyle, provider, out var fracPart) && fracPart.Sign >= 0)
            {
                // 计算小数部分的分母
                var fracPartDenominator = BigInteger.Pow(10, right.Length);

                // 合并整数部分和小数部分
                result = new(intPart * fracPartDenominator + fracPart, fracPartDenominator);

                // 恢复符号
                if (sign < 0)
                {
                    result = -result;
                }
                return true;
            }

            // 解析失败
            return false;
        }

        // 只包含整数部分
        if (BigInteger.TryParse(s, style, provider, out var integer))
        {
            result = new(integer);
            return true;
        }

        // 解析失败
        return false;
    }

    /// <summary>
    /// 解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="style">数字样式</param>
    /// <param name="provider">格式提供程序</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">解析失败时抛出</exception>
    public static BigRational Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => TryParse(s, style, provider, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="provider">格式提供程序</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigRational result) => TryParse(s, DefaultNumberStyle, provider, out result);

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="provider">格式提供程序</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">解析失败时抛出</exception>
    public static BigRational Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => TryParse(s, DefaultNumberStyle, provider, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse(ReadOnlySpan<char> s, out BigRational result) => TryParse(s, DefaultNumberStyle, CultureInfo.CurrentCulture, out result);

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">解析失败时抛出</exception>
    public static BigRational Parse(ReadOnlySpan<char> s) => TryParse(s, DefaultNumberStyle, CultureInfo.CurrentCulture, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="style">数字样式</param>
    /// <param name="provider">格式提供程序</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BigRational result) => TryParse(s.AsSpan(), style, provider, out result);

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="style">数字样式</param>
    /// <param name="provider">格式提供程序</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">当 s 的格式不正确时抛出</exception>
    public static BigRational Parse(string s, NumberStyles style, IFormatProvider? provider) => TryParse(s.AsSpan(), style, provider, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="provider">格式提供程序</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigRational result) => TryParse(s.AsSpan(), DefaultNumberStyle, provider, out result);

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="provider">格式提供程序</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">当 s 的格式不正确时抛出</exception>
    public static BigRational Parse(string s, IFormatProvider? provider) => TryParse(s.AsSpan(), DefaultNumberStyle, provider, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则 <paramref name="result"/> 为 <see cref="NaN"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <param name="result">解析结果</param>
    /// <returns>如果解析成功则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool TryParse([NotNullWhen(true)] string? s, out BigRational result) => TryParse(s.AsSpan(), DefaultNumberStyle, CultureInfo.CurrentCulture, out result);

    /// <summary>
    /// 尝试解析字符串为大有理数
    /// </summary>
    /// <remarks><para>
    /// 支持的格式有:
    /// </para><list type="bullet">
    /// <item><description>整数</description></item>
    /// <item><description>小数 (允许小数点一侧为空)</description></item>
    /// <item><description>带循环节的小数, 循环节用括号括起, 如 0.1(6) 表示 0.1666...</description></item>
    /// <item><description>带'/'的分数</description></item>
    /// </list><para>
    /// 如果解析失败, 则抛出 <see cref="FormatException"/>
    /// </para></remarks>
    /// <param name="s">要解析的字符串</param>
    /// <returns>解析结果</returns>
    /// <exception cref="FormatException">当 s 的格式不正确时抛出</exception>
    public static BigRational Parse(string s) => TryParse(s.AsSpan(), DefaultNumberStyle, CultureInfo.CurrentCulture, out var result) ? result : throw new FormatException("无法解析字符串为大有理数");
}
