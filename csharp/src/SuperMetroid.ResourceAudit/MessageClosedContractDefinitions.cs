namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs", "5EABBA7475F00DDFAB0931844CFF0ABDA0C3BC8F737410D4B51465ED7CD4818E"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs", "175BE431D6D7C958B3A3A5EADBD3B9B407059B31BA5349C3DCB1D5826F306BF7"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs", "D990D99B40107407D384076BCA7EE178073879AA1FD46D8D533360352C11BC99"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageIds.cs", "57B96B36E9562F316B3024A1CBE96663CD2823C89A7BF490FB078083563D71BD"),
    ];

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources,
            "The sole private-constructor loader requires exactly all fifteen named title messages, their validated glyph strings and a full border row. Contains is a membership query; Build accepts only this owned ID set. This does not grant title ownership of panel/notice messages."),
        new("SuperMetroid.Core.Assets.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs", "795D43EFE9820AA175301AAA602057A9A58DFA26B2130A694ED7ACD0805A6C97"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelDefinitions.cs", "3E857C6847EE06012B18138C2AA1074E68E04124D1AFC282930F94E44124D808")],
            "Load requires exactly all seven panel messages, complete four-row templates, validated title glyphs and border cells before private construction. Contains is only a membership query; Build resolves the complete owned panel set, not other message families."),
        new("SuperMetroid.Core.Assets.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs", "903F8CCAAE5D0264CE35DE30DCEBB4C4D0A9BF33B335784BB93D0D14FC185E71"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs", "39B4E4F7AF5C6470DC3C9D4E6A7C5B2A74B8A5E453271631E088D0292493F81C")],
            "Load requires all five notice IDs, complete templates/borders and valid text regions; both save prompts require YES and NO rows. Contains is a membership query. Build covers the owned notice set; ApplySelection covers only the two save prompts, not station-completion notices."),
    ];
}
