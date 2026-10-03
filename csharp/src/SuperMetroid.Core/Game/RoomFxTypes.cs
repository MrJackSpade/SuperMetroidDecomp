namespace SuperMetroid.Core.Game;

/// <summary>
/// Conversion boundary for the exclusive room-FX dispatcher values proven from the retail
/// bank-$83 table. This is deliberately not a flags helper: one record selects one routine.
/// </summary>
public static class RoomFxTypes
{
    /// <summary>
    /// $83:ABF6/$AC16 select the same water page; $88:DBB6 installs ordinary BG3
    /// water motion for the statue effect. Samus dispatches both through the low
    /// nibble ($90:800A/$8E1C), while statue BG2 motion remains independently owned.
    /// </summary>
    public static bool UsesWater(RoomFxType type) =>
        type is RoomFxType.Water or RoomFxType.TourianEntranceStatue;

    /// <summary>
    /// Converts a cartridge byte into its named dispatcher identity. Null table entries
    /// and values outside the retail even-word table fail at the owning record.
    /// </summary>
    public static RoomFxType FromCartridge(byte value, string sourceContext)
    {
        RoomFxType type = (RoomFxType)value;
        if (!Enum.IsDefined(typeof(RoomFxType), type))
        {
            throw new NotSupportedException(
                $"Room FX type ${value:X2} from {sourceContext} is not translated.");
        }

        return type;
    }
}
