namespace SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomCallbackDefinitions"/> members production does not use.</summary>
internal static class RoomCallbackDefinitionsAccess
{
    /// <summary>Converts typed room main and setup callbacks to their native dispatcher pointers.</summary>
    extension(RoomCallbackDefinitions)
    {
        /// <summary>Returns a defined room-main callback's native pointer.</summary>
        internal static ushort PointerOf(RoomMainCallback callback) => Enum.IsDefined(callback)
            ? (ushort)callback
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Returns a defined room-setup callback's native pointer.</summary>
        internal static ushort PointerOf(RoomSetupCallback callback) => Enum.IsDefined(callback)
            ? (ushort)callback
            : throw new ArgumentOutOfRangeException(nameof(callback));
    }
}
