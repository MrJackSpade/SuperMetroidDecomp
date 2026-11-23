namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How long each image of a breaking block stays (#2557).</summary>
/// <param name="Rock">The updates the rock stays.</param>
/// <param name="Pieces">The updates the pieces stay.</param>
/// <param name="Dust">The updates the dust stays.</param>
internal readonly partial record struct BlockBreakupPace(int Rock, int Pieces, int Dust)
{
    /// <summary>The pace nearly every block breaks and every block grows back at: four updates an image.</summary>
    internal static BlockBreakupPace Steady { get; } = new(4, 4, 4);
}
