using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's escape gate: a block's selected visual word, from the authored overrides or the
/// compiled physical word, and the identity of the selected frames.
/// </summary>
internal static class EscapeGateArtwork
{
    /// <summary>Hashes every gate frame's selected visual words, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmEscapeGateVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmEscapeGateVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[4];
            foreach (var frame in MotherBrainEscapeGateDraws.All.OrderBy(item => MotherBrainEscapeGateDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int row = 0; row < words.Length; row++) words[row] = visuals.GetWord(frame.Draw, row);
                content.AppendName("frame", MotherBrainEscapeGateDraws.VisualId(frame.Draw));
                content.Append("runs", 1);
                content.AppendWords("words", words);
            }
        });

    /// <summary>Resolves one escape-gate block's selected appearance independently of its compiled solid collision word.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported escape-gate frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row ordinal is outside the four-block run.</exception>
    internal static ushort Word(PlmVisualOverrides customBlocks, PlmDraw draw, int blockIndex)
    {
        if (!MotherBrainEscapeGateDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Escape-gate visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var words)
            ? words[blockIndex] : new RoomLevelWord(shape.WordAt(blockIndex)).VisualWord;
    }
}
