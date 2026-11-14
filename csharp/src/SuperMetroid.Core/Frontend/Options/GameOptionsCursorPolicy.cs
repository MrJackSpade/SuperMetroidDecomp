namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Named options-phase cursor selection.</summary>
/// <remarks>
/// The fade-in, main and fade-out-to-file-select phases show the primary page's cursor, and the controller and special
/// settings phases their own. The dissolves, the controller page's scrolls, the start-game fade and handoff and the
/// startup phases show no cursor (#1775). Undefined phases reject.
/// </remarks>
internal static class GameOptionsCursorPolicy
{
    /// <summary>The page whose cursor a phase shows, or none during a transition.</summary>
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
