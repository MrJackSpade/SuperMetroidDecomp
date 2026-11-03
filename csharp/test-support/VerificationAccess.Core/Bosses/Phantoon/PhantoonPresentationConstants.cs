using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Cartridge values and helpers of <see cref="PhantoonPresentation"/> that only verification reads.</summary>
internal static class PhantoonPresentationConstants
{
    /// <summary>Interpolates one RGB5 color during Phantoon's fade using channel rounding.</summary>
    /// <param name="numerator">The fade's step, 0 through 65535.</param>
    /// <param name="denominator">The fade's step count, 0 through 65535.</param>
    /// <param name="current">The colour now shown.</param>
    /// <param name="target">The colour the fade reaches.</param>
    internal static RgbColor CalculatePhantoonTransitionColor(
        int numerator,
        int denominator,
        RgbColor current,
        RgbColor target)
    {
        return current.Zip(target, (_, from, to) =>
            CalculatePhantoonTransitionComponent(numerator, denominator, from, to));
    }
    /// <summary>Interpolates one RGB5 channel during a palette transition.</summary>
    internal static int CalculatePhantoonTransitionComponent(
        int numerator,
        int denominator,
        int current,
        int target)
    {
        if (numerator == 0)
            return current;
        if (numerator - 1 == denominator)
            return target;
        // The fade step divides the absolute component delta in 8.8 fixed point,
        // then applies its sign before selecting the high byte. Integer division
        // of the signed delta both adds an extra fade step and rounds darkening
        // in the wrong direction. The divisor is an eight-bit register; the steps left are 1 through the
        // denominator (at most 12), because the first and last steps return above, so it never wraps or reaches zero.
        int remainingSteps = denominator - numerator + 1;
        int quotient = EightEight.FromWhole(Math.Abs(target - current)) / remainingSteps;
        int signedStep = target < current ? -quotient : quotient;
        return UnsignedEightEight.Whole(CoordinateAxis.Wrap(EightEight.FromWhole(current) + signedStep));
    }
}
