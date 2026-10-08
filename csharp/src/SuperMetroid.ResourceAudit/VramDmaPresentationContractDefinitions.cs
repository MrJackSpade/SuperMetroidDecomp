namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "44C416557FCF09011152213B3F15BB52517C1999175A375DA338AA293E5DED09")]),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "E2A5D6F4B5EF50E10D293A29679D4B64F3573FD140426DFB90A553ABBD67FE15"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "4942E6802A8F4B086F7C8D3F4A0FD3530E1C07B5C2F467D97CF76FE6855A1366"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "44C416557FCF09011152213B3F15BB52517C1999175A375DA338AA293E5DED09")]),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "C6047AC3238CA2C39BC3739E8DAEDFDBE51767CE61966A17D5038898652D58E1"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "BF392FFF6867BD74D5EFCB4C9121861AF95D4A9B5E4A71962DB5F4ED1F2EAEDE"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "78E2DAA6A7756788C23DD272EF811D7A1EF50BC5F497E51893309C8C452BDFE4"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "44C416557FCF09011152213B3F15BB52517C1999175A375DA338AA293E5DED09")]),
    ];
}
