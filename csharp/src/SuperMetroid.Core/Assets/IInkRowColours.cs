namespace SuperMetroid.Core.Assets;

/// <summary>An owner of some of the colours its sprites' inks show (#1405).</summary>
public interface IInkRowColours
{
    /// <summary>
    /// Lays the owner's colours for ink row <paramref name="inkRow"/> over <paramref name="colours"/>, which start as the
    /// image's own; the owner leaves alone the colours it does not hold.
    /// </summary>
    void Overlay(int inkRow, Span<RgbColor> colours);
}
