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

    /// <summary>Derived timing and termination facts for one reachable Speed Booster contact program.</summary>
    /// <param name="Start">Native instruction-list address for the PLM program.</param>
    /// <param name="Respawns">Whether the block reappears after its crumble sequence.</param>
    /// <param name="InitialCrumbleDelay">Duration of each of the first three crumble frames.</param>
    /// <param name="UseDrawBlockClone">Whether the respawn terminal uses the Dachora-specific block-clone command.</param>
    private readonly record struct Program(
        ushort Start, bool Respawns, ushort InitialCrumbleDelay,
        bool UseDrawBlockClone)
    {
        /// <summary>Number of timed draw frames before the terminal instruction.</summary>
        internal int FrameCount => Respawns ? 7 : 4;

        /// <summary>Address of the command following the program's timed frame records.</summary>
        internal ushort Terminal => checked((ushort)(Start + 3 + FrameCount * 4));
    }

    /// <summary>Number of reachable Speed Booster collision programs resolved by the mechanics reader.</summary>
    private const int ProgramCount = 5;

    /// <summary>Builds the timing and terminal metadata for a program in the catalog's native address order.</summary>
    /// <param name="index">Zero-based ordinal among the five reachable contact programs.</param>
    /// <returns>The program start, respawn policy, crumble delay, and terminal draw variant.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is outside the catalog.</exception>
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

    /// <summary>Resolves a native control, timer, draw, or terminal word for the bomb reveal and contact programs.</summary>
    /// <param name="address">Bank-relative instruction address to inspect.</param>
    /// <param name="value">Receives the compiled word when the address belongs to one of these programs.</param>
    /// <returns><see langword="true"/> when a compiled mechanics word owns the address.</returns>
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

    /// <summary>Resolves the library-two sound operand stored after a contact program's initial opcode.</summary>
    /// <param name="address">Bank-relative byte address to inspect.</param>
    /// <param name="value">Receives the compiled sound selector when the address matches a program operand.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics byte.</returns>
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
