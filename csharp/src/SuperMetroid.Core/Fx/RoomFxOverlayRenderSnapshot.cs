namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable gameplay effect overlay values published by one frame boundary, with how the effect's overlay blends with the
/// room (#1545), or null when the effect describes no blend.
/// </summary>
public readonly record struct RoomFxOverlayRenderSnapshot(
    RoomFxType Type,
    RoomEffectBlend? Blend,
    Position16<OverlayPixels> Scroll,
    Position16<RoomPixels> Surface,
    RoomLiquidOptions LiquidOptions = RoomLiquidOptions.None,
    int WaterOverlayWavePhase = 0,
    int WaterBackgroundWavePhase = 0,
    int WaterSurfaceScreenY = short.MaxValue)
{
    /// <summary>
    /// Effective circular phase of the lava/acid background per-line table. Zero is the first table
    /// produced after the haze starts; subsequent phases move backward through 16 entries.
    /// </summary>
    public int LavaAcidBackgroundWavePhase { get; init; }
}
