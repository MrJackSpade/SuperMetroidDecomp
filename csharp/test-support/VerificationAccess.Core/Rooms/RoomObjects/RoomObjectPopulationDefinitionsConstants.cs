namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values of <see cref="RoomObjectPopulationDefinitions"/> that only verification reads.</summary>
internal static class RoomObjectPopulationDefinitionsConstants
{
    /// <summary>Number of retail PLM population records the cartridge stores.</summary>
    public const int RetailRecordCount = 941;

    /// <summary>
    /// The retail scroll extensions that continue no trigger or extension (East Pants' stray block), which the catalog
    /// leaves out because they reach nothing (tools/generate-room-object-population-definitions.ps1).
    /// </summary>
    public const int OmittedOrphanScrollExtensionCount = 1;

    /// <summary>Provides the retail PLM population record count for extraction completeness checks.</summary>
    extension(RoomObjectPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomObjectPopulationDefinitionsConstants.RetailRecordCount"/>
        internal static int RetailRecordCount => RoomObjectPopulationDefinitionsConstants.RetailRecordCount;

        /// <inheritdoc cref="RoomObjectPopulationDefinitionsConstants.OmittedOrphanScrollExtensionCount"/>
        internal static int OmittedOrphanScrollExtensionCount => RoomObjectPopulationDefinitionsConstants.OmittedOrphanScrollExtensionCount;
    }
}
