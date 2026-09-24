namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for the sixteen collision/projectile bomb-block
/// entry points and their eight shared animation tails. Draw-list pointers and
/// their collision-changing payloads belong to the separate draw-list domain.
/// </summary>
internal static class RoomPlmBombBlockProgramDefinitions
{
    /// <summary>Collision-triggered library-two sound at the eight $84:CC35-$CD37 heads.</summary>
    internal const byte CollisionBreakSoundId = 0x06;
    /// <summary>Projectile-triggered library-two sound at the eight $84:CC3C-$CD3E heads.</summary>
    internal const byte ProjectileBreakSoundId = 0x0a;

    private readonly record struct Program(
        ushort CollisionHead, ushort ReactionHead, bool Respawns, bool SingleBlock)
    {
        internal ushort Tail => checked((ushort)(ReactionHead + 3));
        internal int FrameCount => Respawns ? 7 : 4;
        internal ushort Terminal => checked((ushort)(Tail + 4 * FrameCount));
    }

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.CollisionBombBlock1x1Respawning,
            RoomPlmInstructionLists.ReactionBombBlock1x1Respawning, true, true),
        new(RoomPlmInstructionLists.CollisionBombBlock2x1Respawning,
            RoomPlmInstructionLists.ReactionBombBlock2x1Respawning, true, false),
        new(RoomPlmInstructionLists.CollisionBombBlock1x2Respawning,
            RoomPlmInstructionLists.ReactionBombBlock1x2Respawning, true, false),
        new(RoomPlmInstructionLists.CollisionBombBlock2x2Respawning,
            RoomPlmInstructionLists.ReactionBombBlock2x2Respawning, true, false),
        new(RoomPlmInstructionLists.CollisionBombBlock1x1Permanent,
            RoomPlmInstructionLists.ReactionBombBlock1x1Permanent, false, true),
        new(RoomPlmInstructionLists.CollisionBombBlock2x1Permanent,
            RoomPlmInstructionLists.ReactionBombBlock2x1Permanent, false, false),
        new(RoomPlmInstructionLists.CollisionBombBlock1x2Permanent,
            RoomPlmInstructionLists.ReactionBombBlock1x2Permanent, false, false),
        new(RoomPlmInstructionLists.CollisionBombBlock2x2Permanent,
            RoomPlmInstructionLists.ReactionBombBlock2x2Permanent, false, false),
    ];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (Program program in Programs)
        {
            if (address == program.CollisionHead || address == program.ReactionHead)
            {
                value = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3;
                return true;
            }

            if (address == program.CollisionHead + 3)
            {
                value = RoomPlmInstructionCodes.Goto;
                return true;
            }

            if (address == program.CollisionHead + 5)
            {
                value = program.Tail;
                return true;
            }

            int frameOffset = address - program.Tail;
            if (frameOffset >= 0 && frameOffset % 4 == 0 &&
                frameOffset / 4 < program.FrameCount)
            {
                int frame = frameOffset / 4;
                value = frame == 3
                    ? program.Respawns ? (ushort)0x0180 : (ushort)1
                    : (ushort)4;
                return true;
            }

            if (address == program.Terminal)
            {
                value = program.Respawns
                    ? program.SingleBlock ? RoomPlmInstructionCodes.DrawPlmBlock : (ushort)1
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }

            if (program.Respawns &&
                address == program.Terminal + (program.SingleBlock ? 2 : 4))
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
            if (address == program.CollisionHead + 2)
            {
                value = CollisionBreakSoundId;
                return true;
            }

            if (address == program.ReactionHead + 2)
            {
                value = ProjectileBreakSoundId;
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
            yield return program.CollisionHead;
            yield return checked((ushort)(program.CollisionHead + 3));
            yield return checked((ushort)(program.CollisionHead + 5));
            yield return program.ReactionHead;
            for (int frame = 0; frame < program.FrameCount; frame++)
                yield return checked((ushort)(program.Tail + 4 * frame));
            yield return program.Terminal;
            if (program.Respawns)
                yield return checked((ushort)(program.Terminal +
                    (program.SingleBlock ? 2 : 4)));
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        foreach (Program program in Programs)
        {
            yield return checked((ushort)(program.CollisionHead + 2));
            yield return checked((ushort)(program.ReactionHead + 2));
        }
    }
}
