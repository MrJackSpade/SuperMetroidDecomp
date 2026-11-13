using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm and foot: their setup, programs, lunges, retreats and walks.</summary>
[RoomLifetime]
internal sealed class KraidLimbs
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidLimbs(EnemyRoomBinding room, RoomEarthquake earthquake, KraidParts parts, KraidEncounterAccess access)
    {
        _room = room;
        _earthquake = earthquake;
        _parts = parts;
        _access = access;
    }

    /// <summary>Installs the arm part in its slot with the rising-or-sinking program.</summary>
    internal void InitializeKraidArm(RoomEnemySlot arm)
    {
        KraidEnemyState state = _access.RequireKraidState(arm);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(arm);
            return;
        }
        KraidParts.Require(arm, _parts.Arm, "arm");
        _access.AttachKraidPart(arm, state);
        arm.Shading = _parts.Body.Shading;
        state.ArmState.Function = KraidAiFunction.NoOperation;
        _access.StartKraidPartProgram(arm, KraidPartProgram.ArmRisingOrSinking);
        state.ArmState.NextFunction = KraidAiFunction.NoOperation;
    }

    /// <summary>Installs a foot part with its initial animation and shared body state.</summary>
    internal void InitializeKraidFoot(RoomEnemySlot foot)
    {
        KraidEnemyState state = _access.RequireKraidState(foot);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(foot);
            return;
        }
        KraidParts.Require(foot, _parts.Foot, "foot");
        _access.AttachKraidPart(foot, state);
        foot.Shading = _parts.Body.Shading;
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootInitial);
        state.FootState.Function = KraidAiFunction.NoOperation;
        state.FootState.NextFunction = 0;
    }

    /// <summary>
    /// The segment a part's animation rests in. Resting after the last beat of a list that runs on into the next, it
    /// already rests in the next, as the part's animation position then lies inside it.
    /// </summary>
    private KraidPartProgram KraidPartResidentProgram(RoomEnemySlot part) =>
        AnimationResidence.ResidentSegment(KraidAnimations.Segments, KraidAnimations.StartingAt, KraidAnimations.RunsOnInto,
            _access.Part(part).Playback);

    /// <summary>
    /// Runs a Kraid part's program where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceKraidPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        KraidPartState partState = _access.Part(part);
        AnimationPlayback<KraidPartProgram> playback = partState.Playback;
        EnemyProgramAnimation.Play(part, KraidAnimations.Segments, ref playback, KraidPartFrameDefinitions.ShownFrames.Of,
            cue => ApplyKraidPartCue(cue) is { } started
                ? new AnimationContinuation<KraidPartProgram>(KraidAnimations.StartingWith(started), AnimationPlace.StartOfAnimation)
                : null);
        partState.Playback = playback;
    }

    /// <summary>Carries out one cue of a Kraid part program.</summary>
    /// <returns>The slow arm program when the speed cue switches the arm; otherwise null.</returns>
    private KraidPartProgram? ApplyKraidPartCue(KraidPartCue cue)
    {
        RoomEnemySlot body = _parts.Body;
        switch (cue)
        {
            case KraidPartCue.RaiseBody:
                body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 1);
                return null;
            case KraidPartCue.LowerBodyAndShake:
                body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 10);
                return null;
            case KraidPartCue.StepSound:
                _access.RequestKraidSound(KraidSoundDefinitions.Footstep);
                return null;
            case KraidPartCue.StepLeft:
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole - 3);
                return null;
            case KraidPartCue.StepRight:
                body.Position = body.Position.WithWrappedWholeX(body.Position.X.Whole + 3);
                return null;
            case KraidPartCue.SlowArmBelowHalfHealth:
                return SelectKraidArmSpeedProgram();
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidPartCue)} {cue}.");
        }
    }

    /// <summary>Counts down the grown body's think delay before opening the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(KraidEnemyState state)
    {
        if (state.ThinkingTimer != 0 && --state.ThinkingTimer == 0)
        {
            state.BodyState.Function = KraidAiFunction.MouthOpenReaction;
            state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
            state.BodyState.HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health >= state.HealthEighthThreshold(6))
            return false;

        state.BodyState.Function = KraidAiFunction.ProcessHeadInstructionAndTimer;
        state.BodyState.FunctionTimer = 180;
        state.BodyState.NextFunction = KraidAiFunction.GrowReleaseCamera;

        KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(state.HeadPlayback).Growth;
        state.HeadPlayback = KraidHeadAnimations.GrowthResumePlayback(resume);
        state.BodyState.HeadTimer = KraidHeadAnimations.GrowthResumeDelay(resume);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 340);

        RoomEnemySlot foot = _parts.Foot;
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootInitial);
        state.FootState.Function = KraidAiFunction.NoOperation;
        RoomEnemySlot arm = _parts.Arm;
        _access.StartKraidPartProgram(arm, KraidPartProgram.ArmNormal);
        foreach (RoomEnemySlot lint in _parts.Lints)
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        return true;
    }

    /// <summary>
    /// Below half health the arm switches to its slow swing, unless its stored program is
    /// already the slow one or a later program.
    /// </summary>
    private KraidPartProgram? SelectKraidArmSpeedProgram()
    {
        RoomEnemySlot body = _parts.Body;
        RoomEnemySlot arm = _parts.Arm;
        int halfHealth = _access.RequireKraidState(body).HealthEighthThreshold(3);
        return body.Health < halfHealth && KraidFunctionGroups.BeforeArmSlow.Contains(KraidPartResidentProgram(arm))
            ? KraidPartProgram.ArmSlow
            : null;
    }

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    internal void RunKraidArmMain(RoomEnemySlot arm, int cameraY)
    {
        KraidEnemyState state = _access.RequireKraidState(arm);
        RoomEnemySlot body = _parts.Body;
        arm.Position = arm.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole - 44)));

        // The arm hides only when its anchor leaves the 224-line gameplay
        // viewport. This is actor visibility, not background priority, so preserve the hide-drawing property.
        bool onScreen = CoordinateAxis.Difference(arm.Position.Y.Whole, cameraY) >= 0 &&
            CoordinateAxis.Difference(arm.Position.Y.Whole - cameraY, 224) < 0;
        arm.Presence = onScreen
            ? arm.Presence.Without(EnemyPresence.Hidden)
            : arm.Presence.With(EnemyPresence.Hidden);

        // While mouth reopens are pending, incrementing the timer here cancels the common
        // animation's later decrement and freezes the selected arm frame.
        if (state.PendingMouthReopens != 0)
            arm.FrameHold = arm.FrameHold.AddFrames(1);
    }

    /// <summary>Dispatches the foot's lunge, retreat, and grown-form walking functions.</summary>
    internal void RunKraidFootMain(RoomEnemySlot foot, int cameraY)
    {
        KraidEnemyState state = _access.RequireKraidState(foot);
        RoomEnemySlot body = _parts.Body;
        foot.Position = foot.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 100)));
        bool onScreen = CoordinateAxis.Difference(foot.Position.Y.Whole, cameraY) >= 0 &&
            CoordinateAxis.Difference(foot.Position.Y.Whole - 224, cameraY) < 0;
        foot.Presence = onScreen
            ? foot.Presence.Without(EnemyPresence.Hidden)
            : foot.Presence.With(EnemyPresence.Hidden);

        KraidFootState part = state.FootState;
        switch (state.FootState.Function)
        {
            case KraidAiFunction.NoOperation:
                return;
            case KraidAiFunction.FootFirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                KraidEncounterAccess.TickKraidFunctionTimer(part);
                return;
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
                PrepareKraidFirstPhaseLunge(body, foot);
                return;
            case KraidAiFunction.FootFirstPhaseLunge:
                RunKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
                TickKraidFootTransitionTimer(foot, part);
                return;
            case KraidAiFunction.HandleFunctionTimer:
                KraidEncounterAccess.TickKraidFunctionTimer(part);
                return;
            case KraidAiFunction.FootFirstPhaseRetreat:
                RunKraidFirstPhaseRetreat(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkToStart:
                RunKraidSecondPhaseWalkToStart(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseInitialize:
                SetKraidWalking(foot, part, targetX: 352, thinkTimer: 180, rightward: true);
                return;
            case KraidAiFunction.FootSecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingRight:
                RunKraidSecondPhaseWalkingRight(body, foot);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
                RunKraidSecondPhaseWalkingLeft(body, foot);
                return;
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
            case KraidAiFunction.ProcessHeadInstructionAndTimer:
            case KraidAiFunction.MainAttackWithMouthOpen:
            case KraidAiFunction.FingernailInitialize:
            case KraidAiFunction.FingernailFire:
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
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseShowLowerBody:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Kraid foot function {state.FootState.Function} is not translated.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _parts.Arm;
        if (KraidFunctionGroups.BeforeArmNormalPause.Contains(KraidPartResidentProgram(arm)))
            return;
        _access.StartKraidPartProgram(arm, KraidPartProgram.ArmDyingOrPreparingToLunge);
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootLungeForward);
        _access.Part(foot).Function = KraidAiFunction.FootFirstPhaseLunge;
        _access.Part(foot).FunctionTimer = 0;
    }

    /// <summary>Moves the small-form foot and body through the forward lunge.</summary>
    private void RunKraidFirstPhaseLunge(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidFootState part)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        if (CoordinateAxis.Difference(body.Position.X.Whole, 92) < 0)
            body.Position = body.Position.WithWholeX(92);
        if (KraidPartResidentProgram(foot) != KraidPartProgram.FootLungeForwardFinished)
            return;
        if (body.Position.X.Whole == 92)
        {
            part.NextFunction = KraidAiFunction.FootFirstPhaseRetreat;
            _access.Part(foot).Function = KraidAiFunction.DecrementFunctionTimerAndStartWalk;
            _access.Part(foot).FunctionTimer = 1;
            _access.SetKraidPartProgram(foot, KraidPartProgram.FootNeutral);
        }
        else
        {
            _access.SetKraidPartProgram(foot, KraidPartProgram.FootLungeForward);
        }
        foot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>Counts down a foot phase and installs its next function at zero.</summary>
    private void TickKraidFootTransitionTimer(RoomEnemySlot foot, KraidFootState part)
    {
        KraidPartState footState = _access.Part(foot);
        if (footState.FunctionTimer == 0)
            return;
        if (--footState.FunctionTimer == 0)
        {
            _access.Part(foot).Function = part.NextFunction;
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
        }
    }

    /// <summary>Returns the foot and body after the small-form lunge.</summary>
    private void RunKraidFirstPhaseRetreat(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidFootState part)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        if (CoordinateAxis.Difference(body.Position.X.Whole, 176) >= 0)
            body.Position = body.Position.WithWholeX(176);
        if (KraidFunctionGroups.BeforeFootWalkingBackwardLoop.Contains(KraidPartResidentProgram(foot)))
            return;
        if (body.Position.X.Whole == 176)
        {
            RoomEnemySlot arm = _parts.Arm;
            _access.StartKraidPartProgram(arm, KraidPartProgram.ArmNormal);
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
            _access.Part(foot).Function = KraidAiFunction.FootFirstPhaseThinking;
            _access.Part(foot).FunctionTimer = 300;
            part.NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
        }
        else
        {
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
        }
    }

    /// <summary>Positions the grown-form foot before its walking cycle begins.</summary>
    private void RunKraidSecondPhaseWalkToStart(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidFootState part)
    {
        Position16<RoomPixels> target = _access.RequireKraidState(body).WalkTarget;
        if (target.X != body.Position.X.Whole)
        {
            if ((target - body.Position.Whole).X >= 0)
                return;
            body.Position = body.Position.WithWholeX(target.X);
        }
        if (KraidFunctionGroups.BeforeFootWalkingBackwardLoop.Contains(KraidPartResidentProgram(foot)))
            return;
        _access.Part(foot).Function = KraidAiFunction.HandleFunctionTimer;
        _access.Part(foot).FunctionTimer = 180;
        part.NextFunction = KraidAiFunction.FootSecondPhaseInitialize;
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
    }

    /// <summary>Chooses the grown foot's next walking target and direction after its think timer ends.</summary>
    internal void RunKraidSecondPhaseThinking(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidFootState part)
    {
        if (--part.ThinkTimer != 0)
            return;

        (int targetX, int thinkTimer) = KraidMovementChoices.Select(body.Position.X.Whole, _access.ReadKraidRandomNumber());
        SetKraidWalking(foot, part, targetX, thinkTimer, rightward: CoordinateAxis.Difference(targetX, body.Position.X.Whole) >= 0);
    }

    /// <summary>
    /// Stores a walk target and starts the foot's walking animation: walking right steps the foot backward, walking left
    /// steps it forward.
    /// </summary>
    private void SetKraidWalking(
        RoomEnemySlot foot,
        KraidFootState part,
        int targetX,
        int thinkTimer,
        bool rightward)
    {
        _access.RequireKraidState(foot).WalkTarget = new Position16<RoomPixels>(targetX, 0);
        part.ThinkTimer = thinkTimer;
        _access.Part(foot).Function = rightward ? KraidAiFunction.FootSecondPhaseWalkingRight : KraidAiFunction.FootSecondPhaseWalkingLeft;
        _access.StartKraidPartProgram(foot, rightward ? KraidPartProgram.FootWalkingBackward : KraidPartProgram.FootWalkingForward);
    }

    /// <summary>Moves the body and foot toward the selected rightward target.</summary>
    private void RunKraidSecondPhaseWalkingRight(RoomEnemySlot body, RoomEnemySlot foot)
    {
        Position16<RoomPixels> target = _access.RequireKraidState(body).WalkTarget;
        if (target.X != body.Position.X.Whole)
        {
            if ((target - body.Position.Whole).X >= 0)
                return;
            body.Position = body.Position.WithWholeX(target.X);
        }
        if (!KraidFunctionGroups.BeforeFootWalkingBackwardLoop.Contains(KraidPartResidentProgram(foot)))
        {
            _access.Part(foot).Function = KraidAiFunction.FootSecondPhaseThinking;
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
        }
    }

    /// <summary>Moves the body and foot toward the selected leftward target.</summary>
    private void RunKraidSecondPhaseWalkingLeft(RoomEnemySlot body, RoomEnemySlot foot)
    {
        Position16<RoomPixels> target = _access.RequireKraidState(body).WalkTarget;
        if ((target - body.Position.Whole).X < 0)
        {
            if (KraidPartResidentProgram(foot) == KraidPartProgram.FootWalkingForwardFinished)
            {
                _access.StartKraidPartProgram(foot, KraidPartProgram.FootWalkingForward);
            }
            return;
        }
        body.Position = body.Position.WithWholeX(target.X);
        if (KraidPartResidentProgram(foot) == KraidPartProgram.FootWalkingForwardFinished)
        {
            _access.Part(foot).Function = KraidAiFunction.FootSecondPhaseThinking;
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootNeutral);
        }
    }
}
