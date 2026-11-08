using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The HUD, its own display layer (#1466): the backend draws its placed pictures at the HUD's fixed place, the top of the
/// screen, at the layer's place in its scene's order (#1514). Gameplay, the turning-floor rooms and the pause screen draw the HUD
/// through this one layer; no background plane holds it.
/// </summary>
/// <param name="Hud">The HUD's placed pictures.</param>
/// <param name="ClearPixelsShowBehind">
/// Whether a picture's clear pixels show what lies behind (#1544); gameplay's HUD band draws them opaque, each in the colour
/// its picture gives it.
/// </param>
[IndependentFlag("whether the layer's clear pixels show what lies behind, an independent setting")]
public sealed record HudLayer(PicturedHud Hud, bool ClearPixelsShowBehind) : DisplayLayer
{
    /// <summary>The HUD's placed pictures.</summary>
    public PicturedHud Hud { get; init; } = Ensure.NotNull(Hud);
}
