namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of a block reaction whose setup saves no terrain (#1554, #2554): every image it shows is a draw at its
/// block, so it restores no block. A <see cref="LastingBlockReactionObject"/> plays it.
/// </summary>
/// <param name="FirstFrameDelay">The updates before the first image shows.</param>
/// <param name="Frames">The images in the order they show, each drawn for its own updates.</param>
internal sealed partial record LastingBlockReactionChange(int FirstFrameDelay, IReadOnlyList<LastingBlockReactionFrame> Frames)
{
    /// <summary>The reaction whose setup makes the whole change: it shows nothing and leaves on its first update.</summary>
    internal static LastingBlockReactionChange MadeAtSpawn { get; } = new(1, []);
}
