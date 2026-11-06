using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud;

/// <summary>A HUD image's installed picture placed where it shows on the HUD (#1466, #1514).</summary>
/// <param name="Picture">The image's picture, under the palette the HUD shows in.</param>
/// <param name="Corner">The screen pixel its upper-left corner sits on, the HUD lying across the top of the screen.</param>
/// <param name="Shading">
/// The shading a frontend screen shows the picture in (#1512), or null for gameplay's HUD, which shows its frame's HUD
/// shading; a backend refuses a frontend HUD picture that names none.
/// </param>
public readonly record struct PlacedHudPicture(SpriteSheetFrame Picture, Position16<ScreenPixels> Corner, PictureShading? Shading = null);
