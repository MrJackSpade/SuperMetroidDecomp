namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Retail left/right Draygon cannon PLM control lists. The intervening bank-$84
/// lists belong to unused diagonal orientations and are deliberately not claimed.
/// Physical cannon draw payloads remain separate from these mechanics words.
/// </summary>
internal static class DraygonCannonPlmProgramDefinitions
{
    /// <summary>Right-facing shielded and destroyed lists, $84:DCDE-DD26.</summary>
    internal const ushort RightStart = 0xdcde;
    /// <summary>End of right-facing list, $84:DD26.</summary>
    internal const ushort RightEnd = 0xdd26;
    /// <summary>Left-facing shielded and destroyed lists, $84:DDB9-DE01.</summary>
    internal const ushort LeftStart = 0xddb9;
    /// <summary>End of left-facing list, $84:DE01.</summary>
    internal const ushort LeftEnd = 0xde01;

    /// <summary>
    /// Two 73-byte programs share shield wait, three-hit threshold, three flashing
    /// pairs and a four-frame damaged loop. Frame durations are eight for initial
    /// shield, three/four while flashing and six while damaged. Packed threshold
    /// changes word parity at offset 21; all original overlapping views are preserved.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (!TryLocate(address, out int start, out int offset) || offset == 72) return false;
        value = (ushort)(ByteAt(start, offset) | ByteAt(start, offset + 1) << 8);
        return true;
    }

    /// <summary>Reads one byte from either compiled left- or right-facing cannon instruction list.</summary>
    /// <param name="address">Bank-$84 address to resolve.</param>
    /// <param name="value">Receives the byte at the address when it belongs to a compiled list; otherwise zero.</param>
    /// <returns><see langword="true"/> when the address lies within either supported list.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (!TryLocate(address, out int start, out int offset)) return false;
        value = ByteAt(start, offset);
        return true;
    }

    /// <summary>Maps a bank-$84 address in either supported list to its list start and byte offset.</summary>
    /// <param name="address">Address to locate in a left- or right-facing cannon list.</param>
    /// <param name="start">Receives the matching list start, or zero when the address is outside both lists.</param>
    /// <param name="offset">Receives the byte offset from <paramref name="start"/>, or zero on failure.</param>
    /// <returns><see langword="true"/> when the address belongs to a supported list.</returns>
    private static bool TryLocate(ushort address, out int start, out int offset)
    {
        start = address >= RightStart && address <= RightEnd ? RightStart :
            address >= LeftStart && address <= LeftEnd ? LeftStart : 0;
        offset = start == 0 ? 0 : address - start;
        return start != 0;
    }

    /// <summary>Extracts one byte from the packed, parity-overlapping mechanics view of a cannon list.</summary>
    /// <param name="start">Start address identifying the left- or right-facing list.</param>
    /// <param name="offset">Byte offset within that list.</param>
    /// <returns>The compiled byte at the requested position.</returns>
    private static byte ByteAt(int start, int offset)
    {
        if (offset == 20) return 3;
        int wordOffset = offset < 20 ? offset & ~1 : 21 + ((offset - 21) & ~1);
        return (byte)(WordAt(start, wordOffset) >> ((offset - wordOffset) * 8));
    }

    /// <summary>Resolves one mechanics word by list-relative offset, including orientation-specific drawing commands.</summary>
    /// <param name="start">Start address identifying the left- or right-facing list.</param>
    /// <param name="offset">Word offset within the compiled list.</param>
    /// <returns>The instruction, duration, or draw operand represented at that position.</returns>
    private static ushort WordAt(int start, int offset)
    {
        bool right = start == RightStart;
        ushort shield = right ? DraygonCannonPlmDrawDefinitions.RightShieldA : DraygonCannonPlmDrawDefinitions.LeftShieldA;
        if (offset is >= 23 and < 47)
        {
            int frame = (offset - 23) / 4;
            if ((offset - 23) % 4 == 0) return (ushort)(3 + (frame & 1));
            return (frame & 1) == 0 ? shield : right ? DraygonCannonPlmDrawDefinitions.RightShieldB : DraygonCannonPlmDrawDefinitions.LeftShieldB;
        }
        if (offset is >= 53 and < 69)
        {
            if ((offset - 53) % 4 == 0) return 6;
            int frame = (offset - 53) / 4;
            return (ushort)((right ? DraygonCannonPlmDrawDefinitions.RightDamagedA : DraygonCannonPlmDrawDefinitions.LeftDamagedA) + frame * (right ? 16 : 20));
        }
        return offset switch
        {
            0 => RoomPlmInstructionCodes.LinkInstruction,
            2 => (ushort)(start + 18),
            4 => RoomPlmInstructionCodes.InstallPreInstruction,
            6 => DraygonCannonRomData.MissileHitPreInstruction,
            8 => 8,
            10 => shield,
            12 => RoomPlmInstructionCodes.Sleep,
            14 or 47 or 69 => RoomPlmInstructionCodes.Goto,
            16 or 49 => (ushort)(start + 8),
            18 => RoomPlmInstructionCodes.IncrementArgumentAndGotoIfGreaterOrEqual,
            21 => (ushort)(start + 51),
            51 => right ? RoomPlmInstructionCodes.DamageDraygonCannonFacingRight : RoomPlmInstructionCodes.DamageDraygonCannonFacingLeft,
            _ => (ushort)(start + 53), // Final goto operand, offset 71.
        };
    }
}
