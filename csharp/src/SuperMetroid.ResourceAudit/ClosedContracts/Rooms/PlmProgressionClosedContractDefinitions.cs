using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed progression/boss-room artwork domains, independent of PLM mechanics or event timing.</summary>
internal static class PlmProgressionClosedContractDefinitions
{
    /// <summary>Common PLM draw-list shape reviewed by the progression artwork contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");
    /// <summary>Elevatube definition also reviewed for Kraid-room visual membership.</summary>
    private static readonly ReviewedSource ElevatubeDefinition = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/MaridiaElevatubePlmDefinitions.cs");

    /// <summary>Closed visual domains for progression blocks and boss-room PLM draws.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Areas.Tourian.Artwork.RoomPlmTourianAccessVisuals", "plm-tourian-access-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/Artwork/RoomPlmTourianAccessVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmSpeedBoosterVisualCatalog", "plm-speed-booster-complete-reveal", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmSpeedBoosterVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SpeedBoosterBlockPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmMaridiaElevatubeVisualCatalog", "plm-maridia-elevatube-complete-draw", ["GetWord"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmMaridiaElevatubeVisualCatalog.cs")]),
        new("SuperMetroid.Core.Bosses.SporeSpawn.Artwork.RoomPlmSporeSpawnCeilingVisualCatalog", "plm-spore-ceiling-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/Artwork/RoomPlmSporeSpawnCeilingVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmSamusEaterVisualCatalog", "plm-samus-eater-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmSamusEaterVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/SamusEaterPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmBotwoonWallVisualCatalog", "plm-botwoon-wall-complete-clear", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmBotwoonWallVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/BotwoonWallPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmKraidVisualCatalog", "plm-kraid-room-complete-draws", ["GetWord", "Frame", "Append"],
            [SharedDrawShape, ElevatubeDefinition,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmKraidVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/KraidRoomPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.Crocomire.Artwork.RoomPlmCrocomireVisualCatalog", "plm-crocomire-arena-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/RoomPlmCrocomireVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmMotherBrainFakeDeathVisualCatalog", "plm-mother-brain-fake-death-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmMotherBrainFakeDeathVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathPlmDrawDefinitions.cs")]),
    ];
}
