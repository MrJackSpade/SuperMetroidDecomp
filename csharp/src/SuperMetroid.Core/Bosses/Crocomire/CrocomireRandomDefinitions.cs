namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The random choices of Crocomire's fight and death: its dust, its acid smoke and its bridge's fragments.</summary>
internal static class CrocomireRandomDefinitions
{
    /// <summary>A dying Crocomire's acid smoke offset from its body's X, 0 through 63 pixels.</summary>
    internal static readonly RandomSampleField AcidSmokeXOffset = new(0, 6);

    /// <summary>
    /// Whether the acid smoke keeps its offset to the right of the body (#2479), taken when the sample's second bit is set,
    /// half the random numbers; otherwise the smoke rises to the left, <see cref="AcidSmokeLeftExtraPixels"/> further from
    /// the body than its offset.
    /// </summary>
    internal static readonly RandomChance AcidSmokeRisesOnRight = RandomChance.Highest(1, new RandomSampleField(1, 1));

    /// <summary>
    /// How much further from the body acid smoke on the left rises than smoke on the right with the same offset: one
    /// pixel, so the smoke on the left rises 1 through 64 pixels from the body's X.
    /// </summary>
    internal const int AcidSmokeLeftExtraPixels = 1;

    /// <summary>
    /// How far above the acid surface's line sixteen pixels down the smoke starts, 0 through 31 pixels.
    /// </summary>
    internal static readonly RandomSampleField AcidSmokeRise = new(8, 5);

    /// <summary>The spread of a crumbling bridge fragment's starting fall speed, 0 through 63 256ths of a pixel an update added to its slowest.</summary>
    internal static readonly RandomSampleField BridgeFragmentFallSpeedSpread = new(0, 6);

    /// <summary>A foot dust puff's jitter to the right of its offset, 0 through 7 pixels.</summary>
    internal static readonly RandomSampleField DustJitter = new(0, 3);

    /// <summary>
    /// The spread of the dust Crocomire raises walking toward the spike wall, 0 through 15 pixels added to its side's
    /// offset.
    /// </summary>
    internal static readonly RandomSampleField SpikeWallDustSpread = new(0, 4);

    /// <summary>The first sample of the half of the random range that raises a step toward the spike wall's dust on his left (#2182).</summary>
    internal const int SpikeWallDustLeftFrom = 2048;

    /// <summary>The first sample of the half of the random range that mirrors a foot dust puff to his other side (#2182).</summary>
    internal const int FootDustMirroredFrom = 4096;

    /// <summary>A random foot dust puff's distance from the body, 0 through 31 pixels on either side.</summary>
    internal static readonly RandomSampleField FootDustOffset = new(0, 5);

    /// <summary>
    /// Whether Crocomire starts a spit attack where his walk offers one (#1619): one chance in four starts the attack.
    /// </summary>
    internal static readonly RandomChance ProjectileAttack = RandomChance.Lowest(1024, new RandomSampleField(0, 12));
}
