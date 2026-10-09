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

    /// <summary>Describes one native crumble instruction list's address and BTS-selected behavior.</summary>
    /// <param name="Start">Bank-$84 address of the list's sound instruction.</param>
    /// <param name="Respawns">Whether the list restores a broken block after its crumble animation.</param>
    /// <param name="Dimension">Shape selector from BTS low bits: zero is a single block; one through three select wider shapes.</param>
    private readonly record struct Program(ushort Start, bool Respawns, int Dimension)
    {
        /// <summary>Gets the number of animation frames before the list reaches its terminal instruction.</summary>
        internal int FrameCount => Respawns ? 7 : 4;

        /// <summary>Gets the address of the instruction following the final animation-frame entry.</summary>
        internal ushort Terminal => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    /// <summary>Number of contact-crumble lists represented by the four shapes and two behaviors.</summary>
    private const int ProgramCount = 8;

    // Native BTS order selects size in its low two bits and permanent versus
    // respawning behavior in bit two. Descriptors are values, not a stored table.
    /// <summary>Derives the crumble-list descriptor for a native reaction-table index.</summary>
    /// <param name="index">BTS reaction index in the catalog's eight-entry order.</param>
    /// <returns>The bank-$84 start address, respawn mode, and shape for that entry.</returns>
    private static Program ProgramAt(int index) => new(
        RoomPlmInstructionLists.ContactCrumbleByReactionIndex(index), index < 4, index & 3);

    /// <summary>Resolves draw-pointer operands embedded in crumble animation lists and linked restore lists.</summary>
    /// <param name="address">Bank-$84 instruction address to inspect.</param>
    /// <param name="value">Receives the matching animation or restoration draw pointer.</param>
    /// <returns><see langword="true"/> when the address contains a draw pointer; otherwise <see langword="false"/>.</returns>
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

    /// <summary>Resolves sound, timing, block-draw, and terminal control words in a crumble instruction list.</summary>
    /// <param name="address">Bank-$84 instruction address to inspect.</param>
    /// <param name="value">Receives the mechanics word when the address belongs to a represented list.</param>
    /// <returns><see langword="true"/> when a mechanics word is defined at the address; otherwise <see langword="false"/>.</returns>
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

    /// <summary>Resolves the one-byte sound identifier operand at each crumble-list entry point.</summary>
    /// <param name="address">Bank-$84 byte address to inspect.</param>
    /// <param name="value">Receives the sound identifier when the address is a represented operand.</param>
    /// <returns><see langword="true"/> when the address contains a mechanics byte; otherwise <see langword="false"/>.</returns>
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
