namespace SuperMetroid.Core.Fx;

/// <summary>Colosseum/Halfie Climb layer blending, native $88:80CA configuration $16.</summary>
public static class WaterfallRoomDisplayRules
{
    /// <summary>The foreground and the sprites are the layers the screen shows directly.</summary>
    public const ScreenLayers MainScreen = ScreenLayers.Bg1 | ScreenLayers.Sprites;
    /// <summary>The waterfall (layer 2) and the liquid (layer 3) are the layers the blend subtracts.</summary>
    public const ScreenLayers Subscreen = ScreenLayers.Bg2 | ScreenLayers.Bg3;
    /// <summary>$88:80CA CGADSUB=$B1: subtract from BG1, eligible OBJ and backdrop without halving.</summary>
    public const SnesColorMathControl ColorMath = SnesColorMathControl.Bg1 | SnesColorMathControl.Obj |
        SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract;
}
