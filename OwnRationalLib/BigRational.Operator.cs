using System.Numerics;

namespace OwnRationalLib;

// BigRational 的运算符重载部分
public partial struct BigRational
{
    /// <summary>
    /// 将大有理数显式转换为 <see cref="double"/>
    /// </summary>
    /// <remarks><para>
    /// 转化结果可能会出现 <see cref="double.NaN"/>, <see cref="double.PositiveInfinity"/>
    /// 或 <see cref="double.NegativeInfinity"/>
    /// </para><para>
    /// 由于 <see cref="double"/> 的精度限制, 转换结果可能会丢失精度
    /// </para></remarks>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的 <see cref="double"/></returns>
    public static explicit operator double(BigRational value)
    {
        // 处理特殊值
        var kind = value.Kind;
        if (kind == RationalKind.NaN)
        {
            return double.NaN;
        }
        else if (kind == RationalKind.PositiveInfinity)
        {
            return double.PositiveInfinity;
        }
        else if (kind == RationalKind.NegativeInfinity)
        {
            return double.NegativeInfinity;
        }

        // 如果是零则直接返回 0d
        if (value.Numerator.IsZero)
        {
            return 0d;
        }

        // 检查是否超出 double 范围
        if (value > double.MaxValue)
        {
            return double.PositiveInfinity;
        }
        else if (value < double.MinValue)
        {
            return double.NegativeInfinity;
        }

        // 如果是整数则直接转换
        if (value.Denominator.IsOne)
        {
            return (double)value.Numerator;
        }

        // 对于非整数, 将分子和分母都缩放到 double 能接受的范围内
        var num = BigInteger.Abs(value.Numerator);
        var den = value.Denominator;

        // 计算位长并选择右移位数, 使两者规模都落在 double 能接受的范围内
        const int doubleMaxBits = 1023;
        var bNum = num.GetBitLength();
        var bDen = den.GetBitLength();
        var shift = Math.Max(0, Math.Max(bNum, bDen) - doubleMaxBits);

        // 确保右移不会把分母移成 0
        if (bDen <= shift)
        {
            shift = Math.Max(0, bDen - 1);
        }

        // 进行右移并转换为 double 后计算结果
        var result = (double)num.ShiftRight(shift) / (double)den.ShiftRight(shift);
        return value.Sign < 0 ? -result : result;
    }

    /// <summary>
    /// 将大有理数显式转换为 <see cref="decimal"/>
    /// </summary>
    /// <remarks>
    /// <see cref="decimal"/> 的精度有限, 只能表示小数点后最多 28 位的数字
    /// </remarks>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的 <see cref="decimal"/></returns>
    /// <exception cref="InvalidOperationException">当大有理数不是普通类型时抛出此异常</exception>
    /// <exception cref="OverflowException">当转换结果超出 <see cref="decimal"/> 范围时抛出此异常</exception>
    public static explicit operator decimal(BigRational value)
    {
        // 只能转换普通类型的大有理数
        if (value.Kind != RationalKind.Normal)
        {
            throw new InvalidOperationException("只有普通类型的大有理数才能转换为 decimal");
        }

        // 如果是零则直接返回 0m
        if (value.Numerator.IsZero)
        {
            return 0m;
        }

        // 检查是否超出 decimal 范围
        if (value > decimal.MaxValue || value < decimal.MinValue)
        {
            throw new OverflowException("转换结果超出 decimal 范围");
        }

        // 如果是整数则直接转换
        if (value.Denominator.IsOne)
        {
            return (decimal)value.Numerator;
        }

        // 对于非整数, 将分子和分母都缩放到 decimal 能接受的范围内
        var num = BigInteger.Abs(value.Numerator);
        var den = value.Denominator;

        // 计算位长并选择右移位数, 使两者规模都落在 decimal 能接受的范围内
        const int decimalMaxBits = 96;
        var bNum = num.GetBitLength();
        var bDen = den.GetBitLength();
        var shift = Math.Max(0, Math.Max(bNum, bDen) - decimalMaxBits);

        // 确保右移不会把分母移成 0
        if (bDen <= shift)
        {
            shift = Math.Max(0, bDen - 1);
        }

        // 进行右移并转换为 decimal 后计算结果
        var result = (decimal)num.ShiftRight(shift) / (decimal)den.ShiftRight(shift);
        return value.Sign < 0 ? -result : result;
    }

    /// <summary>
    /// 将大有理数显式转换为 <see cref="BigInteger"/>, 直接舍弃小数部分
    /// </summary>
    /// <param name="value">要转换的值</param>
    /// <returns>转换后的 <see cref="BigInteger"/></returns>
    /// <exception cref="InvalidOperationException">当大有理数不是普通类型时抛出此异常</exception>
    public static explicit operator BigInteger(BigRational value) => value.Kind == RationalKind.Normal ? value.Numerator / value.Denominator : throw new InvalidOperationException("只有普通类型的大有理数才能转换为大整数");

    /// <summary>
    /// 取正运算符
    /// </summary>
    /// <param name="value">要取正的值</param>
    /// <returns>取正后的大有理数</returns>
    public static BigRational operator +(BigRational value) => value;

    /// <summary>
    /// 取反运算符
    /// </summary>
    /// <param name="value">要取反的值</param>
    /// <returns>取反后的大有理数</returns>
    public static BigRational operator -(BigRational value) => value.Kind switch
    {
        RationalKind.NaN => NaN,
        RationalKind.PositiveInfinity => NegativeInfinity,
        RationalKind.NegativeInfinity => PositiveInfinity,
        RationalKind.Normal => new(-value.Numerator, value.Denominator),
        _ => NaN
    };

    /// <summary>
    /// 自增运算符
    /// </summary>
    /// <param name="value">要自增的值</param>
    /// <returns>自增后的大有理数</returns>
    public static BigRational operator ++(BigRational value) => value + One;

    /// <summary>
    /// 自减运算符
    /// </summary>
    /// <param name="value">要自减的值</param>
    /// <returns>自减后的大有理数</returns>
    public static BigRational operator --(BigRational value) => value + NegativeOne;

    /// <summary>
    /// 加法运算符
    /// </summary>
    /// <remarks>
    /// 对于无穷大和 <see cref="NaN"/> 的运算遵循 <c>IEEE 754</c> 标准:
    /// <list type="bullet">
    /// <item>任何数与 <see cref="NaN"/> 相加结果为 <see cref="NaN"/></item>
    /// <item>正无穷大与正无穷大相加结果为正无穷大</item>
    /// <item>负无穷大与负无穷大相加结果为负无穷大</item>
    /// <item>正无穷大与负无穷大相加结果为 <see cref="NaN"/></item>
    /// <item>正无穷大与任何有限数相加结果为正无穷大</item>
    /// <item>负无穷大与任何有限数相加结果为负无穷大</item>
    /// </list>
    /// </remarks>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>加法后的大有理数</returns>
    public static BigRational operator +(BigRational left, BigRational right)
    {
        // 缓存左右操作数的类型
        var leftKind = left.Kind;
        var rightKind = right.Kind;

        // 如果任一操作数是 NaN, 则结果是 NaN
        if (leftKind == RationalKind.NaN || rightKind == RationalKind.NaN)
        {
            return NaN;
        }

        // 如果两个操作数类型不同, 说明其中一个是无穷大, 则结果是无穷大或 NaN
        if (leftKind != rightKind)
        {
            return leftKind == RationalKind.Normal ? right : rightKind == RationalKind.Normal ? left : NaN;
        }

        // 如果两个操作数都是无穷大, 则结果是无穷大
        if (leftKind != RationalKind.Normal)
        {
            return left;
        }

        // 对于普通类型, 计算通分后的分子和分母
        var gcd = BigInteger.GreatestCommonDivisor(left.Denominator, right.Denominator);
        var lDen = left.Denominator / gcd;
        var rDen = right.Denominator / gcd;
        var den = left.Denominator * rDen;
        var num = left.Numerator * rDen + right.Numerator * lDen;
        return new(num, den);
    }

    /// <summary>
    /// 减法运算符
    /// </summary>
    /// <remarks>
    /// 减法的实现是通过加法和取反来完成的
    /// </remarks>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>减法后的大有理数</returns>
    public static BigRational operator -(BigRational left, BigRational right) => left + -right;

    /// <summary>
    /// 乘法运算符
    /// </summary>
    /// <remarks>
    /// 对于无穷大和 <see cref="NaN"/> 的运算遵循 <c>IEEE 754</c> 标准:
    /// <list type="bullet">
    /// <item>任何数与 <see cref="NaN"/> 相乘结果为 <see cref="NaN"/></item>
    /// <item>正无穷大与正无穷大相乘结果为正无穷大</item>
    /// <item>负无穷大与负无穷大相乘结果为正无穷大</item>
    /// <item>正无穷大与负无穷大相乘结果为负无穷大</item>
    /// <item>正无穷大与任何正有限数相乘结果为正无穷大</item>
    /// <item>正无穷大与任何负有限数相乘结果为负无穷大</item>
    /// <item>负无穷大与任何正有限数相乘结果为负无穷大</item>
    /// <item>负无穷大与任何负有限数相乘结果为正无穷大</item>
    /// <item>零与任何无穷大相乘结果为 <see cref="NaN"/></item>
    /// </list>
    /// </remarks>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>乘法后的大有理数</returns>
    public static BigRational operator *(BigRational left, BigRational right)
    {
        // 缓存左右操作数的类型
        var leftKind = left.Kind;
        var rightKind = right.Kind;

        // 如果任一操作数是 NaN, 则结果是 NaN
        if (leftKind == RationalKind.NaN || rightKind == RationalKind.NaN)
        {
            return NaN;
        }

        // 如果两个操作数类型不同, 说明其中一个是无穷大, 则结果是无穷大或 NaN
        if (leftKind != rightKind)
        {
            return leftKind == RationalKind.Normal ? left.Numerator.IsZero ? NaN : left.Sign < 0 ? -right : right : rightKind == RationalKind.Normal ? right.Numerator.IsZero ? NaN : right.Sign < 0 ? -left : left : NegativeInfinity;
        }

        // 如果两个操作数都是无穷大, 则结果是无穷大
        if (leftKind != RationalKind.Normal)
        {
            return PositiveInfinity;
        }

        // 对于普通类型, 直接计算分子和分母
        var num = left.Numerator * right.Numerator;
        var den = left.Denominator * right.Denominator;
        return new(num, den);
    }

    /// <summary>
    /// 除法运算符
    /// </summary>
    /// <remarks>
    /// 除法的实现是通过乘法和取倒数来完成的, 由于乘法已经处理了无穷大和 <see cref="NaN"/> 的情况,
    /// 因此除数允许为 0
    /// </remarks>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>除法后的大有理数</returns>
    public static BigRational operator /(BigRational left, BigRational right) => left * right.Reciprocal;

    /// <summary>
    /// 取模运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>取模后的大有理数</returns>
    public static BigRational operator %(BigRational left, BigRational right)
    {
        // 只有两个操作数都是普通类型且右操作数不为零时才能进行取模运算
        if (left.Kind != RationalKind.Normal || right.Kind != RationalKind.Normal || right.Numerator.IsZero)
        {
            return NaN;
        }

        // 计算取模结果
        var intPart = (BigInteger)(left / right);
        return left - right * intPart;
    }

    /// <summary>
    /// 等于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果相等, 否则 <see langword="false"/></returns>
    public static bool operator ==(BigRational left, BigRational right) => left.Equals(right);

    /// <summary>
    /// 不等于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果不相等, 否则 <see langword="false"/></returns>
    public static bool operator !=(BigRational left, BigRational right) => !left.Equals(right);

    /// <summary>
    /// 小于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果小于, 否则 <see langword="false"/></returns>
    public static bool operator <(BigRational left, BigRational right) => left.CompareTo(right) < 0;

    /// <summary>
    /// 小于等于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果小于等于, 否则 <see langword="false"/></returns>
    public static bool operator <=(BigRational left, BigRational right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// 大于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果大于, 否则 <see langword="false"/></returns>"
    public static bool operator >(BigRational left, BigRational right) => left.CompareTo(right) > 0;

    /// <summary>
    /// 大于等于运算符
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns><see langword="true"/> 如果大于等于, 否则 <see langword="false"/></returns>
    public static bool operator >=(BigRational left, BigRational right) => left.CompareTo(right) >= 0;
}
