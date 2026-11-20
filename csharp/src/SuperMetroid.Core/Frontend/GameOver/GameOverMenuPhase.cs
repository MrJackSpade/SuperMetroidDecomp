namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Game-over menu steps; the continue fade's two halves share <see cref="GameOverMenuPhase.FadeOutToContinue"/>.</summary>
public enum GameOverMenuPhase
{
    /// <summary>Setup: queues music stop and data load, starts the Baby animation, selects Yes, and begins with zero brightness.</summary>
    Initialize,
    /// <summary>Music gate: continues Baby animation while waiting for the shared music queue to empty, then queues the game-over track.</summary>
    WaitForInitialMusic,
    /// <summary>Fade-in: advances the Baby and raises brightness a level per update until full brightness enables interaction.</summary>
    FadeIn,
    /// <summary>Interaction: advances the Baby, toggles Yes/No on navigation edges, and accepts the selected answer on an A edge.</summary>
    Main,
    /// <summary>Fades the accepted Yes answer to black and hands off with <see cref="GameOverMenuOutcome.Continue"/> for the outer map-view transition.</summary>
    FadeOutToContinue,
    /// <summary>Fades the accepted No answer to black and hands off with <see cref="GameOverMenuOutcome.Title"/> for the outer soft reset.</summary>
    FadeOutToTitle,
    /// <summary>The menu's first step: holds the black screen the death black-out leaves, cancels the room's sounds, and waits out one update.</summary>
    BlackHold,
}
