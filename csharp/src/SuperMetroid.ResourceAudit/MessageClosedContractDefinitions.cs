namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    /// <summary>Stores shared glyph sources.</summary>
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageIds.cs"),
    ];

    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources),
        new("SuperMetroid.Core.Assets.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs")]),
    ];
}
