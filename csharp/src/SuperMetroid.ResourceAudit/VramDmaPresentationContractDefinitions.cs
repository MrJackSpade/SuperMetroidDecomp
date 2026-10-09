namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "7E13642A0E1B95613BC2F4B1C51082711C43BBDCAF0A4B7EE58F686280E9E871"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "18E30F613BFAD79AB0C4B8093D1041749062C6BFFA57B347C69138FED38BAD21"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "22A4DAF4C90D2F9B1779E7EB0D2722FE7B28AD42886744F9139276CB86FB352F"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "49C7E5915AEFAA06AAC4FE3F2BC702B7246DCBADA645765686F9F7AF3D83510F"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "B13EFD92BEDC85CF65A4BC435061C3A6BD0ED1893CF8B2C5A36D810AB2943331"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "BD5C4C72DCD287EB066548250C87C85CF59F8D467B694FEE0D7595E682A69B8A"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
    ];
}
