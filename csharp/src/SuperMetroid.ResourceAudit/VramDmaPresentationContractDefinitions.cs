namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "01E5B5A246ABBC76762AEEF6C385E711F4A4574BD159F35FF3CEBFD7F64A4BCF"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "46CAAD11C6CEAAB7858AAD573FFEDEEF320370577993C861F7171EFE6FE80DFA"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "18A5E04D1B6251E37A73A0BB0EC348FFF3F2548B07CB2E70F2B84573572F7067"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "8DCEC2C354E6F989E7DF06938BEA0A58A8771282D418E4964935F77AD0CB47B5"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "37C2F538981CC2AC5F7975E23FB23CDA085FBE955455B093445CD429B9030F3D"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
    ];
}
