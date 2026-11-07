namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values of <see cref="DoorClosingObjects"/> that only verification reads.</summary>
internal static class DoorClosingObjectsConstants
{
    /// <summary>$8F:E68A-$8F:E6A1, the twelve direction-selected fallback headers.</summary>
    public const int HeaderTableAddress = 0x8fe68a;

    /// <summary>Provides the native door-closing PLM header table location for header comparisons.</summary>
    extension(DoorClosingObjects)
    {
        /// <inheritdoc cref="DoorClosingObjectsConstants.HeaderTableAddress"/>
        internal static int HeaderTableAddress => DoorClosingObjectsConstants.HeaderTableAddress;
    }
}
