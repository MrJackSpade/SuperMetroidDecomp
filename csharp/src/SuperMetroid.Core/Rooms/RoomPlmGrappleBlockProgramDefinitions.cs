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

    /// <summary>Describes one native Grapple-block PLM instruction stream and its timing variant.</summary>
    /// <param name="Start">Bank-local address of the stream's first instruction word.</param>
    /// <param name="Respawns">Whether this stream uses the longer respawning-block sequence and terminal commands.</param>
    private readonly record struct Program(ushort Start, bool Respawns)
    {
        /// <summary>Number of timed breakup frames encoded by this stream's variant.</summary>
        internal int FrameCount => Respawns ? 7 : 4;

        /// <summary>Address of the terminal command word after the timed frame entries.</summary>
        internal ushort TerminalAddress => checked((ushort)(Start + 7 + 4 * FrameCount));
    }

    /// <summary>Number of native instruction streams represented by this definition set.</summary>
    private const int ProgramCount = 2;

    /// <summary>Returns the respawning or permanent stream metadata by its stable local index.</summary>
    /// <param name="index">Zero for the respawning stream or one for the permanent stream.</param>
    /// <returns>The start pointer and timing behavior for the selected stream.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two defined streams.</exception>
    private static Program ProgramAt(int index) => index switch
    {
        0 => new(RoomPlmInstructionLists.RespawningBreakableGrappleBlock, true),
        1 => new(RoomPlmInstructionLists.PermanentBreakableGrappleBlock, false),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>
    /// Resolves address operands that select the grapple sprite or a breakup-frame draw
    /// pointer; instruction words and addresses outside those operand slots are not matched.
    /// </summary>
    /// <param name="address">Bank-local address being checked within either PLM instruction stream.</param>
    /// <param name="value">Receives the mapped draw-list pointer, or zero when no operand is present.</param>
    /// <returns><see langword="true"/> when the address contains a draw-pointer operand.</returns>
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

    /// <summary>Resolves instruction words for timers, sound queueing, frame timing, and terminal PLM actions.</summary>
    /// <param name="address">Bank-local address being checked within either PLM instruction stream.</param>
    /// <param name="value">Receives the mapped command operand, or zero when the address is not a mechanics word.</param>
    /// <returns><see langword="true"/> when the address contains a word used by the instruction program.</returns>
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

    /// <summary>Resolves the one-byte sound identifier operand shared by the two breakup programs.</summary>
    /// <param name="address">Bank-local address being checked within either PLM instruction stream.</param>
    /// <param name="value">Receives the sound identifier, or zero when the address is not that operand.</param>
    /// <returns><see langword="true"/> when the address contains the sound-byte operand.</returns>
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
