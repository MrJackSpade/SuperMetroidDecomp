namespace SuperMetroid.Core.Game;

/// <summary>Calculated geometry and bounded native overreads for Kraid's fingernail contour.</summary>
internal static class KraidNailContour
{
    /// <summary>$A7:BF1D..BF34, four contour records followed by two reachable instruction pairs.</summary>
    internal const int RecordCount = 6;
    /// <summary>$A7:C005, HandleKraidPhase1, operand of JSR at $A7:BF2D.</summary>
    private const ushort HandleFirstPhase = 0xc005;
    /// <summary>$0FD2, Enemy[1].instList, absolute LDA operand at $A7:BF31.</summary>
    private const ushort ArmInstructionAddress = 0x0fd2;
    /// <summary>65816 JSR absolute opcode at $A7:BF2D.</summary>
    private const byte CallOpcode = 0x20;
    /// <summary>65816 LDA absolute opcode at $A7:BF30.</summary>
    private const byte LoadOpcode = 0xad;
    /// <summary>65816 CMP immediate opcode at $A7:BF33.</summary>
    private const byte CompareOpcode = 0xc9;

    /// <summary>Returns a left offset for one of the six reachable contour records.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against pinned bank_A7.asm and original NTSC
    /// words. The four geometry ledges advance 24 pixels from -64 to8. The next two
    /// records are native instruction overreads: JSR opcode plus target low byte,
    /// then the arm instruction-list WRAM address. Preserve their unsigned words.
    /// </remarks>
    internal static ushort LeftAt(int record)
    {
        if ((uint)record >= RecordCount) throw new IndexOutOfRangeException();
        if (record < 4) return unchecked((ushort)(-64 + 24 * record));
        return record == 4 ? unchecked((ushort)(CallOpcode | HandleFirstPhase << 8)) : ArmInstructionAddress;
    }

    /// <summary>Returns the comparison boundary paired with one reachable left offset.</summary>
    /// <remarks>
    /// The first three ledges are 56 pixels apart, starting at16. The final authored
    /// head boundary is -128. Adjacent words splice the JSR target high byte with
    /// LDA, and CMP with the low byte of InstList_KraidArm_Normal_1. These are bounded
    /// instruction-field aliases, not extrapolated contour geometry.
    /// </remarks>
    internal static ushort TopAt(int record)
    {
        if ((uint)record >= RecordCount) throw new IndexOutOfRangeException();
        if (record < 3) return unchecked((ushort)(16 - 56 * record));
        return record switch
        {
            3 => unchecked((ushort)-128),
            4 => (ushort)((HandleFirstPhase >> 8) | LoadOpcode << 8),
            _ => unchecked((ushort)(CompareOpcode | KraidArmInstructionProgramDefinitions.NormalPause << 8)),
        };
    }

    /// <summary>Returns the native selected left offset for a wrapped nail-minus-body Y word.</summary>
    public static ushort LeftOffset(ushort relativeY)
    {
        for (int record = 0; record < RecordCount; record++)
            if (unchecked((short)(TopAt(record) - relativeY)) < 0)
                return LeftAt(record);
        throw new InvalidOperationException("Kraid contour definitions failed their exhaustive signed-word coverage invariant.");
    }
}
