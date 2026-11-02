using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Exact native draw shapes for the individually source-reviewed PLM providers.</summary>
internal static class PlmVisualDomainDefinitions
{
    /// <summary>Returns exact draw lists for a reviewed PLM catalog, including supported alias identities, or null for other providers.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList[]? Get(string qualifiedType) => qualifiedType switch
    {
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmShotBlockVisuals" => RoomPlmShotBlockVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmStationVisuals" => RoomPlmStationVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.Torizo.Artwork.RoomPlmBombTorizoHandVisualCatalog" => RoomPlmBombTorizoHandVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainGlassVisualCatalog" => RoomPlmMotherBrainGlassVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmNoobTubeVisuals" => RoomPlmNoobTubeVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmDownwardGateVisuals" => RoomPlmDownwardGateVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Enemies.Elevator.Artwork.RoomPlmElevatorPlatformVisualCatalog" => RoomPlmElevatorPlatformVisualFiles.DrawLists.ToArray(),
        "SuperMetroid.Core.Bosses.Draygon.Artwork.RoomPlmDraygonCannonVisualCatalog" => RoomPlmDraygonCannonVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Enemies.Chozo.Artwork.RoomPlmChozoStatueVisualCatalog" => RoomPlmChozoStatueVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmLinkedRestoreVisuals" => RoomPlmLinkedRestoreVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Areas.Tourian.Artwork.RoomPlmTourianAccessVisuals" => RoomPlmTourianAccessVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSpeedBoosterVisuals" => RoomPlmSpeedBoosterVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmMaridiaElevatubeVisuals" => RoomPlmMaridiaElevatubeVisualFiles.AllDraws.ToArray(),
        "SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomPlmSporeSpawnCeilingVisualCatalog" => RoomPlmSporeSpawnCeilingVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSamusEaterVisuals" => RoomPlmSamusEaterVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBotwoonWallVisuals" => RoomPlmBotwoonWallVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmKraidVisuals" => RoomPlmKraidVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomPlmCrocomireVisualCatalog" => RoomPlmCrocomireVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainFakeDeathVisualCatalog" => RoomPlmMotherBrainFakeDeathVisualFiles.All.ToArray(),
        // All includes the supported closed/mirrored aliases, not just the
        // authored keys required by construction. Aliases retain exact widths.
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBlueDoorVisuals" => BlueDoorPlmDrawDefinitionsTooling.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmColoredDoorVisuals" => RoomPlmColoredDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGreyDoorVisuals" => RoomPlmGreyDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmEyeDoorVisuals" => RoomPlmEyeDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmEscapeGateVisualCatalog" => RoomPlmEscapeGateVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmCollectibleVisuals" => RoomPlmCollectibleVisualFiles.All
            .Select(frame => SingleBlock(PlmDraw.Of(frame.Draw), frame.Terrain)).ToArray(),
        "SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGrappleBlockVisuals" => RoomPlmGrappleBlockVisualFiles.All
            .Select(frame => SingleBlock(PlmDraw.Of(frame.Frame), frame.Terrain)).ToArray(),
        _ => null,
    };

    /// <summary>Wraps a collectible or grapple-block draw's block as a one-cell draw run for domain comparison.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList SingleBlock(PlmDraw draw, RoomTerrainBlock terrain) =>
        new(draw, new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, new[] { terrain }, new(0, 0)) });
}
