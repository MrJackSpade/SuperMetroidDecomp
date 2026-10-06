namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "F52A359EFB79D9FEA24B7D4569A4AD1467DD8A94B90CDA79CE3A7BE4654B9C74")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "4030EC86F2F18FB11FA9481298140D0D2B96F6E17DC45408FB81C48C8F9D6E7E"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "4942E6802A8F4B086F7C8D3F4A0FD3530E1C07B5C2F467D97CF76FE6855A1366"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "F52A359EFB79D9FEA24B7D4569A4AD1467DD8A94B90CDA79CE3A7BE4654B9C74")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "8852546A00BF49F92815DBD382003DDFAF53EC723CED1108EFCD497DFB53E77B"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "96DE8AB068919A49D73E1EAAE74A6988A03D4B0DE7F13131F1A480EC484C2072"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "78E2DAA6A7756788C23DD272EF811D7A1EF50BC5F497E51893309C8C452BDFE4"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "F52A359EFB79D9FEA24B7D4569A4AD1467DD8A94B90CDA79CE3A7BE4654B9C74")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
    ];
}
