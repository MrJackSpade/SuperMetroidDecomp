namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for the eight ordinary shot-block PLM programs.
/// Their interleaved draw-list operands remain presentation/terrain payloads and are
/// deliberately not resolved here.
/// </summary>
internal static class RoomPlmShotBlockProgramDefinitions
{
    /// <summary>The cartridge's library-two block-break sound operand, $84:CADF and peers.</summary>
    internal const byte BreakSoundId = 0x0a;

    private readonly record struct Program(ushort Start, bool Respawns, bool RestoresLevelWord)
    {
        internal int FrameCount => Respawns ? RestoresLevelWord ? 7 : 8 : 4;
        internal ushort TerminalAddress => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.RespawningShotBlock1x1, true, true),
        new(RoomPlmInstructionLists.RespawningShotBlock2x1, true, false),
        new(RoomPlmInstructionLists.RespawningShotBlock1x2, true, false),
        new(RoomPlmInstructionLists.RespawningShotBlock2x2, true, false),
        new(RoomPlmInstructionLists.PermanentShotBlock1x1, false, false),
        new(RoomPlmInstructionLists.PermanentShotBlock2x1, false, false),
        new(RoomPlmInstructionLists.PermanentShotBlock1x2, false, false),
        new(RoomPlmInstructionLists.PermanentShotBlock2x2, false, false),
    ];

    /// <summary>
    /// Resolves only opcode and duration words. A draw-list pointer is a different domain
    /// even though it occupies alternating words in the same native instruction stream.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (Program program in Programs)
        {
            if (address == program.Start)
            {
                value = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct;
                return true;
            }

            int frameOffset = address - program.Start - 3;
            if (frameOffset >= 0 && frameOffset % 4 == 0)
            {
                int frame = frameOffset / 4;
                if (frame < program.FrameCount)
                {
                    // Native break frames last four ticks. Frame four holds an absent
                    // respawning block for 384 ticks; the final permanent/blank frame
                    // lasts one tick before the slot is deleted or restores its word.
                    value = frame == 3
                        ? program.Respawns ? (ushort)0x0180 : (ushort)1
                        : frame == 7 ? (ushort)1 : (ushort)4;
                    return true;
                }
            }

            if (address == program.TerminalAddress)
            {
                value = program.RestoresLevelWord
                    ? RoomPlmInstructionCodes.DrawPlmBlock
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }

            if (program.RestoresLevelWord && address == program.TerminalAddress + 2)
            {
                value = RoomPlmInstructionCodes.Delete;
                return true;
            }
        }

        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        foreach (Program program in Programs)
        {
            if (address == program.Start + 2)
            {
                value = BreakSoundId;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>All authored control-word addresses, excluding draw-list operands.</summary>
    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        foreach (Program program in Programs)
        {
            yield return program.Start;
            for (int frame = 0; frame < program.FrameCount; frame++)
                yield return checked((ushort)(program.Start + 3 + 4 * frame));
            yield return program.TerminalAddress;
            if (program.RestoresLevelWord)
                yield return checked((ushort)(program.TerminalAddress + 2));
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        foreach (Program program in Programs)
            yield return checked((ushort)(program.Start + 2));
    }
}
