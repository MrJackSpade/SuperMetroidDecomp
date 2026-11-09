using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>The appearance of Spore Spawn's ceiling: the installed picture a cell of a ceiling frame shows (#1557).</summary>
internal static class SporeSpawnCeilingArtwork
{
    /// <summary>The installed picture of one cell of a Spore Spawn ceiling frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four ceiling frames, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectFramePictures pictures, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!SporeSpawnCeilingDraws.TryGetBlock(draw, out _))
            throw new InvalidDataException(
                $"Spore Spawn ceiling visuals lack frame {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= 2)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, runIndex * 2 + blockIndex);
    }
}
