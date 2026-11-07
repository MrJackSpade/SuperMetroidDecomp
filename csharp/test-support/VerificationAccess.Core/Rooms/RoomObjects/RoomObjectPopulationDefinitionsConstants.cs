namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values of <see cref="RoomObjectPopulationDefinitions"/> that only verification reads.</summary>
internal static class RoomObjectPopulationDefinitionsConstants
{
    /// <summary>Number of retail PLM population records expected in the extracted catalog.</summary>
    public const int RetailRecordCount = 941;

    /// <summary>Provides the retail PLM population record count for extraction completeness checks.</summary>
    extension(RoomObjectPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomObjectPopulationDefinitionsConstants.RetailRecordCount"/>
        internal static int RetailRecordCount => RoomObjectPopulationDefinitionsConstants.RetailRecordCount;
    }
}
