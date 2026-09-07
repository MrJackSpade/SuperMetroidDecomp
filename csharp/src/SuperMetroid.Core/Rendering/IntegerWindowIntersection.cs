namespace SuperMetroid.Core.Rendering;

/// <summary>Intersects a bounded pixel interval with an integer linear half-plane.</summary>
internal static class IntegerWindowIntersection
{
    /// <summary>Restricts X to coefficient*X + constant >= threshold; endpoints remain inclusive.</summary>
    internal static bool IntersectGreaterEqual(long coefficient, long constant, long threshold,
        ref long left, ref long right)
    {
        if (coefficient == 0) return constant >= threshold && left <= right;
        long numerator = threshold - constant;
        long quotient = numerator / coefficient;
        long remainder = numerator % coefficient;
        if (coefficient > 0)
        {
            // C# division truncates toward zero. Positive remainder requires rounding
            // up; negative remainder is already the ceiling for a positive divisor.
            long ceiling = quotient + (remainder > 0 ? 1 : 0);
            left = Math.Max(left, ceiling);
        }
        else
        {
            // Dividing by a negative coefficient reverses the inequality. A positive
            // numerator remainder makes the truncated quotient larger than the floor.
            long floor = quotient - (remainder > 0 ? 1 : 0);
            right = Math.Min(right, floor);
        }
        return left <= right;
    }
}
