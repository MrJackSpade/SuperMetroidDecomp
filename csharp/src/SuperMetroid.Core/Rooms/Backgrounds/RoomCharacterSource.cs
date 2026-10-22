using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Where a room's BG1 and BG2 blocks read their characters (#1392): what shows over them and from which VRAM word.</summary>
/// <param name="Artwork">What shows over the characters.</param>
/// <param name="CharacterBase">The VRAM word of character zero.</param>
public readonly record struct RoomCharacterSource(IBgCharacterArtwork Artwork, int CharacterBase)
{
    /// <summary>
    /// The characters a room of <paramref name="characters"/> reads: the room tileset's from the start of VRAM, bound in
    /// <paramref name="backgrounds"/>, or the OBJ character table's, as <paramref name="objects"/> records its uploads.
    /// </summary>
    [PpuLayoutBoundary("codec: the character bases BG1 and BG2 read a room's blocks from")]
    public static RoomCharacterSource Of(RoomBackgroundCharacters characters, BgArtworkBindings backgrounds, ObjCharacterUploads objects) =>
        characters == RoomBackgroundCharacters.ObjectCharacterPage
            ? new(new ObjPageCharacterArtwork(objects), GameplayVramLayout.ObjectCharacters.Offset)
            : new(Ensure.NotNull(backgrounds), RoomGraphicsLayout.AreaCharacters.Offset);
}
