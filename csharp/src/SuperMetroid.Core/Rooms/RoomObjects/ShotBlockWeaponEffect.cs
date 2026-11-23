namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a weapon a shot block yields to does to it (<see cref="WeaponShotBlock.Weapons"/>).</summary>
internal enum ShotBlockWeaponEffect
{
    /// <summary>The weapon breaks the block.</summary>
    Breaks,

    /// <summary>The weapon shows the block's one-update reveal and leaves the live block untouched.</summary>
    ShowsReveal,
}
