using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One of the rocks Kraid's growth knocks from the ceiling (#2400): where it falls and the ceiling block it breaks.</summary>
/// <param name="Crumble">The ceiling block the rock breaks, directly above where it falls, and how that block crumbles.</param>
public readonly record struct KraidCeilingRock(RoomObjectSpawnRequest<KraidRoomMutation> Crumble)
{
    /// <summary>The room X at which the rock falls: the centre of the block it breaks (#2557).</summary>
    public int X => Crumble.Block.ToRoomPixels().ToBlockCentre().X;
}
