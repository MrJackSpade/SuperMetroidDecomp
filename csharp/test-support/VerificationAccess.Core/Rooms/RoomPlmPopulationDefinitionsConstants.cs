using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomPlmPopulationDefinitions"/> that only verification reads.</summary>
internal static class RoomPlmPopulationDefinitionsConstants
{
    /// <summary>Number of retail PLM population records expected in the extracted catalog.</summary>
    public const int RetailRecordCount = 941;

    /// <summary>Provides the retail PLM population record count for extraction completeness checks.</summary>
    extension(RoomPlmPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomPlmPopulationDefinitionsConstants.RetailRecordCount"/>
        internal static int RetailRecordCount => RoomPlmPopulationDefinitionsConstants.RetailRecordCount;
    }
}
