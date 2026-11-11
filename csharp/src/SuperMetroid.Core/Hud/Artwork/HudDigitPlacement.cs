namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>One digit of a HUD counter and where it is shown (#1466).</summary>
/// <param name="Numeral">The numeral the digit shows, zero through nine.</param>
/// <param name="Corner">The screen pixel the digit's upper-left corner shows at.</param>
internal readonly record struct HudDigitPlacement(int Numeral, Position16<ScreenPixels> Corner);
