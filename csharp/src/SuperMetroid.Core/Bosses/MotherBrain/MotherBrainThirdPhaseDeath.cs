using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's final death and the escape it starts: explosions, the fades, the corpse, the escape text, timer and door.</summary>
internal sealed class MotherBrainThirdPhaseDeath
{
    /// <summary>Advances the post-fight corpse animation and its room effects.</summary>
    internal readonly MotherBrainCorpseRottingState CorpseRotting = new();

    /// <summary>The state every late-fight phase reads and writes: the body function and its timer, the neck and head programs, the brain's flags and the earthquake, with the walking and posture requests they share.</summary>
    private readonly MotherBrainLateFightState _state;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainThirdPhaseDeath(EnemyStagingSurfaces? corpseStaging, MotherBrainLateFightState state)
    {
        CorpseStaging = corpseStaging;
        _state = state;
    }

    /// <summary>The bound staging surfaces; corpse rotting requires them.</summary>
    [AllowNull]
    private EnemyStagingSurfaces CorpseStaging => field ?? throw new InvalidDataException(
        "Mother Brain corpse rotting requires the room's enemy staging surfaces.");

    /// <summary>
    /// Exact body enemy property word touched by `$A9:AEE1` and `$A9:AFF7`. Keeping the raw
    /// flags avoids guessing names for engine-wide enemy-property bits before bank `$A0` is
    /// fully translated: death sets `$0400`, then fade completion sets `$0100` and clears
    /// `$2000` with the cartridge's literal OR/AND sequence.
    /// </summary>
    public EnemyPropertyWord BodyProperties { get; internal set; }

    /// <summary>Exact brain enemy property word; death/rotting sets raw bits `$0400/$0100`.</summary>
    public EnemyPropertyWord BrainProperties { get; internal set; }

    /// <summary>Second brain property word cleared when the rot animation completes.</summary>
    public EnemyExtraPropertyWord BrainProperties2 { get; internal set; }

    /// <summary>Second body property word cleared when the faded body becomes non-interactive.</summary>
    public EnemyExtraPropertyWord BodyProperties2 { get; internal set; }

    /// <summary>Shared Mother Brain hitbox-enable word cleared at both death boundaries.</summary>
    public bool HitboxesEnabled { get; internal set; } = true;

    /// <summary>
    /// Native word <c>$0FF0</c>: the explosion interval timer of <c>$A9:B03E</c> and
    /// <c>$A9:B346</c>, aliased with the body sub-function pointer.
    /// </summary>
    public WrappingCountdown DeathExplosionIntervalTimer { get; internal set; }

    /// <summary>
    /// Native word <c>$0FF2</c>, one word under three names: the ascent dust's body
    /// sub-function timer, the seven-record death-explosion index (<c>$A9:B046</c>) and the
    /// four-record escape-door dust index (<c>$A9:B355</c>). Each effect inherits the value
    /// the previous one left unless native code explicitly clears it.
    /// </summary>
    public WrappingCountdown DeathAndEscapeExplosionIndex { get; internal set; }

    /// <summary>OBJ palette selection forced to palette seven when the dying brain effects shut down.</summary>
    public SnesObjAttributeWord BrainPalette { get; internal set; }

    /// <summary>Number of calls made to the body flicker producer at `$A9:AFB6`.</summary>
    public uint BodyFlickerCallCount { get; private set; }

    /// <summary>
    /// Host witness for the `$02C6..0` BG2 tilemap clear and following NMI transfer request.
    /// Rendering code can consume this without pretending the global WRAM tilemap is local.
    /// </summary>
    public bool EnemyBg2TilemapClearRequested { get; private set; }

    /// <summary>How many of the six corpse pages have shown (<c>$A9:9003</c>), which is also the next to show.</summary>
    public int CorpsePagesShown { get; private set; }

    /// <summary>
    /// How many pages of the escape sequence's list have shown (<c>$A6:C4CB</c>): the two escape-timer digit pages, then
    /// the five escape-message pages.
    /// </summary>
    public int EscapePagesShown { get; private set; }

    /// <summary>How many of the two exploded-door pages have shown (<c>$A9:902F</c>).</summary>
    public int ExplodedDoorPagesShown { get; private set; }

    /// <summary>The pages the escape sequence shows one per call: the escape timer's digits, then the escape message.</summary>
    public static int EscapePageCount =>
        MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.EscapeTimer) + MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.EscapeText);

    /// <summary>Unpause-hook enable word cleared when the escape typewriter is installed.</summary>
    public bool MotherBrainUnpauseHookEnabled { get; internal set; } = true;

    /// <summary>
    /// Set when `$A9:B11B` installs the brain-slot draw setup before the decapitated head
    /// starts falling. The separate brain AI is not silently folded into the body function.
    /// </summary>
    public bool BrainDrawSetupRequested { get; private set; }

    /// <summary>
    /// Executes the corpse-table and graphics-buffer half of head initialization
    /// <c>$A9:8705-$870B</c>. A real encounter calls this once when the brain enemy spawns.
    /// </summary>
    public void InitializeCorpseRotting() => CorpseRotting.Initialize(CorpseStaging);

    /// <summary>Selects the next corpse page, rejecting calls after the last has shown.</summary>
    private MotherBrainSpritePage NextCorpsePage()
    {
        int index = CorpsePagesShown;
        if (index >= MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Corpse))
            throw new InvalidOperationException("Every Mother Brain corpse page has already shown.");

        CorpsePagesShown++;
        return MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Corpse, index);
    }

    /// <summary>Selects the next page of the escape sequence: a timer digit page, then a message page.</summary>
    internal MotherBrainSpritePage NextEscapePage()
    {
        int index = EscapePagesShown;
        if (index >= EscapePageCount)
            throw new InvalidOperationException("Every escape sequence page has already shown.");

        EscapePagesShown++;
        int timerPages = MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.EscapeTimer);
        return index < timerPages
            ? MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.EscapeTimer, index)
            : MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.EscapeText, index - timerPages);
    }

    /// <summary>Selects the next exploded-door page, rejecting calls after the last has shown.</summary>
    internal MotherBrainSpritePage NextExplodedDoorPage()
    {
        int index = ExplodedDoorPagesShown;
        if (index >= MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.ExplodedDoor))
            throw new InvalidOperationException("Every exploded-door page has already shown.");

        ExplodedDoorPagesShown++;
        return MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.ExplodedDoor, index);
    }

    /// <summary>
    /// Emits the death-phase explosions at the native timer cadence, consuming one global
    /// random value per projectile so later enemy AI sees the same RNG sequence.
    /// </summary>
    internal void GenerateDeathExplosions(
        MotherBrainDeathExplosionStyle style,
        Func<int>? nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // `$B03E` decrements before testing BPL. A cleared timer therefore wraps and emits
        // immediately. Crucially, the interval literal remains in A across the memory DEC,
        // so the smoky/mixed caller writes exactly `$10`/`$08` after an expiry.
        DeathExplosionIntervalTimer = DeathExplosionIntervalTimer.Decrement().Remaining;
        if (!DeathExplosionIntervalTimer.IsNegative)
            return;

        DeathExplosionIntervalTimer = WrappingCountdown.Start(style == MotherBrainDeathExplosionStyle.Mixed ? 8 : 16);
        DeathAndEscapeExplosionIndex = DeathAndEscapeExplosionIndex.Decrement().Remaining;
        if (DeathAndEscapeExplosionIndex.IsNegative)
            DeathAndEscapeExplosionIndex = WrappingCountdown.Start(MotherBrainDeathExplosionDefinitions.GroupCount - 1);

        int simultaneousCount = style == MotherBrainDeathExplosionStyle.Mixed ? MotherBrainDeathExplosionDefinitions.AnchorsPerGroup : 2;
        int pairIndex = DeathAndEscapeExplosionIndex.RawValue * MotherBrainDeathExplosionDefinitions.AnchorsPerGroup;
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
                ? random < 16384 ? 0 : random < 57344 ? 1 : 2
                : 1;
            (short xOffset, short yOffset) = MotherBrainDeathExplosionDefinitions.Anchor(pairIndex + explosionIndex);
            requests.Add(new MotherBrainDeathExplosionRequest(
                Offset: new Offset16<RoomPixels>(xOffset, yOffset),
                ProjectileParameter: parameter,
                SoundEffect: MotherBrainSoundDefinitions.Explosion));
        }
    }

    /// <summary>
    /// Emits the next four-frame escape-door blast from its repeating native position table.
    /// Returns no request on frames before the shared interval timer expires.
    /// </summary>
    private DustExplosionRequest? GenerateEscapeDoorExplosion(
        Func<int>? nextRandomNumber)
    {
        // `$B346` shares the old death-explosion interval word but replaces its cadence with
        // four. A cleared word underflows on the first call, so a new explosion is immediate.
        DeathExplosionIntervalTimer = DeathExplosionIntervalTimer.Decrement().Remaining;
        if (!DeathExplosionIntervalTimer.IsNegative)
            return null;

        DeathExplosionIntervalTimer = WrappingCountdown.Start(4);
        DeathAndEscapeExplosionIndex = DeathAndEscapeExplosionIndex.Decrement().Remaining;
        if (DeathAndEscapeExplosionIndex.IsNegative)
            DeathAndEscapeExplosionIndex = WrappingCountdown.Start(3);

        // The table is stored as interleaved X/Y words and indexed by `index * 4` bytes.
        // Spell out the semantic pairs so neither host endianness nor array stride can alter
        // the native 3,2,1,0 repeating order.
        Position16<RoomPixels> position = DeathAndEscapeExplosionIndex.RawValue switch
        {
            0 => new(8, 108),
            1 => new(24, 128),
            2 => new(9, 144),
            3 => new(24, 116),
            _ => throw new InvalidOperationException(
                $"Escape-door explosion index ${DeathAndEscapeExplosionIndex.RawValue:X4} escaped its native 0..3 range."),
        };

        if (nextRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Escape-door explosions require the global next-random-number producer.");
        }

        // Native `$B377` advances the one shared RNG exactly once per emitted projectile.
        // Values below a quarter of the range select the smoke animation ($0C); all remaining
        // values retain the preloaded small explosion ($03).
        int random = nextRandomNumber();
        MiscDustAnimation animation = random < 16384 ? MiscDustAnimation.Smoke : MiscDustAnimation.SmallExplosion;
        return new DustExplosionRequest(
            Position: position,
            Animation: animation,
            SoundEffect: Library2Sound.SmallExplosion);
    }

    /// <summary>
    /// Runs the third phase death move to back of room body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathMoveToBackOfRoom(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$A9:AEE1` makes both enemy slots non-interactive and disables the shared
        // hitboxes on every call, then reuses the ordinary medium-speed backward
        // walk producer. Carry means X `$28` has been reached/overshot; the new `$80`
        // timer falls through so the first smoky explosion and decrement happen now.
        BodyProperties = BodyProperties.With(EnemyProperties.IgnoreSamusCollision);
        BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
        HitboxesEnabled = false;
        MotherBrainWalkResult dyingRetreat = _state.RequestWalkBackward(40, animationDelay: 6);
        requests.BodyWalkRequested = dyingRetreat.Requested;
        if (dyingRetreat.ReachedTarget)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding;
            _state.FunctionTimer = WrappingCountdown.Start(128);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death idle whilst exploding body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathIdleWhilstExploding(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom;
        return false;
    }

    /// <summary>
    /// Runs the third phase death stumble to middle of room body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathStumbleToMiddleOfRoom(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // `$AF21` emits smoke before asking for a really-fast forward walk to `$60`.
        // Equality still starts a complete body program; only signed overshoot or the
        // independent `$80` arena clamp reports carry and advances the death script.
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        MotherBrainWalkResult dyingAdvance = _state.RequestWalkForward(96, animationDelay: 2);
        requests.BodyWalkRequested = dyingAdvance.Requested;
        if (dyingAdvance.ReachedTarget)
        {
            _state.SetHeadProgram(MotherBrainHeadProgram.DyingDrool);
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
            _state.NeckAngleDelta = 1280;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects;
            _state.FunctionTimer = WrappingCountdown.Start(32);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death disable brain effects body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDisableBrainEffects(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, requests.DeathExplosions);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            // `$AF5C-$AF94` removes the neck/breath/palette producers, copies sprite
            // palette 1 into palette 7, performs one health-palette refresh, and then
            // deliberately falls through with the already-negative timer.
            _state.LowerNeckMovementIndex = MotherBrainNeckMovement.None;
            _state.UpperNeckMovementIndex = MotherBrainNeckMovement.None;
            _state.DroolGenerationEnabled = false;
            _state.DisableSmallPurpleBreathGeneration();
            _state.BrainPaletteHandlingEnabled = false;
            _state.HealthBasedPaletteHandlingEnabled = false;
            BrainPalette = SnesObjPalettes.Palette(7);
            requests.PaletteRequested = true;
            // `$AF91/$AF94` clear words $0FF0 and $0FF2 before the fade-out explosions.
            DeathExplosionIntervalTimer = WrappingCountdown.Start(0);
            DeathAndEscapeExplosionIndex = WrappingCountdown.Start(0);
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup body fade out body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupBodyFadeOut(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody;
            _state.FunctionTimer = WrappingCountdown.Start(0);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade out body body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeOutBody(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // Body flickering runs every AI call, independently of the seventeen-call
        // palette cadence. Counter values 0..15 copy blackening palettes; value 16
        // is the null terminator and completes the transition without a copy.
        BodyFlickerCallCount++;
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.FunctionTimer = WrappingCountdown.Start(16);
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.PaletteRequested |= paletteIndex < 16;
            if (paletteIndex >= 16)
            {
                EnemyBg2TilemapClearRequested = true;
                BodyProperties = BodyProperties
                    .With(EnemyProperties.Invisible)
                    .Without(EnemyProperties.ProcessInstructions);
                BodyProperties2 = default;
                _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions;
                _state.FunctionTimer = WrappingCountdown.Start(16);
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death final few explosions body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFinalFewExplosions(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        GenerateDeathExplosions(
            style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, requests.DeathExplosions);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation;
        return false;
    }

    /// <summary>
    /// Runs the third phase death realize decapitation body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathRealizeDecapitation()
    {
        // The body installs the decapitated list and a separate brain-slot draw
        // setup, clears its 8.8 falling velocity, then falls through immediately.
        _state.SetHeadProgram(MotherBrainHeadProgram.Decapitated);
        BrainDrawSetupRequested = true;
        _state.FunctionTimer = WrappingCountdown.Start(0);
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround;
        return true;
    }

    /// <summary>
    /// Runs the third phase death brain falls to ground body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBrainFallsToGround()
    {
        // `$B12D` treats FunctionTimer as unsigned 8.8 velocity: add `$20`, use only
        // its high byte as this frame's whole-pixel displacement, and accumulate Y.
        // Crossing `$C4` clamps the head and publishes earthquake type two for 20.
        _state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(_state.FunctionTimer.RawValue + 32)));
        ushort fallingDisplacement = SnesLittleEndian.HighByte(_state.FunctionTimer.RawValue);
        ushort candidateBrainY = unchecked((ushort)(_state.BrainPosition.Y + fallingDisplacement));
        if (candidateBrainY >= 196)
        {
            _state.EarthquakeType = 2;
            _state.EarthquakeTimer = WrappingCountdown.Start(20);
            _state.BrainPosition = _state.BrainPosition.WithY(196);
            CorpsePagesShown = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles;
            _state.FunctionTimer = WrappingCountdown.Start(256);
        }
        else
        {
            _state.BrainPosition = _state.BrainPosition.WithY(candidateBrainY);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load corpse tiles body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathLoadCorpseTiles(MotherBrainRainbowBeamStepRequests requests)
    {
        // ProcessSpriteTilesTransfers handles one record per call and notices the
        // following zero terminator on the sixth call, just like the Baby tile list.
        requests.SpritePage = NextCorpsePage();
        if (CorpsePagesShown == MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Corpse))
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey;
            _state.FunctionTimer = WrappingCountdown.Start(32);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death setup fade to grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSetupFadeToGrey()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.GreyTransitionCounter = 0;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey;
            _state.FunctionTimer = WrappingCountdown.Start(0);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death fade to grey body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathFadeToGrey(MotherBrainRainbowBeamStepRequests requests)
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            int paletteIndex = _state.GreyTransitionCounter;
            _state.GreyTransitionCounter++;
            requests.PaletteRequested = paletteIndex < 8;
            if (paletteIndex < 8)
            {
                _state.FunctionTimer = WrappingCountdown.Start(16);
            }
            else
            {
                _state.SetHeadProgram(MotherBrainHeadProgram.Corpse);
                _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver;
                _state.FunctionTimer = WrappingCountdown.Start(256);
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse tips over body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathCorpseTipsOver()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway;
            BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
            HitboxesEnabled = false;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death corpse rots away body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathCorpseRotsAway(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // Retail initialization happens when the brain enemy slot is created, many
        // phases before death. Keep an explicit public initializer for that normal
        // route, but lazily perform the same work for focused debugger fixtures that
        // start at a late phase. Nothing touches this private table/buffer in between,
        // so the delayed call is state-equivalent while preventing a fake host setup.
        if (!CorpseRotting.IsInitialized)
            InitializeCorpseRotting();

        MotherBrainCorpseRottingStepResult corpseRotting = CorpseRotting.Step(
            CorpseStaging,
            _state.BrainPosition,
            frame.RandomNumberSeed,
            frame.MainEnemyExecutionCounter);
        requests.CorpseRotStepShown = corpseRotting.ShownRotStep ?? requests.CorpseRotStepShown;
        requests.CorpseDustRequests.AddRange(corpseRotting.DustRequests);
        if (corpseRotting.StillRotting)
            return false;

        // Carry clear at `$B1DB` makes the brain invisible/non-solid, clears its
        // second property word, and queues stop + escape music through the ordinary
        // eight-frame-delay music queue. `$14` is loaded and immediately decremented
        // by the fallthrough into `$B211`, leaving observable timer `$13` now.
        BrainProperties = BrainProperties
            .With(EnemyProperties.Invisible)
            .Without(EnemyProperties.ProcessInstructions);
        BrainProperties2 = default;
        requests.MusicStopQueued = true;   // Queue music value `$0000`.
        requests.EscapeMusicQueued = true; // Queue music value `$FF24`.
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay;
        _state.FunctionTimer = WrappingCountdown.Start(20);
        return true;
    }

    /// <summary>
    /// Runs the third phase death20 frame delay body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeath20FrameDelay()
    {
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            // The decapitated brain remains at the corpse coordinates for the full
            // delay. `$B216/$B219` finally park it at (0,0) before escape-timer tile
            // loading begins; this is a real actor-coordinate mutation, not rendering.
            _state.BrainPosition = Position16<RoomPixels>.Zero;
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death load escape timer tiles body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathLoadEscapeTimerTiles(MotherBrainRainbowBeamStepRequests requests)
    {
        requests.EscapeSequencePages.Add(NextEscapePage());
        if (EscapePagesShown == EscapePageCount)
        {
            // Carry set after entry six clears the shared list cursor, installs
            // `$B26D`, and falls through far enough to emit exploded-door entry zero
            // during this same enemy AI call.
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathStartEscape;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death start escape body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathStartEscape(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        requests.EscapeSequencePages.Add(NextExplodedDoorPage());
        if (ExplodedDoorPagesShown != MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.ExplodedDoor))
            return false;

        // The second door record observes the zero terminator and performs the full
        // `$B275-$B2CD` handoff on that call: copy fourteen colors (palette zero is
        // skipped), start escape music, hold the quake indefinitely, create four
        // palette-FX objects, disable the old unpause hook, and initialize text.
        requests.ExplodedDoorPaletteRequested = true; // `$A9:9534`, 14 colors -> target `$0122`.
        requests.EscapeMusicTrackQueued = true;        // Eight-frame-delay music value `$0007`.
        _state.EarthquakeType = 5;
        _state.EarthquakeTimer = WrappingCountdown.Start(ushort.MaxValue);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianShutterRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianBackgroundRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianGeneralLevelRedFlashing);
        requests.EscapePaletteFxRequests.Add(PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs);
        MotherBrainUnpauseHookEnabled = false;
        requests.EscapeTypewriterSetupRequested = true;
        _state.FunctionTimer = WrappingCountdown.Start(32);
        _state.Phase = frame.AlternateEscapeText
            ? MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle
            : MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText;
        return false;
    }

    /// <summary>
    /// Runs the third phase death spawn time bomb set subtitle body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathSpawnTimeBombSetSubtitle(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$B2D1` decrements first. BPL does *not* merely wait: it branches directly
        // into `$B2E3`, so the underlying typewriter advances during all 32..0
        // subtitle-delay values. On underflow the native code installs `$B2E3`,
        // spawns the persistent Japanese subtitle projectile, and falls through to
        // that same typewriter call. Keep the producer request separate because the
        // bank-$86 allocator owns whether a physical slot is actually available.
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (_state.FunctionTimer.IsNegative)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText;
            requests.TimeBombSetSubtitleSpawnRequested = true;
        }
        return true;
    }

    /// <summary>
    /// Runs the third phase death type out zebes escape text body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathTypeOutZebesEscapeText(RainbowBeamAttackFrame frame)
    {
        // `$B2E3` calls the independently owned typewriter with list pointer `$2610`.
        // A request plus explicit completion input preserves that scheduler boundary:
        // the boss never invents character cadence, yet its carry-dependent handoff
        // remains directly testable and debuggable.
        if (frame.TypewriterFinished)
        {
            _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer;
            _state.FunctionTimer = WrappingCountdown.Start(32);
        }
        return false;
    }

    /// <summary>
    /// Runs the third phase death door exploding start timer body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathDoorExplodingStartTimer(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // `$B2F9` always runs the periodic dust producer before touching the escape
        // countdown. The freshly loaded `$20` therefore receives 33 calls (32..0,
        // then `$FFFF`) and can emit dust even on the transition call.
        requests.EscapeDoorExplosion = GenerateEscapeDoorExplosion(frame.NextRandomNumber);
        _state.FunctionTimer = _state.FunctionTimer.Decrement().Remaining;
        if (!_state.FunctionTimer.IsNegative)
            return false;

        // Samus command `$0F`, TimerStatus `$0002`, boss bit `$02`, and event `$0E`
        // belong to four different native subsystems. Publish every edge instead of
        // silently writing the wrong debug room's area flags inside this actor.
        requests.TimerHandlingEnableRequested = true;
        requests.MotherBrainEscapeTimerStartRequested = true;
        requests.MotherBrainBossBitRequested = true;
        requests.ZebesTimebombEventRequested = true;
        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor;
        DeathExplosionIntervalTimer = WrappingCountdown.Start(0);
        DeathAndEscapeExplosionIndex = WrappingCountdown.Start(0);
        return false;
    }

    /// <summary>
    /// Runs the third phase death blow up escape door body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathBlowUpEscapeDoor(MotherBrainRainbowBeamStepRequests requests)
    {
        // `$B3A3` invokes the ordinary highest-free-slot allocator eight times with
        // parameters zero through seven. Allocation success is intentionally decided
        // by the shared enemy-projectile system, not assumed by the boss actor.
        for (int parameter = 0; parameter < 8; parameter++)
            requests.EscapeDoorParticleSpawns.Add(new(parameter));

        _state.Phase = MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing;
        requests.EscapeDoorPlm = new(
            Block: new(0, 6),
            PlmEntry: PlmHeaderId.MotherBrainsRoomEscapeDoor);
        return false;
    }

    /// <summary>
    /// Runs the third phase death keep earthquake going body function. True when it installs the next body function and the cartridge runs
    /// on into it within the same call.
    /// </summary>
    internal bool RunThirdPhaseDeathKeepEarthquakeGoing(RainbowBeamAttackFrame frame, MotherBrainRainbowBeamStepRequests requests)
    {
        // EarthquakeTimer is global WRAM and is normally decremented outside enemy
        // AI. A supplied sample is therefore authoritative. `$B33C` changes only a
        // visible zero to `$FFFF`; every nonzero value, including `$FFFF`, survives.
        if (frame.GlobalEarthquakeTimer is WrappingCountdown observedEarthquakeTimer)
            _state.EarthquakeTimer = observedEarthquakeTimer;
        if (_state.EarthquakeTimer.IsZero)
        {
            _state.EarthquakeTimer = WrappingCountdown.Start(ushort.MaxValue);
            requests.EarthquakeTimerRefreshed = true;
        }
        return false;
    }
}
