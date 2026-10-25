namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to the cartridge encoding of <see cref="DoorOrientation"/>.</summary>
internal static class DoorOrientationAccess
{
    /// <summary>The cartridge's travel direction in bits 0-1 of each orientation byte.</summary>
    private static readonly Dictionary<DoorDirection, int> DirectionBits = new()
    {
        [DoorDirection.Right] = 0,
        [DoorDirection.Left] = 1,
        [DoorDirection.Down] = 2,
        [DoorDirection.Up] = 3,
    };

    /// <summary>The cartridge's <c>Door_Closing_PLMs</c> row group in bits 2-3 of each orientation byte.</summary>
    private static readonly Dictionary<DoorClosingBehavior, int> ClosingRows = new()
    {
        [DoorClosingBehavior.None] = 0,
        [DoorClosingBehavior.BlueDoorCloses] = 1,
        [DoorClosingBehavior.EscapeGateCloses] = 2,
    };

    /// <summary>Encodes a typed door direction and closing behavior into the cartridge's orientation byte.</summary>
    extension(DoorOrientation orientation)
    {
        /// <summary>The cartridge byte, as <c>$82:DE2C</c> stores it in <c>DoorDirection</c>.</summary>
        internal byte Encode() => checked((byte)(ClosingRows[orientation.Closing] << 2 | DirectionBits[orientation.Direction]));
    }
}
