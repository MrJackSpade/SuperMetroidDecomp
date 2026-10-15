namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash programs (#1296), one per bank-$86 list a spawn or family routine enters, in authored order. See
/// <see cref="EyeDoorSweatAnimations"/>.
/// </summary>
internal enum EyeDoorSweatProgram
{
    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat_Falling</c> at $86:B615: the drop falls in one pose.</summary>
    Falling,
    /// <summary><c>InstList_EnemyProjectile_EyeDoorSweat_HitFloor</c> at $86:B61D: the drop splashes and deletes.</summary>
    HitFloor,
}
