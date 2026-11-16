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

    /// <summary>Both spit shots start upward at five pixels per frame.</summary>
    internal static readonly SignedEightEight SpitRise = SignedEightEight.FromPixels(-5);

    /// <summary>The gravity Fake Kraid's spit gains each update: a quarter of a pixel an update.</summary>
    internal static readonly SignedEightEight SpitGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The fastest Fake Kraid's spit falls: four pixels an update.</summary>
    internal static readonly SignedEightEight SpitMaximumFallSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The speed a body spike travels horizontally, toward the side Fake Kraid faces: two pixels an update.</summary>
    internal static readonly SignedEightEight SpikeSpeed = SignedEightEight.FromPixels(2);

    /// <summary>
    /// The updates, beyond his random starting delay, before each body port's first spike, in the order his setup starts
    /// their clocks: the top port's after 64, the middle's after 96 and the bottom's after 48.
    /// </summary>
    internal static IReadOnlyList<KeyValuePair<FakeKraidSpikeRow, int>> SpikeStartDelays { get; } =
    [
        new(FakeKraidSpikeRow.Top, 64),
        new(FakeKraidSpikeRow.Middle, 96),
        new(FakeKraidSpikeRow.Bottom, 48),
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
