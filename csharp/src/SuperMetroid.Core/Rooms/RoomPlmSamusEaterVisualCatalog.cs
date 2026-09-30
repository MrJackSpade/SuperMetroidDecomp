using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One replaceable Samus Eater block appearance in native run order.</summary>
public sealed record RoomPlmSamusEaterVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only level words for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class RoomPlmSamusEaterVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromWordFrames(nameof(RoomPlmSamusEaterVisualCatalog), blocks);

    private readonly Dictionary<ushort, ushort[]> blocks;

    public RoomPlmSamusEaterVisualCatalog(
        IEnumerable<RoomPlmSamusEaterVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmSamusEaterVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !SamusEaterPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Samus Eater visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Samus Eater visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != SamusEaterPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Samus Eater visuals do not cover all eight compiled frames.");
        blocks = selected;
    }

    public static RoomPlmSamusEaterVisualCatalog Stock() => new(
        SamusEaterPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmSamusEaterVisualEntry(
                SamusEaterPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !SamusEaterPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
