namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The families of block collision types the collision routines test (#1452).</summary>
internal static class RoomCollisionTypeExtensions
{
    /// <summary>
    /// Whether the type is one of the eight solid block types (solid, door, spike, special, shootable, vertical
    /// extension, Grapple and bombable), which the cartridge's collision routines admit together as the upper half of the
    /// collision types.
    /// </summary>
    internal static bool IsSolidBlockType(this RoomCollisionType type) => type is RoomCollisionType.SolidBlock
        or RoomCollisionType.DoorBlock or RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock
        or RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or RoomCollisionType.GrappleBlock
        or RoomCollisionType.BombableBlock;
}
