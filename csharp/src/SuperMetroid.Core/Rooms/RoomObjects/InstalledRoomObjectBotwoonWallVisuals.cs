using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the Botwoon wall's nine-block clear (#1557). The clear's air terrain and the crumble program
/// stay the game's; the crumble frames are shot-block frames.
/// </summary>
public sealed class InstalledRoomObjectBotwoonWallVisuals
{
    /// <summary>The nine cell pictures of the clear, top to bottom, owned by these visuals.</summary>
    private readonly RoomBlockAppearance[] blocks;

    /// <summary>Holds the installed picture of the wall clear.</summary>
    /// <param name="blocks">The clear's nine cell pictures, top to bottom; copied.</param>
    /// <exception cref="ArgumentNullException"><paramref name="blocks"/> is null.</exception>
    /// <exception cref="InvalidDataException">There are not nine cell pictures.</exception>
    internal InstalledRoomObjectBotwoonWallVisuals(RoomBlockAppearance[] blocks)
    {
        this.blocks = [.. Ensure.NotNull(blocks)];
        if (this.blocks.Length != BotwoonWallObjectDrawDefinitions.BlockCount)
            throw new InvalidDataException($"The installed Botwoon wall clear has {this.blocks.Length} cells, not {BotwoonWallObjectDrawDefinitions.BlockCount}.");
    }

    /// <summary>The installed picture of one cell of the wall clear.</summary>
    /// <param name="draw">The Botwoon wall-clear draw.</param>
    /// <param name="runIndex">The sole clear-run ordinal, zero.</param>
    /// <param name="blockIndex">Zero-based block ordinal 0..8 within the nine-block clear run.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not the clear draw or the run ordinal is not zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The block ordinal is outside 0..8.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (draw != BotwoonWallObjectDrawDefinitions.Clear ||
            runIndex != 0)
            throw new InvalidDataException(
                $"Botwoon wall visuals lack draw {draw}, run {runIndex}.");
        if ((uint)blockIndex >= BotwoonWallObjectDrawDefinitions.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return blocks[blockIndex];
    }
}
