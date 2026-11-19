namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>The spit shots in the order a spit launches them: the near shot, then the far one.</summary>
    internal static IReadOnlyList<FakeKraidSpit> SpitsInLaunchOrder { get; } = [FakeKraidSpit.Near, FakeKraidSpit.Far];

    /// <summary>
    /// Each spit shot's horizontal speed toward the side Fake Kraid faces, in pixels an update: the near shot two, the far
    /// shot four.
    /// </summary>
    internal static IReadOnlyDictionary<FakeKraidSpit, int> SpitSpeedPixels { get; } = new Dictionary<FakeKraidSpit, int>
    {
        [FakeKraidSpit.Near] = 2,
        [FakeKraidSpit.Far] = 4,
    };

    /// <summary>
    /// How far Fake Kraid steps each walk cue, in pixels, toward the side he walks; his spit launches the same distance
    /// ahead of his body.
    /// </summary>
    internal const int StepPixels = 4;

    /// <summary>How far above his body, in pixels, Fake Kraid's spit leaves it.</summary>
    internal const int SpitYOffset = -16;

    /// <summary>The pickups Fake Kraid's death scatters about where he stood.</summary>
    internal const int DeathDropCount = 4;

    /// <summary>Both spit shots start upward at five pixels per frame.</summary>
    internal static readonly SignedEightEight SpitRise = SignedEightEight.FromPixels(-5);

    /// <summary>The gravity Fake Kraid's spit gains each update: a quarter of a pixel an update.</summary>
    internal static readonly SignedEightEight SpitGravity = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>The fastest Fake Kraid's spit falls: four pixels an update.</summary>
    internal static readonly SignedEightEight SpitMaximumFallSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The speed a body spike travels horizontally, toward the side Fake Kraid faces: two pixels an update.</summary>
    internal static readonly SignedEightEight SpikeSpeed = SignedEightEight.FromPixels(2);

    /// <summary>
    /// The visits of each body port's clock, beyond his random starting delay, up to the one its first spike flies on,
    /// that one included, in the order his setup starts their clocks: the top port's 65, the middle's 97 and the bottom's
    /// 49.
    /// </summary>
    internal static IReadOnlyList<KeyValuePair<FakeKraidSpikeRow, int>> SpikeStartDelays { get; } =
    [
        new(FakeKraidSpikeRow.Top, 65),
        new(FakeKraidSpikeRow.Middle, 97),
        new(FakeKraidSpikeRow.Bottom, 49),
    ];

    /// <summary>
    /// The fixed body launch offsets: the top, middle and bottom ports are 2 pixels above, 12 below
    /// and 24 below body Y. This is port selection, not an interpolated motion curve.
    /// </summary>
    internal static readonly Dictionary<FakeKraidSpikeRow, int> SpikeYOffsets = new()
    {
        [FakeKraidSpikeRow.Top] = -2,
        [FakeKraidSpikeRow.Middle] = 12,
        [FakeKraidSpikeRow.Bottom] = 24,
    };
}
