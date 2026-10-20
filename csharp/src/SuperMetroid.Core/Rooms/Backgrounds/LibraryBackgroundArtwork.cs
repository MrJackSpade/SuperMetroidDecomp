using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The installed tilemap, sky, HUD and character artwork that library-background commands copy instead of ROM bytes; standalone cartridge runs supply none (#633).</summary>
public sealed record LibraryBackgroundArtwork
{
    /// <summary>Tilemap art.</summary>
    public RoomBackgroundTilemapCatalog? TilemapArt { get; init; } = null;

    /// <summary>Sky art.</summary>
    public RoomSkyTilemapCatalog? SkyArt { get; init; } = null;

    /// <summary>Hud art.</summary>
    public HudTileAtlas? HudArt { get; init; } = null;

    /// <summary>Character art.</summary>
    public RoomCharacterAtlasCatalog? CharacterArt { get; init; } = null;

    /// <summary>The bindings character transfers bind into (#1350); a program that transfers characters requires them.</summary>
    public BgArtworkBindings? Backgrounds { get; init; } = null;
}
