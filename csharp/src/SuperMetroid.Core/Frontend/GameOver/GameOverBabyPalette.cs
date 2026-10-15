namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Semantic Baby Metroid palette phases used by the game-over animation.</summary>
public enum GameOverBabyPalette
{
    /// <summary>Neutral colors shared by all idle poses, corresponding to <c>GameOver_BabyMetroid_Palettes_0</c> at $82:BD97.</summary>
    Idle,
    /// <summary>Cry colors paired with the closed pose, corresponding to the sixteen-color block at $82:BDB7.</summary>
    ClosedCry,
    /// <summary>Cry colors paired with the intermediate pose, corresponding to the sixteen-color block at $82:BDD7.</summary>
    MiddleCry,
    /// <summary>Cry colors paired with the open pose, corresponding to the sixteen-color block at $82:BDF7.</summary>
    OpenCry,
}
