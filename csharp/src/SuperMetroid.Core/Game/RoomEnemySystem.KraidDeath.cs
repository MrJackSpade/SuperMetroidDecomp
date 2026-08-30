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
                body.Properties = unchecked((ushort)(body.Properties | 0x8000));
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
                state.MusicRequest = 3;
                if (_isAreaBossDefeated?.Invoke() ?? false)
                {
                    body.VariableA = (ushort)KraidAiFunction.DeathFinishedWasDead;
                }
                else
                {
                    _setAreaBossDefeated?.Invoke();
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
            _cgram!.SetColor(112 + color, ReadWord(_bus!, 0xa7b4f3 + color * 2));
        _slots[1].CurrentInstruction = KraidArmRetractedInstruction;
        _slots[1].InstructionTimer = 1;
        body.VariableA = (ushort)KraidAiFunction.DeathFadeOut;
        body.VariableB = 0x976c;
        body.VariableC = ReadWord(_bus!, 0xa79764);
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
            LastKraidSoundEffect = new KraidSoundRequest(3, 0x001e);
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
        ushort otherProperties = unchecked((ushort)((armProperties & 0x51ff) | 0x0600));
        for (int slot = 2; slot <= 5; slot++)
            _slots[slot].Properties = otherProperties;
        body.VariableA = (ushort)KraidAiFunction.DeathClearTopTilemap;
        state.DeathDropRequestCount = 16;
        state.RoomBackgroundFadeStep = 0;
    }

    private void ProcessKraidSinkTable(RoomEnemySlot body, KraidEnemyState state)
    {
        for (int offset = 0; offset < 0xa8; offset += 6)
        {
            ushort y = ReadWord(_bus!, 0xa7c5e7 + offset);
            if ((y & 0x8000) != 0)
                return;
            if (y != body.YPosition)
                continue;
            state.SinkTableEventCount++;
            ushort function = ReadWord(_bus!, 0xa7c5eb + offset);
            ushort? rockX = function switch
            {
                0xc691 => 0x0070,
                0xc6a7 => 0x00f0,
                0xc6bd => 0x00e0,
                0xc6d3 => 0x0090,
                0xc6e9 => 0x0080,
                0xc6ff => 0x0100,
                _ => null,
            };
            if (rockX.HasValue)
                _ = SpawnKraidCeilingRock(rockX.Value);
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
