namespace SuperMetroid.Core.Game;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>
    /// Two leftward followed by two rightward signed 8.8 velocity pairs from
    /// <c>$A6:9A48-$A6:9A57</c>.
    /// </summary>
    private static readonly FakeKraidSpitLaunch[] SpitLaunches =
    [
        new(0xfe00, 0xfb00),
        new(0xfc00, 0xfb00),
        new(0x0200, 0xfb00),
        new(0x0400, 0xfb00),
    ];

    /// <summary>
    /// Top, middle, and bottom body-spike Y offsets from <c>$86:9E7D-$86:9E82</c>.
    /// </summary>
    private static readonly short[] SpikeYOffsets = [-2, 12, 24];

    /// <summary>Returns one of the two authored spit launches for a facing direction.</summary>
    internal static FakeKraidSpitLaunch SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= 2)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit index {projectile} exceeds its two-entry direction set.");
        }

        return SpitLaunches[(movingRight ? 2 : 0) + projectile];
    }

    /// <summary>Returns the authored Y offset for one of Fake Kraid's three spike rows.</summary>
    internal static short SpikeYOffset(int row)
    {
        if ((uint)row >= SpikeYOffsets.Length)
        {
            throw new InvalidDataException(
                $"Fake Kraid spike row {row} exceeds its three-entry table.");
        }

        return SpikeYOffsets[row];
    }
}

/// <summary>One signed 8.8 Fake Kraid spit launch-velocity pair.</summary>
internal readonly record struct FakeKraidSpitLaunch(
    ushort XVelocity,
    ushort YVelocity);
