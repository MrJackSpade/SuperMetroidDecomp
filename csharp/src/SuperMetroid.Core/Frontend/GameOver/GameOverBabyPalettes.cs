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
        _ => throw new InvalidOperationException($"Undefined {nameof(GameOverBabyFrame)} {frame}."),
    };
}
