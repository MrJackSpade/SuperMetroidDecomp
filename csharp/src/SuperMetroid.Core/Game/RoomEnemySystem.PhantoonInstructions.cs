namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon callbacks reached through common enemy opcode <c>$A0:808A</c>. The opcode's
/// operand is a same-bank function pointer, so dispatching by the literal word preserves
/// the ROM list as the owner of timing and animation order.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Executes the instructions Phantoon's parts own beyond the common set. A callback that
    /// installs another list stops the interpreter for this frame without advancing the cursor.
    /// </summary>
    private bool TryProcessPhantoonPartInstruction(
        RoomEnemySlot slot,
        ushort word,
        ref ushort cursor,
        out bool yieldInterpreter)
    {
        yieldInterpreter = false;
        if (!IsPhantoonPartDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((PhantoonPartInstruction)word))
            return false;

        switch ((PhantoonPartInstruction)word)
        {
            case PhantoonPartInstruction.CallFunctionInY:
                ushort function = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                if (ProcessPhantoonInstructionFunction(slot, function))
                {
                    yieldInterpreter = true;
                    return true;
                }
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Phantoon does not own instruction ${word:X4}.");
        }
    }

    /// <returns>True when the callback installed another instruction list and interpretation must stop.</returns>
    private bool ProcessPhantoonInstructionFunction(
        RoomEnemySlot part,
        ushort function)
    {
        PhantoonEnemyState state = RequireCompletePhantoonStateForPart(part);
        switch (ClosedNativeWords.Decode<PhantoonInstruction>(function, "Phantoon instruction callback"))
        {
            case PhantoonInstruction.PlayPhantoonMaterializationSFX:
                state.LastMaterializationSound =
                    PhantoonSoundDefinitions.MaterializationSound(
                        state.MaterializationSoundIndex);
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= 3)
                    state.MaterializationSoundIndex = 0;
                return false;

            case PhantoonInstruction.SetupEyeOpenPhantoonState:
                BeginPhantoonEyeTracking(state);
                // A0:808A resumes its local instruction cursor after the callback.
                // The following Sleep overwrites this eye's callback-installed list;
                // the body-owned list change still survives on its separate actor.
                return false;

            case PhantoonInstruction.PickNewPhantoonPattern:
                PickPhantoonSecondRoundPattern(state);
                return false;

            case PhantoonInstruction.SpawnCasualFlame:
                SpawnPhantoonDestroyableFlame(state.Body, PhantoonFlameSpawnType.Casual, 0);
                QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlameQueueCapacity);
                return false;

            default:
                throw new InvalidOperationException($"Undefined {nameof(PhantoonInstruction)} {function:X4}.");
        }
    }

    private void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.Tentacles!.VariableA = 0;
        RoomEnemySlot body = state.Body;
        body.InstructionTimer = 1;
        body.CurrentInstruction = PhantoonInstructionProgramDefinitions.EyeHitboxBody;
        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        body.VariableE = PhantoonTimerDefinitions.VulnerableWindow[_nextRandom!() & 7];
        body.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
        state.Eye!.InstructionTimer = 1;
        state.Eye.CurrentInstruction = PhantoonInstructionProgramDefinitions.EyeballCentered;
    }

    private void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        RoomEnemySlot eye = state.Eye!;
        body.VariableE = 60;
        eye.VariableA = PhantoonTimerDefinitions.EyeClosed[_nextRandom!() & 7];
        if ((_enemyFrameNmiFrameCounter & 1) != 0)
        {
            if (eye.VariableC == 0)
                body.VariableA = body.VariableA == 0 ? (ushort)533 : unchecked((ushort)(body.VariableA - 1));
            body.VariableC = 0;
            body.VariableB = 0;
            body.VariableD = 0;
            eye.VariableC = 1;
        }
        else
        {
            if (eye.VariableC != 0)
                body.VariableA = body.VariableA >= 533 ? (ushort)0 : unchecked((ushort)(body.VariableA + 1));
            body.VariableC = 1;
            body.VariableB = 0;
            body.VariableD = 0;
            eye.VariableC = 0;
        }

        body.VariableF = body.Parameter2 != 0
            ? (ushort)PhantoonAiFunction.FadeOutBeforeFirstFlameRain
            : (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        if (body.Parameter2 != 0)
            eye.VariableF = 0;
    }

    private PhantoonEnemyState RequireCompletePhantoonStateForPart(RoomEnemySlot stateSlot) =>
        RequireCompletePhantoonState(stateSlot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody
            ? stateSlot
            : _slots[0]);
}
