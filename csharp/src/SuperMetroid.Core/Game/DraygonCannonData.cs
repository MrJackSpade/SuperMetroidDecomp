namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's cannon-control words and matching projectile origins. Bank $84 writes these
/// words when a wall cannon is destroyed; bank $A5 reads the same words before firing.
/// </summary>
public static class DraygonCannonData
{
    /// <summary>$A5:87AA HandleFiringWallTurret selects four roles; $A5:87E4..87F3 holds their wall muzzle coordinates.</summary>
    internal static DraygonCannonTarget FiringTarget(DraygonFiringCannon cannon) => cannon switch
    {
        DraygonFiringCannon.LowerLeft => new(DraygonCannonControlWord.LowerLeft, new(52, 303)),
        DraygonFiringCannon.UpperRight => new(DraygonCannonControlWord.UpperRight, new(460, 257)),
        DraygonFiringCannon.LowerRight => new(DraygonCannonControlWord.LowerRight, new(460, 350)),
        DraygonFiringCannon.UnusedBottom => new(DraygonCannonControlWord.UnusedBottom, new(444, 392)),
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonFiringCannon)} {(int)cannon}."),
    };
}

/// <summary>One bank-$A5 cannon-control word paired with its wall projectile origin.</summary>
internal readonly record struct DraygonCannonTarget(
    DraygonCannonControlWord DisabledWord,
    Position16<RoomPixels> Origin);

/// <summary>
/// The five bank-$7E cannon-control words, in WRAM order from <c>$7E:8802</c>. Bank $84 writes one when a wall
/// cannon is destroyed; bank $A5 reads it before firing. <see cref="DraygonCannonWorkRamAddresses"/> decodes the
/// addresses room PLM data names them by.
/// </summary>
public enum DraygonCannonControlWord : ushort
{
    /// <summary>Pre-destroyed upper-left cannon word authored by PLM argument <c>$8802</c>.</summary>
    UpperLeft,
    /// <summary>Lower-left cannon word authored by PLM argument <c>$8804</c>.</summary>
    LowerLeft,
    /// <summary>Upper-right cannon word authored by PLM argument <c>$8806</c>.</summary>
    UpperRight,
    /// <summary>Lower-right cannon word authored by PLM argument <c>$8808</c>.</summary>
    LowerRight,
    /// <summary>Unused bottom cannon word initialized to one by Draygon's body setup.</summary>
    UnusedBottom,
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