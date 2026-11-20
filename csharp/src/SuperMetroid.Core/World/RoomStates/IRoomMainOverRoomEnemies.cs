namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A room main whose work acts on one of the room's enemies (#2486, #2554): the room's load has it wait on that enemy
/// before the room's enemies load, after <see cref="RoomMainBehavior.Compose"/>, and the enemy's setup composes it. Only
/// Crocomire's room's rumble is one; every other main is whole from its composition.
/// </summary>
internal interface IRoomMainOverRoomEnemies
{
    /// <summary>Has the room <paramref name="scope"/> builds compose this main over <paramref name="enemies"/>.</summary>
    /// <param name="scope">The room being loaded.</param>
    /// <param name="enemies">The room's enemies, about to load.</param>
    void ComposeOverRoomEnemies(RoomScopeConstruction scope, RoomEnemySystem enemies);
}
