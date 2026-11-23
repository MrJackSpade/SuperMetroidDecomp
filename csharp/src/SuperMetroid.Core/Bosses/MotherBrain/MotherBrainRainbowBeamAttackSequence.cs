using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// phase chains.
/// </summary>
/// <remarks>
/// Her earlier attack choices and the Baby Metroid's own flight are not part of it. It runs her neck's extension, both
/// charge waits, her walks and posture changes, the beam itself, her attack choice while Samus is low on energy, the final
/// charge, the four updates before the Baby arrives and its arrival, the final beam's hold, her painful stagger, where her
/// neck and brain stand, her retreat to the back of the room, her crouch, her greying, and the corpse the Baby watches for.
/// It asks for her colours, the beam's look, her projectiles, the quakes, her pictures, the Baby's arrival and her sounds,
/// and moves her, spends Samus's energy, times her waits, plays her animations and holds Samus itself.
/// </remarks>
public sealed partial class MotherBrainRainbowBeamAttackSequence
{
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
    /// The Baby Metroid of her late fight (#2471), or null before the final beam's spawn makes it in its place in the
    /// room's enemies, from which on its own turn it latches on, drains, heals Samus and dies; her brain's attacks on it
    /// aim at it.
    /// </summary>
    public BabyMetroidCutsceneState? Baby { get; private set; }

    /// <summary>
    /// Makes her late fight's Baby in <paramref name="place"/>, which the final beam's spawn gave it, and holds it from here.
    /// </summary>
    /// <exception cref="InvalidOperationException">Her late fight has made its Baby already.</exception>
    internal BabyMetroidCutsceneState MakeBaby(RoomEnemyPlace place)
    {
        if (Baby is not null)
            throw new InvalidOperationException("Mother Brain attempted to allocate the cutscene Baby more than once.");
        Baby = new BabyMetroidCutsceneState(this, place);
        return Baby;
    }

    /// <summary>
    /// The way the rainbow beam's cone points: the angle she aims it at Samus with, which the beam's
    /// picture is drawn along.
    /// </summary>
    public RotationAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        State.BabyMetroidSpawned = false;
        RainbowAttack.BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// The Baby, having reached her brain, takes Mother Brain aback; she is taken aback on her next turn, not inside the
    /// Baby's.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        State.DrainedPhase = MotherBrainDrainedPhase.TakenAback;
        State.RainbowBeamSoundPlaying = true; // The Baby's drain counts as her beam's sound playing.
    }

    /// <summary>
    /// The Baby sets Mother Brain readying her final attack on it: she walks back again and again without waiting for her
    /// stand-up to finish, until the Baby's flight sets the attack off.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        State.RevivalPhase = MotherBrainRevivalPhase.PrepareForFinalBabyAttack;

    /// <summary>
    /// The Baby sets off Mother Brain's final attack on it: her head plays one last four-ring volley, and her body then holds
    /// still.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        State.RevivalPhase = MotherBrainRevivalPhase.ExecuteFinalBabyAttack;

    /// <summary>
    /// Counts one more four-ring attack of her head's on the Baby, up to the most it counts
    /// (<see cref="MotherBrainLateFightDefinitions.BabyAttackCountLimit"/>).
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        Revival.BabyMetroidAttackCounter = Math.Min(Revival.BabyMetroidAttackCounter + 1, MotherBrainLateFightDefinitions.BabyAttackCountLimit);

    /// <summary>Starts her head's count of attacks on the Baby again from none.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => Revival.BabyMetroidAttackCounter = 0;

    /// <summary>
    /// The Baby, dying, starts Mother Brain's recovery for her third phase; she recovers from her next update on, not on the
    /// Baby's turn, since the room's enemies take their turns in their listed order.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        State.Phase3FightPhase = MotherBrainPhase3FightPhase.RecoverMakeSomeDistance;

        // The Baby is gone, so her head's attack choice no longer counts it as come.
        State.BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Mother Brain's walk and recoil as a shot strikes her in her second or third phase; the damage, the shot's end and
    /// her hurt flash are the common shot reaction's.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(SamusProjectileFamily projectileType)
    {
        // Her reaction to each of the six kinds of shot depends on her form and her third phase's walk credit.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(Encounter.Form, projectileType, ThirdPhaseFight.Phase3WalkCounter);
        ThirdPhaseFight.Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // Her neck's recoil starts on her next third-phase update; any wait her body is in runs out on its next
            // step.
            ThirdPhaseFight.Phase3NeckPhase = MotherBrainPhase3NeckPhase.StartHyperBeamRecoil;
            Encounter.PhaseTimer.LastFor(1);
        }
    }

    /// <summary>
    /// Makes Mother Brain stumble backwards as the Baby strikes her: her quickest backwards walk, toward a pixel left of
    /// where her body stands. The Baby runs after her body's animation pass, so the walk starts on her next update.
    /// </summary>
    public void RequestBabyStumbleBackward()
    {
        int targetX = State.BodyPosition.X - 1;
        State.RequestWalkBackward(targetX, MotherBrainWalkSpeed.ReallyFast);
    }

    /// <summary>
    /// What each stage of the late fight runs on an update (#2553): true when it moves on into the next stage, which the
    /// same update runs.
    /// </summary>
    private static IdentityTable<MotherBrainLateFightStage, Func<MotherBrainRainbowBeamAttackSequence, SamusState, RainbowBeamAttackFrame, MotherBrainRainbowBeamStepRequests, bool>> StageRuns { get; } = new(
        new Dictionary<MotherBrainLateFightStage, Func<MotherBrainRainbowBeamAttackSequence, SamusState, RainbowBeamAttackFrame, MotherBrainRainbowBeamStepRequests, bool>>
        {
            [MotherBrainLateFightStage.RainbowBeam] = static (fight, samus, frame, requests) => fight.RainbowAttack.Run(samus, frame, requests),
            [MotherBrainLateFightStage.FinishingSamusOff] = static (fight, samus, frame, requests) => fight.FinishSamusOff.Run(samus, frame, requests),
            [MotherBrainLateFightStage.DrainedByBaby] = static (fight, _, _, requests) => fight.DrainedByBaby.Run(requests),
            [MotherBrainLateFightStage.Reviving] = static (fight, _, frame, requests) => fight.Revival.Run(frame, requests),
            [MotherBrainLateFightStage.Phase3Fight] = static (fight, _, frame, _) => fight.ThirdPhaseFight.Run(frame),
            [MotherBrainLateFightStage.Dying] = static (fight, _, frame, requests) => fight.ThirdPhaseDeath.Run(frame, requests),
        });

    /// <summary>Executes one update through the current Mother Brain body phase.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        var requests = new MotherBrainRainbowBeamStepRequests();

        bool handsOver;
        do
        {
            handsOver = StageRuns[State.Stage](this, samus, frame, requests);
        }
        while (handsOver);

        return requests.ToResult();
    }
}
