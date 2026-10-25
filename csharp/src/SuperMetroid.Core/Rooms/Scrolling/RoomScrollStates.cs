namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The single conversion between <see cref="RoomScrollState"/> and the byte a room-scroll grid cell stores.
/// A cell is conventionally red, blue, or green; the values are not combinable flags.
/// </summary>
/// <remarks>
/// Explicit room tables are copied as a fixed 50-byte native overread. Some retail tables
/// are shorter than the logical room dimensions and intentionally expose bytes belonging
/// to the following bank-$8F definition. Camera code distinguishes zero, one, and nonzero;
/// it does not validate every copied byte as one of these named values.
/// </remarks>
[SnesEncodingOwner]
public static class RoomScrollStates
{
    /// <summary>The byte a scroll-grid cell stores for <paramref name="state"/>.</summary>
    public static byte ToStorageByte(this RoomScrollState state) => (byte)state;

    /// <summary>
    /// The state a stored scroll-grid byte holds. The conversion is lossless: a byte that names no state, such as the
    /// adjacent byte the camera's bottom-row read observes, travels through unchanged so callers can still tell it apart.
    /// </summary>
    public static RoomScrollState FromStorageByte(byte value) => (RoomScrollState)value;

    /// <summary>Whether <paramref name="value"/> is one of the three authored scroll states.</summary>
    public static bool IsAuthoredByte(byte value) => Enum.IsDefined(FromStorageByte(value));
    /// <summary>
    /// Converts a cartridge-authored byte into its named state and reports the owning data
    /// source when an untranslated value enters a logical room cell.
    /// </summary>
    public static RoomScrollState FromAuthoredByte(byte value, string sourceContext)
    {
        RoomScrollState state = FromStorageByte(value);
        if (!Enum.IsDefined(state))
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
