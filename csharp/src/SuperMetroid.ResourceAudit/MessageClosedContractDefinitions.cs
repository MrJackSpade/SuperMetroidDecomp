namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs", "C4D5242F643029DF925CD91D38134E906C0D0B902FB628449B652A93D99F998E"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs", "CA230B34E2213C0A98F40F133D506D902BBD79871331F4EA9F7117016A00092D"),
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
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs", "CE3332ECB454642DC02D228F0580D836372FC189E056043551DA753D0DC711B6")]),
    ];
}
