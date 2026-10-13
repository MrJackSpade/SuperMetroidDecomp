using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed source-owned interface dispatch sets; external injected implementations are not certified.</summary>
internal static class InterfacePresentationContractDefinitions
{
    /// <summary>Reviewed palette-source and cinematic-sprite interface dispatches with their source-owned implementations and audit files.</summary>
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

    /// <summary>Locates a reviewed production asset source file by its catalog type name.</summary>
    private static ReviewedSource Source(string name) => new("csharp/src/SuperMetroid.Core/Assets/" + name + ".cs");

    /// <summary>Lists the source-owned implementations for audited palette or cinematic dispatch, returning null for contracts outside this closed set.</summary>
    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Assets.IPaletteFxColorSource" => [(typeof(RoomPaletteFxPresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(RoomPaletteFxPresentation).FullName.")), (typeof(TitlePalettePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(TitlePalettePresentation).FullName."))],
        "SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation" =>
            [(typeof(IntroDiscoveryActorSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroDiscoveryActorSpritePresentation).FullName.")), (typeof(IntroEggEffectSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroEggEffectSpritePresentation).FullName.")),
             (typeof(IntroRinkaSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroRinkaSpritePresentation).FullName.")), (typeof(IntroScientistSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroScientistSpritePresentation).FullName.")),
             (typeof(CeresFlightSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresFlightSpritePresentation).FullName.")), (typeof(CeresDestructionSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresDestructionSpritePresentation).FullName.")),
             (typeof(CeresSceneSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresSceneSpritePresentation).FullName.")), (typeof(EndingCloudSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingCloudSpritePresentation).FullName.")),
             (typeof(EndingCompletionTextSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingCompletionTextSpritePresentation).FullName.")), (typeof(EndingExplosionSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingExplosionSpritePresentation).FullName.")),
             (typeof(EndingLogoSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingLogoSpritePresentation).FullName.")), (typeof(EndingRewardSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingRewardSpritePresentation).FullName."))],
        _ => null,
    };
}
