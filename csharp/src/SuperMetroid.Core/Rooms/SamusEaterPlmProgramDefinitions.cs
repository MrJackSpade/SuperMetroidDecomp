namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The two fixed bank-$84 plant instruction lists at $84:ACB8..AD37. Their
/// timers, branches, callbacks, sound operand, and draw-list selectors are
/// named instruction cases; the selected block artwork is separate. Both use five-tick
/// chew stages and two damage callbacks per loop, repeated four/eight times for floor/ceiling.
/// Release clears the hold callback, waits 96 ticks, restores idle for one tick and deletes.
/// No program byte array or generated cache remains.
/// The byte layout preserves the cartridge's odd one-byte timer and sound
/// operands, so word reads at native instruction boundaries remain exact.
/// </summary>
internal static class SamusEaterPlmProgramDefinitions
{
    /// <summary>Floor plant program at $84:ACB8, selected by PLM $B6CB.</summary>
    internal const ushort FloorStart = 0xacb8;
    /// <summary>Ceiling plant program at $84:ACF8, selected by PLM $B6CF.</summary>
    internal const ushort CeilingStart = 0xacf8;
    /// <summary>First byte of the following treadmill program at $84:AD38.</summary>
    internal const ushort EndExclusive = 0xad38;

    /// <summary>Reads two consecutive compiled plant mechanics bytes as a little-endian instruction word.</summary>
    /// <param name="address">The bank-$84 address of the word's low byte.</param>
    /// <param name="value">Receives the assembled word, or zero if either byte is outside the compiled lists.</param>
    /// <returns><see langword="true"/> when both bytes belong to the compiled floor or ceiling program range.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryReadMechanicsByte(address, out byte low) &&
            TryReadMechanicsByte(unchecked((ushort)(address + 1)), out byte high))
        {
            value = (ushort)(low | high << 8);
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>Resolves one control, timing, draw, or sound byte from either compiled plant list.</summary>
    /// <param name="address">The bank-$84 byte address to inspect.</param>
    /// <param name="value">Receives the compiled byte, or zero when the address is outside both lists.</param>
    /// <returns><see langword="true"/> when the address is within the compiled floor or ceiling program range.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address < FloorStart || address >= EndExclusive)
        {
            value = 0;
            return false;
        }
        bool ceiling = address >= CeilingStart;
        int start = ceiling ? CeilingStart : FloorStart;
        int offset = address - start;
        if (offset == 6)
        {
            value = ceiling ? (byte)8 : (byte)4;
            return true;
        }
        if (offset == 21)
        {
            value = ChewSound;
            return true;
        }
        // The one-byte loop counter shifts instruction alignment until the
        // one-byte sound operand restores it. Preserve arbitrary byte/word views.
        int wordOffset = offset is >= 7 and < 21 ? 7 + ((offset - 7) & ~1) : offset & ~1;
        ushort word = InstructionWord(wordOffset, ceiling, start);
        value = (byte)(word >> ((offset - wordOffset) * 8));
        return true;
    }

    /// <summary>Library-two sound $31 at $84:ACCD/$AD0D accompanies each chewing loop.</summary>
    private const byte ChewSound = 0x31;

    /// <summary>Returns the instruction word at a decoded aligned offset in a floor or ceiling plant list.</summary>
    /// <param name="offset">The instruction-byte offset relative to the selected list.</param>
    /// <param name="ceiling">Whether to select ceiling-specific draw operands and loop values.</param>
    /// <param name="start">The start address used to form the list's loop-back target.</param>
    /// <returns>The compiled instruction word at the offset.</returns>
    /// <exception cref="InvalidOperationException">The offset is not a defined instruction boundary.</exception>
    private static ushort InstructionWord(int offset, bool ceiling, int start) => offset switch
    {
        0 => RoomPlmInstructionCodes.InstallPreInstruction,
        2 => SamusEaterPlmRomData.HoldPreInstruction,
        4 => RoomPlmInstructionCodes.SetEightBitTimer,
        7 or 11 or 15 or 24 or 28 or 32 or 36 or 42 => 5,
        9 or 17 or 30 or 38 => ceiling ? SamusEaterPlmDrawDefinitions.CeilingChew2 : SamusEaterPlmDrawDefinitions.FloorChew2,
        13 or 34 => ceiling ? SamusEaterPlmDrawDefinitions.CeilingChew1 : SamusEaterPlmDrawDefinitions.FloorChew1,
        26 or 44 or 56 => ceiling ? SamusEaterPlmDrawDefinitions.CeilingChew3 : SamusEaterPlmDrawDefinitions.FloorChew3,
        19 => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
        22 or 40 => SamusEaterPlmRomData.DamageInstruction,
        46 => RoomPlmInstructionCodes.DecrementTimerAndGoto,
        48 => (ushort)(start + 7),
        50 => SamusEaterPlmRomData.ReleaseImmunityInstruction,
        52 => RoomPlmInstructionCodes.ClearPreInstruction,
        54 => 96,
        58 => 1,
        60 => ceiling ? SamusEaterPlmDrawDefinitions.CeilingIdle : SamusEaterPlmDrawDefinitions.FloorIdle,
        62 => RoomPlmInstructionCodes.Delete,
        _ => throw new InvalidOperationException("Invalid plant instruction word boundary."),
    };
}
