using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>
/// The installed pictures of the room objects' frames, keyed by the frame's draw (#1557, #1732). Each family's provider
/// publishes its pictures by draw (<c>IRoomObjectFamilyPictures.Pictures</c>, #2571); the family's loader builds the
/// provider from <c>RoomObjectFrameImageFiles.Load</c>, which requires a picture of every frame the family's descriptor
/// enumerates and rejects any other, so no frame its family draws is absent. The room reads a frame's picture through
/// <c>RoomObjectVisuals.Image</c>, which composes every family's pictures, and a setup block's through
/// <c>RoomObjectVisuals.SetupBlock</c>; that read's proof is every family's and the setup blocks', so a construction of any
/// of their providers outside its reviewed loader revokes it, as it revokes that provider's own proof.
/// </summary>
internal static class RoomObjectFrameClosedContractDefinitions
{
    /// <summary>
    /// The sources every room-object frame proof rests on: the frame pictures and draw identities, the drawing that names
    /// the frame each object draws, and the shared loader that requires every frame of a family to be pictured.
    /// </summary>
    private static readonly ReviewedSource[] Shared =
    [
        new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectFrameImages.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectFrameImage.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectDraw.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectFrameDraw.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectDrawFamily.cs"),
        new("csharp/src/SuperMetroid.Core/World/RoomObjectBlockDrawing.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectFrameImageFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectFrameFamily.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectFrameLayout.cs"),
    ];

    /// <summary>
    /// One family's proof: its provider, its loader (which enumerates the family's frames through the descriptor), the
    /// descriptor sources the provider's guard and the loader's enumeration share, and <see cref="Shared"/>. The provider
    /// publishes its pictures only to the composed read (<see cref="Composed"/>), so its proof names no read of its own.
    /// </summary>
    private static ClosedPresentationContract Family(string type, string rule, string provider, string loader, params string[] descriptors) =>
        Family([], type, rule, provider, loader, descriptors);

    /// <summary>
    /// One family's proof whose provider also reads its own installed frames: every listed read is guarded by the same
    /// descriptor sources.
    /// </summary>
    private static ClosedPresentationContract Family(string[] methods, string type, string rule, string provider, string loader, params string[] descriptors) =>
        new(type, rule, methods,
            [new(provider), new(loader), .. descriptors.Select(path => new ReviewedSource(path)), .. Shared]);

    /// <summary>
    /// The setup blocks' proof: their loader requires a picture of every <c>RoomObjectSetupBlock</c> once and rejects any
    /// other, so <c>Shown</c> finds every block a setup or reaction writes.
    /// </summary>
    internal static readonly ClosedPresentationContract SetupBlocks = new(
        "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSetupBlocks", "room-object-setup-blocks-complete", ["Shown"],
        [
            new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectSetupBlocks.cs"),
            new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectSetupBlock.cs"),
            new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectSetupBlockFiles.cs"),
        ]);

    /// <summary>Every room-object frame-picture provider, with its reviewed sources.</summary>
    internal static readonly ClosedPresentationContract[] Families =
    [
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectRevealAndWallVisuals", "room-object-reveal-and-wall-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectRevealAndWallVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectRevealAndWallVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BombedRevealObjectDraws.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectBombedRevealDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/CrateriaMainstreetEscapePassageDraws.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/CrateriaMainstreetEscapePassageObjectDefinitions.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/OldTourianEscapeShaftWallDraws.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/OldTourianEscapeShaftWallObjectDefinitions.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/EscapeAnimalWalls.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/EscapeAnimalDraw.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectShotBlockVisuals", "room-object-shot-block-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectShotBlockVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectShotBlockVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ShotBlockObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSpeedBoosterVisuals", "room-object-speed-booster-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectSpeedBoosterVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectSpeedBoosterVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SpeedBoosterBlockObjectDrawDefinitions.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectKraidVisuals", "room-object-kraid-room-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectKraidVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectKraidVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/KraidRoomObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectMaridiaElevatubeVisuals", "room-object-maridia-elevatube-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectMaridiaElevatubeVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectMaridiaElevatubeVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/MaridiaElevatubeObjectDefinitions.cs"),
        Family("SuperMetroid.Core.Bosses.Crocomire.Artwork.InstalledRoomObjectCrocomireVisuals", "room-object-crocomire-arena-complete-frames",
            "csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/InstalledRoomObjectCrocomireVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectCrocomireVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaDraws.cs"),
        Family("SuperMetroid.Core.Areas.Tourian.Artwork.RoomObjectTourianAccessVisuals", "room-object-tourian-access-complete-frames",
            "csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/RoomObjectTourianAccessVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectTourianAccessVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraw.cs"),
        Family("SuperMetroid.Core.Bosses.SporeSpawn.Artwork.InstalledRoomObjectSporeSpawnCeilingVisuals", "room-object-spore-spawn-ceiling-complete-frames",
            "csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/InstalledRoomObjectSporeSpawnCeilingVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectSporeSpawnCeilingVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSamusEaterVisuals", "room-object-samus-eater-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectSamusEaterVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectSamusEaterVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SamusEaterObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBotwoonWallVisuals", "room-object-botwoon-wall-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectBotwoonWallVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectBotwoonWallVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BotwoonWallObjectDraws.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BotwoonWallObjectDrawDefinitions.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BotwoonWallDraw.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectLinkedRestoreVisuals", "room-object-linked-restore-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectLinkedRestoreVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectLinkedRestoreVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/LinkedRestoreObjectFamily.cs"),
        // The closed cap drawn solid shows its facing's closed cap, which the provider reads for each facing.
        Family(["ClosedSolid"], "SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBlueDoorVisuals", "room-object-blue-door-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectBlueDoorVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectBlueDoorVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorCaps.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorObjectDrawDefinitions.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectColoredDoorVisuals", "room-object-colored-door-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectColoredDoorVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectColoredDoorVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorCaps.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGreyDoorVisuals", "room-object-grey-door-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectGreyDoorVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectGreyDoorVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorCaps.cs"),
        // A mirrored clear shows the shared clear's picture mirrored; the provider derives it from that installed frame.
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectEyeDoorVisuals", "room-object-eye-door-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectEyeDoorVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectEyeDoorVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorDraws.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorObjectDrawDefinitions.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectNoobTubeVisuals", "room-object-noob-tube-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectNoobTubeVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectNoobTubeVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/NoobTubeObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectStationVisuals", "room-object-station-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectStationVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectStationVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/StationObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectElevatorPlatformVisuals", "room-object-elevator-platform-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectElevatorPlatformVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectElevatorPlatformVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ElevatorPlatformDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectDownwardGateVisuals", "room-object-downward-gate-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectDownwardGateVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectDownwardGateVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/DownwardGateObjectDraws.cs"),
        Family("SuperMetroid.Core.Bosses.Torizo.Artwork.InstalledRoomObjectBombTorizoHandVisuals", "room-object-bomb-torizo-hand-complete-frames",
            "csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/InstalledRoomObjectBombTorizoHandVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectBombTorizoHandVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraws.cs"),
        Family("SuperMetroid.Core.Enemies.Chozo.Artwork.InstalledRoomObjectChozoStatueVisuals", "room-object-chozo-statue-complete-frames",
            "csharp/src/SuperMetroid.Core/Enemies/Chozo/Artwork/InstalledRoomObjectChozoStatueVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectChozoStatueVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatueDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectCollectibleVisuals", "room-object-collectible-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectCollectibleVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectCollectibleVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/CollectibleObjectDraws.cs"),
        Family("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGrappleBlockVisuals", "room-object-grapple-block-complete-frames",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectGrappleBlockVisuals.cs",
            "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomObjectGrappleBlockVisualFiles.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/GrappleBlockObjectDraws.cs"),
    ];

    /// <summary>
    /// The room's read of its room-object pictures (#2571): every family's pictures composed by draw, and the setup blocks.
    /// Its proof is every family's and the setup blocks', with the composition's own sources.
    /// </summary>
    internal static readonly ClosedPresentationContract Composed = new(
        "SuperMetroid.Core.World.RoomObjectVisuals", "room-object-frames-composed-complete", ["Image", "SetupBlock"],
        [
            new("csharp/src/SuperMetroid.Core/World/RoomObjectVisuals.cs"),
            new("csharp/src/SuperMetroid.Core/World/RoomObjectArtwork.cs"),
            new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/IRoomObjectFamilyPictures.cs"),
            .. Families.Append(SetupBlocks).SelectMany(contract => contract.Sources).DistinctBy(source => source.Path),
        ]);

    /// <summary>Every room-object picture proof: each family's, the setup blocks' and the composed read's.</summary>
    internal static readonly ClosedPresentationContract[] All = [.. Families, SetupBlocks, Composed];
}
