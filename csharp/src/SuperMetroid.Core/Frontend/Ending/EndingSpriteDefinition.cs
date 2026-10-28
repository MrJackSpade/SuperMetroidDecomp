using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels as sixteen-bit words; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Palette">The sprite palette row the actor starts with.</param>
/// <param name="Program">The cinematic sprite program the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    int Palette,
    CinematicSpriteProgram Program)
{
    /// <summary>An actor at signed <paramref name="origin"/>, stored as the wrapped words the actor carries.</summary>
    public static EndingSpriteDefinition At(Offset16<ScreenPixels> origin, int palette, CinematicSpriteProgram program) =>
        new(new(unchecked((ushort)origin.X), unchecked((ushort)origin.Y)), palette, program);
}
