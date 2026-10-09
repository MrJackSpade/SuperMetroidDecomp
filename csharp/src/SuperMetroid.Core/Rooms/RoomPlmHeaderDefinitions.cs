namespace SuperMetroid.Core.Rooms;

/// <summary>Native setup and first-instruction pointers for one retail room PLM header.</summary>
/// <param name="Header">The bank-$84 PLM header address selected by a room's PLM population.</param>
/// <param name="Setup">The native setup callback pointer associated with the header.</param>
/// <param name="InitialInstruction">The first instruction-list pointer used to initialize this PLM.</param>
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

    /// <summary>Resolves a compiled retail PLM header to its setup and initial-instruction pointers.</summary>
    /// <param name="header">The bank-$84 PLM header address referenced by room data.</param>
    /// <returns>The native callback and instruction-list pointers registered for the header.</returns>
    /// <exception cref="InvalidDataException">The header is not present in the compiled retail dispatch.</exception>
    internal static RoomPlmHeaderDefinition Get(ushort header) =>
        TrySelect(header, out var definition) ? definition : throw new InvalidDataException(
            $"Compiled room PLM headers lack retail header $84:{header:X4}.");
}
