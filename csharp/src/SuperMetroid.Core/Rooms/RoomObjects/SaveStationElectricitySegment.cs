using SuperMetroid.Core.Enemies.SaveStation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296): the cycle its discharge repeats and the deletion that
/// follows (#2276). See <see cref="SaveStationElectricityAnimations"/>.
/// </summary>
internal enum SaveStationElectricitySegment
{
    /// <summary>One eight-pose cycle, which the discharge repeats.</summary>
    Cycle,
    /// <summary>The deletion after the last cycle.</summary>
    Deletion,
}
