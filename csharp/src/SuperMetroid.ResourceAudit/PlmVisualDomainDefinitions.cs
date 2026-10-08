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
        // All includes the supported closed/mirrored pointers, not just the
        // authored keys required by construction. Aliases retain exact widths.
        "SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog" => BlueDoorPlmDrawDefinitionsTooling.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog" => ColoredDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog" => GreyDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog" => EyeDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog" => MotherBrainEscapeGatePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog" => RoomPlmCollectibleDrawDefinitions.All.ToArray()
            .Select(frame => SingleWord(frame.Pointer, frame.LevelWord)).ToArray(),
        "SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog" => RoomPlmGrappleBlockDrawDefinitions.All
            .Select(frame => SingleWord(frame.Pointer, frame.LevelWord)).ToArray(),
        _ => null,
    };

    private static RoomPlmShotBlockDrawDefinitions.DrawList SingleWord(ushort pointer, ushort levelWord) =>
        new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(1, new ushort[] { levelWord }, 0, 0) });
}
