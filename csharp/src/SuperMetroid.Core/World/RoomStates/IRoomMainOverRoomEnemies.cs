namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A room main whose work acts on one of the room's enemies (#2486): the room's load composes it with that enemy once the
/// room's enemies have loaded and set it up, after <see cref="RoomMainBehavior.Compose"/>. Only Crocomire's room's rumble
/// is one; every other main is whole from its composition.
/// </summary>
internal interface IRoomMainOverRoomEnemies
{
    /// <summary>Has the room <paramref name="scope"/> builds compose this main over <paramref name="enemies"/>.</summary>
    /// <param name="scope">The room being loaded.</param>
    /// <param name="enemies">The room's enemies, just loaded.</param>
    void ComposeOverRoomEnemies(RoomScopeConstruction scope, RoomEnemySystem enemies);
}
