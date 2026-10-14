namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    /// <summary>Source files shared by title, panel, and notice glyph ownership checks.</summary>
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessageTitlePresentation.cs"),
        new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessageTitleDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageIds.cs"),
    ];

    /// <summary>Closed title, panel, and notice source sets, each checked only for the message IDs it owns.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Hud.Artwork.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources),
        new("SuperMetroid.Core.Hud.Artwork.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessagePanelPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessagePanelDefinitions.cs")]),
        new("SuperMetroid.Core.Hud.Artwork.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessageNoticePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/GameplayMessageNoticeDefinitions.cs")]),
    ];
}
