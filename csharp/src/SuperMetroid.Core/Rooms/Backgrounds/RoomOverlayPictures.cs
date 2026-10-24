using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What a room actor wrote over a background shows in a frame (#1403): the picture of each written word, in the room's
/// tile word sheet over what its character shows (#1392), by the tilemap position it was written at; nothing where
/// nothing was written, which leaves the background showing.
/// </summary>
public sealed class RoomOverlayPictures
{
    /// <summary>Holds each tilemap position's picture.</summary>
    /// <exception cref="ArgumentException">There is not one picture slot for each tilemap position.</exception>
    public RoomOverlayPictures(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length != RoomTilemapOverlay.WordCount)
            throw new ArgumentException($"An overlay needs a picture slot for each of its {RoomTilemapOverlay.WordCount} positions.", nameof(pictures));
        Pictures = pictures;
    }

    /// <summary>Each tilemap position's picture, or null where nothing was written.</summary>
    public ImmutableArray<BgTilePicture?> Pictures { get; }
}
