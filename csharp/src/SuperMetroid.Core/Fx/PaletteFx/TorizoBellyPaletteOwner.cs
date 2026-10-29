namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Torizo whose belly palette is animated.</summary>
public enum TorizoBellyPaletteOwner
{
    /// <summary>Bomb Torizo belly object $8D:F759, selecting the six-frame $E2E9 program over the <see cref="PaletteFxColours.TorizoBelly"/> until enemy slot zero dies.</summary>
    BombTorizo,
    /// <summary>Golden Torizo belly object $8D:F75D, selecting the six-frame $E331 program over the <see cref="PaletteFxColours.TorizoBelly"/> with its own colors and the same enemy-zero death lifetime.</summary>
    GoldenTorizo,
}
