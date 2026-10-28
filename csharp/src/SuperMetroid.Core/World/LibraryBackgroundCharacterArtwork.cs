using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// The installed character artwork a library background brings and the bindings it is bound into (#1406): the Tourian
/// statue ghost's characters and the HUD's, which Kraid's background moves under BG1.
/// </summary>
public sealed record LibraryBackgroundCharacterArtwork
{
    /// <summary>The bindings the background's characters are bound into (#1350).</summary>
    public required BgArtworkBindings Backgrounds { get; init; }

    /// <summary>The installed HUD characters, which Kraid's background moves; null where none is installed.</summary>
    public HudTileAtlas? HudArt { get; init; }

    /// <summary>The installed room characters, which hold the Tourian statue ghost's; null where none is installed.</summary>
    public RoomCharacterAtlases? CharacterArt { get; init; }
}
