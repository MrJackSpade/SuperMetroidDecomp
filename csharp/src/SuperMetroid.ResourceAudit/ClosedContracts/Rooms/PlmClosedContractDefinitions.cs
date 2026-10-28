using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    /// <summary>Common draw-list structure reviewed for all five bank-$84 PLM visual catalogs below.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");

    /// <summary>Limits shot-block, station, Torizo hand, Mother Brain glass, and Noob Tube draws to reviewed artwork owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockVisualCatalog.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmStationVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmStationDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.Torizo.Artwork.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/RoomPlmBombTorizoHandVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/BombTorizoHandArtwork.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmMotherBrainGlassVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainGlassPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainGlassDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainGlassArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmNoobTubeVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/NoobTubePlmDrawDefinitions.cs")]),
    ];
}
