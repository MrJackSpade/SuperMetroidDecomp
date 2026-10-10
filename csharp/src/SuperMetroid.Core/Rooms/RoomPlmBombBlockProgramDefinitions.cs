namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for the sixteen collision/projectile bomb-block
/// entry points and their eight shared animation tails, including draw-pointer
/// operands. Collision-changing draw payloads belong to the separate draw domain.
/// </summary>
internal static class RoomPlmBombBlockProgramDefinitions
{
    /// <summary>Collision-triggered library-two sound at the eight $84:CC35-$CD37 heads.</summary>
    internal const byte CollisionBreakSoundId = 0x06;
    /// <summary>Projectile-triggered library-two sound at the eight $84:CC3C-$CD3E heads.</summary>
    internal const byte ProjectileBreakSoundId = 0x0a;

    private readonly record struct Program(
        ushort CollisionHead, ushort ReactionHead, bool Respawns, int Dimension)
    {
        internal bool SingleBlock => Dimension == 0;
        internal ushort Tail => checked((ushort)(ReactionHead + 3));
        internal int FrameCount => Respawns ? 7 : 4;
        internal ushort Terminal => checked((ushort)(Tail + 4 * FrameCount));
    }

    private const int ProgramCount = 8;

    // BTS low bits select size; bit two selects permanent versus respawning.
    // The collision and projectile heads enter the same shape-specific tail.
    private static Program ProgramAt(int index) => new(
        RoomPlmInstructionLists.CollisionBombByReactionIndex(index),
        RoomPlmInstructionLists.ReactionBombByReactionIndex(index), index < 4, index & 3);

    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            int dimension = program.Dimension;
            int drawOffset = address - program.Tail - 2;
            if (drawOffset >= 0 && drawOffset % 4 == 0 && drawOffset / 4 < program.FrameCount)
            {
                value = RoomPlmBreakAnimationDefinitions.DrawForShape(dimension, drawOffset / 4);
                return true;
            }
            if (program.Respawns && !program.SingleBlock && address == program.Terminal + 2)
            {
                value = dimension switch
                {
                    1 => RoomPlmBombBlockRestoreDrawDefinitions.Horizontal,
                    2 => RoomPlmBombBlockRestoreDrawDefinitions.Vertical,
                    3 => RoomPlmBombBlockRestoreDrawDefinitions.Square,
                    _ => throw new InvalidDataException("Linked bomb-block program has no restore shape."),
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
            if (address == program.CollisionHead || address == program.ReactionHead)
            {
                value = (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum3;
                return true;
            }

            if (address == program.CollisionHead + 3)
            {
                value = (ushort)RoomPlmInstruction.Goto;
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
                    ? program.SingleBlock ? (ushort)RoomPlmInstruction.DrawPlmBlock : (ushort)1
                    : (ushort)RoomPlmInstruction.Delete;
                return true;
            }

            if (program.Respawns &&
                address == program.Terminal + (program.SingleBlock ? 2 : 4))
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
}
