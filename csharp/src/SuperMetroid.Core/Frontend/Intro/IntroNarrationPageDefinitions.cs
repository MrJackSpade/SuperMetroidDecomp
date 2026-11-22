using SuperMetroid.Core.Frontend.Intro.Artwork;
using C = SuperMetroid.Core.Frontend.Intro.IntroNarrationPageClosing;
using I = SuperMetroid.Core.Frontend.Intro.IntroIllustration;
using O = SuperMetroid.Core.Frontend.Intro.IntroNarrationPageOpening;
using P = SuperMetroid.Core.Frontend.Intro.Artwork.IntroNarrationPageId;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's narration pages (#2489): how each starts, what it does once typed and the illustration it turns to. Each
/// illustration leads into the next page, whose crossfade brings the narration back.
/// </summary>
internal static partial class IntroNarrationPageDefinitions
{
    /// <summary>The page the narration opens with, once the first card has faded out.</summary>
    internal const IntroNarrationPageId FirstPage = P.ZebesBattle;

    /// <summary>The last page, which follows the page before it without an illustration and ends the intro.</summary>
    internal const IntroNarrationPageId FinalPage = P.CeresUnderAttack;

    /// <summary>Every narration page's definition.</summary>
    internal static IReadOnlyDictionary<IntroNarrationPageId, IntroNarrationPageDefinition> Pages { get; } =
        new Dictionary<IntroNarrationPageId, IntroNarrationPageDefinition>
        {
            [P.ZebesBattle] = new(O.KeepsSetupCaret, C.AwaitsInput, I.MotherBrainFlashback),
            [P.HatchedLarva] = new(O.RestoresCaret, C.AwaitsSecondSubtitle, I.BabyDiscovery),
            [P.DeliveryToCeres] = new(O.RestoresCaret, C.AwaitsSecondSubtitle, I.BabyDelivery),
            [P.ScientistsFindings] = new(O.RestoresCaret, C.AwaitsSecondSubtitle, I.BabyExamination),
            [P.DistressSignal] = new(O.RestoresCaret, C.AwaitsSecondSubtitle, null),
            [P.CeresUnderAttack] = new(O.ReplacesEarlierPages, C.EndsIntro, null),
        };

    /// <summary>
    /// The page each illustration leads into: the illustration's last actor asks for it, and its crossfade brings the
    /// narration back.
    /// </summary>
    internal static IReadOnlyDictionary<IntroIllustration, IntroNarrationPageId> PageAfter { get; } =
        new Dictionary<IntroIllustration, IntroNarrationPageId>
        {
            [I.MotherBrainFlashback] = P.HatchedLarva,
            [I.BabyDiscovery] = P.DeliveryToCeres,
            [I.BabyDelivery] = P.ScientistsFindings,
            [I.BabyExamination] = P.DistressSignal,
        };
}
