using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's fake-death room blocks: a block's selected visual word, from the authored
/// overrides or the compiled physical word, and the identity of the selected frames.
/// </summary>
internal static class MotherBrainFakeDeathArtwork
{
    /// <summary>Hashes every layout's selected visual words as one flattened frame, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmMotherBrainFakeDeathVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainFakeDeathVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[14];
            foreach (var draw in MotherBrainFakeDeathDraws.All.OrderBy(item => MotherBrainFakeDeathDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                int index = 0;
                for (int run = 0; run < draw.Runs.Length; run++)
                    for (int block = 0; block < draw.Runs.Span[run].LevelWords.Length; block++)
                        words[index++] = visuals.GetWord(draw.Draw, run, block);
                content.AppendName("frame", MotherBrainFakeDeathDraws.VisualId(draw.Draw));
                // Existing installations hash each draw as one flattened visual frame.
                content.Append("runs", 1);
                content.AppendWords("words", words[..index]);
            }
        });

    /// <summary>Resolves a fake-death room block's selected appearance, projecting native run-local indices into its flattened authored layout.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal static ushort Word(PlmVisualOverrides customWords, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (MotherBrainFakeDeathDraws.TryDescribeBackground(draw, out var background))
        {
            if (runIndex != 0 || (uint)blockIndex >= 13)
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords.TryGet(draw, out var selectedBackground)
                ? selectedBackground[blockIndex] : new RoomLevelWord(background.WordAt(blockIndex)).VisualWord;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeRegular(draw, out var regular))
        {
            if ((uint)runIndex >= regular.RunCount || (uint)blockIndex >= regular.Count(runIndex))
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords.TryGet(draw, out var selected)
                ? selected[(runIndex == 0 ? 0 : regular.Count(0)) + blockIndex]
                : new RoomLevelWord(regular.WordAt(runIndex, blockIndex)).VisualWord;
        }
        if (!MotherBrainFakeDeathDraws.TryDescribeBoundary(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain fake-death visuals lack draw {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= (uint)shape.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.Count(run);
        return customWords.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
