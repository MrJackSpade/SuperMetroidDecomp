namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Bounded transitions between the Baby Metroid palette phases.</summary>
public static class GameOverBabyPalettes
{
    /// <summary>The cry palette paired with one animation pose.</summary>
    public static GameOverBabyPalette CryPalette(GameOverBabyFrame frame) => frame switch
    {
        GameOverBabyFrame.Closed => GameOverBabyPalette.ClosedCry,
        GameOverBabyFrame.Middle => GameOverBabyPalette.MiddleCry,
        GameOverBabyFrame.Open => GameOverBabyPalette.OpenCry,
        _ => throw new InvalidOperationException($"Undefined {nameof(GameOverBabyFrame)} {(int)frame}."),
    };

    /// <summary>The palette whose colors a cry palette brightens: the cry before it, or the idle colors for the first cry.</summary>
    /// <exception cref="InvalidOperationException">The idle palette has no predecessor.</exception>
    public static GameOverBabyPalette PreviousCry(GameOverBabyPalette palette) => palette switch
    {
        GameOverBabyPalette.ClosedCry => GameOverBabyPalette.Idle,
        GameOverBabyPalette.MiddleCry => GameOverBabyPalette.ClosedCry,
        GameOverBabyPalette.OpenCry => GameOverBabyPalette.MiddleCry,
        GameOverBabyPalette.Idle => throw new InvalidOperationException("The idle Baby palette precedes every cry palette."),
        _ => throw new InvalidOperationException($"Undefined {nameof(GameOverBabyPalette)} {(int)palette}."),
    };
}
