namespace OwnRationalLib;

// 大有理数结构体的基础属性
public partial struct BigRational
{
    /// <summary>
    /// 获取当前大有理数的类型
    /// </summary>
    public RationalKind Kind => Denominator.IsZero ? Numerator.IsZero ? RationalKind.NaN : Numerator.Sign > 0 ? RationalKind.PositiveInfinity : RationalKind.NegativeInfinity : RationalKind.Normal;

    /// <summary>
    /// 获取当前大有理数的符号
    /// </summary>
    /// <remarks>当前大有理数为负数时返回 -1, 为零时返回 0, 为正数时返回 1</remarks>
    public int Sign => Numerator.Sign;

    /// <summary>
    /// 当前大有理数的倒数
    /// </summary>
    /// <remarks><para>
    /// 如果当前大有理数为 0, 则返回无穷大; 如果当前大有理数为无穷大, 则返回零;
    /// 如果当前大有理数为 <see cref="NaN"/>, 则返回 <see cref="NaN"/>
    /// </para><para>
    /// 注意: 如果对负无穷大取倒数, 则返回 0, 会丢失符号信息
    /// </para></remarks>
    public BigRational Reciprocal => new(Denominator, Numerator);
}
