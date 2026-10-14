using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable bank-$8B ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels as native sixteen-bit words; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Attributes">OBJ attribute word providing the palette bits passed to the cinematic sprite constructor.</param>
/// <param name="Program">The cinematic sprite program the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    SnesObjAttributeWord Attributes,
    CinematicSpriteProgram Program);
