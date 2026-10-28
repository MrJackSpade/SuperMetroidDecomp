namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>The near and far spit shots: two entries in $A6:9A48-9A57.</summary>
    internal const int SpitCount = 2;

    /// <summary>Each spit shot's horizontal speed step: the near shot travels two pixels per frame, the far shot four.</summary>
    internal const int SpitSpeedStep = 2 * 256;

    /// <summary>Both spit shots start upward at five pixels per frame.</summary>
    internal const int SpitRise = -5 * 256;

    /// <summary>The body launch rows in the order the authored row selector counts them.</summary>
    internal static readonly FakeKraidSpikeRow[] Rows = [FakeKraidSpikeRow.Top, FakeKraidSpikeRow.Middle, FakeKraidSpikeRow.Bottom];

    /// <summary>
    /// The fixed body launch offsets from $86:9E7D..9E82: the top, middle and bottom ports are 2 pixels above, 12 below
    /// and 24 below body Y. This is port selection, not an interpolated motion curve.
    /// </summary>
    internal static readonly Dictionary<FakeKraidSpikeRow, int> SpikeYOffsets = new()
    {
        [FakeKraidSpikeRow.Top] = -2,
        [FakeKraidSpikeRow.Middle] = 12,
        [FakeKraidSpikeRow.Bottom] = 24,
    };
}
