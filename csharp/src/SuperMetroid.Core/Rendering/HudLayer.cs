using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The HUD, its own display layer (#1466): the backend draws its placed pictures at the HUD's fixed place, the top of the
/// screen, at the layer's place in its scene's order (#1514). Gameplay, the turning-floor rooms and the pause screen draw the HUD
/// through this one layer; no background plane holds it.
/// </summary>
/// <param name="Hud">The HUD's placed pictures.</param>
/// <param name="DrawsOpaque">
/// Whether the layer is drawn opaque (#1544, #1633): its pictures' clear pixels show the colour each keeps under its alpha
/// rather than what lies behind. Gameplay's HUD band is drawn opaque; the pause screen's HUD is see-through.
/// </param>
[IndependentFlag("whether the layer is drawn opaque, an independent setting")]
public sealed record HudLayer(PicturedHud Hud, bool DrawsOpaque) : DisplayLayer
{
    /// <summary>The HUD's placed pictures.</summary>
    public PicturedHud Hud { get; init; } = Ensure.NotNull(Hud);
}
