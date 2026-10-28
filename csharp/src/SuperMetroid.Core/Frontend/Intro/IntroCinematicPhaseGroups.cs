using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The stretches of the intro the cinematic tests (#1452), named instead of ordering the phase identities.</summary>
internal static class IntroCinematicPhaseGroups
{
    /// <summary>The narration pages and their flashbacks, from page one's music wait through the intro fade out.</summary>
    internal static readonly FrozenSet<IntroCinematicPhase> NarrationPages = new[]
    {
        IntroCinematicPhase.WaitForPageOneMusicQueue, IntroCinematicPhase.FadeInPageOne, IntroCinematicPhase.PageOneText,
        IntroCinematicPhase.PageOneAwaitingInput, IntroCinematicPhase.MotherBrainCrossfade, IntroCinematicPhase.MotherBrainFlashback,
        IntroCinematicPhase.PageTwoCrossfade, IntroCinematicPhase.PageTwoText, IntroCinematicPhase.PageTwoAwaitingInput,
        IntroCinematicPhase.BabyDiscoveryCrossfade, IntroCinematicPhase.BabyDiscovery, IntroCinematicPhase.PageThreeCrossfade,
        IntroCinematicPhase.PageThreeText, IntroCinematicPhase.PageThreeAwaitingInput,
        IntroCinematicPhase.BabyMetroidDeliveryCrossfade, IntroCinematicPhase.BabyMetroidDelivery,
        IntroCinematicPhase.PageFourCrossfade, IntroCinematicPhase.PageFourText, IntroCinematicPhase.PageFourAwaitingInput,
        IntroCinematicPhase.BabyMetroidExaminationCrossfade, IntroCinematicPhase.BabyMetroidExamination,
        IntroCinematicPhase.PageFiveCrossfade, IntroCinematicPhase.PageFiveText, IntroCinematicPhase.PageFiveAwaitingInput,
        IntroCinematicPhase.PageSixText, IntroCinematicPhase.IntroFadeOut,
    }.ToFrozenSet();
}
