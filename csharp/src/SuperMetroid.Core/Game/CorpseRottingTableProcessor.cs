using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact shared row scheduler used by <c>ProcessCorpseRotting</c> at
/// <c>$A9:DB12-$DBDF</c>.
/// </summary>
/// <remarks>
/// Enemy-specific code still owns the pixel-row copy/move operation and the completion
/// hook. This type owns only the native four-byte <c>(signed Y, delay)</c> table and its
/// ordering rules, which are identical for Mother Brain and every dead-monster family.
/// Keeping those responsibilities separate prevents an enemy port from subtly changing
/// the shared delay, final-row, or signed-$FFFF completion semantics.
/// </remarks>
public static class CorpseRottingTableProcessor
{
    /// <summary>Byte stride of one table entry containing a signed Y offset and a delay word.</summary>
    private const int EntryByteCount = 4;

    /// <summary>Builds the descending-Y, ascending-delay table from <c>$A9:DC40</c>.</summary>
    public static void Initialize(
        ISnesAddressSpace bus,
        int tableAddress,
        ushort entryCount)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (entryCount == 0)
            throw new ArgumentOutOfRangeException(nameof(entryCount));

        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            int entryAddress = checked(tableAddress + entryIndex * EntryByteCount);
            WriteWord(bus, entryAddress, unchecked((ushort)(entryCount - 1 - entryIndex)));
            WriteWord(bus, entryAddress + 2, unchecked((ushort)(entryIndex * 2)));
        }
    }

    /// <summary>Runs one complete call of the shared cartridge table processor.</summary>
    /// <param name="bus">Address space used to write updated row offsets, delay words, and finished-entry markers.</param>
    /// <param name="memory">Mutable WRAM backing the same table, used to read its current row and delay words.</param>
    /// <param name="tableAddress">Full WRAM byte address of the first four-byte signed-Y/delay entry.</param>
    /// <param name="entryCount">Nonzero number of entries processed in ascending index order.</param>
    /// <param name="yLimit">Exclusive pixel-row limit for the two-pixel advance; also the native entry-index threshold that returns final completion.</param>
    /// <param name="lateMoveEntryIndex">First entry index that uses the destructive move callback during the last four delay states instead of copying its pixel row.</param>
    /// <param name="copyOrMovePixelRow">
    /// Receives the current unsigned Y row and whether the selected enemy-specific routine
    /// is the destructive move callback rather than the non-destructive copy callback.
    /// </param>
    /// <param name="entryFinished">
    /// Receives each entry index immediately after its final row has moved. The final entry
    /// invokes this callback too, before the routine returns carry clear.
    /// </param>
    /// <returns>True while another rotting call is required; false on final completion.</returns>
    public static bool Step(
        ISnesAddressSpace bus,
        ISnesMutableMemory memory,
        int tableAddress,
        ushort entryCount,
        ushort yLimit,
        ushort lateMoveEntryIndex,
        Action<ushort, bool> copyOrMovePixelRow,
        Action<ushort> entryFinished)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(copyOrMovePixelRow);
        ArgumentNullException.ThrowIfNull(entryFinished);
        if (entryCount == 0)
            throw new ArgumentOutOfRangeException(nameof(entryCount));

        for (ushort entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            int entryAddress = checked(tableAddress + entryIndex * EntryByteCount);
            short yOffset = unchecked((short)SnesWorkRam.ReadWord(memory, entryAddress));

            // `$FFFF` marks a finished non-final entry. The assembly uses BMI, so every
            // signed-negative value is skipped rather than testing one invented sentinel.
            if (yOffset < 0)
                continue;

            ushort timer = SnesWorkRam.ReadWord(memory, entryAddress + 2);
            if (timer != 0)
            {
                timer = unchecked((ushort)(timer - 1));
                WriteWord(bus, entryAddress + 2, timer);
                if (timer < 4)
                {
                    copyOrMovePixelRow(
                        unchecked((ushort)yOffset),
                        entryIndex >= lateMoveEntryIndex);
                }
                continue;
            }

            // A row whose delay has expired always uses the destructive callback, then
            // advances by two pixels. This produces the cartridge's interleaved fall.
            copyOrMovePixelRow(unchecked((ushort)yOffset), true);
            ushort nextYOffset = unchecked((ushort)(yOffset + 2));
            if (nextYOffset < yLimit)
            {
                WriteWord(bus, entryAddress, nextYOffset);
                continue;
            }

            entryFinished(entryIndex);
            if (entryIndex >= yLimit)
                return false;

            WriteWord(bus, entryAddress, 0xffff);
        }

        return true;
    }

    /// <summary>Writes a 16-bit table value to WRAM in the little-endian byte order used by the native processor.</summary>
    /// <param name="bus">Address space receiving the two byte writes.</param>
    /// <param name="address">Full WRAM byte address of the low byte.</param>
    /// <param name="value">Word value to store.</param>
    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }
}
