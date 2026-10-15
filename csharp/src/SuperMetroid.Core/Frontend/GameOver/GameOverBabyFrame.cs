namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Semantic Baby Metroid frames used by the game-over animation.</summary>
public enum GameOverBabyFrame
{
    /// <summary>Closed pose in the triangular animation wave; native menu-spritemap ID $65 resolves to $82:CFF6.</summary>
    Closed,
    /// <summary>Intermediate pose used on both sides of the animation wave; native menu-spritemap ID $66 resolves to $82:CFFD.</summary>
    Middle,
    /// <summary>Open pose at the animation wave's peak; native menu-spritemap ID $67 resolves to $82:D004.</summary>
    Open,
}
