using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arrival: his room setup, the rise through the floor and his growth into the second phase.</summary>
[RoomLifetime]
internal sealed class KraidRise
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
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
        _frameResults = dependencies.FrameResults;
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
            // and untouchable. Its two updates
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            _access.Backdrop.ShowKraidColours();
            state.BodyAppearance = KraidBodyAppearance.Hidden;
            ShowKraidRoomBackdrop();
            _frameResults.KraidObjects.AddRange(KraidObjectDefinitions.DefeatedRoom);
            // The body owns the sinking: a defeated room's body starts at hiding his body and the death's wait
            // before the fade-in. Kraid's other seven parts take their own dead initializer and remain deleted.
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
        state.HurtFrame = 0;
        state.HurtFrameTimer = 0;

        body.Position = body.Position.WithWhole(KraidArenaDefinitions.RiseStart);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyState.RisePhase = KraidRisePhase.RestrictSamusToFirstScreen;
        state.BodyState.PhaseTimer = KraidArenaDefinitions.RiseWaitUpdates;
        state.BodyState.HeadTimer = KraidArenaDefinitions.RiseHeadDelayUpdates;

        // The rocks' colours are the import's shade of them (#1512); his eyes start from his band's colours.
        state.Colours.ShowBandEyes(_head.BandEyes(KraidHead.InitialBand(body, state)));
        // The setup gives the room its rise shake; the rise runs it on later, with whatever shake the room has by then.
        _earthquake.ChangeShake(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground));
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
    internal void ShowKraidRoomBackdrop() => _presentation.Appearance.ShowKraidBackdrop();

    /// <summary>
    /// Brings Kraid's grown body in front of the room's sprites before his second phase. Only a risen
    /// body grows; a defeated room's hidden body never reaches growth.
    /// </summary>
    /// <exception cref="InvalidOperationException">His body is not behind the sprites.</exception>
    private static void BringKraidBodyInFront(KraidEnemyState state) =>
        state.BodyAppearance = state.BodyAppearance == KraidBodyAppearance.BehindSprites
            ? KraidBodyAppearance.InFrontOfSprites
            : throw new InvalidOperationException($"Kraid's growth reached a body that appears {state.BodyAppearance}.");

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemyPlace body, KraidEnemyState state)
    {
        state.BodyState.FightPhase = KraidFightPhase.SecondPhaseThinking;
        _parts.PlaceLintsAtBodyHeights(body);
        state.ThinkingTimer = _head.ReadKraidThinkingTimer();
    }

    /// <summary>Runs the current stage of Kraid's growth: his rise, the camera, his body and his colours.</summary>
    internal void RunKraidGrowthPhase(RoomEnemyPlace body, KraidEnemyState state)
    {
        switch (state.BodyState.GrowthPhase)
        {
            case KraidGrowthPhase.ReleaseCamera:
                ApplyKraidScrolls(layout: KraidScrollLayout.Grown);
                state.BodyState.GrowthPhase = KraidGrowthPhase.BreakCeilingPlatforms;
                state.EjectionLimitY = KraidArenaDefinitions.GrownEjectionLimitY;
                return;

            case KraidGrowthPhase.BreakCeilingPlatforms:
                if (KraidCadenceDefinitions.CeilingBreakRockEveryEighthUpdate.IsDue(body.FrameCounter))
                    RequestKraidRisingRock(body);
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole +
                    (KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(body.Position.Y.Whole) ? -1 : 1));
                body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 1);
                if (KraidCadenceDefinitions.CeilingRockEveryFourthRow.IsDue(body.Position.Y.Whole) &&
                    state.BodyState.CeilingRock < KraidCeilingRocks.Count)
                {
                    int rock = state.BodyState.CeilingRock;
                    _background.SpawnKraidCeilingRock(KraidCeilingRocks.X(rock));
                    _frameResults.KraidObjects.Add(KraidObjectDefinitions.GrowthCeiling[rock]);
                    state.BodyState.CeilingRock = rock + 1;
                }
                if (CoordinateAxis.Difference(body.Position.Y.Whole, KraidArenaDefinitions.GrownY) < 0)
                    state.BodyState.GrowthPhase = KraidGrowthPhase.BringBodyInFront;
                return;

            case KraidGrowthPhase.BringBodyInFront:
                BringKraidBodyInFront(state);
                _parts.Arm.Contact = _parts.Arm.Contact.Without(EnemyContact.IgnoresSamus);
                state.BodyState.GrowthPhase = KraidGrowthPhase.SettleParts;
                state.ShowBody();
                return;

            case KraidGrowthPhase.SettleParts:
                state.BodyState.GrowthPhase = KraidGrowthPhase.ShowRoomBackdrop;
                RoomEnemyPlace foot = _parts.Foot;
                state.FootState.StartAnimation(KraidPartSegment.FootNeutral);
                foreach (KraidLintState lint in state.Lints)
                {
                    lint.SetAnimation(KraidPartSegment.LintPostGrowth);
                    lint.Place.Frame = EnemyFrameId.KraidLintGrown;
                }
                return;

            case KraidGrowthPhase.ShowRoomBackdrop:
                state.BodyState.GrowthPhase = KraidGrowthPhase.FadeInRoomBackground;
                // Redrawing Kraid's room background restarts the shared fade step count.
                _presentation.FadeStepCount.Reset();
                // The background redraw shows the room's backdrop on the same frame that it
                // starts the backdrop's colour fade. Without it, the foreground's authored repeating
                // room-background blocks show an unset picture as transparent,
                // exposing the filler rectangle of Kraid's body layer above his head.
                ShowKraidRoomBackdrop();
                return;

            case KraidGrowthPhase.FadeInRoomBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToKraidColours))
                    return;
                FinishKraidGrowth(body, state);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidGrowthPhase)} {state.BodyState.GrowthPhase}.");
        }
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    internal void FinishKraidGrowth(RoomEnemyPlace body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            KraidLintState lintState = state.Lint(part);
            lintState.PhaseTimer = KraidLints.InitialDelay(part);
            lintState.Phase = KraidLintPhase.AlignToKraidBeforeProducing;
            lintState.LintTravel = 0;
        }
        // Each fingernail waits one launch delay longer than the one before.
        int nailDelay = KraidArenaDefinitions.NailLaunchDelayUpdates;
        foreach (KraidNailState nailState in state.Nails)
        {
            nailState.Phase = KraidNailPhase.WaitBeforeLaunch;
            nailState.PhaseTimer = nailDelay;
            nailDelay += KraidArenaDefinitions.NailLaunchDelayUpdates;
        }
        state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
        state.WalkTargetX = KraidArenaDefinitions.SecondPhaseStartX;
        _ = _parts.Foot;
        state.FootState.Phase = KraidFootPhase.SecondPhaseWalkToStart;
        state.FootState.StartAnimation(KraidPartSegment.FootWalkingBackward);
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    internal void RequestKraidRisingRock(RoomEnemyPlace body)
    {
        int random = _access.ReadKraidRandomNumber();
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, RoomEnemyProjectileKind.KraidRisingRock, KraidProjectiles.Rocks,
            graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        // The random sample's side chooses the rock's pose (#2278).
        rock.StartAnimation(new KraidRockAnimation(KraidRandomDefinitions.RisingRockRight.IsSetIn(random)
            ? KraidRockSegment.RisingRightPose
            : KraidRockSegment.SharedPose));
        // A rock on the left starts one pixel further from the body than its mirror on the right.
        int randomOffset = KraidRandomDefinitions.RisingRockOffset.Of(random);
        if (!KraidRandomDefinitions.RisingRockOffsetRight.IsSetIn(random))
            randomOffset = -(randomOffset + KraidRandomDefinitions.RisingRockLeftExtraPixels);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + randomOffset), KraidArenaDefinitions.RisingRockY));
        rock.Velocity = new(SignedEightEight.FromSixteenthsOfPixel(KraidRandomDefinitions.RisingRockXSpeedSixteenths.Of(random)), KraidRockLaunchDefinitions.RisingRockRise);
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
        _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
    }

    /// <summary>Runs the authored vertical rise and pause phases.</summary>
    internal void RunKraidRisePhase(
        RoomEnemyPlace body,
        KraidEnemyState state,
        SamusState samus)
    {
        switch (state.BodyState.RisePhase)
        {
            case KraidRisePhase.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                if (state.BodyState.CountPhaseTimerDown())
                    state.BodyState.RisePhase = KraidRisePhase.ShowBody;
                return;

            case KraidRisePhase.ShowBody:
                RestrictSamusToKraidFirstScreen(samus);
                state.BodyState.RisePhase = KraidRisePhase.StartQuakeAndMusic;
                // His body shows from here, as he rises through the floor (#1515).
                state.ShowBody();
                return;

            case KraidRisePhase.StartQuakeAndMusic:
                RestrictSamusToKraidFirstScreen(samus);
                state.BodyState.RisePhase = KraidRisePhase.RocksEvery16Frames;
                state.BodyState.RisingRockTimer = KraidArenaDefinitions.RiseSlowRocksUpdates;
                _earthquake.RunFor(KraidArenaDefinitions.RiseShakeUpdates);
                _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
                return;

            case KraidRisePhase.RocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                if (--state.BodyState.RisingRockTimer == 0)
                {
                    state.BodyState.RisePhase = KraidRisePhase.RocksEvery8Frames;
                    state.BodyState.RisingRockTimer = KraidArenaDefinitions.RiseFastRocksUpdates;
                }
                else if (KraidCadenceDefinitions.FirstRiseRockEverySixteenthUpdate.IsDue(state.BodyState.RisingRockTimer))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidRisePhase.RocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                if (--state.BodyState.RisingRockTimer == 0)
                {
                    state.BodyState.RisePhase = KraidRisePhase.RaiseBody;
                    state.BodyState.PhaseTimer = KraidArenaDefinitions.RaiseBodyTimerUpdates;
                }
                else if (KraidCadenceDefinitions.FasterRiseRockEveryEighthUpdate.IsDue(state.BodyState.RisingRockTimer))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidRisePhase.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if (KraidCadenceDefinitions.RaisedBodyFirstRockOfEight.IsDue(_earthquake.RemainingFrames) ||
                    KraidCadenceDefinitions.RaisedBodySecondRockOfEight.IsDue(_earthquake.RemainingFrames))
                    RequestKraidRisingRock(body);
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole +
                    (KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(body.Position.Y.Whole) ? 1 : -1));
                AddSignedKraidVerticalDisplacement(body, RisingBodyLift);
                if (CoordinateAxis.Difference(body.Position.Y.Whole, KraidArenaDefinitions.RisenY) < 0)
                {
                    body.Position = body.Position.WithWholeX(KraidArenaDefinitions.FirstPhaseX);
                    // The rise's last step does not leave execution at the fight setup.
                    // That setup immediately enters the thinking phase, seeds the random thinking
                    // timer and selects the roar animation for the first attack.
                    state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
                    _head.SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its lunge delay starts on the exact rise-completion frame.
                    state.FootState.Phase = KraidFootPhase.FirstPhaseThinking;
                    state.FootState.PhaseTimer = KraidArenaDefinitions.LungeDelayUpdates;
                    state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
                }
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidRisePhase)} {state.BodyState.RisePhase}.");
        }
    }

    /// <summary>
    /// Holds Samus on the room's first screen. It writes only her whole-pixel X
    /// and previous X, so the camera's horizontal follow sees Samus's subsequent
    /// movement away from the second screen's edge as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState samus)
    {
        if (CoordinateAxis.Difference(samus.Position.X.Whole, KraidArenaDefinitions.SecondScreenX) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(KraidArenaDefinitions.SecondScreenX);
            samus.WritePreviousXPosition(KraidArenaDefinitions.SecondScreenX);
        }
    }

    /// <summary>How far Kraid's body rises each update while it raises itself: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen RisingBodyLift = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);

    /// <summary>Adds signed vertical motion to the body position with fixed-point wraparound.</summary>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemyPlace body,
        SignedSixteenSixteen displacement)
    {
        body.Position = body.Position.WithY(body.Position.Y.MovedBy(displacement));
    }
}
