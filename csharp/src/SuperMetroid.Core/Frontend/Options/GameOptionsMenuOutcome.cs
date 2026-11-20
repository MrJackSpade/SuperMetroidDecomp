namespace SuperMetroid.Core.Frontend.Options;

/// <summary>How the game options menu hands off to the frontend (#2562), which it keeps once it reaches it.</summary>
public enum GameOptionsMenuOutcome
{
    /// <summary>The completed start-game fade starts the opening sequence.</summary>
    Intro = 1,
    /// <summary>The completed cancel fade returns to the file-select menu.</summary>
    FileSelect = 2,
}
