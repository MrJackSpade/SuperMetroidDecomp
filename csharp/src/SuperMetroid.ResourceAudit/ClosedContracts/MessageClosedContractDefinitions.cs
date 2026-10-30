using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    /// <summary>Source files shared by title, panel, and notice glyph ownership checks.</summary>
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageTitlePresentation.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageTitleDefinitions.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageGlyphs.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageTilemapDefinitions.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageButtonGlyphs.cs"),
        new("csharp/src/SuperMetroid.Core/Hud/GameplayMessageBoxDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/GameplayMessageIds.cs"),
    ];

    /// <summary>Closed title, panel, and notice source sets, each checked only for the message IDs it owns.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Hud.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources),
        new("SuperMetroid.AssetExtraction.Hud.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessagePanelPresentation.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessagePanelDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Hud.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageNoticePresentation.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageNoticeDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Hud/GameplayMessageNotices.cs")]),
    ];
}
