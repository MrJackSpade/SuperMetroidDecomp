namespace SuperMetroid.Core.Game;

/// <summary>
/// Validation boundary for the three exclusive byte values stored in the room-scroll grid.
/// A cell is red, blue, or green; the values are not combinable flags.
/// </summary>
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
