using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels; an offscreen origin left of or above the screen is held as its coordinate's wrap.</param>
/// <param name="Colouring">The colouring the actor's palette-inheriting parts start drawing in (#1512).</param>
/// <param name="Animation">The cinematic sprite program the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    EndingSpriteColouring Colouring,
    CinematicSpriteSegment Animation)
{
    /// <summary>An actor at signed <paramref name="origin"/>, stored as the wrapped coordinates the actor carries.</summary>
    public static EndingSpriteDefinition At(Offset16<ScreenPixels> origin, EndingSpriteColouring colouring, CinematicSpriteSegment segment) =>
        new(new(CoordinateAxis.Wrap(origin.X), CoordinateAxis.Wrap(origin.Y)), colouring, segment);
}
