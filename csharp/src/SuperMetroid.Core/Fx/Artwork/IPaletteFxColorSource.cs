using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Host-selected presentation colors consumed by the compiled bank-$8D palette-FX
/// interpreter. Implementations expose colors only; timing, destinations, and control
/// flow remain engine-owned mechanics.
/// </summary>
public interface IPaletteFxColorSource
{
    /// <summary>Resolves one BGR555 color by the record that writes it and its position among the record's colors.</summary>
    bool TryReadColor(PaletteFxColorKey key, out Bgr555 color);
}
