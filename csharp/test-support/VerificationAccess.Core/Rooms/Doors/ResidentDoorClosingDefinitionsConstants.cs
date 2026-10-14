namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of <see cref="ResidentDoorClosingDefinitions"/> that only verification reads.</summary>
internal static class ResidentDoorClosingDefinitionsConstants
{
    /// <summary>Number of retail resident door/gate headers with a secondary closing list.</summary>
    public const int Count = 20;

    /// <summary>Provides the number of resident door-closing definitions expected by extraction.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <inheritdoc cref="ResidentDoorClosingDefinitionsConstants.Count"/>
        internal static int Count => ResidentDoorClosingDefinitionsConstants.Count;
    }
}
