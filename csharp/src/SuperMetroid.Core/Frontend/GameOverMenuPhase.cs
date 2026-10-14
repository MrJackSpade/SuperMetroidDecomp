namespace SuperMetroid.Core.Frontend;

/// <summary>Native game-over menu indexes; indexes five and six share <see cref="GameOverMenuPhase.FadeOutToContinue"/>, and ordinals are not native menu-index values.</summary>
public enum GameOverMenuPhase
{
    // Ordinals are serialized; ConfigureGraphics is appended rather than placed first.
    /// <summary>Native menu-index-one setup: queues music stop and data load, starts the Baby animation, selects Yes, and begins with zero brightness.</summary>
    Initialize,
    /// <summary>Native menu-index-two music gate: continues Baby animation while waiting for the shared music queue to empty, then queues the game-over track.</summary>
    WaitForInitialMusic,
    /// <summary>Native menu-index-three fade-in: advances the Baby and raises brightness by one per update until full intensity fifteen enables interaction.</summary>
    FadeIn,
    /// <summary>Native menu-index-four interaction: advances the Baby, toggles Yes/No on navigation edges, and accepts the selected answer on an A edge.</summary>
    Main,
    /// <summary>Native menu indexes five/six: fades the accepted Yes answer to black and publishes <see cref="GameOverMenuState.ContinueRequested"/> for the outer map-view transition.</summary>
    FadeOutToContinue,
    /// <summary>Native menu-index-seven path: fades the accepted No answer to black and publishes <see cref="GameOverMenuState.TitleRequested"/> for the outer soft reset.</summary>
    FadeOutToTitle,
    /// <summary>Native menu-index-zero setup: fades out (already black after state $19), cancels library-three sounds, and waits for NMI before configuring the menu graphics.</summary>
    ConfigureGraphics,
}
