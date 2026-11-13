using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// One glyph the escape typewriter typed (#1348): the pixel of the layer its program places it at, and the name of its
/// picture in the room's written picture sheet (#1466), which the owner writes over its layer.
/// </summary>
/// <param name="Corner">The pixel of the layer, from its top-left, the glyph's top-left corner shows at (#1406, #1514, #1601).</param>
/// <param name="Picture">The name of the glyph's picture, as <see cref="PictureOf"/> spells it.</param>
public readonly record struct EscapeTypewriterGlyph(Position16<PlanePixels> Corner, string Picture)
{
    /// <summary>The name of the picture of <paramref name="character"/> as <paramref name="program"/>'s typewriter types it.</summary>
    public static string PictureOf(EscapeTypewriterMessageId program, char character) => $"{program}Typed.{character}";

    /// <summary>Whether typing <paramref name="character"/> writes a glyph; a space only moves the typing position right.</summary>
    public static bool Types(char character) => character != ' ';
}
