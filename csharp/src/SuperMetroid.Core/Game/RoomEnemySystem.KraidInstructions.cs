using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid-specific commands embedded in the arm and foot's ordinary bank-$A7 instruction
/// lists. They move the shared body, so treating them as cosmetic frame markers would make
/// the foot animate in place while collision and BG2 remain stationary.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Selects Kraid's slow arm program when the body is below half health and the arm has
    /// not yet entered it; otherwise advances past the native callback instruction.
    /// </summary>
    /// <param name="cursor">Instruction cursor immediately after the arm's speed-selection callback.</param>
    /// <returns>The slow program entry when selected, or the next instruction address.</returns>
    private ushort SelectKraidArmSpeedInstruction(ushort cursor)
    {
        RoomEnemySlot body = _slots[0];
        RoomEnemySlot arm = _slots[1];
        ushort halfHealth = RequireKraidState(body).HealthEighthThreshold(3);
        if (unchecked((short)(body.Health - halfHealth)) < 0 &&
            unchecked((short)(arm.CurrentInstruction -
                KraidArmInstructionProgramDefinitions.Slow)) < 0)
        {
            return KraidArmInstructionProgramDefinitions.Slow;
        }
        return unchecked((ushort)(cursor + 2));
    }

    /// <summary>
    /// Applies a native foot-program command to Kraid's shared body, including position changes,
    /// earthquake and sound requests, and the collision-aware horizontal move callback.
    /// </summary>
    /// <param name="instruction">Kraid foot callback opcode being executed.</param>
    /// <param name="level">Room collision data required by the move-right callback; other commands do not use it.</param>
    /// <exception cref="InvalidOperationException">The move-right callback runs without room collision data.</exception>
    /// <exception cref="InvalidDataException">The opcode is not a translated Kraid foot instruction.</exception>
    private void ProcessKraidFootInstruction(ushort instruction, RoomLevelData? level)
    {
        RoomEnemySlot body = _slots[0];
        switch (instruction)
        {
            case KraidInstructionCodes.Instruction_Kraid_NOP_A7B633:
                return;
            case KraidInstructionCodes.Instruction_Kraid_DecrementYPosition:
                body.YPosition = unchecked((ushort)(body.YPosition - 1));
                return;
            case KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking:
                body.YPosition = unchecked((ushort)(body.YPosition + 1));
                EarthquakeType = 1;
                EarthquakeTimer = 10;
                return;
            case KraidInstructionCodes.Instruction_Kraid_QueueSFX76_Lib2_Max6:
                LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0076));
                return;
            case KraidInstructionCodes.Instruction_Kraid_XPositionMinus3:
            case KraidInstructionCodes.Instruction_Kraid_XPositionMinus3_duplicate:
                body.XPosition = unchecked((ushort)(body.XPosition - 3));
                return;
            case KraidInstructionCodes.Instruction_Kraid_XPositionPlus3:
                body.XPosition = unchecked((ushort)(body.XPosition + 3));
                return;
            case KraidInstructionCodes.UNUSED_Instruction_Kraid_MoveRight_A7B683:
                if (level is null)
                    throw new InvalidOperationException(
                        "Kraid's move-right foot instruction requires room collision data.");
                KraidEnemyState state = RequireKraidState(body);
                bool shouldMove = unchecked((short)(body.XPosition - 320)) < 0;
                if (!shouldMove)
                {
                    state.TargetX = unchecked((ushort)(state.TargetX - 1));
                    shouldMove = state.TargetX == 0;
                }
                if (shouldMove && MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level, body, 4 << 16))
                {
                    EarthquakeType = 0;
                    EarthquakeTimer = 7;
                    _slots[5].XPosition = body.XPosition;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid foot instruction $A7:{instruction:X4} is not translated.");
        }
    }
}
