namespace SuperMetroid.Core.Hud;

/// <summary>A HUD image placed on the HUD (#1466).</summary>
/// <param name="Image">The image.</param>
/// <param name="Corner">
/// The screen pixel its upper-left corner shows at: the static top row across the HUD's top, the values drawn below it.
/// </param>
public readonly record struct PlacedHudImage(HudImage Image, Position16<ScreenPixels> Corner);
