using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// One room level as the installation holds it (#1469): the grid's terrain, behaviours and background appearances, and
/// the terrain past the grid, decoded once by the import.
/// </summary>
public sealed class InstalledRoomLevel
{
    /// <summary>Validates and keeps one installed level.</summary>
    /// <param name="size">The grid's size in blocks: the room's width, and the rows its level holds.</param>
    /// <param name="foreground">Each foreground block's terrain, row by row.</param>
    /// <param name="behaviors">Each foreground block's behaviour, row by row.</param>
    /// <param name="background">What each background block shows, row by row.</param>
    /// <param name="following">The level data that followed the grid, read as terrain blocks.</param>
    /// <param name="beyondCount">How many blocks the room's level space holds past its grid; the rest past <paramref name="following"/> are solid.</param>
    /// <exception cref="InvalidDataException">A plane does not cover the grid, or the data past the grid exceeds its space.</exception>
    public InstalledRoomLevel(Extent16<RoomBlocks> size, ImmutableArray<RoomTerrainBlock> foreground,
        ImmutableArray<RoomBlockBehavior> behaviors, ImmutableArray<RoomBlockAppearance> background,
        ImmutableArray<RoomTerrainBlock> following, int beyondCount)
    {
        if (size.X <= 0 || size.Y <= 0)
            throw new InvalidDataException($"An installed room level needs a positive size; got {size.X}x{size.Y}.");
        int blocks = size.Area;
        if (foreground.Length != blocks || behaviors.Length != blocks || background.Length != blocks)
            throw new InvalidDataException($"An installed {size.X}x{size.Y} room level needs {blocks} blocks in each plane.");
        if (beyondCount < following.Length)
            throw new InvalidDataException(
                $"An installed room level holds {following.Length} blocks of data past its grid in a space of {beyondCount}.");
        Size = size;
        Foreground = foreground;
        Behaviors = behaviors;
        Background = background;
        Following = following;
        BeyondCount = beyondCount;
    }

    /// <summary>The grid's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>Each foreground block's terrain, row by row.</summary>
    public ImmutableArray<RoomTerrainBlock> Foreground { get; }

    /// <summary>Each foreground block's behaviour, row by row.</summary>
    public ImmutableArray<RoomBlockBehavior> Behaviors { get; }

    /// <summary>What each background block shows, row by row.</summary>
    public ImmutableArray<RoomBlockAppearance> Background { get; }

    /// <summary>The level data that followed the grid, read as terrain blocks.</summary>
    public ImmutableArray<RoomTerrainBlock> Following { get; }

    /// <summary>How many blocks the room's level space holds past its grid.</summary>
    public int BeyondCount { get; }
}
