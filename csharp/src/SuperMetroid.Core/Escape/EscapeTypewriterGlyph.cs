using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// One glyph the escape typewriter typed (#1348): its tile, the cell of the overlay its program places it at, and the name
/// of its picture in the room's written picture sheet (#1466), which the owner writes over its background.
/// </summary>
/// <param name="Cell">The overlay cell the glyph is typed over (#1406).</param>
/// <param name="Tile">The glyph's tile.</param>
/// <param name="Picture">The name of the glyph's picture, as <see cref="PictureOf"/> spells it.</param>
public readonly record struct EscapeTypewriterGlyph(int Cell, BackgroundTile Tile, string Picture)
{
    /// <summary>The name of the picture of <paramref name="character"/> as <paramref name="program"/>'s typewriter types it.</summary>
    public static string PictureOf(EscapeTypewriterProgramId program, char character) => $"{program}Typed.{character}";
}
