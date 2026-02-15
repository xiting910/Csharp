using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的 INumberBase 属性
public partial struct BigRational
{
    /// <summary>
    /// 进制
    /// </summary>
    public static int Radix => 10;

    /// <summary>
    /// 代表数字 0
    /// </summary>
    public static BigRational Zero => new();

    /// <summary>
    /// 代表数字 1
    /// </summary>
    public static BigRational One => new(BigInteger.One);

    /// <summary>
    /// 代表数字 -1
    /// </summary>
    public static BigRational NegativeOne => new(-BigInteger.One);

    /// <summary>
    /// 代表加法单位元
    /// </summary>
    public static BigRational AdditiveIdentity => new();

    /// <summary>
    /// 代表乘法单位元
    /// </summary>
    public static BigRational MultiplicativeIdentity => new(BigInteger.One);

    /// <summary>
    /// 正无穷大
    /// </summary>
    public static BigRational PositiveInfinity => new(RationalKind.PositiveInfinity);

    /// <summary>
    /// 负无穷大
    /// </summary>
    public static BigRational NegativeInfinity => new(RationalKind.NegativeInfinity);

    /// <summary>
    /// 表示一个非数字值
    /// </summary>
    public static BigRational NaN => new(RationalKind.NaN);
}
