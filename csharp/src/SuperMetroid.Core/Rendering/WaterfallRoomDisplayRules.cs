using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Colosseum/Halfie Climb layer blending, native $88:80CA configuration $16.</summary>
public static class WaterfallRoomDisplayRules
{
    /// <summary>$88:80CA TM=$11: foreground and objects compete on the main screen.</summary>
    public const SnesMainScreenLayers MainScreen = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;
    /// <summary>$88:80CA TS=$06: waterfall BG2 and liquid BG3 compete on the subscreen.</summary>
    public const SnesMainScreenLayers Subscreen = SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Bg3;
    /// <summary>$88:80CA CGADSUB=$B1: subtract from BG1, eligible OBJ and backdrop without halving.</summary>
    public const SnesColorMathControl ColorMath = SnesColorMathControl.Bg1 | SnesColorMathControl.Obj |
        SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract;
}
