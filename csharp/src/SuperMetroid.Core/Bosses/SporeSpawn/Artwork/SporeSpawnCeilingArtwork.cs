using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The appearance of Spore Spawn's ceiling: a cell's selected visual word, from the authored overrides or the compiled
/// physical word, and the identity of the selected frames.
/// </summary>
internal static class SporeSpawnCeilingArtwork
{
    /// <summary>Hashes every ceiling frame's four selected visual words as one flattened frame, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmSporeSpawnCeilingVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmSporeSpawnCeilingVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[4];
            foreach (var draw in SporeSpawnCeilingDraws.All.OrderBy(item => SporeSpawnCeilingDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int index = 0; index < words.Length; index++)
                    words[index] = visuals.GetWord(draw.Draw, index / 2, index % 2);
                content.AppendName("frame", SporeSpawnCeilingDraws.VisualId(draw.Draw));
                // Preserve the original flattened frame identity.
                content.Append("runs", 1);
                content.AppendWords("words", words);
            }
        });

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one.</exception>
    internal static ushort Word(PlmVisualOverrides customWords, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!SporeSpawnCeilingDraws.TryGetWord(draw, out RoomLevelWord stockWord))
            throw new InvalidDataException(
                $"Spore Spawn ceiling visuals lack frame {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customWords.TryGet(draw, out var words)
            ? words[runIndex * 2 + blockIndex] : stockWord.VisualWord;
    }
}
