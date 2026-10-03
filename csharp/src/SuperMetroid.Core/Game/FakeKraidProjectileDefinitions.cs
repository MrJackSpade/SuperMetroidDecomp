namespace SuperMetroid.Core.Game;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>
    /// Top, middle, and bottom body-spike Y offsets from <c>$86:9E7D-$86:9E82</c>.
    /// </summary>
    private static readonly short[] SpikeYOffsets = [-2, 12, 24];

    /// <summary>Computes the signed8.8 near/far launches from $A6:9A48-9A57, mirrored by facing.</summary>
    internal static FakeKraidSpitLaunch SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= 2)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit index {projectile} exceeds its two-entry direction set.");
        }

        // Near/far shots travel at two/four pixels per frame,
        // mirrored with facing; both start upward at five pixels per frame.
        int horizontal = (projectile + 1) * 2 * 256;
        return new(unchecked((ushort)(movingRight ? horizontal : -horizontal)),
            unchecked((ushort)(-5 * 256)));
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
