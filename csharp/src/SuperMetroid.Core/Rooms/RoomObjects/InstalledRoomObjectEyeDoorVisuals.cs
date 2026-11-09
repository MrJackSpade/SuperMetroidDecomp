using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the eye doors' eye, middle, bottom and clearing frames (#1557); the opening clear shows the
/// shared clear's picture mirrored. Door terrain, attack logic, hit counters, timers and persistence stay the game's.
/// </summary>
public sealed class InstalledRoomObjectEyeDoorVisuals
{
    /// <summary>The installed pictures of every eye-door frame but the mirrored opening clear.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every eye-door frame but the mirrored opening clear.</summary>
    /// <param name="pictures">Each installed eye-door frame's cell pictures, keyed by frame.</param>
    internal InstalledRoomObjectEyeDoorVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of an eye-door frame; the opening clear shows the shared clear's, mirrored.</summary>
    /// <param name="draw">The eye-door frame.</param>
    /// <param name="blockIndex">Zero-based block ordinal within the component's single run: two eye blocks, one middle or bottom block, or four clearing blocks.</param>
    /// <returns>The block picture and its mirroring, the horizontal flip toggled for the opening clear.</returns>
    /// <exception cref="InvalidDataException">The draw is not an eye-door frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside the selected component's draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int blockIndex)
    {
        RoomObjectDraw visualSource = EyeDoorDraws.VisualSource(draw);
        if (!EyeDoorDraws.TryDescribe(visualSource, out var shape))
            throw new InvalidDataException($"Eye-door visuals lack frame {draw}.");
        if ((uint)blockIndex >= (uint)shape.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        RoomBlockAppearance appearance = pictures.At(visualSource, blockIndex);
        return draw == visualSource ? appearance : appearance.WithMirroring(appearance.Mirroring ^ BlockMirroring.Horizontal);
    }
}
