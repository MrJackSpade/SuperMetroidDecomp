using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels as sixteen-bit words; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Colouring">The screen colours the actor starts drawing in.</param>
/// <param name="Program">The cinematic sprite program the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    ScreenColourGroup Colouring,
    CinematicSpriteProgram Program)
{
    /// <summary>An actor at signed <paramref name="origin"/>, stored as the wrapped words the actor carries.</summary>
    public static EndingSpriteDefinition At(Offset16<ScreenPixels> origin, ScreenColourGroup colouring, CinematicSpriteProgram program) =>
        new(new(CoordinateAxis.Wrap(origin.X), CoordinateAxis.Wrap(origin.Y)), colouring, program);
}
