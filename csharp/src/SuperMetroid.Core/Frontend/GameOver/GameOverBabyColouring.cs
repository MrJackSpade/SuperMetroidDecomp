namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The colours the game-over Baby Metroid shows: its idle colours, or a cry's colours for the pose it cries in.</summary>
public enum GameOverBabyColouring
{
    /// <summary>The colours of every idle pose.</summary>
    Idle,
    /// <summary>The cry colours of the closed pose.</summary>
    ClosedCry,
    /// <summary>The cry colours of the middle pose.</summary>
    MiddleCry,
    /// <summary>The cry colours of the open pose.</summary>
    OpenCry,
}
