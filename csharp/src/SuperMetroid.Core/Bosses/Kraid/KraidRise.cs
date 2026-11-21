using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
[RoomLifetime]
internal sealed class KraidRise
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Applies Kraid's colours, background pictures, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    private readonly RoomCameraDistance _cameraDistance;
    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;
    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health colours.</summary>
    private readonly KraidHead _head;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidRise(KraidRiseDependencies dependencies)
    {
        _passEvents = dependencies.PassEvents;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _presentation = dependencies.Presentation;
        _cameraDistance = dependencies.CameraDistance;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
        _places = dependencies.Places;
        _bodyLayer = dependencies.BodyLayer;
    }

    /// <summary>The body layer Kraid draws himself, which the room shows (#2194).</summary>
    /// <summary>The room's enemy places, where his body finds his other parts as his room lists them.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Kraid's body layer, which his rise attaches to the room's display.</summary>
    private readonly KraidBodyLayer _bodyLayer;

    /// <summary>Creates his body's encounter state and restores the defeated room when the area-boss flag is set.</summary>
    /// <returns>The encounter the body begins, which it holds from its setup.</returns>
    internal KraidEnemyState InitializeKraidBody(RoomEnemyPlace body)
    {
        // The body begins the encounter each of his parts joins as it sets up (#2218): a body that sets up again begins a
        // new one.
        var state = new KraidEnemyState(new KraidParts(_places, body));
        // The room shows the body layer he draws himself, of this encounter, his backdrop's colours and his own (#2194).
        _bodyLayer.Show(state);
        _presentation.AttachBossBody(_bodyLayer);
        _presentation.BossShadings.Show(BossShadingLayer.KraidBackdrop, _access.Backdrop);
        _presentation.BossShadings.Show(BossShadingLayer.Kraid, state.Colours);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            // The defeated room shows its dead-room background colours before every part of Kraid is hidden, deleted
            // and untouchable. Showing those colours and the room's backdrop restores the defeated arena on every room
            // load, so the broken ceiling and removed floor spikes stay gone when Samus leaves and comes back.
            _access.Backdrop.ShowKraidColours();
            state.HideBody();
            ShowKraidRoomBackdrop();
            foreach (RoomObjectSpawnRequest<KraidRoomMutation> change in KraidObjectDefinitions.DefeatedRoom)
                _passEvents.Publish(new KraidRoomChange(change));
            // The body owns the sinking: a defeated room's body starts at hiding his body and the death's wait
            // before the fade-in. Kraid's other seven parts take their own dead setup and remain deleted.
            state.BodyState.Run = new KraidStageRun.Sunk(KraidSunkPhase.HideBody);
            return state;
        }

        // Kraid's fight tracks the camera like a boss.
        _cameraDistance.Request(CameraDistanceMode.BossTracking);
        ApplyKraidScrolls(layout: KraidScrollLayout.Initial);
        state.EjectionLimitY = KraidArenaDefinitions.FirstPhaseEjectionLimitY;
        state.InitialHealth = body.Health;

        // Kraid's initialization readies his body layer, on which his body shows once he rises.
        state.InitializeBodyLayer();

        body.Position = body.Position.WithWhole(KraidArenaDefinitions.RiseStart);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyState.Run = new KraidStageRun.Rising(KraidRisePhase.RestrictSamusToFirstScreen);
        state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.RiseWaitUpdates);
        state.HoldHeadUntilRise();

        // The rocks' colours are the import's shade of them (#1512); his eyes start from his band's colours.
        state.Colours.ShowBandEyes(_head.BandEyes(KraidHead.InitialBand(body, state)));
        // The setup gives the room its rise shake; the rise runs it on later, with whatever shake the room has by then.
        _earthquake.ChangeShake(KraidRiseDefinitions.RiseShake);
        return state;
    }

    /// <summary>Switches the arena scrolls between the small and grown boss layouts.</summary>
    private void ApplyKraidScrolls(KraidScrollLayout layout)
    {
        ScreenScrollChange[] changes = layout == KraidScrollLayout.Grown
            ? KraidCameraDefinitions.GrownScrolls
            : KraidCameraDefinitions.InitialScrolls;
        foreach (ScreenScrollChange change in changes)
            _room.Services.RoomScroll.Write(change.Screen, change.State);
    }

    /// <summary>
    /// Shows the fixed room backdrop over the room's blocks (#1352, #1406, #1466): the blocks and cells it shows over show
    /// their pictures with it from here.
    /// </summary>
    private void ShowKraidRoomBackdrop() => _presentation.Appearance.ShowKraidBackdrop();

    /// <summary>
    /// Brings Kraid's grown body in front of the room's sprites before his second phase, whatever it showed before, as the
    /// game's growth sets the layer outright.
    /// </summary>
    /// <exception cref="InvalidOperationException">His body is not behind the sprites.</exception>
    private static void BringKraidBodyInFront(KraidEnemyState state) => state.BringBodyInFront();

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemyPlace body, KraidEnemyState state, KraidRisenHead head)
    {
        state.BodyState.Run = new KraidStageRun.Fighting(KraidFightPhase.SecondPhaseThinking, head);
        state.Parts.PlaceLintsAtBodyHeights(body);
        state.ThinkingWait.LastFor(_head.ChooseKraidThinkingUpdates());
    }

    /// <summary>Runs the current stage of Kraid's growth: his rise, the camera, his body and his colours.</summary>
    internal void RunKraidGrowthPhase(RoomEnemyPlace body, KraidEnemyState state, KraidStageRun.Growing growth)
    {
        switch (growth.Phase)
        {
            case KraidGrowthPhase.ReleaseCamera:
                ApplyKraidScrolls(layout: KraidScrollLayout.Grown);
                growth.Phase = KraidGrowthPhase.BreakCeilingPlatforms;
                state.EjectionLimitY = KraidArenaDefinitions.GrownEjectionLimitY;
                return;

            case KraidGrowthPhase.BreakCeilingPlatforms:
                if (KraidCadenceDefinitions.CeilingBreakRock.IsDue(body.FrameCounter))
                    RequestKraidRisingRock(body);
                body.Position = body.Position.WithWholeX(body.Position.X.Whole +
                    1.Toward(KraidCadenceDefinitions.RiseSwayPhase.IsInSecondHalfAt(body.Position.Y.Whole) ? HorizontalDirection.Left : HorizontalDirection.Right));
                body.Position = body.Position.WithWholeY(body.Position.Y.Whole - 1);
                if (KraidCadenceDefinitions.CeilingRockRow.IsDue(body.Position.Y.Whole) &&
                    state.BodyState.NextCeilingRock is { } rock)
                {
                    _background.SpawnKraidCeilingRock(rock.Entry.X);
                    _passEvents.Publish(new KraidRoomChange(rock.Entry.Crumble));
                    state.BodyState.NextCeilingRock = rock.Following;
                }
                if (body.Position.Y.Whole < KraidArenaDefinitions.GrownY)
                    growth.Phase = KraidGrowthPhase.BringBodyInFront;
                return;

            case KraidGrowthPhase.BringBodyInFront:
                BringKraidBodyInFront(state);
                state.Parts.Arm.Contact = state.Parts.Arm.Contact.Without(EnemyContact.IgnoresSamus);
                growth.Phase = KraidGrowthPhase.SettleParts;
                state.ShowBody();
                return;

            case KraidGrowthPhase.SettleParts:
                growth.Phase = KraidGrowthPhase.ShowRoomBackdrop;
                state.FootState.StartAnimation(KraidPartSegment.FootNeutral);
                foreach (KraidLintState lint in state.Lints)
                    lint.ShowPose(KraidPartSegment.LintPostGrowth);
                return;

            case KraidGrowthPhase.ShowRoomBackdrop:
                growth.Phase = KraidGrowthPhase.FadeInRoomBackground;
                // Redrawing Kraid's room background restarts the shared fade step count.
                _presentation.FadeStepCount.Reset();
                // The background redraw shows the room's backdrop on the same frame that it
                // starts the backdrop's colour fade. Without it, the foreground's authored repeating
                // room-background blocks show an unset picture as transparent,
                // exposing the filler rectangle of Kraid's body layer above his head.
                ShowKraidRoomBackdrop();
                return;

            case KraidGrowthPhase.FadeInRoomBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropSettledColours.KraidColours))
                    return;
                FinishKraidGrowth(body, state, growth.Head);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidGrowthPhase)} {growth.Phase}.");
        }
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    private void FinishKraidGrowth(RoomEnemyPlace body, KraidEnemyState state, KraidRisenHead head)
    {
        SetupKraidSecondPhaseThinking(body, state, head);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            KraidLintState lintState = state.Lint(part);
            lintState.PhaseWait.LastFor(KraidLints.InitialDelay(part));
            lintState.Phase = KraidLintPhase.AlignToKraidBeforeProducing;
            lintState.LintTravel = 0;
        }
        // Each fingernail waits one launch delay longer than the one before.
        int nailDelay = KraidArenaDefinitions.NailLaunchDelayUpdates;
        foreach (KraidNailState nailState in state.Nails)
        {
            nailState.Phase = KraidNailPhase.WaitBeforeLaunch;
            nailState.PhaseWait.LastFor(nailDelay);
            nailDelay += KraidArenaDefinitions.NailLaunchDelayUpdates;
        }
        head.Select(KraidHeadSegment.Roar);
        state.WalkTargetX = KraidArenaDefinitions.SecondPhaseStartX;
        state.FootState.Phase = KraidFootPhase.SecondPhaseWalkToStart;
        state.FootState.StartAnimation(KraidPartSegment.FootWalkingBackward);
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    private void RequestKraidRisingRock(RoomEnemyPlace body)
    {
        int random = _access.ReadKraidRandomNumber();
        // The random sample's side chooses the rock's pose (#2278).
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.KraidRisingRock, _background.Rocks, graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading), new KraidRockAnimation(_background.Rocks, KraidRandomDefinitions.RisingRockFliesRight.IsTakenBy(random)
            ? KraidRockSegment.RisingRightPose
            : KraidRockSegment.SharedPose));
        if (rock is null)
            return;

        // A rock on the left starts one pixel further from the body than its mirror on the right.
        int randomOffset = KraidRandomDefinitions.RisingRockOffset.Of(random);
        if (!KraidRandomDefinitions.RisingRockStartsOnRight.IsTakenBy(random))
            randomOffset = -(randomOffset + KraidRandomDefinitions.RisingRockLeftExtraPixels);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(body.Position.X.Whole + randomOffset, KraidArenaDefinitions.RisingRockY));
        rock.Velocity = new(KraidRandomDefinitions.RisingRockXSpeed.Of(random), KraidRockLaunchDefinitions.RisingRockRise);
        _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
    }

    /// <summary>Runs the authored vertical rise and pause phases.</summary>
    internal void RunKraidRisePhase(
        RoomEnemyPlace body,
        KraidEnemyState state,
        KraidStageRun.Rising rising,
        SamusState samus)
    {
        switch (rising.Phase)
        {
            case KraidRisePhase.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                if (state.BodyState.PhaseWait.StepRunsOut())
                    rising.Phase = KraidRisePhase.ShowBody;
                return;

            case KraidRisePhase.ShowBody:
                RestrictSamusToKraidFirstScreen(samus);
                rising.Phase = KraidRisePhase.StartQuakeAndMusic;
                // His body shows from here, as he rises through the floor (#1515).
                state.ShowBody();
                return;

            case KraidRisePhase.StartQuakeAndMusic:
                RestrictSamusToKraidFirstScreen(samus);
                rising.Phase = KraidRisePhase.SlowRocks;
                state.BodyState.RisingRocksWait.LastFor(KraidArenaDefinitions.RiseSlowRocksUpdates);
                _earthquake.RunFor(KraidArenaDefinitions.RiseShakeUpdates);
                _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
                return;

            case KraidRisePhase.SlowRocks:
                RestrictSamusToKraidFirstScreen(samus);
                if (state.BodyState.RisingRocksWait.StepRunsOut())
                {
                    rising.Phase = KraidRisePhase.FastRocks;
                    state.BodyState.RisingRocksWait.LastFor(KraidArenaDefinitions.RiseFastRocksUpdates);
                }
                else if (KraidCadenceDefinitions.FirstRiseRock.IsDue(state.BodyState.RisingRocksWait.UpdatesLeft))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidRisePhase.FastRocks:
                RestrictSamusToKraidFirstScreen(samus);
                if (state.BodyState.RisingRocksWait.StepRunsOut())
                {
                    rising.Phase = KraidRisePhase.RaiseBody;
                }
                else if (KraidCadenceDefinitions.FasterRiseRock.IsDue(state.BodyState.RisingRocksWait.UpdatesLeft))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidRisePhase.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if (KraidCadenceDefinitions.RaisedBodyFirstRockOfEight.IsDue(_earthquake.RemainingFrames) ||
                    KraidCadenceDefinitions.RaisedBodySecondRockOfEight.IsDue(_earthquake.RemainingFrames))
                    RequestKraidRisingRock(body);
                body.Position = body.Position.WithWholeX(body.Position.X.Whole +
                    1.Toward(KraidCadenceDefinitions.RiseSwayPhase.IsInSecondHalfAt(body.Position.Y.Whole) ? HorizontalDirection.Right : HorizontalDirection.Left));
                body.Position = body.Position.AdvanceY(KraidArenaDefinitions.RisingBodyLift);
                if (body.Position.Y.Whole < KraidArenaDefinitions.RisenY)
                {
                    body.Position = body.Position.WithWholeX(KraidArenaDefinitions.FirstPhaseX);
                    // The rise's last update goes on into the fight: it enters the thinking phase,
                    // starts its random thinking and selects the roar animation for the first attack.
                    var fight = new KraidStageRun.Fighting(KraidFightPhase.FirstPhaseThinking, KraidRisenHead.SelectingRoar());
                    state.BodyState.Run = fight;
                    _head.SetupKraidFirstPhaseThinking(body, state, fight);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its lunge delay starts on the exact rise-completion frame.
                    state.FootState.Phase = KraidFootPhase.FirstPhaseThinking;
                    state.FootState.PhaseWait.LastFor(KraidArenaDefinitions.LungeDelayUpdates);
                    state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
                }
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidRisePhase)} {rising.Phase}.");
        }
    }

    /// <summary>
    /// Holds Samus on the room's first screen. It writes only her whole-pixel X
    /// and previous X, so the camera's horizontal follow sees Samus's subsequent
    /// movement away from the second screen's edge as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState samus)
    {
        if (samus.Position.X.Whole >= KraidArenaDefinitions.SecondScreenX)
        {
            samus.Position = samus.Position.WithWholeX(KraidArenaDefinitions.SecondScreenX);
            samus.WritePreviousXPosition(KraidArenaDefinitions.SecondScreenX);
        }
    }

}
