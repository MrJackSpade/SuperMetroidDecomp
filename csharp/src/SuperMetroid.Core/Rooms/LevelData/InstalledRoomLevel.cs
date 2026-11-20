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
    /// <param name="pastGrid">What the level holds past its grid, which a probe past the grid finds.</param>
    /// <exception cref="InvalidDataException">A plane does not cover the grid.</exception>
    public InstalledRoomLevel(Extent16<RoomBlocks> size, ImmutableArray<RoomTerrainBlock> foreground,
        ImmutableArray<RoomBlockBehavior> behaviors, ImmutableArray<RoomBlockAppearance> background,
        LevelDataPastGrid<RoomTerrainBlock> pastGrid)
    {
        if (size.X <= 0 || size.Y <= 0)
            throw new InvalidDataException($"An installed room level needs a positive size; got {size.X}x{size.Y}.");
        int blocks = size.Area;
        if (foreground.Length != blocks || behaviors.Length != blocks || background.Length != blocks)
            throw new InvalidDataException($"An installed {size.X}x{size.Y} room level needs {blocks} blocks in each plane.");
        Size = size;
        Foreground = foreground;
        Behaviors = behaviors;
        Background = background;
        PastGrid = Ensure.NotNull(pastGrid);
    }

    /// <summary>The grid's size in blocks.</summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>Each foreground block's terrain, row by row.</summary>
    public ImmutableArray<RoomTerrainBlock> Foreground { get; }

    /// <summary>Each foreground block's behaviour, row by row.</summary>
    public ImmutableArray<RoomBlockBehavior> Behaviors { get; }

    /// <summary>What each background block shows, row by row.</summary>
    public ImmutableArray<RoomBlockAppearance> Background { get; }

    /// <summary>What the level holds past its grid, which a probe past the room's grid finds.</summary>
    public LevelDataPastGrid<RoomTerrainBlock> PastGrid { get; }
}
