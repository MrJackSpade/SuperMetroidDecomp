using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    /// <summary>Common draw-list structure reviewed for all five bank-$84 PLM visual catalogs below.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/ShotBlockPlmDraws.cs");
    /// <summary>The level words PLM draws share.</summary>
    private static readonly ReviewedSource SharedLevelWords = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmLevelWordDefinitions.cs");

    /// <summary>Limits shot-block, station, Torizo hand, Mother Brain glass, and Noob Tube draws to reviewed artwork owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmShotBlockVisuals", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmShotBlockVisuals.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmStationVisuals", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmStationVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmStationDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/StationPlmDraws.cs")]),
        new("SuperMetroid.Core.Bosses.Torizo.Artwork.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/RoomPlmBombTorizoHandVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/BombTorizoHandArtwork.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmMotherBrainGlassVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainGlassPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainGlassDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainGlassArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmNoobTubeVisuals", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmNoobTubeVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/NoobTubePlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/NoobTubePlmDraws.cs")]),
    ];
}
