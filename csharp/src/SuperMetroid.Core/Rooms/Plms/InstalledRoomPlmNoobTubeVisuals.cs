using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Editable appearance for the tube PLM. The compiled draw geometry, full
/// level words, power-bomb gate, debris, event timing, and water physics remain fixed.
/// </summary>
public sealed class InstalledRoomPlmNoobTubeVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmNoobTubeVisuals), content =>
    {
        Span<ushort> words = stackalloc ushort[48];
        foreach (var frame in NoobTubePlmDraws.All.OrderBy(item => NoobTubePlmDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            NoobTubePlmDraws.TryDescribe(frame.Draw, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
                for (int word = 0; word < shape.WordCount(run); word++)
                    words[count++] = GetWord(frame.Draw, run, word);
            content.AppendName("frame", NoobTubePlmDraws.VisualId(frame.Draw));
            // Preserve the flattened single-run framing of installed tube artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Copied appearance edits for N00b-tube draw layouts that differ from stock visual bits.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all seven N00b-tube layouts and copies visual differences without changing power-bomb gating, debris, event timing, water physics, or physical draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, retaining the full flattened count of its single-block or twelve-block native runs.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public InstalledRoomPlmNoobTubeVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "N00b tube", NoobTubePlmDraws.TryGetByVisualId,
            NoobTubePlmDraws.All.Count());
    }

    /// <summary>Resolves one N00b-tube visual block by projecting native run-local indices into the flattened authored layout.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; layouts contain one, three, or four horizontal runs.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported N00b-tube layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!NoobTubePlmDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"N00b tube visuals lack frame {draw}.");
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
