namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values of <see cref="DoorClosingPlmRomData"/> that only verification reads.</summary>
internal static class DoorClosingPlmRomDataConstants
{
    /// <summary>$8F:E68A-$8F:E6A1, the twelve direction-selected fallback headers.</summary>
    public const int HeaderTableAddress = 0x8fe68a;

    /// <summary>Provides the native door-closing PLM header table location for header comparisons.</summary>
    extension(DoorClosingPlmRomData)
    {
        /// <inheritdoc cref="DoorClosingPlmRomDataConstants.HeaderTableAddress"/>
        internal static int HeaderTableAddress => DoorClosingPlmRomDataConstants.HeaderTableAddress;
    }
}
