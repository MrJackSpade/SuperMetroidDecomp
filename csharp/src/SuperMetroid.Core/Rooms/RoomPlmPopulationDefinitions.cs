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

    internal static void Place(ushort pointer, Action<ushort, byte, byte, ushort> place)
    {
        ArgumentNullException.ThrowIfNull(place);
        if (!TryPlace(pointer, place))
            throw new InvalidDataException($"Compiled room PLM populations lack retail source $8F:{pointer:X4}.");
    }
}
