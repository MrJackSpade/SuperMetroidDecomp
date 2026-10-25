namespace SuperMetroid.Core.Fx;

/// <summary>Immutable gameplay BG3 values published by one accepted NMI.</summary>
public readonly record struct RoomLayer3FxRenderSnapshot(
    RoomFxType Type,
    LayerBlendingConfiguration LayerBlendConfiguration,
    Position16<Bg3Pixels> Scroll,
    Position16<RoomPixels> Surface,
    RoomLiquidOptions LiquidOptions = RoomLiquidOptions.None,
    int WaterBg3WavePhase = 0,
    int WaterBg2WavePhase = 0,
    int WaterSurfaceScreenY = short.MaxValue)
{
    /// <summary>
    /// Effective circular phase of the lava/acid BG2 HDMA table. Zero is the first table
    /// produced after <c>$88:B4CE</c>; subsequent phases move backward through 16 entries.
    /// </summary>
    public int LavaAcidBg2WavePhase { get; init; }
}
