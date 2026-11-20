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

    /// <summary>
    /// The stage the late fight runs her body in (#2208), which names the type of the phase it runs on its next AI turn;
    /// setting a stage's phase enters that stage. The fight begins with her rainbow beam.
    /// </summary>
    public MotherBrainLateFightStage Stage { get; private set; }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.RainbowBeam"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainRainbowPhase RainbowPhase
    {
        get => Stage == MotherBrainLateFightStage.RainbowBeam ? field : throw NotIn(MotherBrainLateFightStage.RainbowBeam);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.RainbowBeam;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.FinishingSamusOff"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainFinishOffPhase FinishOffPhase
    {
        get => Stage == MotherBrainLateFightStage.FinishingSamusOff ? field : throw NotIn(MotherBrainLateFightStage.FinishingSamusOff);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.FinishingSamusOff;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.DrainedByBaby"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainDrainedPhase DrainedPhase
    {
        get => Stage == MotherBrainLateFightStage.DrainedByBaby ? field : throw NotIn(MotherBrainLateFightStage.DrainedByBaby);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.DrainedByBaby;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.Reviving"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainRevivalPhase RevivalPhase
    {
        get => Stage == MotherBrainLateFightStage.Reviving ? field : throw NotIn(MotherBrainLateFightStage.Reviving);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.Reviving;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.Phase3Fight"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainPhase3FightPhase Phase3FightPhase
    {
        get => Stage == MotherBrainLateFightStage.Phase3Fight ? field : throw NotIn(MotherBrainLateFightStage.Phase3Fight);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.Phase3Fight;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainLateFightStage.Dying"/> stage the late fight runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The late fight is in another stage.</exception>
    public MotherBrainDeathPhase DeathPhase
    {
        get => Stage == MotherBrainLateFightStage.Dying ? field : throw NotIn(MotherBrainLateFightStage.Dying);
        internal set
        {
            field = value;
            Stage = MotherBrainLateFightStage.Dying;
        }
    }

    /// <summary>The failure of reading a stage's phase while the late fight is in another.</summary>
    private InvalidOperationException NotIn(MotherBrainLateFightStage stage) =>
        new($"Mother Brain's late fight is in its {Stage} stage, not {stage}.");

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

    /// <summary>Whether her rainbow beam's sound is playing.</summary>
    public bool RainbowBeamSoundPlaying { get; internal set; }

    /// <summary>
    /// Whether Mother Brain's whole body shows her health band's colours. Revival enables it only after
    /// the walk up to the Baby (<see cref="MotherBrainLateFightDefinitions.BabyWalkUpX"/>) has really completed, and disabling her brain's effects as she dies ends it.
    /// </summary>
    public bool HealthColoursEnabled { get; internal set; }

    /// <summary>
    /// Whether Mother Brain lies as a corpse, which the Baby watches for: <see cref="MotherBrainPhase2CorpseState.Corpse"/>
    /// once she has faded fully grey, then <see cref="MotherBrainPhase2CorpseState.Revived"/>.
    /// </summary>
    public MotherBrainPhase2CorpseState Phase2CorpseState { get; internal set; }

    /// <summary>True once the final beam's Baby spawn has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; internal set; }

    /// <summary>Restarts her brain on <paramref name="segment"/>, with its timer; unlike an ordinary enemy animation it keeps no loop count.</summary>
    internal void SetHeadAnimation(MotherBrainHeadSegment segment) =>
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(Encounter, segment);

    /// <summary>Starts her body on <paramref name="segment"/>; the body's animation pass, later in the enemy update, plays it.</summary>
    internal void SetBodyAnimation(MotherBrainBodySegment segment) =>
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(Encounter, segment);

    /// <summary>Draws her head back after the attack: her neck turns at its rising speed, its lower half rising and its upper half lowering.</summary>
    internal void RetractHead()
    {
        Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Rising;
        Encounter.NeckMovementEnabled = true;
        Encounter.LowerNeckMovement = MotherBrainNeckMovement.Raise;
        Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
    }

    /// <summary>Walks her body forwards toward <paramref name="targetX"/> at <paramref name="speed"/> (<see cref="MotherBrainWalks.Forwards"/>).</summary>
    /// <returns>Whether she has reached the walk's target or the arena's front limit.</returns>
    internal bool RequestWalkForward(int targetX, MotherBrainWalkSpeed speed) =>
        MotherBrainWalks.Forwards(Encounter, targetX, speed);

    /// <summary>Walks her body backwards toward <paramref name="targetX"/> at <paramref name="speed"/> (<see cref="MotherBrainWalks.Backwards"/>).</summary>
    /// <returns>Whether she has reached the walk's target or the arena's back limit.</returns>
    internal bool RequestWalkBackward(int targetX, MotherBrainWalkSpeed speed) =>
        MotherBrainWalks.Backwards(Encounter, targetX, speed);

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

    /// <summary>The animation that stands the body up from each pose, or null for a pose that has none.</summary>
    private static IdentityTable<MotherBrainBodyPose, MotherBrainBodySegment?> StandingAnimations => field ??= new(new Dictionary<MotherBrainBodyPose, MotherBrainBodySegment?>
    {
        [MotherBrainBodyPose.Crouched] = MotherBrainBodySegment.StandAfterCrouchFast,
        [MotherBrainBodyPose.LeaningDown] = MotherBrainBodySegment.StandAfterLeaning,
        [MotherBrainBodyPose.Standing] = null,
        [MotherBrainBodyPose.Walking] = null,
        [MotherBrainBodyPose.CrouchingTransition] = null,
        [MotherBrainBodyPose.DeathBeam] = null,
    });

    /// <summary>Starts the pose-specific standing animation and reports whether the body is already standing.</summary>
    internal bool MakeBodyStandUp()
    {
        if (Encounter.Pose == MotherBrainBodyPose.Standing)
            return true;

        MotherBrainBodySegment? segment = StandingAnimations[Encounter.Pose];
        if (segment is { } standUp)
            SetBodyAnimation(standUp);

        return false;
    }
}
