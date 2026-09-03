namespace SuperMetroid.Core.Game;

/// <summary>
/// Conversion boundary for the exclusive room-FX dispatcher values proven from the retail
/// bank-$83 table. This is deliberately not a flags helper: one record selects one routine.
/// </summary>
public static class RoomFxTypes
{
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
