using SuperMetroid.Core.Enemies.SaveStation;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296), one per list a spawn or family routine enters, in authored order. See
/// <see cref="SaveStationElectricityAnimations"/>.
/// </summary>
internal enum SaveStationElectricityProgram
{
    /// <summary>Sets the cycle count.</summary>
    Start,
    /// <summary>One eight-pose cycle, repeated, then deletes.</summary>
    Cycle,
}
