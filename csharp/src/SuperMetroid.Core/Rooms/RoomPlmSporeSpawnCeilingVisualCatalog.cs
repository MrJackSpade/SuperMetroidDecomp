using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Spore Spawn ceiling frame in native draw-run order.</summary>
public sealed record RoomPlmSporeSpawnCeilingVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only block selections for the three crumble frames and final clear.
/// Compiled draw definitions retain their physical words and 2×2 geometry.
/// </summary>
public sealed class RoomPlmSporeSpawnCeilingVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmSporeSpawnCeilingVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var draw in SporeSpawnCeilingPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            for (int index = 0; index < words.Length; index++)
                words[index] = GetWord(draw.Pointer, index / 2, index % 2);
            content.Append("frame", draw.Pointer);
            // Preserve the original flattened frame identity.
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customWords;

    private RoomPlmSporeSpawnCeilingVisualCatalog() { }

    public RoomPlmSporeSpawnCeilingVisualCatalog(
        IEnumerable<RoomPlmSporeSpawnCeilingVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmSporeSpawnCeilingVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !SporeSpawnCeilingPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                !SporeSpawnCeilingPlmDrawDefinitions.TryGetWord(draw.Pointer, out ushort stockWord) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Spore Spawn ceiling visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Spore Spawn ceiling visuals repeat frame {entry.Id}.");
            if (entry.Blocks.Any(word => word != new RoomLevelWord(stockWord).VisualWord))
                selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != SporeSpawnCeilingPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Spore Spawn ceiling visuals do not cover all four compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Calculate each stock square from its physical draw word; retain only custom artwork.</summary>
    public static RoomPlmSporeSpawnCeilingVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!SporeSpawnCeilingPlmDrawDefinitions.TryGetWord(drawPointer, out ushort stockWord))
            throw new InvalidDataException(
                $"Spore Spawn ceiling visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[runIndex * 2 + blockIndex] : new RoomLevelWord(stockWord).VisualWord;
    }
}
