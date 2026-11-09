using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The installed pictures of the Tourian access floor's frames: the empty row, the crumble rows and the six-row clear
/// (#1557). The frames' terrain and run offsets stay the game's.
/// </summary>
public sealed class RoomObjectTourianAccessVisuals
{
    /// <summary>Holds the pictures the import made of every access-floor frame.</summary>
    /// <param name="images">Every access-floor frame's picture, keyed by its draw.</param>
    internal RoomObjectTourianAccessVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every access-floor frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        RoomObjectDraw.TryFrame<TourianAccessDraw>(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Tourian access visuals lack frame {draw}.");
}
