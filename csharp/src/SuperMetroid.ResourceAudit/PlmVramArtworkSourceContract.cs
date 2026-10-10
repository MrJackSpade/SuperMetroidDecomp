namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed installed-transfer pipeline for the bank-$84 DMA audit.</summary>
internal static class PlmVramArtworkSourceContract
{
    internal static readonly ReviewedSource[] Sources =
    [
        .. EnemyArtworkClosedContractDefinitions.All[0].Sources,
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "85E7C3E51A79FAF823B8AB20EE0D591524D7EF3F2A5C66FA0CDA03A302DE9585"),
        // Restored beam routing admits only seven exact $9A sources of 256 bytes;
        // it cannot intercept the existing PLM artwork pages.
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "7F1869C59DF857C747FBB097D8A7FDEF61A370F52031EEFC3715B0A005C1FC16"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "DC1739CCBE41B7B352D88EF920DF5D7E7A4D81180CEED3FA4CF6D3E323B1F2E1"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "166966888B22696A68535EE40C1A53EB0D612B9F18F02344B7376F79C35A1DDB"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "546088CADBD169197A07D944BB3FDA4849A7C1D38555B3DAD17A4FE666C29085"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "97A3BE0CE8E9BA9AD71F1385D30CC23BEDE5965A6EAA078F81E54FABBBB37E2F"),
    ];
}
