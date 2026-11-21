using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's arm and foot: their setup, animations, lunges, retreats and walks.</summary>
[RoomLifetime]
internal sealed partial class KraidLimbs
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
        if (_parts.JoinEncounter(_room, arm, state => state.ArmState) is { } part)
            part.StartAnimation(KraidPartSegment.ArmRisingOrSinking);
    }

    /// <summary>Installs a foot part with its initial animation and shared body state.</summary>
    internal void InitializeKraidFoot(RoomEnemyPlace foot)
    {
        if (_parts.JoinEncounter(_room, foot, state => state.FootState) is not { } part)
            return;
        part.StartAnimation(KraidPartSegment.FootInitial);
        part.Phase = KraidFootPhase.Resting;
    }

    /// <summary>
    /// Runs a Kraid part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and the clock notes each newly shown frame.
    /// </summary>
    internal void AdvanceKraidPartAnimation(KraidAnimatedPartState partState)
    {
        RoomEnemyPlace part = partState.Place;
        if (!part.AnimationClock.Expire())
            return;
        AnimationPlayback<KraidPartSegment> playback = partState.Playback;
        EnemyAnimationPlayback.Play(part, KraidAnimations.Segments, ref playback, partState.Show,
            cue => ApplyKraidPartCue(partState.Kraid, cue) is { } started ? partState.ContinueWith(started) : null);
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
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), KraidArenaDefinitions.StepShakeUpdates);
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
            case KraidPartCue.ArmSwingBegins:
                state.ArmState.BeginSwing();
                return null;
            case KraidPartCue.ArmReachesPause:
                state.ArmState.ReachPause();
                return null;
            case KraidPartCue.WalkReachesEnd:
                state.FootState.ReachWalkEnd();
                return null;
            case KraidPartCue.WalkPlaysOn:
                state.FootState.PlayWalkOn();
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidPartCue)} {cue}.");
        }
    }

    /// <summary>Runs the grown body's thinking, and on its last update opens the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(KraidEnemyState state)
    {
        if (state.ThinkingWait.StepRunsOut())
        {
            state.BodyState.FightPhase = KraidFightPhase.MouthOpenReaction;
            state.StartHeadAnimation(KraidHeadSegment.Roar);
        }
    }

    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemyPlace body, KraidEnemyState state)
    {
        if (body.Health >= state.SevenEighthsHealth)
            return false;

        state.BodyState.FightPhase = KraidFightPhase.HeadAnimationBeforeGrowth;
        state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.GrowthHeadAnimationUpdates);

        state.ResumeHeadRoar(KraidHeadAnimations.ReadsAt(state.HeadPlayback).Growth);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), KraidArenaDefinitions.GrowthShakeUpdates);
        state.FootState.StartAnimation(KraidPartSegment.FootInitial);
        state.FootState.Phase = KraidFootPhase.Resting;
        RoomEnemyPlace arm = _parts.Arm;
        state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
        foreach (RoomEnemyPlace lint in _parts.Lints)
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        return true;
    }

    /// <summary>
    /// Below half health the arm switches to its slow swing, but only while it swings normally.
    /// </summary>
    private KraidPartSegment? SelectKraidArmSpeedAnimation(KraidEnemyState state) =>
        _parts.Body.Health < state.HalfHealth && state.ArmState.SwingsNormally
            ? KraidPartSegment.ArmSlow
            : null;

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    internal void RunKraidArmMain(RoomEnemyPlace arm, KraidEnemyState state, int cameraY)
    {
        RoomEnemyPlace body = _parts.Body;
        arm.Position = arm.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.ArmOffset);

        ShowWithinGameplayView(arm, cameraY);

        // While mouth reopens are pending the arm's swing waits on the frame it shows, its hold kept, and runs on from there
        // once none is pending.
        if (state.PendingMouthReopens != 0)
            arm.AnimationClock.Suspend();
        else
            arm.AnimationClock.Resume();
    }

    /// <summary>
    /// Hides <paramref name="limb"/> only while its anchor lies outside the screen's gameplay view below
    /// <paramref name="cameraY"/> (#2557): it is hidden, not drawn behind anything. The arm and the foot are shown so.
    /// </summary>
    private static void ShowWithinGameplayView(RoomEnemyPlace limb, int cameraY)
    {
        bool onScreen = limb.Position.Y.Whole >= cameraY &&
            limb.Position.Y.Whole - cameraY < ScreenGeometry.ScreenHeightPixels;
        limb.Presence = onScreen
            ? limb.Presence.Without(EnemyPresence.Hidden)
            : limb.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the foot's lunge, retreat, and grown-form walking phases.</summary>
    internal void RunKraidFootMain(RoomEnemyPlace foot, KraidFootState part, int cameraY)
    {
        KraidEnemyState state = part.Kraid;
        RoomEnemyPlace body = _parts.Body;
        foot.Position = foot.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.FootOffset);
        ShowWithinGameplayView(foot, cameraY);

        switch (part.Phase)
        {
            case KraidFootPhase.Resting:
                return;
            case KraidFootPhase.FirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                if (part.PhaseWait.StepRunsOut())
                    part.Phase = KraidFootPhase.PrepareFirstPhaseLunge;
                return;
            case KraidFootPhase.PrepareFirstPhaseLunge:
                PrepareKraidFirstPhaseLunge(body, part);
                return;
            case KraidFootPhase.FirstPhaseLunge:
                RunKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidFootPhase.WaitBeforeRetreat:
                if (part.PhaseWait.StepRunsOut())
                {
                    part.Phase = KraidFootPhase.FirstPhaseRetreat;
                    state.FootState.StartAnimation(KraidPartSegment.FootWalkingBackward);
                }
                return;
            case KraidFootPhase.WaitBeforeSecondPhase:
                if (part.PhaseWait.StepRunsOut())
                    part.Phase = KraidFootPhase.SecondPhaseInitialize;
                return;
            case KraidFootPhase.FirstPhaseRetreat:
                RunKraidFirstPhaseRetreat(body, part);
                return;
            case KraidFootPhase.SecondPhaseWalkToStart:
                RunKraidSecondPhaseWalkToStart(body, part);
                return;
            case KraidFootPhase.SecondPhaseInitialize:
                SetKraidWalking(part, targetX: KraidMovementChoices.StartingX, thinkUpdates: KraidArenaDefinitions.SecondPhaseFirstThinkUpdates, direction: HorizontalDirection.Right);
                return;
            case KraidFootPhase.SecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, part);
                return;
            case KraidFootPhase.SecondPhaseWalkingRight:
                RunKraidSecondPhaseWalkingRight(body, part);
                return;
            case KraidFootPhase.SecondPhaseWalkingLeft:
                RunKraidSecondPhaseWalkingLeft(body, part);
                return;
            default:
                throw new InvalidDataException($"Undefined {nameof(KraidFootPhase)} {part.Phase}.");
        }
    }

    /// <summary>Waits for the arm before starting the small-form foot lunge, or begins growth.</summary>
    private void PrepareKraidFirstPhaseLunge(RoomEnemyPlace body, KraidFootState part)
    {
        KraidEnemyState state = part.Kraid;
        if (TryBeginKraidGrowth(body, state))
            return;
        if (state.ArmState.YetToPause)
            return;
        state.ArmState.StartAnimation(KraidPartSegment.ArmDyingOrPreparingToLunge);
        state.FootState.StartAnimation(KraidPartSegment.FootLungeForward);
        part.Phase = KraidFootPhase.FirstPhaseLunge;
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
        if (!part.ShowsWalkEnd)
            return;
        if (body.Position.X.Whole == KraidArenaDefinitions.LungeReachX)
        {
            part.Phase = KraidFootPhase.WaitBeforeRetreat;
            part.PhaseWait.LastFor(KraidArenaDefinitions.LungeReachPauseUpdates);
            part.SetAnimation(KraidPartSegment.FootNeutral);
        }
        else
        {
            part.SetAnimation(KraidPartSegment.FootLungeForward);
        }
        foot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Returns the foot and body after the small-form lunge.</summary>
    private void RunKraidFirstPhaseRetreat(
        RoomEnemyPlace body,
        KraidFootState part)
    {
        KraidEnemyState state = part.Kraid;
        if (TryBeginKraidGrowth(body, state))
            return;
        if (body.Position.X.Whole >= KraidArenaDefinitions.FirstPhaseX)
            body.Position = body.Position.WithWholeX(KraidArenaDefinitions.FirstPhaseX);
        if (!part.ShowsWalkEnd)
            return;
        if (body.Position.X.Whole == KraidArenaDefinitions.FirstPhaseX)
        {
            state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
            state.FootState.StartAnimation(KraidPartSegment.FootNeutral);
            part.Phase = KraidFootPhase.FirstPhaseThinking;
            part.PhaseWait.LastFor(KraidArenaDefinitions.LungeDelayUpdates);
        }
        else
        {
            state.FootState.StartAnimation(KraidPartSegment.FootWalkingBackward);
        }
    }

    /// <summary>Positions the grown-form foot before its walking cycle begins.</summary>
    private static void RunKraidSecondPhaseWalkToStart(
        RoomEnemyPlace body,
        KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (targetX >= body.Position.X.Whole)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (!part.ShowsWalkEnd)
            return;
        part.Phase = KraidFootPhase.WaitBeforeSecondPhase;
        part.PhaseWait.LastFor(KraidArenaDefinitions.SecondPhaseStartDelayUpdates);
        part.StartAnimation(KraidPartSegment.FootNeutral);
    }

    /// <summary>Chooses the grown foot's next walking target and direction on the last update of its thinking.</summary>
    internal void RunKraidSecondPhaseThinking(
        RoomEnemyPlace body,
        KraidFootState part)
    {
        if (part.ThinkWait.StepKeepsRunning())
            return;

        (int targetX, int thinkUpdates) = KraidMovementChoices.Select(body.Position.X.Whole, _access.ReadKraidRandomNumber());
        SetKraidWalking(part, targetX, thinkUpdates,
            targetX >= body.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left);
    }

    /// <summary>
    /// Stores a walk target and the thinking that follows the walk, and starts the foot's walking animation: walking right
    /// steps the foot backward, walking left steps it forward.
    /// </summary>
    private static void SetKraidWalking(
        KraidFootState part,
        int targetX,
        int thinkUpdates,
        HorizontalDirection direction)
    {
        part.Kraid.WalkTargetX = targetX;
        part.ThinkWait.LastFor(thinkUpdates);
        switch (direction)
        {
            case HorizontalDirection.Right:
                part.Phase = KraidFootPhase.SecondPhaseWalkingRight;
                part.StartAnimation(KraidPartSegment.FootWalkingBackward);
                break;
            case HorizontalDirection.Left:
                part.Phase = KraidFootPhase.SecondPhaseWalkingLeft;
                part.StartAnimation(KraidPartSegment.FootWalkingForward);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined walk direction.");
        }
    }

    /// <summary>Moves the body and foot toward the selected rightward target.</summary>
    private static void RunKraidSecondPhaseWalkingRight(RoomEnemyPlace body, KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX != body.Position.X.Whole)
        {
            if (targetX >= body.Position.X.Whole)
                return;
            body.Position = body.Position.WithWholeX(targetX);
        }
        if (part.ShowsWalkEnd)
        {
            part.Phase = KraidFootPhase.SecondPhaseThinking;
            part.StartAnimation(KraidPartSegment.FootNeutral);
        }
    }

    /// <summary>Moves the body and foot toward the selected leftward target.</summary>
    private static void RunKraidSecondPhaseWalkingLeft(RoomEnemyPlace body, KraidFootState part)
    {
        int targetX = part.Kraid.WalkTargetX;
        if (targetX < body.Position.X.Whole)
        {
            if (part.ShowsWalkEnd)
            {
                part.StartAnimation(KraidPartSegment.FootWalkingForward);
            }
            return;
        }
        body.Position = body.Position.WithWholeX(targetX);
        if (part.ShowsWalkEnd)
        {
            part.Phase = KraidFootPhase.SecondPhaseThinking;
            part.StartAnimation(KraidPartSegment.FootNeutral);
        }
    }
}
