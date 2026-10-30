namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A room's grid in row-major block order (#633, #1469): each foreground block's terrain and behaviour, and what each
/// background block shows.
/// </summary>
public readonly ref struct RoomLevelPlanes
{
    /// <summary>Binds the three planes; <see cref="RoomLevelData"/> validates their lengths against the room size.</summary>
    /// <param name="foreground">Each foreground block's terrain.</param>
    /// <param name="behavior">Each foreground block's behaviour.</param>
    /// <param name="background">What each background block shows.</param>
    public RoomLevelPlanes(ReadOnlySpan<RoomTerrainBlock> foreground, ReadOnlySpan<RoomBlockBehavior> behavior,
        ReadOnlySpan<RoomBlockAppearance> background)
    {
        Foreground = foreground;
        Behavior = behavior;
        Background = background;
    }

    /// <summary>Each foreground block's terrain: what it shows and how it collides.</summary>
    public ReadOnlySpan<RoomTerrainBlock> Foreground { get; }

    /// <summary>Each foreground block's behaviour.</summary>
    public ReadOnlySpan<RoomBlockBehavior> Behavior { get; }

    /// <summary>What each background block shows.</summary>
    public ReadOnlySpan<RoomBlockAppearance> Background { get; }
}
