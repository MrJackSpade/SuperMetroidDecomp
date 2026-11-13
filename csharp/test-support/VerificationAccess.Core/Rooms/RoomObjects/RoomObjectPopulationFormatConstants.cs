namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The cartridge's room-population record layout, which only verification's importer reads.</summary>
internal static class RoomObjectPopulationFormatConstants
{
    /// <summary>Six bytes per native bank-$8F placement: header, block X/Y, argument.</summary>
    public const int RecordByteCount = 6;

    /// <summary>The bytes one bank holds, through which a population's records run.</summary>
    private const int BankByteCount = 65536;

    /// <summary>
    /// The most records the importer reads from one population, the zero header that ends it included: every record the
    /// population's bank holds, so a population with no terminator is rejected rather than read round its bank again
    /// (#1627; the game sets no count of its own on a population).
    /// </summary>
    public const int MaximumParserIterations = BankByteCount / RecordByteCount + 1;
}
