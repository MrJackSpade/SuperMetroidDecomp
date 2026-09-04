namespace SuperMetroid.Core.Game;

/// <summary>
/// Named values written deliberately into the room-scroll grid by cartridge code.
/// A cell is conventionally red, blue, or green; the values are not combinable flags.
/// </summary>
/// <remarks>
/// Explicit room tables are copied as a fixed 50-byte native overread. Some retail tables
/// are shorter than the logical room dimensions and intentionally expose bytes belonging
/// to the following bank-$8F definition. Camera code distinguishes zero, one, and nonzero;
/// it does not validate every copied byte as one of these named values.
/// </remarks>
public static class RoomScrollStates
{
    /// <summary>
    /// Converts a cartridge-authored byte into its named state and reports the owning data
    /// source when an untranslated value enters a logical room cell.
    /// </summary>
    public static RoomScrollState FromCartridge(byte value, string sourceContext)
    {
        RoomScrollState state = (RoomScrollState)value;
        if (!Enum.IsDefined(typeof(RoomScrollState), state))
        {
            throw new NotSupportedException(
                $"Room scroll state ${value:X2} from {sourceContext} is not translated.");
        }

        return state;
    }

    /// <summary>Rejects undefined values passed through a typed host-side API.</summary>
    public static void Validate(RoomScrollState state, string parameterName)
    {
        if (!Enum.IsDefined(typeof(RoomScrollState), state))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                state,
                "Known room scroll states are red boundary, blue, and green.");
        }
    }
}
