using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's death: the sink below the floor, the tilemap clears and the background fade.</summary>
[RoomLifetime]
internal sealed class KraidDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Room scratch memory, services, and tilemap state used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions, top and bottom tilemap transfers and the ceiling rocks.</summary>
    private readonly KraidBackground _background;
    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
    private readonly KraidHead _head;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidDeath(KraidDeathDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
    }

    /// <summary>
    /// <c>$A7:C715</c>: once the body has sunk, BG2 becomes Kraid's map filled with the blank tile, cleared over this
    /// frame and the next. BG2 never returns to the room's own tilemap: on the cartridge Kraid's uploads overwrote it,
    /// so showing it again draws stale room words across the top of the screen (#1427). A defeated room's reload takes
    /// BG2 here too. The one frame in which the cartridge's half-cleared map still shows a strip of body is not
    /// reproduced.
    /// </summary>
    private static void ClearKraidTopTilemapForDeath(KraidEnemyState state)
    {
        state.WorkingTop = KraidBodyPage.Blank;
        state.DisplayedBody = state.DisplayedBody with { Top = KraidBodyPage.Blank, Head = null };
        state.OwnsBg2Tilemap = true;
        state.TopTilemapUploadCount++;
    }

    /// <summary><c>$A7:C751</c>: blanks the rest of Kraid's BG2 map, so it shows nothing for the rest of the visit.</summary>
    private static void ClearKraidBottomTilemapForDeath(KraidEnemyState state)
    {
        state.WorkingBottom = KraidBodyPage.Blank;
        state.DisplayedBody = state.DisplayedBody with { Bottom = KraidBodyPage.Blank };
        state.BottomTilemapUploadCount++;
    }

    /// <summary>Dispatches the staged sinking, palette, and room-repair death sequence.</summary>
    internal void RunKraidDeathFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        VramWriteQueue? vramWriteQueue,
        ushort cameraX)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.DeathInitialize:
                InitializeKraidDeath(body, state);
                return;

            case KraidAiFunction.DeathFadeOut:
                _ = _head.ProcessKraidHeadInstruction(body, state);
                if (_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToBlack))
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathUpdateTopTilemap;
                    state.HurtFrameTimer = WrappingCountdown.Start(1);
                    state.HurtFrame = 1;
                    KraidBackground.TransferKraidTopTilemap(state);
                }
                return;

            case KraidAiFunction.DeathUpdateTopTilemap:
                _ = _head.ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathUpdateBottomTilemap;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].FunctionTimer = KraidBehavior.KraidStillPartTimer;
                }
                KraidBackground.TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.DeathUpdateBottomTilemap:
                _ = _head.ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathSink;
                state.DeathSoundTimer = WrappingCountdown.Start(43);
                body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(256);
                _access.StartKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmRisingOrSinking);
                _access.StartKraidPartProgram(_slotTable.Slots[5], KraidPartProgram.FootInitial);
                state.Parts[_slotTable.Slots[5].SlotIndex].Function = KraidAiFunction.NoOperation;
                KraidBackground.TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidAiFunction.DeathClearTopTilemap:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathClearBottomTilemap;
                ClearKraidTopTilemapForDeath(state);
                return;
            case KraidAiFunction.DeathClearBottomTilemap:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathLoadBg3Quarter1;
                ClearKraidBottomTilemapForDeath(state);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter1:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 0,
                    KraidAiFunction.DeathLoadBg3Quarter2,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter2:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 1,
                    KraidAiFunction.DeathLoadBg3Quarter3,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter3:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 2,
                    KraidAiFunction.DeathLoadBg3Quarter4,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathLoadBg3Quarter4:
                AdvanceKraidDeathBg3Transfer(
                    body,
                    state,
                    transferIndex: 3,
                    KraidAiFunction.DeathFadeInBackground,
                    vramWriteQueue);
                return;
            case KraidAiFunction.DeathFadeInBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToRoomColors))
                    return;
                state.MusicRequest = MusicCommand.SelectTrack(3);
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFinishedWasDead;
                }
                else
                {
                    _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                    state.BossDefeatPersisted = true;
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFinishedWasAlive;
                }
                state.DeathSequenceComplete = true;
                return;
            case KraidAiFunction.DeathFinishedWasAlive:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidAiFunction.DeathFinishedWasDead:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidAiFunction.NoOperation:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowSetBg2Priority:
            case KraidAiFunction.GrowFinishBg2Update:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
            case KraidAiFunction.MainloopThinking:
            case KraidAiFunction.SecondPhaseThinking:
            case KraidAiFunction.MouthOpenReaction:
            case KraidAiFunction.InitializeEyeGlow:
            case KraidAiFunction.GlowEye:
            case KraidAiFunction.UnglowEye:
            case KraidAiFunction.LintInactive:
            case KraidAiFunction.LintProduce:
            case KraidAiFunction.LintCharge:
            case KraidAiFunction.LintFire:
            case KraidAiFunction.FingernailWaitForLint:
            case KraidAiFunction.AlignPartToKraid:
            case KraidAiFunction.HandleFunctionTimer:
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
            case KraidAiFunction.FootFirstPhaseThinking:
            case KraidAiFunction.ProcessHeadInstructionAndTimer:
            case KraidAiFunction.FootSecondPhaseThinking:
            case KraidAiFunction.FootSecondPhaseWalkingRight:
            case KraidAiFunction.FootSecondPhaseWalkToStart:
            case KraidAiFunction.FootSecondPhaseInitialize:
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
            case KraidAiFunction.MainAttackWithMouthOpen:
            case KraidAiFunction.FingernailInitialize:
            case KraidAiFunction.FingernailFire:
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseRetreat:
            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseLoadBottomTilemap:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            default:
                throw new InvalidOperationException($"Kraid body function {state.Parts[body.SlotIndex].Function} is not a death function.");
        }
    }

    /// <summary>
    /// Ports the shared body of <c>$A7:C843</c>/<c>$A7:C851</c>: a living Kraid, or a dead
    /// one whose camera has left the room's left edge, gets <c>SetEnemyPropertiesToDead</c>.
    /// </summary>
    private void MarkKraidDeadUnlessDeadAtLeftEdge(RoomEnemySlot body, ushort cameraX)
    {
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && cameraX == 0)
            return;
        KraidEncounterAccess.MarkKraidPartDead(body);
    }

    /// <summary>Starts the death animation and prepares its room mutation schedule.</summary>
    internal void InitializeKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        if (state.HurtFrame != 0)
            return;
        for (int color = 0; color < 16; color++)
            _presentation.BossPalettes().KraidBody.Write(
                color,
                _background.ReadKraidColor(KraidPaletteSource.DeathArm, color));
        _access.StartKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmDyingOrPreparingToLunge);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFadeOut;
        state.HeadProgram = KraidHeadAnimations.DeathAfterEntryFrame;
        state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.DeathEntryDuration;
        // $A7:C3A8-$C3E8 clear the fingernails' respawn bit, then run EnemyDeath for the
        // nails, then the three lints. A still holds each slot's native index, which
        // EnemyDeath treats as explosion type zero; every part leaves a death explosion.
        foreach (int slot in new[] { 6, 7 })
            _slotTable.Slots[slot].Properties = _slotTable.Slots[slot].Properties.Without(EnemyProperties.RespawnIfKilled);
        foreach (int slot in new[] { 6, 7, 2, 3, 4 })
            _deathsAndDrops.StartGenericEnemyDeath(_slotTable.Slots[slot], deathAnimation: _slotTable.Slots[slot].NativeIndex);
        // The live sequence crumbles successive blocks while Kraid fades/sinks.
        // It must not substitute the instantaneous already-defeated-room clear.
        _frameResults.KraidPlms.Add(KraidPlmDefinitions.LiveDeathSpikes);
    }

    /// <summary>Moves the body downward through the authored sink sequence.</summary>
    private void RunKraidDeathSink(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = _head.ProcessKraidHeadInstruction(body, state);
        state.DeathSoundTimer = state.DeathSoundTimer.Decrement().Remaining;
        if (state.DeathSoundTimer.IsZero)
        {
            _access.LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.RockSpit);
            state.DeathSoundTimer = WrappingCountdown.Start(30);
        }
        ProcessKraidSinkTable(body, state);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - 608)) < 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        EnemyPropertyWord armProperties = _slotTable.Slots[1].Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        _slotTable.Slots[1].Properties = armProperties;
        EnemyPropertyWord otherProperties = armProperties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        for (int slot = 2; slot <= 5; slot++)
            _slotTable.Slots[slot].Properties = otherProperties;
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathClearTopTilemap;
        // $A7:C594 returns the camera to distance index zero before the death drop.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.NormalTracking;

        // $A0:B8EE fills the same sixteen-projectile pool used by ordinary deaths. Each
        // pickup gets a random position in Kraid's 256x64 floor strip and independently
        // rolls header $E2BF's six-byte chance table.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Kraid,
            count: 16,
            xBase: 128,
            xMask: byte.MaxValue,
            yBase: 352,
            yMask: SnesLittleEndian.Word(0, 63));
        state.DeathDropRequestCount = 16;
        // $A7:C59B continues into DrawKraidsRoomBackground, which zeroes the shared numerator.
        _presentation.GradualColorChange.Reset();
    }

    /// <summary>Consumes native sink-table entries as the body descends.</summary>
    internal void ProcessKraidSinkTable(RoomEnemySlot body, KraidEnemyState state)
    {
        if (KraidSinkSchedule.CallbackAt(body.Position.Y.Whole) is KraidSinkCallback function)
        {
            state.SinkTableEventCount++;
            // Every eight-pixel sinking row invokes its cartridge callback, including the
            // deliberately empty RTS used by rows whose only job is the BG2 strip upload.
            // Do not merge that address with the adjacent $C6A7 crumble routine: they are
            // distinct legal indirect-JSR targets in the retail table.
            if (KraidPlmDefinitions.ForSinkCallback(function) is { } request)
            {
                // Each sinking rock starts at the left pixel edge of the removed block.
                _ = _background.SpawnKraidCeilingRock((ushort)(request.Block.X * 16));
                _frameResults.KraidPlms.Add(request);
            }
            return;
        }
    }

    /// <summary>Restores standard BG3 tiles through staged DMA after the body dies.</summary>
    internal void AdvanceKraidDeathBg3Transfer(
        RoomEnemySlot body,
        KraidEnemyState state,
        int transferIndex,
        KraidAiFunction next,
        VramWriteQueue? vramWriteQueue)
    {
        if ((uint)transferIndex >= KraidBackgroundRomData.StandardBg3TransferCount)
            throw new ArgumentOutOfRangeException(nameof(transferIndex));

        int destinationWord = BossVram.KraidBg3RestoreQuarterWord(transferIndex);

        // `$A7:C777-$C7EF` deliberately spreads the $1000-byte restoration over four
        // enemy frames. The runtime path appends the same four native seven-byte DMA
        // records so NMI owns visibility. Isolated enemy audits have no NMI/queue owner;
        // executing that one frame's transfer directly preserves the identical bytes and
        // keeps their per-frame sequencing observable without fabricating a host queue.
        // The restored characters are background artwork bound at the NMI (#1352).
        HudTileAtlas hud = _bossArtwork.HudTileArtwork ?? throw new InvalidDataException(
            "Installed Kraid death restoration has no standard BG3 artwork.");
        if (vramWriteQueue is not null)
            vramWriteQueue.EnqueueBackgroundAsset(
                KraidBackgroundRomData.StandardBg3AssetForQuarter(transferIndex),
                KraidBackgroundRomData.StandardBg3TransferBytes, BossVram.KraidBg3RestoreDma(destinationWord));
        else
            BossVram.BindKraidBg3Restore(_presentation.Backgrounds, transferIndex, hud.KraidRestoreQuarter(transferIndex).Span);

        state.DeathBg3TransferCount++;
        state.Parts[body.SlotIndex].Function = next;
    }
}
