using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels as sixteen-bit words; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Attributes">OBJ attribute word providing the palette bits passed to the cinematic sprite constructor.</param>
/// <param name="Program">The cinematic sprite program the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    SnesObjAttributeWord Attributes,
    CinematicSpriteProgram Program)
{
    /// <summary>An actor at signed origin (<paramref name="x"/>, <paramref name="y"/>), stored as the wrapped words the actor carries.</summary>
    public static EndingSpriteDefinition At(short x, short y, SnesObjAttributeWord attributes, CinematicSpriteProgram program) =>
        new(new(unchecked((ushort)x), unchecked((ushort)y)), attributes, program);
}
