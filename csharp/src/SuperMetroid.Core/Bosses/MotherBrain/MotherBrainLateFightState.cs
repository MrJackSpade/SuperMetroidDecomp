namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What only the late fight keeps (the brain's fall, the beam's sound, the health colours, the corpse, the Baby's arrival
/// and her count of attacks on it), over the encounter whose body, brain, phase, neck and flags every late-fight phase reads and writes in place
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

    /// <summary>
    /// The Baby Metroid of her late fight (#2471): awaited until the final beam's spawn builds it in its place in the room's
    /// enemies (<see cref="BabyHasArrived"/>), from which on its own turn it latches on, drains, heals Samus and dies; her
    /// brain's attacks on it aim at it.
    /// </summary>
    public BabyMetroidArrival Baby { get; private set; } = BabyMetroidArrival.NotYet;

    /// <summary>
    /// How many four-ring attacks Mother Brain has begun against the Baby, zero through twelve, where it stays; her
    /// eleventh attack is made without her attack cry (<see cref="MotherBrainFightDefinitions.SilentBabyAttackCount"/>).
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>Holds <paramref name="baby"/>, which the final beam's spawn built in its place, as her late fight's arrived Baby.</summary>
    internal void BabyHasArrived(BabyMetroidCutsceneState baby) => Baby = new BabyMetroidArrival.Arrived(Ensure.NotNull(baby));

    /// <summary>
    /// Counts one more four-ring attack of her head's on the Baby, up to the most it counts
    /// (<see cref="MotherBrainLateFightDefinitions.BabyAttackCountLimit"/>).
    /// </summary>
    internal void CountBabyMetroidAttack() =>
        BabyMetroidAttackCounter = Math.Min(BabyMetroidAttackCounter + 1, MotherBrainLateFightDefinitions.BabyAttackCountLimit);

    /// <summary>Starts her head's count of attacks on the Baby again from none.</summary>
    internal void ResetBabyMetroidAttacks() => BabyMetroidAttackCounter = 0;

    /// <summary>Restarts her brain on her head's own animation <paramref name="segment"/>, with its timer; unlike an ordinary enemy animation it keeps no loop count.</summary>
    internal void SetHeadAnimation(MotherBrainHeadSegment segment) =>
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(Encounter, segment);

    /// <summary>Restarts her brain on her four-ring volley at Samus, whose own cues act on this late fight.</summary>
    internal void FireVolleyAtSamus() =>
        Encounter.Brain.Start(this, MotherBrainLateFightHeadSegment.AttackingSamusFourOnionRingsPhase3);

    /// <summary>
    /// Restarts her brain on her four-ring volley at the Baby once it has arrived, whose own cues act on the arrived Baby;
    /// before it arrives, on her volley at Samus. She chooses by whether the Baby holds its place in the room's enemies,
    /// which its arrival gives it.
    /// </summary>
    internal void FireVolleyAtBaby()
    {
        switch (Baby)
        {
            case BabyMetroidArrival.Arrived arrived:
                Encounter.Brain.Start(arrived.Baby, MotherBrainBabyAttackSegment.AttackingBabyMetroid);
                return;
            case BabyMetroidArrival.Awaited:
                FireVolleyAtSamus();
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BabyMetroidArrival)} {Baby}.");
        }
    }

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
