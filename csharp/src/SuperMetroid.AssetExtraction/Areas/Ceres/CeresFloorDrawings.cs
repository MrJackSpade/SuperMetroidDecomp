using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Pictures the Ceres floor animation frames (#1467): each frame's runs written over a room's floor map, as the picture of
/// the rectangle its runs span. A cell of that rectangle no run writes keeps the map's tile, which nothing else writes, so
/// the picture replaces exactly what the frame's write changes.
/// </summary>
internal static partial class CeresFloorDrawings
{
    /// <summary>The floor's animation frames in the order the floor sheet holds their pictures (#2567).</summary>
    private static readonly IReadOnlyList<CeresFloorActorPicture> FrameOrder = Ensure.EveryMemberOnce<CeresFloorActorPicture>(
    [
        CeresFloorActorPicture.ElevatorLight,
        CeresFloorActorPicture.ElevatorDark,
        CeresFloorActorPicture.BabyVertical,
        CeresFloorActorPicture.BabyRound,
        CeresFloorActorPicture.BabyHorizontal,
        CeresFloorActorPicture.WingsRaised,
        CeresFloorActorPicture.WingsLowered,
    ]);

    /// <summary>The tiles of each cell <paramref name="frame"/>'s runs write, by floor cell.</summary>
    internal static Dictionary<int, int> Cells(CeresFloorActorPicture frame)
    {
        if (!CeresFloorDrawingDefinitions.Frames.TryGetValue(frame, out IReadOnlyList<CeresFloorRunDefinition>? rows))
            throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");
        var cells = new Dictionary<int, int>();
        foreach (CeresFloorRunDefinition row in rows)
        {
            int cell = row.Row * FloorSheetComposer.Side + row.Column;
            foreach (CeresFloorAtlasRun run in row.Tiles)
            {
                for (int offset = 0; offset < run.Length; offset++)
                    cells[cell++] = run.FirstTile + run.Step * offset;
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
        foreach (CeresFloorActorPicture frame in FrameOrder)
            pictures.Add(Picture(frame, map, tilePixels));
        FloorSheetComposer.RequireSeparateParts(pictures);
        foreach (CeresFloorActorPicture first in FrameOrder)
        {
            foreach (CeresFloorActorPicture second in FrameOrder)
            {
                FloorSheetComposer.FloorPicture one = pictures.Single(picture => picture.Name == RoomFloorSheet.PictureNameOf(first));
                FloorSheetComposer.FloorPicture other = pictures.Single(picture => picture.Name == RoomFloorSheet.PictureNameOf(second));
                if (one.Column == other.Column && one.Row == other.Row && !Cells(first).Keys.ToHashSet().SetEquals(Cells(second).Keys))
                    throw new InvalidDataException($"Ceres floor frames {first} and {second} draw over one part but write different cells.");
            }
        }
        return pictures;
    }

    /// <summary>
    /// The picture each actor rests in on the floor whose cells, row by row, are <paramref name="map"/> (#2573): the first of
    /// its pictures the map shows where the actor stands; an actor whose place shows none of them does not rest on the floor.
    /// </summary>
    internal static IReadOnlyDictionary<CeresFloorActor, CeresFloorActorPicture> Resting(ReadOnlySpan<byte> map)
    {
        byte[] floor = map.ToArray();
        var resting = new Dictionary<CeresFloorActor, CeresFloorActorPicture>();
        foreach (CeresFloorActorPicture picture in FrameOrder)
        {
            CeresFloorActor actor = CeresFloorActors.Of(picture);
            if (!resting.ContainsKey(actor) && Cells(picture).All(pair => floor[pair.Key] == pair.Value))
                resting.Add(actor, picture);
        }
        return resting;
    }

    /// <summary>The floor cells <paramref name="picture"/> covers: its whole picture's rectangle (#2573).</summary>
    internal static IEnumerable<int> Covered(CeresFloorActorPicture picture)
    {
        IReadOnlyDictionary<int, int> written = Cells(picture);
        int left = written.Keys.Min(cell => cell % FloorSheetComposer.Side), right = written.Keys.Max(cell => cell % FloorSheetComposer.Side);
        int top = written.Keys.Min(cell => cell / FloorSheetComposer.Side), bottom = written.Keys.Max(cell => cell / FloorSheetComposer.Side);
        for (int row = top; row <= bottom; row++)
        {
            for (int column = left; column <= right; column++)
                yield return row * FloorSheetComposer.Side + column;
        }
    }

    /// <summary>The picture of <paramref name="frame"/>'s runs written over <paramref name="map"/>.</summary>
    internal static FloorSheetComposer.FloorPicture Picture(CeresFloorActorPicture frame, ReadOnlySpan<byte> map, byte[] tilePixels)
    {
        IReadOnlyDictionary<int, int> written = Cells(frame);
        int left = written.Keys.Min(cell => cell % FloorSheetComposer.Side), right = written.Keys.Max(cell => cell % FloorSheetComposer.Side);
        int top = written.Keys.Min(cell => cell / FloorSheetComposer.Side), bottom = written.Keys.Max(cell => cell / FloorSheetComposer.Side);
        int columns = right - left + 1;
        var cells = new byte[columns * (bottom - top + 1)];
        for (int index = 0; index < cells.Length; index++)
        {
            int cell = (top + index / columns) * FloorSheetComposer.Side + left + index % columns;
            cells[index] = written.TryGetValue(cell, out int tile) ? checked((byte)tile) : map[cell];
        }
        return new FloorSheetComposer.FloorPicture(RoomFloorSheet.PictureNameOf(frame), cells, columns, left, top, tilePixels);
    }
}
