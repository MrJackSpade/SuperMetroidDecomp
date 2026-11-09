using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Reads one room-object family's installed frame pictures as the player left them (#1557): every entry names a frame the
/// family draws and keeps that frame's shape, no frame repeats and every drawn frame is installed. Every frame is kept as
/// installed; none is compared with or replaced by anything the game compiles.
/// </summary>
internal static class RoomObjectFramePictureReader
{
    /// <summary>Reads entries whose cells are flattened in run then cell order.</summary>
    /// <param name="entries">The installed entries.</param>
    /// <param name="family">The family, naming it in a rejection.</param>
    /// <param name="lookup">Resolves an entry's visual identity to the frame's draw list.</param>
    /// <param name="frameCount">The number of frames the entries must cover.</param>
    /// <returns>Every installed frame's pictures.</returns>
    /// <exception cref="ArgumentNullException">The entry sequence or lookup is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a frame, changes a frame's shape, or a frame is missing.</exception>
    internal static RoomObjectFramePictures Flattened(IEnumerable<RoomObjectVisualEntry> entries, string family,
        RoomObjectDrawListExport.VisualLookup lookup, int frameCount)
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(lookup);
        var frames = new Dictionary<RoomObjectDraw, RoomBlockAppearance[]>();
        foreach (RoomObjectVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null || !lookup(entry.Id, out DrawList draw) ||
                entry.Blocks.Length != CellCount(draw))
                throw new InvalidDataException($"{family} visuals changed a frame identity or frame shape.");
            if (!frames.TryAdd(draw.Draw, [.. entry.Blocks]))
                throw new InvalidDataException($"{family} visuals repeat frame {entry.Id}.");
        }
        return Complete(frames, family, frameCount);
    }

    /// <summary>Reads entries whose cells keep each run separate, flattening them in run then cell order.</summary>
    /// <param name="entries">The installed entries.</param>
    /// <param name="family">The family, naming it in a rejection.</param>
    /// <param name="lookup">Resolves an entry's visual identity to the frame's draw list.</param>
    /// <param name="frameCount">The number of frames the entries must cover.</param>
    /// <returns>Every installed frame's pictures.</returns>
    /// <exception cref="ArgumentNullException">The entry sequence or lookup is null.</exception>
    /// <exception cref="InvalidDataException">An entry or run is invalid, a frame repeats, a run's shape differs, or a frame is missing.</exception>
    internal static RoomObjectFramePictures RunByRun(IEnumerable<RoomObjectVisualRunEntry> entries, string family,
        RoomObjectDrawListExport.VisualLookup lookup, int frameCount)
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(lookup);
        var frames = new Dictionary<RoomObjectDraw, RoomBlockAppearance[]>();
        foreach (RoomObjectVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null || !lookup(entry.Id, out DrawList draw) ||
                entry.Runs.Length != draw.Runs.Length)
                throw new InvalidDataException($"{family} visuals changed a frame identity or frame shape.");
            var cells = new List<RoomBlockAppearance>();
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                RoomBlockAppearance[] appearances = entry.Runs[run];
                if (appearances is null || appearances.Length != draw.Runs.Span[run].Count)
                    throw new InvalidDataException($"{family} visuals {entry.Id} change a frame shape.");
                cells.AddRange(appearances);
            }
            if (!frames.TryAdd(draw.Draw, [.. cells]))
                throw new InvalidDataException($"{family} visuals repeat frame {entry.Id}.");
        }
        return Complete(frames, family, frameCount);
    }

    /// <summary>The frames, once every drawn frame is installed.</summary>
    /// <exception cref="InvalidDataException">A drawn frame is not installed.</exception>
    private static RoomObjectFramePictures Complete(Dictionary<RoomObjectDraw, RoomBlockAppearance[]> frames, string family, int frameCount) =>
        frames.Count == frameCount
            ? new RoomObjectFramePictures(family, frames)
            : throw new InvalidDataException($"{family} visuals install {frames.Count} of the {frameCount} frames the game draws.");

    /// <summary>The number of cells across a frame's runs.</summary>
    private static int CellCount(DrawList draw)
    {
        int count = 0;
        foreach (Run run in draw.Runs.Span) count += run.Count;
        return count;
    }
}
