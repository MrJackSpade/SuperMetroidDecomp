namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Power Bomb radius, or a per-frame change of radius, in 1/256 pixels from 0 through 65535. The explosion advances
/// its radii and their shared speed with sixteen-bit wraparound: a speed decelerated below zero wraps to a large value
/// and a radius advanced past the top wraps to a small one, and the window shapes follow those wrapped values. The
/// wrapping and carry-testing operations below are the only arithmetic the explosion performs on it.
/// </summary>
public readonly record struct PowerBombRadius
{
    /// <summary>Stores an already range-checked value.</summary>
    private PowerBombRadius(int rawValue) => RawValue = rawValue;

    /// <summary>The value in 1/256 pixels, 0 through 65535.</summary>
    public int RawValue { get; }

    /// <summary>The whole pixels of the value, 0 through 255; the fraction is dropped.</summary>
    public int WholePixels => SnesUnsignedEightEight.Whole(checked((ushort)RawValue));

    /// <summary>A zero radius or a stationary speed.</summary>
    public static PowerBombRadius Zero => default;

    /// <summary>A value in 1/256 pixels, 0 through 65535; other values are rejected.</summary>
    public static PowerBombRadius FromRaw(int rawValue) =>
        new(Ensure.BetweenInclusive(0, ushort.MaxValue, rawValue));

    /// <summary>This value advanced by <paramref name="other"/>, wrapping through 65535 and zero.</summary>
    public PowerBombRadius WrappingAdd(PowerBombRadius other) => Wrap(RawValue + other.RawValue);

    /// <summary>This value increased by <paramref name="amount"/> 1/256 pixels, wrapping through 65535 and zero.</summary>
    public PowerBombRadius WrappingAdd(int amount) => Wrap(RawValue + amount);

    /// <summary>This value decreased by <paramref name="amount"/> 1/256 pixels, wrapping through zero and 65535.</summary>
    public PowerBombRadius WrappingSubtract(int amount) => Wrap(RawValue - amount);

    /// <summary>
    /// Adds <paramref name="other"/> unless the sum carries past 65535, the carry test the shape phases make before
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

    /// <summary>Reduces an intermediate sum to its sixteen-bit wrapped value.</summary>
    private static PowerBombRadius Wrap(int value) => new(unchecked((ushort)value));
}
