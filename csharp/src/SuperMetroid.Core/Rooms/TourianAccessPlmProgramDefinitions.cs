using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded bank-$84 instruction data for the two access-floor PLMs spawned when
/// the Tourian entrance statue changes state. The adjacent $AB00 routine is
/// executable code, not part of either instruction stream.
/// </summary>
internal static class TourianAccessPlmProgramDefinitions
{
    /// <summary><c>$84:AAE5</c>: six-row crumble loop.</summary>
    internal const ushort Crumble = TourianAccessPlmDefinitions.CrumbleInstructionList;
    /// <summary><c>$84:AB0C</c>: one-pass six-row clear.</summary>
    internal const ushort Clear = TourianAccessPlmDefinitions.ClearInstructionList;
    /// <summary>Native eight-bit timer value for the six crumble rows.</summary>
    internal const byte CrumbleRows = 6;
    /// <summary>Each of the four crumble frames holds for four PLM passes.</summary>
    internal const ushort CrumbleFrameDuration = 4;

    /// <summary>The fixed control words of both streams outside the crumble frames, valued by address.</summary>
    private enum ControlWord : ushort
    {
        /// <summary><c>$84:AAE5</c>: the crumble list's eight-bit timer instruction.</summary>
        CrumbleSetTimer = Crumble,
        /// <summary>The crumble loop's move-down instruction after the four frames.</summary>
        CrumbleMoveDown = Crumble + 19,
        /// <summary>The crumble loop's decrement-and-branch instruction.</summary>
        CrumbleDecrementAndBranch = Crumble + 21,
        /// <summary>The decrement-and-branch operand naming the first crumble frame.</summary>
        CrumbleLoopTarget = Crumble + 23,
        /// <summary>The crumble list's terminal delete.</summary>
        CrumbleDelete = Crumble + 25,
        /// <summary><c>$84:AB0C</c>: the clear list's one-pass duration.</summary>
        ClearDuration = Clear,
        /// <summary>The clear list's draw operand.</summary>
        ClearDraw = Clear + 2,
        /// <summary>The clear list's terminal delete.</summary>
        ClearDelete = Clear + 4,
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (Enum.IsDefined((ControlWord)address))
        {
            value = (ControlWord)address switch
            {
                ControlWord.CrumbleSetTimer => (ushort)RoomPlmInstruction.SetEightBitTimer,
                ControlWord.CrumbleMoveDown => (ushort)RoomPlmInstruction.MoveTourianAccessDown,
                ControlWord.CrumbleDecrementAndBranch => (ushort)RoomPlmInstruction.DecrementTimerAndGoto,
                ControlWord.CrumbleLoopTarget => checked((ushort)(Crumble + 3)),
                ControlWord.CrumbleDelete or ControlWord.ClearDelete => (ushort)RoomPlmInstruction.Delete,
                ControlWord.ClearDuration => 1,
                ControlWord.ClearDraw => (ushort)TourianAccessDraw.Clear,
                _ => throw new InvalidOperationException($"Undefined {nameof(ControlWord)} {(int)(ControlWord)address}."),
            };
            return true;
        }
        if (address is >= (Crumble + 3) and < (Crumble + 19))
        {
            int offset = address - Crumble - 3;
            if (offset % 4 == 0)
            {
                value = CrumbleFrameDuration;
                return true;
            }
            if (offset % 4 == 2)
            {
                value = TourianAccessPlmDrawDefinitions.CrumbleFramePointer(offset / 4);
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == Crumble + 2)
        {
            value = CrumbleRows;
            return true;
        }
        value = 0;
        return false;
    }
}
