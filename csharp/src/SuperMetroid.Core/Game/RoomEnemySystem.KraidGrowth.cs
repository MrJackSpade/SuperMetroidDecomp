using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The 875-HP phase boundary where Kraid releases the camera, tears through the ceiling,
/// raises his BG2 body, fades in room palette six, and enables the three lint launchers.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (unchecked((short)(body.Health - state.HealthEighthThreshold(6))) >= 0)
            return false;

        body.VariableA = (ushort)KraidAiFunction.ProcessHeadInstructionAndTimer;
        body.VariableF = 180;
        state.Parts[0].NextFunction = KraidAiFunction.GrowReleaseCamera;

        ushort selectionWord = KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(
            _bus!, body.VariableB);
        KraidHeadResumeDefinition resume =
            KraidHeadInstructionDefinitions.GrowthResume(selectionWord);
        body.VariableB = resume.Pointer;
        body.VariableC = resume.Timer;
        EarthquakeType = 4;
        EarthquakeTimer = 340;

        RoomEnemySlot foot = _slots[5];
        foot.CurrentInstruction = KraidInitialFootInstruction;
        foot.InstructionTimer = 1;
        foot.VariableA = (ushort)KraidAiFunction.NoOperation;
        RoomEnemySlot arm = _slots[1];
        arm.CurrentInstruction = KraidArmNormalInstruction;
        arm.InstructionTimer = 1;
        for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
            _slots[lintSlot].Properties = _slots[lintSlot].Properties.With(EnemyProperties.Invisible);
        arm.Properties = arm.Properties.With(EnemyProperties.IgnoreSamusCollision);
        return true;
    }

    private void RunKraidGrowthFunction(RoomEnemySlot body, KraidEnemyState state)
    {
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.GrowReleaseCamera:
                ApplyKraidScrolls(grown: true);
                body.VariableA = (ushort)KraidAiFunction.GrowBreakCeilingPlatforms;
                state.CameraReleasedForSecondPhase = true;
                state.MinimumYPositionForEjection = 164;
                return;

            case KraidAiFunction.GrowBreakCeilingPlatforms:
                if ((body.FrameCounter & 7) == 0)
                    RequestKraidRisingRock(body, state);
                body.XPosition = unchecked((ushort)(body.XPosition +
                    ((body.YPosition & 2) == 0 ? 1 : -1)));
                body.YPosition = unchecked((ushort)(body.YPosition - 1));
                if ((body.YPosition & 3) == 0 && body.VariableF < 18)
                {
                    int ceilingIndex = body.VariableF / 2;
                    ushort rockX = KraidCeilingRockPositions.AtByteOffset(body.VariableF);
                    if (SpawnKraidCeilingRock(rockX))
                        state.CeilingRockSpawnCount++;
                    _kraidPlmRequests.Add(KraidPlmDefinitions.GrowthCeiling[ceilingIndex]);
                    body.VariableF = unchecked((ushort)(body.VariableF + 2));
                }
                if (unchecked((short)(body.YPosition - 296)) < 0)
                    body.VariableA = (ushort)KraidAiFunction.GrowSetBg2Priority;
                return;

            case KraidAiFunction.GrowSetBg2Priority:
                SetKraidBg2Priority(state);
                _slots[1].Properties = _slots[1].Properties.Without(
                    EnemyProperties.IgnoreSamusCollision);
                body.VariableA = (ushort)KraidAiFunction.GrowFinishBg2Update;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.GrowFinishBg2Update:
                body.VariableA = (ushort)KraidAiFunction.GrowDrawRoomBackground;
                RoomEnemySlot foot = _slots[5];
                foot.CurrentInstruction = KraidFootInstructionProgramDefinitions.Neutral;
                foot.InstructionTimer = 1;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _slots[lintSlot].CurrentInstruction =
                        KraidLintInstructionLists.Ilist_8B04;
                    _slots[lintSlot].SpritemapPointer = 0x8c6c;
                }
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                body.VariableA = (ushort)KraidAiFunction.GrowFadeInRoomBackground;
                body.VariableE = 0;
                body.VariableF = 0;
                // $A7:ADB0 DrawKraidsRoomBackground zeroes the shared numerator.
                GradualColorChange.Numerator = 0;
                // `$A7:AD9A` queues this character upload on the same frame that it
                // initializes palette-six fading. Without it, BG1's authored repeating
                // room-background blocks decode an uninitialized character as transparent,
                // exposing Kraid BG2's `$0338` filler rectangle above his head.
                UploadKraidRoomBackgroundTiles();
                return;

            case KraidAiFunction.GrowFadeInRoomBackground:
                if (!AdvanceKraidRoomBackgroundFade(fadeToBlack: false))
                    return;
                FinishKraidGrowth(body, state);
                return;
            default:
                throw new InvalidOperationException($"Kraid body function $A7:{body.VariableA:X4} is not a growth function.");
        }
    }

    private bool AdvanceKraidRoomBackgroundFade(
        bool fadeToBlack)
    {
        ushort step = GradualColorChange.Numerator;
        if (step <= 13)
        {
            for (int color = 0; color < 16; color++)
            {
                Bgr555 current = _cgram!.Colors[96 + color];
                Bgr555 target = fadeToBlack
                    ? Bgr555.Black
                    : ReadKraidColor(KraidPaletteSource.RoomBackdrop, color);
                _cgram.SetColor(96 + color, TransitionKraidColor(step, current, target));
            }
            GradualColorChange.Numerator = unchecked((ushort)(step + 1));
            return false;
        }
        GradualColorChange.Numerator = 0;
        return true;
    }

    private static Bgr555 TransitionKraidColor(ushort step, Bgr555 current, Bgr555 target) =>
        current.Zip(target, (_, from, to) => TransitionKraidComponent(step, from, to));

    private static int TransitionKraidComponent(ushort step, int current, int target)
    {
        if (step == 0)
            return current;
        if (step == 13)
            return target;
        int denominator = 13 - step;
        int fixedDelta = Math.Abs(target - current) * 256 / denominator;
        int signedDelta = target < current ? -fixedDelta : fixedDelta;
        return (signedDelta + current * 256) >> 8;
    }

    private void FinishKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            int slot = (int)part;
            RoomEnemySlot lint = _slots[slot];
            lint.VariableF = KraidLintInitializationDefinitions.InitialDelay(part);
            lint.VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
            state.Parts[slot].NextFunction = KraidAiFunction.LintProduce;
            lint.VariableB = 0;
        }
        for (int nailIndex = 0; nailIndex < 2; nailIndex++)
        {
            RoomEnemySlot nail = _slots[6 + nailIndex];
            state.Parts[6 + nailIndex].NextFunction = KraidAiFunction.FingernailInitialize;
            nail.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
            nail.VariableF = unchecked((ushort)(64 + nailIndex * 64));
        }
        _slots[1].VariableC = 1;
        body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
        state.TargetX = 288;
        RoomEnemySlot foot = _slots[5];
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkToStart;
        foot.CurrentInstruction = KraidFootWalkBackInstruction;
        foot.InstructionTimer = 1;
    }
}
