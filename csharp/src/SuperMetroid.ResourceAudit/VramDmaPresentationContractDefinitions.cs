namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "6A0F8917D9FB2B55391C61341DBB0E03901A2FE21AB009AFBE63D41F8E54FFDA")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "A27811EFCBD8940CD241D7C3498E5DDD8EC39B3906CC523E93BFC4248DA58F9E"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "6A0F8917D9FB2B55391C61341DBB0E03901A2FE21AB009AFBE63D41F8E54FFDA")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "6CC0DC440CF5944DAB7C3B07603FA7D1D2EC032B12928E2799C2273229F101FA"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "6A0F8917D9FB2B55391C61341DBB0E03901A2FE21AB009AFBE63D41F8E54FFDA")],
            "Private PNG construction retains exact planar geometry. TryResolve accepts only declared exact source/count pairs and resolves the same bytes as typed uploads. Unowned sources/counts validly return false. This proves installed membership, not nullable runtime binding, corrupted saved operands, NMI timing or rendered pixels."),
    ];
}
