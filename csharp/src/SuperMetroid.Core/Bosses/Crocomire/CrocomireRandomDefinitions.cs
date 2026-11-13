namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The random choices of Crocomire's fight and death: its dust, its acid smoke and its bridge's fragments.</summary>
internal static class CrocomireRandomDefinitions
{
    /// <summary>A dying Crocomire's acid smoke offset from its body's X, 0 through 63 pixels: the sample modulo 64.</summary>
    internal static readonly RandomSampleField AcidSmokeXOffset = new(0, 6);

    /// <summary>
    /// Whether the acid smoke keeps its offset to the right of the body, bit one; when clear the smoke rises to the
    /// left, <see cref="AcidSmokeLeftExtraPixels"/> further from the body than its offset.
    /// </summary>
    internal static readonly RandomSampleField AcidSmokeOnRight = new(1, 1);

    /// <summary>
    /// How much further from the body acid smoke on the left rises than smoke on the right with the same offset: one
    /// pixel, so the smoke on the left rises 1 through 64 pixels from the body's X.
    /// </summary>
    internal const int AcidSmokeLeftExtraPixels = 1;

    /// <summary>
    /// How far above the acid surface's line sixteen pixels down the smoke starts, 0 through 31 pixels: the sample
    /// divided by 256, modulo 32.
    /// </summary>
    internal static readonly RandomSampleField AcidSmokeRise = new(8, 5);

    /// <summary>A crumbling bridge fragment's added fall speed, 0 through 63 in 256ths of a pixel: the sample modulo 64.</summary>
    internal static readonly RandomSampleField BridgeFragmentFallSpeed = new(0, 6);

    /// <summary>A foot dust puff's jitter to the right of its offset, 0 through 7 pixels: the sample modulo 8.</summary>
    internal static readonly RandomSampleField DustJitter = new(0, 3);

    /// <summary>
    /// The spread of the dust Crocomire raises walking toward the spike wall, 0 through 15 pixels added to its side's
    /// offset: the sample modulo 16.
    /// </summary>
    internal static readonly RandomSampleField SpikeWallDustSpread = new(0, 4);

    /// <summary>A random foot dust puff's distance from the body, 0 through 31 pixels on either side: the sample modulo 32.</summary>
    internal static readonly RandomSampleField FootDustOffset = new(0, 5);

    /// <summary>
    /// Whether Crocomire starts a spit attack where his walk offers one (#1619): the lowest 1024 of the 4096 rolls of the
    /// sample modulo 4096, one chance in four, start the attack.
    /// </summary>
    internal static readonly RandomChance ProjectileAttack = RandomChance.Lowest(1024, new RandomSampleField(0, 12));
}
