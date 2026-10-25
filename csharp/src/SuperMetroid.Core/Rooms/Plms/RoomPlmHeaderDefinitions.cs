namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The seventy distinct PLMs retail room populations place; every other PLM is rejected. Setup and execution stay with
/// the PLM families.
/// </summary>
internal static partial class RoomPlmHeaderDefinitions
{

    /// <summary>Selects a compiled retail header or rejects a bank-$84 identity absent from room populations.</summary>
    internal static RoomPlmHeaderDefinition Get(PlmHeaderId header) =>
        TrySelect(header, out var definition) ? definition : throw new InvalidDataException(
            $"Compiled room PLM headers lack retail header {header}.");
}
