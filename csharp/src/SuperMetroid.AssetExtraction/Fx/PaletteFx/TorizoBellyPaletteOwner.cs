using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive Torizo whose belly palette is animated.</summary>
public enum TorizoBellyPaletteOwner
{
    /// <summary>The Bomb Torizo belly object, selecting the six-frame program over the <see cref="ColourEffectColours.TorizoBelly"/> until enemy slot zero dies.</summary>
    BombTorizo,
    /// <summary>The Golden Torizo belly object, selecting the six-frame program over the <see cref="ColourEffectColours.TorizoBelly"/> with its own colors and the same enemy-zero death lifetime.</summary>
    GoldenTorizo,
}
