namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Game-over menu steps; the continue fade's two halves share <see cref="GameOverMenuPhase.FadeOutToContinue"/>.</summary>
public enum GameOverMenuPhase
{
    /// <summary>Setup: queues music stop and data load, starts the Baby animation, selects Yes, and begins with zero brightness.</summary>
    Initialize,
    /// <summary>Music gate: continues Baby animation while waiting for the shared music queue to empty, then queues the game-over track.</summary>
    WaitForInitialMusic,
    /// <summary>Fade-in: advances the Baby and raises brightness by one per update until full intensity fifteen enables interaction.</summary>
    FadeIn,
    /// <summary>Interaction: advances the Baby, toggles Yes/No on navigation edges, and accepts the selected answer on an A edge.</summary>
    Main,
    /// <summary>Fades the accepted Yes answer to black and publishes <see cref="GameOverMenuState.ContinueRequested"/> for the outer map-view transition.</summary>
    FadeOutToContinue,
    /// <summary>Fades the accepted No answer to black and publishes <see cref="GameOverMenuState.TitleRequested"/> for the outer soft reset.</summary>
    FadeOutToTitle,
    /// <summary>The menu's first step: fades out (already black after the death black-out), cancels room-library sounds, and waits out one update before configuring the menu graphics.</summary>
    ConfigureGraphics,
}
