namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Where a HUD ammo counter is drawn and how many digits it shows.</summary>
/// <param name="Anchor">The screen pixel the counter's first digit's upper-left corner shows at.</param>
/// <param name="Digits">The digits.</param>
internal readonly record struct HudAmmoField(Position16<ScreenPixels> Anchor, int Digits);
