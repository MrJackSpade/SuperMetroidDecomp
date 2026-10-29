using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm and foot: their setup, programs, lunges, retreats and walks.</summary>
[RoomLifetime]
internal sealed class KraidLimbs
{
    /// <summary>Room scratch memory and services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidLimbs(EnemyRoomBinding room, RoomEarthquake earthquake, EnemySlotTable slotTable, KraidEncounterAccess access)
    {
        _room = room;
        _earthquake = earthquake;
        _slotTable = slotTable;
        _access = access;
    }

    /// <summary>Installs the arm part in its native slot with the rising-or-sinking program.</summary>
    internal void InitializeKraidArm(RoomEnemySlot arm)
    {
        KraidEnemyState state = _access.RequireKraidState(arm);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(arm);
            return;
        }
        KraidEncounterAccess.EnsureKraidSlot(arm, 1, "arm");
        KraidEncounterAccess.AttachKraidPart(arm, state);
        arm.Colouring = _slotTable.Slots[0].Colouring;
        state.Parts[arm.SlotIndex].Function = KraidAiFunction.NoOperation;
        _access.StartKraidPartProgram(arm, KraidPartProgram.ArmRisingOrSinking);
        state.Parts[arm.SlotIndex].NextFunction = KraidAiFunction.NoOperation;
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
        KraidEncounterAccess.EnsureKraidSlot(foot, 5, "foot");
        KraidEncounterAccess.AttachKraidPart(foot, state);
        foot.Colouring = _slotTable.Slots[0].Colouring;
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootInitial);
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.NoOperation;
        state.Parts[foot.SlotIndex].NextFunction = 0;
    }

    /// <summary>
    /// The program a part's stored cursor belongs to. A cursor resting on a fall-through already belongs to the next
    /// program, as the native instruction pointer then points into it.
    /// </summary>
    private KraidPartProgram KraidPartResidentProgram(RoomEnemySlot part) =>
        KraidAnimations.Programs.ResidentProgram(_access.RequireKraidState(part).Parts[part.SlotIndex].Cursor);

    /// <summary>
    /// Runs a Kraid part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceKraidPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        KraidPartState partState = _access.RequireKraidState(part).Parts[part.SlotIndex];
        AnimationProgramCursor<KraidPartProgram> cursor = partState.Cursor;
        EnemyProgramAnimation.Run(part, KraidAnimations.Programs, ref cursor, KraidPartFrameDefinitions.ShownFrames.Of,
            (cue, continuation) => ApplyKraidPartCue(cue));
        partState.Cursor = cursor;
    }

    /// <summary>Carries out one cue of a Kraid part program.</summary>
    /// <returns>The slow arm program when the speed cue switches the arm; otherwise null.</returns>
    private KraidPartProgram? ApplyKraidPartCue(KraidPartCue cue)
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        switch (cue)
        {
            case KraidPartCue.RaiseBody:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
                return null;
            case KraidPartCue.LowerBodyAndShake:
                body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
                _earthquake.EarthquakeType = 1;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(10);
                return null;
            case KraidPartCue.StepSound:
                _access.LastKraidSoundEffect = new KraidSoundRequest(KraidSoundDefinitions.Footstep);
                return null;
            case KraidPartCue.StepLeft:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 3)));
                return null;
            case KraidPartCue.StepRight:
                body.Position = body.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 3)));
                return null;
            case KraidPartCue.SlowArmBelowHalfHealth:
                return SelectKraidArmSpeedProgram();
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidPartCue)} {cue}.");
        }
    }

    /// <summary>Counts down the grown body's think delay before opening the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(RoomEnemySlot body, KraidEnemyState state)
    {
        if (!state.ThinkingTimer.IsZero && (state.ThinkingTimer = state.ThinkingTimer.Decrement().Remaining).IsZero)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.MouthOpenReaction;
            state.HeadProgram = KraidHeadAnimations.RoarAfterEntryFrame;
            state.Parts[body.SlotIndex].HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemySlot body, KraidEnemyState state)
    {
        if (body.Health >= state.HealthEighthThreshold(6))
            return false;

        state.Parts[body.SlotIndex].Function = KraidAiFunction.ProcessHeadInstructionAndTimer;
        state.Parts[body.SlotIndex].FunctionTimer = 180;
        state.Parts[0].NextFunction = KraidAiFunction.GrowReleaseCamera;

        KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(state.HeadProgram).Growth;
        state.HeadProgram = KraidHeadAnimations.GrowthResumeCursor(resume);
        state.Parts[body.SlotIndex].HeadTimer = checked((ushort)KraidHeadAnimations.GrowthResumeDelay(resume));
        _earthquake.EarthquakeType = 4;
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(340);

        RoomEnemySlot foot = _slotTable.Slots[5];
        _access.StartKraidPartProgram(foot, KraidPartProgram.FootInitial);
        state.Parts[foot.SlotIndex].Function = KraidAiFunction.NoOperation;
        RoomEnemySlot arm = _slotTable.Slots[1];
        _access.StartKraidPartProgram(arm, KraidPartProgram.ArmNormal);
        for (int lintSlot = 2; lintSlot <= 4; lintSlot++)
            _slotTable.Slots[lintSlot].Properties = _slotTable.Slots[lintSlot].Properties.With(EnemyProperties.Invisible);
        arm.Properties = arm.Properties.With(EnemyProperties.IgnoreSamusCollision);
        return true;
    }

    /// <summary>
    /// Ports <c>$A7:8A8F</c>: below half health the arm switches to its slow swing, unless its stored program is
    /// already the slow one or a later program.
    /// </summary>
    private KraidPartProgram? SelectKraidArmSpeedProgram()
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        RoomEnemySlot arm = _slotTable.Slots[1];
        int halfHealth = _access.RequireKraidState(body).HealthEighthThreshold(3);
        return body.Health < halfHealth && KraidFunctionGroups.BeforeArmSlow.Contains(KraidPartResidentProgram(arm))
            ? KraidPartProgram.ArmSlow
            : null;
    }

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    internal void RunKraidArmMain(RoomEnemySlot arm, int cameraY)
    {
        KraidEnemyState state = _access.RequireKraidState(arm);
        RoomEnemySlot body = _slotTable.Slots[0];
        arm.Position = arm.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole - 44))));

        // `$B7D3-$B7F0` hides only the arm when its anchor leaves the 224-line gameplay
        // viewport. This is actor visibility, not BG priority, so preserve property $0100.
        bool onScreen = unchecked((short)(arm.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(arm.Position.Y.Whole - cameraY - 224)) < 0;
        arm.Properties = onScreen
            ? arm.Properties.Without(EnemyProperties.Invisible)
            : arm.Properties.With(EnemyProperties.Invisible);

        // While mouth reopens are pending, incrementing the timer here cancels the common
        // interpreter's later decrement and freezes the selected arm map.
        if (state.PendingMouthReopens != 0)
            arm.InstructionTimer = arm.InstructionTimer.AddFrames(1);
    }

    /// <summary>Dispatches the foot's lunge, retreat, and grown-form walking functions.</summary>
    internal void RunKraidFootMain(RoomEnemySlot foot, int cameraY)
    {
        KraidEnemyState state = _access.RequireKraidState(foot);
        RoomEnemySlot body = _slotTable.Slots[0];
        foot.Position = foot.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 100))));
        bool onScreen = unchecked((short)(foot.Position.Y.Whole - cameraY)) >= 0 &&
            unchecked((short)(foot.Position.Y.Whole - 224 - cameraY)) < 0;
        foot.Properties = onScreen
            ? foot.Properties.Without(EnemyProperties.Invisible)
            : foot.Properties.With(EnemyProperties.Invisible);

        KraidPartState part = state.Parts[5];
        switch (state.Parts[foot.SlotIndex].Function)
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
                    $"Kraid foot function {state.Parts[foot.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _slotTable.Slots[1];
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
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        if (unchecked((short)(body.Position.X.Whole - 92)) < 0)
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
        foot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Counts down a foot phase and installs its next function at zero.</summary>
    private void TickKraidFootTransitionTimer(RoomEnemySlot foot, KraidPartState part)
    {
        WrappingCountdown phaseTimer = WrappingCountdown.FromRaw(checked((ushort)_access.Part(foot).FunctionTimer));
        if (phaseTimer.IsZero)
            return;
        CountdownStep step = phaseTimer.Decrement();
        _access.Part(foot).FunctionTimer = step.Remaining.RawValue;
        if (step.IsZero)
        {
            _access.Part(foot).Function = part.NextFunction;
            _access.StartKraidPartProgram(foot, KraidPartProgram.FootWalkingBackward);
        }
    }

    /// <summary>Returns the foot and body after the small-form lunge.</summary>
    private void RunKraidFirstPhaseRetreat(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, _access.RequireKraidState(body)))
            return;
        if (unchecked((short)(body.Position.X.Whole - 176)) >= 0)
            body.Position = body.Position.WithWholeX(176);
        if (KraidFunctionGroups.BeforeFootWalkingBackwardLoop.Contains(KraidPartResidentProgram(foot)))
            return;
        if (body.Position.X.Whole == 176)
        {
            RoomEnemySlot arm = _slotTable.Slots[1];
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
        KraidPartState part)
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
        KraidPartState part)
    {
        part.ThinkTimer = part.ThinkTimer.Decrement().Remaining;
        if (!part.ThinkTimer.IsZero)
            return;

        (int targetX, int thinkTimer) = KraidMovementChoices.Select(body.Position.X.Whole, _access.ReadKraidRandomNumber());
        SetKraidWalking(foot, part, targetX, thinkTimer, rightward: unchecked((short)(targetX - body.Position.X.Whole)) >= 0);
    }

    /// <summary>
    /// Stores a walk target and starts the foot's walking animation: walking right steps the foot backward, walking left
    /// steps it forward.
    /// </summary>
    private void SetKraidWalking(
        RoomEnemySlot foot,
        KraidPartState part,
        int targetX,
        int thinkTimer,
        bool rightward)
    {
        _access.RequireKraidState(foot).WalkTarget = new Position16<RoomPixels>(checked((ushort)targetX), 0);
        part.ThinkTimer = WrappingCountdown.FromRaw(thinkTimer);
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
