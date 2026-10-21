using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Editable appearance for the cannon PLM. The compiled draw geometry, full
/// level words, hit thresholds, damage transitions, and control writes remain fixed.
/// </summary>
public sealed class RoomPlmDraygonCannonVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmDraygonCannonVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in DraygonCannonPlmDrawDefinitions.All.OrderBy(item => DraygonCannonPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            DraygonCannonPlmDrawDefinitions.TryDescribe(frame.Draw, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
                for (int word = 0; word < shape.WordCount(run); word++)
                    words[count++] = GetWord(frame.Draw, run, word);
            content.AppendName("frame", DraygonCannonPlmDrawDefinitions.VisualId(frame.Draw));
            // Preserve the flattened single-run framing of installed cannon artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Owned edited visual words by canonical frame pointer; null retains compiled stock appearance.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete reachable cannon-frame coverage and copies visual differences without changing shield damage, hit thresholds, control writes, or draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled frame with the exact flattened count of all its run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves reachable frame coverage incomplete.</exception>
    public RoomPlmDraygonCannonVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Draygon cannon", DraygonCannonPlmDrawDefinitions.TryGetByVisualId,
            DraygonCannonPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves a cannon block's selected appearance by projecting native run-local indices into the flattened authored frame.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the frame's native draw run.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index.</param>
    /// <returns>The selected metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!DraygonCannonPlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Draygon cannon visuals lack frame {draw}.");
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
