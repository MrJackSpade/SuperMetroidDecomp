using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the bomb and contact-crumble restoration frames (#1557). The frames' terrain keeps its
/// linked parent and child collision.
/// </summary>
public sealed class InstalledRoomObjectLinkedRestoreVisuals
{
    /// <summary>The installed pictures of every restoration frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every restoration frame.</summary>
    /// <param name="pictures">Each restoration frame's cell pictures in run then cell order, keyed by frame.</param>
    internal InstalledRoomObjectLinkedRestoreVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a restoration frame.</summary>
    /// <param name="draw">The restoration frame.</param>
    /// <param name="runIndex">Zero-based run ordinal; square layouts have two runs and the other layouts one.</param>
    /// <param name="blockIndex">Block ordinal within the run, zero or one, not a flattened entry index.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a restoration frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!Describe(draw, out int runs))
            throw new InvalidDataException($"Linked restoration visuals lack frame {draw}.");
        if ((uint)runIndex >= runs || (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, runIndex * 2 + blockIndex);
    }

    /// <summary>The number of two-cell runs a bomb or contact-crumble restoration frame draws; false for any other draw.</summary>
    internal static bool Describe(RoomObjectDraw draw, out int runs)
    {
        bool found = LinkedRestoreObjectFamily.TryDescribeAny(draw, out var shape);
        runs = found ? shape.RunCount : 0;
        return found;
    }
}
