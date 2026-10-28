using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Where a room's BG1 and BG2 blocks read their characters (#1392, #1406): what shows over them, and the frames the room's
/// animations show over the characters they paint.
/// </summary>
/// <param name="Artwork">What shows over the characters.</param>
/// <param name="Animated">The characters animations paint and the frames they show, which show before what is bound there.</param>
public readonly record struct RoomCharacterSource(IBgCharacterArtwork Artwork, AnimatedCharacterView Animated)
{
    /// <summary>
    /// The characters a room of <paramref name="characters"/> reads: the room's tiles, bound in
    /// <paramref name="backgrounds"/>, or the OBJ character table's, as <paramref name="objects"/> records its uploads;
    /// with what <paramref name="animations"/> declare the room's animations paint there, showing <paramref name="frames"/>.
    /// </summary>
    public static RoomCharacterSource Of(RoomBackgroundCharacters characters, BgArtworkBindings backgrounds, SpriteArtworkPlacements objects,
        RoomAnimatedCharacters animations, ArtworkFrameSelections frames)
    {
        var animated = new AnimatedCharacterView(Ensure.NotNull(animations).Of(characters), Ensure.NotNull(frames));
        return characters == RoomBackgroundCharacters.ObjectCharacterPage
            ? new(new SpriteArtworkBackground(objects), animated)
            : new(Ensure.NotNull(backgrounds).Of(BgCharacterSet.Tiles), animated);
    }
}
