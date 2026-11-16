namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What only the late fight keeps (the brain's fall, the beam's sound, the health colours, the corpse and the Baby's
/// arrival), over the encounter whose body, brain, phase, neck and flags every late-fight phase reads and writes in place
/// (#2209): the encounter is their one owner, so nothing is copied in before a step or published after it.
/// </summary>
internal sealed class MotherBrainLateFightState
{
    /// <summary>Builds the late fight's own state over <paramref name="encounter"/>.</summary>
    internal MotherBrainLateFightState(MotherBrainEnemyState encounter)
    {
        Encounter = Ensure.NotNull(encounter);
    }

    /// <summary>The encounter the late fight runs: her body, her brain and the state they share.</summary>
    internal MotherBrainEnemyState Encounter { get; }

    /// <summary>Where her body stands in the room.</summary>
    internal Position16<RoomPixels> BodyPosition => Encounter.Body.Position.Whole;

    /// <summary>Where her brain lies in the room.</summary>
    internal Position16<RoomPixels> BrainPosition => Encounter.Head.Position.Whole;

    /// <summary>Moves her brain to <paramref name="position"/>, keeping the fractions of a pixel it holds.</summary>
    internal void MoveBrainTo(Position16<RoomPixels> position) =>
        Encounter.Head.Position = Encounter.Head.Position.WithWhole(position);

    /// <summary>
    /// The speed her severed brain falls at once she has realized her decapitation (#1890, #2016): none as it starts to
    /// fall, gaining <see cref="MotherBrainDeathDefinitions.BrainFallAcceleration"/> an update.
    /// </summary>
    public SignedEightEight BrainFallSpeed { get; internal set; }

    /// <summary>Rainbow-beam SFX-playing flag.</summary>
    public bool RainbowBeamSoundPlaying { get; internal set; }

    /// <summary>
    /// Whether Mother Brain's whole body shows her health band's colours. Revival enables it only after
    /// the walk to X 80 has really completed, and disabling her brain's effects as she dies ends it.
    /// </summary>
    public bool HealthColoursEnabled { get; internal set; }

    /// <summary>
    /// Whether Mother Brain lies as a corpse, which the Baby watches for: <see cref="MotherBrainPhase2CorpseState.Corpse"/>
    /// once she has faded fully grey, then <see cref="MotherBrainPhase2CorpseState.Revived"/>.
    /// </summary>
    public MotherBrainPhase2CorpseState Phase2CorpseState { get; internal set; }

    /// <summary>
    /// The updates the finish-off has spent readying the Baby Metroid's arrival so far.
    /// <see cref="MotherBrainReadyingDefinitions.BabyArrivalDuration"/> means it has arrived and the spawn handoff has run.
    /// </summary>
    public int BabyArrivalReadyingElapsed { get; internal set; }

    /// <summary>True once the final beam's Baby spawn has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; internal set; }

    /// <summary>Restarts her brain on <paramref name="segment"/>, with its timer; unlike an ordinary enemy animation it keeps no loop count.</summary>
    internal void SetHeadAnimation(MotherBrainHeadSegment segment) =>
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(Encounter, segment);

    /// <summary>Starts her body on <paramref name="segment"/>; the body's animation pass, later in the enemy update, plays it.</summary>
    internal void SetBodyAnimation(MotherBrainBodySegment segment) =>
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(Encounter, segment);

    /// <summary>Sets the neck angle and opposed raise/lower indices for retreat after the attack.</summary>
    internal void RetractHead()
    {
        // The Japanese and North American release takes 80; the European release takes 99.
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Rising;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
    }

    /// <summary>Walks her body forwards at <paramref name="speed"/>, by the forward target rules.</summary>
    internal MotherBrainWalkResult RequestWalkForward(int targetX, MotherBrainWalkSpeed speed)
    {
        MotherBrainBodySegment segment = speed switch
        {
            MotherBrainWalkSpeed.ReallyFast => MotherBrainBodySegment.WalkForwardReallyFast,
            MotherBrainWalkSpeed.Fast => MotherBrainBodySegment.WalkForwardFast,
            MotherBrainWalkSpeed.Medium => MotherBrainBodySegment.WalkForwardMedium,
            MotherBrainWalkSpeed.Slow => MotherBrainBodySegment.WalkForwardSlow,
            MotherBrainWalkSpeed.ReallySlow => MotherBrainBodySegment.WalkForwardReallySlow,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainWalkSpeed)} {speed}."),
        };
        return MakeBodyWalkForwards(targetX, segment);
    }

    /// <summary>Walks her body backwards at <paramref name="speed"/>, by the backward target rules.</summary>
    internal MotherBrainWalkResult RequestWalkBackward(int targetX, MotherBrainWalkSpeed speed)
    {
        MotherBrainBodySegment segment = speed switch
        {
            MotherBrainWalkSpeed.ReallyFast => MotherBrainBodySegment.WalkBackwardReallyFast,
            MotherBrainWalkSpeed.Fast => MotherBrainBodySegment.WalkBackwardFast,
            MotherBrainWalkSpeed.Medium => MotherBrainBodySegment.WalkBackwardMedium,
            MotherBrainWalkSpeed.Slow => MotherBrainBodySegment.WalkBackwardSlow,
            MotherBrainWalkSpeed.ReallySlow => MotherBrainBodySegment.WalkBackwardReallySlow,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainWalkSpeed)} {speed}."),
        };
        return MakeBodyWalkBackwards(targetX, segment);
    }

    /// <summary>
    /// Makes the body walk forwards. An overshoot of the target reports it reached first; at
    /// equality the walk is still requested. A body still mid-pose then reports it waiting,
    /// ahead of the arena's front limit that also reports the target reached. Only a standing body
    /// inside both limits starts the walk.
    /// </summary>
    private MotherBrainWalkResult MakeBodyWalkForwards(int targetX, MotherBrainBodySegment segment)
    {
        if (BodyPosition.X > targetX)
            return MotherBrainWalkResult.Reached;
        if (Encounter.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (BodyPosition.X >= MotherBrainFightDefinitions.FrontLimitX)
            return MotherBrainWalkResult.Reached;
        SetBodyAnimation(segment);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>
    /// Makes the body walk backwards. Reaching or passing the target reports it reached first, so a walk
    /// can complete the AI phase on the frame its movement step arrives. A body still
    /// mid-pose then reports it waiting, ahead of the arena's back limit that also reports
    /// the target reached. Only a standing body inside both limits starts the walk.
    /// </summary>
    private MotherBrainWalkResult MakeBodyWalkBackwards(int targetX, MotherBrainBodySegment segment)
    {
        if (BodyPosition.X <= targetX)
            return MotherBrainWalkResult.Reached;
        if (Encounter.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (BodyPosition.X < MotherBrainFightDefinitions.BackLimitX)
            return MotherBrainWalkResult.Reached;
        SetBodyAnimation(segment);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>Takes the posture chance (<see cref="MotherBrainRandomDefinitions.PostureChanges"/>) to toggle a standing or leaning body animation.</summary>
    internal void MaybeRequestStandUpOrLeanDown(MotherBrainRandomSample randomNumberSeed)
    {
        // Only a standing or leaning body changes posture. The animation plays later in the enemy update.
        if (!randomNumberSeed.Takes(MotherBrainRandomDefinitions.PostureChanges))
            return;

        if (Encounter.Pose == MotherBrainBodyPose.Standing)
            SetBodyAnimation(MotherBrainBodySegment.LeanDown);
        else if (Encounter.Pose == MotherBrainBodyPose.LeaningDown)
            MakeBodyStandUp();
    }

    /// <summary>Starts the pose-specific standing animation and reports whether the body is already standing.</summary>
    internal bool MakeBodyStandUp()
    {
        if (Encounter.Pose == MotherBrainBodyPose.Standing)
            return true;

        MotherBrainBodySegment? segment = Encounter.Pose switch
        {
            MotherBrainBodyPose.Crouched => MotherBrainBodySegment.StandAfterCrouchFast,
            MotherBrainBodyPose.LeaningDown => MotherBrainBodySegment.StandAfterLeaning,
            MotherBrainBodyPose.Standing or MotherBrainBodyPose.Walking or
                MotherBrainBodyPose.CrouchingTransition or MotherBrainBodyPose.DeathBeam => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyPose)} {Encounter.Pose}."),
        };
        if (segment is { } standUp)
            SetBodyAnimation(standUp);

        return false;
    }
}
