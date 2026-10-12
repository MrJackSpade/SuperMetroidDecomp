namespace SuperMetroid.Core.Rooms;

/// <summary>The bounded nine-block vertical wall-clear draw at $84:930F.</summary>
internal static class BotwoonWallPlmDrawDefinitions
{
    /// <summary><c>$84:930F</c>: clear nine consecutive wall blocks.</summary>
    internal const ushort ClearPointer = 0x930f;

    /// <summary>Defines block count.</summary>
    internal const int BlockCount = 9;

    /// <summary>$84:9311-$9322: nine identical air blocks; indexed only within
    /// the single vertical run. Editable appearance is applied separately.</summary>
    internal static ushort LevelWordAt(int index)
    {
        if ((uint)index >= BlockCount) throw new IndexOutOfRangeException();
        return 0x00ff;
    }

    // Export DTOs are materialized only for asset tooling; gameplay evaluates the fill.
    /// <summary>Gets or sets all.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var words = new ushort[BlockCount];
            for (int index = 0; index < words.Length; index++) words[index] = LevelWordAt(index);
            yield return new(ClearPointer, new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new((ushort)(0x8000 | BlockCount), words, new(0, 0)),
            });
        }
    }

    /// <summary>Visual id.</summary>
    internal static string VisualId(ushort pointer) => pointer == ClearPointer
        ? "clear-wall"
        : throw new InvalidDataException(
            $"Botwoon wall draw ${pointer:X4} has no visual ID.");
}
