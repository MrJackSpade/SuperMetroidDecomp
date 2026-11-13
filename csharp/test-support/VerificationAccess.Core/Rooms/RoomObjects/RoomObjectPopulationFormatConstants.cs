namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The cartridge's room-population record layout, which only verification's importer reads.</summary>
internal static class RoomObjectPopulationFormatConstants
{
    /// <summary>Six bytes per native bank-$8F placement: header, block X/Y, argument.</summary>
    public const int RecordByteCount = 6;

    /// <summary>
    /// The most records the room loader reads from one population, the zero header that ends it included: one more than
    /// <see cref="RoomObjectPopulation.MaximumPlacementCount"/>.
    /// </summary>
    public const int MaximumParserIterations = RoomObjectPopulation.MaximumPlacementCount + 1;
}
