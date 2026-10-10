using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Mutually exclusive X-ray blending routines selected by $91:D27F and bank $88.</summary>
public enum XrayRoomBlendMode
{
    /// <summary>$88:817B substitutes the hidden-block BG2 map inside the beam.</summary>
    RevealBlocks,
    /// <summary>$88:81A4 preserves room backgrounds and does not substitute hidden blocks.</summary>
    PreserveBackgrounds,
    /// <summary>$88:81DB preserves the Fireflea darkness subtraction outside the beam.</summary>
    Fireflea,
}

/// <summary>Room and enemy definition values used by CanXrayShowBlocks ($91:D143).</summary>
public static class XrayRoomDisplayRules
{
    /// <summary>$8F:A66A, one of the two explicitly excluded room headers in $91:D143.</summary>
    public const ushort ExcludedRoomA66A = 0xA66A;
    /// <summary>$8F:CEFB, excluded from revelation; $88:81A4 additionally removes BG2 from TM.</summary>
    public const ushort ExcludedRoomWithHiddenBg2 = 0xCEFB;
    /// <summary>
    /// Native CheckIfXrayShouldShowAnyBlocks ($91:D158-D172): five equality
    /// branches on the active enemy-header boss identity preserve the room background.
    /// These are control-flow cases, not an indexed sequence or numerical progression.
    /// </summary>
    private static bool PreservesBossBackground(ushort bossId) => bossId is 3 or 6 or 7 or 8 or 10;
    /// <summary>$91:D2BC installs RGB5(3,3,3) as CGRAM entry zero after setup.</summary>
    public static Bgr555 ActiveBackdrop => Bgr555.FromWord(0x0c63);

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
    public static XrayRoomBlendMode Select(ushort roomPointer, RoomFxType fx, ushort bossId)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (roomPointer is ExcludedRoomA66A or ExcludedRoomWithHiddenBg2 || PreservesBossBackground(bossId))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
