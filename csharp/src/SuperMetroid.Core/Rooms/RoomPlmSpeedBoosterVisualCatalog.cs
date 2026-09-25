namespace SuperMetroid.Core.Rooms;

/// <summary>The visible block selected by one Speed Booster terrain frame.</summary>
public sealed record RoomPlmSpeedBoosterVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable bomb-reveal appearance. The type-B collision word and PLM timing
/// remain in compiled cartridge definitions.
/// </summary>
public sealed class RoomPlmSpeedBoosterVisualCatalog
{
    private readonly ushort visualWord;

    public RoomPlmSpeedBoosterVisualCatalog(
        IEnumerable<RoomPlmSpeedBoosterVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        RoomPlmSpeedBoosterVisualEntry[] selected = entries.ToArray();
        if (selected.Length != 1 || selected[0] is null ||
            !SpeedBoosterBlockPlmDrawDefinitions.TryGetByVisualId(
                selected[0].Id, out _) ||
            selected[0].Blocks is not { Length: 1 } blocks ||
            !RoomLevelWord.IsValidVisualWord(blocks[0]))
            throw new InvalidDataException(
                "Speed Booster visuals require exactly one bomb-reveal visual block.");
        visualWord = blocks[0];
    }

    public static RoomPlmSpeedBoosterVisualCatalog Stock() => new(
        [new RoomPlmSpeedBoosterVisualEntry(
            SpeedBoosterBlockPlmDrawDefinitions.BombRevealVisualId,
            [new RoomLevelWord(SpeedBoosterBlockPlmDrawDefinitions.BombReveal
                .Runs.Span[0].LevelWords.Span[0]).VisualWord])]);

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (drawPointer != SpeedBoosterBlockPlmDrawDefinitions.BombReveal.Pointer ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Speed Booster visuals lack draw ${drawPointer:X4} run {runIndex} block {blockIndex}.");
        return visualWord;
    }
}
