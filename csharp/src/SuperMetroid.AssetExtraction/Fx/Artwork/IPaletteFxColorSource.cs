
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The import's palette-effect colours, which it pre-colours sheets from (#1512, #1583). The game reads none of them:
/// it shows each palette-effect record as a shade, and timing, destinations and control flow remain its own mechanics.
/// </summary>
public interface IPaletteFxColorSource
{
    /// <summary>Resolves one colour by the record that writes it and its position among the record's colors.</summary>
    bool TryReadColor(PaletteFxColorKey key, out RgbColor color);
}
