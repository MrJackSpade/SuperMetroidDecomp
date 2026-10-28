using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
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
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmShotBlockVisuals" => ShotBlockPlmDraws.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmStationVisuals" => StationPlmDraws.All.ToArray(),
        "SuperMetroid.Core.Bosses.Torizo.Artwork.RoomPlmBombTorizoHandVisualCatalog" => BombTorizoHandPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainGlassVisualCatalog" => MotherBrainGlassPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmNoobTubeVisuals" => NoobTubePlmDraws.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmDownwardGateVisuals" => DownwardGatePlmDraws.All.ToArray(),
        "SuperMetroid.Core.Enemies.Elevator.Artwork.RoomPlmElevatorPlatformVisualCatalog" => ElevatorPlatformPlmDefinitions.DrawLists.ToArray(),
        "SuperMetroid.Core.Bosses.Draygon.Artwork.RoomPlmDraygonCannonVisualCatalog" => DraygonCannonPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Enemies.Chozo.Artwork.RoomPlmChozoStatueVisualCatalog" => ChozoStatuePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmLinkedRestoreVisuals" => RoomPlmLinkedRestoreDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Areas.Tourian.Artwork.RoomPlmTourianAccessVisuals" => TourianAccessDraws.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSpeedBoosterVisuals" => SpeedBoosterBlockPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmMaridiaElevatubeVisuals" => MaridiaElevatubePlmDefinitions.AllDraws.ToArray(),
        "SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomPlmSporeSpawnCeilingVisualCatalog" => SporeSpawnCeilingPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSamusEaterVisuals" => SamusEaterPlmDraws.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBotwoonWallVisuals" => BotwoonWallPlmDraws.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmKraidVisuals" => KraidRoomPlmDraws.All.ToArray(),
        "SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomPlmCrocomireVisualCatalog" => CrocomireArenaPlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainFakeDeathVisualCatalog" => MotherBrainFakeDeathPlmDrawDefinitions.All.ToArray(),
        // All includes the supported closed/mirrored aliases, not just the
        // authored keys required by construction. Aliases retain exact widths.
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBlueDoorVisuals" => BlueDoorPlmDrawDefinitionsTooling.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmColoredDoorVisuals" => ColoredDoorCaps.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGreyDoorVisuals" => GreyDoorCaps.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmEyeDoorVisuals" => EyeDoorDraws.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmEscapeGateVisualCatalog" => MotherBrainEscapeGatePlmDrawDefinitions.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmCollectibleVisuals" => CollectiblePlmDraws.All
            .Select(frame => SingleWord(PlmDraw.Of(frame.Draw), frame.LevelWord.Raw)).ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGrappleBlockVisuals" => GrappleBlockPlmDraws.All
            .Select(frame => SingleWord(PlmDraw.Of(frame.Frame), frame.LevelWord.Raw)).ToArray(),
        _ => null,
    };

    /// <summary>Wraps a collectible or grapple-block level word as a one-cell draw run for domain comparison.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList SingleWord(PlmDraw draw, ushort levelWord) =>
        new(draw, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, new ushort[] { levelWord }, new(0, 0)) });
}
