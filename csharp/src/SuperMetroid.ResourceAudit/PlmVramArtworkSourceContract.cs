namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "D706327F64E76EF1904738CD6F4694B29DB8594098162908445A2D2C9FE55807"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "CBA810140BD14FA6B95FA2058B8646195D722DD5E1E478B51C0D809CF7C6D7D7"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "089FFD24D5F854A30042F9B7E68D8C97C346745B186C1A1EE018D6BAB176962D"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "95FBAC10E5001382B9835BEA8A76864ACD29F7802DBAB2398D6C2021EFD2206B"),
    ];
}
