using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed artwork a library background's command list is replayed over (#1406): the tilemaps it decompresses, the
/// sky pages it copies, and the character artwork it brings, bound into <see cref="Backgrounds"/>.
/// </summary>
internal sealed record LibraryBackgroundArtwork
{
    /// <summary>The installed decompressed tilemaps, by the compressed stream each stands for.</summary>
    public RoomBackgroundTilemapCatalog? TilemapArt { get; init; }

    /// <summary>The installed scrolling-sky tilemap pages.</summary>
    public RoomSkyTilemapCatalog? SkyArt { get; init; }

    /// <summary>The installed HUD characters, which Kraid's list moves.</summary>
    public HudTileAtlas? HudArt { get; init; }

    /// <summary>The installed room characters, which hold the Tourian statue ghost's.</summary>
    public RoomCharacterAtlasCatalog? CharacterArt { get; init; }

    /// <summary>The bindings the list's characters bind into; null binds them over the replayed VRAM alone.</summary>
    public BgArtworkBindings? Backgrounds { get; init; }

    /// <summary>The character artwork the runtime's load binds from, bound into <paramref name="backgrounds"/>.</summary>
    public LibraryBackgroundCharacterArtwork CharactersOver(BgArtworkBindings backgrounds) => new()
    {
        Backgrounds = backgrounds,
        HudArt = HudArt,
        CharacterArt = CharacterArt,
    };
}
