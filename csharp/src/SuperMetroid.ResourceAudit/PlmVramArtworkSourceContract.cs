namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    /// <summary>Pinned sources defining the installed PLM artwork transfer pipeline.</summary>
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "5DCEB3C73E7EBCB4F9B9BA7661524B8C22A779A9093069F2EF4C2B1E4C2225BA"),
        // Restored beam routing admits only seven exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "8E1F84FC08BB7FD11FB5E6D9797E57CA52C59CE26D5E862F15FE746ED4E44540"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "09BE38EDA993166BFF3D12810EC4D8DC6E937D8D2804FEF4AE112F7D083FC370"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "166966888B22696A68535EE40C1A53EB0D612B9F18F02344B7376F79C35A1DDB"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "255CA0175D81599BBB9B2E58072637946E4158FF506412EF48EC9FBDE1623DD0"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "97A3BE0CE8E9BA9AD71F1385D30CC23BEDE5965A6EAA078F81E54FABBBB37E2F"),
    ];
}
