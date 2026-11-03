namespace SuperMetroid.Core.Assets;

/// <summary>A flash (#1512): every ink the draw shows shows one colour, as a hit or freeze flash shows.</summary>
/// <param name="Colour">The colour every ink shows.</param>
public sealed record ColourFlash(RgbColor Colour) : ColourEffect
{
    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown) => Colour;
}
