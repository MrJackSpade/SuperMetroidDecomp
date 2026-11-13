using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
internal sealed class MotherBrainThirdPhaseDeath
{
    /// <summary>The corpse rotting away after the final death, with its own rot schedule.</summary>
    internal readonly MotherBrainCorpseRottingState CorpseRotting = new();

    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head animations and the brain's flags, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainThirdPhaseDeath(MotherBrainLateFightState state)
    {
        _state = state;
    }

    /// <summary>How the body meets Samus and her shots; death makes it ignore Samus.</summary>
    public EnemyContact BodyContact { get; internal set; }

    /// <summary>What the enemy passes run for the body; fade completion stops its animation.</summary>
    public EnemyProcessing BodyProcessing { get; internal set; }

    /// <summary>Whether the body is drawn and still occupies its slot; fade completion hides it.</summary>
    public EnemyPresence BodyPresence { get; internal set; }

    /// <summary>Whether the body collides by its frames' hitboxes; cleared when the faded body becomes non-interactive.</summary>
    public bool BodyCollidesByFrameHitboxes { get; internal set; }

    /// <summary>Whether the body's animation just installed a frame; cleared when the faded body becomes non-interactive.</summary>
    public bool BodyShowsNewFrame { get; internal set; }

    /// <summary>How the brain meets Samus and her shots; death makes it ignore Samus.</summary>
    public EnemyContact BrainContact { get; internal set; }

    /// <summary>What the enemy passes run for the brain; the end of rotting stops its animation.</summary>
    public EnemyProcessing BrainProcessing { get; internal set; }

    /// <summary>Whether the brain is drawn and still occupies its slot; the end of rotting hides it.</summary>
    public EnemyPresence BrainPresence { get; internal set; }

    /// <summary>Whether the brain collides by its frames' hitboxes; cleared when the rot animation completes.</summary>
    public bool BrainCollidesByFrameHitboxes { get; internal set; }

    /// <summary>Whether the brain's animation just installed a frame; cleared when the rot animation completes.</summary>
    public bool BrainShowsNewFrame { get; internal set; }

    /// <summary>Shared Mother Brain hitbox-enable flag cleared at both death boundaries.</summary>
    public bool HitboxesEnabled { get; internal set; } = true;

    /// <summary>
    /// The explosion interval timer of the death explosions,
    /// aliased with the body sub-function counter.
    /// </summary>
    public int DeathExplosionIntervalTimer { get; internal set; }

    /// <summary>
    /// One value under three names: the ascent dust's body
    /// sub-function timer, the seven-record death-explosion index and the
    /// four-record escape-door dust index. Each effect inherits the value
    /// the previous one left unless code explicitly clears it.
    /// </summary>
    public int DeathAndEscapeExplosionIndex { get; internal set; }

    /// <summary>Whether the death has reached the point where the start of Mother Brain's body is cleared (#1466).</summary>
    public bool BodyClearRequested { get; private set; }

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

    /// <summary>
    /// Set when the decapitation step installs the brain-slot draw setup before the decapitated head
    /// starts falling. The separate brain AI is not silently folded into the body function.
    /// </summary>
    public bool BrainDrawSetupRequested { get; private set; }

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
        Func<int>? nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // The death explosions decrement before testing the sign. A cleared timer therefore runs past zero and emits
        // immediately. Crucially, the interval literal remains in A across the memory DEC,
        // so the smoky/mixed caller writes exactly 16/8 after an expiry.
        if (--DeathExplosionIntervalTimer >= 0)
            return;

        DeathExplosionIntervalTimer = style == MotherBrainDeathExplosionStyle.Mixed ? 8 : 16;
        if (--DeathAndEscapeExplosionIndex < 0)
            DeathAndEscapeExplosionIndex = MotherBrainDeathExplosionDefinitions.GroupCount - 1;

        int simultaneousCount = style == MotherBrainDeathExplosionStyle.Mixed ? MotherBrainDeathExplosionDefinitions.AnchorsPerGroup : 2;
        int pairIndex = DeathAndEscapeExplosionIndex * MotherBrainDeathExplosionDefinitions.AnchorsPerGroup;
        for (int explosionIndex = 0; explosionIndex < simultaneousCount; explosionIndex++)
        {
            // The global RNG is called once per projectile, not once per visual batch. A
            // callback is required here so the owning frame runtime advances its real global
            // state; silently deriving private random values would desynchronize later AI.
            if (nextRandomNumber is null)
            {
                throw new InvalidOperationException(
                    "Mother Brain death explosions require the global next-random-number producer.");
            }

            int random = nextRandomNumber();
            int parameter = style == MotherBrainDeathExplosionStyle.Mixed
                ? MotherBrainRandomDefinitions.MixedDeathExplosionAnimation.Choose(random)
                : 1;
            requests.Add(new MotherBrainDeathExplosionRequest(
                Offset: MotherBrainDeathExplosionDefinitions.Anchor(pairIndex + explosionIndex),
                ProjectileParameter: parameter,
                SoundEffect: MotherBrainSoundDefinitions.Explosion));
        }
    }

    /// <summary>
    /// Emits the next four-frame escape-door blast from its repeating position table.
    /// Returns no request on frames before the shared interval timer expires.
    /// </summary>
    private DustExplosionRequest? GenerateEscapeDoorExplosion(
        Func<int>? nextRandomNumber)
    {
        // The escape-door dust shares the old death-explosion interval value but replaces its cadence with
        // four. A cleared timer underflows on the first call, so a new explosion is immediate.
        if (--DeathExplosionIntervalTimer >= 0)
            return null;

        DeathExplosionIntervalTimer = 4;
        if (--DeathAndEscapeExplosionIndex < 0)
            DeathAndEscapeExplosionIndex = 3;

        // The explosion positions repeat in the order 3, 2, 1, 0; the switch spells out each X/Y pair.
        Position16<RoomPixels> position = DeathAndEscapeExplosionIndex switch
        {
            0 => new(8, 108),
            1 => new(24, 128),
            2 => new(9, 144),
            3 => new(24, 116),
            _ => throw new InvalidOperationException(
                $"Escape-door explosion index {DeathAndEscapeExplosionIndex} escaped its 0..3 range."),
        };

        if (nextRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Escape-door explosions require the global next-random-number producer.");
        }

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
    /// Runs the third phase death move to back of room body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathMoveToBackOfRoom(MotherBrainRainbowBeamStepRequests requests)
    {
        // The move-to-back step makes both enemy slots non-interactive and disables the shared
        // hitboxes on every call, then reuses the ordinary medium-speed backward
        // walk producer. Its target reached means X 40 has been reached/overshot; the new 128
        // timer falls through so the first smoky explosion and decrement happen now.
        BodyContact = BodyContact.With(EnemyContact.IgnoresSamus);
        BrainContact = BrainContact.With(EnemyContact.IgnoresSamus);
        HitboxesEnabled = false;
        MotherBrainWalkResult dyingRetreat = _state.RequestWalkBackward(40, animationDelay: 6);
        requests.BodyWalkRequested = dyingRetreat.Requested;
        if (dyingRetreat.ReachedTarget)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding;
            _state.FunctionTimer = 128;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death idle whilst exploding body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathIdleWhilstExploding(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (--_state.FunctionTimer < 0)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom;
        return false;
    }

    /// <summary>
    /// Runs the third phase death stumble to middle of room body function. True when it installs the next body function and the game runs
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
        requests.BodyWalkRequested = dyingAdvance.Requested;
        if (dyingAdvance.ReachedTarget)
        {
            _state.SetHeadAnimation(MotherBrainHeadSegment.DyingDrool);
            _state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects;
            _state.FunctionTimer = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death disable brain effects body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDisableBrainEffects(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        if (--_state.FunctionTimer < 0)
        {
            // Disabling the brain's effects removes the neck/breath/palette producers, copies the body's
            // shared colours into the brain's, performs one health-palette refresh, and then
            // deliberately falls through with the already-negative timer.
            _state.LowerNeckMovement = MotherBrainNeckMovement.None;
            _state.UpperNeckMovement = MotherBrainNeckMovement.None;
            _state.DroolGenerationEnabled = false;
            _state.DisableSmallPurpleBreathGeneration();
            _state.HealthColoursEnabled = false;
            requests.ColoursRequested = true;
            // Disabling the brain's effects clears the death-explosion counters before the fade-out explosions.
            DeathExplosionIntervalTimer = 0;
            DeathAndEscapeExplosionIndex = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup body fade out body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupBodyFadeOut(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (--_state.FunctionTimer < 0)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody;
            _state.FunctionTimer = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade out body body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeOutBody(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // Body flickering runs every AI call, independently of the seventeen-call
        // palette cadence. Counter values 0..15 copy blackening palettes; value 16
        // is past the last palette and completes the transition without a copy.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (--_state.FunctionTimer < 0)
        {
            _state.FunctionTimer = 16;
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.ColoursRequested |= paletteIndex < 16;
            if (paletteIndex >= 16)
            {
                BodyClearRequested = true;
                BodyPresence = BodyPresence.With(EnemyPresence.Hidden);
                BodyProcessing = BodyProcessing.Without(EnemyProcessing.Animates);
                BodyCollidesByFrameHitboxes = false;
                BodyShowsNewFrame = false;
                _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions;
                _state.FunctionTimer = 16;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death final few explosions body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFinalFewExplosions(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        if (--_state.FunctionTimer < 0)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation;
        return false;
    }

    /// <summary>
    /// Runs the third phase death realize decapitation body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathRealizeDecapitation()
    {
        // The body starts the decapitated animation and a separate brain-slot draw
        // setup, clears its 8.8 falling velocity, then falls through immediately.
        _state.SetHeadAnimation(MotherBrainHeadSegment.Decapitated);
        BrainDrawSetupRequested = true;
        _state.FunctionTimer = 0;
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround;
        return true;
    }

    /// <summary>
    /// Runs the third phase death brain falls to ground body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBrainFallsToGround(MotherBrainRainbowBeamStepRequests requests)
    {
        // The fall treats FunctionTimer as unsigned 8.8 velocity: add 32, use only
        // its whole pixels as this frame's displacement, and accumulate Y.
        // Crossing 196 clamps the head and starts the brain's landing earthquake for 20 frames.
        _state.FunctionTimer += 32;
        int fallingDisplacement = UnsignedEightEight.Whole(_state.FunctionTimer);
        Position16<RoomPixels> candidate = _state.BrainPosition + new Offset16<RoomPixels>(0, fallingDisplacement);
        if (candidate.Y >= 196)
        {
            requests.StartEarthquake(MotherBrainDeathDefinitions.BrainLandingEarthquake, 20);
            _state.BrainPosition = _state.BrainPosition.WithY(196);
            CorpseReadyingElapsed = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse;
            _state.FunctionTimer = 256;
        }
        else
        {
            _state.BrainPosition = candidate;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load corpse tiles body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyCorpse()
    {
        // Each call readies one update of her corpse; the last moves on to the fade on that same call.
        ReadyCorpse();
        if (CorpseReadyingElapsed == MotherBrainReadyingDefinitions.CorpseDuration)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey;
            _state.FunctionTimer = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup fade to grey body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupFadeToGrey()
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey;
            _state.FunctionTimer = 0;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade to grey body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        if (--_state.FunctionTimer < 0)
        {
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.ColoursRequested = paletteIndex < 8;
            if (paletteIndex < 8)
            {
                _state.FunctionTimer = 16;
            }
            else
            {
                _state.SetHeadAnimation(MotherBrainHeadSegment.Corpse);
                _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver;
                _state.FunctionTimer = 256;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse tips over body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathCorpseTipsOver()
    {
        if (--_state.FunctionTimer < 0)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway;
            BrainContact = BrainContact.With(EnemyContact.IgnoresSamus);
            HitboxesEnabled = false;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse rots away body function. True when it installs the next body function and the game runs
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
        BrainPresence = BrainPresence.With(EnemyPresence.Hidden);
        BrainProcessing = BrainProcessing.Without(EnemyProcessing.Animates);
        BrainCollidesByFrameHitboxes = false;
        BrainShowsNewFrame = false;
        requests.MusicStopQueued = true;   // Queue the music stop.
        requests.EscapeMusicQueued = true; // Queue the escape music.
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay;
        _state.FunctionTimer = 20;
        return true;
    }

    /// <summary>
    /// Runs the third phase death20 frame delay body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeath20FrameDelay()
    {
        if (--_state.FunctionTimer < 0)
        {
            // The decapitated brain remains at the corpse coordinates for the full
            // delay. Loading the escape-timer tiles finally parks it at (0,0) before escape-timer tile
            // loading begins; this is a real actor-coordinate mutation, not rendering.
            _state.BrainPosition = Position16<RoomPixels>.Zero;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load escape timer tiles body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathReadyEscapeWarning()
    {
        ReadyEscapeWarning();
        if (EscapeWarningReadyingElapsed == MotherBrainReadyingDefinitions.EscapeWarningDuration)
        {
            // The last readying update installs the escape's start and falls through far enough to ready the escape
            // door's explosion's first update during this same enemy AI call.
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathStartEscape;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death start escape body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathStartEscape(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        ReadyExplodedDoor();
        if (ExplodedDoorReadyingElapsed != MotherBrainReadyingDefinitions.ExplodedDoorDuration)
            return false;

        // The last readying update performs the full escape-door handoff on that call: start escape music, hold the quake
        // indefinitely, create four palette-FX objects and initialize text. The door's
        // fragments are pre-coloured in the exploded door's colours, so its colour load is not kept (#1544).
        requests.EscapeMusicTrackQueued = true;        // Eight-frame-delay escape song.
        requests.StartEarthquake(MotherBrainDeathDefinitions.EscapeEarthquake, WrappingCountdown.LongestStart);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianShutterRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianBackgroundRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianGeneralLevelRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs);
        requests.EscapeTypewriterSetupRequested = true;
        _state.FunctionTimer = 32;
        _state.Phase = frame.AlternateEscapeText
            ? MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle
            : MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText;
        return false;
    }

    /// <summary>
    /// Runs the third phase death spawn time bomb set subtitle body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSpawnTimeBombSetSubtitle(MotherBrainRainbowBeamStepRequests requests)
    {
        // The subtitle delay decrements first. Its sign test does *not* merely wait: it branches directly
        // into the typing step, so the underlying typewriter advances during all 32..0
        // subtitle-delay values. On underflow the code installs the typing step,
        // spawns the persistent Japanese subtitle projectile, and falls through to
        // that same typewriter call. Keep the producer request separate because the
        //  allocator owns whether a physical slot is actually available.
        if (--_state.FunctionTimer < 0)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText;
            requests.TimeBombSetSubtitleSpawnRequested = true;
        }
        return true;
    }

    /// <summary>
    /// Runs the third phase death type out zebes escape text body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathTypeOutZebesEscapeText(RainbowBeamAttackFrame frame)
    {
        // The typing step calls the independently owned typewriter with the escape text list.
        // The boss requests the typing and waits for the typewriter to report the text
        // finished; the typewriter alone sets the character cadence.
        if (frame.TypewriterFinished)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer;
            _state.FunctionTimer = 32;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death door exploding start timer body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDoorExplodingStartTimer(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The keep-earthquake-going step always runs the periodic dust producer before touching the escape
        // countdown. The freshly loaded 32 therefore receives 33 calls (32..0,
        // then -1) and can emit dust even on the transition call.
        requests.EscapeDoorExplosion = GenerateEscapeDoorExplosion(frame.NextRandomNumber);
        if (--_state.FunctionTimer >= 0)
            return false;

        // Samus command fifteen, the escape timer status, Mother Brain's boss bit, and the escape event
        // belong to four different subsystems. Publish every edge instead of
        // silently writing the wrong debug room's area flags inside this actor.
        requests.TimerHandlingEnableRequested = true;
        requests.MotherBrainEscapeTimerStartRequested = true;
        requests.MotherBrainBossBitRequested = true;
        requests.ZebesTimebombEventRequested = true;
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor;
        DeathExplosionIntervalTimer = 0;
        DeathAndEscapeExplosionIndex = 0;
        return false;
    }

    /// <summary>
    /// Runs the third phase death blow up escape door body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBlowUpEscapeDoor(MotherBrainRainbowBeamStepRequests requests)
    {
        // The door explosion invokes the ordinary highest-free-slot allocator eight times with
        // door fragments zero through seven. Allocation success is intentionally decided
        // by the shared enemy-projectile system, not assumed by the boss actor.
        for (int parameter = 0; parameter < 8; parameter++)
            requests.EscapeDoorParticleSpawns.Add(new(parameter));

        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing;
        requests.EscapeDoorObject = new(
            Block: new(0, 6),
            RoomObject: MotherBrainRoomMutation.EscapeDoor);
        return false;
    }

    /// <summary>
    /// Runs the third phase death keep earthquake going body function. True when it installs the next body function and the game runs
    /// on into it within the same call.
    /// </summary>
    internal static bool RunThirdPhaseDeathKeepEarthquakeGoing(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // The room's earthquake runs down outside enemy AI. Only a stopped earthquake is run on again, for the
        // longest length, with whatever shake the room has; a running one is left as it is.
        if (frame.RoomEarthquakeStopped)
            requests.RunEarthquakeOn(WrappingCountdown.LongestStart);
        return false;
    }
}
