using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy or missile station (#1554, #2484), used through the access arm at the access block Samus runs into. The
/// station holds its animation and its arm's phase and use as domain values, and advances them on each update: an access
/// Samus triggered starts the arm, unless the station has nothing to give her; the arm extends, holds, activates the
/// station, holds and retracts by its authored timing (<see cref="StationAccessTimingDefinitions"/>), and the station's own
/// animation shows its images (<see cref="StationAnimationDefinitions"/>, #1994). What the station gives and when it turns
/// Samus away is its rule's (<see cref="StationAccessRule"/>). The station's animation and the arm's use each play on the
/// shared animation player (#2480).
/// </summary>
/// <param name="rule">How the station is used.</param>
/// <param name="area">The area whose map a map station serves.</param>
/// <param name="animation">The animation it shows first.</param>
internal sealed partial class AccessStationObject(StationAccessRule rule, AreaId area, StationAnimation animation) : RoomObjectBehaviour
{
    /// <summary>How the station is used: its access blocks, its animations, its reward and its message.</summary>
    internal StationAccessRule Rule { get; } = Ensure.NotNull(rule);

    /// <summary>Area whose map a map station acquires.</summary>
    public AreaId Area { get; } = area;

    /// <summary>Animation the station currently shows.</summary>
    internal StationAnimation ShownAnimation { get; set; } = animation;

    /// <summary>Animation shown once the station has given its reward: the map station's acquired one, or its ordinary one.</summary>
    internal StationAnimation CompletedAnimation => Rule.AcquiredAnimation ?? Rule.Animation;

    /// <summary>The station's animation playing: its first image shows on the station's first update.</summary>
    private readonly AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue> _animation =
        AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue>.Starting(
            StationAnimationDefinitions.Segments, StationAnimationDefinitions.Animations[animation]);

    /// <summary>The access arm's use playing, from the access's first draw.</summary>
    private readonly AnimationPlayer<StationAccessSegment, StationAccessPose, StationAccessCue> _arm =
        AnimationPlayer<StationAccessSegment, StationAccessPose, StationAccessCue>.Starting(
            StationAccessTimingDefinitions.Segments, StationAccessTimingDefinitions.Use);

    /// <summary>
    /// Whether the access arm is idle, about to start at a touch accepted since its last update, in use at the access Samus
    /// used, or waiting for the map screen to close.
    /// </summary>
    internal StationOperationPhase OperationPhase { get; set; } = StationOperationPhase.Idle;

    /// <summary>Whether the station accepts Samus's touch: while idle, or before its next update takes a touch it accepted.</summary>
    internal bool AcceptsTouch => OperationPhase == StationOperationPhase.Idle || OperationPhase is StationOperationPhase.TouchAccepted;

    /// <summary>Samus's accepted <paramref name="access"/> at <paramref name="accessBlock"/>, which the station's next update takes.</summary>
    internal void Trigger(Position16<RoomBlocks> accessBlock, StationAccess access) =>
        OperationPhase = new StationOperationPhase.TouchAccepted(new StationAccessUse(accessBlock, access));

    /// <summary>
    /// Leaving the map screen returns a map station waiting for it to idle. Unpause can arrive before the final access-art
    /// hold expires; that remaining animation keeps running.
    /// </summary>
    internal void ResumeAfterMapUnpause()
    {
        if (OperationPhase == StationOperationPhase.AwaitingMapUnpause)
            OperationPhase = StationOperationPhase.Idle;
    }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        StationObjects stations = context.RoomObjects.Stations;
        IRoomSamus samus = stations.Samus;

        // The access's first draw starts the arm's extension (StationAccessTimingDefinitions.MovementUpdates) on the
        // triggering update itself; the countdown only
        // runs on later updates.
        bool operationStartedThisUpdate = false;
        if (OperationPhase is StationOperationPhase.TouchAccepted { Use: var pending })
        {
            if (Rule.Reward is StationReward.Refills { Refill: var refill } && refill.IsFull(samus))
            {
                // The full-resource branch releases Samus before the access ends. A refill between the touch and this
                // update must release her too.
                samus.Control.Unlock();
                OperationPhase = StationOperationPhase.Idle;
            }
            else
            {
                // The arm draws its first extension image, holds fully inserted (StationAccessTimingDefinitions), then activates
                // the station. The touch locked Samus; this update does not lock her again, so one an intervening
                // unpause freed stays free. A map station's access never reads the map: a station whose map is held
                // has no access blocks (#2508), and the arm's draws leave a used station's access cells solid.
                OperationPhase = new StationOperationPhase.InUse(pending);
                operationStartedThisUpdate = true;
                context.QueueSound(EnemySound.StationEngaged, SoundRequestPriority.Normal);
                DrawAccess(context, pending, _arm.Show(StationAccessTimingDefinitions.Use));
            }
        }

        if (!operationStartedThisUpdate && OperationPhase is StationOperationPhase.InUse { Use: var use })
            StepAccess(context, samus, use);

        // A looping animation of images shows its next image once the shown one's updates run out (#1627).
        StationDraw? shown = _animation.StepToNewFrame();
        if (shown is { } image)
            context.Draw(place, StationObjectDraws.Family, image);
    }

    /// <summary>
    /// Plays the arm's use one update on: it draws the pose the update shows anew, carries out the moments it passes, and
    /// ends the access once it is over.
    /// </summary>
    private void StepAccess(RoomObjectStepContext context, IRoomSamus samus, StationAccessUse use)
    {
        if (_arm.StepToNewFrame(cue => ReachAccessMoment(cue, context, samus)) is { } pose)
        {
            DrawAccess(context, use, pose);
            return;
        }
        if (!_arm.HasStopped)
            return;
        OperationPhase = !Rule.Reward.ReleasesAtActivation && samus.Control.InputLocked
            ? StationOperationPhase.AwaitingMapUnpause
            : StationOperationPhase.Idle;
        // The access only ends here. Recharges released Samus at activation; leaving the map screen releases her after
        // the map.
    }

    /// <summary>What <see cref="ReachAccessMoment"/> does for each <see cref="StationAccessCue"/> member, each member's own arm.</summary>
    private IdentityTable<StationAccessCue, Action<RoomObjectStepContext, IRoomSamus>> ReachAccessMomentForStationAccessCue => field ??= new(new Dictionary<StationAccessCue, Action<RoomObjectStepContext, IRoomSamus>>
    {
        [StationAccessCue.Activates] = (context, samus) =>
        {
            Activate(context.RoomObjects.Stations, samus, context.Outcome);
            // The recharges release Samus right after their message, before the arm retracts. The map leaves her
            // locked for leaving the map screen to release.
            if (Rule.Reward.ReleasesAtActivation)
                samus.Control.Unlock();
        },
        [StationAccessCue.Disengages] = static (context, _) => context.QueueSound(EnemySound.StationDisengaged, SoundRequestPriority.Normal),
    });

    /// <summary>Carries out the moment <paramref name="cue"/> of the arm's use.</summary>
    private void ReachAccessMoment(StationAccessCue cue, RoomObjectStepContext context, IRoomSamus samus)
    {
        ReachAccessMomentForStationAccessCue[cue](context, samus);
    }

    /// <summary>
    /// Gives the station's reward, the refill or the area's map, and its message joins the step's
    /// <paramref name="outcome"/>; the station shows its completed animation from this update's step.
    /// </summary>
    private void Activate(StationObjects stations, IRoomSamus samus, RoomObjectStepOutcome outcome)
    {
        if (Rule.Reward is StationReward.Refills { Refill: var refill })
            refill.Fill(samus);
        else
            stations.System.SetAreaMapAcquired(Area);
        ShownAnimation = CompletedAnimation;
        _animation.Start(StationAnimationDefinitions.Animations[ShownAnimation]);
        outcome.StationActed(new StationCompleted(Rule.CompletedMessage));
    }

    /// <summary>Draws the access arm at <paramref name="use"/>, the access Samus used, in <paramref name="pose"/>.</summary>
    private static void DrawAccess(RoomObjectStepContext context, StationAccessUse use, StationAccessPose pose)
    {
        StationAccessObjectDefinition arm = StationAccessObjectDefinitions.Arms[use.Access];
        StationObjectDraws.Family.Draw(context.Level, use.Block, context.RoomObjects.Drawing,
            pose == StationAccessPose.Extended ? arm.Extended : arm.Retracted);
    }
}
