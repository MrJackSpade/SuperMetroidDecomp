using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Exact room-argument dispatcher table used by PLM header <c>$84:DB44</c>.
/// </summary>
/// <remarks>
/// The room argument is a byte offset into thirteen consecutive words at
/// <c>$84:DB28-$DB40</c>, not an event number. The first nine entries deliberately point to
/// distinct one-byte RTS addresses; the final four select the kill-quota observers that mark
/// events $10 through $13. Calculated native identities and strict bounds prevent an odd or out-of-range room
/// argument from accidentally acquiring a plausible meaning.
/// </remarks>
internal static class MetroidsClearedPlmRomData
{
    /// <summary>
    /// <c>$84:DB42 InstList_PLM_SetsMetroidsClearedStatesWhenRequired</c> is
    /// the complete one-word resident instruction program. Its setup-selected
    /// pre-instruction, not this list, observes the enemy quota.
    /// </summary>
    internal static bool TryReadInstructionWord(ushort address, out ushort value)
    {
        if (address == RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired)
        {
            value = RoomPlmInstructionCodes.Sleep;
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>$84:DAD5: the first of nine consecutive one-byte no-op RTS handlers.</summary>
    private const ushort FirstNoOp = 0xdad5;
    /// <summary>$84:DADE: first of four sixteen-byte quota observers, for event $10.</summary>
    private const ushort FirstQuotaObserver = 0xdade;
    /// <summary>Resolves the native pre-instruction pointer selected during PLM setup.</summary>
    public static ushort ResolvePreInstruction(ushort roomArgument)
    {
        int wordIndex = ValidateAndGetWordIndex(roomArgument);
        return (ushort)(wordIndex < 9 ? FirstNoOp + wordIndex : FirstQuotaObserver + (wordIndex - 9) * 16);
    }

    /// <summary>
    /// Resolves the event written after the enemy death quota is met, or null for one of the
    /// cartridge's nine intentional no-op argument entries.
    /// </summary>
    public static EventNumber? ResolveEvent(ushort roomArgument)
    {
        int wordIndex = ValidateAndGetWordIndex(roomArgument);
        return wordIndex < 9 ? null : (EventNumber)((int)EventNumber.FirstMetroidHallCleared + wordIndex - 9);
    }

    /// <summary>Validates a PLM byte offset and converts it to its index in the thirteen-word table.</summary>
    /// <param name="roomArgument">Even byte offset selecting a word in <c>$84:DB28-$DB40</c>.</param>
    /// <returns>The zero-based index of the selected table word.</returns>
    /// <exception cref="InvalidDataException">The offset is odd or lies beyond the table.</exception>
    private static int ValidateAndGetWordIndex(ushort roomArgument)
    {
        if ((roomArgument & 1) != 0 || roomArgument / 2 >= 13)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM room argument ${roomArgument:X4} is not an even " +
                "offset into cartridge table $84:DB28-$DB40.");
        }

        return roomArgument / 2;
    }
}
