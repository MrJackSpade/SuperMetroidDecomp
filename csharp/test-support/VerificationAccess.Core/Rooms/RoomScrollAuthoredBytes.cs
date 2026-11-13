namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// Verification's reading of a scroll state the cartridge authored as a byte (#1546). The game reads typed scroll writes;
/// only checks and fixture importers decode the authored bytes.
/// </summary>
internal static class RoomScrollAuthoredBytes
{
    /// <summary>Whether <paramref name="value"/> is one of the three authored scroll states.</summary>
    internal static bool IsAuthored(byte value) => value is 0 or 1 or 2;

    /// <summary>The named state an authored byte holds, failing with <paramref name="sourceContext"/> for an untranslated one.</summary>
    /// <exception cref="NotSupportedException">The byte is not an authored scroll state.</exception>
    internal static RoomScrollState FromAuthored(byte value, string sourceContext) => IsAuthored(value)
        ? RoomScrollStates.FromStorageByte(value)
        : throw new NotSupportedException($"Room scroll state ${value:X2} from {sourceContext} is not translated.");
}
