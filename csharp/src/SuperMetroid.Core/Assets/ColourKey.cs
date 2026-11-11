namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour a picture's pixels hold, with the recolour region they lie in (#1633): what a shade names when it shows those
/// pixels in another colour. A picture whose pixels of one colour all recolour alike has one region, zero; a picture whose
/// pixels of one colour recolour differently under some shade carries a recolour-region map, and each group of its pixels
/// that recolours alike is its own region of that colour. A region is told apart only by the shades that name it: its
/// number orders the groups of one colour and stands for nothing else.
/// </summary>
/// <param name="Colour">The colour the pixels show unshaded.</param>
/// <param name="Region">The recolour region the pixels lie in among the picture's pixels of <paramref name="Colour"/>.</param>
public readonly record struct ColourKey(RgbColor Colour, int Region)
{
}
