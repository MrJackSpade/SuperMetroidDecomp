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

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.RespawningBreakableGrappleBlock, true),
        new(RoomPlmInstructionLists.PermanentBreakableGrappleBlock, false),
    ];

    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        foreach (Program program in Programs)
        {
            if (address == program.Start + 2)
            {
                value = RoomPlmGrappleBlockDrawDefinitions.Grapple;
                return true;
            }
            int drawOffset = address - program.Start - 9;
            if (drawOffset >= 0 && drawOffset % 4 == 0 && drawOffset / 4 < program.FrameCount)
            {
                ReadOnlySpan<ushort> draws = [RoomPlmGrappleBlockDrawDefinitions.BreakFrame0,
                    RoomPlmGrappleBlockDrawDefinitions.BreakFrame1, RoomPlmGrappleBlockDrawDefinitions.BreakFrame2,
                    RoomPlmGrappleBlockDrawDefinitions.Blank];
                int frame = drawOffset / 4;
                value = draws[frame < 4 ? frame : 6 - frame];
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (Program program in Programs)
        {
            if (address == program.Start)
            {
                value = program.Respawns ? (ushort)240 : (ushort)120;
                return true;
            }

            if (address == program.Start + 4)
            {
                value = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6;
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
                    ? RoomPlmInstructionCodes.SetPlmBtsToOne
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }

            if (program.Respawns && address == program.TerminalAddress + 2)
            {
                value = RoomPlmInstructionCodes.DrawPlmBlock;
                return true;
            }

            if (program.Respawns && address == program.TerminalAddress + 4)
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
            if (address == program.Start + 6)
            {
                value = BreakSoundId;
                return true;
            }
        }

        value = 0;
        return false;
    }

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        foreach (Program program in Programs)
        {
            yield return program.Start;
            yield return checked((ushort)(program.Start + 4));
            for (int frame = 0; frame < program.FrameCount; frame++)
                yield return checked((ushort)(program.Start + 7 + 4 * frame));
            yield return program.TerminalAddress;
            if (program.Respawns)
            {
                yield return checked((ushort)(program.TerminalAddress + 2));
                yield return checked((ushort)(program.TerminalAddress + 4));
            }
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        foreach (Program program in Programs)
            yield return checked((ushort)(program.Start + 6));
    }
}
