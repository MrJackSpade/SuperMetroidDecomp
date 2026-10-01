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
        "SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog" => TourianAccessPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog" => SpeedBoosterBlockPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog" => MaridiaElevatubePlmDefinitions.AllDraws.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog" => SporeSpawnCeilingPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog" => SamusEaterPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog" => BotwoonWallPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog" => KraidRoomPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog" => CrocomireArenaPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog" => MotherBrainFakeDeathPlmDrawDefinitions.All.ToArray(),
        _ => null,
    };
}
