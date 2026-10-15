namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296), one per bank-$86 list a spawn or family routine enters, in authored order. See
/// <see cref="SaveStationElectricityAnimations"/>.
/// </summary>
internal enum SaveStationElectricityProgram
{
    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_0</c> at $86:E683: sets the cycle count.</summary>
    Start,
    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_1</c> at $86:E687: one eight-pose cycle, repeated, then deletes.</summary>
    Cycle,
}
