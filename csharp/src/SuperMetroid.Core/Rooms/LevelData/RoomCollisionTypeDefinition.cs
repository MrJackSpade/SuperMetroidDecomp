namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What one collision kind of a room's blocks is (<see cref="RoomCollisionTypeDefinitions"/>).</summary>
/// <param name="Behavior">What the kind's blocks' behaviour describes: nothing, a reaction, a slope or an extension.</param>
/// <param name="Reactions">The family of reactions the kind's blocks name, or none for a kind that names no reaction.</param>
/// <param name="BombsPass">Whether a bomb's spread passes through the kind's blocks rather than colliding with them.</param>
/// <param name="BombLeavesAlone">
/// Whether a bomb's blast only reports passing or stopping at the kind's blocks, spawning no room object and changing no
/// terrain.
/// </param>
internal sealed partial record RoomCollisionTypeDefinition(
    RoomCollisionBehaviorKind Behavior,
    BlockReactionFamily? Reactions,
    bool BombsPass,
    bool BombLeavesAlone);
