using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的转换相关部分
public partial struct BigRational
{
    /// <summary>
    /// 将其他数值类型转换为大有理数, 并且检查是否溢出, 如果溢出则转化失败
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <see cref="NaN"/></remarks>
    /// <typeparam name="TOther">要转化的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的大有理数</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertFromChecked<TOther>(TOther value, out BigRational result) where TOther : INumberBase<TOther>
    {
        // 默认值为 NaN
        result = NaN;

        // 直接转换回 BigRational
        if (value is BigRational br)
        {
            result = br;
            return true;
        }

        // 对于整数类型, 直接转换为 BigInteger 再构造
        if (value is BigInteger or int or long or uint or ulong or short or ushort or byte or sbyte)
        {
            result = new((BigInteger)(object)value);
            return true;
        }

        // 对于浮点类型, 检查是否为 NaN 或无穷大
        if (value is float f)
        {
            if (float.IsNaN(f) || float.IsInfinity(f))
            {
                return false;
            }

            result = f;
            return true;
        }

        // 对于双精度浮点数, 检查是否为 NaN 或无穷大
        if (value is double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return false;
            }

            result = d;
            return true;
        }

        // 对于 decimal, 直接转换
        if (value is decimal dec)
        {
            result = dec;
            return true;
        }

        // 其它类型无法转换
        return false;
    }

    /// <summary>
    /// 将其他数值类型转换为大有理数, 溢出时取极值(无穷)
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <see cref="NaN"/></remarks>
    /// <typeparam name="TOther">要转化的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的大有理数</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertFromSaturating<TOther>(TOther value, out BigRational result) where TOther : INumberBase<TOther>
    {
        // 先尝试 Checked 转换
        if (TryConvertFromChecked(value, out result))
        {
            return true;
        }

        // 对于浮点溢出, 取正负无穷
        if (value is float f)
        {
            if (float.IsPositiveInfinity(f))
            {
                result = PositiveInfinity;
                return true;
            }
            if (float.IsNegativeInfinity(f))
            {
                result = NegativeInfinity;
                return true;
            }
            if (float.IsNaN(f))
            {
                result = NaN;
                return true;
            }
        }
        else if (value is double d)
        {
            if (double.IsPositiveInfinity(d))
            {
                result = PositiveInfinity;
                return true;
            }
            if (double.IsNegativeInfinity(d))
            {
                result = NegativeInfinity;
                return true;
            }
            if (double.IsNaN(d))
            {
                result = NaN;
                return true;
            }
        }

        // 其它类型无法转换
        return false;
    }

    /// <summary>
    /// 将其他数值类型转换为大有理数, 遇到小数时截断
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <see cref="NaN"/></remarks>
    /// <typeparam name="TOther">要转化的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的大有理数</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertFromTruncating<TOther>(TOther value, out BigRational result) where TOther : INumberBase<TOther>
    {
        // 默认值为 NaN
        result = NaN;

        // 如果是 BigRational, 直接取整
        if (value is BigRational br)
        {
            result = br.Numerator / br.Denominator;
            return true;
        }

        // 对于 float, double, decimal 类型直接转换为整数部分
        if (value is float f)
        {
            if (float.IsNaN(f) || float.IsInfinity(f))
            {
                return false;
            }
            result = new((BigInteger)f);
            return true;
        }
        else if (value is double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return false;
            }
            result = new((BigInteger)d);
            return true;
        }
        else if (value is decimal dec)
        {
            result = new((BigInteger)dec);
            return true;
        }

        // 其它类型直接用 Checked
        return TryConvertFromChecked(value, out result);
    }

    /// <summary>
    /// 将大有理数转换为其他数值类型, 并且检查是否溢出, 如果溢出则转化失败
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <c>default</c></remarks>
    /// <typeparam name="TOther">要转化成的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的其他数值类型</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertToChecked<TOther>(BigRational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        // 默认值为 default
        result = default;

        // value 的类型
        var kind = value.Kind;

        try
        {
            // 直接转换回 BigRational
            if (typeof(TOther) == typeof(BigRational))
            {
                result = (TOther)(object)value;
                return true;
            }

            // 对于 float 类型判断是否为无穷或 NaN
            if (typeof(TOther) == typeof(float))
            {
                if (kind != RationalKind.Normal)
                {
                    return false;
                }

                var f = (float)((double)value.Numerator / (double)value.Denominator);
                if (float.IsInfinity(f) || float.IsNaN(f))
                {
                    return false;
                }

                result = (TOther)(object)f;
                return true;
            }

            // 对于 double 类型判断是否为无穷或 NaN
            if (typeof(TOther) == typeof(double))
            {
                if (kind != RationalKind.Normal)
                {
                    return false;
                }

                var d = (double)value.Numerator / (double)value.Denominator;
                if (double.IsInfinity(d) || double.IsNaN(d))
                {
                    return false;
                }

                result = (TOther)(object)d;
                return true;
            }

            // 对于 decimal 类型判断是否为无穷或 NaN
            if (typeof(TOther) == typeof(decimal))
            {
                if (kind != RationalKind.Normal)
                {
                    return false;
                }

                var dec = (decimal)value.Numerator / (decimal)value.Denominator;
                result = (TOther)(object)dec;
                return true;
            }

            // 剩下的类型要求是整数
            if (IsInteger(value))
            {
                if (typeof(TOther) == typeof(BigInteger))
                {
                    result = (TOther)(object)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(int))
                {
                    result = (TOther)(object)(int)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(long))
                {
                    result = (TOther)(object)(long)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(uint))
                {
                    result = (TOther)(object)(uint)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(ulong))
                {
                    result = (TOther)(object)(ulong)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(short))
                {
                    result = (TOther)(object)(short)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(ushort))
                {
                    result = (TOther)(object)(ushort)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(byte))
                {
                    result = (TOther)(object)(byte)value.Numerator;
                    return true;
                }
                if (typeof(TOther) == typeof(sbyte))
                {
                    result = (TOther)(object)(sbyte)value.Numerator;
                    return true;
                }
            }
        }
        catch
        {
            // 转化为整数时可能溢出, 返回 false
            return false;
        }
        return false;
    }

    /// <summary>
    /// 尝试将大有理数转换为其他数值类型, 遇到溢出时取极值(无穷)
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <c>default</c></remarks>
    /// <typeparam name="TOther">要转化成的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的其他数值类型</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertToSaturating<TOther>(BigRational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        // 先尝试 Checked 转换
        if (TryConvertToChecked(value, out result))
        {
            return true;
        }

        // 处理浮点溢出
        if (typeof(TOther) == typeof(float))
        {
            if (value.Kind == RationalKind.PositiveInfinity)
            {
                result = (TOther)(object)float.PositiveInfinity;
                return true;
            }
            if (value.Kind == RationalKind.NegativeInfinity)
            {
                result = (TOther)(object)float.NegativeInfinity;
                return true;
            }
            if (value.Kind == RationalKind.NaN)
            {
                result = (TOther)(object)float.NaN;
                return true;
            }
        }
        if (typeof(TOther) == typeof(double))
        {
            if (value.Kind == RationalKind.PositiveInfinity)
            {
                result = (TOther)(object)double.PositiveInfinity;
                return true;
            }
            if (value.Kind == RationalKind.NegativeInfinity)
            {
                result = (TOther)(object)double.NegativeInfinity;
                return true;
            }
            if (value.Kind == RationalKind.NaN)
            {
                result = (TOther)(object)double.NaN;
                return true;
            }
        }

        // 其它类型无法转换
        return false;
    }

    /// <summary>
    /// 尝试将大有理数转换为其他数值类型, 遇到小数时截断
    /// </summary>
    /// <remarks>转化失败时 <c>result</c> 为 <c>default</c></remarks>
    /// <typeparam name="TOther">要转化成的类型</typeparam>
    /// <param name="value">要转化的值</param>
    /// <param name="result">转化后的其他数值类型</param>
    /// <returns><see langword="true"/> 如果成功转化, 否则 <see langword="false"/></returns>
    public static bool TryConvertToTruncating<TOther>(BigRational value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        result = default;
        try
        {
            // 直接转换回 BigRational
            if (typeof(TOther) == typeof(BigRational))
            {
                result = (TOther)(object)value;
                return true;
            }
            if (typeof(TOther) == typeof(float))
            {
                if (value.Kind != RationalKind.Normal)
                {
                    return false;
                }

                var f = (float)((double)value.Numerator / (double)value.Denominator);
                if (float.IsInfinity(f) || float.IsNaN(f))
                {
                    return false;
                }

                result = (TOther)(object)f;
                return true;
            }
            if (typeof(TOther) == typeof(double))
            {
                if (value.Kind != RationalKind.Normal)
                {
                    return false;
                }

                var d = (double)value.Numerator / (double)value.Denominator;
                if (double.IsInfinity(d) || double.IsNaN(d))
                {
                    return false;
                }

                result = (TOther)(object)d;
                return true;
            }
            if (typeof(TOther) == typeof(decimal))
            {
                if (value.Kind != RationalKind.Normal)
                {
                    return false;
                }

                var dec = (decimal)value.Numerator / (decimal)value.Denominator;
                result = (TOther)(object)dec;
                return true;
            }

            // 剩下目标类型为整数, 遇到小数时截断
            if (value.Kind != RationalKind.Normal)
            {
                return false;
            }

            // BigInteger 的商
            var truncated = value.Numerator / value.Denominator;

            if (typeof(TOther) == typeof(BigInteger))
            {
                result = (TOther)(object)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(int))
            {
                result = (TOther)(object)(int)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(long))
            {
                result = (TOther)(object)(long)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(uint))
            {
                result = (TOther)(object)(uint)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(ulong))
            {
                result = (TOther)(object)(ulong)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(short))
            {
                result = (TOther)(object)(short)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(ushort))
            {
                result = (TOther)(object)(ushort)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(byte))
            {
                result = (TOther)(object)(byte)truncated;
                return true;
            }
            if (typeof(TOther) == typeof(sbyte))
            {
                result = (TOther)(object)(sbyte)truncated;
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
    }
}
