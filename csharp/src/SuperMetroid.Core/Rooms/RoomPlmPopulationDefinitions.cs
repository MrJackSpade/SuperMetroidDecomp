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

    /// <summary>Dispatches a compiled room population to its ordered setup operations.</summary>
    /// <param name="pointer">Bank-$8F identity of the retail population list to expand.</param>
    /// <param name="place">Receives each operation's header, room-block coordinates, and setup argument in native order.</param>
    /// <exception cref="ArgumentNullException"><paramref name="place"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">The pointer does not identify a compiled retail population.</exception>
    internal static void Place(ushort pointer, Action<ushort, byte, byte, ushort> place)
    {
        ArgumentNullException.ThrowIfNull(place);
        if (!TryPlace(pointer, place))
            throw new InvalidDataException($"Compiled room PLM populations lack retail source $8F:{pointer:X4}.");
    }
}
