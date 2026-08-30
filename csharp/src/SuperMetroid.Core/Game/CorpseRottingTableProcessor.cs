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

    /// <summary>Reads one native table record without inventing a parallel host state.</summary>
    public static CorpseRottingTableEntry ReadEntry(
        ISnesAddressSpace bus,
        int tableAddress,
        ushort entryCount,
        int entryIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)entryIndex >= entryCount)
            throw new ArgumentOutOfRangeException(nameof(entryIndex));

        int entryAddress = checked(tableAddress + entryIndex * EntryByteCount);
        return new CorpseRottingTableEntry(
            unchecked((short)ReadWord(bus, entryAddress)),
            ReadWord(bus, entryAddress + 2));
    }

    /// <summary>Runs one complete call of the shared cartridge table processor.</summary>
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
        int tableAddress,
        ushort entryCount,
        ushort yLimit,
        ushort lateMoveEntryIndex,
        Action<ushort, bool> copyOrMovePixelRow,
        Action<ushort> entryFinished)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(copyOrMovePixelRow);
        ArgumentNullException.ThrowIfNull(entryFinished);
        if (entryCount == 0)
            throw new ArgumentOutOfRangeException(nameof(entryCount));

        for (ushort entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            int entryAddress = checked(tableAddress + entryIndex * EntryByteCount);
            short yOffset = unchecked((short)ReadWord(bus, entryAddress));

            // `$FFFF` marks a finished non-final entry. The assembly uses BMI, so every
            // signed-negative value is skipped rather than testing one invented sentinel.
            if (yOffset < 0)
                continue;

            ushort timer = ReadWord(bus, entryAddress + 2);
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

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }
}

/// <summary>One shared native <c>(signed Y offset, timer)</c> corpse-rotting record.</summary>
public readonly record struct CorpseRottingTableEntry(short YOffset, ushort Timer);
