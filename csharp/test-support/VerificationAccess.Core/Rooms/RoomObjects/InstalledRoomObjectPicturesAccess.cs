using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectPictures"/> members production does not use.</summary>
internal static class InstalledRoomObjectPicturesAccess
{
    /// <summary>Exposes <see cref="InstalledRoomObjectPictures"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectPictures self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}

/// <summary>Each room-object family's pictures as the repository installation holds them (#2557).</summary>
internal static class RoomObjectPicturesStock
{
    /// <summary>BotwoonWall's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures BotwoonWall => RepositoryInstallation.Installation.LoadRoomObjectBotwoonWallVisuals();

    /// <summary>Collectible's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures Collectible => RepositoryInstallation.Installation.LoadRoomObjectCollectibleVisuals();

    /// <summary>ColoredDoor's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures ColoredDoor => RepositoryInstallation.Installation.LoadRoomObjectColoredDoorVisuals();

    /// <summary>DownwardGate's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures DownwardGate => RepositoryInstallation.Installation.LoadRoomObjectDownwardGateVisuals();

    /// <summary>ElevatorPlatform's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures ElevatorPlatform => RepositoryInstallation.Installation.LoadRoomObjectElevatorPlatformVisuals();

    /// <summary>GrappleBlock's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures GrappleBlock => RepositoryInstallation.Installation.LoadRoomObjectGrappleBlockVisuals();

    /// <summary>GreyDoor's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures GreyDoor => RepositoryInstallation.Installation.LoadRoomObjectGreyDoorVisuals();

    /// <summary>Kraid's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures Kraid => RepositoryInstallation.Installation.LoadRoomObjectKraidVisuals();

    /// <summary>LinkedRestore's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures LinkedRestore => RepositoryInstallation.Installation.LoadRoomObjectLinkedRestoreVisuals();

    /// <summary>MaridiaElevatube's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures MaridiaElevatube => RepositoryInstallation.Installation.LoadRoomObjectMaridiaElevatubeVisuals();

    /// <summary>NoobTube's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures NoobTube => RepositoryInstallation.Installation.LoadRoomObjectNoobTubeVisuals();

    /// <summary>RevealAndWall's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures RevealAndWall => RepositoryInstallation.Installation.LoadRoomObjectRevealAndWallVisuals();

    /// <summary>SamusEater's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures SamusEater => RepositoryInstallation.Installation.LoadRoomObjectSamusEaterVisuals();

    /// <summary>ShotBlock's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures ShotBlock => RepositoryInstallation.Installation.LoadRoomObjectShotBlockVisuals();

    /// <summary>SpeedBooster's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures SpeedBooster => RepositoryInstallation.Installation.LoadRoomObjectSpeedBoosterVisuals();

    /// <summary>Station's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures Station => RepositoryInstallation.Installation.LoadRoomObjectStationVisuals();

    /// <summary>Crocomire's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures Crocomire => RepositoryInstallation.Installation.LoadRoomObjectCrocomireVisuals();

    /// <summary>SporeSpawnCeiling's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures SporeSpawnCeiling => RepositoryInstallation.Installation.LoadRoomObjectSporeSpawnCeilingVisuals();

    /// <summary>BombTorizoHand's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures BombTorizoHand => RepositoryInstallation.Installation.LoadRoomObjectBombTorizoHandVisuals();

    /// <summary>ChozoStatue's pictures as the repository installation holds them.</summary>
    internal static InstalledRoomObjectPictures ChozoStatue => RepositoryInstallation.Installation.LoadRoomObjectChozoStatueVisuals();
}
