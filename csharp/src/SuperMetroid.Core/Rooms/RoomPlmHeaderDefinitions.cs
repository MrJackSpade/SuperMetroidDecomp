namespace SuperMetroid.Core.Rooms;

/// <summary>Native setup and first-instruction pointers for one retail room PLM header.</summary>
public readonly record struct RoomPlmHeaderDefinition(
    ushort Header, ushort Setup, ushort InitialInstruction);

/// <summary>
/// Named header-to-setup/list dispatch for the seventy distinct headers referenced by
/// retail room populations. Each header denotes a native bank-$84 actor definition,
/// not a numerical sample. Cases independently preserve both original little-endian
/// pointer fields; every other ushort is rejected. Enumeration retains ascending
/// header order without a stored record array. Setup/execution stay with their owners.
/// </summary>
internal static partial class RoomPlmHeaderDefinitions
{
    internal const int RetailHeaderCount = 70;

    internal static IEnumerable<RoomPlmHeaderDefinition> All => Enumerate();

    internal static RoomPlmHeaderDefinition Get(ushort header) =>
        TrySelect(header, out var definition) ? definition : throw new InvalidDataException(
            $"Compiled room PLM headers lack retail header $84:{header:X4}.");
}
