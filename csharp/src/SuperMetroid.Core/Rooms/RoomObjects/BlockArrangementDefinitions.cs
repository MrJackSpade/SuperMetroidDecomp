namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The blocks each <see cref="BlockArrangement"/> covers (#2484).</summary>
internal static partial class BlockArrangementDefinitions
{
    /// <summary>
    /// The blocks an arrangement covers besides its first, top-left one: none, the one right of it, the one below it, or
    /// both and the one diagonally between.
    /// </summary>
    internal static IReadOnlyDictionary<BlockArrangement, Offset16<RoomBlocks>[]> Children { get; } = new Dictionary<BlockArrangement, Offset16<RoomBlocks>[]>
    {
        [BlockArrangement.Single] = [],
        [BlockArrangement.Horizontal] = [new(1, 0)],
        [BlockArrangement.Vertical] = [new(0, 1)],
        [BlockArrangement.Square] = [new(1, 0), new(0, 1), new(1, 1)],
    };
}
