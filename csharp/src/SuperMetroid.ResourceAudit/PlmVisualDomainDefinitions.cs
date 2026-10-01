using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native draw shapes for the individually source-reviewed PLM providers.</summary>
internal static class PlmVisualDomainDefinitions
{
    internal static RoomPlmShotBlockDrawDefinitions.DrawList[]? Get(string qualifiedType) => qualifiedType switch
    {
        "SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog" => RoomPlmShotBlockDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog" => RoomPlmStationDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog" => BombTorizoHandPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog" => MotherBrainGlassPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog" => NoobTubePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog" => DownwardGatePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog" => ElevatorPlatformPlmDefinitions.DrawLists.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog" => DraygonCannonPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog" => ChozoStatuePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog" => RoomPlmLinkedRestoreDrawDefinitions.All.ToArray(),
        _ => null,
    };
}
