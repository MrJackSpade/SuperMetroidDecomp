namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Dispatches each of the 284 retail room-state population identities to its
/// ordered setup operations. Named PLM headers select behavior; block coordinates
/// and room arguments supply that operation's parameters. The bank-$8F six-byte
/// records and zero terminators are replaced by calls and return, preserving all
/// 941 operations in their native order so immediate deletion and slot reuse agree.
/// Unknown identities are rejected, including addresses within a known population.
/// </summary>
internal static partial class RoomPlmPopulationDefinitions
{

    /// <summary>Emits the complete compiled PLM population for a bank-$8F room pointer and rejects unknown populations.</summary>
    /// <param name="pointer">The room's native PLM population pointer.</param>
    /// <param name="place">Receives each PLM header, tile coordinates, and room argument in native order.</param>
    internal static void Place(ushort pointer, Action<PlmHeaderId, byte, byte, ushort> place)
    {
        Ensure.NotNull(place);
        if (!TryPlace(pointer, place))
            throw new InvalidDataException($"Compiled room PLM populations lack retail source $8F:{pointer:X4}.");
    }
}
