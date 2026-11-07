using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class RoomObjectProgressionClosedContractDefinitions
{
    /// <summary>Common PLM draw-list shape reviewed by the progression artwork contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ShotBlockObjectDraws.cs");
    /// <summary>The terrain blocks PLM draws share.</summary>
    private static readonly ReviewedSource SharedTerrainBlocks = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTerrainBlocks.cs");
    /// <summary>Elevatube definition also reviewed for Kraid-room visual membership.</summary>
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/MaridiaElevatubeObjectDefinitions.cs");

    /// <summary>Closed visual domains for progression blocks and boss-room PLM draws.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Areas.Tourian.Artwork.RoomObjectTourianAccessVisuals", "plm-tourian-access-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/RoomObjectTourianAccessVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraws.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSpeedBoosterVisuals", "plm-speed-booster-complete-reveal", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectSpeedBoosterVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SpeedBoosterBlockObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SpeedBoosterBlockObjectDraws.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectMaridiaElevatubeVisuals", "plm-maridia-elevatube-complete-draw", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectMaridiaElevatubeVisuals.cs")]),
        new("SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomObjectSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/RoomObjectSporeSpawnCeilingVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/SporeSpawnCeilingArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectSamusEaterVisuals", "plm-samus-eater-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectSamusEaterVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SamusEaterObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SamusEaterObjectDraws.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBotwoonWallVisuals", "plm-botwoon-wall-complete-clear", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectBotwoonWallVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BotwoonWallObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/BotwoonWallObjectDraws.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectKraidVisuals", "plm-kraid-room-complete-draws", ["GetAppearance", "Frame", "Append"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectKraidVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/KraidRoomObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/KraidRoomObjectDraws.cs")]),
        new("SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomObjectCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/RoomObjectCrocomireVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/CrocomireArenaArtwork.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomObjectMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomObjectMotherBrainFakeDeathVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainFakeDeathArtwork.cs")]),
    ];
}
