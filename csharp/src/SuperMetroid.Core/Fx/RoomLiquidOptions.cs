namespace SuperMetroid.Core.Fx;

/// <summary>
/// The liquid options a room-FX record selects. Each liquid tests the options one at a time, and records combine
/// them (a tide with a drifting surface, a wave with a tide), so they are composable. Two options mean different
/// things to water and to lava or acid; their names say both. Options no liquid tests are kept as loaded.
/// </summary>
[Flags]
public enum RoomLiquidOptions
{
    /// <summary>No option: a still liquid with no BG2 wave and no tide.</summary>
    None = 0,

    /// <summary>Water: the BG3 surface texture drifts right by a quarter pixel every frame.</summary>
    SurfaceDrift = 0x01,

    /// <summary>
    /// Water: BG2 ripples with the horizontal water wave below the surface. Lava and acid: BG2 ripples with the
    /// vertical heat-haze wave, which takes priority over the horizontal one.
    /// </summary>
    Bg2Wave = 0x02,

    /// <summary>
    /// Water: Samus's movement ignores the water surface until a PLM clears this option (the n00b tube's glass).
    /// Lava and acid: BG2 ripples with the horizontal heat wave when the vertical one is not selected.
    /// </summary>
    WaterPhysicsDisabledOrHorizontalHeatWave = 0x04,

    /// <summary>An option some records set that no liquid tests; kept so the record's options round-trip.</summary>
    Untested = 0x08,

    /// <summary>The surface rises and falls by up to 32 pixels; the small tide takes priority when both are set.</summary>
    LargeTide = 0x40,

    /// <summary>The surface rises and falls by up to 8 pixels.</summary>
    SmallTide = 0x80,
}
