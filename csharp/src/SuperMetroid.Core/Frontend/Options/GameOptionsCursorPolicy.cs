namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Named options-phase cursor selection.</summary>
/// <remarks>
/// Phases 2/3/B select the primary page, 7 controller and 8 special.
/// Dissolve 5/6, scroll 9/A and start-game fade C select zero, meaning the installed
/// hidden anchor, as does start-game handoff 4 (<see cref="GameOptionsPhase.StartGame"/>).
/// Startup phases 0/1 (<see cref="GameOptionsPhase.FinishFadingOut"/> and
/// <see cref="GameOptionsPhase.LoadingMenu"/>) also select zero. No selection
/// table or generated selection cache is retained. Forged managed phases reject.
/// </remarks>
internal static class GameOptionsCursorPolicy
{
    /// <summary>Selects the primary, controller, or special page for visible phases; transitional phases use the hidden anchor.</summary>
    internal static GameOptionsPage? Select(GameOptionsPhase phase) => phase switch
    {
        GameOptionsPhase.FadeIn or GameOptionsPhase.Main or GameOptionsPhase.FadeOutToFileSelect => GameOptionsPage.Primary,
        GameOptionsPhase.ControllerSettings => GameOptionsPage.Controller,
        GameOptionsPhase.SpecialSettings => GameOptionsPage.Special,
        GameOptionsPhase.DissolveOut or GameOptionsPhase.DissolveIn or
            GameOptionsPhase.ScrollControllerDown or GameOptionsPhase.ScrollControllerUp or
            GameOptionsPhase.FadeOutToIntro or GameOptionsPhase.StartGame or
            GameOptionsPhase.FinishFadingOut or GameOptionsPhase.LoadingMenu => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase)),
    };
}
