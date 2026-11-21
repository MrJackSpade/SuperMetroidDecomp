using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A warning a frame shows in the room (#1403, #1514, #2573), its own layer: each of its pictures' frames in the room's
/// warning picture sheet (#1392), placed where it shows in the room, in the order shown, a later picture covering an earlier
/// one wherever they overlap. The warning shows at its depth, the foreground's or the background's, covering what that
/// depth shows behind it within each of its pictures; where it shows no picture, the room shows.
/// </summary>
public sealed partial class ShownRoomWarning
{
    /// <summary>Holds the pictures, in the order shown, and the depth the warning shows at.</summary>
    /// <exception cref="ArgumentException">No pictures are given.</exception>
    public ShownRoomWarning(ImmutableArray<PlacedWrittenPicture> pictures, RoomWarningDepth depth)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("A warning needs its pictures.", nameof(pictures));
        Pictures = pictures;
        Depth = Enum.IsDefined(depth) ? depth : throw new ArgumentOutOfRangeException(nameof(depth), depth, "Undefined warning depth.");
    }

    /// <summary>The pictures, in the order shown.</summary>
    public ImmutableArray<PlacedWrittenPicture> Pictures { get; }

    /// <summary>The depth the warning shows at.</summary>
    public RoomWarningDepth Depth { get; }
}
