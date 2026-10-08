namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "F6A7375060930B0782A2E38370E943F5DA2D514E02D4E593597994F55528B6ED"),
        // Restored beam routing admits only seven exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "1126B27483E6BD7707D0151E78885148E886554DB19B98E4DFAD215F6026B5EA"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "E04B3B0C46A207140CFE3419E8DBF84CFCDC67316536E87C56262DF3BC9ACF39"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "41D81669A818AA966D13EE1FDC61BD9E4D9168928E9347F8BB854325E8FD4F55"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "684564292FA2DEAE104D897E1B7210F65CC55B6799F1970C5A654E2E41968E9B"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "95FBAC10E5001382B9835BEA8A76864ACD29F7802DBAB2398D6C2021EFD2206B"),
    ];
}
