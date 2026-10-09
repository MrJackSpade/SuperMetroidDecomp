using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Spore Spawn ceiling frame in native draw-run order.</summary>
/// <param name="Id">Case-sensitive compiled identity: <c>clear-ceiling</c> or <c>crumble-frame-0</c> through <c>crumble-frame-2</c>.</param>
/// <param name="Blocks">Four mutable input visual words in row-major order across the two-by-two 16-by-16-block square; only the ten-bit metatile index and two parent flip bits are allowed.</param>
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

    /// <summary>Validates complete coverage of the clear and three crumble frames, capturing edited ceiling appearance without changing compiled geometry or collision.</summary>
    /// <param name="entries">Each of the four compiled visual identities exactly once, with four valid presentation-only words per frame.</param>
    /// <remarks>Edited arrays are copied; stock-identical frames use compiled visual words, so later edits to caller arrays cannot change this catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">A frame is missing, duplicated, null, unknown, incorrectly shaped, or contains collision bits in its visual words.</exception>
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

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <param name="drawPointer">Bank-$84 pointer $9413 for clear, or $9423/$9433/$9443 for successive crumble frames.</param>
    /// <param name="runIndex">Horizontal row index: zero for the top row, one for the bottom.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within that two-cell row.</param>
    /// <returns>A twelve-bit metatile/parent-flip word without the separately compiled collision nibble.</returns>
    /// <exception cref="InvalidDataException"><paramref name="drawPointer"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one; the reported parameter is <paramref name="blockIndex"/>.</exception>
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
