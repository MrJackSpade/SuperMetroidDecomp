using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only level words for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class RoomPlmSamusEaterVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmSamusEaterVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[8];
        foreach (var draw in SamusEaterPlmDrawDefinitions.All.OrderBy(item => SamusEaterPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            int index = 0;
            for (int run = 0; run < 3; run++)
                for (int block = 0; block < SamusEaterPlmDrawDefinitions.Draw.Count(run); block++)
                    words[index++] = GetWord(draw.Draw, run, block);
            content.AppendName("frame", SamusEaterPlmDrawDefinitions.VisualId(draw.Draw));
            // Preserve the original flattened-frame hash, not the three physical runs.
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    /// <summary>Owned visual-word overrides keyed by plant pose; null uses every stock appearance.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all eight three-run plant poses and copies authored visual differences without changing collision, run geometry, chewing timing, or Samus interaction.</summary>
    /// <param name="entries">Exactly one eight-word visual-only entry for each compiled floor and ceiling pose.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public RoomPlmSamusEaterVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customWords = new PlmVisualOverrides(entries, "Samus Eater", SamusEaterPlmDrawDefinitions.TryGetByVisualId,
            SamusEaterPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one selected plant block, translating native run-local indices into the flattened authored payload.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal 0..2.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the selected run.</param>
    /// <returns>The authored metatile/flip word, or the compiled physical word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported pose.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!SamusEaterPlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame {draw}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterPlmDrawDefinitions.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterPlmDrawDefinitions.Draw.Count(run);
        return customWords.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
