namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for the eight Samus-contact crumble PLM lists.
/// Interleaved animation and linked-restoration draw pointers are separate
/// physical/presentation payloads and are not claimed by this catalog.
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

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.ContactCrumble1x1Respawning, true, 0),
        new(RoomPlmInstructionLists.ContactCrumble2x1Respawning, true, 1),
        new(RoomPlmInstructionLists.ContactCrumble1x2Respawning, true, 2),
        new(RoomPlmInstructionLists.ContactCrumble2x2Respawning, true, 3),
        new(RoomPlmInstructionLists.ContactCrumble1x1Permanent, false, 0),
        new(RoomPlmInstructionLists.ContactCrumble2x1Permanent, false, 1),
        new(RoomPlmInstructionLists.ContactCrumble1x2Permanent, false, 2),
        new(RoomPlmInstructionLists.ContactCrumble2x2Permanent, false, 3),
    ];

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

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        foreach (Program program in Programs)
        {
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
        foreach (Program program in Programs)
            yield return checked((ushort)(program.Start + 2));
    }
}
