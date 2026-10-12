namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded Kraid ceiling and spike PLM programs at $84:AB6D..ABE2. The
/// $ABD6 move-right callback is machine code, not an instruction list.
/// </summary>
internal static class KraidRoomPlmProgramDefinitions
{
    /// <summary><c>$84:AB6D</c>: ceiling crumble into background one.</summary>
    internal const ushort CrumbleCeilingBackground1 =
        RoomPlmInstructionLists.CrumbleKraidCeilingIntoBackground1;
    /// <summary><c>$84:ABA3</c>: clear the defeated ceiling.</summary>
    internal const ushort ClearCeiling = RoomPlmInstructionLists.ClearKraidCeiling;
    /// <summary><c>$84:ABA9</c>: eleven two-block spike crumble passes.</summary>
    internal const ushort CrumbleSpikes = RoomPlmInstructionLists.CrumbleKraidSpikes;
    /// <summary><c>$84:ABAC</c>: spike loop body target.</summary>
    internal const ushort SpikeLoopBody = 0xabac;
    /// <summary><c>$84:ABD6</c>: first byte of move-right callback machine code.</summary>
    internal const ushort MoveRightCallback = (ushort)RoomPlmInstruction.MoveRightOneBlock;
    /// <summary><c>$84:ABDD</c>: clear defeated spikes.</summary>
    internal const ushort ClearSpikes = RoomPlmInstructionLists.ClearKraidSpikes;
    /// <summary><c>$84:ABAB</c>: number of two-block spike crumble passes.</summary>
    internal const byte SpikePassCount = 11;
    /// <summary>Native duration of each Kraid crumble appearance.</summary>
    internal const ushort CrumbleFrameDuration = 3;

    /// <summary>Evaluates the three ceiling crumble programs, two clear programs and
    /// two-column spike loop. Only exact instruction-word starts are accepted.</summary>
    /// <remarks>Each crumble stage is four duration/draw pairs followed by a command.
    /// The ceiling groups are eighteen bytes apart; the spike loop uses the same
    /// layout twice, moves right after each column, then decrements its byte timer.
    /// The byte timer changes alignment at ABAC; ABD6..ABDC is excluded machine code.</remarks>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int ceilingOffset = address - CrumbleCeilingBackground1;
        if ((uint)ceilingOffset < ClearCeiling - CrumbleCeilingBackground1 &&
            (ceilingOffset & 1) == 0)
        {
            ushort finalDraw = (ceilingOffset / 18) switch
            {
                0 => (ushort)KraidRoomDraw.CeilingBackground1,
                1 => (ushort)KraidRoomDraw.CeilingBackground2,
                _ => (ushort)KraidRoomDraw.CeilingBackground3,
            };
            value = CrumbleWord((ceilingOffset % 18) / 2, finalDraw, (ushort)RoomPlmInstruction.Delete);
            return true;
        }
        int spikeOffset = address - SpikeLoopBody;
        if ((uint)spikeOffset < MoveRightCallback - SpikeLoopBody && (spikeOffset & 1) == 0)
        {
            int word = spikeOffset / 2;
            value = word < 18
                ? CrumbleWord(word % 9, word < 9
                    ? (ushort)KraidRoomDraw.SpikeFirst : (ushort)KraidRoomDraw.SpikeSecond,
                    MoveRightCallback)
                : word switch
                {
                    18 => (ushort)RoomPlmInstruction.DecrementTimerAndGoto,
                    19 => SpikeLoopBody,
                    _ => (ushort)RoomPlmInstruction.Delete,
                };
            return true;
        }
        if (!Enum.IsDefined((ProgramWord)address))
        {
            value = 0;
            return false;
        }
        value = (ProgramWord)address switch
        {
            ProgramWord.ClearCeilingDuration or ProgramWord.ClearSpikesDuration => 1,
            ProgramWord.ClearCeilingDraw => (ushort)KraidRoomDraw.ClearCeiling,
            ProgramWord.ClearSpikesDraw => (ushort)KraidRoomDraw.ClearSpikes,
            ProgramWord.ClearCeilingDelete or ProgramWord.ClearSpikesDelete => (ushort)RoomPlmInstruction.Delete,
            ProgramWord.CrumbleSpikesSetTimer => (ushort)RoomPlmInstruction.SetEightBitTimer,
            _ => throw new InvalidOperationException($"Undefined {nameof(ProgramWord)} {address:X4}."),
        };
        return true;
    }

    /// <summary>Instruction words outside the regular crumble groups and spike loop, by bank-<c>$84</c> address.</summary>
    private enum ProgramWord : ushort
    {
        /// <summary>$84:ABA3: ceiling-clear duration.</summary>
        ClearCeilingDuration = ClearCeiling,
        /// <summary>$84:ABA5: ceiling-clear draw.</summary>
        ClearCeilingDraw = ClearCeiling + 2,
        /// <summary>$84:ABA7: ceiling-clear delete opcode.</summary>
        ClearCeilingDelete = ClearCeiling + 4,
        /// <summary>$84:ABA9: spike set-eight-bit-timer opcode; its byte operand follows.</summary>
        CrumbleSpikesSetTimer = CrumbleSpikes,
        /// <summary>$84:ABDD: spike-clear duration.</summary>
        ClearSpikesDuration = ClearSpikes,
        /// <summary>$84:ABDF: spike-clear draw.</summary>
        ClearSpikesDraw = ClearSpikes + 2,
        /// <summary>$84:ABE1: spike-clear delete opcode.</summary>
        ClearSpikesDelete = ClearSpikes + 4,
    }

    private static ushort CrumbleWord(int word, ushort finalDraw, ushort afterDraw) => word switch
    {
        0 or 2 or 4 or 6 => CrumbleFrameDuration,
        1 => (ushort)KraidRoomDraw.CrumbleFirst,
        3 => (ushort)KraidRoomDraw.CrumbleSecond,
        5 => (ushort)KraidRoomDraw.CrumbleThird,
        7 => finalDraw,
        8 => afterDraw,
        _ => throw new ArgumentOutOfRangeException(nameof(word)),
    };

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == CrumbleSpikes + 2)
        {
            value = SpikePassCount;
            return true;
        }
        value = 0;
        return false;
    }
}
