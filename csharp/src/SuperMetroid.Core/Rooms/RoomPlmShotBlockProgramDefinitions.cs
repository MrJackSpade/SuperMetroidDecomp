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

    /// <summary>Program roles in the native control-address enumeration order.</summary>
    private enum Role
    {
        /// <summary>Identifies respawning shot1x1.</summary>
        RespawningShot1x1,
        /// <summary>Identifies respawning shot2x1.</summary>
        RespawningShot2x1,
        /// <summary>Identifies respawning shot1x2.</summary>
        RespawningShot1x2,
        /// <summary>Identifies respawning shot2x2.</summary>
        RespawningShot2x2,
        /// <summary>Identifies permanent shot1x1.</summary>
        PermanentShot1x1,
        /// <summary>Identifies permanent shot2x1.</summary>
        PermanentShot2x1,
        /// <summary>Identifies permanent shot1x2.</summary>
        PermanentShot1x2,
        /// <summary>Identifies permanent shot2x2.</summary>
        PermanentShot2x2,
        /// <summary>Identifies respawning super missile.</summary>
        RespawningSuperMissile,
        /// <summary>Identifies respawning power bomb.</summary>
        RespawningPowerBomb,
        /// <summary>Identifies permanent super missile.</summary>
        PermanentSuperMissile,
        /// <summary>Identifies permanent power bomb.</summary>
        PermanentPowerBomb,
        /// <summary>Identifies enemy breakable terrain.</summary>
        EnemyBreakableTerrain,
    }

    /// <summary>Represents program.</summary>
    private readonly record struct Program(Role Role, ushort Start, bool Respawns, bool RestoresLevelWord,
        ushort FirstDraw, int DrawStride, ushort RestoreDraw = 0,
        RoomPlmInstruction SoundOpcode = RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct)
    {
        /// <summary>Gets frame count.</summary>
        internal int FrameCount => Respawns ? RestoresLevelWord ? 7 : 8 : 4;
        /// <summary>Gets terminal address.</summary>
        internal ushort TerminalAddress => checked((ushort)(Start + 3 + 4 * FrameCount));
    }

    // Enumeration preserves the public control-address order; named program roles
    // determine shape, restoration and sound routing instead of stored records.
    /// <summary>Stores programs.</summary>
    private static readonly Program[] Programs = [.. Enum.GetValues<Role>().Select(ProgramFor)];

    /// <summary>Starts of.</summary>
    private static ushort StartOf(Role role) => role switch
    {
        Role.RespawningShot1x1 => RoomPlmInstructionLists.RespawningShotBlock1x1,
        Role.RespawningShot2x1 => RoomPlmInstructionLists.RespawningShotBlock2x1,
        Role.RespawningShot1x2 => RoomPlmInstructionLists.RespawningShotBlock1x2,
        Role.RespawningShot2x2 => RoomPlmInstructionLists.RespawningShotBlock2x2,
        Role.PermanentShot1x1 => RoomPlmInstructionLists.PermanentShotBlock1x1,
        Role.PermanentShot2x1 => RoomPlmInstructionLists.PermanentShotBlock2x1,
        Role.PermanentShot1x2 => RoomPlmInstructionLists.PermanentShotBlock1x2,
        Role.PermanentShot2x2 => RoomPlmInstructionLists.PermanentShotBlock2x2,
        Role.RespawningSuperMissile => RoomPlmInstructionLists.RespawningSuperMissileBlock,
        Role.RespawningPowerBomb => RoomPlmInstructionLists.RespawningPowerBombBlock,
        Role.PermanentSuperMissile => RoomPlmInstructionLists.PermanentSuperMissileBlock,
        Role.PermanentPowerBomb => RoomPlmInstructionLists.PermanentPowerBombBlock,
        Role.EnemyBreakableTerrain => EnemyBreakableTerrainDefinitions.InstructionList,
        _ => throw new InvalidOperationException($"Undefined shot-block role {role}."),
    };

    /// <summary>Program for.</summary>
    private static Program ProgramFor(Role role)
    {
        bool respawns = role is Role.RespawningShot1x1 or Role.RespawningShot2x1 or
            Role.RespawningShot1x2 or Role.RespawningShot2x2 or Role.RespawningSuperMissile or
            Role.RespawningPowerBomb;
        (ushort first, int stride, ushort restore) = role switch
        {
            Role.RespawningShot2x1 or Role.PermanentShot2x1 =>
                (RoomPlmShotBlockDrawDefinitions.HorizontalFrame0, 8, (ushort)ShotBlockRestoreDraw.Horizontal),
            Role.RespawningShot1x2 or Role.PermanentShot1x2 =>
                (RoomPlmShotBlockDrawDefinitions.VerticalFrame0, 8, (ushort)ShotBlockRestoreDraw.Vertical),
            Role.RespawningShot2x2 or Role.PermanentShot2x2 =>
                (RoomPlmShotBlockDrawDefinitions.SquareFrame0, 16, (ushort)ShotBlockRestoreDraw.Square),
            Role.RespawningShot1x1 or Role.PermanentShot1x1 or Role.RespawningSuperMissile or
                Role.RespawningPowerBomb or Role.PermanentSuperMissile or Role.PermanentPowerBomb or
                Role.EnemyBreakableTerrain =>
                (RoomPlmShotBlockDrawDefinitions.SingleFrame0, 6, (ushort)0),
            _ => throw new InvalidOperationException($"Undefined shot-block role {role}."),
        };
        RoomPlmInstruction sound = role switch
        {
            Role.RespawningSuperMissile or Role.PermanentSuperMissile =>
                RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            Role.EnemyBreakableTerrain => RoomPlmInstruction.QueueSoundLibrary2Maximum3,
            Role.RespawningShot1x1 or Role.RespawningShot2x1 or Role.RespawningShot1x2 or
                Role.RespawningShot2x2 or Role.PermanentShot1x1 or Role.PermanentShot2x1 or
                Role.PermanentShot1x2 or Role.PermanentShot2x2 or Role.RespawningPowerBomb or
                Role.PermanentPowerBomb => RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct,
            _ => throw new InvalidOperationException($"Undefined shot-block role {role}."),
        };
        return new(role, StartOf(role), respawns, respawns && restore == 0, first, stride,
            respawns ? restore : (ushort)0, sound);
    }

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
                    if (program.Role == Role.PermanentPowerBomb)
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

    /// <summary>Tries to read mechanics byte.</summary>
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
}
