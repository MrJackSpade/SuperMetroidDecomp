using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid;

/// <summary>
/// One band of Kraid's health colour sequences (#2297): the shade it is installed as and the sixteen-colour group of the
/// sequence that holds its colours.
/// </summary>
/// <param name="Shade">The shade the band's colours are installed as: the hurt flash's or a health band's.</param>
/// <param name="ColourGroup">The sixteen-colour group of the sequence holding the band's colours, from zero.</param>
public sealed record KraidHealthSequenceBand(PictureShade Shade, int ColourGroup)
{
    /// <summary>The sequence's colour the band's first colour is.</summary>
    public int FirstColour => ColourGroup * KraidPalettes.BandColors;
}
