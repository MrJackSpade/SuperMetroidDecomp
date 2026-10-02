namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for the eight Samus-contact crumble PLM lists.
/// Includes interleaved animation and linked-restoration pointer operands.
/// Their physical/presentation payloads remain separate draw definitions.
/// </summary>
internal static class RoomPlmContactCrumbleProgramDefinitions
{
    /// <summary>Direct library-two sound operand at the eight $84:C9F9-$CACA entries.</summary>
    internal const byte BreakSoundId = 0x0a;

    private readonly record struct Program(ushort Start, bool Respawns, int Dimension)
    {
        internal int FrameCount => Respawns ? 7 : 4;
        internal ushort Terminal => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    private const int ProgramCount = 8;

    // Native BTS order selects size in its low two bits and permanent versus
    // respawning behavior in bit two. Descriptors are values, not a stored table.
    private static Program ProgramAt(int index) => new(
        RoomPlmInstructionLists.ContactCrumbleByReactionIndex(index), index < 4, index & 3);

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
                    1 => RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal,
                    2 => RoomPlmContactCrumbleRestoreDrawDefinitions.Vertical,
                    3 => RoomPlmContactCrumbleRestoreDrawDefinitions.Square,
                    _ => throw new InvalidDataException("Linked crumble program has no restore shape."),
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
                value = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct;
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
                        ? RoomPlmInstructionCodes.DrawPlmBlock
                        : (ushort)1
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }

            if (program.Respawns &&
                address == program.Terminal + (program.Dimension == 0 ? 2 : 4))
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

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            yield return program.Start;
            for (int frame = 0; frame < program.FrameCount; frame++)
                yield return checked((ushort)(program.Start + 3 + 4 * frame));
            yield return program.Terminal;
            if (program.Respawns)
                yield return checked((ushort)(program.Terminal +
                    (program.Dimension == 0 ? 2 : 4)));
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        for (int index = 0; index < ProgramCount; index++)
            yield return checked((ushort)(ProgramAt(index).Start + 2));
    }
}
