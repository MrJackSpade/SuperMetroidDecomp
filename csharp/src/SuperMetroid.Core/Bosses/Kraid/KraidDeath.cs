using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's death: the sink below the floor, hiding his body and the background fade.</summary>
[RoomLifetime]
internal sealed class KraidDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Room scratch memory and services used throughout the Kraid fight.</summary>
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
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;
    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
    private readonly KraidHead _head;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidDeath(KraidDeathDependencies dependencies)
    {
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
    /// <c>$A7:C715</c>: once the body has sunk, his body is hidden: the upper half of his body layer shows his blank on
    /// this update and the lower half on the next. His body layer stays shown in place of the room's background layer:
    /// on the cartridge his body overwrote the room's, so showing it again draws a stale strip of it across the top of
    /// the screen (#1427). A defeated room's reload hides his body here too. The one update in which the cartridge's
    /// half-cleared layer still shows a strip of body is not reproduced.
    /// </summary>
    private static void HideKraidUpperBody(KraidEnemyState state)
    {
        state.BodyAppearance = KraidBodyAppearance.Hidden;
        state.ShowUpperBody();
        state.ShowsBodyLayer = true;
    }

    /// <summary>Dispatches the staged sinking, palette, and room-repair death sequence.</summary>
    internal void RunKraidDeathFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        int cameraX)
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
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathShowUpperBody;
                    state.HurtFrameTimer = WrappingCountdown.Start(1);
                    state.HurtFrame = 1;
                    state.ShowUpperBody();
                }
                return;

            case KraidAiFunction.DeathShowUpperBody:
                _ = _head.ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathShowLowerBody;
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
                    state.Parts[_slotTable.Slots[lintSlot].SlotIndex].FunctionTimer = KraidBehavior.KraidStillPartTimer;
                }
                state.ShowUpperBody();
                return;

            case KraidAiFunction.DeathShowLowerBody:
                _ = _head.ProcessKraidHeadInstruction(body, state);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathSink;
                state.DeathSoundTimer = WrappingCountdown.Start(43);
                body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(256);
                _access.StartKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmRisingOrSinking);
                _access.StartKraidPartProgram(_slotTable.Slots[5], KraidPartProgram.FootInitial);
                state.Parts[_slotTable.Slots[5].SlotIndex].Function = KraidAiFunction.NoOperation;
                state.ShowLowerBody();
                return;

            case KraidAiFunction.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidAiFunction.DeathHideUpperBody:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathHideLowerBody;
                HideKraidUpperBody(state);
                return;
            case KraidAiFunction.DeathHideLowerBody:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathWaitBeforeFadeIn1;
                // `$A7:C751`: the lower half shows his hidden body, so his layer shows nothing for the rest of the visit.
                state.ShowLowerBody();
                return;
            // The death waits four updates before the room fades back in.
            case KraidAiFunction.DeathWaitBeforeFadeIn1:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathWaitBeforeFadeIn2;
                return;
            case KraidAiFunction.DeathWaitBeforeFadeIn2:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathWaitBeforeFadeIn3;
                return;
            case KraidAiFunction.DeathWaitBeforeFadeIn3:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathWaitBeforeFadeIn4;
                return;
            case KraidAiFunction.DeathWaitBeforeFadeIn4:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFadeInBackground;
                return;
            case KraidAiFunction.DeathFadeInBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToRoomColors))
                    return;
                state.MusicRequest = MusicRequest.SelectTrack(3);
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
            case KraidAiFunction.GrowBringBodyInFront:
            case KraidAiFunction.GrowShowLowerBodyInFront:
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
            case KraidAiFunction.RaiseShowLowerBody:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidOperationException($"Kraid body function {state.Parts[body.SlotIndex].Function} is not a death function.");
        }
    }

    /// <summary>
    /// Ports the shared body of <c>$A7:C843</c>/<c>$A7:C851</c>: a living Kraid, or a dead
    /// one whose camera has left the room's left edge, gets <c>SetEnemyPropertiesToDead</c>.
    /// </summary>
    private void MarkKraidDeadUnlessDeadAtLeftEdge(RoomEnemySlot body, int cameraX)
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
            _presentation.BossColours().KraidBody.Write(
                color,
                _background.ReadKraidColor(KraidPaletteSource.DeathArm, color));
        _access.StartKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmDyingOrPreparingToLunge);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathFadeOut;
        state.HeadProgram = KraidHeadAnimations.DeathAfterEntryFrame;
        state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.DeathEntryDuration;
        // $A7:C3A8-$C3E8 clear the fingernails' respawn bit, then run EnemyDeath for the
        // nails, then the three lints. The death runs without choosing an explosion, so each
        // part leaves the first death explosion.
        foreach (int slot in new[] { 6, 7 })
            _slotTable.Slots[slot].Properties = _slotTable.Slots[slot].Properties.Without(EnemyProperties.RespawnIfKilled);
        foreach (int slot in new[] { 6, 7, 2, 3, 4 })
            _deathsAndDrops.StartGenericEnemyDeath(_slotTable.Slots[slot], deathAnimation: 0);
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
        EnemyProperties armProperties = _slotTable.Slots[1].Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        _slotTable.Slots[1].Properties = armProperties;
        EnemyProperties otherProperties = armProperties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted);
        for (int slot = 2; slot <= 5; slot++)
            _slotTable.Slots[slot].Properties = otherProperties;
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathHideUpperBody;
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
            yMask: 63);
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
            // deliberately empty RTS used by rows whose only job is clearing a strip of his body.
            // Do not merge that address with the adjacent $C6A7 crumble routine: they are
            // distinct legal indirect-JSR targets in the retail table.
            if (KraidPlms.ForSinkCallback(function) is { } request)
            {
                // Each sinking rock starts at the left pixel edge of the removed block.
                _ = _background.SpawnKraidCeilingRock(request.Block.X * 16);
                _frameResults.KraidPlms.Add(request);
            }
            return;
        }
    }
}
