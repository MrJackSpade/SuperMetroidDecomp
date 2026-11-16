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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathMoveToBackOfRoom()
    {
        // Moving to the back, the body and the brain ignore Samus and have no hitboxes, and she walks back at medium
        // speed. Once she reaches X 40 the idle's timer of 128 starts, and its first smoky explosion and step happen
        // in this same update.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathStumbleToMiddleOfRoom(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The stumble smokes before she walks forward really fast toward X 96. Standing at 96 still starts a whole
        // step; only passing it, or reaching the arena's limit at 128, ends the stumble.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathDisableBrainEffects(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            // Her neck stops, her drool and breath stop, her colours stop following her health, the brain comes away
            // from the neck, and the body's fade-out is set up in this same update.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeOutBody(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The body flickers on every update, apart from the colours' seventeen-update cadence. The fade's sixteen steps show its blackening colourings; its
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
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathRealizeDecapitation()
    {
        // The body starts the decapitated animation and the brain's own draw without its neck, which hands the brain's
        // position from the articulated neck to the fall; it starts the brain falling from rest, and the fall takes
        // its first step in this same update.
        _state.SetHeadAnimation(MotherBrainHeadSegment.Decapitated);
        Encounter.BrainAction = MotherBrainBrainAction.SetupBrainToBeDrawn;
        _state.BrainFallSpeed = SignedEightEight.Zero;
        Encounter.DeathPhase = MotherBrainDeathPhase.BrainFallsToGround;
        return true;
    }

    /// <summary>
    /// Runs the third phase death brain falls to ground body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyCorpse()
    {
        // Each update readies one update of her corpse; the last moves on to the fade in that same update.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
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
    /// on into it in the same update.
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

        // Once the corpse has rotted away the brain is hidden, stops animating and loses its frames' hitboxes, and
        // the music stops and the escape music is queued with the ordinary eight-frame delay. The pause's timer of
        // twenty takes its first step in this same update, so it reads nineteen when the update ends.
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
    /// it moves to the next body phase and the game runs on into it in the same update.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyEscapeWarning()
    {
        ReadyEscapeWarning();
        if (EscapeWarningReadyingElapsed == MotherBrainReadyingDefinitions.EscapeWarningDuration)
        {
            // The last readying update starts the escape, which readies the first update of the escape door's
            // explosion in this same update.
            Encounter.DeathPhase = MotherBrainDeathPhase.StartEscape;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death start escape body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathStartEscape(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        ReadyExplodedDoor();
        if (ExplodedDoorReadyingElapsed != MotherBrainReadyingDefinitions.ExplodedDoorDuration)
            return false;

        // The last readying update starts the escape: the escape music, the quake held indefinitely, four colour
        // effects and the escape text. The door's
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathSpawnTimeBombSetSubtitle(MotherBrainRainbowBeamStepRequests requests)
    {
        // The subtitle's delay does not merely wait: the escape text types on in every update of it, from 32 down
        // to zero. When it runs out the typing proper begins and the Japanese subtitle is requested, which spawns
        // only where the enemy projectiles have room; the text types on in that same update.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.TypeOutZebesEscapeText;
            requests.TimeBombSetSubtitleSpawnRequested = true;
        }
        return true;
    }

    /// <summary>
    /// Runs the third phase death type out zebes escape text body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathTypeOutZebesEscapeText(RainbowBeamAttackFrame frame)
    {
        // The escape text types on by its own cadence; Mother Brain's death waits for it to finish.
        if (frame.TypewriterFinished)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.DoorExplodingStartTimer;
            Encounter.PhaseTimer.Count = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death door exploding start timer body phase. True when it moves to the next body phase and the game runs
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathDoorExplodingStartTimer(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The door's dust bursts before the countdown steps, so the countdown of 32 takes 33 updates and dust may
        // burst even on the update it runs out.
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
    /// on into it in the same update.
    /// </summary>
    internal bool RunThirdPhaseDeathBlowUpEscapeDoor(MotherBrainRainbowBeamStepRequests requests)
    {
        // The door bursts into its eight fragments, each spawning where the enemy projectiles have room.
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
    /// on into it in the same update.
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
    /// into it in the same update.
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
