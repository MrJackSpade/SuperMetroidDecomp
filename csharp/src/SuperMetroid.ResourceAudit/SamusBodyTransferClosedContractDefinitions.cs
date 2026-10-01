namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "E666CEBCF6BA4F1ECD82464A8377F2A5FD80CB5AC709AA3D0D2F2DA0CA73F4AD"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.ContentIdentity.cs", "A9BCEECECD5218005E8DCB9880BB5B4E85474BF11D6671366082FBDABB63B8B8"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "54473AB54B5BAB712905D8838BA204D60D83AA47A699AFD9EE888CA8CF1E2066"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs", "92B942C3417DA086E5CDCF45B2594A28B339E96AF222EFBC5035DCB6DBEBB8DE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs", "0270E426620D8301AA82027E2000EECD3048B3F0E3E004DA8D07FC4729613331")],
            "Construction clones selectors and requires every physical seven-byte record between sorted set starts and the fixed exclusive end, rejecting missing, duplicate or malformed groups. Definitions are indexed independently of exposed group arrays. Frame checks its selected top/optional bottom definitions before publication; DefinitionAddress validates and returns the same physical identity, preserving native cross-group and cross-half arithmetic. The reviewed transfer producer stores only these validated identities for DefinitionAt. This establishes admitted valid-domain resource availability, not arbitrary animation counters, edited invalid selectors, corrupted or incompatible saved pointers, installation, timing or pixels; those invalid inputs still fail loudly. Unselected backing words are deliberately not required to be valid frame records."),
    ];
}
