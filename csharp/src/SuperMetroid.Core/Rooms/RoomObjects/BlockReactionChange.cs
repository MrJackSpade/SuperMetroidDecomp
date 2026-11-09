namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of a block reaction (#1554): how many updates pass before its first image, and its images in
/// order, each staying for its own updates. The reaction ends on the update after its last image's updates run out; a
/// reaction with no images ends on its first update.
/// </summary>
/// <param name="FirstFrameDelay">The updates from the reaction's spawn to its first image, at least one.</param>
/// <param name="Frames">The images in the order they show.</param>
internal sealed record BlockReactionChange(int FirstFrameDelay, IReadOnlyList<BlockReactionFrame> Frames)
{
    /// <summary>A reaction whose whole effect its spawn made: it shows nothing and leaves the room on its first update.</summary>
    internal static BlockReactionChange MadeAtSpawn { get; } = new(1, []);
}
