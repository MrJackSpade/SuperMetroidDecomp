using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Editable appearance for the glass PLM. The compiled draw geometry, full
/// level words, hit thresholds, shards, and event timing remain fixed.
/// </summary>
public sealed class RoomPlmMotherBrainGlassVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainGlassVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[16];
        foreach (var frame in MotherBrainGlassPlmDrawDefinitions.All.OrderBy(item => MotherBrainGlassPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            MotherBrainGlassPlmDrawDefinitions.TryDescribe(frame.Draw, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
                for (int word = 0; word < shape.WordCount(run); word++)
                    words[count++] = GetWord(frame.Draw, run, word);
            content.AppendName("frame", MotherBrainGlassPlmDrawDefinitions.VisualId(frame.Draw));
            // Preserve the flattened single-run framing of installed glass artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Copied visual overrides by compiled glass-layout pointer; unchanged layouts use stock visual words.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all eleven glass layouts and copies visual differences without changing damage thresholds, shards, event timing, physical words, or signed draw offsets.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled glass identity, with the full flattened count of its native run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmMotherBrainGlassVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Mother Brain glass", MotherBrainGlassPlmDrawDefinitions.TryGetByVisualId,
            MotherBrainGlassPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one glass-layout visual block by translating native run-local indices into the flattened authored frame.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; supported layouts contain one through four runs.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored metatile/flip word, or the compiled physical word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported glass layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!MotherBrainGlassPlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame {draw}.");
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
