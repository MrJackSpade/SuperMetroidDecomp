namespace SuperMetroid.Core.Game;

/// <summary>
/// Conversion boundary for the exclusive room-FX dispatcher values proven from the retail
/// bank-$83 table. This is deliberately not a flags helper: one record selects one routine.
/// </summary>
public static class RoomFxTypes
{
    /// <summary>
    /// Converts a cartridge byte into its named dispatcher identity. Unnamed no-op entries
    /// and values outside the retail even-word table fail at the owning record.
    /// </summary>
    /// <remarks>Named semantic cases replace enum metadata lookup. Native $83:AC18..AC44
    /// supplies these dispatcher slots; $0E..$1E share the RTL at $88:B278 and remain
    /// rejected by the existing port boundary. Zero is the supported named no-effect
    /// identity, despite sharing that RTL. Odd bytes and bytes above $2C are rejected.
    /// Checked against supported NTSC J/U v1.0 and pinned bank_83.asm for #1165.</remarks>
    public static RoomFxType FromCartridge(byte value, string sourceContext)
    {
        RoomFxType type = (RoomFxType)value;
        if (type is not (RoomFxType.None or RoomFxType.Lava or RoomFxType.Acid or
            RoomFxType.Water or RoomFxType.Spores or RoomFxType.Rain or RoomFxType.Fog or
            RoomFxType.ScrollingSky or RoomFxType.UnusedScrollingSky or RoomFxType.Fireflea or
            RoomFxType.TourianEntranceStatue or RoomFxType.CeresRidley or
            RoomFxType.CeresElevator or RoomFxType.CeresHaze))
        {
            throw new NotSupportedException(
                $"Room FX type ${value:X2} from {sourceContext} is not translated.");
        }

        return type;
    }
}
