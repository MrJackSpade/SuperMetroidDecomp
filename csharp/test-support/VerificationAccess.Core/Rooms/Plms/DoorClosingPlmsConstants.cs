namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values of <see cref="DoorClosingPlms"/> that only verification reads.</summary>
internal static class DoorClosingPlmsConstants
{
    /// <summary>$8F:E68A-$8F:E6A1, the twelve direction-selected fallback headers.</summary>
    public const int HeaderTableAddress = 0x8fe68a;

    /// <summary>Provides the native door-closing PLM header table location for header comparisons.</summary>
    extension(DoorClosingPlms)
    {
        /// <inheritdoc cref="DoorClosingPlmsConstants.HeaderTableAddress"/>
        internal static int HeaderTableAddress => DoorClosingPlmsConstants.HeaderTableAddress;
    }
}
