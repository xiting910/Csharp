using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OwnRationalLib;

// 大有理数结构体的基础方法
public partial struct BigRational
{
    /// <summary>
    /// 比较当前大有理数和指定的大有理数的大小
    /// </summary>
    /// <param name="other">要比较的另一个大有理数</param>
    /// <returns>如果当前大有理数小于指定的大有理数, 则返回小于 0 的值;
    /// 如果等于, 则返回 0; 如果大于, 则返回大于 0 的值</returns>
    /// <exception cref="InvalidOperationException">如果任一实例是 <see cref="NaN"/></exception>
    public int CompareTo(BigRational other)
    {
        // 缓存两个实例的类型
        var kind = Kind;
        var otherKind = other.Kind;

        // 如果任一实例是 NaN, 则抛出异常
        if (kind == RationalKind.NaN || otherKind == RationalKind.NaN)
        {
            throw new InvalidOperationException("无法比较 NaN 类型的大有理数");
        }

        // 如果两个实例类型不相同, 说明至少有一个是无穷大
        if (kind != otherKind)
        {
            // 如果 this 是正无穷大或 other 是负无穷大, 则 this 大于 other
            if (kind == RationalKind.PositiveInfinity || otherKind == RationalKind.NegativeInfinity)
            {
                return 1;
            }

            // 如果 this 是负无穷大或 other 是正无穷大, 则 this 小于 other
            if (kind == RationalKind.NegativeInfinity || otherKind == RationalKind.PositiveInfinity)
            {
                return -1;
            }
        }

        // 两个实例是类型相同的情况下, 如果不是 Normal, 直接认为它们相等
        if (kind != RationalKind.Normal)
        {
            return 0;
        }

        // 缓存两个实例的符号
        var sign = Sign;
        var otherSign = other.Sign;

        // 负数小于正数
        if (sign < 0 && otherSign > 0)
        {
            return -1;
        }

        // 正数大于负数
        if (sign > 0 && otherSign < 0)
        {
            return 1;
        }

        // 两个有理数同号的情况
        var left = Numerator * other.Denominator;
        var right = other.Numerator * Denominator;
        return left.CompareTo(right);
    }

    /// <summary>
    /// 比较当前实例和指定对象的大小
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果当前实例小于指定对象, 则返回小于 0 的值;
    /// 如果等于, 则返回 0; 如果大于, 则返回大于 0 的值</returns>
    /// <exception cref="ArgumentException">如果指定对象不是 <see cref="BigRational"/> 类型</exception>
    /// <exception cref="InvalidOperationException">如果任一实例是 <see cref="NaN"/></exception>
    public int CompareTo([NotNull] object? obj) => obj is BigRational other ? CompareTo(other) : throw new ArgumentException("要比较的对象不是 BigRational 类型", nameof(obj));

    /// <summary>
    /// 判断当前大有理数是否等于指定的大有理数
    /// </summary>
    /// <param name="other">要比较的另一个大有理数</param>
    /// <returns>如果当前大有理数等于指定的大有理数, 则返回 <see langword="true"/>; 否则返回 <see langword="false"/></returns>
    public bool Equals(BigRational other)
    {
        var kind = Kind;
        var otherKind = other.Kind;
        return kind != RationalKind.NaN && kind == otherKind && (kind != RationalKind.Normal || (Numerator == other.Numerator && Denominator == other.Denominator));
    }

    /// <summary>
    /// 判断当前实例是否等于指定对象
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果当前实例等于指定对象, 则返回 <see langword="true"/>; 否则返回 <see langword="false"/></returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BigRational other && Equals(other);

    /// <summary>
    /// 获取当前实例的哈希码
    /// </summary>
    /// <returns>当前实例的哈希码</returns>
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    /// <summary>
    /// 重写 <see cref="object.ToString"/> 方法, 返回分数形式的字符串表示
    /// </summary>
    /// <remarks>如果想得到小数形式的字符串, 请使用 <see cref="ToDecimalString"/> 方法</remarks>
    /// <returns>分数形式的字符串</returns>
    public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
}
