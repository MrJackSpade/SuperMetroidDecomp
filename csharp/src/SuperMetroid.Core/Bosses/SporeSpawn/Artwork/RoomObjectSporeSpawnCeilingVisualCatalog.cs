using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The installed pictures of Spore Spawn's ceiling frames: the three crumble frames and the final clear (#1557). The
/// frames' terrain and two-by-two geometry stay the game's.
/// </summary>
public sealed class RoomObjectSporeSpawnCeilingVisualCatalog
{
    /// <summary>The installed pictures of the four ceiling frames.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every ceiling frame.</summary>
    /// <param name="pictures">Each ceiling frame's four cell pictures, keyed by frame.</param>
    internal RoomObjectSporeSpawnCeilingVisualCatalog(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a Spore Spawn ceiling frame.</summary>
    /// <param name="draw">The ceiling frame.</param>
    /// <param name="runIndex">Horizontal row index: zero for the top row, one for the bottom.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within that two-cell row.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four ceiling frames, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        SporeSpawnCeilingArtwork.Appearance(pictures, draw, runIndex, blockIndex);
}
