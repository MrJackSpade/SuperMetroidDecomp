namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values of <see cref="RoomObjectHeaderDefinitions"/> that only verification reads.</summary>
internal static class RoomObjectHeaderDefinitionsConstants
{
    /// <summary>Number of retail PLM headers expected in the extracted header catalog.</summary>
    public const int RetailHeaderCount = 70;

    /// <summary>Provides the retail room PLM header count for imported header completeness checks.</summary>
    extension(RoomObjectHeaderDefinitions)
    {
        /// <inheritdoc cref="RoomObjectHeaderDefinitionsConstants.RetailHeaderCount"/>
        internal static int RetailHeaderCount => RoomObjectHeaderDefinitionsConstants.RetailHeaderCount;
    }
}
