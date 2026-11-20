namespace SuperMetroid.Core.Enemies;

/// <summary>An entry that places its enemy at a room position.</summary>
/// <param name="Position">The enemy's initial room position in whole pixels, each coordinate wrapping around the room.</param>
public sealed record EnemyPlacedAt(Position16<RoomPixels> Position) : EnemyPlacing
{
    /// <summary>The place's whole position becomes the entry's; its fraction of a pixel stays.</summary>
    /// <param name="current">Where the place stands before the spawn.</param>
    internal override FixedPosition<RoomPixels> Place(FixedPosition<RoomPixels> current) => current.WithWhole(Position);
}
