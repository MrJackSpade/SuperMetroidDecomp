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
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "50181E5C609316DD0F2C695663588BB09571977292EA5C829505D768E2DBDEED"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "DDFAAF3A4D7581D3C3238C8444ADCD45EC6E1FFF350390A5EE87C365DE41416D"),
        new("csharp/src/SuperMetroid.Core/Hardware/VramWriteQueue.cs", "166966888B22696A68535EE40C1A53EB0D612B9F18F02344B7376F79C35A1DDB"),
        new("csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs", "BB5147E13E3CF77F422710E023E5BD307BE7AD9AED2E7BFE0C7A2A41C0560C9A"),
        new("csharp/src/SuperMetroid.Core/Hardware/SnesDmaSourceMap.cs", "97A3BE0CE8E9BA9AD71F1385D30CC23BEDE5965A6EAA078F81E54FABBBB37E2F"),
    ];
}
