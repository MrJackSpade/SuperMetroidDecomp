namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "AFE95F8424D21D6E7FD5383C25E0F396C17D0AFE8D7AB48EE6C2FD70E331FAF8"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "5E12AE1C0B8D2CB64A026992A5794CE078C27BF3CD93A35FBBE520CD4AB1B023"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "6AA48BB79454BC395893427AF9197E1282758E55CFA11DBE22E8F11BB52D4DEC"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPlacementDefinitions.cs", "5B6A6AF7A8D2B2017C1EEE58309048120D62553C3BDFB53357079BA3FDCB3DA0"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "CC7B22A530795D32AA66349CDD8DF6F81F1D92FC4C1F5974B497288952227A8F"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "A3807AAA45D8EF8F1454F35F6EA8395A5A8F7A99A2274B442B29988CBD1F2E13"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs", "5868E5D2E3143148816B22FEE514C59F95EBB35AE6DAA00947D6566DA50251F1"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "D33647C4EFA8D1FA7B508722BAEEC29071B2494496B226F509C7F939F5FA30E0"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "21700346BC5B9F4AE863364301BF30BE52C32F171E0815E1D394E917AE00417A"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "F2C3949295407224792482DD7176681FEDCC7E1F38AD089FA430E307D67A1C61"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs", "3BCEC4134B6E790D496A5ACCA6DF5EF34143D65D559B15B4D2696B67EE51760A"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "45CD6CD6EE78F55F57674E3409410C931B585412D6B9AC96DD2AE394AB663D81"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "4B0B9976585AB8D26F7F6E7A08F3F573B8CE08CFA7E67D5837EF8FD9BE6D658A"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "A3EBC9C3919859C3467276DAF51D07B5E990E38354A305B6563ED0101627AD71"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "7BB7DA3506A41E4C6113041361FF042D588B980E9B56B5967DEC2A3169510B3B"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "177A9BDB5FA1B19108C1AD208CB40BE9C61573D6A74CEAC2432B6C55B11B9389"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "0AFFEFC57566447448A64D036D25589EC4F458778B75C15BF303EE8BCB345A3B"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "9BCF1BD6BBAC548A8680F964FD043C416F569DB2ED2460F3E792DC0906D4AC52"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "EE9D40C31A450BAFF376746F319B29B63D0B15308A6CBD85DCE3861DDB263247"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "E777F900E1075C08E1ACF2921E38547ECDC81151C3BF24559B5BDDD0FE0D0DD4"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileWords.cs", "53E2BCCED161FB8351D7D6FD5BC537006B9C6E53E2A3030898CC8009A38D3B99"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs", "213CF362830CF39C5AD0536EC990CAA631487096FDE2ED20F518EB9EAB06E2E6")]),
    ];
}
