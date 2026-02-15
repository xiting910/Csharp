using System.Numerics;

namespace OwnRationalLib;

// 大有理数结构体的数学方法
public partial struct BigRational
{
    /// <summary>
    /// 尝试将大有理数分解为整数部分和小数部分
    /// </summary>
    /// <remarks><para>
    /// 该方法仅支持普通大有理数, 对于 <see cref="NaN"/>, 正无穷大, 负无穷大等均返回 <see langword="false"/>
    /// </para><para>
    /// 如果分解成功, 则满足: <see langword="this"/> = <paramref name="integerPart"/> + <paramref name="fractionalPart"/>
    /// 且 <paramref name="fractionalPart"/> 的绝对值小于 1, 否则 <paramref name="integerPart"/> 会被设置为 <c>0</c>,
    /// <paramref name="fractionalPart"/> 会被设置为 <see cref="NaN"/>
    /// </para><para>
    /// 注意: 当大有理数为负数时, 整数部分会向零取整, 例如 <c>-1.7</c> 会被分解为 <c>-1</c> 和 <c>-0.7</c>
    /// </para></remarks>
    /// <param name="integerPart">整数部分</param>
    /// <param name="fractionalPart">小数部分</param>
    /// <returns>如果分解成功则返回 <see langword="true"/>, 否则返回 <see langword="false"/></returns>
    public bool TryDecompose(out BigInteger integerPart, out BigRational fractionalPart)
    {
        // 初始化输出参数
        integerPart = BigInteger.Zero;
        fractionalPart = NaN;

        // 仅对普通大有理数进行分解
        if (Kind != RationalKind.Normal)
        {
            return false;
        }

        // 计算整数部分和小数部分
        integerPart = BigInteger.DivRem(Numerator, Denominator, out var fractionalNumerator);
        fractionalPart = new(fractionalNumerator, Denominator);
        return true;
    }

    /// <summary>
    /// 尝试计算大有理数的平方根(不丢失精度)
    /// </summary>
    /// <remarks>
    /// 该方法仅支持非负的普通大有理数, 对于 <see cref="NaN"/>, 正无穷大, 负无穷大, 负数等均返回 <see langword="false"/>;
    /// 当开方会导致精度丢失时也会返回 <see langword="false"/>;
    /// 返回 <see langword="false"/> 时, <paramref name="result"/> 会被设置为 <see cref="NaN"/>
    /// </remarks>
    /// <param name="result">计算结果</param>
    /// <returns>如果计算成功则返回 <see langword="true"/>, 否则返回 <see langword="false"/></returns>
    public bool TrySqrt(out BigRational result)
    {
        // 初始化结果为 NaN
        result = NaN;

        // 检查是否为普通且非负
        if (Kind != RationalKind.Normal || Numerator.Sign < 0)
        {
            return false;
        }

        // 处理0和1的特殊情况
        if (IsZero(this) || IsOne(this))
        {
            result = this;
            return true;
        }

        // 计算分子和分母的平方根
        var numSqrt = Numerator.Sqrt();
        var denSqrt = Denominator.Sqrt();

        // 检查分子和分母的平方是否等于原始值
        if (numSqrt * numSqrt == Numerator && denSqrt * denSqrt == Denominator)
        {
            result = new(numSqrt, denSqrt);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 计算满足精度要求的大有理数的平方根
    /// </summary>
    /// <remarks><para>
    /// 该方法仅支持非负的普通大有理数, 对于 <see cref="NaN"/>, 正无穷大, 负无穷大, 负数等均会抛出异常
    /// </para><para>
    /// 理论上该方法可以计算任意精度的平方根, 但实际上过高的精度会导致计算时间过长以及内存消耗过大
    /// </para><para>
    /// 注意: 当迭代次数达到最大迭代次数时, 可能无法满足所需的精度要求, 此时会返回当前的近似值
    /// </para></remarks>
    /// <param name="precision">所需的精度(小数点后的位数), 默认为 10</param>
    /// <param name="maxIterations">最大迭代次数, 默认为 128</param>
    /// <returns>计算结果</returns>
    /// <exception cref="InvalidOperationException">如果输入值不是非负的普通大有理数</exception>
    /// <exception cref="ArgumentOutOfRangeException">如果某一个参数超出范围</exception>
    public BigRational Sqrt(int precision = 10, int maxIterations = 128)
    {
        // 参数检查
        ArgumentOutOfRangeException.ThrowIfNegative(precision, nameof(precision));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxIterations, nameof(maxIterations));

        // 检查是否为普通且非负
        if (Kind != RationalKind.Normal || Numerator.Sign < 0)
        {
            throw new InvalidOperationException("只能对非负的普通大有理数进行开平方操作");
        }

        // 处理0和1的特殊情况
        if (IsZero(this) || IsOne(this))
        {
            return this;
        }

        // 计算分子和分母的平方根
        var numSqrt = Numerator.Sqrt();
        var denSqrt = Denominator.Sqrt();

        // 计算精度
        var scale = new BigRational(10).Pow(-precision);

        // 直接使用牛顿迭代法计算平方根
        var x = new BigRational(numSqrt, denSqrt);
        BigRational xNext;
        for (var i = 0; i < maxIterations; i++)
        {
            // 牛顿迭代: x_{n+1} = (x_n + a / x_n) / 2
            xNext = (x + this / x) / 2;

            // 检查是否达到所需精度
            if (Abs(xNext - x) < scale)
            {
                return xNext;
            }

            // 更新迭代值
            x = xNext;
        }

        // 达到最大迭代次数, 返回当前近似值
        return x;
    }

    /// <summary>
    /// 计算大有理数的 <paramref name="n"/> 次方
    /// </summary>
    /// <param name="n">指数</param>
    /// <returns>计算结果</returns>
    /// <exception cref="InvalidOperationException">如果输入值不是普通大有理数</exception>
    public BigRational Pow(int n)
    {
        // 检查输入值是否为普通大有理数
        if (Kind != RationalKind.Normal)
        {
            throw new InvalidOperationException("只能对普通大有理数进行幂运算");
        }

        // 底数为 0 时, 只有指数大于 0 时才有意义
        if (IsZero(this))
        {
            return n > 0 ? Zero : NaN;
        }

        // 指数为 0 时, 返回 1
        if (n == 0)
        {
            return One;
        }

        // 处理指数为 1 或底数为 1 的特殊情况
        if (n == 1 || IsOne(this))
        {
            return this;
        }

        // 指数为负时, 计算倒数的正指数幂
        if (n < 0)
        {
            return Reciprocal.Pow(-n);
        }

        // 使用指数平方算法计算幂
        var result = One;
        var baseValue = this;
        while (n > 0)
        {
            if ((n & 1) == 1)
            {
                result *= baseValue;
            }
            baseValue *= baseValue;
            n >>= 1;
        }
        return result;
    }

    /// <summary>
    /// 将大有理数向下取整为最接近的整数
    /// </summary>
    /// <returns>向下取整后的整数</returns>
    /// <exception cref="InvalidOperationException">如果输入值不是普通大有理数</exception>
    public BigInteger Floor() => Kind == RationalKind.Normal ? Numerator.Sign switch
    {
        < 0 => (Numerator - Denominator + 1) / Denominator,
        > 0 => Numerator / Denominator,
        _ => BigInteger.Zero,
    } : throw new InvalidOperationException("只能对普通大有理数进行取整操作");

    /// <summary>
    /// 将大有理数向上取整为最接近的整数
    /// </summary>
    /// <returns>向上取整后的整数</returns>
    /// <exception cref="InvalidOperationException">如果输入值不是普通大有理数</exception>
    public BigInteger Ceiling() => Kind == RationalKind.Normal ? Numerator.Sign switch
    {
        < 0 => Numerator / Denominator,
        > 0 => (Numerator + Denominator - 1) / Denominator,
        _ => BigInteger.Zero,
    } : throw new InvalidOperationException("只能对普通大有理数进行取整操作");

    /// <summary>
    /// 将大有理数取整为最接近的整数, 默认使用银行家取整
    /// </summary>
    /// <param name="mode">取整模式</param>
    /// <returns>四舍五入后的整数</returns>
    /// <exception cref="InvalidOperationException">如果输入值不是普通大有理数</exception>
    public BigInteger Round(MidpointRounding mode = MidpointRounding.ToEven)
    {
        // 如果已经是整数, 则直接返回
        if (Denominator.IsOne)
        {
            return Numerator / Denominator;
        }

        // 计算整数部分和小数部分
        var integerPart = Floor();
        var fractionalPart = this - integerPart;

        // 计算 1/2
        var half = new BigRational(1, 2);

        // 根据小数部分与 1/2 的比较结果和取整模式决定返回值
        return fractionalPart < half ? integerPart : fractionalPart > half ? integerPart + 1 : mode switch
        {
            MidpointRounding.ToEven => integerPart.IsEven ? integerPart : integerPart + 1,
            MidpointRounding.AwayFromZero => Numerator.Sign > 0 ? integerPart + 1 : integerPart,
            MidpointRounding.ToZero => Numerator.Sign > 0 ? integerPart : integerPart + 1,
            MidpointRounding.ToNegativeInfinity => integerPart,
            MidpointRounding.ToPositiveInfinity => integerPart + 1,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "不支持的取整模式")
        };
    }
}
