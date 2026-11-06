using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;

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
    /// <summary>Applies Kraid palettes, background tiles, and sprite presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Coordinates Zebetite behavior registered alongside Kraid's room actors.</summary>
    private readonly ZebetiteBehavior _zebetiteBehavior;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
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

    /// <summary>Creates slot-zero encounter state and restores the defeated room when the area-boss flag is set.</summary>
    internal void InitializeKraidBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Kraid body requires the first enemy slot, not slot {body.SlotIndex}.");
        }

        _access.Kraid = new KraidEnemyState();
        KraidEnemyState state = _access.Kraid;
        KraidEncounterAccess.AttachKraidPart(body, state);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            // The defeated-room initializer installs the dead-room background palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            _access.Backdrop.ShowKraidColours();
            state.BodyAppearance = KraidBodyAppearance.Hidden;
            ShowKraidRoomBackdrop();
            _frameResults.KraidPlms.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body owns the sinking: a defeated room's body starts at hiding his body layer's halves and
            // the death's four-update wait. The other seven physical records take their own dead initializer and
            // remain deleted as in the original game.
            state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathHideUpperBody;
            return;
        }

        // Kraid's fight tracks the camera like a boss.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(layout: KraidScrollLayout.Initial);
        state.EjectionLimit = new Position16<RoomPixels>(0, 324);
        state.InitialHealth = body.Health;

        // Kraid's initialization readies his body layer, on which his body shows once he rises.
        state.InitializeBodyLayer();
        state.HurtFrame = 0;
        state.HurtFrameTimer = WrappingCountdown.Start(0);

        body.Position = body.Position.WithWhole(new(176, 592));
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.RestrictSamusToFirstScreen;
        state.Parts[body.SlotIndex].FunctionTimer = WrappingCountdown.Start(300);
        state.Parts[body.SlotIndex].HeadTimer = WrappingCountdown.Start(64);
        state.Parts[_parts.Body.SlotIndex].NextFunction = KraidAiFunction.RaiseKraidThroughFloor;

        // The rocks' colours are the import's shade of them (#1512); his eyes start from his band's colours.
        state.Colours.EyeGlow.ShowBand(_head.BandEyes(KraidHead.InitialBand(body, state)));
        _earthquake.EarthquakeType = 5;
    }

    /// <summary>Switches the arena scrolls between the small and grown boss layouts.</summary>
    private void ApplyKraidScrolls(KraidScrollLayout layout)
    {
        for (int index = 0; index < KraidCameraDefinitions.ScreenCount; index++)
            _room.Services.RoomScroll.Write(index, layout == KraidScrollLayout.Grown
                ? KraidCamera.GrownScroll(index)
                : KraidCamera.InitialScroll(index));
    }

    /// <summary>
    /// Shows the fixed room backdrop over the room's tiles (#1352, #1406, #1466): the blocks and cells it shows over show
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
        state.Parts[body.SlotIndex].Function = KraidAiFunction.SecondPhaseThinking;
        _parts.TopLint.Position = _parts.TopLint.Position.WithWrappedWholeY(body.Position.Y.Whole - 20);
        _parts.MiddleLint.Position = _parts.MiddleLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 46);
        _parts.BottomLint.Position = _parts.BottomLint.Position.WithWrappedWholeY(body.Position.Y.Whole + 112);
        state.ThinkingTimer = WrappingCountdown.FromRaw(_head.ReadKraidThinkingTimer());
    }

    /// <summary>Dispatches the rise, camera, body and palette stages of growth.</summary>
    internal void RunKraidGrowthFunction(RoomEnemySlot body, KraidEnemyState state)
    {
        switch (state.Parts[body.SlotIndex].Function)
        {
            case KraidAiFunction.GrowReleaseCamera:
                ApplyKraidScrolls(layout: KraidScrollLayout.Grown);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowBreakCeilingPlatforms;
                state.CameraReleasedForSecondPhase = true;
                state.EjectionLimit = new Position16<RoomPixels>(0, 164);
                return;

            case KraidAiFunction.GrowBreakCeilingPlatforms:
                if ((body.FrameCounter & 7) == 0)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? 1 : -1));
                body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 1);
                if ((body.Position.Y.Whole & 3) == 0 && state.Parts[body.SlotIndex].CeilingRock < KraidCeilingRocks.Count)
                {
                    int rock = state.Parts[body.SlotIndex].CeilingRock;
                    if (_background.SpawnKraidCeilingRock(KraidCeilingRocks.X(rock)))
                        state.CeilingRockSpawnCount++;
                    _frameResults.KraidPlms.Add(KraidPlmDefinitions.GrowthCeiling[rock]);
                    state.Parts[body.SlotIndex].CeilingRock = rock + 1;
                }
                if (CoordinateAxis.Difference(body.Position.Y.Whole, 296) < 0)
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowBringBodyInFront;
                return;

            case KraidAiFunction.GrowBringBodyInFront:
                BringKraidBodyInFront(state);
                _parts.Arm.Properties = _parts.Arm.Properties.Without(
                    EnemyProperties.IgnoreSamusCollision);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowShowLowerBodyInFront;
                state.ShowBody();
                return;

            case KraidAiFunction.GrowShowLowerBodyInFront:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowDrawRoomBackground;
                RoomEnemySlot foot = _parts.Foot;
                _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
                foreach (RoomEnemySlot lint in _parts.Lints)
                {
                    _access.SetKraidPartProgram(lint, KraidPartProgram.LintPostGrowth);
                    lint.Frame = KraidPartFrameDefinitions.ShownFrames.Of(new KraidPartFrame(KraidPartFrameKind.Lint, 1));
                }
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowFadeInRoomBackground;
                // The original game clears the value it kept the ceiling rocks in, which is the function timer again.
                state.Parts[body.SlotIndex].FunctionTimer = WrappingCountdown.Start(0);
                // DrawKraidsRoomBackground zeroes the shared numerator.
                _presentation.GradualColorChange.Reset();
                // The background redraw queues this character upload on the same frame that it
                // initializes palette-six fading. Without it, the foreground's authored repeating
                // room-background blocks decode an uninitialized character as transparent,
                // exposing the filler rectangle of Kraid's body layer above his head.
                ShowKraidRoomBackdrop();
                return;

            case KraidAiFunction.GrowFadeInRoomBackground:
                if (!_background.AdvanceKraidRoomBackgroundFade(fade: KraidBackdropFade.ToRoomColors))
                    return;
                FinishKraidGrowth(body, state);
                return;
            case KraidAiFunction.NoOperation:
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
            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathShowUpperBody:
            case KraidAiFunction.DeathShowLowerBody:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathHideUpperBody:
            case KraidAiFunction.DeathHideLowerBody:
            case KraidAiFunction.DeathWaitBeforeFadeIn1:
            case KraidAiFunction.DeathWaitBeforeFadeIn2:
            case KraidAiFunction.DeathWaitBeforeFadeIn3:
            case KraidAiFunction.DeathWaitBeforeFadeIn4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseShowLowerBody:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidOperationException($"Kraid body function {state.Parts[body.SlotIndex].Function} is not a growth function.");
        }
    }

    /// <summary>Installs the grown body pose, room scrolls, and second-phase behavior.</summary>
    internal void FinishKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        SetupKraidSecondPhaseThinking(body, state);

        foreach (KraidLintPart part in Enum.GetValues<KraidLintPart>())
        {
            RoomEnemySlot lint = _parts.Lint(part);
            state.Parts[lint.SlotIndex].FunctionTimer = WrappingCountdown.Start(KraidLints.InitialDelay(part));
            state.Parts[lint.SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
            state.Parts[lint.SlotIndex].NextFunction = KraidAiFunction.LintProduce;
            state.Parts[lint.SlotIndex].LintTravel = 0;
        }
        // The second fingernail waits 64 updates longer than the first.
        int nailDelay = 64;
        foreach (RoomEnemySlot nail in _parts.Nails)
        {
            state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
            state.Parts[nail.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
            state.Parts[nail.SlotIndex].FunctionTimer = WrappingCountdown.Start(nailDelay);
            nailDelay += 64;
        }
        state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
        state.WalkTarget = new Position16<RoomPixels>(288, 0);
        RoomEnemySlot foot = _parts.Foot;
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.FootSecondPhaseWalkToStart;
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
    }

    /// <summary>Requests a rock from the floor during the growth sequence.</summary>
    internal void RequestKraidRisingRock(RoomEnemySlot body, KraidEnemyState state)
    {
        state.RiseRockSpawnRequestCount++;
        int random = _access.ReadKraidRandomNumber();
        RoomEnemyProjectileKind kind = (random & 16) != 0
            ? RoomEnemyProjectileKind.KraidRisingRockRight
            : RoomEnemyProjectileKind.KraidRisingRockLeft;
        RoomEnemyProjectileSlot? rock = _projectiles.AllocateEnemyProjectile();
        if (rock is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, kind, graphics: SpriteGraphics.SharedIn(KraidColours.RocksShading));
        rock.StartAnimation(new KraidRockAnimation(kind == RoomEnemyProjectileKind.KraidRisingRockRight
            ? KraidRockProgram.RisingRightPose
            : KraidRockProgram.SharedPose));
        // The rock initializer tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 63;
        if ((random & 1) == 0)
            randomOffset = CoordinateAxis.AsSigned(~randomOffset);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + randomOffset), 432));
        rock.Velocity = new(SignedEightEight.Wrapped(random & 1008), SignedEightEight.FromSignedRaw(-1280));
        rock.Graphics = SpriteGraphics.SharedIn(KraidColours.RocksShading);
        state.SpawnedRiseRockCount++;
        _access.RequestKraidSound(KraidSoundDefinitions.RockSpit);
    }

    /// <summary>Dispatches the authored vertical rise and pause functions.</summary>
    internal void RunKraidRiseFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusState? samus)
    {
        KraidAiFunction function = state.Parts[body.SlotIndex].Function;
        switch (function)
        {
            case KraidAiFunction.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                KraidEncounterAccess.TickKraidFunctionTimer(state.Parts[_parts.Body.SlotIndex]);
                return;

            case KraidAiFunction.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseShowLowerBody;
                // His body shows from here, as he rises through the floor (#1515).
                state.ShowBody();
                return;

            case KraidAiFunction.RaiseShowLowerBody:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery16Frames;
                state.Parts[body.SlotIndex].RisingRockTimer = WrappingCountdown.Start(120);
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(496);
                _access.RequestKraidMusic(MusicRequest.SelectTrack(5));
                return;

            case KraidAiFunction.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                CountdownStep slowRockStep = state.Parts[body.SlotIndex].RisingRockTimer.Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = slowRockStep.Remaining;
                if (slowRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery8Frames;
                    state.Parts[body.SlotIndex].RisingRockTimer = WrappingCountdown.Start(96);
                }
                else if (state.Parts[body.SlotIndex].RisingRockTimer.IsOnMultipleOf(16))
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                CountdownStep fastRockStep = state.Parts[body.SlotIndex].RisingRockTimer.Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = fastRockStep.Remaining;
                if (fastRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseBody;
                    state.Parts[body.SlotIndex].FunctionTimer = WrappingCountdown.Start(288);
                }
                else if (state.Parts[body.SlotIndex].RisingRockTimer.IsOnMultipleOf(8))
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if (_earthquake.EarthquakeTimer.CyclePhase(1, 8) is 0 or 2)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? -1 : 1));
                AddSignedKraidVerticalDisplacement(body, -32768);
                if (CoordinateAxis.Difference(body.Position.Y.Whole, 457) < 0)
                {
                    body.Position = body.Position.WithWholeX(176);
                    // The rise's last step does not leave execution at the fight setup.
                    // That setup immediately installs the thinking function, seeds the random thinking
                    // timer and selects the roar list for the first attack.
                    state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
                    _head.SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    RoomEnemySlot foot = _parts.Foot;
                    state.Parts[foot.SlotIndex].Function = KraidAiFunction.FootFirstPhaseThinking;
                    state.Parts[foot.SlotIndex].FunctionTimer = WrappingCountdown.Start(300);
                    state.Parts[foot.SlotIndex].NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
                    _access.StartKraidPartProgram(_parts.Arm, KraidPartProgram.ArmNormal);
                }
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
            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathShowUpperBody:
            case KraidAiFunction.DeathShowLowerBody:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathHideUpperBody:
            case KraidAiFunction.DeathHideLowerBody:
            case KraidAiFunction.DeathWaitBeforeFadeIn1:
            case KraidAiFunction.DeathWaitBeforeFadeIn2:
            case KraidAiFunction.DeathWaitBeforeFadeIn3:
            case KraidAiFunction.DeathWaitBeforeFadeIn4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
            case KraidAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Kraid body function {state.Parts[body.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>
    /// Ports <c>RestrictSamusXPositionToFirstScreen</c>. It writes the integer X
    /// and previous X words only, so the camera's horizontal follow sees Samus's subsequent
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

    /// <summary>Adds signed vertical motion to the body position with fixed-point wraparound.</summary>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        int displacement)
    {
        uint position = body.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        body.Position = body.Position.WithY(FixedCoordinate.FromRaw(unchecked(position)));
    }
}
