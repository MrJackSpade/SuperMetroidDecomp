namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values and helpers of <see cref="RoomObjectPopulationFormat"/> that only verification reads.</summary>
internal static class RoomObjectPopulationFormatConstants
{
    /// <summary>Six bytes per native bank-$8F placement: header, block X/Y, argument.</summary>
    public const int RecordByteCount = 6;
}
