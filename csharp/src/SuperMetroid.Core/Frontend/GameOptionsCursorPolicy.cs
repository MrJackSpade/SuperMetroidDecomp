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
    /// <summary>
    /// Selects the cursor's managed page for a game-options phase; phases without a cursor page
    /// use the installed hidden anchor.
    /// </summary>
    /// <param name="phase">Current options phase whose cursor selection is being resolved.</param>
    /// <returns>The primary, controller, or special page, or <see langword="null"/> for the hidden anchor.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase is not one of the supported managed phases.</exception>
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
