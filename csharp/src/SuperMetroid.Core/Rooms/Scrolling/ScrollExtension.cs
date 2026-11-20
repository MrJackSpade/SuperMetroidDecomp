using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The collision type and offset a scroll extension block writes.</summary>
/// <param name="CollisionType">The collision type.</param>
/// <param name="Offset">The signed block offset the extension's behaviour names, -1 or 1.</param>
internal readonly record struct ScrollExtension(RoomCollisionType CollisionType, int Offset)
{
    /// <summary>The block one further than <paramref name="block"/> along the extension's run, away from the trigger it extends.</summary>
    /// <param name="block">A block of the run, or the trigger the run starts from.</param>
    internal Position16<RoomBlocks> Beyond(Position16<RoomBlocks> block) =>
        CollisionType == RoomCollisionType.HorizontalExtension ? block.WithX(block.X - Offset) : block.WithY(block.Y - Offset);
}
