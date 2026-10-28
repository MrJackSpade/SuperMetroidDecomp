namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts the frontmost sprite pixel, or adds the sprites onto the screen using saturated five-bit colour arithmetic.</summary>
public sealed record SpritesLayer(bool AddToScreen = false, ScreenTintLayer? FixedColor = null) : DisplayLayer;
