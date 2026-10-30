namespace SuperMetroid.AssetExtraction.Frontend.GameOver;

/// <summary>Mutually exclusive game-over text elements in the menu's load order.</summary>
public enum GameOverTextElement
{
    /// <summary>GAME OVER, starting at page cell (11,5).</summary>
    Title,
    /// <summary>FIND THE METROID LARVA, starting at page cell (5,14).</summary>
    Objective,
    /// <summary>TRY AGAIN, starting at page cell (10,16).</summary>
    Prompt,
    /// <summary>YES - RETURN TO GAME, starting at page cell (7,19).</summary>
    ReturnToGame,
    /// <summary>NO - GO TO TITLE, starting at page cell (7,23).</summary>
    ReturnToTitle,
}
