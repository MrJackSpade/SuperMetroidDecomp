namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "F6A7375060930B0782A2E38370E943F5DA2D514E02D4E593597994F55528B6ED"),
        // Restored beam routing admits only five exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "F1ECEE3ECFD650E9203E9C2A7C8CCAC6E86A47196EFCE839603C3E94A68DC161"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "429F58C4B5E87C3C6051CD9E3F8EBFEFAA4692760B7B4A6FA12178442F452B7B"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "CBA810140BD14FA6B95FA2058B8646195D722DD5E1E478B51C0D809CF7C6D7D7"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "089FFD24D5F854A30042F9B7E68D8C97C346745B186C1A1EE018D6BAB176962D"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "95FBAC10E5001382B9835BEA8A76864ACD29F7802DBAB2398D6C2021EFD2206B"),
    ];
}
