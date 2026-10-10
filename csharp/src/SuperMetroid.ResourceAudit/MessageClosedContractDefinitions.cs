namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs", "E6BF6B8328812976B42527B947FE5B454774B6B67E6806B05B4FF1D605BB9DA8"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs", "1A30B19CFB20D2C97C92B53726C280B4EC4F224D2E4A0012EFBC349776854361"),
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
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs", "48446B5D1E427F244C8A9991BC1A870244CAFDEAB75F6D45C010933CF591CC8B")]),
    ];
}
