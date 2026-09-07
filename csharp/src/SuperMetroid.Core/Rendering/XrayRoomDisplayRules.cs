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

    /// <summary>Preserves the native Fireflea precedence over the ordinary room/boss exclusions.</summary>
    public static XrayRoomBlendMode Select(ushort roomPointer, RoomFxType fx, ushort bossId)
    {
        if (fx == RoomFxType.Fireflea) return XrayRoomBlendMode.Fireflea;
        if (roomPointer is ExcludedRoomA66A or ExcludedRoomWithHiddenBg2 || ExcludedBossIds.Contains(bossId))
            return XrayRoomBlendMode.PreserveBackgrounds;
        return XrayRoomBlendMode.RevealBlocks;
    }
}
