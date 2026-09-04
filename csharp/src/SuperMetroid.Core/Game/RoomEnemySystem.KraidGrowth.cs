namespace SuperMetroid.Core.Game;

/// <summary>
/// The 875-HP phase boundary where Kraid releases the camera, tears through the ceiling,
/// raises his BG2 body, fades in room palette six, and enables the three lint launchers.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (unchecked((short)(body.Health - state.HealthEighthThresholds[6])) >= 0)
            return false;

        body.VariableA = (ushort)KraidAiFunction.ProcessHeadInstructionAndTimer;
        body.VariableF = 180;
        state.Parts[0].NextFunction = KraidAiFunction.GrowReleaseCamera;

        ushort nextTilemap = ReadWord(
            _bus!, 0xa70000 | unchecked((ushort)(body.VariableB + 2)));
        int byteSelector = nextTilemap switch
        {
            0x97c8 => 50,
            0x9ac8 => 42,
            0x9dc8 => 34,
            _ => 26,
        };
        body.VariableB = unchecked((ushort)(byteSelector - 0x6926));
        body.VariableC = ReadWord(
            _bus!, EnemyRomTablePointers.Kraid.InitialTimerWords + byteSelector);
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
                    ushort rockX = ReadWord(
                        _bus!, EnemyRomTablePointers.Kraid.CeilingRockXWords + body.VariableF);
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
                foot.CurrentInstruction = KraidInstructionLists.Ilist_86ED;
                foot.InstructionTimer = 1;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _slots[lintSlot].CurrentInstruction = KraidInstructionLists.Ilist_8B04;
                    _slots[lintSlot].SpritemapPointer = 0x8c6c;
                }
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                body.VariableA = (ushort)KraidAiFunction.GrowFadeInRoomBackground;
                body.VariableE = 0;
                body.VariableF = 0;
                state.RoomBackgroundFadeStep = 0;
                // `$A7:AD9A` queues this character upload on the same frame that it
                // initializes palette-six fading. Without it, BG1's authored repeating
                // room-background blocks decode an uninitialized character as transparent,
                // exposing Kraid BG2's `$0338` filler rectangle above his head.
                _vram!.ExecuteQueuedWrite(
                    _bus!,
                    KraidBackgroundRomData.RoomBackgroundTileAddress,
                    KraidBackgroundRomData.RoomBackgroundTileBytes,
                    KraidBackgroundRomData.RoomBackgroundTileVramWord);
                return;

            case KraidAiFunction.GrowFadeInRoomBackground:
                if (!AdvanceKraidRoomBackgroundFade(state, fadeToBlack: false))
                    return;
                FinishKraidGrowth(body, state);
                return;
        }
    }

    private bool AdvanceKraidRoomBackgroundFade(
        KraidEnemyState state,
        bool fadeToBlack)
    {
        ushort step = state.RoomBackgroundFadeStep;
        if (step <= 13)
        {
            for (int color = 0; color < 16; color++)
            {
                ushort current = _cgram!.Colors[96 + color];
                ushort target = fadeToBlack
                    ? (ushort)0
                    : ReadWord(
                        _bus!, EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords + color * 2);
                _cgram.SetColor(96 + color, TransitionKraidColor(step, current, target));
            }
            state.RoomBackgroundFadeStep = unchecked((ushort)(step + 1));
            return false;
        }
        state.RoomBackgroundFadeStep = 0;
        return true;
    }

    private static ushort TransitionKraidColor(ushort step, ushort current, ushort target)
    {
        int red = TransitionKraidComponent(step, current & 31, target & 31);
        int green = TransitionKraidComponent(step, (current >> 5) & 31, (target >> 5) & 31);
        int blue = TransitionKraidComponent(step, (current >> 10) & 31, (target >> 10) & 31);
        return unchecked((ushort)(red | (green << 5) | (blue << 10)));
    }

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
        ushort[] lintTimers = [0x0120, 0x00a0, 0x0040];
        for (int index = 0; index < lintTimers.Length; index++)
        {
            RoomEnemySlot lint = _slots[index + 2];
            lint.VariableF = lintTimers[index];
            lint.VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
            state.Parts[index + 2].NextFunction = KraidAiFunction.LintProduce;
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
        body.VariableB = 0x96da;
        state.TargetX = 288;
        RoomEnemySlot foot = _slots[5];
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkToStart;
        foot.CurrentInstruction = KraidFootWalkBackInstruction;
        foot.InstructionTimer = 1;
    }
}
