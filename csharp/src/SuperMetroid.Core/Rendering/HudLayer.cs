using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The HUD, its own display layer (#1466): the backend draws its placed pictures at the HUD's fixed place, the top of the
/// screen, at the layer's place in its scene's order (#1514). Gameplay, the turning-floor rooms and the pause screen draw the HUD
/// through this one layer; no background plane holds it.
/// </summary>
/// <param name="Hud">The HUD's placed pictures.</param>
/// <param name="TransparentColorZero">Whether a picture's colour zero shows what lies behind; gameplay's HUD band draws it opaque.</param>
[IndependentFlag("the layer's colour-zero transparency, an independent setting")]
public sealed record HudLayer(PicturedHud Hud, bool TransparentColorZero) : DisplayLayer
{
    /// <summary>The HUD's placed pictures.</summary>
    public PicturedHud Hud { get; init; } = Ensure.NotNull(Hud);
}
