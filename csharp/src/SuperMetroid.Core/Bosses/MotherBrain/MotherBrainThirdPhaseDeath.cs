using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
internal sealed class MotherBrainThirdPhaseDeath
{
    /// <summary>The corpse rotting away after the final death, with its own rot schedule.</summary>
    internal readonly MotherBrainCorpseRottingState CorpseRotting = new();

    /// <summary>The late fight's own state, with the walking and posture helpers every late-fight phase shares.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>The encounter the late fight runs, the one owner of the state these phases read and write.</summary>
    private MotherBrainEnemyState Encounter => _state.Encounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    /// <param name="state">The state every late-fight phase reads and writes.</param>
    /// <param name="bursts">
    /// Where and when the death explosions and the escape door's dust burst: the cycle of the encounter the sequence is
    /// attached to, which her ascent's dust left (#1872).
    /// </param>
    internal MotherBrainThirdPhaseDeath(MotherBrainLateFightState state, MotherBrainBurstCycle bursts)
    {
        _state = state;
        _bursts = Ensure.NotNull(bursts);
    }

    /// <summary>
    /// Where and when the death explosions and the escape door's dust burst: the encounter's cycle, which her ascent's dust
    /// left (#1872).
    /// </summary>
    private readonly MotherBrainBurstCycle _bursts;

    /// <summary>
    /// The updates her death has spent readying her corpse so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.CorpseDuration"/>.
    /// </summary>
    public int CorpseReadyingElapsed { get; private set; }

    /// <summary>
    /// The updates her death has spent readying the escape timer and message so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.EscapeWarningDuration"/>.
    /// </summary>
    public int EscapeWarningReadyingElapsed { get; private set; }

    /// <summary>
    /// The updates her death has spent readying the escape door's explosion so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.ExplodedDoorDuration"/>.
    /// </summary>
    public int ExplodedDoorReadyingElapsed { get; private set; }

    /// <summary>Runs one update of readying her corpse, rejecting calls once it is readied.</summary>
    private void ReadyCorpse()
    {
        if (CorpseReadyingElapsed >= MotherBrainReadyingDefinitions.CorpseDuration)
            throw new InvalidOperationException("Mother Brain's corpse is already readied.");

        CorpseReadyingElapsed++;
    }

    /// <summary>Runs one update of readying the escape timer and message, rejecting calls once they are readied.</summary>
    internal void ReadyEscapeWarning()
    {
        if (EscapeWarningReadyingElapsed >= MotherBrainReadyingDefinitions.EscapeWarningDuration)
            throw new InvalidOperationException("The escape timer and message are already readied.");

        EscapeWarningReadyingElapsed++;
    }

    /// <summary>Runs one update of readying the escape door's explosion, rejecting calls once it is readied.</summary>
    internal void ReadyExplodedDoor()
    {
        if (ExplodedDoorReadyingElapsed >= MotherBrainReadyingDefinitions.ExplodedDoorDuration)
            throw new InvalidOperationException("The escape door's explosion is already readied.");

        ExplodedDoorReadyingElapsed++;
    }

    /// <summary>
    /// Emits the death-phase explosions at the timer cadence, consuming one global
    /// random value per projectile so later enemy AI sees the same RNG sequence.
    /// </summary>
    internal void GenerateDeathExplosions(
        MotherBrainDeathExplosionStyle style,
        Func<int> nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // A restarted interval runs out on the first update, so a burst after a restart is immediate; after a burst the
        // smoky explosions wait sixteen more updates and the mixed ones eight. Each burst takes the group before the last,
        // continuing the place the ascent's dust left (#1872).
        if (!_bursts.IntervalRunsOut(style == MotherBrainDeathExplosionStyle.Mixed ? 8 : 16))
            return;

        int group = _bursts.NextPlace(MotherBrainDeathExplosionDefinitions.GroupCount);
        int simultaneousCount = style == MotherBrainDeathExplosionStyle.Mixed ? MotherBrainDeathExplosionDefinitions.AnchorsPerGroup : 2;
        int pairIndex = group * MotherBrainDeathExplosionDefinitions.AnchorsPerGroup;
        for (int explosionIndex = 0; explosionIndex < simultaneousCount; explosionIndex++)
        {
            // The shared random sequence advances once per projectile, not once per visual batch.
            int random = nextRandomNumber();
            MotherBrainDeathExplosionAnimation animation = style == MotherBrainDeathExplosionStyle.Mixed
                ? MotherBrainRandomDefinitions.MixedDeathExplosionAnimation.Choose(random)
                : MotherBrainDeathExplosionAnimation.Smoke;
            requests.Add(new MotherBrainDeathExplosionRequest(
                Offset: MotherBrainDeathExplosionDefinitions.Anchor(pairIndex + explosionIndex),
                Animation: animation,
                SoundEffect: MotherBrainSoundDefinitions.Explosion));
        }
    }

    /// <summary>
    /// Emits the escape door's next dust burst, at the place before the last; no request on updates before the interval
    /// runs out.
    /// </summary>
    private DustExplosionRequest? GenerateEscapeDoorExplosion(
        Func<int> nextRandomNumber)
    {
        // The door's dust continues the interval and the place the last death explosions left (#1872), then waits four
        // updates between bursts.
        if (!_bursts.IntervalRunsOut(4))
            return null;

        int place = _bursts.NextPlace(MotherBrainDeathDefinitions.EscapeDoorDustPositions.Length);
        Position16<RoomPixels> position = Ensure.Element(MotherBrainDeathDefinitions.EscapeDoorDustPositions, place);

        // The escape-door dust advances the one shared RNG exactly once per emitted projectile.
        // Its smoke chance selects the smoke animation; every other sample retains the preloaded small explosion.
        MiscDustAnimation animation = MotherBrainRandomDefinitions.EscapeDoorDustSmokes.IsTakenBy(nextRandomNumber())
            ? MiscDustAnimation.Smoke
            : MiscDustAnimation.SmallExplosion;
        return new DustExplosionRequest(
            Position: position,
            Animation: animation,
            SoundEffect: EnemySound.SmallExplosion);
    }

    /// <summary>
    /// Runs the third phase death move to back of room body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathMoveToBackOfRoom()
    {
        // The move-to-back step makes both enemies non-interactive and disables the shared
        // hitboxes on every call, then reuses the ordinary medium-speed backward
        // walk producer. Its target reached means X 40 has been reached/overshot; the new 128
        // timer falls through so the first smoky explosion and decrement happen now.
        Encounter.Body.Contact = Encounter.Body.Contact.With(EnemyContact.IgnoresSamus);
        Encounter.Head.Contact = Encounter.Head.Contact.With(EnemyContact.IgnoresSamus);
        Encounter.HitboxesEnabled = MotherBrainHitboxParts.None;
        MotherBrainWalkResult dyingRetreat = _state.RequestWalkBackward(40, animationDelay: 6);
        if (dyingRetreat.ReachedTarget)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.IdleWhilstExploding;
            Encounter.PhaseTimer.Count = 128;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death idle whilst exploding body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathIdleWhilstExploding(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.DeathPhase = MotherBrainDeathPhase.StumbleToMiddleOfRoom;
        return false;
    }

    /// <summary>
    /// Runs the third phase death stumble to middle of room body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathStumbleToMiddleOfRoom(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The stumble emits smoke before asking for a really-fast forward walk to 96.
        // Equality still starts a complete body animation; only signed overshoot or the
        // independent 128 arena clamp reports the target reached and advances the death script.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        MotherBrainWalkResult dyingAdvance = _state.RequestWalkForward(96, animationDelay: 2);
        if (dyingAdvance.ReachedTarget)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
            Encounter.DeathPhase = MotherBrainDeathPhase.DisableBrainEffects;
            Encounter.PhaseTimer.Count = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death disable brain effects body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDisableBrainEffects(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            // Disabling the brain's effects removes the neck/breath/colour producers, copies the body's
            // shared colours into the brain's, performs one health-colour refresh, and then
            // deliberately falls through with the already-negative timer.
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.None;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.None;
            Encounter.DroolGenerationEnabled = false;
            Encounter.SmallPurpleBreathGenerationEnabled = false;
            _state.HealthColoursEnabled = false;
            requests.BrainDetached = true;
            // Disabling the brain's effects restarts the bursts before the fade-out explosions.
            _bursts.Restart();
            Encounter.DeathPhase = MotherBrainDeathPhase.SetupBodyFadeOut;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup body fade out body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupBodyFadeOut(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathBodyFade.Restart();
            Encounter.DeathPhase = MotherBrainDeathPhase.FadeOutBody;
            Encounter.PhaseTimer.Count = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade out body body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeOutBody(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // Body flickering runs every AI call, independently of the seventeen-call
        // colour cadence. The fade's sixteen steps show its blackening colourings; its
        // seventeenth shows none and completes it.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        requests.BodyFlickers = true;
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.PhaseTimer.Count = 16;
            int fadeStep = Encounter.DeathBodyFade.TakeStep();
            if (fadeStep < 16)
                requests.DeathFadeStep = fadeStep;
            if (fadeStep >= 16)
            {
                requests.BodyClearRequested = true;
                RoomEnemyPlace body = Encounter.Body;
                body.Presence = body.Presence.With(EnemyPresence.Hidden);
                body.Processing = body.Processing.Without(EnemyProcessing.Animates);
                body.CollidesByFrameHitboxes = false;
                body.AnimationClock.ShowsNewFrame = false;
                Encounter.DeathPhase = MotherBrainDeathPhase.FinalFewExplosions;
                Encounter.PhaseTimer.Count = 16;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death final few explosions body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFinalFewExplosions(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.DeathPhase = MotherBrainDeathPhase.RealizeDecapitation;
        return false;
    }

    /// <summary>
    /// Runs the third phase death realize decapitation body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathRealizeDecapitation()
    {
        // The body starts the decapitated animation and the brain's own draw without its neck, which hands the brain's
        // position from the articulated neck to the fall; it starts the brain falling from rest, then falls through
        // immediately.
        _state.SetHeadAnimation(MotherBrainHeadSegment.Decapitated);
        Encounter.BrainAction = MotherBrainBrainAction.SetupBrainToBeDrawn;
        _state.BrainFallSpeed = SignedEightEight.Zero;
        Encounter.DeathPhase = MotherBrainDeathPhase.BrainFallsToGround;
        return true;
    }

    /// <summary>
    /// Runs the third phase death brain falls to ground body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBrainFallsToGround(MotherBrainRainbowBeamStepRequests requests)
    {
        // The brain gains fall speed each update and falls the speed's whole pixels.
        // Crossing 196 clamps the head and starts the brain's landing earthquake for 20 frames.
        _state.BrainFallSpeed += MotherBrainDeathDefinitions.BrainFallAcceleration;
        int fallingDisplacement = _state.BrainFallSpeed.UnsignedWholePixels;
        Position16<RoomPixels> candidate = _state.BrainPosition + new Offset16<RoomPixels>(0, fallingDisplacement);
        if (candidate.Y >= 196)
        {
            requests.StartEarthquake(MotherBrainDeathDefinitions.BrainLandingEarthquake, 20);
            _state.MoveBrainTo(_state.BrainPosition.WithY(196));
            CorpseReadyingElapsed = 0;
            Encounter.DeathPhase = MotherBrainDeathPhase.ReadyCorpse;
        }
        else
        {
            _state.MoveBrainTo(candidate);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load corpse pictures body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyCorpse()
    {
        // Each call readies one update of her corpse; the last moves on to the fade on that same call.
        ReadyCorpse();
        if (CorpseReadyingElapsed == MotherBrainReadyingDefinitions.CorpseDuration)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.SetupFadeToGrey;
            Encounter.PhaseTimer.Count = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup fade to grey body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupFadeToGrey()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.CorpseGreying.Restart();
            Encounter.DeathPhase = MotherBrainDeathPhase.FadeToGrey;
            Encounter.PhaseTimer.Count = 0;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade to grey body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            int greyStep = Encounter.CorpseGreying.TakeStep();
            if (greyStep < 8)
            {
                requests.CorpseFadeStep = greyStep;
                Encounter.PhaseTimer.Count = 16;
            }
            else
            {
                _state.SetHeadAnimation(MotherBrainHeadSegment.Corpse);
                Encounter.DeathPhase = MotherBrainDeathPhase.CorpseTipsOver;
                Encounter.PhaseTimer.Count = 256;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse tips over body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathCorpseTipsOver()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.CorpseRotsAway;
            Encounter.Head.Contact = Encounter.Head.Contact.With(EnemyContact.IgnoresSamus);
            Encounter.HitboxesEnabled = MotherBrainHitboxParts.None;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse rots away body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathCorpseRotsAway(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        MotherBrainCorpseRottingStepResult corpseRotting = CorpseRotting.Step(
            _state.BrainPosition,
            frame.RandomNumberSeed,
            frame.MainEnemyExecutionCounter);
        requests.CorpseDustRequests.AddRange(corpseRotting.DustRequests);
        if (corpseRotting.StillRotting)
            return false;

        // Once the corpse has rotted away the brain becomes invisible and non-solid, stops
        // its animation and frame-hitbox collision, and queues stop + escape music through the ordinary
        // eight-frame-delay music queue. Twenty is loaded and immediately decremented
        // by the fallthrough into the next step, leaving observable timer nineteen now.
        RoomEnemyPlace brain = Encounter.Head;
        brain.Presence = brain.Presence.With(EnemyPresence.Hidden);
        brain.Processing = brain.Processing.Without(EnemyProcessing.Animates);
        brain.CollidesByFrameHitboxes = false;
        brain.AnimationClock.ShowsNewFrame = false;
        requests.MusicStopQueued = true;   // Queue the music stop.
        requests.EscapeMusicQueued = true; // Queue the escape music.
        Encounter.DeathPhase = MotherBrainDeathPhase.PauseBeforeEscape;
        Encounter.PhaseTimer.Count = 20;
        return true;
    }

    /// <summary>
    /// Runs the pause that opens Mother Brain's third-phase death, counting its timer of twenty down until it runs out. True when
    /// it moves to the next body phase and the game runs on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeath20FrameDelay()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            // The decapitated brain remains at the corpse coordinates for the full delay, then is parked in the room's
            // top-left corner. The parking is gameplay-observable (#2257): the hidden brain still meets Samus's bombs by its
            // radius, its frames' hitboxes being off, so at the corpse a bomb laid during the escape would strike it.
            _state.MoveBrainTo(Position16<RoomPixels>.Zero);
            Encounter.DeathPhase = MotherBrainDeathPhase.ReadyEscapeWarning;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load escape timer pictures body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyEscapeWarning()
    {
        ReadyEscapeWarning();
        if (EscapeWarningReadyingElapsed == MotherBrainReadyingDefinitions.EscapeWarningDuration)
        {
            // The last readying update installs the escape's start and falls through far enough to ready the escape
            // door's explosion's first update during this same enemy AI call.
            Encounter.DeathPhase = MotherBrainDeathPhase.StartEscape;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death start escape body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathStartEscape(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        ReadyExplodedDoor();
        if (ExplodedDoorReadyingElapsed != MotherBrainReadyingDefinitions.ExplodedDoorDuration)
            return false;

        // The last readying update performs the full escape-door handoff on that call: start escape music, hold the quake
        // indefinitely, create four colour effects and initialize text. The door's
        // fragments are pre-coloured in the exploded door's colours, so its colour load is not kept (#1544).
        requests.EscapeMusicTrackQueued = true;        // Eight-frame-delay escape song.
        requests.StartIndefiniteEarthquake(MotherBrainDeathDefinitions.EscapeEarthquake);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianShutterRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianBackgroundRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianGeneralLevelRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs);
        requests.EscapeTypewriterSetupRequested = true;
        Encounter.PhaseTimer.Count = 32;
        Encounter.DeathPhase = frame.AlternateEscapeText
            ? MotherBrainDeathPhase.SpawnTimeBombSetSubtitle
            : MotherBrainDeathPhase.TypeOutZebesEscapeText;
        return false;
    }

    /// <summary>
    /// Runs the third phase death spawn time bomb set subtitle body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSpawnTimeBombSetSubtitle(MotherBrainRainbowBeamStepRequests requests)
    {
        // The subtitle delay steps first. Its step does *not* merely wait: it branches directly
        // into the typing step, so the underlying typewriter advances during all 32..0
        // subtitle-delay values. When it runs out the code installs the typing step,
        // spawns the persistent Japanese subtitle projectile, and falls through to
        // that same typewriter call. Keep the producer request separate because the
        //  allocator owns whether a place in the projectile pool is actually free.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.TypeOutZebesEscapeText;
            requests.TimeBombSetSubtitleSpawnRequested = true;
        }
        return true;
    }

    /// <summary>
    /// Runs the third phase death type out zebes escape text body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathTypeOutZebesEscapeText(RainbowBeamAttackFrame frame)
    {
        // The typing step calls the independently owned typewriter with the escape text list.
        // The boss requests the typing and waits for the typewriter to report the text
        // finished; the typewriter alone sets the character cadence.
        if (frame.TypewriterFinished)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.DoorExplodingStartTimer;
            Encounter.PhaseTimer.Count = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death door exploding start timer body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDoorExplodingStartTimer(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The keep-earthquake-going step always runs the periodic dust producer before touching the escape
        // countdown. The freshly loaded 32 therefore receives 33 calls (32..0,
        // then -1) and can emit dust even on the transition call.
        requests.EscapeDoorExplosion = GenerateEscapeDoorExplosion(frame.NextRandomNumber);
        if (Encounter.PhaseTimer.StepKeepsRunning())
            return false;

        // Ending the drained get-up, starting the escape timer, recording Mother Brain's defeat and the escape event
        // happen together here; each is requested of what owns it.
        requests.GetUpReleaseRequested = true;
        requests.MotherBrainEscapeTimerStartRequested = true;
        requests.MotherBrainDefeatRecordRequested = true;
        requests.ZebesTimebombEventRequested = true;
        Encounter.DeathPhase = MotherBrainDeathPhase.BlowUpEscapeDoor;
        _bursts.Restart();
        return false;
    }

    /// <summary>
    /// Runs the third phase death blow up escape door body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBlowUpEscapeDoor(MotherBrainRainbowBeamStepRequests requests)
    {
        // The door explosion spawns eight projectiles in the highest free places, as every spawn does, with
        // door fragments zero through seven. Allocation success is intentionally decided
        // by the shared enemy-projectile system, not assumed by the boss actor.
        foreach (MotherBrainDoorFragmentDefinition fragment in MotherBrainDoorFragmentDefinitions.Fragments)
            requests.EscapeDoorParticleSpawns.Add(new(fragment));

        Encounter.DeathPhase = MotherBrainDeathPhase.KeepEarthquakeGoing;
        requests.EscapeDoorObject = new(
            Block: new(0, 6),
            RoomObject: MotherBrainRoomMutation.EscapeDoor);
        return false;
    }

    /// <summary>
    /// Runs the third phase death keep earthquake going body phase. True when it moves to the next body phase and the game runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunThirdPhaseDeathKeepEarthquakeGoing(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // A producer may give the room's earthquake a length, which runs down outside enemy AI. Only a stopped earthquake
        // is run on again, indefinitely, with whatever shake the room has; a running one is left as it is.
        if (frame.RoomEarthquakeStopped)
            requests.KeepEarthquakeGoing();
        return false;
    }

    /// <summary>
    /// Runs the body's current <see cref="MotherBrainDeathPhase"/> phase. True when it moves to the next body phase and the game runs on
    /// into it within the same call.
    /// </summary>
    internal bool Run(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests) => Encounter.DeathPhase switch
    {
        MotherBrainDeathPhase.MoveToBackOfRoom => RunThirdPhaseDeathMoveToBackOfRoom(),
        MotherBrainDeathPhase.IdleWhilstExploding => RunThirdPhaseDeathIdleWhilstExploding(frame, requests),
        MotherBrainDeathPhase.StumbleToMiddleOfRoom => RunThirdPhaseDeathStumbleToMiddleOfRoom(frame, requests),
        MotherBrainDeathPhase.DisableBrainEffects => RunThirdPhaseDeathDisableBrainEffects(frame, requests),
        MotherBrainDeathPhase.SetupBodyFadeOut => RunThirdPhaseDeathSetupBodyFadeOut(frame, requests),
        MotherBrainDeathPhase.FadeOutBody => RunThirdPhaseDeathFadeOutBody(frame, requests),
        MotherBrainDeathPhase.FinalFewExplosions => RunThirdPhaseDeathFinalFewExplosions(frame, requests),
        MotherBrainDeathPhase.RealizeDecapitation => RunThirdPhaseDeathRealizeDecapitation(),
        MotherBrainDeathPhase.BrainFallsToGround => RunThirdPhaseDeathBrainFallsToGround(requests),
        MotherBrainDeathPhase.ReadyCorpse => RunThirdPhaseDeathReadyCorpse(),
        MotherBrainDeathPhase.SetupFadeToGrey => RunThirdPhaseDeathSetupFadeToGrey(),
        MotherBrainDeathPhase.FadeToGrey => RunThirdPhaseDeathFadeToGrey(requests),
        MotherBrainDeathPhase.CorpseTipsOver => RunThirdPhaseDeathCorpseTipsOver(),
        MotherBrainDeathPhase.CorpseRotsAway => RunThirdPhaseDeathCorpseRotsAway(frame, requests),
        MotherBrainDeathPhase.PauseBeforeEscape => RunThirdPhaseDeath20FrameDelay(),
        MotherBrainDeathPhase.ReadyEscapeWarning => RunThirdPhaseDeathReadyEscapeWarning(),
        MotherBrainDeathPhase.StartEscape => RunThirdPhaseDeathStartEscape(frame, requests),
        MotherBrainDeathPhase.SpawnTimeBombSetSubtitle => RunThirdPhaseDeathSpawnTimeBombSetSubtitle(requests),
        MotherBrainDeathPhase.TypeOutZebesEscapeText => RunThirdPhaseDeathTypeOutZebesEscapeText(frame),
        MotherBrainDeathPhase.DoorExplodingStartTimer => RunThirdPhaseDeathDoorExplodingStartTimer(frame, requests),
        MotherBrainDeathPhase.BlowUpEscapeDoor => RunThirdPhaseDeathBlowUpEscapeDoor(requests),
        MotherBrainDeathPhase.KeepEarthquakeGoing => RunThirdPhaseDeathKeepEarthquakeGoing(frame, requests),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainDeathPhase)} {Encounter.DeathPhase}."),
    };
}
