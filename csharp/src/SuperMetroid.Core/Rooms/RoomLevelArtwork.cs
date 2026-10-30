using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Replaceable initial appearances of one room level's foreground and background blocks. Terrain, collision and
/// behaviours stay in the room's installed level and are not part of the artwork.
/// </summary>
public sealed class RoomLevelArtwork
{
    /// <summary>What each foreground block initially shows, independent of its terrain.</summary>
    private readonly RoomBlockAppearance[] foreground;
    /// <summary>What each background block shows.</summary>
    private readonly RoomBlockAppearance[] background;

    /// <summary>Copies and validates both initial planes for one room level.</summary>
    /// <param name="level">The level the room states share.</param>
    /// <param name="size">Positive plane size in 16-pixel blocks: the width is the row stride and each plane holds width times height blocks.</param>
    /// <param name="foreground">What each foreground block initially shows, row by row.</param>
    /// <param name="background">What each background block shows, row by row.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is nonpositive.</exception>
    /// <exception cref="InvalidDataException">A plane has the wrong block count.</exception>
    public RoomLevelArtwork(RoomLevelId level, Extent16<RoomBlocks> size,
        ReadOnlySpan<RoomBlockAppearance> foreground,
        ReadOnlySpan<RoomBlockAppearance> background)
    {
        Ensure.NotNull(level.Name);
        if (size.X <= 0 || size.Y <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Room layout dimensions must be positive.");
        int expected = size.Area;
        if (foreground.Length != expected || background.Length != expected)
            throw new InvalidDataException(
                $"Room layout {level} needs {expected} blocks in each plane.");

        Level = level;
        Size = size;
        this.foreground = foreground.ToArray();
        this.background = background.ToArray();
    }

    /// <summary>The level the room states using the same initial artwork share.</summary>
    public RoomLevelId Level { get; }
    /// <summary>Size of each plane in 16-pixel blocks: the width is the row stride.</summary>
    public Extent16<RoomBlocks> Size { get; }
    /// <summary>What each foreground block initially shows, row by row, over the room's unchanged terrain.</summary>
    public ReadOnlyMemory<RoomBlockAppearance> Foreground => foreground;
    /// <summary>What each background block shows, row by row.</summary>
    public ReadOnlyMemory<RoomBlockAppearance> Background => background;
}
