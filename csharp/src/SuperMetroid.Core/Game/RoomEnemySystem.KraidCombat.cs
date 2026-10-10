using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's first-phase body thinker and private eight-byte BG2 head instruction stream.
/// This is separate from ordinary enemy bytecode: each entry carries a timer, a 704-byte
/// head tilemap, a vulnerable-mouth hitbox, and an invulnerable-mouth hitbox.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private void RunKraidCombatFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        VramWriteQueue? vramWriteQueue,
        ushort cameraX)
    {
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.MainloopThinking:
                if (state.ThinkingTimer != 0 && --state.ThinkingTimer == 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.MainAttackWithMouthOpen;
                    body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
                    body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
                }
                return;

            case KraidAiFunction.MainAttackWithMouthOpen:
                RunKraidMouthOpenAttack(body, state);
                return;

            case KraidAiFunction.MouthOpenReaction:
                RunKraidMouthOpenReaction(body, state);
                return;

            case KraidAiFunction.InitializeEyeGlow:
                InitializeKraidEyeGlow(body, state);
                return;

            case KraidAiFunction.GlowEye:
                GlowKraidEye(body, state);
                return;

            case KraidAiFunction.UnglowEye:
                UnglowKraidEye(body, state);
                return;

            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(body, state.Parts[0]);
                return;

            case KraidAiFunction.ProcessHeadInstructionAndTimer:
                _ = ProcessKraidHeadInstruction(body, state);
                TickKraidFunctionTimer(body, state.Parts[0]);
                return;

            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowSetBg2Priority:
            case KraidAiFunction.GrowFinishBg2Update:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
                RunKraidGrowthFunction(body, state);
                return;

            case KraidAiFunction.SecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, state);
                return;

            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathUpdateTopTilemap:
            case KraidAiFunction.DeathUpdateBottomTilemap:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathClearTopTilemap:
            case KraidAiFunction.DeathClearBottomTilemap:
            case KraidAiFunction.DeathLoadBg3Quarter1:
            case KraidAiFunction.DeathLoadBg3Quarter2:
            case KraidAiFunction.DeathLoadBg3Quarter3:
            case KraidAiFunction.DeathLoadBg3Quarter4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
                RunKraidDeathFunction(body, state, vramWriteQueue, cameraX);
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid body function $A7:{body.VariableA:X4} is not translated.");
        }
    }

    private void SetupKraidFirstPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
        _slots[2].YPosition = unchecked((ushort)(body.YPosition - 20));
        _slots[3].YPosition = unchecked((ushort)(body.YPosition + 46));
        _slots[4].YPosition = unchecked((ushort)(body.YPosition + 112));
        state.ThinkingTimer = ReadKraidThinkingTimer();
    }

    private void SetupKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
        _slots[2].YPosition = unchecked((ushort)(body.YPosition - 20));
        _slots[3].YPosition = unchecked((ushort)(body.YPosition + 46));
        _slots[4].YPosition = unchecked((ushort)(body.YPosition + 112));
        state.ThinkingTimer = ReadKraidThinkingTimer();
    }

    private static void RunKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        if (state.ThinkingTimer != 0 && --state.ThinkingTimer == 0)
        {
            body.VariableA = (ushort)KraidAiFunction.MouthOpenReaction;
            body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
            body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
        }
    }

    private void RunKraidMouthOpenAttack(RoomEnemySlot body, KraidEnemyState state)
    {
        ushort result = ProcessKraidHeadInstruction(body, state);
        if (result == ushort.MaxValue)
        {
            SetupKraidFirstPhaseThinking(body, state);
            body.VariableC = 90;

            // Bits 2 and 8-15 are populated by body/mouth projectile handling. Preserve
            // the cartridge arithmetic now so adding collision cannot change sequencing.
            if ((state.MouthFlags & 4) != 0)
            {
                state.MouthFlags = unchecked((ushort)(state.MouthFlags - 0x0100));
                if ((state.MouthFlags & 0xff00) != 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
                    body.VariableF = 64;
                    // `$B6BF` (eye glow initialization) is the next collision-driven
                    // function; it remains deliberately unnamed until that slice lands.
                    state.Parts[0].NextFunction = (KraidAiFunction)0xb6bf;
                    state.Unknown2 = 2;
                    return;
                }
            }
            state.MouthFlags = 0;
            return;
        }

        // The open-mouth tilemap remains installed for 64 frames. Retail emits a rock on
        // every sixteenth timer value, including the entry frame after `$AF3D` reloads it.
        if (state.CurrentHeadTilemap == KraidHeadInstructionDefinitions.OpenMouthTilemap &&
            (body.VariableC & 0x000f) == 0)
        {
            state.SpitRockRequestCount++;
            if (SpawnKraidSpitRock(body))
                state.SpawnedSpitRockCount++;
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
        }
    }

    private ushort ProcessKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        ushort result = body.VariableC;
        if (result == 0)
            return 0;
        body.VariableC = unchecked((ushort)(body.VariableC - 1));
        return result == 1 ? ExecuteKraidHeadInstruction(body, state) : result;
    }

    private ushort ExecuteKraidHeadInstruction(RoomEnemySlot body, KraidEnemyState state)
    {
        for (int commandCount = 0; commandCount < 16; commandCount++)
        {
            ushort cursor = body.VariableB;
            KraidHeadInstructionDefinition instruction =
                KraidHeadInstructionDefinitions.Resolve(cursor);
            switch (instruction.Kind)
            {
                case KraidHeadInstructionKind.Terminate:
                    return ushort.MaxValue;

                case KraidHeadInstructionKind.RoarSound:
                case KraidHeadInstructionKind.DyingSound:
                    LastKraidSoundEffect = new KraidSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            instruction.SoundId));
                    if (instruction.Kind == KraidHeadInstructionKind.RoarSound)
                        state.RoarRequestCount++;
                    body.VariableB = unchecked((ushort)(cursor + 2));
                    continue;

                case KraidHeadInstructionKind.Frame:
                    body.VariableC = instruction.Duration;
                    state.CurrentHeadTilemap = instruction.Tilemap;
                    state.VulnerableMouthHitbox = instruction.VulnerableHitbox;
                    state.InvulnerableMouthHitbox = instruction.InvulnerableHitbox;
                    body.VariableB = unchecked((ushort)(cursor + 8));
                    TransferKraidHeadTilemap(state, state.CurrentHeadTilemap);
                    return 1;

                default:
                    throw new InvalidDataException(
                        $"Kraid head instruction kind {instruction.Kind} is not translated.");
            }
        }
        throw new InvalidDataException("Kraid head instruction stream exceeded its command guard.");
    }

    private void RunKraidMouthOpenReaction(RoomEnemySlot body, KraidEnemyState state)
    {
        if (ProcessKraidHeadInstruction(body, state) != ushort.MaxValue)
            return;
        body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
        body.VariableC = 90;
        if ((state.MouthFlags & 4) != 0)
        {
            state.MouthFlags = unchecked((ushort)(state.MouthFlags - 0x0100));
            if ((state.MouthFlags & 0xff00) != 0)
            {
                body.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
                body.VariableF = 64;
                state.Parts[0].NextFunction = KraidAiFunction.InitializeEyeGlow;
                state.Unknown2 = 2;
                return;
            }
        }
        state.MouthFlags = 0;
    }

    private void InitializeKraidEyeGlow(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.GlowEye;
        body.VariableB = KraidHeadInstructionDefinitions.EyeGlowContinuation;
        body.VariableC = KraidHeadInstructionDefinitions.EyeGlowEntryTimer;
        GlowKraidEye(body, state);
    }

    private void GlowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        int completedChannels = 0;
        for (int colorIndex = 113; colorIndex < 116; colorIndex++)
        {
            Bgr555 color = _cgram!.Colors[colorIndex];
            int red = color.Red;
            int green = color.Green;
            if (red >= 30)
            {
                red = 31;
                completedChannels++;
            }
            else
            {
                red++;
            }
            if (green >= 30)
            {
                green = 31;
                completedChannels++;
            }
            else
            {
                green++;
            }
            _cgram.SetColor(colorIndex, new Bgr555(red, green, color.Blue));
        }
        if (completedChannels >= 6)
            body.VariableA = (ushort)KraidAiFunction.UnglowEye;
    }

    private void UnglowKraidEye(RoomEnemySlot body, KraidEnemyState state)
    {
        int thresholdWordOffset = 14;
        while (thresholdWordOffset != 0 &&
            unchecked((short)(
                body.Health - state.HealthEighthThreshold(thresholdWordOffset / 2))) < 0)
        {
            thresholdWordOffset -= 2;
        }
        int sourceColor = 8 * (thresholdWordOffset + 2) + 1;
        int changedChannels = 0;
        for (int eye = 0; eye < 3; eye++)
        {
            int colorIndex = 113 + eye;
            Bgr555 current = _cgram!.Colors[colorIndex];
            Bgr555 target = ReadKraidColor(KraidPaletteSource.Health, sourceColor + eye);
            int red = current.Red;
            int green = current.Green;
            // Native decrements toward any differing target and masks each field, so a
            // channel below its target wraps within its five bits without borrowing.
            if (red != target.Red)
            {
                red = (red - 1) & Bgr555.MaxChannel;
                changedChannels++;
            }
            if (green != target.Green)
            {
                green = (green - 1) & Bgr555.MaxChannel;
                changedChannels++;
            }
            _cgram.SetColor(colorIndex, new Bgr555(red, green, current.Blue));
        }
        if (changedChannels == 0)
        {
            body.VariableA = (ushort)KraidAiFunction.MouthOpenReaction;
            body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
            body.VariableC = KraidHeadInstructionDefinitions.RoarEntryTimer;
        }
    }
}
