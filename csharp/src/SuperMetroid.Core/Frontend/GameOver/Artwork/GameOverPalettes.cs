namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>Typed view of the game-over palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: game-over palettes")]
public readonly struct GameOverPalettes(IPaletteColors cgram)
{
    /// <summary>The Baby Metroid colours.</summary>
    public CgramPalette Baby => new(cgram, GameOverPaletteLayout.Baby);
}
