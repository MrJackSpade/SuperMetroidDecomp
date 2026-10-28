using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling block draws: which PLM draws are the ceiling's, the two-by-two square each fills from
/// <see cref="SporeSpawnCeilingPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class SporeSpawnCeilingDraws
{
    /// <summary>The one air cell a ceiling draw fills its square with.</summary>
    internal static bool TryGetWord(PlmDraw request, out RoomLevelWord word)
    {
        if (request is not PlmFrameDraw<SporeSpawnCeilingDraw>)
        {
            word = default;
            return false;
        }
        var list = PlmDraw.FrameOf<SporeSpawnCeilingDraw>(request, "SporeSpawnCeilingDraw");
        word = SporeSpawnCeilingPlmDrawDefinitions.Cells.TryGetValue(list, out RoomLevelWord cell)
            ? cell
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {list}.");
        return true;
    }

    /// <summary>Temporary draw records for the artwork interface; runtime draws the scalar two-by-two square directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SporeSpawnCeilingDraw frame in Enum.GetValues<SporeSpawnCeilingDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Selects a ceiling draw's published artwork key.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<SporeSpawnCeilingDraw>(request, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw frame) =>
        SporeSpawnCeilingPlmDrawDefinitions.VisualIds.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {frame}.");

    /// <summary>Finds the native physical square associated with an editable ceiling-frame key.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds a two-run physical square for a supported clear or crumble draw.</summary>
    internal static bool TryGet(PlmDraw request,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (TryGetWord(request, out RoomLevelWord word))
        {
            list = Square(request, word);
            return true;
        }
        list = default;
        return false;
    }

    /// <summary>Duplicates one physical tile across the two horizontal runs of the ceiling's two-by-two layout.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList Square(
        PlmDraw draw, RoomLevelWord physicalWord) => new(draw,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new RoomPlmShotBlockDrawDefinitions.Run(
                PlmRunDirection.Horizontal, new ushort[] { physicalWord.Raw, physicalWord.Raw }, new(0, 1)),
            new RoomPlmShotBlockDrawDefinitions.Run(
                PlmRunDirection.Horizontal, new ushort[] { physicalWord.Raw, physicalWord.Raw }, new(0, 0)),
        });
}
