namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Named options-phase cursor selection.</summary>
/// <remarks>
/// The fade-in, main and fade-out-to-file-select phases show the primary page's cursor, and the controller and special
/// settings phases their own. The dissolves, the controller page's scrolls, the start-game fade and handoff and the
/// opening black hold show no cursor (#1775). Undefined phases reject.
/// </remarks>
internal static class GameOptionsCursorPolicy
{
    /// <summary>The select of each phase.</summary>
    private static IdentityTable<GameOptionsPhase, GameOptionsPage?> SelectByPhase => field ??= new(new Dictionary<GameOptionsPhase, GameOptionsPage?>
    {
        [GameOptionsPhase.FadeIn] = GameOptionsPage.Primary,
        [GameOptionsPhase.Main] = GameOptionsPage.Primary,
        [GameOptionsPhase.FadeOutToFileSelect] = GameOptionsPage.Primary,
        [GameOptionsPhase.ControllerSettings] = GameOptionsPage.Controller,
        [GameOptionsPhase.SpecialSettings] = GameOptionsPage.Special,
        [GameOptionsPhase.DissolveOut] = null,
        [GameOptionsPhase.DissolveIn] = null,
        [GameOptionsPhase.ScrollControllerDown] = null,
        [GameOptionsPhase.ScrollControllerUp] = null,
        [GameOptionsPhase.FadeOutToIntro] = null,
        [GameOptionsPhase.StartGame] = null,
        [GameOptionsPhase.BlackHold] = null,
    });

    /// <summary>The page whose cursor a phase shows, or none during a transition.</summary>
    internal static GameOptionsPage? Select(GameOptionsPhase phase) => SelectByPhase[phase];
}
