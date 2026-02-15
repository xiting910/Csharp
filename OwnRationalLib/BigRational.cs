using System.Numerics;

namespace OwnRationalLib;

/// <summary>
/// 表示一个任意精度的大有理数
/// </summary>
/// <remarks><c>default</c>值是 0/0 (<see cref="NaN"/>)</remarks>
public readonly partial struct BigRational : IComparable, IComparable<BigRational>, INumber<BigRational>, ISignedNumber<BigRational>, INumberBase<BigRational>, IParsable<BigRational>, IEquatable<BigRational>, IComparisonOperators<BigRational, BigRational, bool>, IEqualityOperators<BigRational, BigRational, bool>, IUnaryNegationOperators<BigRational, BigRational>, IUnaryPlusOperators<BigRational, BigRational>, IAdditionOperators<BigRational, BigRational, BigRational>, ISubtractionOperators<BigRational, BigRational, BigRational>, IMultiplyOperators<BigRational, BigRational, BigRational>, IDivisionOperators<BigRational, BigRational, BigRational>, IModulusOperators<BigRational, BigRational, BigRational>
{
    /// <summary>
    /// 当前大有理数的分子
    /// </summary>
    public BigInteger Numerator { get; }

    /// <summary>
    /// 当前大有理数的分母, 非负整数
    /// </summary>
    public BigInteger Denominator { get; }

    /// <summary>
    /// 默认构造函数, 初始化为 0/1
    /// </summary>
    public BigRational()
    {
        Numerator = BigInteger.Zero;
        Denominator = BigInteger.One;
    }

    /// <summary>
    /// 构造函数, 初始化一个大有理数(整数)
    /// </summary>
    /// <param name="numerator">分子</param>
    public BigRational(BigInteger numerator)
    {
        Numerator = numerator;
        Denominator = BigInteger.One;
    }

    /// <summary>
    /// 构造函数, 初始化一个大有理数, 自动约分化简
    /// </summary>
    /// <remarks>此构造函数允许分母为 <c>0</c>, 此时大有理数为无穷大或 <see cref="NaN"/></remarks>
    /// <param name="numerator">分子</param>
    /// <param name="denominator">分母</param>
    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        // 如果分母为 0, 则根据分子符号设置类型
        if (denominator.IsZero)
        {
            Numerator = numerator.Sign switch
            {
                -1 => -BigInteger.One,
                1 => BigInteger.One,
                0 => BigInteger.Zero,
                _ => BigInteger.Zero
            };
            Denominator = BigInteger.Zero;
            return;
        }

        // 如果分子为 0, 则直接设置为 0/1
        if (numerator.IsZero)
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
            return;
        }

        // 如果分母为负数, 则将分子分母都取反
        if (denominator.Sign < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        // 约分化简
        var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    /// <summary>
    /// 根据有理数类型构造大有理数, 只支持非普通类型的构造
    /// </summary>
    /// <param name="kind">有理数类型</param>
    /// <exception cref="ArgumentException">当 kind 为 Normal 时抛出此异常</exception>
    /// <exception cref="ArgumentOutOfRangeException">当 kind 不在枚举定义范围内时抛出此异常</exception>
    public BigRational(RationalKind kind)
    {
        switch (kind)
        {
            case RationalKind.PositiveInfinity:
                Numerator = BigInteger.One;
                Denominator = BigInteger.Zero;
                break;
            case RationalKind.NegativeInfinity:
                Numerator = -BigInteger.One;
                Denominator = BigInteger.Zero;
                break;
            case RationalKind.NaN:
                Numerator = BigInteger.Zero;
                Denominator = BigInteger.Zero;
                break;
            case RationalKind.Normal:
                throw new ArgumentException("普通类型的大有理数不支持通过类型构造", nameof(kind));
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "未知的有理数类型");
        }
    }
}
