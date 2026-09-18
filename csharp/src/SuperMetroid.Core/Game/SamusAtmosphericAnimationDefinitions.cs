namespace SuperMetroid.Core.Game;

/// <summary>One fixed-point environmental damage contribution per accepted frame.</summary>
internal readonly record struct SamusLiquidDamageRate(ushort SubDamage, ushort WholeDamage);

/// <summary>Compiled liquid-damage rates from Samus's fixed bank-$90 physics constants.</summary>
internal static class SamusLiquidDamageDefinitions
{
    /// <summary>Lava rate at <c>$90:9E8B-$9E8E</c>: one half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Lava = new(0x8000, 0x0000);

    /// <summary>Acid rate at <c>$90:9E8F-$9E92</c>: one and one-half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Acid = new(0x8000, 0x0001);
}

/// <summary>Compiled frame cadence for the seven active Samus atmospheric-effect types.</summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>
    /// Frame timers selected through <c>$90:8B93-$8BA1</c>, stored contiguously at
    /// <c>$90:8BA5-$8BED</c>. Type zero and pointer entry eight are inactive sentinels.
    /// </summary>
    private static readonly ushort[][] FrameTimers =
    [
        [],
        [3, 3, 3, 3],
        [3, 3, 3, 3],
        [2, 2, 3, 3, 3, 5, 5, 6, 7],
        [2, 2, 2, 2],
        [5, 5, 5, 5, 5, 5, 5, 5],
        [3, 4, 5, 6],
        [3, 4, 5, 6],
    ];

    /// <summary>Returns the authored frame count for an active atmospheric type.</summary>
    internal static byte FrameCount(byte type) => checked((byte)ForType(type).Length);

    /// <summary>Returns one authored frame duration for an active atmospheric type.</summary>
    internal static ushort FrameTimer(byte type, byte frame)
    {
        ushort[] timers = ForType(type);
        if (frame >= timers.Length)
        {
            throw new InvalidDataException(
                $"Atmospheric type {type} frame {frame} is outside {timers.Length} authored frames.");
        }

        return timers[frame];
    }

    private static ushort[] ForType(byte type)
    {
        if (type is 0 or > 7)
        {
            throw new InvalidDataException(
                $"Atmospheric animation type {type} is outside active types one through seven.");
        }

        return FrameTimers[type];
    }
}
