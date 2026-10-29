using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed artwork a library background's command list is replayed over (#1406): the tilemaps it decompresses, the
/// sky pages it copies, and the character artwork it brings.
/// </summary>
internal sealed record LibraryBackgroundArtwork
{
    /// <summary>The installed decompressed tilemaps, by the compressed stream each stands for.</summary>
    public RoomBackgroundTilemaps? TilemapArt { get; init; }

    /// <summary>The installed scrolling-sky tilemap pages.</summary>
    public RoomSkyTilemaps? SkyArt { get; init; }

    /// <summary>The installed HUD characters, which Kraid's list moves.</summary>
    public HudTileAtlas? HudArt { get; init; }

    /// <summary>The installed room characters, which hold the Tourian statue ghost's.</summary>
    public RoomCharacterAtlases? CharacterArt { get; init; }
}
