namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control and frame selectors for the five reachable Speed
/// Booster collision PLMs and the one bomb-special reveal. Physical draw
/// payloads and editable block appearances remain separate definitions.
/// </summary>
internal static class SpeedBoosterBlockPlmProgramDefinitions
{
    /// <summary><c>$84:C928</c>: a normal bomb briefly reveals a special Speed Booster block.</summary>
    internal const ushort BombReveal = RoomPlmInstructionLists.BombReactionSpeedBlock;
    /// <summary><c>$84:A4F3</c>: the one-block physical reveal selected by the bomb list.</summary>
    internal const ushort BombRevealDraw = 0xa4f3;
    /// <summary>Library-two destruction sound operand <c>$06</c> in all five contact lists.</summary>
    internal const byte BreakSoundId = 0x06;

    private readonly record struct Program(
        ushort Start, bool Respawns, ushort InitialCrumbleDelay,
        bool UseDrawBlockClone)
    {
        internal int FrameCount => Respawns ? 7 : 4;
        internal ushort Terminal => checked((ushort)(Start + 3 + FrameCount * 4));
    }

    private const int ProgramCount = 5;

    /// <summary>The five reachable Speed Booster collision programs, in native table order.</summary>
    private enum SpeedBlockProgram
    {
        /// <summary>Slow-crumbling respawning Brinstar block.</summary>
        BrinstarSlowRespawning,
        /// <summary>Ordinary respawning block.</summary>
        Respawning,
        /// <summary>Dachora-room respawning block drawn through the block clone.</summary>
        DachoraRespawning,
        /// <summary>Slow-crumbling permanent Brinstar block.</summary>
        BrinstarSlowPermanent,
        /// <summary>Ordinary permanent block.</summary>
        Permanent,
    }

    private static Program ProgramAt(int index) => ProgramOf(index switch
    {
        0 => SpeedBlockProgram.BrinstarSlowRespawning,
        1 => SpeedBlockProgram.Respawning,
        2 => SpeedBlockProgram.DachoraRespawning,
        3 => SpeedBlockProgram.BrinstarSlowPermanent,
        4 => SpeedBlockProgram.Permanent,
        _ => throw new IndexOutOfRangeException(),
    });

    private static Program ProgramOf(SpeedBlockProgram program) => program switch
    {
        SpeedBlockProgram.BrinstarSlowRespawning => new(RoomPlmInstructionLists.SpeedBlockBrinstarSlowRespawning,
            Respawns: true, InitialCrumbleDelay: 2, UseDrawBlockClone: false),
        SpeedBlockProgram.Respawning => new(RoomPlmInstructionLists.SpeedBlockRespawning,
            Respawns: true, InitialCrumbleDelay: 1, UseDrawBlockClone: false),
        SpeedBlockProgram.DachoraRespawning => new(RoomPlmInstructionLists.SpeedBlockDachoraRespawning,
            Respawns: true, InitialCrumbleDelay: 1, UseDrawBlockClone: true),
        SpeedBlockProgram.BrinstarSlowPermanent => new(RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent,
            Respawns: false, InitialCrumbleDelay: 2, UseDrawBlockClone: false),
        SpeedBlockProgram.Permanent => new(RoomPlmInstructionLists.SpeedBlockPermanent,
            Respawns: false, InitialCrumbleDelay: 1, UseDrawBlockClone: false),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBlockProgram)} {program}."),
    };

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == BombReveal)
        {
            value = 1;
            return true;
        }
        if (address == BombReveal + 2)
        {
            value = BombRevealDraw;
            return true;
        }
        if (address == BombReveal + 4)
        {
            value = (ushort)RoomPlmInstruction.Delete;
            return true;
        }

        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start)
            {
                value = (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct;
                return true;
            }
            int frameOffset = address - program.Start - 3;
            if (frameOffset >= 0 && frameOffset / 4 < program.FrameCount)
            {
                int frame = frameOffset / 4;
                if (frameOffset % 4 == 0)
                {
                    value = frame switch
                    {
                        < 3 => program.InitialCrumbleDelay,
                        3 => program.Respawns ? (ushort)48 : (ushort)1,
                        _ => 4,
                    };
                    return true;
                }
                if (frameOffset % 4 == 2)
                {
                    int artFrame = frame <= 3 ? frame : 6 - frame;
                    value = checked((ushort)(
                        RoomPlmShotBlockDrawDefinitions.SingleFrame0 +
                        artFrame * 6));
                    return true;
                }
            }
            if (address == program.Terminal)
            {
                value = program.Respawns
                    ? program.UseDrawBlockClone
                        ? (ushort)RoomPlmInstruction.DrawPlmBlockClone
                        : (ushort)RoomPlmInstruction.DrawPlmBlock
                    : (ushort)RoomPlmInstruction.Delete;
                return true;
            }
            if (program.Respawns && address == program.Terminal + 2)
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
