namespace SuperMetroid.Core.Rooms;

/// <summary>The elevatube PLM's single editable visible block.</summary>
public sealed record RoomPlmMaridiaElevatubeVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable elevatube presentation. The physical block, hold, sound, and
/// deletion continue to use compiled cartridge definitions.
/// </summary>
public sealed class RoomPlmMaridiaElevatubeVisualCatalog
{
    private readonly ushort visualWord;

    public RoomPlmMaridiaElevatubeVisualCatalog(
        IEnumerable<RoomPlmMaridiaElevatubeVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RoomPlmMaridiaElevatubeVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !MaridiaElevatubePlmDefinitions.TryGetDrawByVisualId(
                selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks ||
            !RoomLevelWord.IsValidVisualWord(blocks[0]))
            throw new InvalidDataException(
                "Maridia elevatube visuals require exactly one visual block.");
        visualWord = blocks[0];
    }

    public static RoomPlmMaridiaElevatubeVisualCatalog Stock() => new(
        [new RoomPlmMaridiaElevatubeVisualEntry(
            MaridiaElevatubePlmDefinitions.VisualId,
            [new RoomLevelWord(MaridiaElevatubePlmDefinitions.Draw.Runs
                .Span[0].LevelWords.Span[0]).VisualWord])]);

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != MaridiaElevatubePlmDefinitions.DrawPointer ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Maridia elevatube visuals lack draw ${drawPointer:X4} run {runIndex} block {blockIndex}.");
        return visualWord;
    }
}
