using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Exact room-argument dispatcher table used by PLM header <c>$84:DB44</c>.
/// </summary>
/// <remarks>
/// The room argument is a byte offset into thirteen consecutive words at
/// <c>$84:DB28-$DB40</c>, not an event number. The first nine entries deliberately point to
/// distinct one-byte RTS addresses; the final four select the kill-quota observers that mark
/// events $10 through $13. Keeping both tables lossless prevents an odd or out-of-range room
/// argument from accidentally acquiring a plausible meaning.
/// </remarks>
internal static class MetroidsClearedPlmRomData
{
    private static readonly ushort[] PreInstructionByArgumentWord =
    [
        0xdad5,
        0xdad6,
        0xdad7,
        0xdad8,
        0xdad9,
        0xdada,
        0xdadb,
        0xdadc,
        0xdadd,
        0xdade,
        0xdaee,
        0xdafe,
        0xdb0e,
    ];

    private static readonly EventNumber?[] EventByArgumentWord =
    [
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        EventNumber.FirstMetroidHallCleared,
        EventNumber.FirstMetroidShaftCleared,
        EventNumber.SecondMetroidHallCleared,
        EventNumber.SecondMetroidShaftCleared,
    ];

    /// <summary>Resolves the native pre-instruction pointer selected during PLM setup.</summary>
    public static ushort ResolvePreInstruction(ushort roomArgument)
    {
        int wordIndex = ValidateAndGetWordIndex(roomArgument);
        return PreInstructionByArgumentWord[wordIndex];
    }

    /// <summary>
    /// Resolves the event written after the enemy death quota is met, or null for one of the
    /// cartridge's nine intentional no-op argument entries.
    /// </summary>
    public static EventNumber? ResolveEvent(ushort roomArgument)
    {
        int wordIndex = ValidateAndGetWordIndex(roomArgument);
        return EventByArgumentWord[wordIndex];
    }

    private static int ValidateAndGetWordIndex(ushort roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument / 2 >= PreInstructionByArgumentWord.Length)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM room argument ${roomArgument:X4} is not an even " +
                "offset into cartridge table $84:DB28-$DB40.");
        }

        return roomArgument / 2;
    }
}
