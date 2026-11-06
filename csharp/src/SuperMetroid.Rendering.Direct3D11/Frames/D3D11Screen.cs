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
}
