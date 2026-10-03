namespace SuperMetroid.Core.Frontend;

/// <summary>Named options-phase selection corresponding to $82:F2ED..F306.</summary>
/// <remarks>
/// Native phases 2/3/B select the primary page, 7 controller and 8 special.
/// Dissolve 5/6, scroll 9/A and start-game fade C select zero, meaning the installed
/// hidden anchor. Native startup phases 0/1 and start-game handoff 4 also select
/// zero; these lifecycle steps have no separate managed menu phase. No pointer
/// array or generated selection cache is retained. Forged managed phases reject.
/// </remarks>
internal static class GameOptionsCursorPolicy
{
    internal static GameOptionsPage? Select(GameOptionsPhase phase) => phase switch
    {
        GameOptionsPhase.FadeIn or GameOptionsPhase.Main or GameOptionsPhase.FadeOutToFileSelect => GameOptionsPage.Primary,
        GameOptionsPhase.ControllerSettings => GameOptionsPage.Controller,
        GameOptionsPhase.SpecialSettings => GameOptionsPage.Special,
        GameOptionsPhase.DissolveOut or GameOptionsPhase.DissolveIn or
            GameOptionsPhase.ScrollControllerDown or GameOptionsPhase.ScrollControllerUp or
            GameOptionsPhase.FadeOutToIntro => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };
}
