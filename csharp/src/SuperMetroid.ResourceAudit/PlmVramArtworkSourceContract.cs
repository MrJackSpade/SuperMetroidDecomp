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
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "5772BCE58E4D673C2D2847858318E08064A5E8D7842E7D2039F3F069DCA7A985"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "E04B3B0C46A207140CFE3419E8DBF84CFCDC67316536E87C56262DF3BC9ACF39"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "CBA810140BD14FA6B95FA2058B8646195D722DD5E1E478B51C0D809CF7C6D7D7"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "089FFD24D5F854A30042F9B7E68D8C97C346745B186C1A1EE018D6BAB176962D"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "95FBAC10E5001382B9835BEA8A76864ACD29F7802DBAB2398D6C2021EFD2206B"),
    ];
}
