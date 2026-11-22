using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
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
    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidLimbs(EnemyRoomBinding room, RoomEarthquake earthquake, KraidEncounterAccess access)
    {
        _room = room;
        _earthquake = earthquake;
        _access = access;
    }

    /// <summary>Installs the arm part in its place with the rising-or-sinking animation.</summary>
    /// <returns>The encounter the arm joins, which it holds from its setup.</returns>
    internal KraidEnemyState InitializeKraidArm(RoomEnemyPlace arm, KraidEnemyState state)
    {
        // The arm joins the encounter its body began, which it holds from here (#2218).
        Ensure.NotNull(state);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(arm);
            return state;
        }
        arm.Shading = state.Parts.Body.Shading;
        state.ArmState.StartAnimation(KraidPartSegment.ArmRisingOrSinking);
        return state;
    }

    /// <summary>Installs a foot part with its initial animation and shared body state.</summary>
    /// <returns>The encounter the foot joins, which it holds from its setup.</returns>
    internal KraidEnemyState InitializeKraidFoot(RoomEnemyPlace foot, KraidEnemyState state)
    {
        // The foot joins the encounter its body began, which it holds from here (#2218).
        Ensure.NotNull(state);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(foot);
            return state;
        }
        foot.Shading = state.Parts.Body.Shading;
        state.FootState.StartAnimation(KraidPartSegment.FootInitial);
        state.FootState.Phase = KraidFootPhase.Resting;
        return state;
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

    /// <summary>What <see cref="ApplyKraidPartCue"/> comes to for each <see cref="KraidPartCue"/> member, each member's own arm.</summary>
    private IdentityTable<KraidPartCue, Func<KraidEnemyState, RoomEnemyPlace, KraidPartSegment?>> ApplyKraidPartCueForKraidPartCue => field ??= new(new Dictionary<KraidPartCue, Func<KraidEnemyState, RoomEnemyPlace, KraidPartSegment?>>
    {
        [KraidPartCue.RaiseBody] = (state, body) =>
        {
            body.Position = body.Position.WithWholeY(body.Position.Y.Whole - 1);
            return null;
        },
        [KraidPartCue.LowerBodyAndShake] = (state, body) =>
        {
            body.Position = body.Position.WithWholeY(body.Position.Y.Whole + 1);
            _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), KraidArenaDefinitions.StepShakeUpdates);
            return null;
        },
        [KraidPartCue.StepSound] = (state, body) =>
        {
            _access.RequestKraidSound(KraidSoundDefinitions.Footstep);
            return null;
        },
        [KraidPartCue.StepLeft] = (state, body) =>
        {
            body.Position = body.Position.WithWholeX(body.Position.X.Whole - KraidArenaDefinitions.StepPixels);
            return null;
        },
        [KraidPartCue.StepRight] = (state, body) =>
        {
            body.Position = body.Position.WithWholeX(body.Position.X.Whole + KraidArenaDefinitions.StepPixels);
            return null;
        },
        [KraidPartCue.SlowArmBelowHalfHealth] = (state, body) => SelectKraidArmSpeedAnimation(state),
        [KraidPartCue.ArmSwingBegins] = (state, body) =>
        {
            state.ArmState.BeginSwing();
            return null;
        },
        [KraidPartCue.ArmReachesPause] = (state, body) =>
        {
            state.ArmState.ReachPause();
            return null;
        },
        [KraidPartCue.WalkReachesEnd] = (state, body) =>
        {
            state.FootState.ReachWalkEnd();
            return null;
        },
        [KraidPartCue.WalkPlaysOn] = (state, body) =>
        {
            state.FootState.PlayWalkOn();
            return null;
        },
    });

    /// <summary>Carries out one cue of a Kraid part animation.</summary>
    /// <returns>The slow arm animation when the speed cue switches the arm; otherwise null.</returns>
    private KraidPartSegment? ApplyKraidPartCue(KraidEnemyState state, KraidPartCue cue)
    {
        RoomEnemyPlace body = state.Parts.Body;
        return ApplyKraidPartCueForKraidPartCue[cue](state, body);
    }

    /// <summary>Runs the grown body's thinking, and on its last update opens the mouth for its next attack.</summary>
    internal static void RunKraidSecondPhaseThinking(KraidEnemyState state, KraidStageRun.Fighting fight)
    {
        if (state.ThinkingWait.StepRunsOut())
        {
            fight.Phase = KraidFightPhase.MouthOpenReaction;
            fight.Head.Start(KraidHeadSegment.Roar, state.HeadHold);
        }
    }

    /// <summary>Starts growth only after the small-form transition requirements are met.</summary>
    internal bool TryBeginKraidGrowth(RoomEnemyPlace body, KraidEnemyState state)
    {
        // His foot's first-phase lunges, which look for his growth, run only while he fights his first phase.
        if (body.Health >= state.SevenEighthsHealth || state.BodyState.Run is not KraidStageRun.Fighting fight)
            return false;

        fight.Phase = KraidFightPhase.HeadAnimationBeforeGrowth;
        state.BodyState.PhaseWait.LastFor(KraidArenaDefinitions.GrowthHeadAnimationUpdates);

        // His growth resumes the roar from where his head's animation stands.
        fight.Head.ResumeRoarForGrowth(state.HeadHold);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), KraidArenaDefinitions.GrowthShakeUpdates);
        state.FootState.StartAnimation(KraidPartSegment.FootInitial);
        state.FootState.Phase = KraidFootPhase.Resting;
        RoomEnemyPlace arm = state.Parts.Arm;
        state.ArmState.StartAnimation(KraidPartSegment.ArmNormal);
        foreach (RoomEnemyPlace lint in state.Parts.Lints)
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
        arm.Contact = arm.Contact.With(EnemyContact.IgnoresSamus);
        return true;
    }

    /// <summary>
    /// Below half health the arm switches to its slow swing, but only while it swings normally.
    /// </summary>
    private static KraidPartSegment? SelectKraidArmSpeedAnimation(KraidEnemyState state) =>
        state.Parts.Body.Health < state.HalfHealth && state.ArmState.SwingsNormally
            ? KraidPartSegment.ArmSlow
            : null;

    /// <summary>Keeps the arm aligned to the body and advances its animation.</summary>
    internal static void RunKraidArmMain(RoomEnemyPlace arm, KraidEnemyState state, int cameraY)
    {
        RoomEnemyPlace body = state.Parts.Body;
        arm.Position = arm.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.ArmOffset);

        ShowWithinGameplayView(arm, cameraY);

        // While mouth reopens are pending the arm's swing waits on the frame it shows, its hold kept, and runs on from there
        // once none is pending.
        if (state.PendingMouthReopens != 0)
            arm.AnimationClock.Suspend();
        else
            arm.AnimationClock.Resume();
    }

    /// <summary>What <see cref="RunKraidFootMain"/> does for each <see cref="KraidFootPhase"/> member, each member's own arm.</summary>
    private IdentityTable<KraidFootPhase, Action<RoomEnemyPlace, KraidFootState, KraidEnemyState, RoomEnemyPlace>> RunKraidFootMainForKraidFootPhase => field ??= new(new Dictionary<KraidFootPhase, Action<RoomEnemyPlace, KraidFootState, KraidEnemyState, RoomEnemyPlace>>
    {
        [KraidFootPhase.Resting] = (foot, part, state, body) => { },
        [KraidFootPhase.FirstPhaseThinking] = (foot, part, state, body) =>
        {
            if (TryBeginKraidGrowth(body, state))
                return;
            if (part.PhaseWait.StepRunsOut())
                part.Phase = KraidFootPhase.PrepareFirstPhaseLunge;
        },
        [KraidFootPhase.PrepareFirstPhaseLunge] = (foot, part, state, body) => PrepareKraidFirstPhaseLunge(body, part),
        [KraidFootPhase.FirstPhaseLunge] = (foot, part, state, body) => RunKraidFirstPhaseLunge(body, foot, part),
        [KraidFootPhase.WaitBeforeRetreat] = (foot, part, state, body) =>
        {
            if (part.PhaseWait.StepRunsOut())
            {
                part.Phase = KraidFootPhase.FirstPhaseRetreat;
                state.FootState.StartAnimation(KraidPartSegment.FootWalkingBackward);
            }
        },
        [KraidFootPhase.WaitBeforeSecondPhase] = (foot, part, state, body) =>
        {
            if (part.PhaseWait.StepRunsOut())
                part.Phase = KraidFootPhase.SecondPhaseInitialize;
        },
        [KraidFootPhase.FirstPhaseRetreat] = (foot, part, state, body) => RunKraidFirstPhaseRetreat(body, part),
        [KraidFootPhase.SecondPhaseWalkToStart] = (foot, part, state, body) => RunKraidSecondPhaseWalkToStart(body, part),
        [KraidFootPhase.SecondPhaseInitialize] = (foot, part, state, body) => SetKraidWalking(part, targetX: KraidMovementChoices.StartingX, thinkUpdates: KraidArenaDefinitions.SecondPhaseFirstThinkUpdates, direction: HorizontalDirection.Right),
        [KraidFootPhase.SecondPhaseThinking] = (foot, part, state, body) => RunKraidSecondPhaseThinking(body, part),
        [KraidFootPhase.SecondPhaseWalkingRight] = (foot, part, state, body) => RunKraidSecondPhaseWalkingRight(body, part),
        [KraidFootPhase.SecondPhaseWalkingLeft] = (foot, part, state, body) => RunKraidSecondPhaseWalkingLeft(body, part),
    });
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
        RoomEnemyPlace body = part.Kraid.Parts.Body;
        foot.Position = foot.Position.WithWhole(body.Position.Whole + KraidArenaDefinitions.FootOffset);
        ShowWithinGameplayView(foot, cameraY);

        RunKraidFootMainForKraidFootPhase[part.Phase](foot, part, state, body);
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
