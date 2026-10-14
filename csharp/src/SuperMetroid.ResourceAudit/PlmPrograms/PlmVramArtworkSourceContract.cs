using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    /// <summary>Limits reviewed bank-$84 DMA producers to enemy artwork, runtime uploads, exact beam sources, and their queue and extraction owners.</summary>
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/RuntimeArtwork.cs"),
        // Restored beam routing admits only seven exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamTileCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamTileAtlasDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyTileArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs"),
    ];
}
