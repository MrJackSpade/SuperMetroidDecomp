namespace SuperMetroid.Core.Rendering;

/// <summary>The screen a frame shows: its size and the band gameplay's HUD fills (#1409).</summary>
public static class DisplayScreen
{
    /// <summary>The screen's width in pixels.</summary>
    public const int Width = 256;
    /// <summary>The screen's height in lines.</summary>
    public const int Height = 224;
    /// <summary>The lines the HUD fills at the top of the screen during gameplay.</summary>
    public const int HudHeight = 32;
}
