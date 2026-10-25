namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The identity of one room level: the initial blocks, block types and background blocks a room state loads. Room states
/// that share a level name the same identity; the installed level stream and visual layout are keyed by it.
/// </summary>
/// <param name="Name">The level's name, which its installed files are named by.</param>
public readonly record struct RoomLevelId(string Name)
{
    /// <summary>The level's name.</summary>
    public string Name { get; } = Ensure.NotNull(Name);

    /// <summary>The level's name.</summary>
    public override string ToString() => Name;
}
