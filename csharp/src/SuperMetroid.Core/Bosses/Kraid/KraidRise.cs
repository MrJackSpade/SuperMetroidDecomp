using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;

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
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Kraid palettes, background tiles, and OAM presentation.</summary>
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
        _slotTable = dependencies.SlotTable;
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
                $"Kraid body requires native slot zero, not slot {body.SlotIndex}.");
        }

        _access.Kraid = new KraidEnemyState();
        KraidEnemyState state = _access.Kraid;
        KraidEncounterAccess.AttachKraidPart(body, state);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            // `$A7:A959` installs the dead-room BG palette before the shared part initializer
            // marks every physical actor invisible/deleted/non-interactive. Its two calls
            // immediately before that initializer are not cosmetic bookkeeping: they
            // restore the defeated arena on every room load so the broken ceiling and
            // removed floor spikes cannot return when the player exits and re-enters.
            LoadKraidColorBand(KraidPaletteSource.RoomBackdrop, _presentation.BossColours().KraidRoomBackdrop);
            state.BodyAppearance = KraidBodyAppearance.Hidden;
            ShowKraidRoomBackdrop();
            _frameResults.KraidPlms.AddRange(KraidPlmDefinitions.DefeatedRoom);
            // The body owns `$A7:C715-$C815`: a defeated room's body starts at hiding his body layer's halves and
            // the death's four-update wait. The other seven physical records take their own dead initializer and
            // remain deleted as on cartridge.
            state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathHideUpperBody;
            return;
        }

        // $A7:A9E4-A9E7.
        _zebetiteBehavior.CameraDistanceIndex = CameraDistanceMode.BossTracking;
        ApplyKraidScrolls(layout: KraidScrollLayout.Initial);
        state.EjectionLimit = new Position16<RoomPixels>(0, 324);
        state.InitialHealth = body.Health;

        // `$A7:AAC6` readies his body, whose halves rise, head, growth and death functions show on independently
        // timed updates.
        state.InitializeBodyLayer();
        state.HurtFrame = 0;
        state.HurtFrameTimer = WrappingCountdown.Start(0);

        body.Position = body.Position.WithWhole(new(176, 592));
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].Function = KraidAiFunction.RestrictSamusToFirstScreen;
        state.Parts[body.SlotIndex].FunctionTimer = 300;
        state.Parts[body.SlotIndex].HeadTimer = 64;
        state.Parts[0].NextFunction = KraidAiFunction.RaiseKraidThroughFloor;

        // Background palette three's target colors are written at CGRAM palette eleven in
        // native target-palette storage. The software renderer exposes that buffer directly.
        LoadKraidColorBand(KraidPaletteSource.InitialTarget, _presentation.BossColours().KraidSprites);
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
    /// <c>$A7:AD3A</c>: brings Kraid's grown body in front of the room's sprites before his second phase. Only a risen
    /// body grows; a defeated room's hidden body never reaches growth.
    /// </summary>
    /// <exception cref="InvalidOperationException">His body is not behind the sprites.</exception>
    private static void BringKraidBodyInFront(KraidEnemyState state) =>
        state.BodyAppearance = state.BodyAppearance == KraidBodyAppearance.BehindSprites
            ? KraidBodyAppearance.InFrontOfSprites
            : throw new InvalidOperationException($"Kraid's growth reached a body that appears {state.BodyAppearance}.");

    /// <summary>Copies one Kraid palette source band to its CGRAM destination.</summary>
    internal void LoadKraidColorBand(KraidPaletteSource source, ColourRun destination)
    {
        for (int color = 0; color < KraidPalettes.BandColors; color++)
            destination.Write(color, _background.ReadKraidColor(source, color));
    }

    /// <summary>Restores the grown-body thinking state after an attack.</summary>
    private void SetupKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.SecondPhaseThinking;
        _slotTable.Slots[2].Position = _slotTable.Slots[2].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 20)));
        _slotTable.Slots[3].Position = _slotTable.Slots[3].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 46)));
        _slotTable.Slots[4].Position = _slotTable.Slots[4].Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 112)));
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
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? 1 : -1))));
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                if ((body.Position.Y.Whole & 3) == 0 && state.Parts[body.SlotIndex].CeilingRock < KraidCeilingRocks.Count)
                {
                    int rock = state.Parts[body.SlotIndex].CeilingRock;
                    if (_background.SpawnKraidCeilingRock(KraidCeilingRocks.X(rock)))
                        state.CeilingRockSpawnCount++;
                    _frameResults.KraidPlms.Add(KraidPlmDefinitions.GrowthCeiling[rock]);
                    state.Parts[body.SlotIndex].CeilingRock = rock + 1;
                }
                if (unchecked((short)(body.Position.Y.Whole - 296)) < 0)
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowBringBodyInFront;
                return;

            case KraidAiFunction.GrowBringBodyInFront:
                BringKraidBodyInFront(state);
                _slotTable.Slots[1].Properties = _slotTable.Slots[1].Properties.Without(
                    EnemyProperties.IgnoreSamusCollision);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowShowLowerBodyInFront;
                state.ShowUpperBody();
                return;

            case KraidAiFunction.GrowShowLowerBodyInFront:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowDrawRoomBackground;
                RoomEnemySlot foot = _slotTable.Slots[5];
                _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
                for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
                {
                    _access.SetKraidPartProgram(_slotTable.Slots[lintSlot], KraidPartProgram.LintPostGrowth);
                    _slotTable.Slots[lintSlot].Frame = KraidPartFrameDefinitions.ShownFrames.Of(new KraidPartFrame(KraidPartFrameKind.Lint, 1));
                }
                state.ShowLowerBody();
                return;

            case KraidAiFunction.GrowDrawRoomBackground:
                state.Parts[body.SlotIndex].Function = KraidAiFunction.GrowFadeInRoomBackground;
                // The original game clears the value it kept the ceiling rocks in, which is the function timer again.
                state.Parts[body.SlotIndex].FunctionTimer = 0;
                // $A7:ADB0 DrawKraidsRoomBackground zeroes the shared numerator.
                _presentation.GradualColorChange.Reset();
                // `$A7:AD9A` queues this character upload on the same frame that it
                // initializes palette-six fading. Without it, BG1's authored repeating
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
            int slot = KraidLintInitializationDefinitions.Slot[part];
            RoomEnemySlot lint = _slotTable.Slots[slot];
            state.Parts[lint.SlotIndex].FunctionTimer = KraidLints.InitialDelay(part);
            state.Parts[lint.SlotIndex].Function = KraidAiFunction.AlignPartToKraid;
            state.Parts[slot].NextFunction = KraidAiFunction.LintProduce;
            state.Parts[lint.SlotIndex].LintTravel = 0;
        }
        for (int nailIndex = 0; nailIndex < 2; nailIndex++)
        {
            RoomEnemySlot nail = _slotTable.Slots[6 + nailIndex];
            state.Parts[6 + nailIndex].NextFunction = KraidAiFunction.FingernailInitialize;
            state.Parts[nail.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
            state.Parts[nail.SlotIndex].FunctionTimer = unchecked((ushort)(64 + nailIndex * 64));
        }
        state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
        state.WalkTarget = new Position16<RoomPixels>(288, 0);
        RoomEnemySlot foot = _slotTable.Slots[5];
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

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(rock, kind, graphics: SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(BossColourDefinitions.KraidSprites)));
        rock.StartAnimation(new KraidRockAnimation(kind == RoomEnemyProjectileKind.KraidRisingRockRight
            ? KraidRockProgram.RisingRightPose
            : KraidRockProgram.SharedPose));
        // $86:9D17 tests bit zero: clear complements the offset to the left.
        int randomOffset = random & 63;
        if ((random & 1) == 0)
            randomOffset = unchecked((short)~randomOffset);
        rock.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(body.Position.X.Whole + randomOffset)), 432));
        rock.Velocity = new(new SignedEightEight(unchecked((ushort)(random & 1008))), SignedEightEight.FromSignedRaw(-1280));
        rock.Graphics = SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(BossColourDefinitions.KraidSprites));
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
                KraidEncounterAccess.TickKraidFunctionTimer(state.Parts[0]);
                return;

            case KraidAiFunction.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseShowLowerBody;
                state.ShowUpperBody();
                return;

            case KraidAiFunction.RaiseShowLowerBody:
                RestrictSamusToKraidFirstScreen(samus);
                state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery16Frames;
                state.Parts[body.SlotIndex].RisingRockTimer = 120;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(496);
                _access.RequestKraidMusic(MusicRequest.SelectTrack(5));
                state.ShowLowerBody();
                return;

            case KraidAiFunction.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                CountdownStep slowRockStep = WrappingCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].RisingRockTimer)).Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = slowRockStep.Remaining.RawValue;
                if (slowRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseRocksEvery8Frames;
                    state.Parts[body.SlotIndex].RisingRockTimer = 96;
                }
                else if ((state.Parts[body.SlotIndex].RisingRockTimer & 15) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                CountdownStep fastRockStep = WrappingCountdown.FromRaw(checked((ushort)state.Parts[body.SlotIndex].RisingRockTimer)).Decrement();
                state.Parts[body.SlotIndex].RisingRockTimer = fastRockStep.Remaining.RawValue;
                if (fastRockStep.IsZero)
                {
                    state.Parts[body.SlotIndex].Function = KraidAiFunction.RaiseBody;
                    state.Parts[body.SlotIndex].FunctionTimer = 288;
                }
                else if ((state.Parts[body.SlotIndex].RisingRockTimer & 7) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if ((_earthquake.EarthquakeTimer.RawValue & 5) == 0)
                    RequestKraidRisingRock(body, state);
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole +
                    ((body.Position.Y.Whole & 2) == 0 ? -1 : 1))));
                AddSignedKraidVerticalDisplacement(body, -32768);
                if (unchecked((short)(body.Position.Y.Whole - 457)) < 0)
                {
                    body.Position = body.Position.WithWholeX(176);
                    // `$C97B` does not leave execution at setup routine `$ADE9`.
                    // That JSR immediately installs `$AEA4`, seeds the random thinking
                    // timer and selects roar list entry `$96DA` for the first attack.
                    state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
                    _head.SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    RoomEnemySlot foot = _slotTable.Slots[5];
                    state.Parts[foot.SlotIndex].Function = KraidAiFunction.FootFirstPhaseThinking;
                    state.Parts[foot.SlotIndex].FunctionTimer = 300;
                    state.Parts[5].NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
                    _access.StartKraidPartProgram(_slotTable.Slots[1], KraidPartProgram.ArmNormal);
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
    /// Ports <c>RestrictSamusXPositionToFirstScreen</c> ($A7:C9EE). It writes the integer X
    /// and previous X words only, so the camera's $90:95A0 compare sees Samus's subsequent
    /// movement away from $100 as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState? samus)
    {
        if (samus is not null && unchecked((short)(samus.Position.X.Whole - 256)) >= 0)
        {
            samus.Position = samus.Position.WithWholeX(256);
            samus.WritePreviousXPosition(256);
        }
    }

    /// <summary>Adds native signed vertical motion to the body position with fixed-point wraparound.</summary>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        int displacement)
    {
        uint position = body.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        body.Position = body.Position.WithY(FixedCoordinate.FromRaw(unchecked(position)));
    }
}
