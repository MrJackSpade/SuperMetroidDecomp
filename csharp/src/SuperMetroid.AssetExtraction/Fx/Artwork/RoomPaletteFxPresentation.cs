
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The colors of room and cinematic palette animations, import data only the import reads (#1583, #1637): it pre-colours the
/// sheets each record shows from them. Every record colour is kept as extracted. Timing, destinations, instructions, and
/// side effects remain compiled engine mechanics.
/// </summary>
public sealed class RoomPaletteFxPresentation : IPaletteFxColorSource
{
    /// <summary>Every installed colour, by the record colour it is.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> colors;

    /// <summary>Keeps the complete installed colour set.</summary>
    /// <param name="colors">Every record colour of every palette-effect family, as installed.</param>
    internal RoomPaletteFxPresentation(Dictionary<PaletteFxColorKey, RgbColor> colors) => this.colors = colors;

    /// <inheritdoc />
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) => colors.TryGetValue(key, out color);
}
