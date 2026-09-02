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
                state.LastMaterializationSound = ReadWord(
                    _bus!,
                    0xa7cded + state.MaterializationSoundIndex * 2);
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= 3)
                    state.MaterializationSoundIndex = 0;
                return false;

            case PhantoonInstructionCodes.SetupEyeOpenPhantoonState:
                BeginPhantoonEyeTracking(state);
                return true;

            case PhantoonInstructionCodes.PickNewPhantoonPattern:
                PickPhantoonSecondRoundPattern(state, nmiFrameCounter8);
                return false;

            case PhantoonInstructionCodes.SpawnCasualFlame:
                SpawnPhantoonDestroyableFlame(state.Body, parameter: 0);
                state.LastMaterializationSound = 0x001d;
                return false;

            default:
                throw new InvalidDataException(
                    $"Phantoon instruction callback $A7:{function:X4} is not translated.");
        }
    }

    private void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.Tentacles!.VariableA = 0;
        RoomEnemySlot body = state.Body;
        body.InstructionTimer = 1;
        body.CurrentInstruction = 0xcc4d;
        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        body.VariableE = ReadWord(_bus!, 0xa7cd41 + (_nextRandom!() & 7) * 2);
        body.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
        state.Eye!.InstructionTimer = 1;
        state.Eye.CurrentInstruction = 0xcc9d;
    }

    private void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        RoomEnemySlot body = state.Body;
        RoomEnemySlot eye = state.Eye!;
        body.VariableE = 60;
        eye.VariableA = ReadWord(_bus!, 0xa7cd53 + (_nextRandom!() & 7) * 2);
        if ((nmiFrameCounter8 & 1) != 0)
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
        RequireCompletePhantoonState(stateSlot.EnemyDefinitionPointer == PhantoonBodyDefinition
            ? stateSlot
            : _slots[0]);
}
