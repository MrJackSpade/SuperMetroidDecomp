using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.ResourceAudit.RoomObjectPrograms;

/// <summary>Exact native draw shapes for the individually source-reviewed PLM providers.</summary>
internal static class RoomObjectVisualDomainDefinitions
{
    /// <summary>Returns exact draw lists for a reviewed PLM catalog, including supported alias identities, or null for other providers.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList[]? Get(string qualifiedType) => qualifiedType switch
    {
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectShotBlockVisuals" => RoomObjectShotBlockVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectStationVisuals" => RoomObjectStationVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.Torizo.Artwork.RoomObjectBombTorizoHandVisualCatalog" => RoomObjectBombTorizoHandVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomObjectMotherBrainGlassVisualCatalog" => RoomObjectMotherBrainGlassVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectNoobTubeVisuals" => RoomObjectNoobTubeVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectDownwardGateVisuals" => RoomObjectDownwardGateVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Enemies.Elevator.Artwork.RoomObjectElevatorPlatformVisualCatalog" => RoomObjectElevatorPlatformVisualFiles.DrawLists.ToArray(),
        "SuperMetroid.Core.Bosses.Draygon.Artwork.RoomObjectDraygonCannonVisualCatalog" => RoomObjectDraygonCannonVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Enemies.Chozo.Artwork.RoomObjectChozoStatueVisualCatalog" => RoomObjectChozoStatueVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectLinkedRestoreVisuals" => RoomObjectLinkedRestoreVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Areas.Tourian.Artwork.RoomObjectTourianAccessVisuals" => RoomObjectTourianAccessVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSpeedBoosterVisuals" => RoomObjectSpeedBoosterVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectMaridiaElevatubeVisuals" => RoomObjectMaridiaElevatubeVisualFiles.AllDraws.ToArray(),
        "SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomObjectSporeSpawnCeilingVisualCatalog" => RoomObjectSporeSpawnCeilingVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSamusEaterVisuals" => RoomObjectSamusEaterVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBotwoonWallVisuals" => RoomObjectBotwoonWallVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectKraidVisuals" => RoomObjectKraidVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomObjectCrocomireVisualCatalog" => RoomObjectCrocomireVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomObjectMotherBrainFakeDeathVisualCatalog" => RoomObjectMotherBrainFakeDeathVisualFiles.All.ToArray(),
        // All includes the supported closed/mirrored aliases, not just the
        // authored keys required by construction. Aliases retain exact widths.
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBlueDoorVisuals" => BlueDoorObjectDrawDefinitionsTooling.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectColoredDoorVisuals" => RoomObjectColoredDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGreyDoorVisuals" => RoomObjectGreyDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectEyeDoorVisuals" => RoomObjectEyeDoorVisualFiles.All.ToArray(),
        "SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomObjectEscapeGateVisualCatalog" => RoomObjectEscapeGateVisualFiles.All.ToArray(),
        // Drawing asks for the installed picture of every collectible frame but a block pair's, which lies under the item's
        // own picture and writes only its terrain (#1538, #1557).
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectCollectibleVisuals" => RoomObjectCollectibleVisualFiles.Editable
            .Select(frame => SingleBlock(RoomObjectDraw.Of(frame.Draw), frame.Terrain)).ToArray(),
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGrappleBlockVisuals" => RoomObjectGrappleBlockVisualFiles.All
            .Select(frame => SingleBlock(RoomObjectDraw.Of(frame.Frame), frame.Terrain)).ToArray(),
        _ => null,
    };

    /// <summary>Wraps a collectible or grapple-block draw's block as a one-cell draw run for domain comparison.</summary>
    private static RoomObjectShotBlockDrawDefinitions.DrawList SingleBlock(RoomObjectDraw draw, RoomTerrainBlock terrain) =>
        new(draw, new RoomObjectShotBlockDrawDefinitions.Run[] { new(RoomObjectRunDirection.Horizontal, new[] { terrain }, new(0, 0)) });
}
