using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The appearance of Crocomire's bridge and invisible-wall blocks: a block's selected visual word, from the authored
/// overrides or the compiled physical word, and the identity of the selected frames.
/// </summary>
internal static class CrocomireArenaArtwork
{
    /// <summary>Hashes every arena layout's selected visual words, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmCrocomireVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmCrocomireVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[24];
            foreach (var draw in CrocomireArenaDraws.All.OrderBy(item => CrocomireArenaDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                CrocomireArenaDraws.TryDescribe(draw.Draw, out var shape);
                int count = shape.RunCount * shape.WordsPerRun;
                for (int index = 0; index < count; index++)
                    words[index] = visuals.GetWord(draw.Draw, index / shape.WordsPerRun, index % shape.WordsPerRun);
                content.AppendName("frame", CrocomireArenaDraws.VisualId(draw.Draw));
                // Preserve the flattened-frame identity used by existing visual installations.
                content.Append("runs", 1);
                content.AppendWords("words", words[..count]);
            }
        });

    /// <summary>Resolves one arena-layout block, translating native run-local indices into the flattened visual override.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal static ushort Word(PlmVisualOverrides customWords, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = runIndex * shape.WordsPerRun + blockIndex;
        return customWords.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
