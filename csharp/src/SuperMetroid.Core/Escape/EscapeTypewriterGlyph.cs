using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// One glyph the escape typewriter typed (#1348): the cell of the layer its program places it at, and the name of its
/// picture in the room's written picture sheet (#1466), which the owner writes over its layer.
/// </summary>
/// <param name="Cell">The cell of the layer the glyph is typed over (#1406, #1514).</param>
/// <param name="Picture">The name of the glyph's picture, as <see cref="PictureOf"/> spells it.</param>
public readonly record struct EscapeTypewriterGlyph(Position16<TilemapCells> Cell, string Picture)
{
    /// <summary>The name of the picture of <paramref name="character"/> as <paramref name="program"/>'s typewriter types it.</summary>
    public static string PictureOf(EscapeTypewriterProgramId program, char character) => $"{program}Typed.{character}";

    /// <summary>Whether typing <paramref name="character"/> writes a glyph; a space only moves to the next cell.</summary>
    public static bool Types(char character) => character != ' ';
}
