using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Stable, independent identities of selected room-actor art, not the PLM programs.</summary>
public static class RoomPlmPresentationIdentity
{
    /// <summary>Fingerprints the catalogs already bound by a playable host; nothing is read or imported here.</summary>
    public static IReadOnlyDictionary<string, string> Create(
        RoomPlmShotBlockVisualCatalog shotBlock,
        RoomPlmGrappleBlockVisualCatalog grappleBlock,
        RoomPlmStationVisualCatalog station,
        RoomPlmBlueDoorVisualCatalog blueDoor,
        RoomPlmColoredDoorVisualCatalog coloredDoor,
        RoomPlmGreyDoorVisualCatalog greyDoor,
        RoomPlmEyeDoorVisualCatalog eyeDoor,
        RoomPlmMotherBrainGlassVisualCatalog motherBrainGlass,
        RoomPlmNoobTubeVisualCatalog noobTube,
        RoomPlmDownwardGateVisualCatalog downwardGate,
        RoomPlmElevatorPlatformVisualCatalog elevatorPlatform,
        RoomPlmEscapeGateVisualCatalog escapeGate,
        RoomPlmBombTorizoHandVisualCatalog bombTorizoHand,
        RoomPlmDraygonCannonVisualCatalog draygonCannon,
        RoomPlmChozoStatueVisualCatalog chozoStatue,
        RoomPlmLinkedRestoreVisualCatalog linkedRestore,
        RoomPlmTourianAccessVisualCatalog tourianAccess,
        RoomPlmSpeedBoosterVisualCatalog speedBooster,
        RoomPlmMaridiaElevatubeVisualCatalog maridiaElevatube,
        RoomPlmSporeSpawnCeilingVisualCatalog sporeSpawnCeiling,
        RoomPlmSamusEaterVisualCatalog samusEater,
        RoomPlmBotwoonWallVisualCatalog botwoonWall,
        RoomPlmKraidVisualCatalog kraid,
        RoomPlmCrocomireVisualCatalog crocomire,
        RoomPlmMotherBrainFakeDeathVisualCatalog motherBrainFakeDeath,
        RoomPlmCollectibleVisualCatalog collectible,
        RoomPlmDynamicCollectibleArtCatalog dynamicCollectibleArt) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName] = shotBlock.ContentIdentity,
            [GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName] = grappleBlock.ContentIdentity,
            [GameInstallationLayout.RoomPlmStationVisualDirectoryName] = station.ContentIdentity,
            [GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName] = blueDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName] = coloredDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName] = greyDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName] = eyeDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName] = motherBrainGlass.ContentIdentity,
            [GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName] = noobTube.ContentIdentity,
            [GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName] = downwardGate.ContentIdentity,
            [GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName] = elevatorPlatform.ContentIdentity,
            [GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName] = escapeGate.ContentIdentity,
            [GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName] = bombTorizoHand.ContentIdentity,
            [GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName] = draygonCannon.ContentIdentity,
            [GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName] = chozoStatue.ContentIdentity,
            [GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName] = linkedRestore.ContentIdentity,
            [GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName] = tourianAccess.ContentIdentity,
            [GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName] = speedBooster.ContentIdentity,
            [GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName] = maridiaElevatube.ContentIdentity,
            [GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName] = sporeSpawnCeiling.ContentIdentity,
            [GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName] = samusEater.ContentIdentity,
            [GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName] = botwoonWall.ContentIdentity,
            [GameInstallationLayout.RoomPlmKraidVisualDirectoryName] = kraid.ContentIdentity,
            [GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName] = crocomire.ContentIdentity,
            [GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName] = motherBrainFakeDeath.ContentIdentity,
            [GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName] = collectible.ContentIdentity,
            [GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName] = dynamicCollectibleArt.ContentIdentity,
        };

    /// <summary>Loads the same selected art domains when inspecting an installed diagnostic artifact.</summary>
    public static IReadOnlyDictionary<string, string> Load(GameInstallation installation)
    {
        ArgumentNullException.ThrowIfNull(installation);
        return Create(
            installation.LoadRoomPlmShotBlockVisuals(),
            installation.LoadRoomPlmGrappleBlockVisuals(),
            installation.LoadRoomPlmStationVisuals(),
            installation.LoadRoomPlmBlueDoorVisuals(),
            installation.LoadRoomPlmColoredDoorVisuals(),
            installation.LoadRoomPlmGreyDoorVisuals(),
            installation.LoadRoomPlmEyeDoorVisuals(),
            installation.LoadRoomPlmMotherBrainGlassVisuals(),
            installation.LoadRoomPlmNoobTubeVisuals(),
            installation.LoadRoomPlmDownwardGateVisuals(),
            installation.LoadRoomPlmElevatorPlatformVisuals(),
            installation.LoadRoomPlmEscapeGateVisuals(),
            installation.LoadRoomPlmBombTorizoHandVisuals(),
            installation.LoadRoomPlmDraygonCannonVisuals(),
            installation.LoadRoomPlmChozoStatueVisuals(),
            installation.LoadRoomPlmLinkedRestoreVisuals(),
            installation.LoadRoomPlmTourianAccessVisuals(),
            installation.LoadRoomPlmSpeedBoosterVisuals(),
            installation.LoadRoomPlmMaridiaElevatubeVisuals(),
            installation.LoadRoomPlmSporeSpawnCeilingVisuals(),
            installation.LoadRoomPlmSamusEaterVisuals(),
            installation.LoadRoomPlmBotwoonWallVisuals(),
            installation.LoadRoomPlmKraidVisuals(),
            installation.LoadRoomPlmCrocomireVisuals(),
            installation.LoadRoomPlmMotherBrainFakeDeathVisuals(),
            installation.LoadRoomPlmCollectibleVisuals(),
            installation.LoadRoomPlmDynamicCollectibleArt());
    }
}

