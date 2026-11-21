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
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
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
        _parts = dependencies.Parts;
        _presentation = dependencies.Presentation;
        _cameraDistance = dependencies.CameraDistance;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
        _bodyLayer = dependencies.BodyLayer;
    }

    /// <summary>The body layer Kraid draws himself, which the room shows (#2194).</summary>
    private readonly KraidBodyLayer _bodyLayer;

    /// <summary>Creates his body's encounter state and restores the defeated room when the area-boss flag is set.</summary>
    internal void InitializeKraidBody(RoomEnemyPlace body)
    {
        // The body begins the encounter each of his parts joins as it sets up (#2218): a body that sets up again begins a
        // new one.
        var state = new KraidEnemyState(_parts);
        KraidEncounterAccess.AttachKraidPart(state.BodyState);
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
            state.BodyAppearance = KraidBodyAppearance.Hidden;
            ShowKraidRoomBackdrop();
            foreach (RoomObjectSpawnRequest<KraidRoomMutation> change in KraidObjectDefinitions.DefeatedRoom)
                _passEvents.Publish(new KraidRoomChange(change));
            // The body owns the sinking: a defeated room's body starts at hiding his body and the death's wait
            // before the fade-in. Kraid's other seven parts take their own dead setup and remain deleted.
            state.BodyState.DeathPhase = KraidDeathPhase.HideBody;
            return;
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
        state.BodyState.RisePhase = KraidRisePhase.RestrictSamusToFirstScreen;
        state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.RiseWaitUpdates);
        state.HoldHeadUntilRise();

        // The rocks' colours are the import's shade of them (#1512); his eyes start from his band's colours.
        state.Colours.ShowBandEyes(_head.BandEyes(KraidHead.InitialBand(body, state)));
        // The setup gives the room its rise shake; the rise runs it on later, with whatever shake the room has by then.
        _earthquake.ChangeShake(KraidRiseDefinitions.RiseShake);
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
    private static void BringKraidBodyInFront(KraidEnemyState state) =>
        state.BodyAppearance = KraidBodyAppearance.InFrontOfSprites;

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemyPlace body, KraidEnemyState state)
    {
        state.BodyState.FightPhase = KraidFightPhase.SecondPhaseThinking;
        _parts.PlaceLintsAtBodyHeights(body);
        state.ThinkingWait.LastFor(_head.ChooseKraidThinkingUpdates());
    }

    /// <summary>What <c>RunKraidGrowthPhase</c> does for each <see cref="KraidGrowthPhase"/> member, each member's own arm.</summary>
    private IdentityTable<KraidGrowthPhase, Action<RoomEnemyPlace, KraidEnemyState>> RunKraidGrowthPhaseForKraidGrowthPhase => field ??= new(new Dictionary<KraidGrowthPhase, Action<RoomEnemyPlace, KraidEnemyState>>
    {
        [KraidGrowthPhase.ReleaseCamera] = (body, state) =>
        {
            ApplyKraidScrolls(layout: KraidScrollLayout.Grown);
            state.BodyState.GrowthPhase = KraidGrowthPhase.BreakCeilingPlatforms;
            state.EjectionLimitY = KraidArenaDefinitions.GrownEjectionLimitY;
        },
        [KraidGrowthPhase.BreakCeilingPlatforms] = (body, state) =>
        {
            if (KraidCadenceDefinitions.CeilingBreakRock.IsDue(body.FrameCounter))
                RequestKraidRisingRock(body);
            body.Position = body.Position.WithWholeX(body.Position.X.Whole +
                1.Toward(KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(body.Position.Y.Whole) ? HorizontalDirection.Left : HorizontalDirection.Right));
            body.Position = body.Position.WithWholeY(body.Position.Y.Whole - 1);
            if (KraidCadenceDefinitions.CeilingRockRow.IsDue(body.Position.Y.Whole) &&
                state.BodyState.NextCeilingRock is { } rock)
            {
                _background.SpawnKraidCeilingRock(rock.Entry.X);
                _passEvents.Publish(new KraidRoomChange(rock.Entry.Crumble));
                state.BodyState.NextCeilingRock = rock.Following;
            }
            if (body.Position.Y.Whole < KraidArenaDefinitions.GrownY)
                state.BodyState.GrowthPhase = KraidGrowthPhase.BringBodyInFront;
        },
        [KraidGrowthPhase.BringBodyInFront] = (body, state) =>
        {
            BringKraidBodyInFront(state);
            _parts.Arm.Contact = _parts.Arm.Contact.Without(EnemyContact.IgnoresSamus);
            state.BodyState.GrowthPhase = KraidGrowthPhase.SettleParts;
            state.ShowBody();
        },
        [KraidGrowthPhase.SettleParts] = (body, state) =>
        {
            state.BodyState.GrowthPhase = KraidGrowthPhase.ShowRoomBackdrop;
            state.FootState.StartAnimation(KraidPartSegment.FootNeutral);
            foreach (KraidLintState lint in state.Lints)
                lint.ShowPose(KraidPartSegment.LintPostGrowth);
        },
        [KraidGrowthPhase.ShowRoomBackdrop] = (body, state) =>
        {
            state.BodyState.GrowthPhase = KraidGrowthPhase.FadeInRoomBackground;
            // Redrawing Kraid's room background restarts the shared fade step count.
            _presentation.FadeStepCount.Reset();
            // The background redraw shows the room's backdrop on the same frame that it
            // starts the backdrop's colour fade. Without it, the foreground's authored repeating
            // room-background blocks show an unset picture as transparent,
            // exposing the filler rectangle of Kraid's body layer above his head.
            ShowKraidRoomBackdrop();
        },
        [KraidGrowthPhase.FadeInRoomBackground] = (body, state) =>
        {
            if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToKraidColours))
                return;
            FinishKraidGrowth(body, state);
        },
    });

    /// <summary>Runs the current stage of Kraid's growth: his rise, the camera, his body and his colours.</summary>
    internal void RunKraidGrowthPhase(RoomEnemyPlace body, KraidEnemyState state)
    {
        RunKraidGrowthPhaseForKraidGrowthPhase[state.BodyState.GrowthPhase](body, state);
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    private void FinishKraidGrowth(RoomEnemyPlace body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

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
        state.SelectHeadAnimation(KraidHeadSegment.Roar);
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

    /// <summary>What <c>RunKraidRisePhase</c> does for each <see cref="KraidRisePhase"/> member, each member's own arm.</summary>
    private IdentityTable<KraidRisePhase, Action<RoomEnemyPlace, KraidEnemyState, SamusState>> RunKraidRisePhaseForKraidRisePhase => field ??= new(new Dictionary<KraidRisePhase, Action<RoomEnemyPlace, KraidEnemyState, SamusState>>
    {
        [KraidRisePhase.RestrictSamusToFirstScreen] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            if (state.BodyState.PhaseWait.StepRunsOut())
                state.BodyState.RisePhase = KraidRisePhase.ShowBody;
        },
        [KraidRisePhase.ShowBody] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            state.BodyState.RisePhase = KraidRisePhase.StartQuakeAndMusic;
            // His body shows from here, as he rises through the floor (#1515).
            state.ShowBody();
        },
        [KraidRisePhase.StartQuakeAndMusic] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            state.BodyState.RisePhase = KraidRisePhase.SlowRocks;
            state.BodyState.RisingRocksWait.LastFor(KraidArenaDefinitions.RiseSlowRocksUpdates);
            _earthquake.RunFor(KraidArenaDefinitions.RiseShakeUpdates);
            _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
        },
        [KraidRisePhase.SlowRocks] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            if (state.BodyState.RisingRocksWait.StepRunsOut())
            {
                state.BodyState.RisePhase = KraidRisePhase.FastRocks;
                state.BodyState.RisingRocksWait.LastFor(KraidArenaDefinitions.RiseFastRocksUpdates);
            }
            else if (KraidCadenceDefinitions.FirstRiseRock.IsDue(state.BodyState.RisingRocksWait.UpdatesLeft))
            {
                RequestKraidRisingRock(body);
            }
        },
        [KraidRisePhase.FastRocks] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            if (state.BodyState.RisingRocksWait.StepRunsOut())
            {
                state.BodyState.RisePhase = KraidRisePhase.RaiseBody;
            }
            else if (KraidCadenceDefinitions.FasterRiseRock.IsDue(state.BodyState.RisingRocksWait.UpdatesLeft))
            {
                RequestKraidRisingRock(body);
            }
        },
        [KraidRisePhase.RaiseBody] = (body, state, samus) =>
        {
            RestrictSamusToKraidFirstScreen(samus);
            if (KraidCadenceDefinitions.RaisedBodyFirstRockOfEight.IsDue(_earthquake.RemainingFrames) ||
                KraidCadenceDefinitions.RaisedBodySecondRockOfEight.IsDue(_earthquake.RemainingFrames))
                RequestKraidRisingRock(body);
            body.Position = body.Position.WithWholeX(body.Position.X.Whole +
                1.Toward(KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(body.Position.Y.Whole) ? HorizontalDirection.Right : HorizontalDirection.Left));
            body.Position = body.Position.AdvanceY(KraidArenaDefinitions.RisingBodyLift);
            if (body.Position.Y.Whole < KraidArenaDefinitions.RisenY)
            {
                body.Position = body.Position.WithWholeX(KraidArenaDefinitions.FirstPhaseX);
                // The rise's last update goes on into the fight: it enters the thinking phase,
                // starts its random thinking and selects the roar animation for the first attack.
                state.SelectHeadAnimation(KraidHeadSegment.Roar);
                _head.SetupKraidFirstPhaseThinking(body, state);

                // The foot owns first-phase lunges independently of the body thinker.
                // Its lunge delay starts on the exact rise-completion frame.
                state.FootState.Phase = KraidFootPhase.FirstPhaseThinking;
                state.FootState.PhaseWait.LastFor(KraidArenaDefinitions.LungeDelayUpdates);
                state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
            }
        },
    });

    /// <summary>Runs the authored vertical rise and pause phases.</summary>
    internal void RunKraidRisePhase(
        RoomEnemyPlace body,
        KraidEnemyState state,
        SamusState samus)
    {
        RunKraidRisePhaseForKraidRisePhase[state.BodyState.RisePhase](body, state, samus);
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
