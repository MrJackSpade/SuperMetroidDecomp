namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "6A538FDB8C87B18D17F1FAC767B8C71259822D8730CFB6E2B7AB3FC8AC4AB708"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "1E9EA019CB38CA024B15EC658037EBABAB24F0CF5D43411760C2FD1EF05F34B5"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "2DC96921E97617DFC6695A579904C86938F57C9725BE73379D426F68995D1749"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPlacementDefinitions.cs", "F05C6645CBC1D4419166D361C1865BDB6E611CBC7816ADDE00ABFD28D01C7098"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "C5FC4C4CCFD7ACFAA8833A33DF0F302CC1A6D5FC367433B05569D64B09C9942B"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "012612D5038094238C5FF9442742D744204A07379B433D0B5F9A825419C1A524"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs", "7192C8E0506810E4E3C3B385E51B8A6608A0D1C6DB9AF7072CD25FBE3770C648"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "EC89958443968CDA449D4A655F6302A874E79518F313AD8B165D48ECAAFAA703"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "AA031FE0F4ED1119B8D27B9394152B8D6FDE63516D0339C9D97D802D034B3885"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "92B942C3417DA086E5CDCF45B2594A28B339E96AF222EFBC5035DCB6DBEBB8DE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs", "9CE41DB7A4F1094110987965C4A903C84601AA41ADC00CC291CD526D3CAA8F23"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "EAEA4FC902BC3CC212C384D16FE371541373972A230EFD9A76306B4CF82C45EF"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "DFD26B6C54E96889B010186D0B65795BBFB9BD68621C2EF4FC379690D6AC9894"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "5B6B3512518F81E9DBD595B5465786FD5CA903564D5E6B3C233B183B61B378DC"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "D5EFAD79D85614FC4E6A2456329D303A154AC3AEBAD7922F4A3EE3302BC2C2E2"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "48F98641513B038D3B4A73EBD5D6F46F17DE8F22B74DA5D91B7B883CFD758FDE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "BBB348752ABA20A9A13CD0D6E17A4A15B35E4DA199F5D9F54701A277D99C85A4"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "4E51DF248648BBAC3C31C3C9312C4B4F2A4DF8BCF97520217C7183640547465E"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "452B184D6FC9F55826EBDF8AB90EDA286A900B50B4CA0633C79D065B063E6F9C"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "DC898B112A4DBB7C9C0B9445545C637F2EE37C241D6757152659A5EE7FF6096D"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "F4FCCF227D6D58676E8565799A8F66F74424BFD01E6FA245C0635B76C810CE0C"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileWords.cs", "324E1D753EE30378467C48EB8AF65E17986E92FF8C2AAA3F2B544D8F6AEFAA53"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs", "ACF9C408B2C9F81C129C1215D65542A846D5CEE3EE8BF9292777D3DF37794DDA")],
            "Construction preserves supplied selector edits over named defaults and exact shared frame components, and requires every physical seven-byte record between sorted set starts and the fixed exclusive end, rejecting missing, duplicate or malformed groups. Definitions are indexed independently of exposed group arrays. Frame checks its selected top/optional bottom definitions before publication; DefinitionAddress validates and returns the same physical identity, preserving native cross-group and cross-half arithmetic. The reviewed transfer producer stores only these validated identities for DefinitionAt. This establishes admitted valid-domain resource availability, not arbitrary animation counters, edited invalid selectors, corrupted or incompatible saved pointers, installation, timing or pixels; those invalid inputs still fail loudly. Unselected backing words are deliberately not required to be valid frame records."),
    ];
}
