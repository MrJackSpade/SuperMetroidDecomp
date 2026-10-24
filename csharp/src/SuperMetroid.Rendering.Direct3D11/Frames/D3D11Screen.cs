namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The screen the backend composes: the display packet's native size and the bands gameplay divides it into.</summary>
internal static class D3D11Screen
{
    /// <summary>The screen's width in pixels.</summary>
    internal const int Width = 256;
    /// <summary>The screen's height in pixels.</summary>
    internal const int Height = 224;
    /// <summary>The rows gameplay's HUD fills at the top of the screen.</summary>
    internal const int HudHeight = 32;
    /// <summary>
    /// How far below its scroll a scrolled layer's first row is: the screen's first row shows each layer's line one past
    /// its scroll, so a layer's vertical scroll is drawn this many lines further down.
    /// </summary>
    internal const int LayerLineOffset = 1;
}
