namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>CGRAM ranges the game-over screen animates (#630).</summary>
[PpuLayoutBoundary("catalog: game-over palette")]
public static class GameOverPaletteLayout
{
    /// <summary>CGADD $C0-$CF: OBJ palette 4, the game-over Baby Metroid colours.</summary>
    public static readonly PpuLayoutCell Baby = PpuMemoryLayout.Colors(nameof(Baby),
        GameOverRomData.BabyAnimation.PaletteDestinationIndex, GameOverRomData.BabyAnimation.PaletteColorCount, "Bgr555 OBJ palette 4");
}
