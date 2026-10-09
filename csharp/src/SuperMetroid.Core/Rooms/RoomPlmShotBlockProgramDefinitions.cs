namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for ordinary, Super Missile, Power Bomb and
/// enemy-breakable shot-block PLM programs.
/// Draw-list operands name compiled terrain mutations, not editable artwork bytes.
/// Their payloads remain in the separate draw-list domain.
/// Native programs at $84:CADF-$CC34 and $84:CD53-$CD67 use a packed sound byte,
/// four-byte timed draw records, and named restore/delete commands. Program roles
/// select shape, respawn policy and sound queue; no descriptor table is stored.
/// </summary>
internal static class RoomPlmShotBlockProgramDefinitions
{
    /// <summary>The cartridge's library-two block-break sound operand, $84:CADF and peers.</summary>
    internal const byte BreakSoundId = 0x0a;

    /// <summary>Derived control-flow facts used to resolve the mechanics and draw words of one native shot-block instruction list.</summary>
    /// <param name="Start">First instruction address for the native PLM program.</param>
    /// <param name="Respawns">Whether the broken block later respawns instead of being permanently removed.</param>
    /// <param name="RestoresLevelWord">Whether the terminal command restores the PLM's level word before deletion.</param>
    /// <param name="FirstDraw">Draw-list pointer used for the first break-animation frame.</param>
    /// <param name="DrawStride">Address increment between successive shape frames in the draw list.</param>
    /// <param name="RestoreDraw">Draw-list pointer used by the final restoration frame, when present.</param>
    /// <param name="SoundOpcode">Native sound-queue opcode selected for this block's break sound.</param>
    private readonly record struct Program(ushort Start, bool Respawns, bool RestoresLevelWord,
        ushort FirstDraw, int DrawStride, ushort RestoreDraw = 0,
        ushort SoundOpcode = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct)
    {
        /// <summary>Number of timed draw frames emitted before the terminal command.</summary>
        internal int FrameCount => Respawns ? RestoresLevelWord ? 7 : 8 : 4;

        /// <summary>Address immediately following all timed frame records for this program.</summary>
        internal ushort TerminalAddress => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    /// <summary>Number of native shot-block programs enumerated by the address resolver.</summary>
    private const int ProgramCount = 13;

    // Enumeration preserves the public control-address order; named program roles
    // determine shape, restoration and sound routing instead of stored records.
    /// <summary>Builds the derived mechanics and draw metadata for a program in native address order.</summary>
    /// <param name="index">Zero-based index among ordinary, special-weapon, and enemy-breakable programs.</param>
    /// <returns>The addresses, timing shape, restoration behavior, and sound routing for that program.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the enumerated program set.</exception>
    private static Program ProgramAt(int index)
    {
        ushort start = index switch
        {
            0 => RoomPlmInstructionLists.RespawningShotBlock1x1,
            1 => RoomPlmInstructionLists.RespawningShotBlock2x1,
            2 => RoomPlmInstructionLists.RespawningShotBlock1x2,
            3 => RoomPlmInstructionLists.RespawningShotBlock2x2,
            4 => RoomPlmInstructionLists.PermanentShotBlock1x1,
            5 => RoomPlmInstructionLists.PermanentShotBlock2x1,
            6 => RoomPlmInstructionLists.PermanentShotBlock1x2,
            7 => RoomPlmInstructionLists.PermanentShotBlock2x2,
            8 => RoomPlmInstructionLists.RespawningSuperMissileBlock,
            9 => RoomPlmInstructionLists.RespawningPowerBombBlock,
            10 => RoomPlmInstructionLists.PermanentSuperMissileBlock,
            11 => RoomPlmInstructionLists.PermanentPowerBombBlock,
            12 => EnemyBreakableTerrainDefinitions.InstructionList,
            _ => throw new IndexOutOfRangeException(),
        };
        bool respawns = start is RoomPlmInstructionLists.RespawningShotBlock1x1 or
            RoomPlmInstructionLists.RespawningShotBlock2x1 or RoomPlmInstructionLists.RespawningShotBlock1x2 or
            RoomPlmInstructionLists.RespawningShotBlock2x2 or RoomPlmInstructionLists.RespawningSuperMissileBlock or
            RoomPlmInstructionLists.RespawningPowerBombBlock;
        (ushort first, int stride, ushort restore) = start switch
        {
            RoomPlmInstructionLists.RespawningShotBlock2x1 or RoomPlmInstructionLists.PermanentShotBlock2x1 =>
                (RoomPlmShotBlockDrawDefinitions.HorizontalFrame0, 8, RoomPlmShotBlockDrawDefinitions.RestoreHorizontal),
            RoomPlmInstructionLists.RespawningShotBlock1x2 or RoomPlmInstructionLists.PermanentShotBlock1x2 =>
                (RoomPlmShotBlockDrawDefinitions.VerticalFrame0, 8, RoomPlmShotBlockDrawDefinitions.RestoreVertical),
            RoomPlmInstructionLists.RespawningShotBlock2x2 or RoomPlmInstructionLists.PermanentShotBlock2x2 =>
                (RoomPlmShotBlockDrawDefinitions.SquareFrame0, 16, RoomPlmShotBlockDrawDefinitions.RestoreSquare),
            _ => (RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6, (ushort)0),
        };
        ushort sound = start switch
        {
            RoomPlmInstructionLists.RespawningSuperMissileBlock or RoomPlmInstructionLists.PermanentSuperMissileBlock =>
                RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            EnemyBreakableTerrainDefinitions.InstructionList => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3,
            _ => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct,
        };
        return new(start, respawns, respawns && restore == 0, first, stride, respawns ? restore : (ushort)0, sound);
    }

    /// <summary>
    /// Resolves the second word of each native timer/draw record, including $84:CBBC.
    /// This identity selects a collision-changing draw definition; it is not a read
    /// of that definition's artwork, nor an opcode/duration word.
    /// </summary>
    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            int offset = address - program.Start - 5;
            if (offset < 0 || offset % 4 != 0 || offset / 4 >= program.FrameCount)
                continue;
            int frame = offset / 4;
            value = frame == 7 ? program.RestoreDraw : checked((ushort)(
                program.FirstDraw + (frame <= 3 ? frame : 6 - frame) * program.DrawStride));
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Resolves only opcode and duration words. A draw-list pointer is a different domain
    /// even though it occupies alternating words in the same native instruction stream.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start)
            {
                value = program.SoundOpcode;
                return true;
            }

            int frameOffset = address - program.Start - 3;
            if (frameOffset >= 0 && frameOffset % 4 == 0)
            {
                int frame = frameOffset / 4;
                if (frame < program.FrameCount)
                {
                    // Native break frames last four ticks. Frame four holds an absent
                    // respawning block for 384 ticks; the final permanent/blank frame
                    // lasts one tick before the slot is deleted or restores its word.
                    value = frame == 3
                        ? program.Respawns ? (ushort)0x0180 : (ushort)1
                        : frame == 7 ? (ushort)1 : (ushort)4;
                    // The permanent Power Bomb parent uses the native faster
                    // 3/2/1/1 sequence, unlike ordinary/Super Missile parents.
                    if (program.Start == RoomPlmInstructionLists.PermanentPowerBombBlock)
                        value = (ushort)(frame < 2 ? 3 - frame : 1);
                    return true;
                }
            }

            if (address == program.TerminalAddress)
            {
                value = program.RestoresLevelWord
                    ? RoomPlmInstructionCodes.DrawPlmBlock
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }

            if (program.RestoresLevelWord && address == program.TerminalAddress + 2)
            {
                value = RoomPlmInstructionCodes.Delete;
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves a byte operand stored after a program's sound opcode.</summary>
    /// <param name="address">Address in the native instruction stream to inspect.</param>
    /// <param name="value">Receives the sound selector when the address names one.</param>
    /// <returns><see langword="true"/> if the address is a mechanics byte; otherwise <see langword="false"/>.</returns>
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
