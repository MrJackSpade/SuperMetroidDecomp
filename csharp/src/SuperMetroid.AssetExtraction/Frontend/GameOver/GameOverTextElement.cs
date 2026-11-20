namespace SuperMetroid.AssetExtraction.Frontend.GameOver;

/// <summary>Mutually exclusive game-over text elements in the menu's load order.</summary>
public enum GameOverTextElement
{
    /// <summary>GAME OVER, the page's title; <see cref="GameOverTexts"/> holds where it starts.</summary>
    Title,
    /// <summary>FIND THE METROID LARVA, the objective; <see cref="GameOverTexts"/> holds where it starts.</summary>
    Objective,
    /// <summary>TRY AGAIN, the prompt; <see cref="GameOverTexts"/> holds where it starts.</summary>
    Prompt,
    /// <summary>YES - RETURN TO GAME, the first choice; <see cref="GameOverTexts"/> holds where it starts.</summary>
    ReturnToGame,
    /// <summary>NO - GO TO TITLE, the second choice; <see cref="GameOverTexts"/> holds where it starts.</summary>
    ReturnToTitle,
}
