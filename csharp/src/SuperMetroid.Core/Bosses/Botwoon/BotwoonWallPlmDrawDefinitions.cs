using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>The bounded nine-block vertical wall-clear draw at $84:930F.</summary>
internal static class BotwoonWallPlmDrawDefinitions
{
    /// <summary><c>$84:930F</c>: clear nine consecutive wall blocks.</summary>
    internal const ushort ClearPointer = 0x930f;

    /// <summary>Number of air blocks written by the single vertical clear-wall run.</summary>
    internal const int BlockCount = 9;

    /// <summary>$84:9311-$9322: nine identical air blocks; indexed only within
    /// the single vertical run. Editable appearance is applied separately.</summary>
    internal static ushort LevelWordAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return 0x00ff;
    }

    /// <summary>Materializes the nine-block draw record for asset tooling; gameplay evaluates the compiled fill directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var words = new ushort[BlockCount];
            for (int index = 0; index < words.Length; index++) words[index] = LevelWordAt(index);
            yield return new(ClearPointer, new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(0x8000 | BlockCount, words, new(0, 0)),
            });
        }
    }

    /// <summary>Assigns the lone wall-clear pointer its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(ushort pointer) => pointer == ClearPointer
        ? "clear-wall"
        : throw new InvalidDataException(
            $"Botwoon wall draw ${pointer:X4} has no visual ID.");
}
