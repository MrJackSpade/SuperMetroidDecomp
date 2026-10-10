namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge control words for the two breakable-Grapple-block PLM instruction
/// streams, including interleaved draw-pointer operands. Presentation and
/// terrain payloads remain in the separate draw-list domain.
/// </summary>
internal static class RoomPlmGrappleBlockProgramDefinitions
{
    /// <summary>Library-two sound operand at $84:CD70 and $84:CDAF.</summary>
    internal const byte BreakSoundId = 0x0a;

    private readonly record struct Program(ushort Start, bool Respawns)
    {
        internal int FrameCount => Respawns ? 7 : 4;
        internal ushort TerminalAddress => checked((ushort)(Start + 7 + 4 * FrameCount));
    }

    private const int ProgramCount = 2;

    private static Program ProgramAt(int index) => index switch
    {
        0 => new(RoomPlmInstructionLists.RespawningBreakableGrappleBlock, true),
        1 => new(RoomPlmInstructionLists.PermanentBreakableGrappleBlock, false),
        _ => throw new IndexOutOfRangeException(),
    };

    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start + 2)
            {
                value = RoomPlmGrappleBlockDrawDefinitions.Grapple;
                return true;
            }
            int drawOffset = address - program.Start - 9;
            if (drawOffset >= 0 && drawOffset % 4 == 0 && drawOffset / 4 < program.FrameCount)
            {
                int frame = drawOffset / 4;
                // Three breakup poses, blank, then the same poses in reverse.
                value = (frame < 4 ? frame : 6 - frame) switch
                {
                    0 => RoomPlmGrappleBlockDrawDefinitions.BreakFrame0,
                    1 => RoomPlmGrappleBlockDrawDefinitions.BreakFrame1,
                    2 => RoomPlmGrappleBlockDrawDefinitions.BreakFrame2,
                    _ => RoomPlmGrappleBlockDrawDefinitions.Blank,
                };
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start)
            {
                value = program.Respawns ? (ushort)240 : (ushort)120;
                return true;
            }

            if (address == program.Start + 4)
            {
                value = (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6;
                return true;
            }

            int frameOffset = address - program.Start - 7;
            if (frameOffset >= 0 && frameOffset % 4 == 0 &&
                frameOffset / 4 < program.FrameCount)
            {
                int frame = frameOffset / 4;
                value = program.Respawns && frame == 3 ? (ushort)6
                    : !program.Respawns && frame == 3 ? (ushort)1
                    : (ushort)4;
                return true;
            }

            if (address == program.TerminalAddress)
            {
                value = program.Respawns
                    ? (ushort)RoomPlmInstruction.SetPlmBtsToOne
                    : (ushort)RoomPlmInstruction.Delete;
                return true;
            }

            if (program.Respawns && address == program.TerminalAddress + 2)
            {
                value = (ushort)RoomPlmInstruction.DrawPlmBlock;
                return true;
            }

            if (program.Respawns && address == program.TerminalAddress + 4)
            {
                value = (ushort)RoomPlmInstruction.Delete;
                return true;
            }
        }

        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start + 6)
            {
                value = BreakSoundId;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
