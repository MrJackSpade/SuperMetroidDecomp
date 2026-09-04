namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's zero-health sequence from `$A7:C360` through the persisted boss-bit handoff.
/// The body continues to own its private head stream while sinking; lints/nails die at the
/// authored initializer, and the arm/foot remain attached until the BG2 body clears floor.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private void RunKraidDeathFunction(RoomEnemySlot body, KraidEnemyState state)
    {
        switch ((KraidAiFunction)body.VariableA)
        {
            case KraidAiFunction.DeathInitialize:
                InitializeKraidDeath(body, state);
                return;

            case KraidAiFunction.DeathFadeOut:
                _ = ProcessKraidHeadInstruction(body, state);
                if (AdvanceKraidRoomBackgroundFade(state, fadeToBlack: true))
                {
                    body.VariableA = (ushort)KraidAiFunction.DeathUpdateTopTilemap;
                    state.HurtFrameTimer = 1;
                    state.HurtFrame = 1;
                    state.TopTilemapUploadCount++;
                }
                return;

            case KraidAiFunction.DeathUpdateTopTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                body.VariableA = (ushort)KraidAiFunction.DeathUpdateBottomTilemap;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _slots[lintSlot].VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
                    _slots[lintSlot].VariableF = 0x7fff;
                }
                state.TopTilemapUploadCount++;
                return;

            case KraidAiFunction.DeathUpdateBottomTilemap:
                _ = ProcessKraidHeadInstruction(body, state);
                body.VariableA = (ushort)KraidAiFunction.DeathSink;
                state.DeathSoundTimer = 43;
                body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
                EarthquakeType = 1;
                EarthquakeTimer = 256;
                _slots[1].CurrentInstruction = KraidInitialArmInstruction;
                _slots[1].InstructionTimer = 1;
                _slots[5].CurrentInstruction = KraidInitialFootInstruction;
                _slots[5].InstructionTimer = 1;
                _slots[5].VariableA = (ushort)KraidAiFunction.NoOperation;
                state.BottomTilemapUploadCount++;
                return;

            case KraidAiFunction.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidAiFunction.DeathClearTopTilemap:
                body.VariableA = (ushort)KraidAiFunction.DeathClearBottomTilemap;
                state.TopTilemapUploadCount++;
                return;
            case KraidAiFunction.DeathClearBottomTilemap:
                body.VariableA = (ushort)KraidAiFunction.DeathLoadBg3Quarter1;
                state.BottomTilemapUploadCount++;
                return;
            case KraidAiFunction.DeathLoadBg3Quarter1:
                AdvanceKraidDeathBg3Transfer(body, state, KraidAiFunction.DeathLoadBg3Quarter2);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter2:
                AdvanceKraidDeathBg3Transfer(body, state, KraidAiFunction.DeathLoadBg3Quarter3);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter3:
                AdvanceKraidDeathBg3Transfer(body, state, KraidAiFunction.DeathLoadBg3Quarter4);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter4:
                AdvanceKraidDeathBg3Transfer(body, state, KraidAiFunction.DeathFadeInBackground);
                state.RoomBackgroundFadeStep = 0;
                return;
            case KraidAiFunction.DeathFadeInBackground:
                if (!AdvanceKraidRoomBackgroundFade(state, fadeToBlack: false))
                    return;
                state.MusicRequest = MusicCommand.SelectTrack(3);
                if (RequireAreaBossDefeated())
                {
                    body.VariableA = (ushort)KraidAiFunction.DeathFinishedWasDead;
                }
                else
                {
                    RequireSetAreaBossDefeated();
                    state.BossDefeatPersisted = true;
                    body.VariableA = (ushort)KraidAiFunction.DeathFinishedWasAlive;
                }
                state.DeathSequenceComplete = true;
                return;
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
                return;
        }
    }

    private void InitializeKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        if (state.HurtFrame != 0)
            return;
        for (int color = 0; color < 16; color++)
            _cgram!.SetColor(
                112 + color,
                ReadWord(_bus!, EnemyRomTablePointers.Kraid.DeathArmPaletteWords + color * 2));
        _slots[1].CurrentInstruction = KraidArmRetractedInstruction;
        _slots[1].InstructionTimer = 1;
        body.VariableA = (ushort)KraidAiFunction.DeathFadeOut;
        body.VariableB = 0x976c;
        body.VariableC = ReadWord(_bus!, EnemyRomTablePointers.Kraid.DeathInitialTimerWord);
        state.RoomBackgroundFadeStep = 0;
        foreach (int slot in new[] { 2, 3, 4, 6, 7 })
        {
            _slots[slot].Properties = _slots[slot].Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
        }
    }

    private void RunKraidDeathSink(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = ProcessKraidHeadInstruction(body, state);
        state.DeathSoundTimer = unchecked((ushort)(state.DeathSoundTimer - 1));
        if (state.DeathSoundTimer == 0)
        {
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001e));
            state.DeathSoundTimer = 30;
        }
        ProcessKraidSinkTable(body, state);
        body.YPosition = unchecked((ushort)(body.YPosition + 1));
        if (unchecked((short)(body.YPosition - 608)) < 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        ushort armProperties = _slots[1].Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        _slots[1].Properties = armProperties;
        ushort otherProperties = armProperties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        for (int slot = 2; slot <= 5; slot++)
            _slots[slot].Properties = otherProperties;
        body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;

        // $A0:B8EE fills the same sixteen-projectile pool used by ordinary deaths. Each
        // pickup gets a random position in Kraid's 256x64 floor strip and independently
        // rolls header $E2BF's six-byte chance table.
        SpawnEnemyDropScatter(
            KraidDefinition,
            count: 16,
            xBase: 128,
            xMask: 0x00ff,
            yBase: 352,
            yMask: 0x3f00);
        state.DeathDropRequestCount = 16;
        state.RoomBackgroundFadeStep = 0;
    }

    private void ProcessKraidSinkTable(RoomEnemySlot body, KraidEnemyState state)
    {
        for (int offset = 0; offset < 0xa8; offset += 6)
        {
            ushort y = ReadWord(
                _bus!, EnemyRomTablePointers.Kraid.DeathExplosionRecords + offset);
            if ((y & 0x8000) != 0)
                return;
            if (y != body.YPosition)
                continue;
            state.SinkTableEventCount++;
            ushort function = ReadWord(
                _bus!, EnemyRomTablePointers.Kraid.DeathExplosionRecords + 4 + offset);
            // Every eight-pixel sinking row invokes its cartridge callback, including the
            // deliberately empty RTS used by rows whose only job is the BG2 strip upload.
            // Do not merge that address with the adjacent $C6A7 crumble routine: they are
            // distinct legal indirect-JSR targets in the retail table.
            ushort? rockX = function switch
            {
                KraidSinkCallbacks.NoOperation => null,
                KraidSinkCallbacks.CrumbleLeftPlatformLeft => 0x0070,
                KraidSinkCallbacks.CrumbleRightPlatformMiddle => 0x00f0,
                KraidSinkCallbacks.CrumbleRightPlatformLeft => 0x00e0,
                KraidSinkCallbacks.CrumbleLeftPlatformRight => 0x0090,
                KraidSinkCallbacks.CrumbleLeftPlatformMiddle => 0x0080,
                KraidSinkCallbacks.CrumbleRightPlatformRight => 0x0100,
                _ => throw new InvalidDataException(
                    $"Kraid sink table Y=${y:X4} names unknown function $A7:{function:X4}."),
            };
            if (rockX is ushort xPosition)
                _ = SpawnKraidCeilingRock(xPosition);
            return;
        }
    }

    private static void AdvanceKraidDeathBg3Transfer(
        RoomEnemySlot body,
        KraidEnemyState state,
        KraidAiFunction next)
    {
        state.DeathBg3TransferCount++;
        body.VariableA = (ushort)next;
    }
}
