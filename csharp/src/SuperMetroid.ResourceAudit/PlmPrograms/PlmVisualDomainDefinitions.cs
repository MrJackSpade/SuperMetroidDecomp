using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Exact native draw shapes for the individually source-reviewed PLM providers.</summary>
internal static class PlmVisualDomainDefinitions
{
    /// <summary>Returns exact draw lists for a reviewed PLM catalog, including supported alias identities, or null for other providers.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList[]? Get(string qualifiedType) => qualifiedType switch
    {
        "SuperMetroid.Core.Rooms.Plms.RoomPlmShotBlockVisualCatalog" => RoomPlmShotBlockDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmStationVisualCatalog" => RoomPlmStationDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmBombTorizoHandVisualCatalog" => BombTorizoHandPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmMotherBrainGlassVisualCatalog" => MotherBrainGlassPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmNoobTubeVisualCatalog" => NoobTubePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmDownwardGateVisualCatalog" => DownwardGatePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmElevatorPlatformVisualCatalog" => ElevatorPlatformPlmDefinitions.DrawLists.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmDraygonCannonVisualCatalog" => DraygonCannonPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmChozoStatueVisualCatalog" => ChozoStatuePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmLinkedRestoreVisualCatalog" => RoomPlmLinkedRestoreDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmTourianAccessVisualCatalog" => TourianAccessPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmSpeedBoosterVisualCatalog" => SpeedBoosterBlockPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmMaridiaElevatubeVisualCatalog" => MaridiaElevatubePlmDefinitions.AllDraws.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmSporeSpawnCeilingVisualCatalog" => SporeSpawnCeilingPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmSamusEaterVisualCatalog" => SamusEaterPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmBotwoonWallVisualCatalog" => BotwoonWallPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmKraidVisualCatalog" => KraidRoomPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmCrocomireVisualCatalog" => CrocomireArenaPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmMotherBrainFakeDeathVisualCatalog" => MotherBrainFakeDeathPlmDrawDefinitions.All.ToArray(),
        // All includes the supported closed/mirrored pointers, not just the
        // authored keys required by construction. Aliases retain exact widths.
        "SuperMetroid.Core.Rooms.Plms.RoomPlmBlueDoorVisualCatalog" => BlueDoorPlmDrawDefinitionsTooling.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmColoredDoorVisualCatalog" => ColoredDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmGreyDoorVisualCatalog" => GreyDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmEyeDoorVisualCatalog" => EyeDoorPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmEscapeGateVisualCatalog" => MotherBrainEscapeGatePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmCollectibleVisualCatalog" => RoomPlmCollectibleDrawDefinitions.All
            .Select(frame => SingleWord(checked((ushort)frame.Pointer), frame.LevelWord.Raw)).ToArray(),
        "SuperMetroid.Core.Rooms.Plms.RoomPlmGrappleBlockVisualCatalog" => RoomPlmGrappleBlockDrawDefinitions.All
            .Select(frame => SingleWord(frame.Pointer, frame.LevelWord)).ToArray(),
        _ => null,
    };

    /// <summary>Wraps a collectible or grapple-block level word as a one-cell draw run for domain comparison.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList SingleWord(ushort pointer, ushort levelWord) =>
        new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(1, new ushort[] { levelWord }, new(0, 0)) });
}
