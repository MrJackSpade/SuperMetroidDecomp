namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named header-to-setup/list dispatch for the seventy distinct headers referenced by
/// retail room populations. Each header denotes a native bank-$84 actor definition,
/// not a numerical sample. Cases independently preserve both original little-endian
/// pointer fields; every other header is rejected. Enumeration retains ascending
/// header order without a stored record array. Setup/execution stay with their owners.
/// </summary>
internal static partial class RoomPlmHeaderDefinitions
{

    /// <summary>Selects a compiled retail header or rejects a bank-$84 identity absent from room populations.</summary>
    internal static RoomPlmHeaderDefinition Get(PlmHeaderId header) =>
        TrySelect(header, out var definition) ? definition : throw new InvalidDataException(
            $"Compiled room PLM headers lack retail header $84:{(ushort)header:X4}.");
}
