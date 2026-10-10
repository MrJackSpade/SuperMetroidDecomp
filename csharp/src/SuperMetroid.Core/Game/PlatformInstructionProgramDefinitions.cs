namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Tripper and Kamer's moving and vertically-still loops.
/// Their thirty-two interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class PlatformInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Left_0</c> at $A3:9BBB.</summary>
    internal const ushort KamerMovingLeft = 0x9bbb;
    /// <summary><c>InstList_Kamer2_VerticallyMoving_Right_0</c> at $A3:9BD1.</summary>
    internal const ushort KamerMovingRight = 0x9bd1;
    /// <summary><c>InstList_Kamer2_VerticallyStill_Left_0</c> at $A3:9BE7.</summary>
    internal const ushort KamerStillLeft = 0x9be7;
    /// <summary><c>InstList_Kamer2_VerticallyStill_Right_0</c> at $A3:9BFD.</summary>
    internal const ushort KamerStillRight = 0x9bfd;
    /// <summary><c>InstList_Tripper_VerticallyMoving_Left_0</c> at $A3:9C13.</summary>
    internal const ushort TripperMovingLeft = 0x9c13;
    /// <summary><c>InstList_Tripper_VerticallyMoving_Right_0</c> at $A3:9C29.</summary>
    internal const ushort TripperMovingRight = 0x9c29;
    /// <summary>
    /// <c>InstList_Tripper_VerticallyStill_Right_0</c> at $A3:9C3F. The native art label
    /// is opposite its published leftward horizontal movement, so this mechanics-facing
    /// name follows the callback effect used by the translated AI.
    /// </summary>
    internal const ushort TripperStillMovingLeft = 0x9c3f;
    /// <summary>
    /// <c>InstList_Tripper_VerticallyStill_Left_0</c> at $A3:9C55. The native art label
    /// is opposite its published rightward horizontal movement.
    /// </summary>
    internal const ushort TripperStillMovingRight = 0x9c55;

    /// <summary>Program choice at $A3:9E47-9EBA, called by the zero/nonzero
    /// direction branches at $A3:9ECB and $A3:9EF1. Initialization uses the still cases.</summary>
    internal static ushort SelectProgram(bool isKamer, bool verticallyMoving, PlatformHorizontalMovement direction) =>
        (isKamer, verticallyMoving, direction == PlatformHorizontalMovement.Left) switch
        {
            (true, true, true) => KamerMovingLeft,
            (true, true, false) => KamerMovingRight,
            (true, false, true) => KamerStillLeft,
            (true, false, false) => KamerStillRight,
            (false, true, true) => TripperMovingLeft,
            (false, true, false) => TripperMovingRight,
            (false, false, true) => TripperStillMovingLeft,
            (false, false, false) => TripperStillMovingRight,
        };
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - KamerMovingLeft;
        int local = offset % 22;
        return (uint)offset < 176 && local >= 4 && local <= 16 && local % 4 == 0;
    }

    /// <summary>Direction callback, four timed frames, then goto the first timed frame.
    /// Moving and still states have distinct native callbacks; the callback is not repeated by the loop.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - KamerMovingLeft;
        if ((uint)offset < 176)
        {
            int program = offset / 22;
            int local = offset % 22;
            if (local == 0)
            {
                bool right = (program & 1) != 0;
                bool moving = (program & 2) == 0;
                return (moving, right) switch
                {
                    (true, false) => (ushort)PlatformInstruction.SetMovingLeftXMovementDuplicate,
                    (true, true) => (ushort)PlatformInstruction.SetMovingRightXMovementDuplicate,
                    (false, false) => (ushort)PlatformInstruction.SetMovingLeftXMovement,
                    (false, true) => (ushort)PlatformInstruction.SetMovingRightXMovement,
                };
            }
            if (local >= 2 && local <= 14 && local % 4 == 2)
                return (ushort)(program < 4 ? 10 : 7 + ((local - 2) / 4) % 2);
            if (local == 18) return (ushort)CommonEnemyInstruction.Goto;
            if (local == 20) return (ushort)(address - 18);
        }
        throw new InvalidDataException(
            $"Tripper/Kamer instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
