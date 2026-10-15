namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Literal indirect targets stored in Puyo's ordinary enemy variable D. Entries zero
/// through six in the ROM hop table select these functions.
/// </summary>
public enum PuyoAirborneFunction : ushort
{
    /// <summary>$A2:9D0B, <c>Function_Puyo_Airborne_Normal_ShortHop</c>: moves the small normal hop and selects the slow grounded animation on landing or terrain collision.</summary>
    NormalShortHop = 0x9d0b,
    /// <summary>$A2:9D2B, <c>Function_Puyo_Airborne_Normal_BigHop</c>: moves the taller normal hop and selects the medium grounded animation on landing or terrain collision.</summary>
    NormalBigHop = 0x9d2b,
    /// <summary>$A2:9D4B, <c>Function_Puyo_Airborne_Normal_LongHop</c>: moves hop-table record two with increased horizontal speed and selects the fast grounded animation on landing or terrain collision.</summary>
    NormalLongHop = 0x9d4b,
    /// <summary>$A2:9D6B, <c>Function_Puyo_Airborne_GiantHop</c>: moves the 128-pixel-height hop; normal landing restores small-hop selection, while wall or ceiling collision retains dropping recovery.</summary>
    GiantHop = 0x9d6b,
    /// <summary>$A2:9D98, <c>Function_Puyo_Airborne_Dropping</c>: falls vertically at the hop record's packed 8.8 speed until terrain contact selects a random small or big post-drop bounce.</summary>
    Dropping = 0x9d98,
    /// <summary>$A2:9DCD, <c>Function_Puyo_Airborne_Dropped</c>: moves either post-drop bounce and selects a giant hop for the next grounded launch when the hopping animation is cleared by terrain contact.</summary>
    Dropped = 0x9dcd,
}
