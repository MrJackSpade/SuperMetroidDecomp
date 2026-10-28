using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Four native two-by-two physical ceiling draws. Each list has two
/// horizontal runs, calculated without a stored layout cache. Editable visible
/// block selections remain a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{

    /// <summary>
    /// Clear fills with the blank tile; crumble advances through tiles $53..55
    /// in its three successive sixteen-byte records. Every cell uses the same air word.
    /// </summary>
    internal static bool TryGetWord(PlmDraw request, out RoomLevelWord word)
    {
        if (request is not PlmFrameDraw<SporeSpawnCeilingDraw>)
        {
            word = default;
            return false;
        }
        var list = PlmDraw.FrameOf<SporeSpawnCeilingDraw>(request, "SporeSpawnCeilingDraw");
        word = new RoomLevelWord(list switch
        {
            SporeSpawnCeilingDraw.Clear => 0x00ff,
            SporeSpawnCeilingDraw.CrumbleFirst => 0x53,
            SporeSpawnCeilingDraw.CrumbleSecond => 0x54,
            SporeSpawnCeilingDraw.CrumbleThird => 0x55,
            _ => throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {list}."),
        });
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
    internal static string VisualId(SporeSpawnCeilingDraw frame) => frame switch
    {
        SporeSpawnCeilingDraw.Clear => "clear-ceiling",
        SporeSpawnCeilingDraw.CrumbleFirst => "crumble-frame-0",
        SporeSpawnCeilingDraw.CrumbleSecond => "crumble-frame-1",
        SporeSpawnCeilingDraw.CrumbleThird => "crumble-frame-2",
        _ => throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {frame}."),
    };

    /// <summary>Finds the native physical square associated with an editable ceiling-frame key.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds a two-run physical square for a supported clear or crumble draw.</summary>
    internal static bool TryGet(PlmDraw request,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (TryGetWord(request, out RoomLevelWord word))
        {
            list = Square(request, checked((ushort)word));
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
