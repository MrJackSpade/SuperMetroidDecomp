using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room-FX state and installed artwork that outlive each room: the Fireflea darkness state and the fixed
/// colour, which room loading never clears, and the host's FX catalogs. Each room's <see cref="RoomLayer3FxState"/>
/// reads them here.
/// </summary>
public sealed class RoomFxSession
{
    /// <summary>The Fireflea FX state. It is session-lived like the cartridge's WRAM, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea { get; } = new();

    /// <summary>The room's fixed colour. It is session-lived, so it keeps the last writer's colour across rooms.</summary>
    public RoomFixedColor FixedColors { get; } = new();

    /// <summary>Current host-owned liquid/rain frame art; never stored in debugger state.</summary>
    [field: NonSerialized]
    public RoomFxAnimatedTileAtlas? AnimatedTileArtwork { get; set; }

    /// <summary>Current host-owned room-FX blend colors; never stored in debugger state.</summary>
    [field: NonSerialized]
    public RoomFxBlendInks? PaletteBlendColors { get; set; }
}
