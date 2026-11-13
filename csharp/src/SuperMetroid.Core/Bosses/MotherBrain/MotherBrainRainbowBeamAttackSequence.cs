using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// function chains.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// updates readying the Baby's arrival, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Palette, beam-effect, projectile, earthquake, graphics, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, program, and Samus-command
/// mutations execute directly.
/// </remarks>
public sealed class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>
    /// Signed beam-explosion positions. The index increments before
    /// lookup, so its initial zero value selects the second record.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionOffsets =
        [new(-8, -7), new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7)];

    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs and the brain's flags, with the walking and posture requests they share.</summary>
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


    /// <summary>Builds the sequence's phases over one shared late-fight state.</summary>
    public MotherBrainRainbowBeamAttackSequence()
    {
        State = new MotherBrainLateFightState();
        RainbowAttack = new MotherBrainRainbowAttackPhases(_movement, State);
        FinishSamusOff = new MotherBrainFinishSamusOffPhases(State);
        DrainedByBaby = new MotherBrainDrainedByBabyPhases(State);
        Revival = new MotherBrainRevivalPhases(State);
        ThirdPhaseFight = new MotherBrainThirdPhaseFight(State);
        ThirdPhaseDeath = new MotherBrainThirdPhaseDeath(State);
    }

    /// <summary>
    /// Current angle, in 256ths of a turn, of the rainbow beam's per-line effect.
    /// Exposing the movement owner's angle lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public RotationAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>Reads and writes <paramref name="walk"/> from now on, the walk credit of the encounter attaching this sequence.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => ThirdPhaseFight.ShareWalkCounter(Ensure.NotNull(walk));

    /// <summary>
    /// Inherits the death-and-escape explosion index from the room's encounter state when the live sequence
    /// attaches; the fake-death ascent dust last wrote it.
    /// </summary>
    internal void InheritDeathAndEscapeExplosionIndex(int bodySubFunctionTimer) =>
        ThirdPhaseDeath.DeathAndEscapeExplosionIndex = bodySubFunctionTimer;

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
    /// Imports the physical room actor's state before one live scheduler call. The standalone
    /// verifier may continue to own <see cref="MotherBrainLateFightState.Body"/> directly; gameplay instead lets the
    /// ordinary enemy program move the real body and feeds the resulting state back here.
    /// This deliberately does not copy program positions or timers: there must be only
    /// one visible animation runner in a live room.
    /// </summary>
    internal void SynchronizeLiveActor(MotherBrainLiveActorSnapshot actor)
    {
        State.Body.Position = actor.Body;
        State.Body.Pose = actor.BodyPose;
        State.Body.Form = actor.Form;
        ThirdPhaseDeath.BodyContact = actor.BodyContact;
        ThirdPhaseDeath.BodyProcessing = actor.BodyProcessing;
        ThirdPhaseDeath.BodyPresence = actor.BodyPresence;
        ThirdPhaseDeath.BodyCollidesByFrameHitboxes = actor.BodyCollidesByFrameHitboxes;
        ThirdPhaseDeath.BodyShowsNewFrame = actor.BodyShowsNewFrame;
        State.BrainPosition = actor.Brain;
        State.BrainHealth = actor.BrainHealth;
        ThirdPhaseDeath.BrainContact = actor.BrainContact;
        ThirdPhaseDeath.BrainProcessing = actor.BrainProcessing;
        ThirdPhaseDeath.BrainPresence = actor.BrainPresence;
        ThirdPhaseDeath.BrainCollidesByFrameHitboxes = actor.BrainCollidesByFrameHitboxes;
        ThirdPhaseDeath.BrainShowsNewFrame = actor.BrainShowsNewFrame;
        State.NeckMovementEnabled = actor.NeckMovementEnabled;
        State.LowerNeckMovement = actor.LowerNeckMovement;
        State.UpperNeckMovement = actor.UpperNeckMovement;
        ThirdPhaseDeath.HitboxesEnabled = actor.HitboxesEnabled;
    }

    /// <summary>
    /// The program most recently requested by the state machine's body helper. Gameplay copies
    /// this program into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's program timer.
    /// </summary>
    internal MotherBrainBodySegment? RequestedBodyAnimation => State.Body.Animation;

    /// <summary>
    /// Carries out the cross-enemy function write. The Baby actor
    /// installs the taken-aback step after it has reached the brain; Mother Brain executes that new
    /// function on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        State.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback;
        State.RainbowBeamSoundPlaying = true; // The Baby's drain writes the shared SFX flag.
    }

    /// <summary>
    /// Runs the Baby's cross-enemy write. It does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this function with the final attack.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack;

    /// <summary>
    /// Runs the Baby's cross-enemy write. Mother Brain installs one last
    /// four-ring head program and then leaves her body function at the empty holding step.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack;

    /// <summary>
    /// Applies the head's attack-counter step. Live gameplay advances the ordinary
    /// room animation script, while the reusable head verifier advances its private list;
    /// both must mutate this one saturating counter.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        Revival.BabyMetroidAttackCounter = Math.Min(Revival.BabyMetroidAttackCounter + 1, 12);

    /// <summary>Applies the head's attack-counter reset step.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => Revival.BabyMetroidAttackCounter = 0;

    /// <summary>
    /// Runs the Baby's final cross-enemy write. The body does
    /// not execute the recovery inside the Baby's slot; increasing-slot enemy order makes
    /// this installed phase visible on Mother Brain's following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        State.Phase = MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance;

        // Clears only the shared enemy-slot index. This host flag is the equivalent
        // liveness witness used by head attack selection after the actor has deleted itself.
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
        // host enum so a caller cannot accidentally smuggle a larger
        // value past that domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(State.Body.Form, projectileType, ThirdPhaseFight.Phase3WalkCounter);
        ThirdPhaseFight.Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // The neck function itself runs on Mother Brain's next ordinary phase-three
            // main call.
            ThirdPhaseFight.Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            State.FunctionTimer = 0;
        }
    }

    /// <summary>
    /// Runs the Baby's call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        int targetX = CoordinateAxis.Wrap(State.Body.Position.X - 1);
        return State.MakeBodyWalkBackwards(targetX, MotherBrainBodySegment.WalkBackwardReallyFast).Requested;
    }

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        if (State.Phase is null)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainBodyFunction? phaseBefore = State.Phase;
        uint headAnimationScriptRequestSerialBefore = State.HeadAnimationRequestSerial;
        var requests = new MotherBrainRainbowBeamStepRequests();

        bool handsOver;
        do
        {
            switch (State.Phase)
            {
                case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowExtendNeck();
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartCharging();
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowRetractNeck(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowWaitForCharge(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowExtendNeckDown();
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartFiring(samus, frame);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowMoveSamusTowardWall(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowOneFrameDelay(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartDrainingSamus(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowDrainingSamus(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowFinishFiring(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowLetSamusFall(samus);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowWaitForSamusToLand(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowLowerHead();
                    break;
                case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
                    handsOver = RainbowAttack.RunSecondPhaseRainbowDecideNextAction(samus, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOff(samus, frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffStandUp(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffAdmire();
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffChargeFinalBeam();
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffReadyBaby(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffFireFinalBeam(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
                    handsOver = MotherBrainFinishSamusOffPhases.RunSecondPhaseFinalRainbowBeamHolding();
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyTakenAback();
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyRegainBalance(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyFiringRainbowBeam(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyRainbowBeamRunOut(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyMoveToBackOfRoom(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyGoIntoLowPowerMode(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyPrepareTransitionToGrey();
                    break;
                case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
                    handsOver = DrainedByBaby.RunSecondPhaseDrainedByBabyTransitionToGrey(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
                    handsOver = Revival.RunSecondPhaseReviveInanimateGrey();
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
                    handsOver = Revival.RunSecondPhaseReviveShowSignsOfLife();
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
                    handsOver = Revival.RunSecondPhaseReviveTransitionFromGrey(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
                    handsOver = Revival.RunSecondPhaseReviveWakeUp(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
                    handsOver = Revival.RunSecondPhaseReviveWakeUpStretch();
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
                    handsOver = Revival.RunSecondPhaseReviveWalkUpToBaby(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
                    handsOver = Revival.RunSecondPhaseRevivePrepareNeckForBabyDeath();
                    break;
                case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
                    handsOver = Revival.RunSecondPhaseReviveFinishPreparingForBabyDeath(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
                    handsOver = Revival.RunSecondPhaseMurderBabyAttack(frame, requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
                    handsOver = Revival.RunSecondPhaseMurderBabyAttackCooldown();
                    break;
                case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
                    handsOver = Revival.RunSecondPhasePrepareForFinalBabyAttack(requests);
                    break;
                case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
                    handsOver = Revival.RunSecondPhaseExecuteFinalBabyAttack();
                    break;
                case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
                    handsOver = MotherBrainRevivalPhases.RunSecondPhaseFinalBabyAttackHolding();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
                    handsOver = ThirdPhaseFight.RunThirdPhaseRecoverMakeSomeDistance(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
                    handsOver = ThirdPhaseFight.RunThirdPhaseRecoverSetupForFighting();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseFightingMain:
                    handsOver = ThirdPhaseFight.RunThirdPhaseFightingMain(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
                    handsOver = ThirdPhaseFight.RunThirdPhaseFightingAttackCooldown();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathMoveToBackOfRoom(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathIdleWhilstExploding(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathStumbleToMiddleOfRoom(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathDisableBrainEffects(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSetupBodyFadeOut(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFadeOutBody(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFinalFewExplosions(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathRealizeDecapitation();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathBrainFallsToGround(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathReadyCorpse();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSetupFadeToGrey();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathFadeToGrey(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathCorpseTipsOver();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathCorpseRotsAway(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeath20FrameDelay();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathReadyEscapeWarning();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathStartEscape(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathSpawnTimeBombSetSubtitle(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathTypeOutZebesEscapeText(frame);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathDoorExplodingStartTimer(frame, requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathBlowUpEscapeDoor(requests);
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                    handsOver = MotherBrainThirdPhaseDeath.RunThirdPhaseDeathKeepEarthquakeGoing(frame, requests);
                    break;
                // The sequence owns only the rainbow-beam and later body functions; the others belong to the room adapter.
                case MotherBrainBodyFunction.FirstPhase:
                case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
                case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
                case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
                case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
                case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
                case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
                case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows1And2:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows3And4:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows5And6:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows7And8:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows9And10:
                case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows11And12:
                case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                case MotherBrainBodyFunction.FakeDeathAscentReadyStandingBody:
                case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
                case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
                case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
                case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
                case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
                case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
                case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
                case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
                case MotherBrainBodyFunction.SecondPhaseThinking:
                case MotherBrainBodyFunction.SecondPhaseTryAttack:
                case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
                case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
                case MotherBrainBodyFunction.SecondPhaseBombCrouch:
                case MotherBrainBodyFunction.SecondPhaseBombFired:
                case MotherBrainBodyFunction.SecondPhaseBombStandUp:
                case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
                case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
                case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
                case MotherBrainBodyFunction.SecondPhaseHandBeam:
                case null:
                default:
                    throw new InvalidOperationException($"Unsupported rainbow-beam phase {State.Phase}.");
            }
        }
        while (handsOver);

        return requests.ToResult(phaseBefore, State.Phase, State.HeadAnimationRequestSerial != headAnimationScriptRequestSerialBefore);
    }
}
