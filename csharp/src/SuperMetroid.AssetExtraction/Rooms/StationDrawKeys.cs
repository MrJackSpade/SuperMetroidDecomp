using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each station frame (<see cref="StationDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class StationDrawKeys
{
    /// <summary>Each StationDraw member's installed key.</summary>
    internal static InstalledKeys<StationDraw> Installed { get; } = new(new Dictionary<StationDraw, string>
    {
        [StationDraw.MapDim] = "map-dim",
        [StationDraw.MapLit] = "map-lit",
        [StationDraw.MapBright] = "map-bright",
        [StationDraw.EnergyDim] = "energy-dim",
        [StationDraw.EnergyLit] = "energy-lit",
        [StationDraw.EnergyBright] = "energy-bright",
        [StationDraw.MissileDim] = "missile-dim",
        [StationDraw.MissileLit] = "missile-lit",
        [StationDraw.MissileBright] = "missile-bright",
        [StationDraw.SaveIdle] = "save-idle",
        [StationDraw.SaveLit] = "save-lit",
        [StationDraw.SaveDark] = "save-dark",
        [StationDraw.MapRightRetracted] = "map-right-retracted",
        [StationDraw.MapRightExtended] = "map-right-extended",
        [StationDraw.MapLeftRetracted] = "map-left-retracted",
        [StationDraw.MapLeftExtended] = "map-left-extended",
        [StationDraw.ResourceRightRetracted] = "resource-right-retracted",
        [StationDraw.ResourceRightExtended] = "resource-right-extended",
        [StationDraw.ResourceLeftRetracted] = "resource-left-retracted",
        [StationDraw.ResourceLeftExtended] = "resource-left-extended",
    });
}
