using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The order the station frames are published in. Each family's published names are its draw identity's keys
/// (<see cref="StationDrawKeys"/> and its siblings); only the import and its files know them, and the game draws by draw.
/// </summary>
internal static partial class RoomObjectVisualIdDefinitions
{
    /// <summary>The station draws in their published enumeration order: each glow by station, then save and access states.</summary>
    internal static readonly StationDraw[] StationPublishedOrder =
    [
        StationDraw.MapDim, StationDraw.EnergyDim, StationDraw.MissileDim,
        StationDraw.MapLit, StationDraw.EnergyLit, StationDraw.MissileLit,
        StationDraw.MapBright, StationDraw.EnergyBright, StationDraw.MissileBright,
        StationDraw.SaveIdle, StationDraw.SaveLit, StationDraw.SaveDark,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];
}
