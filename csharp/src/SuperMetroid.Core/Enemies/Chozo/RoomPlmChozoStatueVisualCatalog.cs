using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Editable Chozo hand and slope-access appearances. The calculated draw geometry and full
/// level words remain independent of installed visual block references.
/// </summary>
public sealed class RoomPlmChozoStatueVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmChozoStatueVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[27];
        foreach (var frame in ChozoStatuePlmDrawDefinitions.All.OrderBy(item => ChozoStatuePlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            ChozoStatuePlmDrawDefinitions.TryDescribe(frame.Draw, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
                for (int word = 0; word < shape.WordCount(run); word++)
                    words[count++] = GetWord(frame.Draw, run, word);
            content.AppendName("frame", ChozoStatuePlmDrawDefinitions.VisualId(frame.Draw));
            // Preserve the flattened single-run framing of installed Chozo artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Visual-only overrides keyed by draw; absent when every selected block uses its compiled visual word.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete Chozo-layout coverage and copies authored visual differences without replacing physical level words or calculated run geometry.</summary>
    /// <param name="entries">One entry per compiled layout with its exact flattened block count and visual-only word payload.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, duplicates an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmChozoStatueVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Chozo statue", ChozoStatuePlmDrawDefinitions.TryGetByVisualId,
            ChozoStatuePlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one selected Chozo-layout visual word, projecting run-local indices into the flattened authored payload while preserving compiled mechanics.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a room coordinate or flattened entry index.</param>
    /// <returns>The metatile reference and parent flips; unchanged layouts derive these bits from compiled stock words.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Chozo layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!ChozoStatuePlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Chozo statue visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.WordCount(run);
        return customBlocks.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
