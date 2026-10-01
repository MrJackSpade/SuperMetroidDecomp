namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control words for ordinary, Super Missile, Power Bomb and
/// enemy-breakable shot-block PLM programs.
/// Draw-list operands name compiled terrain mutations, not editable artwork bytes.
/// Their payloads remain in the separate draw-list domain.
/// </summary>
internal static class RoomPlmShotBlockProgramDefinitions
{
    /// <summary>The cartridge's library-two block-break sound operand, $84:CADF and peers.</summary>
    internal const byte BreakSoundId = 0x0a;

    private readonly record struct Program(ushort Start, bool Respawns, bool RestoresLevelWord,
        ushort FirstDraw, int DrawStride, ushort RestoreDraw = 0,
        ushort SoundOpcode = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct)
    {
        internal int FrameCount => Respawns ? RestoresLevelWord ? 7 : 8 : 4;
        internal ushort TerminalAddress => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.RespawningShotBlock1x1, true, true,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6),
        new(RoomPlmInstructionLists.RespawningShotBlock2x1, true, false,
            RoomPlmShotBlockDrawDefinitions.HorizontalFrame0, 8, RoomPlmShotBlockDrawDefinitions.RestoreHorizontal),
        new(RoomPlmInstructionLists.RespawningShotBlock1x2, true, false,
            RoomPlmShotBlockDrawDefinitions.VerticalFrame0, 8, RoomPlmShotBlockDrawDefinitions.RestoreVertical),
        new(RoomPlmInstructionLists.RespawningShotBlock2x2, true, false,
            RoomPlmShotBlockDrawDefinitions.SquareFrame0, 16, RoomPlmShotBlockDrawDefinitions.RestoreSquare),
        new(RoomPlmInstructionLists.PermanentShotBlock1x1, false, false,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6),
        new(RoomPlmInstructionLists.PermanentShotBlock2x1, false, false,
            RoomPlmShotBlockDrawDefinitions.HorizontalFrame0, 8),
        new(RoomPlmInstructionLists.PermanentShotBlock1x2, false, false,
            RoomPlmShotBlockDrawDefinitions.VerticalFrame0, 8),
        new(RoomPlmInstructionLists.PermanentShotBlock2x2, false, false,
            RoomPlmShotBlockDrawDefinitions.SquareFrame0, 16),
        new(RoomPlmInstructionLists.RespawningSuperMissileBlock, true, true,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6,
            SoundOpcode: RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6),
        new(RoomPlmInstructionLists.RespawningPowerBombBlock, true, true,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6),
        new(RoomPlmInstructionLists.PermanentSuperMissileBlock, false, false,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6,
            SoundOpcode: RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6),
        new(RoomPlmInstructionLists.PermanentPowerBombBlock, false, false,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6),
        new(EnemyBreakableTerrainDefinitions.InstructionList, false, false,
            RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6,
            SoundOpcode: RoomPlmInstructionCodes.QueueSoundLibrary2Maximum3),
    ];

    /// <summary>
    /// Resolves the second word of each native timer/draw record, including $84:CBBC.
    /// This identity selects a collision-changing draw definition; it is not a read
    /// of that definition's artwork, nor an opcode/duration word.
    /// </summary>
    internal static bool TryReadDrawPointerWord(ushort address, out ushort value)
    {
        foreach (Program program in Programs)
        {
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
        foreach (Program program in Programs)
        {
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

    /// <summary>All authored control-word addresses, excluding draw-list operands.</summary>
    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        foreach (Program program in Programs)
        {
            yield return program.Start;
            for (int frame = 0; frame < program.FrameCount; frame++)
                yield return checked((ushort)(program.Start + 3 + 4 * frame));
            yield return program.TerminalAddress;
            if (program.RestoresLevelWord)
                yield return checked((ushort)(program.TerminalAddress + 2));
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        foreach (Program program in Programs)
            yield return checked((ushort)(program.Start + 2));
    }
}
