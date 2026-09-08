namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's cannon-control words and matching projectile origins. Bank $84 writes these
/// words when a wall cannon is destroyed; bank $A5 reads the same words before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>$84:DC67/$DCA7 cannon instructions write a word through PLM_Vars in WRAM bank $7E.</summary>
    public const int ControlWordBank = 0x7e0000;
    /// <summary>Pre-destroyed upper-left cannon word authored by PLM argument <c>$8802</c>.</summary>
    public const ushort UpperLeftDisabledWord = 0x8802;
    /// <summary>Lower-left cannon word authored by PLM argument <c>$8804</c>.</summary>
    public const ushort LowerLeftDisabledWord = 0x8804;
    /// <summary>Upper-right cannon word authored by PLM argument <c>$8806</c>.</summary>
    public const ushort UpperRightDisabledWord = 0x8806;
    /// <summary>Lower-right cannon word authored by PLM argument <c>$8808</c>.</summary>
    public const ushort LowerRightDisabledWord = 0x8808;
    /// <summary>Unused bottom cannon word initialized to one by Draygon's body setup.</summary>
    public const ushort UnusedBottomDisabledWord = 0x880a;

    /// <summary>Selectable firing positions indexed by <c>(random &amp; 3)</c>.</summary>
    internal static readonly DraygonCannonTarget[] FiringTargets =
    [
        new(LowerLeftDisabledWord, 0x0034, 0x012f),
        new(UpperRightDisabledWord, 0x01cc, 0x0101),
        new(LowerRightDisabledWord, 0x01cc, 0x015e),
        new(UnusedBottomDisabledWord, 0x01bc, 0x0188),
    ];

    public static bool IsControlWord(ushort address) => address is
        UpperLeftDisabledWord or LowerLeftDisabledWord or UpperRightDisabledWord or
        LowerRightDisabledWord or UnusedBottomDisabledWord;
}

/// <summary>One bank-$A5 cannon-control word paired with its wall projectile origin.</summary>
internal readonly record struct DraygonCannonTarget(
    ushort DisabledWord,
    ushort X,
    ushort Y);
