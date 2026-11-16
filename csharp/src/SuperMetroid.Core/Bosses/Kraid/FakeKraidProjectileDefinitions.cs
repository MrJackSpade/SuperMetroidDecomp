namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Fixed launch definitions for Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectileDefinitions
{
    /// <summary>The near and far spit shots.</summary>
    internal const int SpitCount = 2;

    /// <summary>Each spit shot's horizontal speed step in pixels per frame: the near shot travels two pixels per frame, the far shot four.</summary>
    internal const int SpitSpeedStepPixels = 2;

    /// <summary>Both spit shots start upward at five pixels per frame.</summary>
    internal static readonly SignedEightEight SpitRise = SignedEightEight.FromPixels(-5);

    /// <summary>The gravity Mini-Kraid's spit gains each update: a quarter of a pixel an update.</summary>
    internal static readonly SignedEightEight SpitGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The fastest Mini-Kraid's spit falls: four pixels an update.</summary>
    internal static readonly SignedEightEight SpitMaximumFallSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The speed a body spike travels horizontally, toward the side Mini-Kraid faces: two pixels an update.</summary>
    internal static readonly SignedEightEight SpikeSpeed = SignedEightEight.FromPixels(2);

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
