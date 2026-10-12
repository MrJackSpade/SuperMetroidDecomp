using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Stable, independent identities of selected room-actor art, not the PLM programs.</summary>
public static class RoomPlmPresentationIdentity
{
    /// <summary>Fingerprints the catalogs already bound by a playable host; nothing is read or imported here.</summary>
    public static IReadOnlyDictionary<string, string> Create(RoomPlmVisualCatalogSet catalogs) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName] = catalogs.ShotBlock.ContentIdentity,
            [GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName] = catalogs.GrappleBlock.ContentIdentity,
            [GameInstallationLayout.RoomPlmStationVisualDirectoryName] = catalogs.Station.ContentIdentity,
            [GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName] = catalogs.BlueDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName] = catalogs.ColoredDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName] = catalogs.GreyDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName] = catalogs.EyeDoor.ContentIdentity,
            [GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName] = catalogs.MotherBrainGlass.ContentIdentity,
            [GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName] = catalogs.NoobTube.ContentIdentity,
            [GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName] = catalogs.DownwardGate.ContentIdentity,
            [GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName] = catalogs.ElevatorPlatform.ContentIdentity,
            [GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName] = catalogs.EscapeGate.ContentIdentity,
            [GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName] = catalogs.BombTorizoHand.ContentIdentity,
            [GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName] = catalogs.DraygonCannon.ContentIdentity,
            [GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName] = catalogs.ChozoStatue.ContentIdentity,
            [GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName] = catalogs.LinkedRestore.ContentIdentity,
            [GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName] = catalogs.TourianAccess.ContentIdentity,
            [GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName] = catalogs.SpeedBooster.ContentIdentity,
            [GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName] = catalogs.MaridiaElevatube.ContentIdentity,
            [GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName] = catalogs.SporeSpawnCeiling.ContentIdentity,
            [GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName] = catalogs.SamusEater.ContentIdentity,
            [GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName] = catalogs.BotwoonWall.ContentIdentity,
            [GameInstallationLayout.RoomPlmKraidVisualDirectoryName] = catalogs.Kraid.ContentIdentity,
            [GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName] = catalogs.Crocomire.ContentIdentity,
            [GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName] = catalogs.MotherBrainFakeDeath.ContentIdentity,
            [GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName] = catalogs.Collectible.ContentIdentity,
            [GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName] = catalogs.DynamicCollectibleArt.ContentIdentity,
        };

    /// <summary>Loads the same selected art domains when inspecting an installed diagnostic artifact.</summary>
    public static IReadOnlyDictionary<string, string> Load(GameInstallation installation)
    {
        Ensure.NotNull(installation);
        return Create(new RoomPlmVisualCatalogSet { ShotBlock = installation.LoadRoomPlmShotBlockVisuals(), GrappleBlock = installation.LoadRoomPlmGrappleBlockVisuals(), Station = installation.LoadRoomPlmStationVisuals(), BlueDoor = installation.LoadRoomPlmBlueDoorVisuals(), ColoredDoor = installation.LoadRoomPlmColoredDoorVisuals(), GreyDoor = installation.LoadRoomPlmGreyDoorVisuals(), EyeDoor = installation.LoadRoomPlmEyeDoorVisuals(), MotherBrainGlass = installation.LoadRoomPlmMotherBrainGlassVisuals(), NoobTube = installation.LoadRoomPlmNoobTubeVisuals(), DownwardGate = installation.LoadRoomPlmDownwardGateVisuals(), ElevatorPlatform = installation.LoadRoomPlmElevatorPlatformVisuals(), EscapeGate = installation.LoadRoomPlmEscapeGateVisuals(), BombTorizoHand = installation.LoadRoomPlmBombTorizoHandVisuals(), DraygonCannon = installation.LoadRoomPlmDraygonCannonVisuals(), ChozoStatue = installation.LoadRoomPlmChozoStatueVisuals(), LinkedRestore = installation.LoadRoomPlmLinkedRestoreVisuals(), TourianAccess = installation.LoadRoomPlmTourianAccessVisuals(), SpeedBooster = installation.LoadRoomPlmSpeedBoosterVisuals(), MaridiaElevatube = installation.LoadRoomPlmMaridiaElevatubeVisuals(), SporeSpawnCeiling = installation.LoadRoomPlmSporeSpawnCeilingVisuals(), SamusEater = installation.LoadRoomPlmSamusEaterVisuals(), BotwoonWall = installation.LoadRoomPlmBotwoonWallVisuals(), Kraid = installation.LoadRoomPlmKraidVisuals(), Crocomire = installation.LoadRoomPlmCrocomireVisuals(), MotherBrainFakeDeath = installation.LoadRoomPlmMotherBrainFakeDeathVisuals(), Collectible = installation.LoadRoomPlmCollectibleVisuals(), DynamicCollectibleArt = installation.LoadRoomPlmDynamicCollectibleArt() });
    }
}

