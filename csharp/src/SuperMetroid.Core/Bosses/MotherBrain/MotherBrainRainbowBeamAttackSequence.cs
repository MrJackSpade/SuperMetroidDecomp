using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// phase chains.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// updates readying the Baby's arrival, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Colour, beam-effect, projectile, earthquake, graphics, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, animation, and Samus-control
/// mutations execute directly.
/// </remarks>
public sealed class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>
    /// Where the beam's explosions burst from Samus, whom each then follows, in the order they burst: the first beam explosion takes the
    /// first, and the last is followed by the first again (#2258).
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionOffsets =
        [new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7), new(-8, -7)];

    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
    internal MotherBrainLateFightState State { get; }

    /// <summary>Mother Brain's rainbow-beam attack: charging, firing, pinning and draining Samus, letting her fall and deciding what comes next.</summary>
    internal MotherBrainRainbowAttackPhases RainbowAttack { get; }

    /// <summary>Mother Brain finishing Samus off: standing up, admiring, and charging and firing the final beam until the Baby Metroid arrives.</summary>
    internal MotherBrainFinishSamusOffPhases FinishSamusOff { get; }

    /// <summary>Mother Brain drained by the Baby Metroid: taken aback, firing back, running out, retreating and turning gray.</summary>
    internal MotherBrainDrainedByBabyPhases DrainedByBaby { get; }

    /// <summary>Mother Brain reviving and killing the Baby Metroid: signs of life, waking, walking up and the attacks on the baby.</summary>
    internal MotherBrainRevivalPhases Revival { get; }

    /// <summary>Mother Brain's third phase: recovering, setting up and fighting Samus after the baby cutscene.</summary>
    internal MotherBrainThirdPhaseFight ThirdPhaseFight { get; }

    /// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
    internal MotherBrainThirdPhaseDeath ThirdPhaseDeath { get; }


    /// <summary>
    /// Builds the sequence's phases over <paramref name="encounter"/>, whose body, brain, phase, neck and flags they read
    /// and write in place (#2209), with its walk credit and burst cycle.
    /// </summary>
    /// <param name="encounter">
    /// The encounter the sequence runs: its walk credit is the one phase three walks on and the shot reactions spend, and
    /// its burst cycle is the one her ascent's dust left and the death explosions and the escape door's dust continue
    /// (#1872).
    /// </param>
    internal MotherBrainRainbowBeamAttackSequence(MotherBrainEnemyState encounter)
    {
        Encounter = Ensure.NotNull(encounter);
        State = new MotherBrainLateFightState(encounter);
        RainbowAttack = new MotherBrainRainbowAttackPhases(_movement, State);
        FinishSamusOff = new MotherBrainFinishSamusOffPhases(State);
        DrainedByBaby = new MotherBrainDrainedByBabyPhases(State);
        Revival = new MotherBrainRevivalPhases(State);
        ThirdPhaseFight = new MotherBrainThirdPhaseFight(State, encounter.Walk);
        ThirdPhaseDeath = new MotherBrainThirdPhaseDeath(State, encounter.Bursts);
    }

    /// <summary>The encounter the sequence runs, the one owner of the state its phases read and write.</summary>
    internal MotherBrainEnemyState Encounter { get; }

    /// <summary>
    /// Current angle, in 256ths of a turn, of the rainbow beam's cone.
    /// Exposing the movement owner's angle lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public RotationAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        State.BabyArrivalReadyingElapsed = 0;
        State.BabyMetroidSpawned = false;
        RainbowAttack.BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// Carries out the cross-enemy phase write. The Baby moves Mother Brain on to her taken-aback step after it has
    /// reached the brain; Mother Brain runs that new
    /// phase on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        Encounter.DrainedPhase = MotherBrainDrainedPhase.TakenAback;
        State.RainbowBeamSoundPlaying = true; // The Baby's drain writes the shared SFX flag.
    }

    /// <summary>
    /// Runs the Baby's cross-enemy write. It does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this phase with the final attack.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        Encounter.RevivalPhase = MotherBrainRevivalPhase.PrepareForFinalBabyAttack;

    /// <summary>
    /// Runs the Baby's cross-enemy write. Mother Brain installs one last
    /// four-ring head animation and then leaves her body phase at the empty holding step.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        Encounter.RevivalPhase = MotherBrainRevivalPhase.ExecuteFinalBabyAttack;

    /// <summary>
    /// Applies the head's attack-counter step; the counter stops at twelve.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        Revival.BabyMetroidAttackCounter = Math.Min(Revival.BabyMetroidAttackCounter + 1, MotherBrainLateFightDefinitions.BabyAttackCountLimit);

    /// <summary>Applies the head's attack-counter reset step.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => Revival.BabyMetroidAttackCounter = 0;

    /// <summary>
    /// Runs the Baby's final cross-enemy write. The body does
    /// not run the recovery on the Baby's turn; the room's enemies run in their listed order, so
    /// Mother Brain runs this installed phase on her following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        Encounter.Phase3FightPhase = MotherBrainPhase3FightPhase.RecoverMakeSomeDistance;

        // Clears only the Baby's spawned flag, which head attack selection reads after the
        // Baby has deleted itself.
        State.BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction. The ordinary enemy-shot reaction owns damage,
    /// projectile deletion, and flash time; this method intentionally does not duplicate
    /// any of those separate systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // The shot reaction knows only the eight projectile types it reacts to. Validate the
        // projectile type so nothing can accidentally smuggle a larger
        // value past that domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(Encounter.Form, projectileType, ThirdPhaseFight.Phase3WalkCounter);
        ThirdPhaseFight.Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // The neck phase itself runs on Mother Brain's next ordinary phase-three
            // main call.
            ThirdPhaseFight.Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            Encounter.PhaseTimer.Count = 0;
        }
    }

    /// <summary>
    /// Makes Mother Brain stumble backwards as the Baby strikes her: her quickest backwards walk, toward a pixel left of
    /// where her body stands. The Baby runs after her body's animation pass, so the walk starts on her next update.
    /// </summary>
    public void RequestBabyStumbleBackward()
    {
        int targetX = CoordinateAxis.Wrap(State.BodyPosition.X - 1);
        State.RequestWalkBackward(targetX, MotherBrainWalkSpeed.ReallyFast);
    }

    /// <summary>Executes one call through the current Mother Brain body phase.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        var requests = new MotherBrainRainbowBeamStepRequests();

        bool handsOver;
        do
        {
            handsOver = Encounter.Stage switch
            {
                MotherBrainBodyStage.RainbowBeam => RainbowAttack.Run(samus, frame, requests),
                MotherBrainBodyStage.FinishingSamusOff => FinishSamusOff.Run(samus, frame, requests),
                MotherBrainBodyStage.DrainedByBaby => DrainedByBaby.Run(requests),
                MotherBrainBodyStage.Reviving => Revival.Run(frame, requests),
                MotherBrainBodyStage.Phase3Fight => ThirdPhaseFight.Run(frame),
                MotherBrainBodyStage.Dying => ThirdPhaseDeath.Run(frame, requests),
                // The sequence runs only the rainbow beam and the stages after it; the others belong to the room adapter.
                MotherBrainBodyStage.Glass or MotherBrainBodyStage.FakeDeathDescent or MotherBrainBodyStage.FakeDeathAscent or MotherBrainBodyStage.Phase2Fight or _ =>
                    throw new InvalidOperationException($"The late fight does not run Mother Brain's {Encounter.Stage} stage."),
            };
        }
        while (handsOver);

        return requests.ToResult();
    }
}
