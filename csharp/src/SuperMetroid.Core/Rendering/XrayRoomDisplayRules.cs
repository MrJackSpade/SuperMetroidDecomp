using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Rendering;

/// <summary>Room and enemy definition values used by CanXrayShowBlocks ($91:D143).</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>True only for the excluded room whose preserved display also drops BG2 ($88:81A4).</summary>
    public static bool RemovesBg2(RoomHeaderDefinition room) => room == RoomHeaderDefinitions.GlassTunnel;
    /// <summary>$91:D2BC installs RGB5(3,3,3) as CGRAM entry zero after setup.</summary>
    public static Bgr555 ActiveBackdrop => new(3, 3, 3);

    /// <summary>CGADSUB assignments from $88:817B, $88:81A4 and $88:81DB, preserving the room's subtraction bit.</summary>
    public static SnesColorMathControl ColorMath(XrayRoomBlendMode mode, bool subtract)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        var sources = SnesColorMathControl.Bg1 | SnesColorMathControl.Backdrop;
        if (mode is XrayRoomBlendMode.RevealBlocks or XrayRoomBlendMode.Fireflea)
            sources |= SnesColorMathControl.Bg2 | SnesColorMathControl.Obj;
        if (mode == XrayRoomBlendMode.Fireflea) return sources | SnesColorMathControl.Subtract;
        return sources | SnesColorMathControl.Half | (subtract ? SnesColorMathControl.Subtract : 0);
    }

    /// <summary>Preserves the native Fireflea precedence over the ordinary room/boss exclusions.</summary>
    public static XrayRoomBlendMode Select(RoomHeaderDefinition room, RoomFxType fx, int bossId)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (!XrayBlockRevealRules.RevealsBlocks(room, bossId))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
