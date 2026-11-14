using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
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
    /// <summary>Applies Kraid palettes, background pictures, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Kraid's state and part lookups, part animations, phase timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>Kraid's room background: its fade, color transitions and the ceiling rocks.</summary>
    private readonly KraidBackground _background;
    /// <summary>Kraid's head: the mouth-open attack and reaction, the eye glow and the health palettes.</summary>
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
        _zebetiteBehavior = dependencies.ZebetiteBehavior;
        _access = dependencies.Access;
        _background = dependencies.Background;
        _head = dependencies.Head;
    }

    /// <summary>Creates his body's encounter state and restores the defeated room when the area-boss flag is set.</summary>
    internal void InitializeKraidBody(RoomEnemySlot body)
    {
        KraidParts.Require(body, _parts.Body, "body");

        _access.Kraid = new KraidEnemyState();
        KraidEnemyState state = _access.Kraid;
        _access.AttachKraidPart(body, state);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            // The defeated-room initializer installs the dead-room background palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            _access.Backdrop.ShowKraidColours();
            state.BodyAppearance = KraidBodyAppearance.Hidden;
            ShowKraidRoomBackdrop();
            _frameResults.KraidObjects.AddRange(KraidObjectDefinitions.DefeatedRoom);
            // The body owns the sinking: a defeated room's body starts at hiding his body layer's halves and
            // the death's four-update wait. Kraid's other seven parts take their own dead initializer and
            // remain deleted as in the game.
            state.BodyState.Phase = KraidPhase.DeathHideUpperBody;
            return;
        }

        // Kraid's fight tracks the camera like a boss.
        _zebetiteBehavior.CameraDistance = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(layout: KraidScrollLayout.Initial);
        state.EjectionLimit = new Position16<RoomPixels>(0, 324);
        state.InitialHealth = body.Health;

        // Kraid's initialization readies his body layer, on which his body shows once he rises.
        state.InitializeBodyLayer();
        state.HurtFrame = 0;
        state.HurtFrameTimer = 0;

        body.Position = body.Position.WithWhole(new(176, 592));
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyState.Phase = KraidPhase.RestrictSamusToFirstScreen;
        state.BodyState.PhaseTimer = 300;
        state.BodyState.HeadTimer = 64;
        state.BodyState.PhaseToResume = KraidPhase.RaiseKraidThroughFloor;

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
    private void SetupKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.BodyState.Phase = KraidPhase.SecondPhaseThinking;
        _parts.TopLint.Position = _parts.TopLint.Position.WithWrappedWholeY(body.Position.Y.Whole - 20);
        _parts.MiddleLint.Position = _parts.MiddleLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 46);
        _parts.BottomLint.Position = _parts.BottomLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 112);
        state.ThinkingTimer = _head.ReadKraidThinkingTimer();
    }

    /// <summary>Dispatches the rise, camera, body and palette stages of growth.</summary>
    internal void RunKraidGrowthPhase(RoomEnemySlot body, KraidEnemyState state)
    {
        switch (state.BodyState.Phase)
        {
            case KraidPhase.GrowReleaseCamera:
                ApplyKraidScrolls(layout: KraidScrollLayout.Grown);
                state.BodyState.Phase = KraidPhase.GrowBreakCeilingPlatforms;
                state.EjectionLimit = new Position16<RoomPixels>(0, 164);
                return;

            case KraidPhase.GrowBreakCeilingPlatforms:
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
                if (CoordinateAxis.Difference(body.Position.Y.Whole, 296) < 0)
                    state.BodyState.Phase = KraidPhase.GrowBringBodyInFront;
                return;

            case KraidPhase.GrowBringBodyInFront:
                BringKraidBodyInFront(state);
                _parts.Arm.Contact = _parts.Arm.Contact.Without(EnemyContact.IgnoresSamus);
                state.BodyState.Phase = KraidPhase.GrowShowLowerBodyInFront;
                state.ShowBody();
                return;

            case KraidPhase.GrowShowLowerBodyInFront:
                state.BodyState.Phase = KraidPhase.GrowDrawRoomBackground;
                RoomEnemySlot foot = _parts.Foot;
                _access.StartKraidPartAnimation(foot, KraidPartSegment.FootNeutral);
                foreach (RoomEnemySlot lint in _parts.Lints)
                {
                    _access.SetKraidPartAnimation(lint, KraidPartSegment.LintPostGrowth);
                    lint.Frame = KraidPartFrameDefinitions.ShownFrames.Of(new KraidPartFrame(KraidPartFrameKind.Lint, 1));
                }
                return;

            case KraidPhase.GrowDrawRoomBackground:
                state.BodyState.Phase = KraidPhase.GrowFadeInRoomBackground;
                // The game clears the value it kept the ceiling rocks in, which is the phase timer again.
                state.BodyState.PhaseTimer = 0;
                // Redrawing Kraid's room background restarts the shared fade step count.
                _presentation.FadeStepCount.Reset();
                // The background redraw shows the room's backdrop on the same frame that it
                // starts the backdrop's colour fade. Without it, the foreground's authored repeating
                // room-background blocks show an unset picture as transparent,
                // exposing the filler rectangle of Kraid's body layer above his head.
                ShowKraidRoomBackdrop();
                return;

            case KraidPhase.GrowFadeInRoomBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToRoomColors))
                    return;
                FinishKraidGrowth(body, state);
                return;
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
            case KraidPhase.WaitToResumePhase:
            case KraidPhase.FootWaitBeforeWalk:
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
            case KraidPhase.DeathInitialize:
            case KraidPhase.DeathFadeOut:
            case KraidPhase.DeathShowUpperBody:
            case KraidPhase.DeathShowLowerBody:
            case KraidPhase.DeathSink:
            case KraidPhase.DeathHideUpperBody:
            case KraidPhase.DeathHideLowerBody:
            case KraidPhase.DeathWaitBeforeFadeIn1:
            case KraidPhase.DeathWaitBeforeFadeIn2:
            case KraidPhase.DeathWaitBeforeFadeIn3:
            case KraidPhase.DeathWaitBeforeFadeIn4:
            case KraidPhase.DeathFadeInBackground:
            case KraidPhase.DeathFinishedWasAlive:
            case KraidPhase.DeathFinishedWasDead:
            case KraidPhase.RestrictSamusToFirstScreen:
            case KraidPhase.RaiseKraidThroughFloor:
            case KraidPhase.RaiseShowLowerBody:
            case KraidPhase.RaiseRocksEvery16Frames:
            case KraidPhase.RaiseRocksEvery8Frames:
            case KraidPhase.RaiseBody:
            default:
                throw new InvalidOperationException($"Kraid body phase {state.BodyState.Phase} is not a growth phase.");
        }
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    internal void FinishKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            RoomEnemySlot lint = _parts.Lint(part);
            _parts.LintStateOf(state, lint).PhaseTimer = KraidLints.InitialDelay(part);
            _parts.LintStateOf(state, lint).Phase = KraidPhase.AlignPartToKraid;
            _parts.LintStateOf(state, lint).PhaseToResume = KraidPhase.LintProduce;
            _parts.LintStateOf(state, lint).LintTravel = 0;
        }
        // The second fingernail waits 64 updates longer than the first.
        int nailDelay = 64;
        foreach (RoomEnemySlot nail in _parts.Nails)
        {
            _parts.NailStateOf(state, nail).PhaseToResume = KraidPhase.FingernailInitialize;
            _parts.NailStateOf(state, nail).Phase = KraidPhase.WaitToResumePhase;
            _parts.NailStateOf(state, nail).PhaseTimer = nailDelay;
            nailDelay += 64;
        }
        state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
        state.WalkTarget = new Position16<RoomPixels>(288, 0);
        RoomEnemySlot foot = _parts.Foot;
        state.FootState.Phase = KraidPhase.FootSecondPhaseWalkToStart;
        _access.StartKraidPartAnimation(foot, KraidPartSegment.FootWalkingBackward);
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    internal void RequestKraidRisingRock(RoomEnemySlot body)
    {
        int random = _access.ReadKraidRandomNumber();
        RoomEnemyProjectileKind kind = KraidRandomDefinitions.RisingRockRight.IsSetIn(random)
            ? RoomEnemyProjectileKind.KraidRisingRockRight
            : RoomEnemyProjectileKind.KraidRisingRockLeft;
        EnemyProjectile? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, kind, KraidProjectiles.Rocks, graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(kind == RoomEnemyProjectileKind.KraidRisingRockRight
            ? KraidRockSegment.RisingRightPose
            : KraidRockSegment.SharedPose));
        // A rock on the left starts one pixel further from the body than its mirror on the right.
        int randomOffset = KraidRandomDefinitions.RisingRockOffset.Of(random);
        if (!KraidRandomDefinitions.RisingRockOffsetRight.IsSetIn(random))
            randomOffset = CoordinateAxis.AsSigned(-(randomOffset + KraidRandomDefinitions.RisingRockLeftExtraPixels));
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + randomOffset), 432));
        rock.Velocity = new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(KraidRandomDefinitions.RisingRockXSpeedSteps.Of(random) * KraidRandomDefinitions.RisingRockXSpeedStep), SignedEightEight.FromPixels(-5));
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
        _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
    }

    /// <summary>Runs the authored vertical rise and pause phases.</summary>
    internal void RunKraidRisePhase(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusState? samus)
    {
        KraidPhase phase = state.BodyState.Phase;
        switch (phase)
        {
            case KraidPhase.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                KraidEncounterAccess.TickKraidPhaseTimer(state.BodyState);
                return;

            case KraidPhase.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                state.BodyState.Phase = KraidPhase.RaiseShowLowerBody;
                // His body shows from here, as he rises through the floor (#1515).
                state.ShowBody();
                return;

            case KraidPhase.RaiseShowLowerBody:
                RestrictSamusToKraidFirstScreen(samus);
                state.BodyState.Phase = KraidPhase.RaiseRocksEvery16Frames;
                state.BodyState.RisingRockTimer = 120;
                _earthquake.RunFor(496);
                _access.RequestKraidMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
                return;

            case KraidPhase.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                if (--state.BodyState.RisingRockTimer == 0)
                {
                    state.BodyState.Phase = KraidPhase.RaiseRocksEvery8Frames;
                    state.BodyState.RisingRockTimer = 96;
                }
                else if (KraidCadenceDefinitions.FirstRiseRockEverySixteenthUpdate.IsDue(state.BodyState.RisingRockTimer))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidPhase.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                if (--state.BodyState.RisingRockTimer == 0)
                {
                    state.BodyState.Phase = KraidPhase.RaiseBody;
                    state.BodyState.PhaseTimer = 288;
                }
                else if (KraidCadenceDefinitions.FasterRiseRockEveryEighthUpdate.IsDue(state.BodyState.RisingRockTimer))
                {
                    RequestKraidRisingRock(body);
                }
                return;

            case KraidPhase.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if (KraidCadenceDefinitions.RaisedBodyFirstRockOfEight.IsDue(_earthquake.RemainingFrames) ||
                    KraidCadenceDefinitions.RaisedBodySecondRockOfEight.IsDue(_earthquake.RemainingFrames))
                    RequestKraidRisingRock(body);
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole +
                    (KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(body.Position.Y.Whole) ? 1 : -1));
                AddSignedKraidVerticalDisplacement(body, RisingBodyLift);
                if (CoordinateAxis.Difference(body.Position.Y.Whole, 457) < 0)
                {
                    body.Position = body.Position.WithWholeX(176);
                    // The rise's last step does not leave execution at the fight setup.
                    // That setup immediately enters the thinking phase, seeds the random thinking
                    // timer and selects the roar animation for the first attack.
                    state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
                    _head.SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    state.FootState.Phase = KraidPhase.FootFirstPhaseThinking;
                    state.FootState.PhaseTimer = 300;
                    state.FootState.PhaseToResume = KraidPhase.FootPrepareFirstPhaseLunge;
                    _access.StartKraidPartAnimation(_parts.Arm, KraidPartSegment.ArmNormal);
                }
                return;

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
            case KraidPhase.WaitToResumePhase:
            case KraidPhase.FootWaitBeforeWalk:
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
            case KraidPhase.DeathInitialize:
            case KraidPhase.DeathFadeOut:
            case KraidPhase.DeathShowUpperBody:
            case KraidPhase.DeathShowLowerBody:
            case KraidPhase.DeathSink:
            case KraidPhase.DeathHideUpperBody:
            case KraidPhase.DeathHideLowerBody:
            case KraidPhase.DeathWaitBeforeFadeIn1:
            case KraidPhase.DeathWaitBeforeFadeIn2:
            case KraidPhase.DeathWaitBeforeFadeIn3:
            case KraidPhase.DeathWaitBeforeFadeIn4:
            case KraidPhase.DeathFadeInBackground:
            case KraidPhase.DeathFinishedWasAlive:
            case KraidPhase.DeathFinishedWasDead:
            default:
                throw new InvalidDataException(
                    $"Kraid body has no behaviour for phase {state.BodyState.Phase}.");
        }
    }

    /// <summary>
    /// Holds Samus on the room's first screen. It writes only her whole-pixel X
    /// and previous X, so the camera's horizontal follow sees Samus's subsequent
    /// movement away from 256 as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState? samus)
    {
        if (samus is not null && CoordinateAxis.Difference(samus.Position.X.Whole, 256) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(256);
            samus.WritePreviousXPosition(256);
        }
    }

    /// <summary>How far Kraid's body rises each update while it raises itself: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen RisingBodyLift = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);

    /// <summary>Adds signed vertical motion to the body position with fixed-point wraparound.</summary>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        SignedSixteenSixteen displacement)
    {
        body.Position = body.Position.WithY(body.Position.Y.MovedBy(displacement));
    }
}
