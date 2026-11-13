namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Power Bomb radius, or a per-frame change of radius, in 1/256 pixels from 0 through 65535. The explosion advances
/// its radii and their shared speed with wraparound at 65536: a speed decelerated below zero wraps to a large value
/// and a radius advanced past the top wraps to a small one, and the window shapes follow those wrapped values. The
/// wrapping and overflow-testing operations below are the only arithmetic the explosion performs on it.
/// </summary>
public readonly record struct PowerBombRadius
{
    /// <summary>Stores an already range-checked value.</summary>
    private PowerBombRadius(int rawValue) => RawValue = rawValue;

    /// <summary>The value in 1/256 pixels, 0 through 65535.</summary>
    public int RawValue { get; }

    /// <summary>The whole pixels of the value, 0 through 255; the fraction is dropped.</summary>
    public int WholePixels => UnsignedEightEight.Whole(Ensure.BetweenInclusive(0, ushort.MaxValue, RawValue));

    /// <summary>Whether <paramref name="left"/> is the smaller radius or speed.</summary>
    public static bool operator <(PowerBombRadius left, PowerBombRadius right) => left.RawValue < right.RawValue;

    /// <summary>Whether <paramref name="left"/> is the larger radius or speed.</summary>
    public static bool operator >(PowerBombRadius left, PowerBombRadius right) => left.RawValue > right.RawValue;

    /// <summary>A zero radius or a stationary speed.</summary>
    public static PowerBombRadius Zero => default;

    /// <summary>A value in 1/256 pixels, 0 through 65535; other values are rejected.</summary>
    public static PowerBombRadius FromRaw(int rawValue) =>
        new(Ensure.BetweenInclusive(0, ushort.MaxValue, rawValue));

    /// <summary>A whole number of pixels, 0 through 255, with no fraction.</summary>
    public static PowerBombRadius FromWholePixels(int pixels) => FromRaw(EightEight.FromWhole(Ensure.BetweenInclusive(0, byte.MaxValue, pixels)));

    /// <summary>This value advanced by <paramref name="other"/>, wrapping through 65535 and zero.</summary>
    public PowerBombRadius WrappingAdd(PowerBombRadius other) => Wrap(RawValue + other.RawValue);

    /// <summary>This value increased by <paramref name="amount"/> 1/256 pixels, wrapping through 65535 and zero.</summary>
    public PowerBombRadius WrappingAdd(int amount) => Wrap(RawValue + amount);

    /// <summary>This value decreased by <paramref name="amount"/> 1/256 pixels, wrapping through zero and 65535.</summary>
    public PowerBombRadius WrappingSubtract(int amount) => Wrap(RawValue - amount);

    /// <summary>
    /// Adds <paramref name="other"/> unless the sum passes 65535, the overflow test the shape phases make before
    /// changing either the radius or its speed.
    /// </summary>
    /// <returns>True with the sum when it fits; false, leaving <paramref name="sum"/> at this value, when it carries.</returns>
    public bool TryAddWithoutCarry(PowerBombRadius other, out PowerBombRadius sum)
    {
        int total = RawValue + other.RawValue;
        if (total > ushort.MaxValue)
        {
            sum = this;
            return false;
        }

        sum = new PowerBombRadius(total);
        return true;
    }

    /// <summary>The values a radius or speed counts through before it continues from zero.</summary>
    private const int Steps = 65536;

    /// <summary>An intermediate sum continued through the ends of the range: past the top from zero, below zero from the top.</summary>
    private static PowerBombRadius Wrap(int value) => new((value % Steps + Steps) % Steps);
}
