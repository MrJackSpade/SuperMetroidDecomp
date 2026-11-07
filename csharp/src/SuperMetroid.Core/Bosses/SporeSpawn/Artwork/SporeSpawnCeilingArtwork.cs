using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The appearance of Spore Spawn's ceiling: a cell's selected appearance, from the authored overrides or the compiled
/// terrain block, and the identity of the selected frames.
/// </summary>
internal static class SporeSpawnCeilingArtwork
{
    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectVisualOverrides customAppearances, RoomObjectDraw draw, int runIndex, int blockIndex)
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
