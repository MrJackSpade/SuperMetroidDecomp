namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Where the installed layout places one item icon (#1908), and whether the template leaves its place blank.</summary>
/// <param name="Anchor">The screen pixel the icon's upper-left corner shows at.</param>
/// <param name="ShowsOverTemplate">
/// Whether the template leaves the icon's place blank: the HUD draws an icon only over a blank place, and nothing but the
/// template ever sits under an icon.
/// </param>
internal readonly partial record struct HudIconPlacement(Position16<ScreenPixels> Anchor, bool ShowsOverTemplate);
