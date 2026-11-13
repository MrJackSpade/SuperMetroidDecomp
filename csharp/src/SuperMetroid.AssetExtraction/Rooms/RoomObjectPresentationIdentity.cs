using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Stable, independent identities of selected room-actor art, not the PLM programs.</summary>
public static class RoomObjectPresentationIdentity
{
    /// <summary>Fingerprints the catalogs already bound by a playable host; nothing is read or imported here.</summary>
    public static IReadOnlyDictionary<string, string> Create(RoomObjectVisualCatalogSet catalogs) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName] = catalogs.ShotBlock.ContentIdentity,
            [GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName] = catalogs.GrappleBlock.ContentIdentity,
            [GameInstallationLayout.RoomObjectStationVisualDirectoryName] = catalogs.Station.ContentIdentity,
            [GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName] = catalogs.BlueDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName] = catalogs.ColoredDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName] = catalogs.GreyDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName] = catalogs.EyeDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName] = catalogs.MotherBrainGlass.ContentIdentity,
            [GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName] = catalogs.NoobTube.ContentIdentity,
            [GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName] = catalogs.DownwardGate.ContentIdentity,
            [GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName] = catalogs.ElevatorPlatform.ContentIdentity,
            [GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName] = catalogs.EscapeGate.ContentIdentity,
            [GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName] = catalogs.BombTorizoHand.ContentIdentity,
            [GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName] = catalogs.DraygonCannon.ContentIdentity,
            [GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName] = catalogs.PhantoonDoor.ContentIdentity,
            [GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName] = catalogs.ChozoStatue.ContentIdentity,
            [GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName] = catalogs.LinkedRestore.ContentIdentity,
            [GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName] = catalogs.TourianAccess.ContentIdentity,
            [GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName] = catalogs.SpeedBooster.ContentIdentity,
            [GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName] = catalogs.MaridiaElevatube.ContentIdentity,
            [GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName] = catalogs.SporeSpawnCeiling.ContentIdentity,
            [GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName] = catalogs.SamusEater.ContentIdentity,
            [GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName] = catalogs.BotwoonWall.ContentIdentity,
            [GameInstallationLayout.RoomObjectKraidVisualDirectoryName] = catalogs.Kraid.ContentIdentity,
            [GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName] = catalogs.Crocomire.ContentIdentity,
            [GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName] = catalogs.MotherBrainFakeDeath.ContentIdentity,
            [GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName] = catalogs.Collectible.ContentIdentity,
            [GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName] = catalogs.RevealAndWall.ContentIdentity,
            [GameInstallationLayout.RoomObjectSetupBlockDirectoryName] = catalogs.SetupBlocks.ContentIdentity,
        };
}

