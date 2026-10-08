namespace SuperMetroid.ResourceAudit;

/// <summary>Closed message-family resource ownership, not a promise that every family owns every message.</summary>
internal static class MessageClosedContractDefinitions
{
    private static readonly ReviewedSource[] SharedGlyphSources =
    [
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs", "5EABBA7475F00DDFAB0931844CFF0ABDA0C3BC8F737410D4B51465ED7CD4818E"),
        new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitleDefinitions.cs", "175BE431D6D7C958B3A3A5EADBD3B9B407059B31BA5349C3DCB1D5826F306BF7"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs", "1BA8298AB9D9F0E9324CD08B936A4939363F77C3CE7F447A6667EE81AA76E84C"),
        new("csharp/src/SuperMetroid.Core/Game/GameplayMessageIds.cs", "400AF5615A00F434B6200EADCA93DF013A3F93B50B740EF48DF4E20603322CD8"),
    ];

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameplayMessageTitlePresentation", "message-titles-v1-complete-owned-id-set",
            ["Contains", "Build"], SharedGlyphSources),
        new("SuperMetroid.Core.Assets.GameplayMessagePanelPresentation", "message-panels-v1-complete-owned-id-set",
            ["Contains", "Build"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs", "795D43EFE9820AA175301AAA602057A9A58DFA26B2130A694ED7ACD0805A6C97"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelDefinitions.cs", "3E857C6847EE06012B18138C2AA1074E68E04124D1AFC282930F94E44124D808")]),
        new("SuperMetroid.Core.Assets.GameplayMessageNoticePresentation", "message-notices-v1-complete-owned-id-set",
            ["Contains", "Build", "ApplySelection"],
            [.. SharedGlyphSources,
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs", "903F8CCAAE5D0264CE35DE30DCEBB4C4D0A9BF33B335784BB93D0D14FC185E71"),
             new("csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs", "EDA3F1979E4A230DBAEAA4B333729C7B3D640C4A007940B8BA4282779C1CE44C")]),
    ];
}
