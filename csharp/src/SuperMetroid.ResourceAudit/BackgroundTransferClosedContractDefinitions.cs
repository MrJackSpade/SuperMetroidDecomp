namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    /// <summary>Stores shared bg2 loader.</summary>
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs");
    /// <summary>Stores shared bg2 sequence.</summary>
    private static readonly ReviewedSource SharedBg2Sequence = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinitionSequence.cs");
    /// <summary>Stores shared bg2 layout.</summary>
    private static readonly ReviewedSource SharedBg2Layout = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinition.cs");

    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        Bg2("Phantoon"),
        Bg2("Draygon"),
        Bg2("Crocomire",
            new ReviewedSource("csharp/src/SuperMetroid.Core/Assets/CrocomireBodyVisualDefinitions.cs")),
        new("SuperMetroid.Core.Assets.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyBg2FrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyVisualDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.RoomFxLayer3TilemapCatalog", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxLayer3TilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxAtmosphereTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxSporeTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxLiquidTilemapDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog", "room-fx-complete-eight-blends-and-zero-clear", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxPaletteBlendCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs")]),
        new("SuperMetroid.Core.Assets.EndingObjectArtworkCatalog", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingObjectArtworkCatalog.cs")]),
        new("SuperMetroid.Core.Assets.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),
    ];

    /// <summary>bg2.</summary>
    private static ClosedPresentationContract Bg2(string family, params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Assets." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameCatalog.cs"),
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameDefinitions.cs"), .. additionalSources]);
}
