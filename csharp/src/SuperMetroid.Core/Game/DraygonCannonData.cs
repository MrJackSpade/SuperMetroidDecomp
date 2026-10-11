namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's cannon-control words and matching projectile origins. Bank $84 writes these
/// words when a wall cannon is destroyed; bank $A5 reads the same words before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>$84:DC67/$DCA7 cannon instructions write a word through PLM_Vars in WRAM bank $7E.</summary>
    public const int ControlWordBank = 0x7e0000;

    /// <summary>$A5:87AA HandleFiringWallTurret selects four roles; $A5:87E4..87F3 holds their wall muzzle coordinates.</summary>
    internal static DraygonCannonTarget FiringTarget(DraygonFiringCannon cannon) => cannon switch
    {
        DraygonFiringCannon.LowerLeft => new(DraygonCannonControlWord.LowerLeft, 52, 303),
        DraygonFiringCannon.UpperRight => new(DraygonCannonControlWord.UpperRight, 460, 257),
        DraygonFiringCannon.LowerRight => new(DraygonCannonControlWord.LowerRight, 460, 350),
        DraygonFiringCannon.UnusedBottom => new(DraygonCannonControlWord.UnusedBottom, 444, 392),
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonFiringCannon)} {(int)cannon}."),
    };
    /// <summary>Checks whether a bank-$7E word pointer names one of the five modeled cannon-disable words, without reading its current value.</summary>
    /// <param name="address">Bank-relative WRAM byte address, not a cannon selector or a full 24-bit bus address.</param>
    /// <returns><see langword="true"/> for $8802, $8804, $8806, $8808, or $880A, including the pre-disabled upper-left and unused bottom roles; <see langword="false"/> otherwise.</returns>
    public static bool IsControlWord(ushort address) => Enum.IsDefined((DraygonCannonControlWord)address);
}

/// <summary>One bank-$A5 cannon-control word paired with its wall projectile origin.</summary>
internal readonly record struct DraygonCannonTarget(
    DraygonCannonControlWord DisabledWord,
    ushort X,
    ushort Y);

/// <summary>
/// The five bank-$7E cannon-control words, valued by their WRAM address. Bank $84 writes one
/// when a wall cannon is destroyed; bank $A5 reads it before firing.
/// </summary>
public enum DraygonCannonControlWord : ushort
{
    /// <summary>Pre-destroyed upper-left cannon word authored by PLM argument <c>$8802</c>.</summary>
    UpperLeft = 0x8802,
    /// <summary>Lower-left cannon word authored by PLM argument <c>$8804</c>.</summary>
    LowerLeft = 0x8804,
    /// <summary>Upper-right cannon word authored by PLM argument <c>$8806</c>.</summary>
    UpperRight = 0x8806,
    /// <summary>Lower-right cannon word authored by PLM argument <c>$8808</c>.</summary>
    LowerRight = 0x8808,
    /// <summary>Unused bottom cannon word initialized to one by Draygon's body setup.</summary>
    UnusedBottom = 0x880a,
}

/// <summary>Four mutually exclusive cannon roles selected by HandleFiringWallTurret's low random bits.</summary>
internal enum DraygonFiringCannon : byte
{
    /// <summary>$A5:87E4 HandleFiringWallTurret origin, controlled by $7E:8804.</summary>
    LowerLeft,
    /// <summary>$A5:87E8 HandleFiringWallTurret origin, controlled by $7E:8806.</summary>
    UpperRight,
    /// <summary>$A5:87EC HandleFiringWallTurret origin, controlled by $7E:8808.</summary>
    LowerRight,
    /// <summary>$A5:87F0 HandleFiringWallTurret origin, controlled by $7E:880A.</summary>
    UnusedBottom,
}