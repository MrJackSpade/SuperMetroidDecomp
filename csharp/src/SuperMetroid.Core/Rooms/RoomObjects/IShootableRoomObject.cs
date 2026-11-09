namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A typed room object a projectile can strike at its block (#1554). The strike is kept until the object's next update
/// reads it; a later strike before then replaces it.
/// </summary>
internal interface IShootableRoomObject
{
    /// <summary>A projectile of <paramref name="hit"/>'s family struck the object's block.</summary>
    void Strike(ResidentObjectProjectileHit hit);
}
