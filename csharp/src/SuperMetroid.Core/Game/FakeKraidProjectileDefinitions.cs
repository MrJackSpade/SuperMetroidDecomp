namespace SuperMetroid.Core.Game;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
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

    /// <summary>Selects a fixed body launch position from $86:9E7D..9E82.
    /// Native $A6:9AC2..9B10 selects one of three independent spike timers at byte
    /// offsets0/2/4; the managed boundary divides that selector by two. The selected
    /// top/middle/bottom port is respectively2 pixels above,12 below or24 below body Y.
    /// $86:9E4E adds this signed integer offset with16-bit wrap, independently of
    /// animation and facing. This is port selection, not an interpolated motion curve.</summary>
    internal static short SpikeYOffset(int row) => (FakeKraidSpikeRow)row switch
    {
        FakeKraidSpikeRow.Top => -2,
        FakeKraidSpikeRow.Middle => 12,
        FakeKraidSpikeRow.Bottom => 24,
        _ => throw new InvalidDataException($"Fake Kraid spike row {row} is not a body launch port."),
    };
}

/// <summary>Mutually exclusive body-spike ports selected by the three native timers.</summary>
internal enum FakeKraidSpikeRow
{
    /// <summary>Top timer at native byte offset0; launch offset word at86:9E7D.</summary>
    Top = 0,
    /// <summary>Middle timer at native byte offset2; launch offset word at86:9E7F.</summary>
    Middle = 1,
    /// <summary>Bottom timer at native byte offset4; launch offset word at86:9E81.</summary>
    Bottom = 2,
}

/// <summary>One signed 8.8 Fake Kraid spit launch-velocity pair.</summary>
internal readonly record struct FakeKraidSpitLaunch(
    ushort XVelocity,
    ushort YVelocity);
