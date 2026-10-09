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

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (!TryLocate(address, out int start, out int offset)) return false;
        value = ByteAt(start, offset);
        return true;
    }

    private static bool TryLocate(ushort address, out int start, out int offset)
    {
        start = address is >= RightStart and <= RightEnd ? RightStart :
            address is >= LeftStart and <= LeftEnd ? LeftStart : 0;
        offset = start == 0 ? 0 : address - start;
        return start != 0;
    }

    private static byte ByteAt(int start, int offset)
    {
        if (offset == 20) return 3;
        int wordOffset = offset < 20 ? offset & ~1 : 21 + ((offset - 21) & ~1);
        return (byte)(WordAt(start, wordOffset) >> ((offset - wordOffset) * 8));
    }

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
