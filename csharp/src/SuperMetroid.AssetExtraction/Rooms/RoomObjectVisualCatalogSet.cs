using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Every room PLM visual catalog whose contents form the PLM presentation identity (#633).</summary>
public sealed record RoomObjectVisualCatalogSet
{
    /// <summary>Shot block.</summary>
    public required InstalledRoomObjectPictures ShotBlock { get; init; }

    /// <summary>Grapple block.</summary>
    public required InstalledRoomObjectPictures GrappleBlock { get; init; }

    /// <summary>Station.</summary>
    public required InstalledRoomObjectPictures Station { get; init; }

    /// <summary>Blue door.</summary>
    public required InstalledRoomObjectBlueDoorVisuals BlueDoor { get; init; }

    /// <summary>Colored door.</summary>
    public required InstalledRoomObjectPictures ColoredDoor { get; init; }

    /// <summary>Grey door.</summary>
    public required InstalledRoomObjectPictures GreyDoor { get; init; }

    /// <summary>Eye door.</summary>
    public required InstalledRoomObjectEyeDoorVisuals EyeDoor { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required InstalledMotherBrainGlassFrames MotherBrainGlass { get; init; }

    /// <summary>Noob tube.</summary>
    public required InstalledRoomObjectPictures NoobTube { get; init; }

    /// <summary>Downward gate.</summary>
    public required InstalledRoomObjectPictures DownwardGate { get; init; }

    /// <summary>Elevator platform.</summary>
    public required InstalledRoomObjectPictures ElevatorPlatform { get; init; }

    /// <summary>Escape gate.</summary>
    public required InstalledEscapeGateFrames EscapeGate { get; init; }

    /// <summary>Bomb torizo hand.</summary>
    public required InstalledRoomObjectPictures BombTorizoHand { get; init; }

    /// <summary>Draygon cannon.</summary>
    public required InstalledDraygonCannonFrames DraygonCannon { get; init; }

    /// <summary>Phantoon door.</summary>
    public required InstalledPhantoonDoorFrames PhantoonDoor { get; init; }

    /// <summary>Chozo statue.</summary>
    public required InstalledRoomObjectPictures ChozoStatue { get; init; }

    /// <summary>Linked restore.</summary>
    public required InstalledRoomObjectPictures LinkedRestore { get; init; }

    /// <summary>Tourian access.</summary>
    public required RoomObjectTourianAccessVisuals TourianAccess { get; init; }

    /// <summary>Speed booster.</summary>
    public required InstalledRoomObjectPictures SpeedBooster { get; init; }

    /// <summary>Maridia elevatube.</summary>
    public required InstalledRoomObjectPictures MaridiaElevatube { get; init; }

    /// <summary>Spore spawn ceiling.</summary>
    public required InstalledRoomObjectPictures SporeSpawnCeiling { get; init; }

    /// <summary>Samus eater.</summary>
    public required InstalledRoomObjectPictures SamusEater { get; init; }

    /// <summary>Botwoon wall.</summary>
    public required InstalledRoomObjectPictures BotwoonWall { get; init; }

    /// <summary>Kraid.</summary>
    public required InstalledRoomObjectPictures Kraid { get; init; }

    /// <summary>Crocomire.</summary>
    public required InstalledRoomObjectPictures Crocomire { get; init; }

    /// <summary>Mother brain fake death.</summary>
    public required InstalledMotherBrainFakeDeathFrames MotherBrainFakeDeath { get; init; }

    /// <summary>Collectible.</summary>
    public required InstalledRoomObjectPictures Collectible { get; init; }

    /// <summary>Bombed reveals, escape passage, escape shaft wall and rescue wall.</summary>
    public required InstalledRoomObjectPictures RevealAndWall { get; init; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    public required InstalledRoomObjectSetupBlocks SetupBlocks { get; init; }

    /// <summary>
    /// Every catalog as installed under the content directory <paramref name="content"/>, which the import reads to learn
    /// the blocks the room objects write (#1636).
    /// </summary>
    /// <exception cref="InvalidDataException">A catalog's installed file is invalid.</exception>
    internal static RoomObjectVisualCatalogSet Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return new RoomObjectVisualCatalogSet
        {
            ShotBlock = RoomObjectShotBlockVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName)),
            GrappleBlock = RoomObjectGrappleBlockVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName)),
            Station = RoomObjectStationVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectStationVisualDirectoryName)),
            BlueDoor = RoomObjectBlueDoorVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectBlueDoorVisualDirectoryName)),
            ColoredDoor = RoomObjectColoredDoorVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectColoredDoorVisualDirectoryName)),
            GreyDoor = RoomObjectGreyDoorVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName)),
            EyeDoor = RoomObjectEyeDoorVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectEyeDoorVisualDirectoryName)),
            MotherBrainGlass = RoomObjectMotherBrainGlassVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName)),
            NoobTube = RoomObjectNoobTubeVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName)),
            DownwardGate = RoomObjectDownwardGateVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectDownwardGateVisualDirectoryName)),
            ElevatorPlatform = RoomObjectElevatorPlatformVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName)),
            EscapeGate = RoomObjectEscapeGateVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName)),
            BombTorizoHand = RoomObjectBombTorizoHandVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectBombTorizoHandVisualDirectoryName)),
            DraygonCannon = RoomObjectDraygonCannonVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectDraygonCannonVisualDirectoryName)),
            PhantoonDoor = RoomObjectPhantoonDoorVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectPhantoonDoorVisualDirectoryName)),
            ChozoStatue = RoomObjectChozoStatueVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName)),
            LinkedRestore = RoomObjectLinkedRestoreVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName)),
            TourianAccess = RoomObjectTourianAccessVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName)),
            SpeedBooster = RoomObjectSpeedBoosterVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName)),
            MaridiaElevatube = RoomObjectMaridiaElevatubeVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectMaridiaElevatubeVisualDirectoryName)),
            SporeSpawnCeiling = RoomObjectSporeSpawnCeilingVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName)),
            SamusEater = RoomObjectSamusEaterVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName)),
            BotwoonWall = RoomObjectBotwoonWallVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName)),
            Kraid = RoomObjectKraidVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectKraidVisualDirectoryName)),
            Crocomire = RoomObjectCrocomireVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectCrocomireVisualDirectoryName)),
            MotherBrainFakeDeath = RoomObjectMotherBrainFakeDeathVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectMotherBrainFakeDeathVisualDirectoryName)),
            Collectible = RoomObjectCollectibleVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName)),
            RevealAndWall = RoomObjectRevealAndWallVisualFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName)),
            SetupBlocks = RoomObjectSetupBlockFiles.Load(Path.Combine(content, GameInstallationLayout.RoomObjectSetupBlockDirectoryName)),
        };
    }
}
