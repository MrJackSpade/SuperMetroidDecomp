namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "CC2C0E776E97A7007626349833DB951FAF75DB85E3ABA87BE7DC2ABE84C02734"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "A4351C192178EE82E2CD2ABE00E4AE6DC4DFA74A31DCE647E139B85DED510593"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPlacementDefinitions.cs", "C9212B7385C216433030D71358173A9A320C76087172B5DC5D282AA24C985769"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "C5FC4C4CCFD7ACFAA8833A33DF0F302CC1A6D5FC367433B05569D64B09C9942B"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "012612D5038094238C5FF9442742D744204A07379B433D0B5F9A825419C1A524"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs", "7192C8E0506810E4E3C3B385E51B8A6608A0D1C6DB9AF7072CD25FBE3770C648"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "8C3C72A4F5EC8950B3BC033D85C01FA0D5AB7F9CF393A12E9EAB14F128F242E1"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "800428CC9CEA5E27F304564F96E085A2BF4A0426B9C7CD233090EA4046295E2D"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "92B942C3417DA086E5CDCF45B2594A28B339E96AF222EFBC5035DCB6DBEBB8DE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs", "0270E426620D8301AA82027E2000EECD3048B3F0E3E004DA8D07FC4729613331"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "F72F36369363F143F95D6138A5A1CD7F1D7D56170B476123F5FC506D9888DBF7"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "F4FCCF227D6D58676E8565799A8F66F74424BFD01E6FA245C0635B76C810CE0C"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileWords.cs", "06A4FA2F98E9104B4CBA39021A8270A158EEB35F6E86124C362640590E6B2F03")],
            "Construction preserves supplied selector edits over named defaults and exact shared frame components, and requires every physical seven-byte record between sorted set starts and the fixed exclusive end, rejecting missing, duplicate or malformed groups. Definitions are indexed independently of exposed group arrays. Frame checks its selected top/optional bottom definitions before publication; DefinitionAddress validates and returns the same physical identity, preserving native cross-group and cross-half arithmetic. The reviewed transfer producer stores only these validated identities for DefinitionAt. This establishes admitted valid-domain resource availability, not arbitrary animation counters, edited invalid selectors, corrupted or incompatible saved pointers, installation, timing or pixels; those invalid inputs still fail loudly. Unselected backing words are deliberately not required to be valid frame records."),
    ];
}
