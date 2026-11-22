namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>What a scroll extension block writes: an extension leading one block back toward the trigger it extends.</summary>
/// <param name="TowardTrigger">The way back to the trigger.</param>
internal readonly partial record struct ScrollExtension(ScrollExtensionDirection TowardTrigger)
{
    /// <summary>Whether the extension runs along a row, its trigger to its left or right, rather than down a column.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    internal bool Horizontal => TowardTrigger switch
    {
        ScrollExtensionDirection.Left or ScrollExtensionDirection.Right => true,
        ScrollExtensionDirection.Up or ScrollExtensionDirection.Down => false,
        _ => throw new ArgumentOutOfRangeException(nameof(TowardTrigger), TowardTrigger, "Undefined scroll extension direction."),
    };

    /// <summary>The blocks the extension steps along its axis toward its trigger: back for left and up, forward for right and down.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    internal int BlocksTowardTrigger => TowardTrigger switch
    {
        ScrollExtensionDirection.Left or ScrollExtensionDirection.Up => -1,
        ScrollExtensionDirection.Right or ScrollExtensionDirection.Down => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(TowardTrigger), TowardTrigger, "Undefined scroll extension direction."),
    };

    /// <summary>The block one further than <paramref name="block"/> along the extension's run, away from the trigger it extends.</summary>
    /// <param name="block">A block of the run, or the trigger the run starts from.</param>
    internal Position16<RoomBlocks> Beyond(Position16<RoomBlocks> block) =>
        Horizontal ? block.WithX(block.X - BlocksTowardTrigger) : block.WithY(block.Y - BlocksTowardTrigger);
}
