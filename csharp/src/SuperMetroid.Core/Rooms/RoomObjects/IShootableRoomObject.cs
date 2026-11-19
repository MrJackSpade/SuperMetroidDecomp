namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A typed room object a projectile can strike at its block (#1554). A door, the n00b tube and Draygon's wall cannon keep
/// the strike until they take it (<see cref="Compatibility.StrikeAwaitingRoomObject{THit}"/>), which a door does only once
/// it takes shots (#2484); a later strike before then replaces it. Mother Brain's glass counts a strike as it lands.
/// </summary>
internal interface IShootableRoomObject
{
    /// <summary>A projectile of <paramref name="hit"/>'s family struck the object's block.</summary>
    void Strike(ResidentObjectProjectileHit hit);
}
