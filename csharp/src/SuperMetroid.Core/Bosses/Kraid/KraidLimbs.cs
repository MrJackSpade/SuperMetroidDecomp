using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm and foot: their setup, animations, lunges, retreats and walks.</summary>
[RoomLifetime]
internal sealed class KraidLimbs
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes arena shake during growth, death, and heavy movement.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidLimbs(EnemyRoomBinding room, RoomEarthquake earthquake, KraidParts parts, KraidEncounterAccess access)
    {
        _room = room;
        _earthquake = earthquake;
        _parts = parts;
        _access = access;
    }

    /// <summary>Installs the arm part in its place with the rising-or-sinking animation.</summary>
    internal void InitializeKraidArm(RoomEnemyPlace arm)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(arm);
            return;
        }
        // The arm joins the encounter its body began, which it holds from here (#2218).
        KraidEnemyState state = KraidEntity.EncounterOf(_parts.Body);
        _access.AttachKraidPart(arm, state);
        arm.Shading = _parts.Body.Shading;
        _access.StartKraidPartAnimation(state, arm, KraidPartSegment.ArmRisingOrSinking);
    }

    /// <summary>Installs a foot part with its initial animation and shared body state.</summary>
    internal void InitializeKraidFoot(RoomEnemyPlace foot)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(foot);
            return;
        }
        // The foot joins the encounter its body began, which it holds from here (#2218).
        KraidEnemyState state = KraidEntity.EncounterOf(_parts.Body);
        _access.AttachKraidPart(foot, state);
        foot.Shading = _parts.Body.Shading;
        _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootInitial);
        state.FootState.Phase = KraidFootPhase.Resting;
    }

    /// <summary>
    /// Whether the arm swings normally (#1886): it plays its normal swing or that swing's pause, the animations from which it
    /// slows once Kraid is below half health. An arm already slow, rising or sinking, dying or preparing a lunge keeps its
    /// animation.
    /// </summary>
    private static bool ArmSwingsNormally(KraidEnemyState state) =>
        state.ArmState.Playback.PlayingSegment is KraidPartSegment.ArmNormal or KraidPartSegment.ArmNormalPause;

    /// <summary>
    /// Whether the arm has yet to reach its pause (#1886): it plays its normal swing and has not shown the swing's last
    /// frame. While that last frame shows, the arm already counts as paused, as it does through the pause and any later
    /// animation.
    /// </summary>
    private static bool ArmYetToPause(KraidEnemyState state)
    {
        AnimationPlayback<KraidPartSegment> arm = state.ArmState.Playback;
        return arm.PlayingSegment == KraidPartSegment.ArmNormal && !arm.HasShownPhaseEnd;
    }

    /// <summary>
    /// Whether the foot plays <paramref name="walk"/> and shows its last frame (#1886): a forward walk or lunge has finished
    /// and rests on it, and the backward walk has reached the frame from which it loops.
    /// </summary>
    private static bool FootShowsLastFrameOf(KraidFootState foot, KraidPartSegment walk) =>
        foot.Playback.HasShownLastBeatOf(walk);

    /// <summary>
    /// Runs a Kraid part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceKraidPartAnimation(RoomEnemyPlace part)
    {
        if (!part.AnimationClock.Expire())
            return;
        KraidPartState partState = part.RequireSpeciesState<KraidPartState>();
        AnimationPlayback<KraidPartSegment> playback = partState.Playback;
        EnemyAnimationPlayback.Play(part, KraidAnimations.Segments, ref playback, partState.Show,
            cue => ApplyKraidPartCue(partState.Kraid, cue) is { } started
                ? new AnimationContinuation<KraidPartSegment>(KraidAnimations.StartingWith(started), AnimationPlace.StartOfAnimation)
                : null);
        partState.Playback = playback;
    }

    /// <summary>Carries out one cue of a Kraid part animation.</summary>
    /// <returns>The slow arm animation when the speed cue switches the arm; otherwise null.</returns>
    private KraidPartSegment? ApplyKraidPartCue(KraidEnemyState state, KraidPartCue cue)
    {
        RoomEnemyPlace body = _parts.Body;
        switch (cue)
        {
            case KraidPartCue.RaiseBody:
                body.Position = body.Position.WithWholeY(body.Position.Y.Whole - 1);
                return null;
            case KraidPartCue.LowerBodyAndShake:
                body.Position = body.Position.WithWholeY(body.Position.Y.Whole + 1);
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), KraidArenaDefinitions.StepShakeUpdates);
                return null;
            case KraidPartCue.StepSound:
                _access.RequestKraidSound(KraidSoundDefinitions.Footstep);
                return null;
            case KraidPartCue.StepLeft:
                body.Position = body.Position.WithWholeX(body.Position.X.Whole - KraidArenaDefinitions.StepPixels);
                return null;
            case KraidPartCue.StepRight:
                body.Position = body.Position.WithWholeX(body.Position.X.Whole + KraidArenaDefinitions.StepPixels);
                return null;
            case KraidPartCue.SlowArmBelowHalfHealth:
                return SelectKraidArmSpeedAnimation(state);
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidPartCue)} {cue}.");
        }
    }

    /// <summary>Counts down the grown body's think delay before opening the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(KraidEnemyState state)
    {
        if (state.ThinkingTimer != 0 && --state.ThinkingTimer == 0)
        {
            state.BodyState.FightPhase = KraidFightPhase.MouthOpenReaction;
            state.HeadPlayback = KraidHeadAnimations.RoarAfterEntryFrame;
            state.BodyState.HeadTimer = KraidHeadAnimations.RoarEntryDuration;
        }
    }

    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemyPlace body, KraidEnemyState state)
    {
        if (body.Health >= state.SevenEighthsHealth)
            return false;

        state.BodyState.FightPhase = KraidFightPhase.HeadAnimationBeforeGrowth;
        state.BodyState.PhaseTimer = KraidArenaDefinitions.GrowthHeadAnimationUpdates;

        KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(state.HeadPlayback).Growth;
        state.HeadPlayback = KraidHeadAnimations.GrowthResumePlayback(resume);
        state.BodyState.HeadTimer = KraidHeadAnimations.GrowthResumeDelay(resume);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), KraidArenaDefinitions.GrowthShakeUpdates);

        RoomEnemyPlace foot = _parts.Foot;
        _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootInitial);
        state.FootState.Phase = KraidFootPhase.Resting;
        RoomEnemyPlace arm = _parts.Arm;
        _access.StartKraidPartAnimation(state, arm, KraidPartSegment.ArmNormal);
        foreach (RoomEnemyPlace lint in _parts.Lints)
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        return true;
    }

    /// <summary>
    /// Below half health the arm switches to its slow swing, unless its stored animation is
    /// already the slow one or a later animation.
    /// </summary>
    private KraidPartSegment? SelectKraidArmSpeedAnimation(KraidEnemyState state) =>
        _parts.Body.Health < state.HalfHealth && ArmSwingsNormally(state)
            ? KraidPartSegment.ArmSlow
            : null;

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    internal void RunKraidArmMain(RoomEnemyPlace arm, KraidEnemyState state, int cameraY)
    {
        RoomEnemyPlace body = _parts.Body;
        arm.Position = arm.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.ArmOffset);

        // The arm hides only when its anchor leaves the screen's gameplay
        // viewport. This is actor visibility, not background priority, so preserve the hide-drawing property.
        bool onScreen = arm.Position.Y.Whole >= cameraY &&
            arm.Position.Y.Whole - cameraY < ScreenGeometry.ScreenHeightPixels;
        arm.Presence = onScreen
            ? arm.Presence.Without(EnemyPresence.Hidden)
            : arm.Presence.With(EnemyPresence.Hidden);

        // While mouth reopens are pending, incrementing the timer here cancels the common
        // animation's later decrement and freezes the selected arm frame.
        if (state.PendingMouthReopens != 0)
            arm.AnimationClock.Extend(1);
    }

    /// <summary>Runs the foot's lunge, retreat, and grown-form walking phases.</summary>
    internal void RunKraidFootMain(RoomEnemyPlace foot, KraidFootState part, int cameraY)
    {
        KraidEnemyState state = part.Kraid;
        RoomEnemyPlace body = _parts.Body;
        foot.Position = foot.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.FootOffset);
        bool onScreen = foot.Position.Y.Whole >= cameraY &&
            foot.Position.Y.Whole - ScreenGeometry.ScreenHeightPixels < cameraY;
        foot.Presence = onScreen
            ? foot.Presence.Without(EnemyPresence.Hidden)
            : foot.Presence.With(EnemyPresence.Hidden);

        switch (part.Phase)
        {
            case KraidFootPhase.Resting:
                return;
            case KraidFootPhase.FirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                if (part.CountPhaseTimerDown())
                    part.Phase = KraidFootPhase.PrepareFirstPhaseLunge;
                return;
            case KraidFootPhase.PrepareFirstPhaseLunge:
                PrepareKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidFootPhase.FirstPhaseLunge:
                RunKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidFootPhase.WaitBeforeRetreat:
                if (part.CountPhaseTimerDown())
                {
                    part.Phase = KraidFootPhase.FirstPhaseRetreat;
                    _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootWalkingBackward);
                }
                return;
            case KraidFootPhase.WaitBeforeSecondPhase:
                if (part.CountPhaseTimerDown())
                    part.Phase = KraidFootPhase.SecondPhaseInitialize;
                return;
            case KraidFootPhase.FirstPhaseRetreat:
                RunKraidFirstPhaseRetreat(body, foot, part);
                return;
            case KraidFootPhase.SecondPhaseWalkToStart:
                RunKraidSecondPhaseWalkToStart(body, foot, part);
                return;
            case KraidFootPhase.SecondPhaseInitialize:
                SetKraidWalking(foot, part, targetX: KraidMovementChoices.StartingX, thinkTimer: KraidArenaDefinitions.SecondPhaseFirstThinkUpdates, direction: HorizontalDirection.Right);
                return;
            case KraidFootPhase.SecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, foot, part);
                return;
            case KraidFootPhase.SecondPhaseWalking when part.WalkDirection == HorizontalDirection.Right:
                RunKraidSecondPhaseWalkingRight(body, foot, part);
                return;
            case KraidFootPhase.SecondPhaseWalking:
                RunKraidSecondPhaseWalkingLeft(body, foot, part);
                return;
            default:
                throw new InvalidDataException($"Undefined {nameof(KraidFootPhase)} {part.Phase}.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemyPlace body, RoomEnemyPlace foot, KraidFootState part)
    {
        KraidEnemyState state = part.Kraid;
        if (TryBeginKraidGrowth(body, state))
            return;
        RoomEnemyPlace arm = _parts.Arm;
        if (ArmYetToPause(state))
            return;
        _access.StartKraidPartAnimation(state, arm, KraidPartSegment.ArmDyingOrPreparingToLunge);
        _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootLungeForward);
        part.Phase = KraidFootPhase.FirstPhaseLunge;
        part.PhaseTimer = 0;
    }

    /// <summary>Moves the small-form foot and body through the forward lunge.</summary>
    private void RunKraidFirstPhaseLunge(
        RoomEnemyPlace body,
        RoomEnemyPlace foot,
        KraidFootState part)
    {
        if (TryBeginKraidGrowth(body, part.Kraid))
            return;
        if (body.Position.X.Whole < KraidArenaDefinitions.LungeReachX)
            body.Position = body.Position.WithWholeX(KraidArenaDefinitions.LungeReachX);
        if (!FootShowsLastFrameOf(part, KraidPartSegment.FootLungeForward))
            return;
        if (body.Position.X.Whole == KraidArenaDefinitions.LungeReachX)
        {
            part.Phase = KraidFootPhase.WaitBeforeRetreat;
            part.PhaseTimer = 1;
            _access.SetKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootNeutral);
        }
        else
        {
            _access.SetKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootLungeForward);
        }
        foot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Returns the foot and body after the small-form lunge.</summary>
    private void RunKraidFirstPhaseRetreat(
        RoomEnemyPlace body,
        RoomEnemyPlace foot,
        KraidFootState part)
    {
        KraidEnemyState state = part.Kraid;
        if (TryBeginKraidGrowth(body, state))
            return;
        if (body.Position.X.Whole >= KraidArenaDefinitions.FirstPhaseX)
            body.Position = body.Position.WithWholeX(KraidArenaDefinitions.FirstPhaseX);
        if (!FootShowsLastFrameOf(part, KraidPartSegment.FootWalkingBackward))
            return;
        if (body.Position.X.Whole == KraidArenaDefinitions.FirstPhaseX)
        {
            RoomEnemyPlace arm = _parts.Arm;
            _access.StartKraidPartAnimation(state, arm, KraidPartSegment.ArmNormal);
            _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootNeutral);
            part.Phase = KraidFootPhase.FirstPhaseThinking;
            part.PhaseTimer = KraidArenaDefinitions.LungeDelayUpdates;
        }
        else
        {
            _access.StartKraidPartAnimation(state, foot, KraidPartSegment.FootWalkingBackward);
        }
    }

    /// <summary>Positions the grown-form foot before its walking cycle begins.</summary>
    private void RunKraidSecondPhaseWalkToStart(
        RoomEnemyPlace body,
        RoomEnemyPlace foot,
        KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (targetX >= body.Position.X.Whole)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (!FootShowsLastFrameOf(part, KraidPartSegment.FootWalkingBackward))
            return;
        part.Phase = KraidFootPhase.WaitBeforeSecondPhase;
        part.PhaseTimer = KraidArenaDefinitions.SecondPhaseStartDelayUpdates;
        _access.StartKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootNeutral);
    }

    /// <summary>Chooses the grown foot's next walking target and direction after its think timer ends.</summary>
    internal void RunKraidSecondPhaseThinking(
        RoomEnemyPlace body,
        RoomEnemyPlace foot,
        KraidFootState part)
    {
        if (--part.ThinkTimer != 0)
            return;

        (int targetX, int thinkTimer) = KraidMovementChoices.Select(body.Position.X.Whole, _access.ReadKraidRandomNumber());
        SetKraidWalking(foot, part, targetX, thinkTimer,
            targetX >= body.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left);
    }

    /// <summary>
    /// Stores a walk target and starts the foot's walking animation: walking right steps the foot backward, walking left
    /// steps it forward.
    /// </summary>
    private void SetKraidWalking(
        RoomEnemyPlace foot,
        KraidFootState part,
        int targetX,
        int thinkTimer,
        HorizontalDirection direction)
    {
        part.Kraid.WalkTargetX = targetX;
        part.ThinkTimer = thinkTimer;
        part.Phase = KraidFootPhase.SecondPhaseWalking;
        part.WalkDirection = direction;
        _access.StartKraidPartAnimation(part.Kraid, foot,
            direction == HorizontalDirection.Right ? KraidPartSegment.FootWalkingBackward : KraidPartSegment.FootWalkingForward);
    }

    /// <summary>Moves the body and foot toward the selected rightward target.</summary>
    private void RunKraidSecondPhaseWalkingRight(RoomEnemyPlace body, RoomEnemyPlace foot, KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (targetX >= body.Position.X.Whole)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (FootShowsLastFrameOf(part, KraidPartSegment.FootWalkingBackward))
        {
            part.Phase = KraidFootPhase.SecondPhaseThinking;
            _access.StartKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootNeutral);
        }
    }

    /// <summary>Moves the body and foot toward the selected leftward target.</summary>
    private void RunKraidSecondPhaseWalkingLeft(RoomEnemyPlace body, RoomEnemyPlace foot, KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX < body.Position.X.Whole)
        {
            if (FootShowsLastFrameOf(part, KraidPartSegment.FootWalkingForward))
            {
                _access.StartKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootWalkingForward);
            }
            return;
        }
        body.Position = body.Position.WithWholeX(targetX);
        if (FootShowsLastFrameOf(part, KraidPartSegment.FootWalkingForward))
        {
            part.Phase = KraidFootPhase.SecondPhaseThinking;
            _access.StartKraidPartAnimation(part.Kraid, foot, KraidPartSegment.FootNeutral);
        }
    }
}
