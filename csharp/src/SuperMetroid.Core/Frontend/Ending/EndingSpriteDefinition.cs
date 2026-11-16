using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable ending-sprite constructor data.</summary>
/// <param name="Position">Origin in signed screen pixels; an offscreen origin left of or above the screen is negative.</param>
/// <param name="Colouring">The colouring the actor's parts that take a colouring start drawing in (#1512).</param>
/// <param name="Animation">The cinematic sprite animation the actor runs.</param>
internal readonly record struct EndingSpriteDefinition(
    Offset16<ScreenPixels> Position,
    EndingSpriteColouring Colouring,
    EndingSceneAnimation Animation);
