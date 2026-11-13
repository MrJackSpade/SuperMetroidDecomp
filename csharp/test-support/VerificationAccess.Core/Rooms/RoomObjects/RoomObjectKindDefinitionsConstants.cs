namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values of <see cref="RoomObjectKindDefinitions"/> that only verification reads.</summary>
internal static class RoomObjectKindDefinitionsConstants
{
    /// <summary>Number of room-object kinds retail room populations place.</summary>
    public const int PlacedKindCount = 70;

    /// <summary>Provides the placed room-object kind count for imported completeness checks.</summary>
    extension(RoomObjectKindDefinitions)
    {
        /// <inheritdoc cref="RoomObjectKindDefinitionsConstants.PlacedKindCount"/>
        internal static int PlacedKindCount => RoomObjectKindDefinitionsConstants.PlacedKindCount;
    }
}
