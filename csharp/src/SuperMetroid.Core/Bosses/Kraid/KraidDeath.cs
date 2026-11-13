using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's death: the sink below the floor, hiding his body and the background fade.</summary>
[RoomLifetime]
internal sealed class KraidDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Applies Kraid palettes, background tiles, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Kraid's state and part lookups, part animations, function timer, random numbers and the sound he last requested.</summary>
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
        _parts = dependencies.Parts;
        _presentation = dependencies.Presentation;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
    }

    /// <summary>
    /// Once the body has sunk, his whole body is hidden (#1515). His body layer stays shown in place of the room's
    /// background layer: in the game his body overwrote the room's, so showing it again draws a stale strip of it
    /// across the top of the screen (#1427). A defeated room's reload hides his body here too.
    /// </summary>
    private static void HideKraidBody(KraidEnemyState state)
    {
        state.BodyAppearance = KraidBodyAppearance.Hidden;
        state.ShowBody();
        state.ShowsBodyLayer = true;
    }

    /// <summary>Dispatches the staged sinking, palette, and room-repair death sequence.</summary>
    internal void RunKraidDeathPhase(
        RoomEnemySlot body,
        KraidEnemyState state,
        int cameraX)
    {
        switch (state.BodyState.Phase)
        {
            case KraidPhase.DeathInitialize:
                InitializeKraidDeath(state);
                return;

            case KraidPhase.DeathFadeOut:
                _ = _head.AdvanceKraidHeadAnimation(state);
                if (_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToBlack))
                {
                    state.BodyState.Phase = KraidPhase.DeathShowUpperBody;
                    state.HurtFrameTimer = 1;
                    state.HurtFrame = 1;
                    state.ShowBody();
                }
                return;

            case KraidPhase.DeathShowUpperBody:
                _ = _head.AdvanceKraidHeadAnimation(state);
                state.BodyState.Phase = KraidPhase.DeathShowLowerBody;
                foreach (RoomEnemySlot lint in _parts.Lints)
                {
                    _parts.LintStateOf(state, lint).Phase = KraidPhase.AlignPartToKraid;
                    _parts.LintStateOf(state, lint).FunctionTimer = KraidBehavior.KraidStillPartTimer;
                }
                state.ShowBody();
                return;

            case KraidPhase.DeathShowLowerBody:
                _ = _head.AdvanceKraidHeadAnimation(state);
                state.BodyState.Phase = KraidPhase.DeathSink;
                state.DeathSoundTimer = 43;
                body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 256);
                _access.StartKraidPartAnimation(_parts.Arm, KraidPartSegment.ArmRisingOrSinking);
                _access.StartKraidPartAnimation(_parts.Foot, KraidPartSegment.FootInitial);
                state.FootState.Phase = KraidPhase.NoOperation;
                return;

            case KraidPhase.DeathSink:
                RunKraidDeathSink(body, state);
                return;

            case KraidPhase.DeathHideUpperBody:
                state.BodyState.Phase = KraidPhase.DeathHideLowerBody;
                HideKraidBody(state);
                return;
            case KraidPhase.DeathHideLowerBody:
                state.BodyState.Phase = KraidPhase.DeathWaitBeforeFadeIn1;
                return;
            // The death waits four updates before the room fades back in.
            case KraidPhase.DeathWaitBeforeFadeIn1:
                state.BodyState.Phase = KraidPhase.DeathWaitBeforeFadeIn2;
                return;
            case KraidPhase.DeathWaitBeforeFadeIn2:
                state.BodyState.Phase = KraidPhase.DeathWaitBeforeFadeIn3;
                return;
            case KraidPhase.DeathWaitBeforeFadeIn3:
                state.BodyState.Phase = KraidPhase.DeathWaitBeforeFadeIn4;
                return;
            case KraidPhase.DeathWaitBeforeFadeIn4:
                state.BodyState.Phase = KraidPhase.DeathFadeInBackground;
                return;
            case KraidPhase.DeathFadeInBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToRoomColors))
                    return;
                _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
                {
                    state.BodyState.Phase = KraidPhase.DeathFinishedWasDead;
                }
                else
                {
                    _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                    state.BodyState.Phase = KraidPhase.DeathFinishedWasAlive;
                }
                return;
            case KraidPhase.DeathFinishedWasAlive:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidPhase.DeathFinishedWasDead:
                MarkKraidDeadUnlessDeadAtLeftEdge(body, cameraX);
                return;
            case KraidPhase.NoOperation:
            case KraidPhase.GrowBreakCeilingPlatforms:
            case KraidPhase.GrowBringBodyInFront:
            case KraidPhase.GrowShowLowerBodyInFront:
            case KraidPhase.GrowDrawRoomBackground:
            case KraidPhase.GrowFadeInRoomBackground:
            case KraidPhase.MainloopThinking:
            case KraidPhase.SecondPhaseThinking:
            case KraidPhase.MouthOpenReaction:
            case KraidPhase.InitializeEyeGlow:
            case KraidPhase.GlowEye:
            case KraidPhase.UnglowEye:
            case KraidPhase.LintInactive:
            case KraidPhase.LintProduce:
            case KraidPhase.LintCharge:
            case KraidPhase.LintFire:
            case KraidPhase.FingernailWaitForLint:
            case KraidPhase.AlignPartToKraid:
            case KraidPhase.HandleFunctionTimer:
            case KraidPhase.DecrementFunctionTimerAndStartWalk:
            case KraidPhase.FootFirstPhaseThinking:
            case KraidPhase.AdvanceHeadAnimationAndTimer:
            case KraidPhase.FootSecondPhaseThinking:
            case KraidPhase.FootSecondPhaseWalkingRight:
            case KraidPhase.FootSecondPhaseWalkToStart:
            case KraidPhase.FootSecondPhaseInitialize:
            case KraidPhase.FootSecondPhaseWalkingLeft:
            case KraidPhase.MainAttackWithMouthOpen:
            case KraidPhase.FingernailInitialize:
            case KraidPhase.FingernailFire:
            case KraidPhase.FootPrepareFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseRetreat:
            case KraidPhase.GrowReleaseCamera:
            case KraidPhase.RestrictSamusToFirstScreen:
            case KraidPhase.RaiseKraidThroughFloor:
            case KraidPhase.RaiseShowLowerBody:
            case KraidPhase.RaiseRocksEvery16Frames:
            case KraidPhase.RaiseRocksEvery8Frames:
            case KraidPhase.RaiseBody:
            case KraidPhase.None:
            default:
                throw new InvalidOperationException($"Kraid body function {state.BodyState.Phase} is not a death function.");
        }
    }

    /// <summary>
    /// Runs the shared body of both death-drop checks: a living Kraid, or a dead
    /// one whose camera has left the room's left edge, is given a dead enemy's properties.
    /// </summary>
    private void MarkKraidDeadUnlessDeadAtLeftEdge(RoomEnemySlot body, int cameraX)
    {
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && cameraX == 0)
            return;
        KraidEncounterAccess.MarkKraidPartDead(body);
    }

    /// <summary>Starts the death animation and prepares its room mutation schedule.</summary>
    internal void InitializeKraidDeath(KraidEnemyState state)
    {
        if (state.HurtFrame != 0)
            return;
        state.Colours.ShowDeathArm();
        _access.StartKraidPartAnimation(_parts.Arm, KraidPartSegment.ArmDyingOrPreparingToLunge);
        state.BodyState.Phase = KraidPhase.DeathFadeOut;
        state.HeadPlayback = KraidHeadAnimations.DeathAfterEntryFrame;
        state.BodyState.HeadTimer = KraidHeadAnimations.DeathEntryDuration;
        // The death clears the fingernails' respawn bit, then kills the
        // nails, then the three lints. The death runs without choosing an explosion, so each
        // part leaves the first death explosion.
        foreach (RoomEnemySlot nail in _parts.Nails)
            nail.Presence = nail.Presence.Without(EnemyPresence.RespawnsIfKilled);
        foreach (RoomEnemySlot nail in _parts.Nails)
            _deathsAndDrops.StartGenericEnemyDeath(nail, deathAnimation: EnemyDeathExplosion.Small);
        foreach (RoomEnemySlot lint in _parts.Lints)
            _deathsAndDrops.StartGenericEnemyDeath(lint, deathAnimation: EnemyDeathExplosion.Small);
        // The live sequence crumbles successive blocks while Kraid fades/sinks.
        // It must not substitute the instantaneous already-defeated-room clear.
        _frameResults.KraidObjects.Add(KraidObjectDefinitions.LiveDeathSpikes);
    }

    /// <summary>Moves the body downward through the authored sink sequence.</summary>
    private void RunKraidDeathSink(RoomEnemySlot body, KraidEnemyState state)
    {
        _ = _head.AdvanceKraidHeadAnimation(state);
        if (--state.DeathSoundTimer == 0)
        {
            _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
            state.DeathSoundTimer = 30;
        }
        ProcessKraidSinkTable(body);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        if (CoordinateAxis.Difference(body.Position.Y.Whole, 608) < 0)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        RoomEnemySlot arm = _parts.Arm;
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        arm.Presence = arm.Presence.With(EnemyPresence.Deleted);
        // The lints and the foot take the arm's new properties, no longer solid and without animation or off-screen
        // processing.
        EnemyContact otherContact = arm.Contact.Without(EnemyContact.SolidToSamus);
        EnemyProcessing otherProcessing = arm.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        foreach (RoomEnemySlot lint in _parts.Lints)
        {
            lint.Contact = otherContact;
            lint.Processing = otherProcessing;
            lint.Presence = arm.Presence;
        }
        _parts.Foot.Contact = otherContact;
        _parts.Foot.Processing = otherProcessing;
        _parts.Foot.Presence = arm.Presence;
        state.BodyState.Phase = KraidPhase.DeathHideUpperBody;
        // The death returns the camera to distance index zero before the death drop.
        _zebetiteBehavior.CameraDistance = CameraDistanceMode.NormalTracking;

        // The death drops fill the same sixteen-projectile pool used by ordinary deaths. Each
        // pickup gets a random position in Kraid's 256x64 floor strip and independently
        // rolls Kraid's six-entry chance table.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.Kraid,
            count: 16,
            xBase: 128,
            xScatter: EnemyDropScatterDefinitions.WideArenaHorizontalScatter,
            yBase: 352,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        // The death continues into redrawing Kraid's room background, which restarts the shared fade step count.
        _presentation.FadeStepCount.Reset();
    }

    /// <summary>Consumes sink-table entries as the body descends.</summary>
    internal void ProcessKraidSinkTable(RoomEnemySlot body)
    {
        if (KraidSinkSchedule.CallbackAt(body.Position.Y.Whole) is KraidSinkCallback function)
        {
            // Every eight-pixel sinking row invokes its callback, including the
            // deliberately empty callback used by rows whose only job is clearing a strip of his body.
            // Do not merge that callback with the adjacent crumble callback: they are
            // distinct legal callbacks in the retail table.
            if (KraidObjects.ForSinkCallback(function) is { } request)
            {
                // Each sinking rock starts at the left pixel edge of the removed block.
                _ = _background.SpawnKraidCeilingRock(request.Block.X * 16);
                _frameResults.KraidObjects.Add(request);
            }
            return;
        }
    }
}
