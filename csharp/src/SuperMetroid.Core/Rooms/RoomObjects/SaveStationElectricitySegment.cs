using SuperMetroid.Core.Enemies.SaveStation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296), one per list a spawn or family step enters, in authored order. See
/// <see cref="SaveStationElectricityAnimations"/>.
/// </summary>
internal enum SaveStationElectricitySegment
{
    /// <summary>Sets the cycle count.</summary>
    Start,
    /// <summary>One eight-pose cycle, repeated, then deletes.</summary>
    Cycle,
}
