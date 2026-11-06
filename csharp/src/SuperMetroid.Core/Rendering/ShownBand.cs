using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A band of gameplay screen lines that shows its own choice of the gameplay scene's layers (#1514), as Mother Brain's
/// rise shows her body only between the room's upper band and its floor, and hides the actors on the floor's band.
/// </summary>
/// <param name="Lines">The band's lines, at or below the HUD.</param>
/// <param name="Shows">The room's layers and the actors the band shows.</param>
public readonly record struct ShownBand(ScreenLineBand Lines, ScreenLayers Shows);
