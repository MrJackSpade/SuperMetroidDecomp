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

    /// <summary>Describes one bomb-block collision/reaction pair and the shared animation tail selected by its shape and persistence mode.</summary>
    /// <param name="CollisionHead">Native instruction-list address entered when the block is broken by collision.</param>
    /// <param name="ReactionHead">Native instruction-list address entered when the block reacts to a projectile.</param>
    /// <param name="Respawns">Whether the tail restores the block after its break animation.</param>
    /// <param name="Dimension">Shape selector: zero for one block, then horizontal, vertical, or square multi-block layouts.</param>
    private readonly record struct Program(
        ushort CollisionHead, ushort ReactionHead, bool Respawns, int Dimension)
    {
        /// <summary>True when this shape uses a single PLM block and has no multi-block restoration draw.</summary>
        internal bool SingleBlock => Dimension == 0;
        /// <summary>Start address of the shared break-animation tail after the reaction-list prefix.</summary>
        internal ushort Tail => checked((ushort)(ReactionHead + 3));
        /// <summary>Number of timed poses in the shared tail: seven for respawning variants and four for permanent variants.</summary>
        internal int FrameCount => Respawns ? 7 : 4;
        /// <summary>Address immediately after the final timed frame in the shared tail.</summary>
        internal ushort Terminal => checked((ushort)(Tail + 4 * FrameCount));
    }

    /// <summary>Number of shape and persistence variants compiled for collision and projectile entry paths.</summary>
    private const int ProgramCount = 8;

    // BTS low bits select size; bit two selects permanent versus respawning.
    // The collision and projectile heads enter the same shape-specific tail.
    /// <summary>Builds the metadata for one shape and persistence variant from its shared reaction index.</summary>
    /// <param name="index">Zero-based index among the eight compiled bomb-block variants.</param>
    /// <returns>Native entry points and animation traits for the selected variant.</returns>
    private static Program ProgramAt(int index) => new(
        RoomPlmInstructionLists.CollisionBombByReactionIndex(index),
        RoomPlmInstructionLists.ReactionBombByReactionIndex(index), index < 4, index & 3);

    /// <summary>Resolves a frame or respawn draw-pointer operand to the shape-specific draw definition it selects.</summary>
    /// <param name="address">Native instruction-stream address of a draw-pointer operand.</param>
    /// <param name="value">Receives the draw definition pointer when the address belongs to a compiled program.</param>
    /// <returns>True when the address is a frame draw operand or a multi-block respawn draw operand.</returns>
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

    /// <summary>Resolves an instruction or frame-duration word from the compiled bomb-block control streams.</summary>
    /// <param name="address">Native instruction-stream address to look up.</param>
    /// <param name="value">Receives the instruction, destination, or frame duration when found.</param>
    /// <returns>True when the address is a mechanics word in one of the compiled programs.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
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

    /// <summary>Resolves the collision- or projectile-triggered sound operand at a bomb-block program head.</summary>
    /// <param name="address">Native instruction-stream byte address to look up.</param>
    /// <param name="value">Receives the library-two sound identifier when found.</param>
    /// <returns>True when the address contains a compiled break-sound operand.</returns>
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
