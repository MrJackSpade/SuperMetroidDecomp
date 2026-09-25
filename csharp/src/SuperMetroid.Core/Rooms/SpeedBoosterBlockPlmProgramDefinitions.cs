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

    private static readonly Program[] Programs =
    [
        new(RoomPlmInstructionLists.SpeedBlockBrinstarSlowRespawning,
            Respawns: true, InitialCrumbleDelay: 2, UseDrawBlockClone: false),
        new(RoomPlmInstructionLists.SpeedBlockRespawning,
            Respawns: true, InitialCrumbleDelay: 1, UseDrawBlockClone: false),
        new(RoomPlmInstructionLists.SpeedBlockDachoraRespawning,
            Respawns: true, InitialCrumbleDelay: 1, UseDrawBlockClone: true),
        new(RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent,
            Respawns: false, InitialCrumbleDelay: 2, UseDrawBlockClone: false),
        new(RoomPlmInstructionLists.SpeedBlockPermanent,
            Respawns: false, InitialCrumbleDelay: 1, UseDrawBlockClone: false),
    ];

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

        foreach (Program program in Programs)
        {
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

    internal static IEnumerable<ushort> MechanicsWordAddresses()
    {
        yield return BombReveal;
        yield return checked((ushort)(BombReveal + 2));
        yield return checked((ushort)(BombReveal + 4));
        foreach (Program program in Programs)
        {
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
        foreach (Program program in Programs)
            yield return checked((ushort)(program.Start + 2));
    }
}
