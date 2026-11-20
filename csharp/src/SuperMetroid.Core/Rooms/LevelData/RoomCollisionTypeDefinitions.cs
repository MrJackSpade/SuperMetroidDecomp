namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Each collision kind of a room's blocks.</summary>
internal static class RoomCollisionTypeDefinitions
{
    /// <summary>
    /// Each kind: what its blocks' behaviour describes and the reaction family they name, whether a bomb's spread passes
    /// through them (the air kinds but shootable air), and whether a bomb's blast leaves them alone (air and spike air,
    /// special and unrevealed air, slopes, and solid, door, spike and grapple blocks).
    /// </summary>
    internal static readonly IReadOnlyDictionary<RoomCollisionType, RoomCollisionTypeDefinition> Types =
        new Dictionary<RoomCollisionType, RoomCollisionTypeDefinition>
        {
            [RoomCollisionType.Air] = new(RoomCollisionBehaviorKind.None, null, BombsPass: true, BombLeavesAlone: true),
            [RoomCollisionType.Slope] = new(RoomCollisionBehaviorKind.Slope, null, BombsPass: false, BombLeavesAlone: true),
            [RoomCollisionType.SpikeAir] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Spike, BombsPass: true, BombLeavesAlone: true),
            [RoomCollisionType.SpecialAir] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.SpecialAir, BombsPass: true, BombLeavesAlone: true),
            [RoomCollisionType.ShootableAir] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Shootable, BombsPass: false, BombLeavesAlone: false),
            [RoomCollisionType.HorizontalExtension] = new(RoomCollisionBehaviorKind.Extension, null, BombsPass: false, BombLeavesAlone: false),
            [RoomCollisionType.UnrevealedAir] = new(RoomCollisionBehaviorKind.None, null, BombsPass: true, BombLeavesAlone: true),
            [RoomCollisionType.BombableAir] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Bombable, BombsPass: true, BombLeavesAlone: false),
            [RoomCollisionType.SolidBlock] = new(RoomCollisionBehaviorKind.None, null, BombsPass: false, BombLeavesAlone: true),
            [RoomCollisionType.DoorBlock] = new(RoomCollisionBehaviorKind.None, null, BombsPass: false, BombLeavesAlone: true),
            [RoomCollisionType.SpikeBlock] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Spike, BombsPass: false, BombLeavesAlone: true),
            [RoomCollisionType.SpecialBlock] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Special, BombsPass: false, BombLeavesAlone: false),
            [RoomCollisionType.ShootableBlock] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Shootable, BombsPass: false, BombLeavesAlone: false),
            [RoomCollisionType.VerticalExtension] = new(RoomCollisionBehaviorKind.Extension, null, BombsPass: false, BombLeavesAlone: false),
            [RoomCollisionType.GrappleBlock] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Grapple, BombsPass: false, BombLeavesAlone: true),
            [RoomCollisionType.BombableBlock] = new(RoomCollisionBehaviorKind.Reaction, BlockReactionFamily.Bombable, BombsPass: false, BombLeavesAlone: false),
        };
}
