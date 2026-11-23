using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Fireflea's darkness (#1514): its shade darkens the background and the backdrop where each is frontmost, everywhere on
/// the screen; the foreground and Samus keep their colours.
/// </summary>
/// <param name="Shade">The shade the darkness takes from the background and the backdrop.</param>
public sealed partial record FirefleaDarknessBlend(RgbColor Shade) : GameplayBlend;
