namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A typed room object a projectile can strike at its block (#1554). The object keeps the strike until it takes it, which
/// a door does only once it takes shots (#2484); a later strike before then replaces it.
/// </summary>
internal interface IShootableRoomObject
{
    /// <summary>A projectile of <paramref name="hit"/>'s family struck the object's block.</summary>
    void Strike(ResidentObjectProjectileHit hit);
}
