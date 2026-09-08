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
    /// <summary>Enemy-header boss identities rejected by the five equality checks in $91:D143.</summary>
    public static ReadOnlySpan<ushort> ExcludedBossIds => [3, 6, 7, 8, 10];
    /// <summary>$91:D2BC installs RGB5(3,3,3) as CGRAM entry zero after setup.</summary>
    public const ushort ActiveBackdrop = 0x0C63;
    /// <summary>$91:D223 installs $91:D27F before setup call one; its first execution is call two, leaving next-stage counter three.</summary>
    public const byte FirstBlendedSetupStage = 3;
    /// <summary>WRAM $0074, the red COLDATA register mirror preserved by excluded-room X-ray.</summary>
    public const int FixedRedMirror = 0x74;
    /// <summary>WRAM $0075, the green COLDATA register mirror.</summary>
    public const int FixedGreenMirror = 0x75;
    /// <summary>WRAM $0076, the blue COLDATA register mirror.</summary>
    public const int FixedBlueMirror = 0x76;

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
        if (roomPointer is ExcludedRoomA66A or ExcludedRoomWithHiddenBg2 || ExcludedBossIds.Contains(bossId))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
