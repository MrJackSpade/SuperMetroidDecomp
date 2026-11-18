namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How the blocks a shot-block, bomb-block or contact-crumble draw covers lie (#1862, #2484): one block, two side by side,
/// two stacked, or a two-by-two square. The blocks each covers are <see cref="BlockArrangementDefinitions.Children"/>.
/// </summary>
internal enum BlockArrangement
{
    /// <summary>One block.</summary>
    Single,
    /// <summary>Two blocks side by side.</summary>
    Horizontal,
    /// <summary>Two blocks stacked.</summary>
    Vertical,
    /// <summary>A two-by-two square.</summary>
    Square,
}
