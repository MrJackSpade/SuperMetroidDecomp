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

    private static Program ProgramAt(int index)
    {
        ushort start = index switch
        {
            0 => RoomPlmInstructionLists.SpeedBlockBrinstarSlowRespawning,
            1 => RoomPlmInstructionLists.SpeedBlockRespawning,
            2 => RoomPlmInstructionLists.SpeedBlockDachoraRespawning,
            3 => RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent,
            4 => RoomPlmInstructionLists.SpeedBlockPermanent,
            _ => throw new IndexOutOfRangeException(),
        };
        bool slow = start is RoomPlmInstructionLists.SpeedBlockBrinstarSlowRespawning
            or RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent;
        bool respawns = start is not (RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent
            or RoomPlmInstructionLists.SpeedBlockPermanent);
        return new(start, respawns, slow ? (ushort)2 : (ushort)1,
            start == RoomPlmInstructionLists.SpeedBlockDachoraRespawning);
    }

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
            value = RoomPlmInstructionCodes.Delete;
            return true;
        }

        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            if (address == program.Start)
            {
                value = RoomPlmInstructionCodes.QueueSoundLibrary2Maximum1Direct;
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
                        ? RoomPlmInstructionCodes.DrawPlmBlockClone
                        : RoomPlmInstructionCodes.DrawPlmBlock
                    : RoomPlmInstructionCodes.Delete;
                return true;
            }
            if (program.Respawns && address == program.Terminal + 2)
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

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        yield return BombReveal;
        yield return checked((ushort)(BombReveal + 2));
        yield return checked((ushort)(BombReveal + 4));
        for (int index = 0; index < ProgramCount; index++)
        {
            Program program = ProgramAt(index);
            yield return program.Start;
            for (int frame = 0; frame < program.FrameCount; frame++)
            {
                yield return checked((ushort)(program.Start + 3 + frame * 4));
                yield return checked((ushort)(program.Start + 5 + frame * 4));
            }
            yield return program.Terminal;
            if (program.Respawns)
                yield return checked((ushort)(program.Terminal + 2));
        }
    }

    internal static IEnumerable<ushort> MechanicsByteAddresses()
    {
        for (int index = 0; index < ProgramCount; index++)
            yield return checked((ushort)(ProgramAt(index).Start + 2));
    }
}
