using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// An installed picture's own colours (#1544): the list its pixels name their colours from, which holds exactly the
/// colours its pixels show, in the shades it is pre-coloured in (#1512), and, for a room tileset's picture, the colour its
/// scene shows where nothing is drawn.
/// </summary>
/// <param name="Standard">The colours the picture was coloured with, each named by its place in the list.</param>
public sealed record PictureColours(ImmutableArray<RgbColor> Standard)
{
    /// <summary>The shades the import pre-coloured the picture in besides its standard colours, by name (#1512).</summary>
    public ImmutableDictionary<PictureShade, ShadeInks> Shades { get; init; } = ImmutableDictionary<PictureShade, ShadeInks>.Empty;

    /// <summary>
    /// The place in the list of the colour the picture's scene shows where nothing is drawn: a room tileset's backdrop,
    /// which the room's colour effects change as they change its pictures' colours. Null for a picture whose scene takes
    /// its backdrop from elsewhere.
    /// </summary>
    public int? Backdrop { get; init; }
}
