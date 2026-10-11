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
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "01E5B5A246ABBC76762AEEF6C385E711F4A4574BD159F35FF3CEBFD7F64A4BCF"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs", "11876A7680FA1FD848378E03CF5F0CC655CEC8ECCC9698AF98041820B287F19C"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "46CAAD11C6CEAAB7858AAD573FFEDEEF320370577993C861F7171EFE6FE80DFA"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs", "A7E09B8EDE22BFD4933EC6D87A5F4615AD0B5A0BBBC6286F6DFCDBCBEBAAF0D0"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "368BAF27A59AD317E14B4D907C23EA5BB2438547E593FC781712960F529E631B")]),
    ];
}
