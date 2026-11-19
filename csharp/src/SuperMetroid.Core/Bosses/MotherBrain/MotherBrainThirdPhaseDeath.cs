using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
internal sealed class MotherBrainThirdPhaseDeath
{
    /// <summary>The corpse rotting away after the final death, with its own rot schedule.</summary>
    internal readonly MotherBrainCorpseRottingState CorpseRotting = new();

    /// <summary>The late fight's own state, with the walking and standing every late-fight phase shares.</summary>
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
    /// Emits the death-phase explosions at the timer cadence, consuming one global
    /// random value per projectile so later enemy AI sees the same RNG sequence.
    /// </summary>
    internal void GenerateDeathExplosions(
        MotherBrainDeathExplosionStyle style,
        Func<int> nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // A restarted interval runs out on the first update, so a burst after a restart is immediate; after a burst the
        // smoky and mixed explosions wait their own intervals. Each burst takes the group before the last,
        // continuing the place the ascent's dust left (#1872).
        if (!_bursts.IntervalRunsOut(style == MotherBrainDeathExplosionStyle.Mixed
            ? MotherBrainDeathExplosionDefinitions.MixedIntervalUpdates
            : MotherBrainDeathExplosionDefinitions.SmokyIntervalUpdates))
            return;

        MotherBrainDeathExplosionGroup group = _bursts.NextOf(MotherBrainDeathExplosionDefinitions.Groups);
        foreach (Offset16<RoomPixels> anchor in style == MotherBrainDeathExplosionStyle.Mixed ? group.Mixed : group.Smoky)
        {
            // The shared random sequence advances once per projectile, not once per visual batch.
            int random = nextRandomNumber();
            MotherBrainDeathExplosionAnimation animation = style == MotherBrainDeathExplosionStyle.Mixed
                ? MotherBrainRandomDefinitions.MixedDeathExplosionAnimation.Choose(random)
                : MotherBrainDeathExplosionAnimation.Smoke;
            requests.Add(new MotherBrainDeathExplosionRequest(
                Offset: anchor,
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
        // The door's dust continues the interval and the place the last death explosions left (#1872), then waits its
        // interval between bursts.
        if (!_bursts.IntervalRunsOut(MotherBrainDeathDefinitions.EscapeDoorDustIntervalUpdates))
            return null;

        Position16<RoomPixels> position = _bursts.NextOf(MotherBrainDeathDefinitions.EscapeDoorDustPositions);

        // The escape-door dust advances the one shared RNG exactly once per emitted projectile.
        // Its smoke chance selects the smoke animation; every other sample retains the preloaded small explosion.
        DustExplosionAnimation animation = MotherBrainRandomDefinitions.EscapeDoorDustSmokes.IsTakenBy(nextRandomNumber())
            ? DustExplosionAnimation.Smoke
            : DustExplosionAnimation.SmallExplosion;
        return new DustExplosionRequest(
            Position: position,
            Animation: animation,
            SoundEffect: EnemySound.SmallExplosion);
    }

    /// <summary>
    /// Walks her back, ignoring Samus and with no hit areas, to the back of the room. True when her next phase runs on
    /// in this same update.
    /// </summary>
    internal bool RetreatToBackOfRoom()
    {
        // Moving to the back, the body and the brain ignore Samus and have no hitboxes, and she walks back at medium
        // speed. Once she reaches the back of the room the idle's wait starts, and its first smoky explosion and update happen
        // in this same update.
        Encounter.Body.Contact = Encounter.Body.Contact.With(EnemyContact.IgnoresSamus);
        Encounter.Head.Contact = Encounter.Head.Contact.With(EnemyContact.IgnoresSamus);
        Encounter.HitboxesEnabled = MotherBrainHitboxParts.None;
        if (_state.RequestWalkBackward(MotherBrainFightDefinitions.BackOfRoomX, MotherBrainWalkSpeed.Medium))
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.IdleWhilstExploding;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.IdleExplodingUpdates);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Bursts smoky explosions over her at the back of the room until the wait runs out. True when her next phase runs
    /// on in this same update.
    /// </summary>
    internal bool ExplodeAtBackOfRoom(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.DeathPhase = MotherBrainDeathPhase.StumbleToMiddleOfRoom;
        return false;
    }

    /// <summary>
    /// Stumbles her forward really fast through smoky explosions toward the middle of the room, then lowers her neck
    /// and drools. True when her next phase runs on in this same update.
    /// </summary>
    internal bool StumbleForward(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The stumble smokes before she walks forward really fast toward the middle of the room. Standing on the
        // stumble's target still starts a whole step; only passing it, or reaching the arena's front limit, ends it.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (_state.RequestWalkForward(MotherBrainLateFightDefinitions.DyingStumbleX, MotherBrainWalkSpeed.ReallyFast))
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            Encounter.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            Encounter.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
            Encounter.DeathPhase = MotherBrainDeathPhase.DisableBrainEffects;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BeforeBrainEffectsStopUpdates);
        }
        return false;
    }

    /// <summary>
    /// When the wait runs out, stops her neck, drool, breath and health colours and lets her brain come away from the
    /// neck. True when her next phase runs on in this same update.
    /// </summary>
    internal bool LetGoOfBrain(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
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
            requests.Start(MotherBrainStepEffect.BrainDetached);
            // Disabling the brain's effects restarts the bursts before the fade-out explosions.
            _bursts.Restart();
            Encounter.DeathPhase = MotherBrainDeathPhase.SetupBodyFadeOut;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Bursts mixed explosions until the wait runs out, then starts her body's fade. True when her next phase runs on
    /// in this same update.
    /// </summary>
    internal bool WaitBeforeBodyFades(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathBodyFade.StartAfter(1);
            Encounter.DeathPhase = MotherBrainDeathPhase.FadeOutBody;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Fades her flickering body to black through mixed explosions, then hides it. True when her next phase runs on in
    /// this same update.
    /// </summary>
    internal bool FadeOutBody(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The body flickers on every update. The fade shows its blackening colourings; the update it is over shows none
        // and hides the body.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        requests.Start(MotherBrainStepEffect.BodyFlickers);
        if (Encounter.DeathBodyFade.Step() is { } image)
            requests.DeathFadeImage = image;
        else if (Encounter.DeathBodyFade.IsOver)
        {
            requests.Start(MotherBrainStepEffect.BodyCleared);
            RoomEnemyPlace body = Encounter.Body;
            body.Presence = body.Presence.With(EnemyPresence.Hidden);
            body.Processing = body.Processing.Without(EnemyProcessing.Animates);
            body.CollidesByFrameHitboxes = false;
            body.AnimationClock.Halt();
            Encounter.DeathPhase = MotherBrainDeathPhase.FinalFewExplosions;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.FinalExplosionsUpdates);
        }
        return false;
    }

    /// <summary>
    /// Bursts the last mixed explosions until the wait runs out. True when her next phase runs on in this same update.
    /// </summary>
    internal bool ExplodeAfterBodyFades(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (Encounter.PhaseTimer.StepRunsOut())
            Encounter.DeathPhase = MotherBrainDeathPhase.RealizeDecapitation;
        return false;
    }

    /// <summary>
    /// Shows her decapitated and starts her brain's fall from rest. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal bool DropBrain()
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
    /// Drops her brain faster each update until it lands on the floor, shaking the room. True when her next phase runs
    /// on in this same update.
    /// </summary>
    internal bool FallBrainToFloor(MotherBrainRainbowBeamStepRequests requests)
    {
        // The brain gains fall speed each update and falls the speed's whole pixels.
        // Reaching the floor clamps the head and starts the brain's landing earthquake.
        _state.BrainFallSpeed += MotherBrainDeathDefinitions.BrainFallAcceleration;
        int fallingDisplacement = _state.BrainFallSpeed.WholePixels;
        Position16<RoomPixels> candidate = _state.BrainPosition + new Offset16<RoomPixels>(0, fallingDisplacement);
        if (candidate.Y >= MotherBrainFightDefinitions.BrainFloorY)
        {
            requests.StartEarthquake(MotherBrainDeathDefinitions.BrainLandingEarthquake, MotherBrainLateFightDefinitions.BrainLandingShakeUpdates);
            _state.MoveBrainTo(_state.BrainPosition.WithY(MotherBrainFightDefinitions.BrainFloorY));
            Encounter.DeathPhase = MotherBrainDeathPhase.BrainSettles;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BrainSettlesUpdates);
        }
        else
        {
            _state.MoveBrainTo(candidate);
        }
        return false;
    }

    /// <summary>
    /// Lets her landed brain settle until the wait runs out, then starts the wait before her corpse greys. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool SettleCorpse()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.SetupFadeToGrey;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BeforeCorpseGreyUpdates);
        }
        return false;
    }

    /// <summary>
    /// Waits, then starts her corpse's fade to grey. True when her next phase runs on in this same update.
    /// </summary>
    internal bool WaitBeforeCorpseGreys()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.CorpseGreying.StartAfter(1);
            Encounter.DeathPhase = MotherBrainDeathPhase.FadeToGrey;
        }
        return false;
    }

    /// <summary>
    /// Greys her corpse through its fade, then shows the corpse and starts the wait before it tips over. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool GreyCorpse(MotherBrainRainbowBeamStepRequests requests)
    {
        if (Encounter.CorpseGreying.Step() is { } image)
            requests.CorpseFadeImage = image;
        else if (Encounter.CorpseGreying.IsOver)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.Corpse);
            Encounter.DeathPhase = MotherBrainDeathPhase.CorpseTipsOver;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.CorpseTipsOverUpdates);
        }
        return false;
    }

    /// <summary>
    /// Waits as the corpse tips over, then it stops meeting Samus and starts to rot. True when her next phase runs on
    /// in this same update.
    /// </summary>
    internal bool TipCorpseOver()
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
    /// Rots her corpse away with its dust, then hides the brain, stops the music and queues the escape music. True when
    /// her next phase runs on in this same update.
    /// </summary>
    internal bool RotCorpse(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        MotherBrainCorpseRottingStepResult corpseRotting = CorpseRotting.Step(
            _state.BrainPosition,
            frame.RandomNumberSeed,
            frame.EnemyPassCount);
        requests.CorpseDustRequests.AddRange(corpseRotting.DustRequests);
        if (corpseRotting.StillRotting)
            return false;

        // Once the corpse has rotted away the brain is hidden, stops animating and loses its frames' hitboxes, and
        // the music stops and the escape music is queued after the short wait every queued song takes. The pause's first update is
        // this same update.
        RoomEnemyPlace brain = Encounter.Head;
        brain.Presence = brain.Presence.With(EnemyPresence.Hidden);
        brain.Processing = brain.Processing.Without(EnemyProcessing.Animates);
        brain.CollidesByFrameHitboxes = false;
        brain.AnimationClock.Halt();
        requests.Start(MotherBrainStepEffect.MusicStop);   // Queue the music stop.
        requests.Start(MotherBrainStepEffect.EscapeMusic); // Queue the escape music.
        Encounter.DeathPhase = MotherBrainDeathPhase.PauseBeforeEscape;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.BeforeEscapeUpdates);
        return true;
    }

    /// <summary>
    /// Pauses before the escape, once her corpse has rotted away, until the wait runs out. True when her next phase
    /// runs on in this same update.
    /// </summary>
    internal bool PauseBeforeEscape()
    {
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            // The decapitated brain remains at the corpse coordinates for the full delay, then is parked in the room's
            // top-left corner. The parking is gameplay-observable (#2257): the hidden brain still meets Samus's bombs by its
            // radius, its frames' hitboxes being off, so at the corpse a bomb laid during the escape would strike it.
            _state.MoveBrainTo(Position16<RoomPixels>.Zero);
            Encounter.DeathPhase = MotherBrainDeathPhase.HoldBeforeEscape;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.HoldBeforeEscapeUpdates);
        }
        return false;
    }

    /// <summary>
    /// Holds the room, the brain parked, until the wait runs out, then starts the wait for the escape's start, whose first
    /// update is the hold's last. True when her next phase runs on in this same update.
    /// </summary>
    internal bool HoldBeforeEscape()
    {
        if (!Encounter.PhaseTimer.StepRunsOut())
            return false;

        Encounter.DeathPhase = MotherBrainDeathPhase.StartEscape;
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.EscapeStartUpdates);
        return true;
    }

    /// <summary>
    /// Waits for the escape's start, then starts the escape's music, earthquake, red flashing and text. True when her next
    /// phase runs on in this same update.
    /// </summary>
    internal bool StartEscape(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        if (!Encounter.PhaseTimer.StepRunsOut())
            return false;

        // The wait's last update starts the escape: the escape music, the quake held indefinitely, four colour
        // effects and the escape text. The door's
        // fragments are pre-coloured in the exploded door's colours, so its colour load is not kept (#1544).
        requests.Start(MotherBrainStepEffect.EscapeMusicTrack);
        requests.StartIndefiniteEarthquake(MotherBrainDeathDefinitions.EscapeEarthquake);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianShutterRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianBackgroundRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianGeneralLevelRedFlashing);
        requests.EscapeColourEffectRequests.Add(ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs);
        requests.Start(MotherBrainStepEffect.EscapeTypewriterSetup);
        Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.EscapeTextDelayUpdates);
        Encounter.DeathPhase = frame.AlternateEscapeText
            ? MotherBrainDeathPhase.SpawnTimeBombSetSubtitle
            : MotherBrainDeathPhase.TypeOutZebesEscapeText;
        return false;
    }

    /// <summary>
    /// Types the escape text on until the wait runs out, then asks for the Japanese time-bomb subtitle. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool ShowTimeBombSubtitle(MotherBrainRainbowBeamStepRequests requests)
    {
        // The subtitle's delay does not merely wait: the escape text types on in every update of it. When it runs out the typing proper begins and the Japanese subtitle is requested, which spawns
        // only where the enemy projectiles have room; the text types on in that same update.
        if (Encounter.PhaseTimer.StepRunsOut())
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.TypeOutZebesEscapeText;
            requests.Start(MotherBrainStepEffect.TimeBombSetSubtitle);
        }
        return true;
    }

    /// <summary>
    /// Waits for the escape text to finish typing, then starts the wait before the escape door bursts. True when her
    /// next phase runs on in this same update.
    /// </summary>
    internal bool TypeEscapeText(RainbowBeamAttackFrame frame)
    {
        // The escape text types on by its own cadence; Mother Brain's death waits for it to finish.
        if (frame.TypewriterFinished)
        {
            Encounter.DeathPhase = MotherBrainDeathPhase.DoorExplodingStartTimer;
            Encounter.PhaseTimer.LastFor(MotherBrainLateFightDefinitions.EscapeDoorCountdownUpdates);
        }
        return false;
    }

    /// <summary>
    /// Bursts dust over the escape door until the wait runs out, then starts the escape timer and records her defeat.
    /// True when her next phase runs on in this same update.
    /// </summary>
    internal bool BurstDoorAndStartTimer(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The door's dust bursts before the countdown steps, so dust may burst even on the countdown's last update.
        requests.EscapeDoorExplosion = GenerateEscapeDoorExplosion(frame.NextRandomNumber);
        if (Encounter.PhaseTimer.StepKeepsRunning())
            return false;

        // Ending the drained get-up, starting the escape timer, recording Mother Brain's defeat and the escape event
        // happen together here; each is requested of what owns it.
        requests.Start(MotherBrainStepEffect.GetUpRelease);
        requests.Start(MotherBrainStepEffect.EscapeTimerStart);
        requests.Start(MotherBrainStepEffect.DefeatRecord);
        requests.Start(MotherBrainStepEffect.ZebesTimebombEvent);
        Encounter.DeathPhase = MotherBrainDeathPhase.BlowUpEscapeDoor;
        _bursts.Restart();
        return false;
    }

    /// <summary>
    /// Blows the escape door into its fragments and opens it. True when her next phase runs on in this same update.
    /// </summary>
    internal bool BlowUpEscapeDoor(MotherBrainRainbowBeamStepRequests requests)
    {
        // The door bursts into its eight fragments, each spawning where the enemy projectiles have room.
        foreach (MotherBrainDoorFragmentDefinition fragment in MotherBrainDoorFragmentDefinitions.Fragments)
            requests.EscapeDoorParticleSpawns.Add(new(fragment));

        Encounter.DeathPhase = MotherBrainDeathPhase.KeepEarthquakeGoing;
        requests.EscapeDoorObject = new(
            Block: MotherBrainDeathDefinitions.EscapeDoorBlock,
            RoomObject: MotherBrainRoomMutation.EscapeDoor);
        return false;
    }

    /// <summary>
    /// Keeps the escape's earthquake going whenever it has stopped. True when her next phase runs on in this same
    /// update.
    /// </summary>
    internal static bool KeepEarthquakeGoing(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
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
        MotherBrainDeathPhase.MoveToBackOfRoom => RetreatToBackOfRoom(),
        MotherBrainDeathPhase.IdleWhilstExploding => ExplodeAtBackOfRoom(frame, requests),
        MotherBrainDeathPhase.StumbleToMiddleOfRoom => StumbleForward(frame, requests),
        MotherBrainDeathPhase.DisableBrainEffects => LetGoOfBrain(frame, requests),
        MotherBrainDeathPhase.SetupBodyFadeOut => WaitBeforeBodyFades(frame, requests),
        MotherBrainDeathPhase.FadeOutBody => FadeOutBody(frame, requests),
        MotherBrainDeathPhase.FinalFewExplosions => ExplodeAfterBodyFades(frame, requests),
        MotherBrainDeathPhase.RealizeDecapitation => DropBrain(),
        MotherBrainDeathPhase.BrainFallsToGround => FallBrainToFloor(requests),
        MotherBrainDeathPhase.BrainSettles => SettleCorpse(),
        MotherBrainDeathPhase.SetupFadeToGrey => WaitBeforeCorpseGreys(),
        MotherBrainDeathPhase.FadeToGrey => GreyCorpse(requests),
        MotherBrainDeathPhase.CorpseTipsOver => TipCorpseOver(),
        MotherBrainDeathPhase.CorpseRotsAway => RotCorpse(frame, requests),
        MotherBrainDeathPhase.PauseBeforeEscape => PauseBeforeEscape(),
        MotherBrainDeathPhase.HoldBeforeEscape => HoldBeforeEscape(),
        MotherBrainDeathPhase.StartEscape => StartEscape(frame, requests),
        MotherBrainDeathPhase.SpawnTimeBombSetSubtitle => ShowTimeBombSubtitle(requests),
        MotherBrainDeathPhase.TypeOutZebesEscapeText => TypeEscapeText(frame),
        MotherBrainDeathPhase.DoorExplodingStartTimer => BurstDoorAndStartTimer(frame, requests),
        MotherBrainDeathPhase.BlowUpEscapeDoor => BlowUpEscapeDoor(requests),
        MotherBrainDeathPhase.KeepEarthquakeGoing => KeepEarthquakeGoing(frame, requests),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainDeathPhase)} {Encounter.DeathPhase}."),
    };
}
