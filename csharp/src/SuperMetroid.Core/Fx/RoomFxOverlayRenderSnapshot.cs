namespace SuperMetroid.Core.Fx;

/// <summary>Immutable gameplay BG3 values published by one frame boundary.</summary>
public readonly record struct RoomFxOverlayRenderSnapshot(
    RoomFxType Type,
    LayerBlendingConfiguration LayerBlendConfiguration,
    Position16<OverlayPixels> Scroll,
    Position16<RoomPixels> Surface,
    RoomLiquidOptions LiquidOptions = RoomLiquidOptions.None,
    int WaterBg3WavePhase = 0,
    int WaterBg2WavePhase = 0,
    int WaterSurfaceScreenY = short.MaxValue)
{
    /// <summary>
    /// Effective circular phase of the lava/acid BG2 per-line table. Zero is the first table
    /// produced after the haze starts; subsequent phases move backward through 16 entries.
    /// </summary>
    public int LavaAcidBg2WavePhase { get; init; }
}
