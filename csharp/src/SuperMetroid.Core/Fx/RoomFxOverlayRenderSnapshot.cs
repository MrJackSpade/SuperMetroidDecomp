namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable gameplay effect overlay values published by one frame boundary, with how the effect's overlay blends with the
/// room (#1545), or null when the effect describes no blend. <c>Liquid</c> is the liquid the effect fills the room with, or null for an effect
/// with none (#1695). <c>Surface</c> is the liquid surface's room line, or null with
/// no surface; <c>WaterSurfaceScreenY</c> its screen line, or null while no surface is placed on the screen (#1546).
/// <c>Scroll</c> is an atmosphere's scroll; a liquid scrolls only across, and lies on the screen where
/// <see cref="LiquidSurfaceLine"/> places it (#1706).
/// </summary>
public readonly record struct RoomFxOverlayRenderSnapshot(
    RoomFxType Type,
    RoomEffectBlend? Blend,
    Position16<OverlayPixels> Scroll,
    int? Surface,
    RoomLiquid? Liquid = null,
    int WaterOverlayWavePhase = 0,
    int WaterBackgroundWavePhase = 0,
    int? WaterSurfaceScreenY = null)
{
    /// <summary>
    /// The phase of the lava or acid heat haze over the background, circular over its sixteen samples. Zero is the phase
    /// the haze shows first after it starts; each later phase steps the samples backward one place.
    /// </summary>
    public int LavaAcidBackgroundWavePhase { get; init; }

    /// <summary>
    /// The screen line the liquid's surface lay on when the effect last placed the liquid on the screen (#1706), or null
    /// while no liquid is placed: none has been, or the liquid had no surface within the room then. The effect overlay
    /// shows the liquid only from this line, even where the surface has since returned to the room.
    /// </summary>
    public int? LiquidSurfaceLine { get; init; }
}
