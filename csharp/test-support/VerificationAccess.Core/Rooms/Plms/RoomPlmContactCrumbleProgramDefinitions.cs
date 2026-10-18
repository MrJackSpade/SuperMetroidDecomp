namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Fixed bank-$84 control words for the eight Samus-contact crumble PLM lists.
/// Includes interleaved animation and linked-restoration pointer operands.
/// Their physical/presentation payloads remain separate draw definitions.
/// </summary>
internal static class RoomPlmContactCrumbleProgramDefinitions
{
    /// <summary>Direct library-two sound operand at the eight $84:C9F9-$CACA entries.</summary>
    internal const byte BreakSoundId = 0x0a;

    /// <summary>One contact-crumble list's start, shape, and permanent or respawning behavior.</summary>
    internal readonly record struct Program(ushort Start, bool Respawns, int Dimension)
    {
        /// <summary>Respawning lists draw seven stages; permanent lists stop after four.</summary>
        internal int FrameCount => Respawns ? 7 : 4;
        /// <summary>Address immediately after the list's timed draw stages.</summary>
        internal ushort Terminal => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    /// <summary>Four shapes each have a respawning and a permanent contact list.</summary>
    internal const int ProgramCount = 8;

    /// <summary>Decodes shape from low BTS bits and respawn behavior from bit two without a stored descriptor table.</summary>
    internal static Program ProgramAt(int index) => new(
        RoomPlmInstructionListsConstants.ContactCrumbleByReactionIndex(index), index < 4, index & 3);

    /// <summary>Resolves a timed animation draw or linked restore pointer in one of the eight lists.</summary>
    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            int drawOffset = address - program.Start - 5;
            if (drawOffset >= 0 && drawOffset % 4 == 0 && drawOffset / 4 < program.FrameCount)
            {
                value = RoomPlmBreakAnimationDefinitions.DrawForShape(program.Dimension, drawOffset / 4);
                return true;
            }
            if (program.Respawns && program.Dimension != 0 && address == program.Terminal + 2)
            {
                value = program.Dimension switch
                {
                    1 => (ushort)ContactCrumbleRestoreDraw.Horizontal,
                    2 => (ushort)ContactCrumbleRestoreDraw.Vertical,
                    3 => (ushort)ContactCrumbleRestoreDraw.Square,
                    _ => throw new InvalidDataException("Linked crumble program has no restore shape."),
                };
                return true;
            }
        }
        value = 0;
        return false;
    }

    /// <summary>Resolves sound, duration, final draw, and deletion control words by native address.</summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start)
            {
                value = (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct;
                return true;
            }

            int frameOffset = address - program.Start - 3;
            if (frameOffset >= 0 && frameOffset % 4 == 0 &&
                frameOffset / 4 < program.FrameCount)
            {
                int frame = frameOffset / 4;
                value = !program.Respawns
                    ? frame == 3 ? (ushort)1 : (ushort)4
                    : (ushort)(frame switch
                    {
                        0 when program.Dimension == 0 => 8,
                        1 when program.Dimension == 0 => 6,
                        3 when program.Dimension <= 1 => 16,
                        3 => 32,
                        _ => 4,
                    });
                return true;
            }

            if (address == program.Terminal)
            {
                value = program.Respawns
                    ? program.Dimension == 0
                        ? (ushort)RoomPlmInstruction.DrawPlmBlock
                        : (ushort)1
                    : (ushort)RoomPlmInstruction.Delete;
                return true;
            }

            if (program.Respawns &&
                address == program.Terminal + (program.Dimension == 0 ? 2 : 4))
            {
                value = (ushort)RoomPlmInstruction.Delete;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves the direct break-sound byte following each list's initial command.</summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start + 2)
            {
                value = BreakSoundId;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
