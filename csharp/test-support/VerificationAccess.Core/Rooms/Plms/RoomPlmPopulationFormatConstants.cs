namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values and helpers of <see cref="RoomPlmPopulationFormat"/> that only verification reads.</summary>
internal static class RoomPlmPopulationFormatConstants
{
    /// <summary>Six bytes per native bank-$8F placement: header, block X/Y, argument.</summary>
    public const int RecordByteCount = 6;
}
