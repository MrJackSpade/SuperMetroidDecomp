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
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "50424F1949ED3D0DD46BD79E4B6840E688E5C92696CBD5E3B0B732E4F24F1D87"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "71167DC60C5A6F3A10DC31C57A1D2A7131614DEA90550C9AC133A1D44AF8B5FA"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "CBA810140BD14FA6B95FA2058B8646195D722DD5E1E478B51C0D809CF7C6D7D7"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "089FFD24D5F854A30042F9B7E68D8C97C346745B186C1A1EE018D6BAB176962D"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "95FBAC10E5001382B9835BEA8A76864ACD29F7802DBAB2398D6C2021EFD2206B"),
    ];
}
