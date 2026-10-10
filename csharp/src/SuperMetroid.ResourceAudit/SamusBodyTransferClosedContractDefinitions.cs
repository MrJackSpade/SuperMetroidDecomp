namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "109C60BC93864BEA7F6E0AD7D8424A037D69F3DE67570F3D0AA1E110D967F3DE"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "836BE1CAB9EA0038E60CCEFC22BBDE1A86A0165DEFB0E2E140133082E5051D03"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "2D75B9A6939185909469C37CF1711ECCA5BA59B1FA1CC137E81D4A88A8168AF3"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPlacementDefinitions.cs", "CD31B4A22A838F3086529C7257A69DC969087C0BA393593499CDE625C287FE4D"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "EBEF3087A9892090B00AB17D5FBC0827B40D6D4344551B60A52C40561A00D184"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "996DFE3E185EF497B98E86164EE3A705AFD90FD2BEC63B93DA360CBC1A8ACC38"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs", "A4A4E2BC0F3E5D766D117C462636FA81EFC1ABB483637CCAEFCE6652D508311E"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "D33647C4EFA8D1FA7B508722BAEEC29071B2494496B226F509C7F939F5FA30E0"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "21700346BC5B9F4AE863364301BF30BE52C32F171E0815E1D394E917AE00417A"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "F2C3949295407224792482DD7176681FEDCC7E1F38AD089FA430E307D67A1C61"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs", "3A549ADC312838892F1416612266DC4B16E2E4B77DAE100E8B194E2F1CF27585"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "44FE41A6F5B2C05B8F372C9204C8116F60F984B199A4E6835C83F703BBFB7D2A"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "4BA7312382B06BF65CF7F09D96CB9947E58EEBA63E3A01CD68D748E8CECE6804"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "B0EF6EB7D1154ADDE345A678959A5FCC97EB94E3555B2633864B1DB7C24B3445"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "3513BD799155CE8428C97652B5FFF68A8B1CC5175AF40958742BF6D1C6054B54"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "7BF84FD6D6B3185B670FC2DB0C8951B53D886A8B4547080C35DC6C7E909B0849"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "508F338277A39A05D1257D29F9E38F03AC902F004543FBEC1E0075DE4E3D9DDD"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "AEC295EEC2013263835DA5FCA9BE81604F6A8D3B8550CDBDCDB0999ABE335403"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "9ABD1C3A883B639287F5E273F0DDAE5CCBFDAA860E68E7482A8B30E490CC15D8"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "82D88C7F296DD00736D81FBD300F77FFA928107C72F99CE40EA0D763521AF3A0"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileWords.cs", "53E2BCCED161FB8351D7D6FD5BC537006B9C6E53E2A3030898CC8009A38D3B99"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs", "F28577FD0536F007A2458EFA116C51AFD3D2A4781637EC38B40A0647EC0569A0")]),
    ];
}
