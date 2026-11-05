using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What a room actor wrote over a layer shows in a frame (#1403, #1514): each written picture's frame in the room's
/// written picture sheet (#1392), where it shows on the layer, in the order written; a later picture covers an earlier one
/// wherever they overlap. The layer shows through where nothing was written.
/// </summary>
public sealed class RoomOverlayPictures
{
    /// <summary>Holds the pictures, in the order written.</summary>
    /// <exception cref="ArgumentException">No pictures are given.</exception>
    public RoomOverlayPictures(ImmutableArray<PlacedWrittenPicture> pictures)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("An overlay needs its pictures.", nameof(pictures));
        Pictures = pictures;
    }

    /// <summary>The pictures, in the order written.</summary>
    public ImmutableArray<PlacedWrittenPicture> Pictures { get; }
}
