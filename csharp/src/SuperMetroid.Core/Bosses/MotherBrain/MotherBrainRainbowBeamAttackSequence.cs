using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// function chains at <c>$A9:B8EB-$A9:BFCF</c>.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// frame-spread Baby tile transfers, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Palette, HDMA, projectile, earthquake, VRAM, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, instruction-list, and Samus-command
/// mutations execute directly.
/// </remarks>
public sealed class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>
    /// Signed beam-explosion positions from $A9:BCA6/$BCB6. The index increments before
    /// lookup, so its initial zero value selects the second record.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionOffsets =
        [new(-8, -7), new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7)];

    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
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


    /// <summary>Standalone cartridge fixtures omit artwork and staging; installed rooms bind both.</summary>
    public MotherBrainRainbowBeamAttackSequence(RoomCharacterAtlas? corpseArtwork = null,
        EnemyStagingSurfaces? corpseStaging = null)
    {
        State = new MotherBrainLateFightState();
        RainbowAttack = new MotherBrainRainbowAttackPhases(_movement, State);
        FinishSamusOff = new MotherBrainFinishSamusOffPhases(State);
        DrainedByBaby = new MotherBrainDrainedByBabyPhases(State);
        Revival = new MotherBrainRevivalPhases(State);
        ThirdPhaseFight = new MotherBrainThirdPhaseFight(State);
        ThirdPhaseDeath = new MotherBrainThirdPhaseDeath(corpseArtwork, corpseStaging, State);
    }

    /// <summary>
    /// Current byte angle produced by `$A9:BBA9` for the bank-$88 rainbow HDMA renderer.
    /// Exposing the translated movement owner's word lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public SnesAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>
    /// Lower neck angle updated by the brain-slot handler at <c>$A9:9072</c>. The final
    /// beam has already lowered it to <c>$3000</c> before the Baby interrupts the body.
    /// </summary>
    public SnesAngle LowerNeckAngle { get; internal set; } = SnesAngle.FromRaw(12288);

    /// <summary>Upper neck angle, correspondingly lowered to <c>$2000</c>.</summary>
    public SnesAngle UpperNeckAngle { get; internal set; } = SnesAngle.FromRaw(8192);

    /// <summary>
    /// Number of live Mother Brain bomb enemy projectiles at body WRAM <c>$7E:802A</c>.
    /// </summary>
    /// <remarks>
    /// The counter belongs to Mother Brain even though bank <c>$86</c> owns each bomb's
    /// motion. Bomb initialization increments it and both native deletion paths decrement
    /// it. Keeping the mutation behind these two methods makes that cross-bank ownership
    /// visible instead of silently deriving a count from the host projectile collection.
    /// </remarks>
    public int BombCounter { get; private set; }

    /// <summary>Reads and writes <paramref name="walk"/> from now on, the word of the encounter attaching this sequence.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => ThirdPhaseFight.ShareWalkCounter(Ensure.NotNull(walk));

    /// <summary>
    /// Inherits word <c>$0FF2</c> from the room's encounter state when the live sequence
    /// attaches; the fake-death ascent dust last wrote it.
    /// </summary>
    internal void InheritDeathAndEscapeExplosionIndex(ushort bodySubFunctionTimer) =>
        ThirdPhaseDeath.DeathAndEscapeExplosionIndex = NativeCountdown.FromRaw(bodySubFunctionTimer);

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle at `$A9:B8EB`, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        State.BabyMetroidTileTransferIndex = 0;
        State.BabyMetroidSpawned = false;
        FinishSamusOff.RainbowBeamPaletteAnimationIndex = 0;
        RainbowAttack.BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// Imports the physical room actor words before one live scheduler call. The standalone
    /// verifier may continue to own <see cref="MotherBrainLateFightState.Body"/> directly; gameplay instead lets the
    /// ordinary enemy bytecode move the real body and feeds those resulting words back here.
    /// This deliberately does not copy instruction pointers or timers: there must be only
    /// one visible animation interpreter in a live room.
    /// </summary>
    internal void SynchronizeLiveActor(MotherBrainLiveActorSnapshot actor)
    {
        State.Body.Position = actor.Body;
        State.Body.Pose = actor.BodyPose;
        State.Body.Form = actor.Form;
        ThirdPhaseDeath.BodyProperties = actor.BodyProperties;
        ThirdPhaseDeath.BodyProperties2 = actor.BodyExtraProperties;
        State.BrainPosition = actor.Brain;
        State.BrainHealth = actor.BrainHealth;
        ThirdPhaseDeath.BrainProperties = actor.BrainProperties;
        ThirdPhaseDeath.BrainProperties2 = actor.BrainExtraProperties;
        LowerNeckAngle = actor.LowerNeckAngle;
        UpperNeckAngle = actor.UpperNeckAngle;
        State.NeckMovementEnabled = actor.NeckMovementEnabled;
        State.LowerNeckMovementIndex = actor.LowerNeckMovementIndex;
        State.UpperNeckMovementIndex = actor.UpperNeckMovementIndex;
        BombCounter = actor.BombCounter;
        ThirdPhaseDeath.HitboxesEnabled = actor.HitboxesEnabled;
    }

    /// <summary>
    /// The list most recently requested by the state machine's body helper. Gameplay copies
    /// this pointer into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's instruction timer.
    /// </summary>
    internal MotherBrainBodyProgram? RequestedBodyProgram => State.Body.Program;

    /// <summary>
    /// Ports the cross-enemy function-pointer write at <c>$A9:C8CD</c>. The Baby actor
    /// installs <c>$BE38</c> after it has reached the brain; Mother Brain executes that new
    /// function on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        State.Phase = MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback;
        State.RainbowBeamSoundPlaying = true; // `$A9:C8DA-$C8DD` writes the shared SFX flag.
    }

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CB23</c>. The native routine does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this function with <c>$C19A</c>.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack;

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CBA8</c>. Mother Brain installs one last
    /// `$9DB1` four-ring head program and then leaves her body function at the native RTS.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        State.Phase = MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack;

    /// <summary>
    /// Applies physical head opcode <c>$A9:9EA3</c>. Live gameplay advances the ordinary
    /// room instruction list, while the reusable head verifier advances its private list;
    /// both must mutate this one native saturating counter.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        Revival.BabyMetroidAttackCounter = Math.Min(Revival.BabyMetroidAttackCounter + 1, 12);

    /// <summary>Applies physical head opcode <c>$A9:9EB5</c>.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => Revival.BabyMetroidAttackCounter = 0;

    /// <summary>
    /// Ports the Baby's final cross-enemy write at <c>$A9:CD02-$CD05</c>. The body does
    /// not execute <c>$C1CF</c> inside the Baby's slot; increasing-slot enemy order makes
    /// this installed phase visible on Mother Brain's following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        State.Phase = MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance;

        // Native clears only the shared enemy-slot index. This host flag is the equivalent
        // liveness witness used by head attack selection after the actor has deleted itself.
        State.BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction at <c>$A9:B562-$B5C4</c>. The ordinary enemy-shot routine owns damage,
    /// projectile deletion, and flash time; this method intentionally does not duplicate
    /// any of those separately translated systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot accidentally smuggle a larger
        // value past that native domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(State.Body.Form, projectileType, ThirdPhaseFight.Phase3WalkCounter);
        ThirdPhaseFight.Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // The neck function itself runs on Mother Brain's next ordinary phase-three
            // main call.
            ThirdPhaseFight.Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            State.FunctionTimer = NativeCountdown.Start(0);
        }
    }

    /// <summary>
    /// Ports the Baby's <c>$A9:C879</c> call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        ushort targetX = unchecked((ushort)(State.Body.Position.X - 1));
        return State.MakeBodyWalkBackwards(targetX, MotherBrainBodyProgram.WalkBackwardReallyFast).Requested;
    }

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        if (State.Phase is null)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainBodyFunction? phaseBefore = State.Phase;
        uint headInstructionListRequestSerialBefore = State.HeadInstructionListRequestSerial;
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
                    handsOver = RainbowAttack.RunSecondPhaseRainbowStartDrainingSamus();
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
                    handsOver = RainbowAttack.RunSecondPhaseRainbowWaitForSamusToLand(samus, requests);
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
                case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
                    handsOver = FinishSamusOff.RunSecondPhaseFinishSamusOffLoadBabyTiles(requests);
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
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathBrainFallsToGround();
                    break;
                case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathLoadCorpseTiles(requests);
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
                case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathLoadEscapeTimerTiles(requests);
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
                    handsOver = ThirdPhaseDeath.RunThirdPhaseDeathKeepEarthquakeGoing(frame, requests);
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
                case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
                case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
                case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
                case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
                case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
                case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
                case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
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

        return requests.ToResult(phaseBefore, State.Phase, State.HeadInstructionListRequestSerial != headInstructionListRequestSerialBefore);
    }
}
