namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The five bank-$7E cannon-control words, in WRAM order from <c>$7E:8802</c>. Bank $84 writes one when a wall
/// cannon is destroyed; bank $A5 reads it before firing. The port keeps them in <c>DisabledCannonWords</c>.
/// <see cref="DraygonCannonWorkRamAddresses"/> decodes the
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
