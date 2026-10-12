namespace SuperMetroid.ResourceAudit;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.AreaMapPresentationCatalog", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs", "76E7E8B33D3D47A35942DBB543990D96FE14C93EE90C5D8596CAF39CDC1157E9"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs", "91CC78FC8EE0DF2DA2F103C4A652CF4179C1EDD94813C6D5EDF87D76D70C088E"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapStockRules.cs", "5979DB0A235235DBC53743E2AFA7FCA07F023D2EA3C5D35CE365490F651C24A9"),
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "2AF1A093EA6E63501107800DE9D4737250AECEE847AFE66EE5D1A4F3311D5F91"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs", "31F356AC14B07054376FE7AA397919D2D426BCC96EB9B0BEECDF9D632716F28F"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "8AE7EEF367A1BF265186D296B5315E675474FC5CB1242F52C430DB4F8494588A"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs", "4EBDEB3A4BA4DAC84CAD50FFAF2EFE4A38DFB1A7C9FE484C55008DC414F3BB3A"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "368BAF27A59AD317E14B4D907C23EA5BB2438547E593FC781712960F529E631B")]),
    ];
}
