namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "2AF1A093EA6E63501107800DE9D4737250AECEE847AFE66EE5D1A4F3311D5F91"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "8AE7EEF367A1BF265186D296B5315E675474FC5CB1242F52C430DB4F8494588A"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "2301E9CFE817C517847B018A7A4323FAD9DC242732C39873AC5FCAD3C595A856"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "8DCEC2C354E6F989E7DF06938BEA0A58A8771282D418E4964935F77AD0CB47B5"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "37C2F538981CC2AC5F7975E23FB23CDA085FBE955455B093445CD429B9030F3D"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892")]),
    ];
}
