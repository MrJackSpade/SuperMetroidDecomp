namespace SuperMetroid.Core.Frontend;

/// <summary>Named options-phase selection corresponding to $82:F2ED..F306.</summary>
/// <remarks>
/// Native phases 2/3/B select the primary page, 7 controller and 8 special.
/// Dissolve 5/6, scroll 9/A and start-game fade C select zero, meaning the installed
/// hidden anchor, as does start-game handoff 4 (<see cref="GameOptionsPhase.StartGame"/>).
/// Native startup phases 0/1 also select zero and have no separate managed phase. No pointer
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
            GameOptionsPhase.FadeOutToIntro or GameOptionsPhase.StartGame => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };
}
