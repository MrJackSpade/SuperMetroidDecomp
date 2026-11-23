using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Stable, independent identities of selected room-actor art, not the PLM programs.</summary>
public static partial class RoomObjectPresentationIdentity
{
    /// <summary>Fingerprints the catalogs already bound by a playable host; nothing is read or imported here.</summary>
    public static IReadOnlyDictionary<string, string> Create(RoomObjectVisualCatalogSet catalogs) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName] = catalogs.ShotBlock.ContentIdentity(
                GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName, RoomObjectShotBlockVisualFiles.Family),
            [GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName] = catalogs.GrappleBlock.ContentIdentity(
                GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName, RoomObjectGrappleBlockVisualFiles.Family),
            [GameInstallationLayout.RoomObjectStationVisualDirectoryName] = catalogs.Station.ContentIdentity(
                GameInstallationLayout.RoomObjectStationVisualDirectoryName, RoomObjectStationVisualFiles.Family),
            [GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName] = catalogs.BlueDoor.ContentIdentity(
                GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName, RoomObjectBlueDoorVisualFiles.Family),
            [GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName] = catalogs.ColoredDoor.ContentIdentity(
                GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName, RoomObjectColoredDoorVisualFiles.Family),
            [GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName] = catalogs.GreyDoor.ContentIdentity(
                GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName, RoomObjectGreyDoorVisualFiles.Family),
            [GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName] = catalogs.EyeDoor.ContentIdentity(
                GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName, RoomObjectEyeDoorVisualFiles.Family),
            [GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName] = catalogs.MotherBrainGlass.ContentIdentity,
            [GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName] = catalogs.NoobTube.ContentIdentity(
                GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName, RoomObjectNoobTubeVisualFiles.Family),
            [GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName] = catalogs.DownwardGate.ContentIdentity(
                GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName, RoomObjectDownwardGateVisualFiles.Family),
            [GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName] = catalogs.ElevatorPlatform.ContentIdentity(
                GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName, RoomObjectElevatorPlatformVisualFiles.Family),
            [GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName] = catalogs.EscapeGate.ContentIdentity(
                GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName, RoomObjectEscapeGateVisualFiles.Family),
            [GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName] = catalogs.BombTorizoHand.ContentIdentity(
                GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName, RoomObjectBombTorizoHandVisualFiles.Family),
            [GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName] = catalogs.DraygonCannon.ContentIdentity,
            [GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName] = catalogs.PhantoonDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName] = catalogs.ChozoStatue.ContentIdentity(
                GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName, RoomObjectChozoStatueVisualFiles.Family),
            [GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName] = catalogs.LinkedRestore.ContentIdentity(
                GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName, RoomObjectLinkedRestoreVisualFiles.Family),
            [GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName] = catalogs.TourianAccess.ContentIdentity,
            [GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName] = catalogs.SpeedBooster.ContentIdentity(
                GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName, RoomObjectSpeedBoosterVisualFiles.Family),
            [GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName] = catalogs.MaridiaElevatube.ContentIdentity(
                GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName, RoomObjectMaridiaElevatubeVisualFiles.Family),
            [GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName] = catalogs.SporeSpawnCeiling.ContentIdentity(
                GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName, RoomObjectSporeSpawnCeilingVisualFiles.Family),
            [GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName] = catalogs.SamusEater.ContentIdentity(
                GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName, RoomObjectSamusEaterVisualFiles.Family),
            [GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName] = catalogs.BotwoonWall.ContentIdentity(
                GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName, RoomObjectBotwoonWallVisualFiles.Family),
            [GameInstallationLayout.RoomObjectKraidVisualDirectoryName] = catalogs.Kraid.ContentIdentity(
                GameInstallationLayout.RoomObjectKraidVisualDirectoryName, RoomObjectKraidVisualFiles.Family),
            [GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName] = catalogs.Crocomire.ContentIdentity(
                GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName, RoomObjectCrocomireVisualFiles.Family),
            [GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName] = catalogs.MotherBrainFakeDeath.ContentIdentity,
            [GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName] = catalogs.Collectible.ContentIdentity(
                GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName, RoomObjectCollectibleVisualFiles.Family),
            [GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName] = catalogs.RevealAndWall.ContentIdentity(
                GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName, RoomObjectRevealAndWallVisualFiles.Family),
            [GameInstallationLayout.RoomObjectSetupBlockDirectoryName] = catalogs.SetupBlocks.ContentIdentity,
        };
}

