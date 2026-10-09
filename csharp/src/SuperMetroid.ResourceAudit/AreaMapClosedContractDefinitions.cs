namespace SuperMetroid.ResourceAudit;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.AreaMapPresentationCatalog", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs", "76E7E8B33D3D47A35942DBB543990D96FE14C93EE90C5D8596CAF39CDC1157E9"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs", "6983B013E9087483AAC896A9F95D08BBD5FE9A96F9BEB036815F04FB0E7862BA"),
             new("csharp/src/SuperMetroid.Core/Assets/AreaMapStockRules.cs", "FC95BDC4F7ECDCFA0B448BF443273311D45B437C31DBE9CFE0D1F037CC2D4B8F"),
             new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "7E13642A0E1B95613BC2F4B1C51082711C43BBDCAF0A4B7EE58F686280E9E871"),
             new("csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs", "9AE09DBDAE455EAFD608A18F87FA18BA0664618F0484BD2BE43FA7C7CC267719"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "18E30F613BFAD79AB0C4B8093D1041749062C6BFFA57B347C69138FED38BAD21"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "22A4DAF4C90D2F9B1779E7EB0D2722FE7B28AD42886744F9139276CB86FB352F"),
             new("csharp/src/SuperMetroid.Core/Game/KraidBackgroundRomData.cs", "A7E09B8EDE22BFD4933EC6D87A5F4615AD0B5A0BBBC6286F6DFCDBCBEBAAF0D0"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "368BAF27A59AD317E14B4D907C23EA5BB2438547E593FC781712960F529E631B")]),
    ];
}
