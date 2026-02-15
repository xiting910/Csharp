using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的 INumberBase 方法
public partial struct BigRational
{
    /// <summary>
    /// 对一个大有理数取绝对值
    /// </summary>
    /// <param name="value">要取绝对值的大有理数</param>
    /// <returns>取绝对值后的大有理数</returns>
    public static BigRational Abs(BigRational value)
    {
        var kind = value.Kind;
        return kind == RationalKind.NaN ? NaN : kind == RationalKind.Normal ? new(BigInteger.Abs(value.Numerator), value.Denominator) : PositiveInfinity;
    }

    /// <summary>
    /// 判断一个大有理数是否是规范形式
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>总是返回 <see langword="true"/> 因为大有理数一定是规范形式</returns>
    public static bool IsCanonical(BigRational value) => true;

    /// <summary>
    /// 判断一个大有理数是否是次正规数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>总是返回 <see langword="false"/> 因为大有理数没有次正规数的概念</returns>
    public static bool IsSubnormal(BigRational value) => false;

    /// <summary>
    /// 判断一个大有理数是否是复数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>总是返回 <see langword="false"/> 因为大有理数不可能是复数</returns>
    public static bool IsComplexNumber(BigRational value) => false;

    /// <summary>
    /// 判断一个大有理数是否是虚数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>总是返回 <see langword="false"/> 因为大有理数不可能是虚数</returns>
    public static bool IsImaginaryNumber(BigRational value) => false;

    /// <summary>
    /// 判断一个大有理数是否是实数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>总是返回 <see langword="true"/> 因为大有理数一定是实数</returns>
    public static bool IsRealNumber(BigRational value) => true;

    /// <summary>
    /// 判断一个大有理数是否是偶数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是偶数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsEvenInteger(BigRational value) => IsInteger(value) && value.Numerator % 2 == 0;

    /// <summary>
    /// 判断一个大有理数是否是奇数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是奇数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsOddInteger(BigRational value) => IsInteger(value) && value.Numerator % 2 != 0;

    /// <summary>
    /// 判断一个大有理数是否是整数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是整数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsInteger(BigRational value) => value.Kind == RationalKind.Normal && value.Denominator.IsOne;

    /// <summary>
    /// 判断一个大有理数是否是有限数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是有限数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsFinite(BigRational value) => value.Kind == RationalKind.Normal;

    /// <summary>
    /// 判断一个大有理数是否是无穷大
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是无穷大则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsInfinity(BigRational value) => value.Kind is RationalKind.PositiveInfinity or RationalKind.NegativeInfinity;

    /// <summary>
    /// 判断一个大有理数是否是无效数字
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是无效数字则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsNaN(BigRational value) => value.Kind == RationalKind.NaN;

    /// <summary>
    /// 判断一个大有理数是否是正常形式
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是正常形式则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsNormal(BigRational value) => value.Kind == RationalKind.Normal;

    /// <summary>
    /// 判断一个大有理数是否是负数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是负数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsNegative(BigRational value) => value.Kind != RationalKind.NaN && value.Sign < 0;

    /// <summary>
    /// 判断一个大有理数是否是负无穷大
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是负无穷大则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsNegativeInfinity(BigRational value) => value.Kind == RationalKind.NegativeInfinity;

    /// <summary>
    /// 判断一个大有理数是否是正数
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是正数则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsPositive(BigRational value) => value.Kind != RationalKind.NaN && value.Sign > 0;

    /// <summary>
    /// 判断一个大有理数是否是正无穷大
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是正无穷大则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsPositiveInfinity(BigRational value) => value.Kind == RationalKind.PositiveInfinity;

    /// <summary>
    /// 判断一个大有理数是否是 0
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是 0 则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsZero(BigRational value) => value.Kind == RationalKind.Normal && value.Numerator.IsZero;

    /// <summary>
    /// 判断一个大有理数是否是 1
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是 1 则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsOne(BigRational value) => value.Kind == RationalKind.Normal && value.Numerator == value.Denominator;

    /// <summary>
    /// 判断一个大有理数是否是 -1
    /// </summary>
    /// <param name="value">要判断的大有理数</param>
    /// <returns>如果是 -1 则为 <see langword="true"/>, 否则为 <see langword="false"/></returns>
    public static bool IsNegativeOne(BigRational value) => value.Kind == RationalKind.Normal && value.Numerator == -value.Denominator;

    /// <summary>
    /// 获取两个大有理数中绝对值较大的那个, 不支持 <see cref="NaN"/>
    /// </summary>
    /// <param name="x">要比较的大有理数</param>
    /// <param name="y">要比较的大有理数</param>
    /// <returns>返回绝对值较大的那个有理数</returns>
    /// <exception cref="InvalidOperationException">当任一参数为 <see cref="NaN"/> 时抛出</exception>
    public static BigRational MaxMagnitude(BigRational x, BigRational y) => x.Kind == RationalKind.NaN || y.Kind == RationalKind.NaN ? throw new InvalidOperationException("无法比较 NaN 类型的大有理数") : Abs(x) > Abs(y) ? x : y;

    /// <summary>
    /// 获取两个大有理数中绝对值较大的那个, 支持 <see cref="NaN"/>
    /// </summary>
    /// <param name="x">要比较的大有理数</param>
    /// <param name="y">要比较的大有理数</param>
    /// <returns>返回绝对值较大的那个有理数</returns>
    public static BigRational MaxMagnitudeNumber(BigRational x, BigRational y) => x.Kind == RationalKind.NaN ? y : y.Kind == RationalKind.NaN ? x : Abs(x) > Abs(y) ? x : y;

    /// <summary>
    /// 获取两个大有理数中绝对值较小的那个, 不支持 <see cref="NaN"/>
    /// </summary>
    /// <param name="x">要比较的大有理数</param>
    /// <param name="y">要比较的大有理数</param>
    /// <returns>返回绝对值较小的那个有理数</returns>
    /// <exception cref="InvalidOperationException">当任一参数为 <see cref="NaN"/> 时抛出</exception>
    public static BigRational MinMagnitude(BigRational x, BigRational y) => x.Kind == RationalKind.NaN || y.Kind == RationalKind.NaN ? throw new InvalidOperationException("无法比较 NaN 类型的大有理数") : Abs(x) > Abs(y) ? y : x;

    /// <summary>
    /// 获取两个小有理数中绝对值较小的那个, 支持 <see cref="NaN"/>
    /// </summary>
    /// <param name="x">要比较的有理数</param>
    /// <param name="y">要比较的有理数</param>
    /// <returns>返回绝对值较小的那个有理数</returns>
    public static BigRational MinMagnitudeNumber(BigRational x, BigRational y) => x.Kind == RationalKind.NaN ? y : y.Kind == RationalKind.NaN ? x : Abs(x) > Abs(y) ? y : x;
}
