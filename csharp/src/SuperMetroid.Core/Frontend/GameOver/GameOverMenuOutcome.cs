namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>How the game-over menu hands off to the outer game-state loop (#2562), which it keeps once it reaches it.</summary>
public enum GameOverMenuOutcome
{
    /// <summary>The menu runs: it has handed off to nothing yet.</summary>
    Choosing,
    /// <summary>The accepted Yes answer has faded fully to black: the loop opens the file-select map view rather than reloading the save inside this scene.</summary>
    Continue,
    /// <summary>Yes was accepted while the save resumes at the Ceres elevator: the loop publishes the Ceres game state and reloads the save without fading.</summary>
    CeresArrival,
    /// <summary>The accepted No answer has faded fully to black: the loop enters its soft-reset path toward the title.</summary>
    Title,
}
