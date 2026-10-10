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

    private readonly record struct Program(ushort Start, bool Respawns, bool RestoresLevelWord,
        ushort FirstDraw, int DrawStride, ushort RestoreDraw = 0,
        RoomPlmInstruction SoundOpcode = RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct)
    {
        internal int FrameCount => Respawns ? RestoresLevelWord ? 7 : 8 : 4;
        internal ushort TerminalAddress => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    private const int ProgramCount = 13;

    // Enumeration preserves the public control-address order; named program roles
    // determine shape, restoration and sound routing instead of stored records.
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
        RoomPlmInstruction sound = start switch
        {
            RoomPlmInstructionLists.RespawningSuperMissileBlock or RoomPlmInstructionLists.PermanentSuperMissileBlock =>
                RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            EnemyBreakableTerrainDefinitions.InstructionList => RoomPlmInstruction.QueueSoundLibrary2Maximum3,
            _ => RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct,
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
                value = (ushort)program.SoundOpcode;
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
                    ? (ushort)RoomPlmInstruction.DrawPlmBlock
                    : (ushort)RoomPlmInstruction.Delete;
                return true;
            }

            if (program.RestoresLevelWord && address == program.TerminalAddress + 2)
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
