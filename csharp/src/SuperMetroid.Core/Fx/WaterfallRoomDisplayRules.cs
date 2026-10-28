using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>Colosseum/Halfie Climb layer blending, native $88:80CA configuration $16.</summary>
public static class WaterfallRoomDisplayRules
{
    /// <summary>The foreground and the sprites are the layers the screen shows directly.</summary>
    public const ScreenLayers MainScreen = ScreenLayers.Foreground | ScreenLayers.Sprites;
    /// <summary>The waterfall and the liquid are subtracted from the foreground, the blending sprites and the backdrop, unhalved.</summary>
    public const ColorBlend Blend = ColorBlend.Bg1 | ColorBlend.Sprites |
        ColorBlend.Backdrop | ColorBlend.Subtract;
}
