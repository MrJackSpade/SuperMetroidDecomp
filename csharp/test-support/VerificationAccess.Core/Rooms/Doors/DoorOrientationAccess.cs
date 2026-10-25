namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to the cartridge encoding of <see cref="DoorOrientation"/>.</summary>
internal static class DoorOrientationAccess
{
    /// <summary>Encodes a typed door direction and closing flag into the cartridge's orientation byte.</summary>
    extension(DoorOrientation orientation)
    {
        /// <summary>The cartridge byte, as <c>$82:DE2C</c> stores it in <c>DoorDirection</c>.</summary>
        internal byte Encode() => unchecked((byte)((byte)orientation.Closing << 2 | (byte)orientation.Direction));
    }
}
