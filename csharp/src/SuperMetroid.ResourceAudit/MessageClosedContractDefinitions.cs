namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs", "BB4EBD6707C9F4D2F4FB3BAE490E8E812507CB4F128BE340F10AE53C4A4E016A"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs", "1588AA333E2557791EFECFFDFD2B4783B3BFE9E407BD36DBF004FCD7260F69F4"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs", "34111FB80D5A53785C6CA5F323802F469B15FD7D5BB5BE3DB10EBD67BACCB3BC"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageIds.cs", "76347AB16D00426201E22A6C8B2918D272F5407B559A06D6702CDBFE6A2E0895"),
    ];

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources),
        new("SuperMetroid.Core.Assets.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs", "14F92D2FB8A7E1AB33B244AE35F11302D2C329563E650D1C04D2BF2BC82A4A69"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelDefinitions.cs", "B52D1BB339F5A1D533E09CA4E435B5312FBE916B107F884D782E8EAFA9DC8267")]),
        new("SuperMetroid.Core.Assets.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs", "9A8E0A349EC9EA87322034B1E34D6CB2ACB2C5130B5B16A805918C98B5D3A8E3"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs", "1CC0612B378B670E693FEB10CBB259CA0125FF963C733A0C2536AEEBEB363286")]),
    ];
}
