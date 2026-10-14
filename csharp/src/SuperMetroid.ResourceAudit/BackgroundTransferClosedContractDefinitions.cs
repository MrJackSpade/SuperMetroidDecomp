namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    /// <summary>Shared loader reviewed by each enemy BG2 transfer contract.</summary>
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyBg2FrameCatalog.cs");
    /// <summary>Shared sequence shape reviewed by each enemy BG2 transfer contract.</summary>
    private static readonly ReviewedSource SharedBg2Sequence = new(
        "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyBg2FrameDefinitionSequence.cs");
    /// <summary>Shared frame layout reviewed by each enemy BG2 transfer contract.</summary>
    private static readonly ReviewedSource SharedBg2Layout = new(
        "csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyBg2FrameDefinition.cs");

    /// <summary>Closed contracts for boss BG2 frames, room FX transfers, ending fragments, and gunship takeoff artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        Bg2("Phantoon"),
        Bg2("Draygon"),
        Bg2("Crocomire",
            new ReviewedSource("csharp/src/SuperMetroid.Core/Bosses/Crocomire/Artwork/CrocomireBodyVisualDefinitions.cs")),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainBodyBg2FrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainBodyVisualDefinitions.cs")]),
        new("SuperMetroid.Core.Fx.Artwork.RoomFxLayer3TilemapCatalog", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLayer3TilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxAtmosphereTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxSporeTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxLiquidTilemapDefinitions.cs")]),
        new("SuperMetroid.Core.Fx.Artwork.RoomFxPaletteBlendCatalog", "room-fx-complete-eight-blends-and-zero-clear", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxPaletteBlendCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/RoomFxRomData.cs")]),
        new("SuperMetroid.Core.Frontend.Ending.Artwork.EndingObjectArtworkCatalog", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingObjectArtworkCatalog.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/GunshipLiftoffArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),
    ];

    /// <summary>Includes the common BG2 sources and the named boss's catalog and frame definitions, which live in its boss's Artwork module.</summary>
    private static ClosedPresentationContract Bg2(string family, params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Bosses." + family + ".Artwork." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
         new("csharp/src/SuperMetroid.Core/Bosses/" + family + "/Artwork/" + family + "Bg2FrameCatalog.cs"),
         new("csharp/src/SuperMetroid.Core/Bosses/" + family + "/Artwork/" + family + "Bg2FrameDefinitions.cs"), .. additionalSources]);
}
