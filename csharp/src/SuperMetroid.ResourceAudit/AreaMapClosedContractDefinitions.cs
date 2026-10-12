namespace SuperMetroid.ResourceAudit;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.AreaMapPresentationCatalog", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),
    ];
}
