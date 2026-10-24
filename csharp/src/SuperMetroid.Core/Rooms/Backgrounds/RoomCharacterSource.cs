using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Where a room's BG1 and BG2 blocks read their characters (#1392): what shows over them and from which VRAM word, and the
/// frames the room's animations show over the characters they paint (#1406).
/// </summary>
/// <param name="Artwork">What shows over the characters.</param>
/// <param name="CharacterBase">The VRAM word of character zero.</param>
/// <param name="Animated">The characters animations paint and the frames they show, which show before what is bound there.</param>
public readonly record struct RoomCharacterSource(IBgCharacterArtwork Artwork, int CharacterBase, AnimatedCharacterView Animated)
{
    /// <summary>
    /// The characters a room of <paramref name="characters"/> reads: the room tileset's from the start of VRAM, bound in
    /// <paramref name="backgrounds"/>, or the OBJ character table's, as <paramref name="objects"/> records its uploads;
    /// with what <paramref name="animations"/> declare the room's animations paint there, showing <paramref name="frames"/>.
    /// </summary>
    public static RoomCharacterSource Of(RoomBackgroundCharacters characters, BgArtworkBindings backgrounds, ObjCharacterUploads objects,
        RoomAnimatedCharacters animations, ArtworkFrameSelections frames)
    {
        var animated = new AnimatedCharacterView(Ensure.NotNull(animations).Of(characters), Ensure.NotNull(frames));
        return characters == RoomBackgroundCharacters.ObjectCharacterPage
            ? new(new ObjPageCharacterArtwork(objects), CharacterBaseOf(characters), animated)
            : new(Ensure.NotNull(backgrounds), CharacterBaseOf(characters), animated);
    }

    /// <summary>The VRAM word of character zero a room of <paramref name="characters"/> reads its BG1 and BG2 characters from.</summary>
    [PpuLayoutBoundary("codec: the character bases BG1 and BG2 read a room's characters from")]
    public static int CharacterBaseOf(RoomBackgroundCharacters characters) =>
        characters == RoomBackgroundCharacters.ObjectCharacterPage ? GameplayVramLayout.ObjectCharacters.Offset : RoomGraphicsLayout.AreaCharacters.Offset;
}
