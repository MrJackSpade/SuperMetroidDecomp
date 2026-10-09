namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon callbacks reached through common enemy opcode <c>$A0:808A</c>. The opcode's
/// operand is a same-bank function pointer, so dispatching by the literal word preserves
/// the ROM list as the owner of timing and animation order.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <returns>True when the callback installed another instruction list and interpretation must stop.</returns>
    private bool ProcessPhantoonInstructionFunction(
        RoomEnemySlot part,
        ushort function,
        byte nmiFrameCounter8)
    {
        PhantoonEnemyState state = RequireCompletePhantoonStateForPart(part);
        switch (function)
        {
            case PhantoonInstructionCodes.PlayPhantoonMaterializationSFX:
                state.LastMaterializationSound =
                    PhantoonSoundDefinitions.MaterializationSound(
                        state.MaterializationSoundIndex);
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= 3)
                    state.MaterializationSoundIndex = 0;
                return false;

            case PhantoonInstructionCodes.SetupEyeOpenPhantoonState:
                BeginPhantoonEyeTracking(state);
                // A0:808A resumes its local instruction cursor after the callback.
                // The following Sleep overwrites this eye's callback-installed list;
                // the body-owned list change still survives on its separate actor.
                return false;

            case PhantoonInstructionCodes.PickNewPhantoonPattern:
                PickPhantoonSecondRoundPattern(state, nmiFrameCounter8);
                return false;

            case PhantoonInstructionCodes.SpawnCasualFlame:
                SpawnPhantoonDestroyableFlame(state.Body, parameter: 0);
                QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlameQueueCapacity);
                return false;

            default:
                throw new InvalidDataException(
                    $"Phantoon instruction callback $A7:{function:X4} is not translated.");
        }
    }

    /// <summary>Opens Phantoon's eye, starts a randomized vulnerable window, and centers the eye for tracking.</summary>
    /// <param name="state">Complete encounter state whose body, eye, and tentacle slots receive the tracking setup.</param>
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

    /// <summary>Chooses the alternating second-round body pattern and refreshes the eye-closed and attack timers.</summary>
    /// <param name="state">Encounter state whose body and eye variables determine and receive the next pattern.</param>
    /// <param name="nmiFrameCounter8">Eight-bit NMI frame value supplied by the callback; pattern parity is sampled from the room's current enemy-frame counter.</param>
    private void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
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

    /// <summary>Resolves the full four-part encounter state from either the body slot or one of its companion slots.</summary>
    /// <param name="stateSlot">Body or companion slot whose shared Phantoon state is needed.</param>
    /// <returns>The encounter state rooted at the body slot.</returns>
    /// <exception cref="InvalidOperationException">The eye, tentacle, or mouth slot has not been initialized.</exception>
    private PhantoonEnemyState RequireCompletePhantoonStateForPart(RoomEnemySlot stateSlot) =>
        RequireCompletePhantoonState(stateSlot.EnemyDefinitionPointer == PhantoonBodyDefinition
            ? stateSlot
            : _slots[0]);
}
