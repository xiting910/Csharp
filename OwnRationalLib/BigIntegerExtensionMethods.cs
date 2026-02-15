using System.Numerics;

namespace OwnRationalLib;

/// <summary>
/// 提供 <see cref="BigInteger"/> 类的扩展方法
/// </summary>
public static class BigIntegerExtensionMethods
{
    /// <summary>
    /// 判断一个大整数是否为平方数
    /// </summary>
    /// <param name="value">要判断的值</param>
    /// <returns>如果是平方数则返回 <see langword="true"/>; 否则返回 <see langword="false"/></returns>
    public static bool IsPerfectSquare(this BigInteger value)
    {
        // 负数不是平方数
        if (value.Sign < 0)
        {
            return false;
        }

        // 0 和 1 是平方数
        if (value.IsZero || value.IsOne)
        {
            return true;
        }

        // 计算平方根并验证
        var sqrt = value.Sqrt();
        return sqrt * sqrt == value;
    }

    /// <summary>
    /// 将大整数开平方, 向下取整
    /// </summary>
    /// <param name="value">要开平方的值</param>
    /// <returns>开平方后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当值为负数时抛出此异常</exception>
    public static BigInteger Sqrt(this BigInteger value)
    {
        // 负数不能开平方
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        // 0 和 1 的平方根是其本身
        if (value.IsZero || value.IsOne)
        {
            return value;
        }

        // 使用牛顿迭代法计算平方根
        var bitLength = value.GetBitLength();
        var x = BigInteger.One.ShiftLeft((bitLength + 1) >> 1);
        var y = (x + value / x) >> 1;
        while (y < x)
        {
            x = y;
            y = (value / x + x) >> 1;
        }
        return x;
    }

    /// <summary>
    /// 将大整数开 <paramref name="n"/> 次方, 向下取整
    /// </summary>
    /// <param name="value">要开 n 次方的值</param>
    /// <param name="n">要开方的次数</param>
    /// <returns>开 <paramref name="n"/> 次方后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 n 小于 2 时抛出此异常</exception>
    /// <exception cref="ArgumentException">当对负数开偶数次方时抛出此异常</exception>
    public static BigInteger NthRoot(this BigInteger value, int n)
    {
        // n 必须大于等于 2
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 2, nameof(n));

        // 如果 n 等于 2, 则调用平方根方法
        if (n == 2)
        {
            return value.Sqrt();
        }

        // 负数不能开偶数次方
        if (value.Sign < 0 && (n & 1) == 0)
        {
            throw new ArgumentException("不能对负数开偶数次方", nameof(value));
        }

        // 0 和 1 的 n 次方根是其本身, -1 的奇数次方根也是其本身
        if (value.IsZero || value.IsOne || value == -BigInteger.One)
        {
            return value;
        }

        // 使用牛顿迭代法计算 n 次方根
        var sign = value.Sign;
        var absValue = BigInteger.Abs(value);
        var bitLength = absValue.GetBitLength();
        var x = BigInteger.One.ShiftLeft((bitLength + n - 1) / n);
        var nMinus1 = n - 1;
        var y = (nMinus1 * x + absValue / BigInteger.Pow(x, nMinus1)) / n;
        while (y < x)
        {
            x = y;
            y = (nMinus1 * x + absValue / BigInteger.Pow(x, nMinus1)) / n;
        }
        return sign < 0 ? -x : x;
    }

    /// <summary>
    /// 将大整数左移指定的位数
    /// </summary>
    /// <param name="value">要左移的值</param>
    /// <param name="shift">要左移的位数</param>
    /// <returns>左移后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当位移值为负数时抛出此异常</exception>
    public static BigInteger ShiftLeft(this BigInteger value, long shift)
    {
        // 位移值不能为负数
        ArgumentOutOfRangeException.ThrowIfNegative(shift);

        // 位移 0 位或值为 0 时不变
        if (shift == 0 || value.IsZero)
        {
            return value;
        }

        // 分块左移, 避免直接将 long 转为 int 导致异常
        int s;
        while (shift > 0)
        {
            s = shift > int.MaxValue ? int.MaxValue : (int)shift;
            value <<= s;
            shift -= s;
        }
        return value;
    }

    /// <summary>
    /// 将大整数右移指定的位数
    /// </summary>
    /// <param name="value">要右移的值</param>
    /// <param name="shift">要右移的位数</param>
    /// <returns>右移后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当位移值为负数时抛出此异常</exception>
    public static BigInteger ShiftRight(this BigInteger value, long shift)
    {
        // 位移值不能为负数
        ArgumentOutOfRangeException.ThrowIfNegative(shift);

        // 位移 0 位或值为 0 时不变
        if (shift == 0 || value.IsZero)
        {
            return value;
        }

        // 如果右移位数大于等于值的位长, 则结果为 0
        var bValue = value.GetBitLength();
        if (shift >= bValue)
        {
            return BigInteger.Zero;
        }

        // 分块右移, 避免直接将 long 转为 int 导致异常
        int s;
        while (shift > 0)
        {
            s = shift > int.MaxValue ? int.MaxValue : (int)shift;
            value >>= s;
            shift -= s;
            if (value.IsZero)
            {
                return value;
            }
        }
        return value;
    }

    /// <summary>
    /// 获取大整数的十进制位长
    /// </summary>
    /// <param name="value">要获取位长的值</param>
    /// <returns>十进制位长</returns>
    public static int GetDecimalDigitCount(this BigInteger value)
    {
        // 0 的位长为 1
        if (value.IsZero)
        {
            return 1;
        }

        // 计算位长
        value = BigInteger.Abs(value);
        var log10 = BigInteger.Log10(value);
        var digits = (int)Math.Floor(log10) + 1;

        // 修正可能的浮点误差
        if (BigInteger.Pow(10, digits - 1) > value)
        {
            digits--;
        }
        else if (BigInteger.Pow(10, digits) <= value)
        {
            digits++;
        }

        // 返回位长
        return digits;
    }
}
