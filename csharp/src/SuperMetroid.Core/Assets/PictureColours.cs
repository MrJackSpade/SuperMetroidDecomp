using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// What an installed sheet image's colours do (#1633): the shades it is pre-coloured in and the colour its scene shows
/// where nothing is drawn. Its own colours are its pixels'.
/// </summary>
public sealed record PictureColours
{

    /// <summary>Each shade the image is pre-coloured in, by the shade that names it.</summary>
    public ImmutableDictionary<PictureShade, PictureRecolouring> Shades { get; init; } = ImmutableDictionary<PictureShade, PictureRecolouring>.Empty;

    /// <summary>
    /// The colour the image's scene shows where nothing is drawn, for a room's block sheet (#1544), as the key the image's
    /// shades recolour it by; null for an image whose scene has no backdrop of its own.
    /// </summary>
    public ColourKey? Backdrop { get; init; }
}
