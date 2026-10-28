using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    /// <summary>Common PLM draw-list shape reviewed by the progression artwork contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/ShotBlockPlmDraws.cs");
    /// <summary>The level words PLM draws share.</summary>
    private static readonly ReviewedSource SharedLevelWords = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmLevelWordDefinitions.cs");
    /// <summary>Elevatube definition also reviewed for Kraid-room visual membership.</summary>
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/MaridiaElevatubePlmDefinitions.cs");
    /// <summary>The Maridia elevatube draws' artwork names.</summary>
    private static readonly ReviewedSource ElevatubeDraws = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/MaridiaElevatubeDraws.cs");

    /// <summary>Closed visual domains for progression blocks and boss-room PLM draws.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Areas.Tourian.Artwork.RoomPlmTourianAccessVisuals", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/RoomPlmTourianAccessVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSpeedBoosterVisuals", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmSpeedBoosterVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SpeedBoosterBlockPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SpeedBoosterBlockPlmDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmMaridiaElevatubeVisuals", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords, ElevatubeDefinition, ElevatubeDraws,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmMaridiaElevatubeVisuals.cs")]),
        new("SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/RoomPlmSporeSpawnCeilingVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmSamusEaterVisuals", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmSamusEaterVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SamusEaterPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SamusEaterPlmDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBotwoonWallVisuals", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmBotwoonWallVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/BotwoonWallPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/BotwoonWallPlmDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmKraidVisuals", "plm-kraid-room-complete-draws", ["GetWord", "Frame", "Append"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords, ElevatubeDefinition, ElevatubeDraws,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmKraidVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/KraidRoomPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/KraidRoomPlmDraws.cs")]),
        new("SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/RoomPlmCrocomireVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmMotherBrainFakeDeathVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathPlmDrawDefinitions.cs")]),
    ];
}
