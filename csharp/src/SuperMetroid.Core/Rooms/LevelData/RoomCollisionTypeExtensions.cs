namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The families of collision kinds the game's collision tests ask about (#1452, #1469).</summary>
public static class RoomCollisionTypeExtensions
{
    /// <summary>
    /// Whether the kind is one of the eight solid kinds (solid, door, spike, special, shootable, vertical extension,
    /// grapple and bombable), which the game's collision tests admit together.
    /// </summary>
    public static bool IsSolidKind(this RoomCollisionType type) => type is RoomCollisionType.SolidBlock
        or RoomCollisionType.DoorBlock or RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock
        or RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or RoomCollisionType.GrappleBlock
        or RoomCollisionType.BombableBlock;
}
