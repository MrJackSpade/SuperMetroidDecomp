using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    /// <summary>Shared loader reviewed by each enemy BG2 transfer contract.</summary>
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyBg2FrameCatalog.cs");
    /// <summary>Shared frame layout reviewed by each enemy BG2 transfer contract.</summary>
    private static readonly ReviewedSource SharedBg2Layout = new(
        "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySpritemapDefinition.cs");

    /// <summary>Closed contracts for boss BG2 frames, room FX transfers, ending fragments, and gunship takeoff artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        Bg2("Phantoon"),
        Bg2("Draygon"),
        Bg2("Crocomire",
            new ReviewedSource("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/CrocomireBodyVisualDefinitions.cs")),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainBodyBg2FrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainBodyVisualDefinitions.cs")]),
        new("SuperMetroid.Core.Fx.Artwork.RoomFxLayer3Pages", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLayer3Pages.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLayer3TilemapFormat.cs")]),
        new("SuperMetroid.Core.Fx.Artwork.RoomFxBlendInks", "room-fx-complete-eight-blends-and-zero-clear", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxBlendInks.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxPaletteBlendDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxPaletteBlendRules.cs")]),
        new("SuperMetroid.Core.Frontend.Ending.Artwork.EndingObjectArtwork", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingObjectArtwork.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffTransferDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IInstalledArtworkContent.cs")]),
    ];

    /// <summary>Includes the common BG2 sources and the named boss's catalog and frame definitions, which live in its boss's Artwork module.</summary>
    private static ClosedPresentationContract Bg2(string family, params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Bosses." + family + ".Artwork." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout,
         new("csharp/src/SuperMetroid.Core/Bosses/" + family + "/Artwork/" + family + "Bg2FrameCatalog.cs"),
         new("csharp/src/SuperMetroid.Core/Bosses/" + family + "/Artwork/" + family + "Bg2FrameDefinitions.cs"), .. additionalSources]);
}
