using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed source-owned interface dispatch sets; external injected implementations are not certified.</summary>
internal static class InterfacePresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.IPaletteFxColorSource", "palette-interface-total-membership-query", ["TryReadColor"],
            [Source("IPaletteFxColorSource"),
             Source("RoomPaletteFxPresentation"),
             Source("LoadingPaletteInputView"),
             Source("LoadingPaletteColorDefinitions"),
             Source("HeatPaletteColorDefinitions"),
             new("csharp/src/SuperMetroid.Core/Game/PaletteFxHeatInstructionListDefinitions.cs"),
             Source("HeatPaletteInputView"),
             Source("TitlePalettePresentation")]),
        new("SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation", "cinematic-interface-complete-owned-frame-domains", ["Draw"],
            [Source("IIntroCinematicSpritePresentation"),
             Source("IntroDiscoveryActorSpritePresentation"),
             Source("IntroDiscoveryActorSpriteDefinitions"),
             Source("IntroEggEffectSpritePresentation"),
             Source("IntroEggEffectSpriteDefinitions"),
             Source("IntroRinkaSpritePresentation"),
             Source("IntroRinkaSpriteDefinitions"),
             Source("IntroScientistSpritePresentation"),
             Source("IntroScientistSpriteDefinitions"),
             Source("CeresFlightSpritePresentation"),
             Source("CeresDestructionSpritePresentation"),
             Source("EndingCloudSpritePresentation"),
             Source("EndingCompletionTextSpritePresentation"),
             Source("EndingExplosionSpritePresentation"),
             Source("EndingLogoSpritePresentation"),
             Source("EndingRewardSpritePresentation"),
             Source("IntroCinematicSpriteCompiler")]),
    ];

    private static ReviewedSource Source(string name) => new("csharp/src/SuperMetroid.Core/Assets/" + name + ".cs");

    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Assets.IPaletteFxColorSource" => [typeof(RoomPaletteFxPresentation).FullName!, typeof(TitlePalettePresentation).FullName!],
        "SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation" =>
            [typeof(IntroDiscoveryActorSpritePresentation).FullName!, typeof(IntroEggEffectSpritePresentation).FullName!,
             typeof(IntroRinkaSpritePresentation).FullName!, typeof(IntroScientistSpritePresentation).FullName!,
             typeof(CeresFlightSpritePresentation).FullName!, typeof(CeresDestructionSpritePresentation).FullName!,
             typeof(CeresSceneSpritePresentation).FullName!, typeof(EndingCloudSpritePresentation).FullName!,
             typeof(EndingCompletionTextSpritePresentation).FullName!, typeof(EndingExplosionSpritePresentation).FullName!,
             typeof(EndingLogoSpritePresentation).FullName!, typeof(EndingRewardSpritePresentation).FullName!],
        _ => null,
    };
}
