using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The appearance of Crocomire's bridge and invisible-wall blocks: a block's selected appearance, from the authored
/// overrides or the compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class CrocomireArenaArtwork
{
    /// <summary>Hashes every arena layout's selected appearances, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmCrocomireVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmCrocomireVisualCatalog), content =>
        {
            var appearances = new RoomBlockAppearance[24];
            foreach (var draw in CrocomireArenaDraws.All.OrderBy(item => CrocomireArenaDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                CrocomireArenaDraws.TryDescribe(draw.Draw, out var shape);
                int count = shape.RunCount * shape.BlocksPerRun;
                for (int index = 0; index < count; index++)
                    appearances[index] = visuals.GetAppearance(draw.Draw, index / shape.BlocksPerRun, index % shape.BlocksPerRun) ??
                        throw new InvalidDataException($"Crocomire visuals lack draw {draw.Draw}.");
                content.AppendName("frame", CrocomireArenaDraws.VisualId(draw.Draw));
                // Preserve the flattened-frame identity used by existing visual installations.
                content.Append("runs", 1);
                content.AppendAppearances("words", appearances.AsSpan(0, count));
            }
        });

    /// <summary>Resolves one arena-layout block, translating native run-local indices into the flattened visual override.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customAppearances, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = runIndex * shape.BlocksPerRun + blockIndex;
        return customAppearances.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
