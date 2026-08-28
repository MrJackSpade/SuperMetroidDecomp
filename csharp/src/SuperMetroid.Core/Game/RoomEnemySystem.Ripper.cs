using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal translation of enemy $D47F (Ripper) from bank $A2. This is intentionally kept
/// separate from the room scheduler: the same common speed table and terrain mover are used
/// by many later enemy families, while these list pointers and reversal rules belong only to
/// the Ripper actor.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort RipperDefinition = 0xd47f;

    private const ushort RipperMovingRightInstruction = 0xe477;
    private const ushort RipperMovingLeftInstruction = 0xe48b;
    private const int CommonLinearEnemySpeedTable = 0xa28187;

    /// <summary>Ports <c>Ripper_Init</c> at $A2:E49F.</summary>
    private void InitializeRipper(RoomEnemySlot slot)
    {
        // Population parameter two is the initial facing/direction selector. A nonzero word
        // chooses the positive table pair and the right-facing list; zero chooses the
        // negative pair and left-facing list.
        bool movingRight = slot.Parameter2 != 0;
        slot.CurrentInstruction = movingRight
            ? RipperMovingRightInstruction
            : RipperMovingLeftInstruction;

        // Each logical speed occupies eight bytes in the common table:
        //   +0 signed pixel velocity, +2 subpixel velocity,
        //   +4 negated pixel velocity, +6 negated subpixel velocity.
        // Variable E retains the byte offset because the reversal routine reuses it.
        slot.VariableE = unchecked((ushort)(slot.Parameter1 * 8));
        LoadRipperVelocity(slot, movingRight);
    }

    /// <summary>Ports <c>Ripper_Main</c> at $A2:E4DA.</summary>
    private void RunRipperMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Ripper movement requires the active room collision allocation.");
        }

        // $A0:C6AB consumes a signed 16.16 displacement whose high and low words are kept
        // in Variables D/C. On collision the native helper aligns the actor flush with the
        // wall before Ripper swaps direction and animation.
        int displacement = unchecked(((short)slot.VariableD << 16) | slot.VariableC);
        if (!MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        bool wasMovingLeft = (short)slot.VariableD < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(slot, nowMovingRight);
        slot.CurrentInstruction = nowMovingRight
            ? RipperMovingRightInstruction
            : RipperMovingLeftInstruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private void LoadRipperVelocity(RoomEnemySlot slot, bool movingRight)
    {
        int address = CommonLinearEnemySpeedTable + slot.VariableE + (movingRight ? 0 : 4);
        slot.VariableD = ReadWord(_bus!, address);
        slot.VariableC = ReadWord(_bus!, address + 2);
    }
}
