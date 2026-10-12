namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/RuntimeArtwork.cs"),
        // Restored beam routing admits only seven exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs"),
    ];
}
