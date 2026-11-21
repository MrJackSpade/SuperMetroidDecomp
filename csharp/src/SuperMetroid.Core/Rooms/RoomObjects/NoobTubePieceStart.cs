namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How a piece of the broken n00b tube starts moving: the crack and the released-air bubbles start the animation that
/// begins at a segment, and each glass shard starts its own flight (#2044). The tube's enemy-side behaviour turns it into
/// the animation the piece plays.
/// </summary>
internal abstract partial record NoobTubePieceStart
{
    /// <summary>Only the two cases below start a piece.</summary>
    private NoobTubePieceStart()
    {
    }

    /// <summary>A piece that starts the animation beginning at <paramref name="Segment"/>: the crack or a bubble.</summary>
    /// <param name="Segment">The segment the piece's animation begins at.</param>
    internal sealed partial record AtSegment(NoobTubeProjectileSegment Segment) : NoobTubePieceStart;

    /// <summary>A glass shard, which starts its own flight.</summary>
    /// <param name="Shard">The shard whose flight it starts.</param>
    internal sealed partial record ShardFlight(NoobTubeShard Shard) : NoobTubePieceStart;
}
