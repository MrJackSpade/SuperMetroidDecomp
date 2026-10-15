namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values of <see cref="RoomPlmHeaderDefinitions"/> that only verification reads.</summary>
internal static class RoomPlmHeaderDefinitionsConstants
{
    /// <summary>Number of retail PLM headers expected in the extracted header catalog.</summary>
    public const int RetailHeaderCount = 70;

    /// <summary>Provides the retail room PLM header count for imported header completeness checks.</summary>
    extension(RoomPlmHeaderDefinitions)
    {
        /// <inheritdoc cref="RoomPlmHeaderDefinitionsConstants.RetailHeaderCount"/>
        internal static int RetailHeaderCount => RoomPlmHeaderDefinitionsConstants.RetailHeaderCount;
    }
}
