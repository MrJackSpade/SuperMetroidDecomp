using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Pictures the Ceres floor animation frames (#1467): each frame's runs written over a room's floor map, as the picture of
/// the rectangle its runs span. A cell of that rectangle no run writes keeps the map's tile, which nothing else writes, so
/// the picture replaces exactly what the frame's write changes.
/// </summary>
internal static class CeresFloorDrawings
{
    /// <summary>The tiles of each cell <paramref name="frame"/>'s runs write, by floor cell.</summary>
    internal static IReadOnlyDictionary<int, byte> Cells(CeresFloorAnimationFrame frame)
    {
        if (!CeresFloorDrawingDefinitions.Frames.TryGetValue(frame, out IReadOnlyList<CeresFloorRunDefinition>? rows))
            throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");
        var cells = new Dictionary<int, byte>();
        foreach (CeresFloorRunDefinition row in rows)
        {
            int cell = row.Row * FloorSheetComposer.Side + row.Column;
            foreach (CeresFloorAtlasRun run in row.Tiles)
            {
                for (int offset = 0; offset < run.Length; offset++)
                    cells[cell++] = checked((byte)(run.FirstTile + run.Step * offset));
            }
        }
        return cells;
    }

    /// <summary>
    /// The picture of every frame over a room floor whose cells, row by row, are <paramref name="map"/>, showing
    /// <paramref name="tilePixels"/>' tiles. Frames drawn over one part of the floor write the same cells, and frames over
    /// different parts span rectangles apart.
    /// </summary>
    /// <exception cref="InvalidDataException">Two frames over one part write different cells, or two parts overlap.</exception>
    internal static IReadOnlyList<FloorSheetComposer.FloorPicture> Pictures(ReadOnlySpan<byte> map, byte[] tilePixels)
    {
        var pictures = new List<FloorSheetComposer.FloorPicture>();
        foreach (CeresFloorAnimationFrame frame in Enum.GetValues<CeresFloorAnimationFrame>())
            pictures.Add(Picture(frame, map, tilePixels));
        FloorSheetComposer.RequireSeparateParts(pictures);
        foreach (CeresFloorAnimationFrame first in Enum.GetValues<CeresFloorAnimationFrame>())
        {
            foreach (CeresFloorAnimationFrame second in Enum.GetValues<CeresFloorAnimationFrame>())
            {
                FloorSheetComposer.FloorPicture one = pictures.Single(picture => picture.Name == CeresFloorAnimation.PictureOf(first));
                FloorSheetComposer.FloorPicture other = pictures.Single(picture => picture.Name == CeresFloorAnimation.PictureOf(second));
                if (one.Column == other.Column && one.Row == other.Row && !Cells(first).Keys.ToHashSet().SetEquals(Cells(second).Keys))
                    throw new InvalidDataException($"Ceres floor frames {first} and {second} draw over one part but write different cells.");
            }
        }
        return pictures;
    }

    /// <summary>The picture of <paramref name="frame"/>'s runs written over <paramref name="map"/>.</summary>
    private static FloorSheetComposer.FloorPicture Picture(CeresFloorAnimationFrame frame, ReadOnlySpan<byte> map, byte[] tilePixels)
    {
        IReadOnlyDictionary<int, byte> written = Cells(frame);
        int left = written.Keys.Min(cell => cell % FloorSheetComposer.Side), right = written.Keys.Max(cell => cell % FloorSheetComposer.Side);
        int top = written.Keys.Min(cell => cell / FloorSheetComposer.Side), bottom = written.Keys.Max(cell => cell / FloorSheetComposer.Side);
        int columns = right - left + 1;
        var cells = new byte[columns * (bottom - top + 1)];
        for (int index = 0; index < cells.Length; index++)
        {
            int cell = (top + index / columns) * FloorSheetComposer.Side + left + index % columns;
            cells[index] = written.TryGetValue(cell, out byte tile) ? tile : map[cell];
        }
        return new FloorSheetComposer.FloorPicture(CeresFloorAnimation.PictureOf(frame), cells, columns, left, top, tilePixels);
    }
}
