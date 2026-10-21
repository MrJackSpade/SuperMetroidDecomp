using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete physical level words, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmCrocomireVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[24];
        foreach (var draw in CrocomireArenaPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            CrocomireArenaPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int count = shape.RunCount * shape.WordsPerRun;
            for (int index = 0; index < count; index++)
                words[index] = GetWord(draw.Pointer, index / shape.WordsPerRun, index % shape.WordsPerRun);
            content.Append("frame", draw.Pointer);
            // Preserve the flattened-frame identity used by existing visual installations.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Copied visual-only differences for the five compiled Crocomire arena layouts.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all five Crocomire arena layouts and copies visual differences without changing physical level words, run geometry, or PLM programs.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled draw identity with its full flattened block count.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmCrocomireVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customWords = new PlmVisualOverrides(entries, "Crocomire", CrocomireArenaPlmDrawDefinitions.TryGetByVisualId,
            CrocomireArenaPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one arena-layout block, translating native run-local indices into the flattened visual override.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported Crocomire arena draw layout.</param>
    /// <param name="runIndex">Zero-based ordinal of the compiled draw run.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The selected metatile/flip word, derived from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)blockIndex >= (uint)draw.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = runIndex * draw.WordsPerRun + blockIndex;
        return customWords.TryGet(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
