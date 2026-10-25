namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>Computes the signed8.8 near/far launches from $A6:9A48-9A57, mirrored by facing.</summary>
    internal static EightEightVector<RoomPixels> SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= 2)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit index {projectile} exceeds its two-entry direction set.");
        }

        // Near/far shots travel at two/four pixels per frame,
        // mirrored with facing; both start upward at five pixels per frame.
        int horizontal = (projectile + 1) * 2 * 256;
        return new((short)(movingRight ? horizontal : -horizontal), -5 * 256);
    }

    /// <summary>Selects a fixed body launch position from $86:9E7D..9E82.
    /// Native $A6:9AC2..9B10 selects one of three independent spike timers at byte
    /// offsets0/2/4; the managed boundary divides that selector by two. The selected
    /// top/middle/bottom port is respectively2 pixels above,12 below or24 below body Y.
    /// $86:9E4E adds this signed integer offset with16-bit wrap, independently of
    /// animation and facing. This is port selection, not an interpolated motion curve.</summary>
    internal static short SpikeYOffset(int row) => (uint)row < (uint)Rows.Length
        ? Rows[row] switch
        {
            FakeKraidSpikeRow.Top => (short)-2,
            FakeKraidSpikeRow.Middle => (short)12,
            FakeKraidSpikeRow.Bottom => (short)24,
            _ => throw new InvalidDataException($"Fake Kraid spike row {Rows[row]} is not a body launch port."),
        }
        : throw new InvalidDataException($"Fake Kraid spike row {row} is not a body launch port.");

    /// <summary>The body launch rows in the order the authored row selector counts them.</summary>
    private static readonly FakeKraidSpikeRow[] Rows = [FakeKraidSpikeRow.Top, FakeKraidSpikeRow.Middle, FakeKraidSpikeRow.Bottom];
}
