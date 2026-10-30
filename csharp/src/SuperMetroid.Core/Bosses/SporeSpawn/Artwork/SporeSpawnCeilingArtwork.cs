using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The appearance of Spore Spawn's ceiling: a cell's selected appearance, from the authored overrides or the compiled
/// terrain block, and the identity of the selected frames.
/// </summary>
internal static class SporeSpawnCeilingArtwork
{
    /// <summary>Hashes every ceiling frame's four selected appearances as one flattened frame, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmSporeSpawnCeilingVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmSporeSpawnCeilingVisualCatalog), content =>
        {
            var appearances = new RoomBlockAppearance[4];
            foreach (var draw in SporeSpawnCeilingDraws.All.OrderBy(item => SporeSpawnCeilingDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                for (int index = 0; index < appearances.Length; index++)
                    appearances[index] = visuals.GetAppearance(draw.Draw, index / 2, index % 2) ??
                        throw new InvalidDataException($"Spore Spawn ceiling visuals lack frame {draw.Draw}.");
                content.AppendName("frame", SporeSpawnCeilingDraws.VisualId(draw.Draw));
                // Preserve the original flattened frame identity.
                content.Append("runs", 1);
                content.AppendAppearances("words", appearances);
            }
        });

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customAppearances, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!SporeSpawnCeilingDraws.TryGetBlock(draw, out RoomTerrainBlock stock))
            throw new InvalidDataException(
                $"Spore Spawn ceiling visuals lack frame {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customAppearances.TryGet(draw, out var appearances)
            ? appearances[runIndex * 2 + blockIndex] : stock.Appearance;
    }
}
