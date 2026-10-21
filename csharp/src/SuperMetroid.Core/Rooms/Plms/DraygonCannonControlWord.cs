namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The five bank-$7E cannon-control words, in WRAM order from <c>$7E:8802</c>. Bank $84 writes one when a wall
/// cannon is destroyed; bank $A5 reads it before firing. The port keeps them in <c>DisabledCannonWords</c>. Room
/// PLM data names each by its address; the room import decodes that into the word a cannon placement carries.
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
