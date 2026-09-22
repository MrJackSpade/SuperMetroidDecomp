namespace SuperMetroid.Core.Assets;

/// <summary>
/// Host-selected presentation colors consumed by the compiled bank-$8D palette-FX
/// interpreter. Implementations expose colors only; timing, destinations, and control
/// flow remain engine-owned mechanics.
/// </summary>
public interface IPaletteFxColorSource
{
    /// <summary>Resolves one BGR555 color by its native bank-local presentation address.</summary>
    bool TryReadColor(ushort pointer, out ushort color);
}
