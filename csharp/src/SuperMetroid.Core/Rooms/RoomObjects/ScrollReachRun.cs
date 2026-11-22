using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A straight run of a scroll trigger's reach (#2557): how many blocks one extension write carries it on.</summary>
/// <param name="Extension">The extension each block of the run writes, which also says the way the run goes.</param>
/// <param name="Blocks">How many blocks the run covers; one or more.</param>
internal readonly partial record struct ScrollReachRun(ScrollExtension Extension, int Blocks)
{
    /// <summary>How many blocks the run covers; one or more.</summary>
    public int Blocks { get; } = Blocks >= 1 ? Blocks : throw new ArgumentOutOfRangeException(nameof(Blocks), Blocks, "A run covers one block or more.");
}
