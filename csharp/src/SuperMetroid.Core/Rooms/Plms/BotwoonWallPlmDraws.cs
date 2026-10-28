using static SuperMetroid.Core.Rooms.Plms.BotwoonWallPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The blocks and artwork name of Botwoon's wall.</summary>
internal static class BotwoonWallPlmDraws
{
    /// <summary>$84:9311-$9322: nine identical air blocks; indexed only within
    /// the single vertical run. Editable appearance is applied separately.</summary>
    internal static ushort LevelWordAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return PlmLevelWordDefinitions.BlankAir;
    }

    /// <summary>Assigns the lone wall-clear pointer its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(PlmDraw draw) => draw == Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>Materializes the nine-block draw record for asset tooling; gameplay evaluates the compiled fill directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var words = new ushort[BlockCount];
            for (int index = 0; index < words.Length; index++) words[index] = LevelWordAt(index);
            yield return new(Clear, new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(PlmRunDirection.Vertical, words, new(0, 0)),
            });
        }
    }
}
