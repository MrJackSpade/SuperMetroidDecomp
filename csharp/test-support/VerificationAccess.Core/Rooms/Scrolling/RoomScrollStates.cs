namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// Verification's conversion between <see cref="RoomScrollState"/> and the byte the original game stores for a scroll
/// cell (#1551). The game reads typed cells; only checks and fixture importers read native scroll bytes, and they compare
/// through this codec.
/// </summary>
/// <remarks>
/// The original tells a zero byte (red), a one (blue) and every other value apart, so a byte past the three authored
/// states, such as one a short explicit table reaches in the data stored after it, reads as green.
/// </remarks>
[EncodingOwner("room scroll grid byte")]
internal static class RoomScrollStates
{
    /// <summary>The byte the original stores for <paramref name="state"/>.</summary>
    internal static byte ToStorageByte(this RoomScrollState state) => state switch
    {
        RoomScrollState.RedBoundary => 0,
        RoomScrollState.Blue => 1,
        RoomScrollState.Green => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Not a room scroll state."),
    };

    /// <summary>The state the camera reads a stored scroll byte as.</summary>
    internal static RoomScrollState FromStorageByte(int value) => value switch
    {
        0 => RoomScrollState.RedBoundary,
        1 => RoomScrollState.Blue,
        _ => RoomScrollState.Green,
    };

    /// <summary>The states the camera reads a run of stored scroll bytes as, in order.</summary>
    internal static RoomScrollState[] FromStorageBytes(ReadOnlySpan<byte> bytes)
    {
        var states = new RoomScrollState[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
            states[index] = FromStorageByte(bytes[index]);
        return states;
    }
}
