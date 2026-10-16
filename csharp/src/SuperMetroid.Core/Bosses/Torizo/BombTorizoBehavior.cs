using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BombTorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BombTorizo, EnemyDefinitionId.GoldenTorizo];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BombTorizoEntity(this, slot, common);

    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves body movement against room blocks and slopes.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Owns the eighteen-slot enemy projectile pool used by Torizo attacks.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Receives the impact quake when a falling or jumping statue lands.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Exposes the two live CGRAM rows occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Provides the NMI counter used by Torizo instruction choices.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Applies shot, bomb, and touch damage through the shared collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects the Bomb and Golden Torizo callbacks to their room-scoped services.</summary>
    internal BombTorizoBehavior(BombTorizoBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>
    /// Dispatches an already-colliding family-<c>$0500</c> bomb through Torizo hitbox
    /// callbacks <c>$AA:C97C/$C9C2/$D667</c>. The bank-$A0 multibox walker has already set
    /// direction bit <c>$10</c>; this method owns only the species callback and its common
    /// no-death damage tail.
    /// </summary>
    private void ResolveTorizoNormalBomb(
        RoomEnemySlot torizo,
        SamusBombProjectileSlot bomb,
        ushort selectedShotAiPointer)
    {
        EnemyInteractionCallback? selectedShotAi = EnemyCollision.HitboxCallback(torizo, selectedShotAiPointer);
        bool isBombTorizo = torizo.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = torizo.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo;
        if (!isBombTorizo && !isGoldenTorizo)
            throw new ArgumentException("Normal-bomb Torizo dispatch requires a Torizo body.");

        TorizoEnemyState state = RequireBombTorizoState(torizo);
        if (selectedShotAi == EnemyInteractionCallback.TorizoStandUpSitDownShot)
        {
            // `$C9C2` is a complete no-op in Bomb Torizo's area. Golden Torizo jumps to
            // `$D658`, which accepts common damage only outside the active flash and while
            // `toriz_var_04` is zero. This transitional callback never arms the later
            // counterattack bits or captures a projectile family.
            if (isBombTorizo || !torizo.FlashTimer.IsZero || state.ShotGuard != 0)
                return;

            _collision.ApplyCommonNormalBombDamage(torizo, bomb, runGenericDeath: false);
            if (torizo.Health == 0)
                BeginBombTorizoDeath(torizo, state);
            return;
        }

        bool validMainCallback = isBombTorizo
            ? selectedShotAi == EnemyInteractionCallback.BombTorizoShot
            : selectedShotAi is EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.GoldenTorizoShot;
        if (!validMainCallback)
        {
            throw new InvalidDataException(
                $"Torizo normal-bomb hitbox AI $AA:{selectedShotAiPointer:X4} is not translated.");
        }

        // Both normal callbacks return during the hurt flash. Bomb Torizo also uses its
        // awakening guard as a hard rejection; Golden's nonzero guard deliberately falls
        // through to common damage instead.
        if (!torizo.FlashTimer.IsZero || isBombTorizo && state.ShotGuard != 0)
            return;

        if (isGoldenTorizo && state.ShotGuard == 0)
        {
            // Family `$0500` is neither Missile nor Super Missile. `$D667` records it only
            // when the Super-catch latch is clear, then takes LABEL_11 and arms bit `$2000`
            // before common damage. A set `$1000` latch skips the family write but still
            // reaches that same counterattack bit.
            if (!HasBehavior(torizo, TorizoBehaviorFlags.CaughtSuperMissile))
                state.CapturedProjectileFamily = SamusBombProjectileSystem.NormalBombType;
            SetBehavior(torizo, TorizoBehaviorFlags.Stunned);
        }

        _collision.ApplyCommonNormalBombDamage(torizo, bomb, runGenericDeath: false);
        if (torizo.Health == 0)
            BeginBombTorizoDeath(torizo, state);
    }
    /// <summary>Health below which the head-break list interrupts active Bomb Torizo AI.</summary>
    private const ushort BombTorizoHeadExplosionHealth = 350;
    /// <summary>Health below which the later core-break list interrupts active AI.</summary>
    private const ushort BombTorizoCoreExplosionHealth = 100;

    /// <summary>Current statue state, cleared when the room reloads.</summary>
    internal TorizoEnemyState? _torizoState;
    /// <summary>Drop requests published by the Chozo-orb projectile instruction.</summary>
    internal readonly List<TorizoOrbDropRequest> _torizoOrbDropRequests = new();

    /// <summary>Golden Torizo's state while definition $EF7F owns slot zero.</summary>
    public TorizoEnemyState? GoldenTorizo =>
        _torizoState is { IsGolden: true } ? _torizoState : null;

    /// <summary>Last delayed music request emitted by awakening/death bytecode.</summary>
    public BombTorizoMusicRequest? LastBombTorizoMusicRequest { get; private set; }

    /// <summary>Discards the previous statue, orb requests, and frame-level sound/music publications on room load.</summary>
    internal void ResetBombTorizoRoomState()
    {
        _torizoState = null;
        _torizoOrbDropRequests.Clear();
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>
    /// Ports shared <c>Torizo_Init</c> at $AA:C87F for the area-zero Bomb and area-two
    /// Golden definitions. The cartridge indexes parallel two-word tables by area; using
    /// the concrete definition here is equivalent and prevents an unrelated host area enum
    /// from becoming a second authority for actor identity.
    /// </summary>
    internal void InitializeBombTorizo(RoomEnemySlot torizo, SamusState? samus, SnesButton controllerInput)
    {
        bool isGolden = torizo.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo;
        var state = new TorizoEnemyState(torizo, isGolden);
        _torizoState = state;

        // Boss bit four is shared by the Bomb and Golden Torizo encounters, but it is stored
        // per area. The room loader supplies the current area's accessor, keeping the actor
        // independent from save-memory layout and avoiding a room-number special case.
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaTorizo))
        {
            torizo.Properties = torizo.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            return;
        }

        TorizoInitializationDefinition definition = isGolden
            ? TorizoInitializationDefinitions.Golden : TorizoInitializationDefinitions.Bomb;
        torizo.Position = torizo.Position.WithWhole(definition.Position);
        state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(definition.Program);
        torizo.Properties = unchecked((ushort)(torizo.Properties | definition.PropertyMask));
        torizo.ExtraProperties = torizo.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        torizo.Radius = definition.Radius;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        torizo.Timer = NativeCountdown.Start(0);
        torizo.PaletteIndex = 0;
        torizo.SpritemapPointer = TorizoFrameDefinitions.InitialSpritemap;

        state.Function = TorizoFunction.Falling;
        state.PreInstruction = TorizoPreInstruction.Idle;
        state.Velocity = new EightEightVector<RoomPixels>(0, 256);

        // Init writes two shared target rows and then selects the encounter-specific body
        // pair. This runtime exposes live CGRAM rather than separate current/target buffers,
        // so install those final visible rows directly at their native palette indexes.
        LoadTorizoSharedPaletteRows();
        if (isGolden)
        {
            LoadGoldenTorizoBasePalette();
            if (controllerInput == GoldenTorizoCodeDefinitions.ControllerChord)
            {
                if (samus is null)
                    throw new InvalidOperationException("The Golden Torizo code requires the live Samus owner.");
                // Native initialization overwrites both inventory words, even if
                // this reduces capacity or equips normally incompatible beams.
                samus.Health = samus.MaxHealth = GoldenTorizoCodeDefinitions.Energy;
                samus.ReserveEnergy = samus.MaxReserveEnergy = GoldenTorizoCodeDefinitions.ReserveEnergy;
                samus.Missiles = samus.MaxMissiles = GoldenTorizoCodeDefinitions.Missiles;
                samus.SuperMissiles = samus.MaxSuperMissiles = GoldenTorizoCodeDefinitions.SuperMissiles;
                samus.PowerBombs = samus.MaxPowerBombs = GoldenTorizoCodeDefinitions.PowerBombs;
                samus.EquippedItems = samus.CollectedItems = GoldenTorizoCodeDefinitions.Items;
                samus.EquippedBeams = samus.CollectedBeams = GoldenTorizoCodeDefinitions.Beams;
            }
        }
        else
            LoadBombTorizoPalette();
    }

    /// <summary>Writes the two palette rows shared by both statue variants to live CGRAM.</summary>
    private void LoadTorizoSharedPaletteRows()
    {
        ReadOnlySpan<Bgr555> rows = TorizoPaletteDefinitions.SharedRows;
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossPalettes.TorizoSharedFirst.Write(color, rows[color]);
            _presentation.BossPalettes.TorizoSharedSecond.Write(color, rows[16 + color]);
        }
    }

    /// <summary>Installs Bomb Torizo's initial body and belly colors in palette rows nine and ten.</summary>
    private void LoadBombTorizoPalette() =>
        LoadTorizoBodyPalette(TorizoPaletteDefinitions.BombInitial[..16], TorizoPaletteDefinitions.BombInitial[16..]);

    /// <summary>
    /// Installs Golden Torizo's initial body colors before hurt frames begin using the
    /// health-indexed bank-$84 gradient.
    /// </summary>
    private void LoadGoldenTorizoBasePalette() =>
        LoadTorizoBodyPalette(TorizoPaletteDefinitions.GoldenInitial[..16], TorizoPaletteDefinitions.GoldenInitial[16..]);

    /// <summary>Restores the normal body rows on alternating Bomb Torizo hurt frames.</summary>
    private void LoadTorizoDeathPalette() =>
        LoadTorizoBodyPalette(TorizoPaletteDefinitions.Normal[..16], TorizoPaletteDefinitions.Normal[16..]);

    /// <summary>Creates the per-statue gradual color transition against the live CGRAM image on first use.</summary>
    private CartridgePaletteTransition GetTorizoPaletteTransition(TorizoEnemyState state) =>
        state.PaletteTransition ??= new CartridgePaletteTransition(
            _presentation.CgramRows.Image, TorizoPaletteDefinitions.FadeDenominator, _presentation.GradualColorChange);

    /// <summary>Sets target colors beginning at the first body entry for the authored palette fade.</summary>
    private void SetTorizoPaletteTarget(TorizoEnemyState state, ReadOnlySpan<Bgr555> colors)
    {
        var transition = GetTorizoPaletteTransition(state);
        for (int color = 0; color < colors.Length; color++)
            transition.SetTargetColor(TorizoPaletteDefinitions.FirstBodyColor + color, colors[color]);
    }
    /// <summary>Selects Golden Torizo's body and belly gradient row from health bits 11 through 14.</summary>
    private void LoadGoldenTorizoHealthPalette(ushort health)
    {
        // $84:8000 selects one of eight 16-color rows using bits 11..14 of health, clamping
        // the upper half to row seven. Deriving the row start algebraically preserves the
        // original descending copy while keeping the loop natural for debugger inspection.
        int selector = (SnesLittleEndian.HighByte(health)) & TorizoCombatDefinitions.HealthPaletteRowBits;
        if ((selector & TorizoCombatDefinitions.HealthPaletteUpperHalf) != 0)
            selector = 56;
        int sourceWord = selector * 2;
        var colors = _room.Artwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Golden Torizo health palettes require installed artwork.");
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossPalettes.TorizoBody.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Enemies.Artwork.EnemyAuxiliaryPalette.GoldenTorizoBody,
                    sourceWord / 16, color));
            _presentation.BossPalettes.TorizoBelly.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Enemies.Artwork.EnemyAuxiliaryPalette.GoldenTorizoBelly,
                    sourceWord / 16, color));
        }
    }

    /// <summary>Copies the supplied body and belly rows into their live boss-palette slots.</summary>
    private void LoadTorizoBodyPalette(
        ReadOnlySpan<Bgr555> rowNine,
        ReadOnlySpan<Bgr555> rowTen)
    {
        for (int color = 0; color < 16; color++)
        {
            _presentation.BossPalettes.TorizoBody.Write(color, rowNine[color]);
            _presentation.BossPalettes.TorizoBelly.Write(color, rowTen[color]);
        }
    }

    /// <summary>Ports <c>Torizo_Main</c> and its <c>toriz_var_E</c> dispatcher.</summary>
    private void RunBombTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case TorizoFunction.Idle:
                return;

            case TorizoFunction.Falling:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                ApplyBombTorizoGravity(torizo, state, RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.WaitForHandTrigger:
                // $AA:C6C6 scans all forty PLM headers for $D6EA. A supplied PLM owner is
                // authoritative; standalone enemy audits omit it and therefore model the
                // already-collected item/removed hand trigger.
                torizo.Properties = torizo.Properties.With(EnemyProperties.SolidToSamus);
                if (_room.Services.Plms.HasActiveHeader(PlmHeaderId.BombTorizoHand))
                    return;

                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(6),
                    MusicCommandDelay.EightFrames);
                torizo.Properties = torizo.Properties.Without(EnemyProperties.SolidToSamus);
                state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoAnimations.AfterSleep(state.BodyProgram.Program));
                torizo.InstructionTimer = NativeCountdown.Start(1);
                state.AwakeningReleased = true;
                return;

            case TorizoFunction.Active:
                RunBombTorizoActiveState(torizo, state, level);
                return;

            case TorizoFunction.GoldenWaitForSamus:
                // $AA:D5C2 advances the list operand only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus is not null && samus.Position.Y.Whole > 320 && samus.Position.X.Whole > 368)
                {
                    state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoAnimations.AfterSleep(state.BodyProgram.Program));
                    torizo.InstructionTimer = NativeCountdown.Start(1);
                }
                return;

            case TorizoFunction.GoldenGravity:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                ApplyBombTorizoGravity(torizo, state, RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.GoldenPreInstruction:
                MaybeSpawnBombTorizoLowHealthDrool(torizo);
                RunBombTorizoPreInstruction(torizo, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Torizo main function $AA:{(int)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Counts consecutive Samus space-jump poses before running the shared statue dispatcher.</summary>
    private void RunGoldenTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // GoldTorizo_Main counts consecutive space-jump poses for its anti-stall leap
        // decision, then calls the same toriz_var_E dispatcher as Bomb Torizo.
        if (samus?.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose)
            state.SamusSpaceJumpFrames = unchecked((ushort)(state.SamusSpaceJumpFrames + 1));
        else
            state.SamusSpaceJumpFrames = 0;
        RunBombTorizoMain(torizo, state, samus, level);
    }

    /// <summary>Interrupts active AI at the head and core health thresholds before normal pre-instructions resume.</summary>
    private void RunBombTorizoActiveState(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        MaybeSpawnBombTorizoLowHealthDrool(torizo);

        // The two high parameter-two bits are death-sequence latches. Before they are set,
        // health thresholds interrupt the current authored list with the core/head bursts.
        if (!HasBehavior(torizo, TorizoBehaviorFlags.GutBlownUp) &&
            torizo.Health < BombTorizoHeadExplosionHealth)
        {
            state.InterruptedCursor = state.BodyProgram;
            state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.SpecialCallableBlowUpBombTorizosGut);
            torizo.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        if (!HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp) &&
            torizo.Health < BombTorizoCoreExplosionHealth)
        {
            state.ReturnCursor = AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                ? TorizoBodyProgram.FacingLeftFacelessTurningLeft
                : TorizoBodyProgram.FacingRightFacelessTurningRight);
            state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.CallableBlowUpBombTorizosFace);
            torizo.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        RunBombTorizoPreInstruction(torizo, state, level);
    }

    /// <summary>Dispatches the movement callback installed by the current Bomb or Golden instruction list.</summary>
    private void RunBombTorizoPreInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        switch (state.PreInstruction)
        {
            case TorizoPreInstruction.Idle:
                return;
            case TorizoPreInstruction.AirTransition:
                RunBombTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstruction.Gravity:
                ApplyBombTorizoGravity(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstruction.Jump:
                RunBombTorizoJump(torizo, state, RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstruction.GoldenAirTransition:
                RunGoldenTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoPreInstruction.GoldenGravity:
                ApplyBombTorizoGravity(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            default:
                throw new InvalidDataException(
                    $"Bomb Torizo pre-instruction $AA:{(int)state.PreInstruction:X4} is not translated.");
        }
    }

    /// <summary>Moves the statue vertically and starts a landing quake when downward motion hits terrain.</summary>
    private void ApplyBombTorizoGravity(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        int displacement = SnesSixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue);
        if (_terrain.MoveEnemyVertically(level, torizo, displacement))
        {
            short velocity = state.Velocity.Y.SignedRawValue;
            if (velocity is >= 0 and not 256)
            {
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(32);
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(256));
            }
            return;
        }

        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + 40))));
    }

    /// <summary>Advances a jump across slopes; landing restores the saved list and triggers the impact quake.</summary>
    private void RunBombTorizoJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            torizo,
            SnesSixteenSixteen.FromEightEight(state.Velocity.X.SignedRawValue));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);

        if (_terrain.MoveEnemyVertically(
                level,
                torizo,
                SnesSixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue)))
        {
            state.BodyProgram = state.ReturnCursor;
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(256));
            _earthquake.EarthquakeType = 4;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(32);
            return;
        }

        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            state.Velocity.Y.RawValue + state.VerticalAcceleration))));
    }

    /// <summary>Waits for Bomb Torizo's landing timer or selects the airborne list while a downward probe is clear.</summary>
    private void RunBombTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        bool alternateLanding = HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
        if (!state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = state.AirTransitionTimer.Decrement().Remaining;
            if (state.AirTransitionTimer.IsZero)
            {
                state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(alternateLanding
                    ? (FacesRight(torizo)
                        ? TorizoBodyProgram.FacingLeftFacelessTurningLeft
                        : TorizoBodyProgram.FacingRightFacelessTurningRight)
                    : (FacesRight(torizo)
                        ? TorizoBodyProgram.FacingLeftTurningLeft
                        : TorizoBodyProgram.FacingRightTurningRight));
                torizo.InstructionTimer = NativeCountdown.Start(1);
                return;
            }
        }

        int fallingSpeed = Math.Min(Math.Abs(state.Velocity.X.SignedRawValue) + 1, 15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SnesSixteenSixteen.FromWhole(fallingSpeed)))
        {
            // $AA:C752 swaps to the authored in-air list while the downward probe remains
            // clear. Landing itself is handled by the list-installed gravity/jump callback.
            RequireBombTorizoState(torizo).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                ? TorizoBodyProgram.FacingRightFalling0
                : TorizoBodyProgram.FacingLeftFalling0);
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = new EightEightVector<RoomPixels>(0, 256);
        }
    }

    /// <summary>Uses Golden Torizo's timed facing transition and landing lists during an airborne probe.</summary>
    private void RunGoldenTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        // $AA:D5F1 is the Golden analogue of $C752. Its only behavioral difference is the
        // sixteen-frame facing transition and Golden landing lists selected when it expires.
        if (!state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = state.AirTransitionTimer.Decrement().Remaining;
            if (state.AirTransitionTimer.IsZero)
            {
                RequireBombTorizoState(torizo).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                    ? TorizoBodyProgram.GoldenTurningLeft
                    : TorizoBodyProgram.GoldenTurningRight);
                torizo.InstructionTimer = NativeCountdown.Start(1);
                return;
            }
        }

        int fallingSpeed = Math.Min(
            Math.Abs(state.Velocity.X.SignedRawValue) + 1,
            15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SnesSixteenSixteen.FromWhole(fallingSpeed)))
        {
            RequireBombTorizoState(torizo).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                ? TorizoBodyProgram.FacingRightFalling0
                : TorizoBodyProgram.FacingLeftFalling0);
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = new EightEightVector<RoomPixels>(0, 256);
        }
    }

    /// <summary>Uses the room RNG to emit drool only while a living Torizo is below the head-break threshold.</summary>
    private void MaybeSpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo)
    {
        if (torizo.Health is 0 or >= BombTorizoHeadExplosionHealth)
            return;
        if (_room.Random.Read() is ushort random &&
            (random & TorizoRandomDefinitions.LowHealthDroolMask) == 0)
        {
            SpawnBombTorizoInitialDrool(torizo);
        }
    }

    /// <summary>Ports Bomb Torizo's custom hurt callback $AA:C67E.</summary>
    internal void ApplyBombTorizoHurt(RoomEnemySlot torizo)
    {
        MaybeSpawnBombTorizoLowHealthDrool(torizo);
        if ((torizo.FlashTimer.RawValue & 1) == 0)
        {
            if (torizo.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo)
                LoadGoldenTorizoHealthPalette(torizo.Health);
            else
                LoadTorizoDeathPalette();
        }
        else
        {
            for (int color = 0; color < 16; color++)
            {
                _presentation.BossPalettes.TorizoBody.Write(color, Bgr555.White);
                _presentation.BossPalettes.TorizoBelly.Write(color, Bgr555.White);
            }
        }
    }

    /// <summary>Enters the authored death list once and removes the dying statue from Samus collision.</summary>
    internal static void BeginBombTorizoDeath(RoomEnemySlot torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Function = TorizoFunction.Idle;
        state.BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.DeathSequence0);
        torizo.InstructionTimer = NativeCountdown.Start(1);
        SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp | TorizoBehaviorFlags.FaceBlownUp);
        // EnemyShot_Torizo_Normal `$AA:C9B7-$AA:C9BD` sets property $0400 when
        // entering the death stream. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible instruction frames continue.
        torizo.Properties = torizo.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Records the area boss bit, scatters sixteen drops in the variant's arena, and requests post-fight music.</summary>
    private void FinishBombTorizoDeath(TorizoEnemyState state)
    {
        _room.Services.Bosses.SetDefeated(BossBits.AreaTorizo);
        state.BossBitSet = true;

        // The two statues share header $EEFF's chance table but use different native
        // arena rectangles: Bomb Torizo uses the 128x64 upper room region, while Golden
        // Torizo uses a 256x64 strip much lower in its room.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.BombTorizo,
            count: 16,
            xBase: state.IsGolden ? (ushort)128 : (ushort)64,
            xMask: state.IsGolden ? TorizoRandomDefinitions.GoldenDropXMask : TorizoRandomDefinitions.BombDropXMask,
            yBase: state.IsGolden ? (ushort)288 : (ushort)96,
            yMask: TorizoRandomDefinitions.DropYMask);
        state.ItemDropRequested = true;
        LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
            MusicCommand.SelectTrack(3),
            MusicCommandDelay.EightFrames);
    }

    /// <summary>Torizo's posture bits from its enemy parameter 1.</summary>
    private static TorizoPostureFlags Posture(RoomEnemySlot torizo) => (TorizoPostureFlags)torizo.Parameter1 & TorizoPostureFlags.All;

    /// <summary>Whether Torizo faces right.</summary>
    private static bool FacesRight(RoomEnemySlot torizo) => Posture(torizo).HasFlag(TorizoPostureFlags.FacingRight);

    /// <summary>Replaces Torizo's posture bits, keeping parameter 1's low thirteen bits.</summary>
    private static void SetPosture(RoomEnemySlot torizo, TorizoPostureFlags posture) =>
        torizo.Parameter1 = (ushort)(((TorizoPostureFlags)torizo.Parameter1 & ~TorizoPostureFlags.All) | posture);

    /// <summary>Whether Torizo has latched <paramref name="flag"/>.</summary>
    private static bool HasBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flag) => ((TorizoBehaviorFlags)torizo.Parameter2).HasFlag(flag);

    /// <summary>Latches <paramref name="flags"/>.</summary>
    private static void SetBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => torizo.Parameter2 = (ushort)((TorizoBehaviorFlags)torizo.Parameter2 | flags);

    /// <summary>Clears <paramref name="flags"/>.</summary>
    private static void ClearBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => torizo.Parameter2 = (ushort)((TorizoBehaviorFlags)torizo.Parameter2 & ~flags);

    /// <summary>Queues <paramref name="sound"/> through the shared enemy queue.</summary>
    private void PlayTorizoSound(SoundEffectId sound)
    {
        _audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>Requires a collision layer for movement callbacks that cannot run outside a loaded room.</summary>
    private static RoomLevelData RequireBombTorizoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Bomb Torizo movement requires the active room collision layer.");

    /// <summary>Rejects a slot unless it is the statue currently bound to this room owner.</summary>
    internal TorizoEnemyState RequireBombTorizoState(RoomEnemySlot torizo) =>
        _torizoState is not null && ReferenceEquals(_torizoState.Slot, torizo)
            ? _torizoState
            : throw new InvalidOperationException(
                $"Enemy slot {torizo.SlotIndex} has no initialized Bomb Torizo state.");

    /// <summary>Routes overlapping normal bombs through the selected Torizo hitbox callback.</summary>
    internal void RegisterBombTorizoBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            c => ResolveTorizoNormalBomb(c.Enemy, c.Bomb, c.SelectedShotAiPointer));
    }

    /// <summary>Installs movement handlers and typed program cues for statue fragments, orbs, attacks, and reflected missiles.</summary>
    internal void RegisterBombTorizoProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunBombTorizoStatueBreakingPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling);

        table.RegisterPreInstruction(context =>
        {
            RunBombTorizoChozoOrbPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizosChozoOrbs);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoChozoOrbPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs);

        table.RegisterPreInstruction(context =>
        {
            RunBombTorizoSonicBoomPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.TorizoSonicBoom);

        table.RegisterPreInstruction(context =>
        {
            RunBombTorizoDroolPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizoLowHealthDrool_Falling);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoEggPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoEggHorizontalCharge(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoEggFall(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoSuperMissilePreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoSuperMissileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown);

        table.RegisterPreInstruction(context =>
        {
            RunGoldenTorizoEyeBeamPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEyeBeam);

        table.RegisterBranchingCue<TorizoProjectileProgram, TorizoProjectileCue>(ApplyTorizoProjectileCue);
        table.RegisterShotAnimation(RoomEnemyProjectileKind.BombTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbShot));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.GoldenTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbShot));
        // The egg's definition names the Chozo orb's wall-impact list as its shot list.
        table.RegisterShotAnimation(RoomEnemyProjectileKind.GoldenTorizoEgg, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbWallImpact));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.GoldenTorizoSuperMissile, () => new TorizoProjectileAnimation(TorizoProjectileProgram.SuperMissileImpact));
    }

    /// <summary>
    /// Carries out one cue of a Torizo projectile's program, returning the program a conditional cue continues at, or
    /// null to run on.
    /// </summary>
    internal TorizoProjectileProgram? ApplyTorizoProjectileCue(RoomEnemyProjectileSlot projectile, SamusState? samus, TorizoProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case TorizoProjectileCueKind.ClearPreInstruction:
                projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_868170;
                return null;
            case TorizoProjectileCueKind.SetPreInstruction:
                projectile.PreInstruction = (ushort)cue.PreInstruction;
                return null;
            case TorizoProjectileCueKind.SetProperties:
                EnemyProjectileEngine.SetPropertyFlags(projectile, cue.Properties);
                return null;
            case TorizoProjectileCueKind.ClearProperties:
                EnemyProjectileEngine.ClearPropertyFlags(projectile, cue.Properties);
                return null;
            case TorizoProjectileCueKind.PlaySound:
                _audio.QueueEnemySound(cue.Sound, cue.MaximumQueued);
                return null;
            case TorizoProjectileCueKind.Delete:
                projectile.Clear();
                return null;
            case TorizoProjectileCueKind.Hatch:
                // Variable 0 holds the launching Torizo's facing word, negative when it faces right.
                return unchecked((short)projectile.Variable0) < 0 ? TorizoProjectileProgram.EggHatchedRight : TorizoProjectileProgram.EggHatchedLeft;
            case TorizoProjectileCueKind.GoToIfEyeBeamExplosionsDisabled:
            {
                TorizoEnemyState state = GoldenTorizo ??
                    throw new InvalidOperationException("Golden Torizo's eye beam has no owning Torizo state.");
                return state.EyeBeamExplosionsEnabled ? null : cue.Target;
            }
            case TorizoProjectileCueKind.GoToSometimes:
                return _projectiles.TakeOneInFourBranch() ? cue.Target : null;
            case TorizoProjectileCueKind.AimSuperMissileTowardSamus:
            case TorizoProjectileCueKind.AimSuperMissileAwayFromSamus:
                SetGoldenTorizoSuperMissileVelocity(
                    projectile,
                    samus ?? throw new InvalidOperationException("Golden Torizo super-missile aiming requires the active Samus actor."),
                    awayFromSamus: cue.Kind == TorizoProjectileCueKind.AimSuperMissileAwayFromSamus);
                return null;
            case TorizoProjectileCueKind.SetRadius:
                projectile.Radius = cue.Radius;
                return null;
            case TorizoProjectileCueKind.SpawnOrbDrops:
                RequestTorizoChozoOrbDrop(projectile);
                return null;
            case TorizoProjectileCueKind.Scatter:
                _projectiles.Scatter(projectile, cue.Scatter);
                return null;
            case TorizoProjectileCueKind.ReturnToOrigin:
                projectile.Position = projectile.Position.WithWhole(new(projectile.Variable0, projectile.Variable1));
                return null;
            case TorizoProjectileCueKind.RaiseDust:
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 4)));
                return null;
            case TorizoProjectileCueKind.StopBlockingShots:
                projectile.BlocksSamusProjectiles = false;
                return null;
            case TorizoProjectileCueKind.StopHarmingSamus:
                projectile.CanDamageSamus = false;
                return null;
            case TorizoProjectileCueKind.DrawInFront:
                projectile.DrawPriority = EnemyProjectileDrawPriority.High;
                return null;
            case TorizoProjectileCueKind.NoOperation:
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The program a Torizo projectile's typed animation is running.</summary>
    private static TorizoProjectileProgram RunningTorizoProjectileProgram(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is TorizoProjectileAnimation animation
            ? animation.Cursor.Program
            : throw new InvalidOperationException($"Enemy projectile {projectile.Kind} is not running a Torizo program.");

    /// <summary>Ports BombTorizo's initialization AI.</summary>
    internal void RunBombTorizoInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? samus, SnesButton controllerInput, ushort _1)
    {
        InitializeBombTorizo(slot, samus, controllerInput);
    }

    /// <summary>Ports BombTorizo's main AI.</summary>
    internal void RunBombTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBombTorizoMain(slot, RequireBombTorizoState(slot), frame.Samus, frame.Level);
    }

    /// <summary>Ports GoldenTorizo's main AI.</summary>
    internal void RunGoldenTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGoldenTorizoMain(
            slot,
            RequireBombTorizoState(slot),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances BombTorizo's typed animation.</summary>
    internal void AdvanceBombTorizoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceTorizoAnimation(slot, new TorizoInstructionInputs { Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounterLow = unchecked((byte)_frameContext._enemyFrameNmiFrameCounter) });
    }

    /// <summary>Validates extended hitbox callbacks, handles catches, and defers death to Torizo's own sequence.</summary>
    internal void RegisterBombTorizoShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == (enemy.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo
                                                         ? EnemyInteractionCallback.BombTorizoShot
                                                         : EnemyInteractionCallback.GoldenTorizoShot))
        {
            // Golden's definition installs $D667, but its ordinary extended body hitboxes call shared $C97C; that
            // wrapper dispatches to $D667 by area. Stand-up/sit-down maps deliberately use $C9C2 instead. That
            // callback is a no-op for area-zero Bomb Torizo and a guarded direct-damage path for Golden Torizo, so it
            // must remain distinguishable below.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes || c.HitboxShotAi is not (
                        EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.TorizoStandUpSitDownShot))
                {
                    throw new InvalidDataException(
                        $"Torizo hitbox shot AI $AA:{c.HitboxShotAiPointer:X4} is not translated.");
                }
                return false;
            },
            BeforeImpact = ResolveTorizoShotBeforeImpact,
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) =>
            {
                if (c.Enemy.Health == 0)
                    BeginBombTorizoDeath(c.Enemy, RequireBombTorizoState(c.Enemy));
            },
        });
    }

    /// <summary>Registers extended body touch damage without invoking the generic death animation.</summary>
    internal void RegisterBombTorizoTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BombTorizoTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes || context.HitboxTouchAi != EnemyInteractionCallback.BombTorizoTouch)
                {
                    throw new InvalidDataException(
                        $"Torizo hitbox touch AI $AA:{context.HitboxTouchAiPointer:X4} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
                if (context.Slot.Health == 0)
                    BeginBombTorizoDeath(context.Slot, RequireBombTorizoState(context.Slot));
            });
    }
    /// <summary>Runs Bomb or Golden Torizo's body program where the enemy instruction interpreter would.</summary>
    internal void AdvanceTorizoAnimation(RoomEnemySlot torizo, TorizoInstructionInputs inputs)
    {
        if (!EnemyProgramAnimation.Expire(torizo))
            return;
        TorizoEnemyState state = RequireBombTorizoState(torizo);
        AnimationProgramCursor<TorizoBodyProgram> cursor = state.BodyProgram;
        // The death and wake repeats count the body's timer word, as the native timer commands do.
        EnemyProgramAnimation.RunTo(torizo, TorizoAnimations.Programs, ref cursor, TorizoFrameDefinitions.Spritemap,
            (cue, continuation) => ApplyTorizoCue(torizo, state, inputs, cue, continuation));
        state.BodyProgram = cursor;
    }

    /// <summary>Copies tile transfer <paramref name="transfer"/>'s installed characters to VRAM, as the common $814B does.</summary>
    [PpuLayoutBoundary("codec: Torizo instruction-list VRAM transfer records ($AA, bank-$A0 $814B)")]
    private void ApplyTorizoTileTransfer(int transfer)
    {
        TorizoInstructionVramTransferDefinition definition = TorizoInstructionVramTransferDefinitions.Entries[transfer];
        var installed = _room.Artwork?.TorizoInstructionVram ?? throw new InvalidDataException(
            "Torizo instruction-time tile transfers require installed artwork.");
        if (!installed.TryResolve(definition.SourceAddress, definition.ByteCount, out ReadOnlyMemory<byte> characters))
            throw new InvalidDataException($"Torizo tile transfer {transfer} has no installed art.");
        _presentation.Vram.LoadBytes(definition.DestinationWord * 2, characters.Span);
    }

    /// <summary>
    /// Carries out one cue of Bomb or Golden Torizo's body program, returning the cursor a branch, call or return
    /// continues at, or null to run on. <paramref name="continuation"/> is the cursor just past the cue: the return
    /// point a call saves. <see cref="TorizoInstructionInputs.FrameCounterLow"/> is the low byte of
    /// <c>NMI_FrameCounter</c> ($05B6), which both attack choosers ($AA:C5B9, $AA:D53B) add; only its bit 3 survives.
    /// </summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ApplyTorizoCue(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        TorizoInstructionInputs inputs,
        TorizoCue cue,
        AnimationProgramCursor<TorizoBodyProgram> continuation)
    {
        switch (cue.Kind)
        {
            case TorizoCueKind.SetPreInstruction:
                state.PreInstruction = cue.PreInstruction;
                return null;

            case TorizoCueKind.ClearPreInstruction:
                state.PreInstruction = TorizoPreInstruction.Idle;
                return null;

            case TorizoCueKind.SetFunction:
                state.Function = cue.Function;
                return null;

            case TorizoCueKind.BlowUpGut:
                SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp);
                for (int piece = 0; piece < 6; piece++)
                    SpawnBombTorizoLowHealthDrool(torizo);
                return null;

            case TorizoCueKind.MarkFaceBlownUp:
                SetBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
                return null;

            case TorizoCueKind.MakeVisible:
                torizo.Properties = torizo.Properties.Without(EnemyProperties.Invisible);
                return null;

            case TorizoCueKind.MakeInvisible:
                torizo.Properties = torizo.Properties.With(EnemyProperties.Invisible);
                return null;

            case TorizoCueKind.FadeToBlack:
                SetTorizoPaletteTarget(state, new Bgr555[32]);
                return null;

            case TorizoCueKind.FinishDeath:
                FinishBombTorizoDeath(state);
                return null;

            case TorizoCueKind.AdvancePaletteFade:
                GetTorizoPaletteTransition(state).Step(_presentation.Cgram, TorizoPaletteDefinitions.BodyPaletteMask);
                return null;

            case TorizoCueKind.FadeToNormalColors:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Normal);
                return null;

            case TorizoCueKind.StartBombTorizoFight:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                return null;

            case TorizoCueKind.NoOperation:
                return null;

            case TorizoCueKind.LockAnimation:
                state.ShotGuard = TorizoCombatDefinitions.AwakeningShotGuard;
                return null;

            case TorizoCueKind.UnlockAnimation:
                state.ShotGuard = 0;
                return null;

            case TorizoCueKind.GoToIfFaceBlownUpElseIfGolden:
            {
                if (HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp))
                    return AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target);
                // The Bomb variant runs on past both targets.
                return state.IsGolden ? AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Alternate) : null;
            }

            case TorizoCueKind.Link:
                state.ReturnCursor = AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target);
                return null;

            case TorizoCueKind.Return:
                return state.ReturnCursor;

            case TorizoCueKind.ResumeInterrupted:
                return state.InterruptedCursor;

            case TorizoCueKind.SpawnLowHealthExplosions:
                for (int explosion = 0; explosion < 6; explosion++)
                    SpawnBombTorizoLowHealthExplosion(torizo, cue.Value);
                // The program then waits forty updates, as the native callback's sleep does.
                torizo.FlashTimer = NativeCountdown.Start(40);
                return null;

            case TorizoCueKind.SpawnDeathExplosion:
                SpawnBombTorizoDeathExplosion(torizo);
                // The program then waits one update, as the native callback's sleep does.
                torizo.FlashTimer = NativeCountdown.Start(1);
                return null;

            case TorizoCueKind.SpawnLandingDust:
                SpawnBombTorizoLandingDust(torizo, rightFoot: true);
                SpawnBombTorizoLandingDust(torizo, rightFoot: false);
                return null;

            case TorizoCueKind.SpawnLowHealthDrool:
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    SpawnBombTorizoInitialDrool(torizo);
                return null;

            case TorizoCueKind.SetTurningAround:
                SetPosture(torizo, Posture(torizo) | TorizoPostureFlags.TurningAround);
                return null;

            case TorizoCueKind.SteppedLeftWithLeftFoot:
                SetPosture(torizo, TorizoPostureFlags.None);
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                return null;

            case TorizoCueKind.SteppedRightWithRightFoot:
                SetPosture(torizo, TorizoPostureFlags.FacingRight);
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                return null;

            case TorizoCueKind.SteppedLeftWithRightFoot:
                SetPosture(torizo, TorizoPostureFlags.OppositeLegForward);
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                return null;

            case TorizoCueKind.SteppedRightWithLeftFoot:
                SetPosture(torizo, TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward);
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                return null;

            case TorizoCueKind.StandUp:
                ApplyBombTorizoMapOffset(torizo, cue.Value, subtract: false);
                return null;

            case TorizoCueKind.SitDown:
                ApplyBombTorizoMapOffset(torizo, cue.Value, subtract: true);
                return null;

            case TorizoCueKind.WalkBombTorizo:
                return ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { TableOffset = cue.Value, CollisionFacingRight = TorizoBodyProgram.FacingLeftTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightTurningRight });

            case TorizoCueKind.WalkFacelessBombTorizo:
                return ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { TableOffset = cue.Value, CollisionFacingRight = TorizoBodyProgram.FacingLeftFacelessTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightFacelessTurningRight });

            case TorizoCueKind.GoToIfRising:
                return state.Velocity.Y.SignedRawValue < 0 ? AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target) : null;

            case TorizoCueKind.CallIfSamusCloseBehind:
                return SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    cue.Target);

            case TorizoCueKind.JumpBackIfSamusCloseInFront:
                return SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    inputs.Samus,
                    cue.Target);

            case TorizoCueKind.CallBombTorizoAttack:
            {
                if (inputs.Samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                state.ReturnCursor = continuation;
                bool chooseFirst = inputs.Samus.Missiles < 5 ||
                    ((inputs.FrameCounterLow + (inputs.Samus.Position.X.Whole & 1) + (inputs.Samus.Position.X.Whole >> 1)) & 8) != 0;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(chooseFirst ? cue.Target : cue.Alternate);
            }

            case TorizoCueKind.SpawnBombTorizoChozoOrbs:
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                return null;

            case TorizoCueKind.SpawnBombTorizoSonicBoom:
                SpawnBombTorizoSonicBoom(torizo, cue.Value);
                return null;

            case TorizoCueKind.SpawnGoldenTorizoSonicBoom:
                SpawnGoldenTorizoSonicBoom(torizo, cue.Value);
                return null;

            case TorizoCueKind.SpawnExplosiveSwipe:
                SpawnBombTorizoExplosiveSwipe(torizo, cue.Value);
                return null;

            case TorizoCueKind.PlayShotSound:
                PlayTorizoSound(SoundEffectLibrary2Sounds.ShotTorizo);
                return null;

            case TorizoCueKind.PlayFootstepSound:
                PlayTorizoSound(SoundEffectLibrary2Sounds.TorizoFootsteps);
                return null;

            case TorizoCueKind.GoToIfAirborne:
                return torizo.Position.Y.Whole == 375 ? null : AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target);

            case TorizoCueKind.LoadGoldenTorizoPalettes:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Golden);
                return null;

            case TorizoCueKind.StartGoldenTorizoFight:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                torizo.Radius = new(18, 48);
                return null;

            case TorizoCueKind.ClearCaughtSuperMissile:
                ClearBehavior(torizo, TorizoBehaviorFlags.CaughtSuperMissile);
                return null;

            case TorizoCueKind.SpawnEgg:
                SpawnGoldenTorizoEgg(torizo);
                return null;

            case TorizoCueKind.GoToIfEyeBeamActive:
                return _projectiles.EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target)
                    : null;

            case TorizoCueKind.DisableEyeBeamExplosions:
                state.EyeBeamExplosionsEnabled = false;
                return null;

            case TorizoCueKind.EnableEyeBeamExplosions:
                state.EyeBeamExplosionsEnabled = true;
                return null;

            case TorizoCueKind.UnmarkStunned:
                ClearBehavior(torizo, TorizoBehaviorFlags.Stunned);
                return null;

            case TorizoCueKind.PlayEggReleasedSound:
                PlayTorizoSound(SoundEffectLibrary2Sounds.GoldenTorizoEggReleased);
                return null;

            case TorizoCueKind.PlayLaserSound:
                PlayTorizoSound(SoundEffectLibrary2Sounds.GoldenTorizoLaser);
                return null;

            case TorizoCueKind.PlaySonicBoomSound:
                PlayTorizoSound(SoundEffectLibrary2Sounds.TorizoSonicBoom);
                return null;

            case TorizoCueKind.SpawnSuperMissile:
                SpawnGoldenTorizoSuperMissile(torizo);
                return null;

            case TorizoCueKind.GoToIfSamusMorphedBehind:
                return SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    cue.Target);

            case TorizoCueKind.SpawnEyeBeam:
                SpawnGoldenTorizoEyeBeam(torizo, cue.Value);
                return null;

            case TorizoCueKind.CallSometimesIfSamusMorphedInFront:
                return SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    cue.Target);

            case TorizoCueKind.CallSometimesIfHealthLow:
                if (torizo.Health > 1928 || (_room.Random.Generate() & TorizoRandomDefinitions.LowHealthCallMask) != 0)
                    return null;
                state.DecisionCounter = 0;
                state.ReturnCursor = continuation;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target);

            case TorizoCueKind.CallIfStunHealthHigh:
                if (torizo.Health <= 10800 ||
                    !HasBehavior(torizo, TorizoBehaviorFlags.Stunned))
                    return null;
                state.ReturnCursor = continuation;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(cue.Target);

            case TorizoCueKind.JumpForwardIfSamusFar:
                return SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.ControllerInput,
                    cue.Target);

            case TorizoCueKind.SpawnGoldenTorizoChozoOrbs:
                SpawnGoldenTorizoChozoOrb(torizo);
                return null;

            case TorizoCueKind.JumpBackIfSamusCloseInFrontGolden:
                return SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    inputs.Samus,
                    cue.Target);

            case TorizoCueKind.CallGoldenTorizoAttack:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(inputs.Samus);
                state.ReturnCursor = continuation;
                bool chooseFirst = activeSamus.Missiles < 32 ||
                    ((inputs.FrameCounterLow + (activeSamus.Position.X.Whole & 1) +
                        (activeSamus.Position.X.Whole >> 1)) & 8) != 0;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(chooseFirst ? cue.Target : cue.Alternate);
            }

            case TorizoCueKind.WalkGoldenTorizo:
                return ProcessGoldenTorizoWalkInstruction(
                    torizo,
                    state,
                    inputs.Samus,
                    RequireBombTorizoLevel(inputs.Level),
                    cue.Value);

            case TorizoCueKind.CopyTiles:
                ApplyTorizoTileTransfer(cue.Transfer);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Branches to the grounded-ball response only when Samus is four to 39 pixels away behind the statue.</summary>
    private static AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoMorphBallBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        bool isGroundedBall = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose;
        if (BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 4 || distance >= 40 || !isGroundedBall)
        {
            return null;
        }

        state.DecisionCounter = 0;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Uses the room RNG to select a forward-facing attack at medium horizontal range.</summary>
    private AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoMediumRangeBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        AnimationProgramCursor<TorizoBodyProgram> continuation,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (!BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 32 || distance >= 96 || (_room.Random.Generate() & TorizoRandomDefinitions.MediumRangeAttackMask) != 0)
        {
            return null;
        }

        state.ReturnCursor = continuation;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Launches the forward leap when distant Samus movement or sustained space jumping breaks the wait.</summary>
    private AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoSpaceJumpCounter(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        SnesButton controllerInput,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (distance < 112 || !BombTorizoFunction12IsNonNegative(torizo, samus))
            return null;
        SnesButton heldButtons = controllerInput;
        if (state.SamusSpaceJumpFrames <= 360 &&
            (!heldButtons.HasAny(SnesButtons.HorizontalDirections) ||
                (_room.Random.Generate() & TorizoRandomDefinitions.ForwardLeapMask) == 0))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Starts a backward jump after enough decisions or when Samus closes inside 32 pixels.</summary>
    private static AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoRepeatedJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (state.DecisionCounter < 8 &&
            (distance >= 32 || !BombTorizoFunction12IsNonNegative(torizo, samus)))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoBackwardJump(torizo, state);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Moves by the Golden walk table, branches on wall collision, and arms a sixteen-frame air transition near Samus.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ProcessGoldenTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort tableOffset)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(GoldenTorizoWalkDefinitions.Velocity(tableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = NativeCountdown.Start(0);
            return AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                ? TorizoBodyProgram.GoldenTurningLeft
                : TorizoBodyProgram.GoldenTurningRight);
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = NativeCountdown.Start(16);
        }
        return null;
    }

    /// <summary>Sets the facing-dependent forward leap velocity and restarts its instruction timer.</summary>
    private static void StartGoldenTorizoForwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            FacesRight(torizo) ? (short)512 : (short)-512, -1472);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Sets the faster reverse leap velocity used by Golden Torizo's repeated-jump branch.</summary>
    private static void StartGoldenTorizoBackwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            FacesRight(torizo) ? (short)-768 : (short)768, -1152);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Requires the active Samus actor for distance and attack decisions in Golden bytecode.</summary>
    private static SamusState RequireGoldenTorizoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Golden Torizo instruction selection requires the active Samus actor.");

    /// <summary>Applies or reverses the posture offset indexed by a Bomb Torizo movement table operand.</summary>
    internal static void ApplyBombTorizoMapOffset(
        RoomEnemySlot torizo,
        ushort tableOffset,
        bool subtract)
    {
        Offset16<RoomPixels> displacement = BombTorizoMovementDefinitions.Posture(tableOffset);
        torizo.Position += subtract ? -displacement : displacement;
    }

    /// <summary>Uses the walk table's velocity and wall branches, arming a 72-frame air transition when Samus crosses the facing test.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ProcessBombTorizoWalkInstruction(RoomEnemySlot torizo, TorizoEnemyState state, SamusState? samus, RoomLevelData level, TorizoWalkStep step)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(BombTorizoMovementDefinitions.WalkVelocity(step.TableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = NativeCountdown.Start(0);
            return AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(torizo)
                ? step.CollisionFacingRight
                : step.CollisionFacingLeft);
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = NativeCountdown.Start(72);
        }
        return null;
    }

    /// <summary>Saves the next instruction and branches when Samus is within 56 pixels behind the statue.</summary>
    private static AnimationProgramCursor<TorizoBodyProgram>? SelectBombTorizoCloseBehindBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        AnimationProgramCursor<TorizoBodyProgram> continuation,
        TorizoBodyProgram target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 56)
            return null;
        if (BombTorizoParameterMatchesSamusDelta(torizo, samus))
            return null;
        state.ReturnCursor = continuation;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Launches the reverse jump when Samus is within 32 pixels in the selected facing half-plane.</summary>
    private static AnimationProgramCursor<TorizoBodyProgram>? SelectBombTorizoCloseFrontJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 32 ||
            !BombTorizoFunction12IsNonNegative(torizo, samus))
        {
            return null;
        }

        // $AA:C22D launches away from the currently faced direction at 3 px/frame and
        // starts with -4.5 px/frame vertical speed, adding 40/256 px/frame each tick.
        state.Velocity = new EightEightVector<RoomPixels>(
            FacesRight(torizo) ? (short)-768 : (short)768, -1152);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>
    /// Compares the facing bit with Samus-X minus enemy-X for walking branches; the
    /// cartridge's other facing helper deliberately uses the inverse subtraction.
    /// </summary>
    private static bool BombTorizoParameterMatchesSamusDelta(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // Instructions $C470/$C4E5/$C567 compare the facing bit against SamusX-enemyX.
        // Keep that subtraction direction explicit: Torizo_Func_12 below deliberately uses
        // the inverse delta, and collapsing both tests into one "faces Samus" helper silently
        // reverses one family of ROM branches.
        return (((short)torizo.Parameter1 ^ unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) &
            short.MinValue) == 0;
    }

    /// <summary>Tests the native parameter-one XOR enemy-X minus Samus-X sign used by attack branches.</summary>
    internal static bool BombTorizoFunction12IsNonNegative(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // $AA:C58B calls Torizo_Func_12, whose operand is parameter1 XOR
        // (enemyX-SamusX), rather than the SamusX-enemyX test used by the walkers.
        return (((short)torizo.Parameter1 ^ unchecked((short)(torizo.Position.X.Whole - samus.Position.X.Whole))) &
            short.MinValue) == 0;
    }
    /// <summary>Requests the recurring hurt-state drool variant with randomized flight.</summary>
    internal void SpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoLowHealthDrool);

    /// <summary>Requests the awakening drool variant whose position and speed use carry-sensitive jitter.</summary>
    internal void SpawnBombTorizoInitialDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);

    /// <summary>Allocates either drool program and consumes room RNG in the native variant-specific order.</summary>
    private void SpawnBombTorizoDrool(
        RoomEnemySlot torizo,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolNoDelay));

        ushort random;
        if (kind == RoomEnemyProjectileKind.BombTorizoLowHealthDrool)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(SelectLowHealthDroolProgram(_room.Random.Generate())));
            random = _room.Random.Generate();
        }
        else
        {
            random = _room.Random.Generate();
        }
        if (kind == RoomEnemyProjectileKind.BombTorizoInitialDrool)
        {
            // $86:A66D-$A683 chain the carry: CLC; ADC Y; ADC #$FFFB, then the Y velocity's
            // ADC #$0030 adds that second addition's carry (set whenever Y + jitter >= 5).
            int jitteredY = (random & 3) + torizo.Position.Y.Whole;
            int yWithOffset = jitteredY + TorizoMotionDefinitions.InitialDroolLift;
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)yWithOffset));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((random & TorizoRandomDefinitions.DroolFallJitterMask) + 48 + SnesCarry.OutOfWord(yWithOffset)))));
            // $86:A68D-$A6C3: the X offset's ADC carries out of CLC; ADC X. Every branch
            // stores a zero X velocity.
            int jitteredX = (_room.Random.Generate() & 3) + torizo.Position.X.Whole;
            int carry = SnesCarry.OutOfWord(jitteredX);
            projectile.Position = projectile.Position.WithWholeX(Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround)
                ? unchecked((ushort)jitteredX)
                : FacesRight(torizo)
                    ? unchecked((ushort)(jitteredX + 8 + carry))
                    : unchecked((ushort)(jitteredX + TorizoMotionDefinitions.InitialDroolLeftOffset + carry)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(torizo.Position.Y.Whole - 5)));

        // The recurring low-health drool chooses a random direction ($86:A5F9-$A62C). The
        // turning branch's word index is random & $1FE; the X velocity is the sign-extended
        // sine and the Y velocity the negative cosine of that angle.
        int angle;
        if (Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround))
        {
            angle = (random & TorizoRandomDefinitions.DroolTurningAngleMask) >> 1;
        }
        else
        {
            int baseAngle = FacesRight(torizo) ? 32 : 224;
            angle = unchecked((byte)(baseAngle + (random & TorizoRandomDefinitions.DroolFacingAngleMask) - 8));
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedSine(unchecked((byte)angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle))));
        projectile.Position = projectile.Position.WithWholeX(FacesRight(torizo)
            ? unchecked((ushort)(torizo.Position.X.Whole + 8))
            : unchecked((ushort)(torizo.Position.X.Whole - 8)));
    }

    /// <summary>
    /// Applies the native <c>LSR / AND #$000E</c> index into the eight-entry <c>$86:A64D</c> low-health-drool list table,
    /// whose entries repeat the no-delay, two-frame and four-frame lists.
    /// </summary>
    internal static TorizoProjectileProgram SelectLowHealthDroolProgram(ushort random) =>
        (((random >> 2) & 7) % 3) switch
        {
            0 => TorizoProjectileProgram.DroolNoDelay,
            1 => TorizoProjectileProgram.DroolTwoFrameDelay,
            _ => TorizoProjectileProgram.DroolFourFrameDelay,
        };

    /// <summary>Places a swipe explosion at the attack-table hand offset, mirroring it for right-facing Torizo.</summary>
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, ushort parameter)
    {
        Offset16<RoomPixels> placement = BombTorizoAttackDefinitions.Swipe(parameter);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SwipeInitial));

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            (FacesRight(torizo) ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the facing-adjusted table offset and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, ushort parameter)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttackDefinitions.LowHealthExplosion(
                parameter,
                facingRight: FacesRight(torizo));

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ExplosionLowHealthInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
        projectile.Variable0 = projectile.Position.X.Whole;
        projectile.Variable1 = projectile.Position.Y.Whole;
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later instruction frames.</summary>
    internal void SpawnBombTorizoDeathExplosion(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoDeathExplosion,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ExplosionDeathInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
        projectile.Variable0 = torizo.Position.X.Whole;
        projectile.Variable1 = torizo.Position.Y.Whole;
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb(
                facingRight: FacesRight(torizo)));
    }

    /// <summary>Starts Bomb Torizo's sonic-boom program at one of two randomized body heights.</summary>
    internal void SpawnBombTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Starts Golden Torizo's sonic-boom variant with the shared directional flight setup.</summary>
    internal void SpawnGoldenTorizoSonicBoom(RoomEnemySlot torizo, ushort parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Places a sonic boom 32 pixels ahead and selects the fired-left or fired-right list.</summary>
    private void SpawnTorizoSonicBoom(
        RoomEnemySlot torizo,
        ushort parameter,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        projectile.DirectionParameter = parameter;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
            torizo.Position.Y.Whole + ((_room.Random.Generate() & 1) != 0 ? -12 : 20))));
        if (FacesRight(torizo))
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole + 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(624));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomFiredRight));
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole - 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-624)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomFiredLeft));
        }
    }

    /// <summary>Allocates Golden Torizo's orb using the variant's facing-specific offset and randomized speed.</summary>
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(
                facingRight: FacesRight(torizo)));
    }

    /// <summary>Starts a Golden egg with the cartridge's fixed 66-frame hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEgg,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));

        projectile.Variable1 = TorizoMotionDefinitions.EggHatchTimer;
        projectile.Variable0 = torizo.Parameter1;
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg(
                movingRight: unchecked((short)torizo.Parameter1) < 0));
    }

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent program.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        bool facingRight = FacesRight(torizo);
        projectile.Variable0 = unchecked((ushort)(torizo.SlotIndex * 2));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (facingRight ? 30 : -30))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileProgram.SuperMissileRightInitial : TorizoProjectileProgram.SuperMissileLeftInitial));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemySlot torizo, ushort parameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            unchecked((ushort)(torizo.VramTilesIndex | torizo.PaletteIndex)));
        projectile.DirectionParameter = parameter;
        bool facingRight = FacesRight(torizo);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight));

        int angle = ((_room.Random.Generate() & TorizoRandomDefinitions.EyeBeamSpreadMask) - 16 + 192 + (facingRight ? 0 : 128)) & TorizoRandomDefinitions.EyeBeamAngleMask;
        int sineIndex = angle >> 1;
        // The native combined table starts at negative cosine, not sine. Its
        // quarter-turn offset selects sine for X; the unshifted sample gives Y.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex)))));
    }

    /// <summary>Applies the projectile definition and draws horizontal jitter before vertical jitter.</summary>
    internal void InitializeTorizoRandomizedProjectile(
        RoomEnemyProjectileSlot projectile,
        RoomEnemySlot torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Program));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + definition.Offset);
        // The native initializer draws the X jitter before the Y jitter.
        short xVelocity = unchecked((short)(
            definition.BaseVelocity.X.SignedRawValue + unchecked((byte)_room.Random.Generate()) - 128));
        short yVelocity = unchecked((short)(
            definition.BaseVelocity.Y.SignedRawValue + unchecked((byte)_room.Random.Generate()) - 128));
        projectile.Velocity = new EightEightVector<RoomPixels>(xVelocity, yVelocity);
    }

    /// <summary>Places a foot-specific dust effect 48 pixels below the statue after a landing step.</summary>
    internal void SpawnBombTorizoLandingDust(RoomEnemySlot torizo, bool rightFoot)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        RoomEnemyProjectileKind kind = rightFoot
            ? RoomEnemyProjectileKind.BombTorizoRightFootDust
            : RoomEnemyProjectileKind.BombTorizoLeftFootDust;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(rightFoot ? TorizoProjectileProgram.LandingDustRightFoot : TorizoProjectileProgram.LandingDustLeftFoot));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (rightFoot ? 24 : -24))), unchecked((ushort)(torizo.Position.Y.Whole + 48))));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void RunBombTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbWallImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbFloorImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 18))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact program.</summary>
    internal static void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomWallImpact));
            projectile.Variable0 = projectile.Position.X.Whole;
            projectile.Variable1 = projectile.Position.Y.Whole;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (unchecked((short)projectile.Velocity.X.RawValue) < 0 ? -16 : 16)))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.X))
            projectile.Clear();
    }

    /// <summary>
    /// Ports <c>$86:A887</c>, shared by Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load <c>$0003</c> when they cross zero.
    /// </summary>
    internal static void RunBombTorizoDroolPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolWallImpact));
            return;
        }

        short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
        if (xVelocity < 0)
        {
            int dragged = xVelocity + 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged < 0 ? dragged : 3))));
        }
        else
        {
            int dragged = xVelocity - 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged >= 0 ? dragged : 3))));
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 3)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolFloorImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void RunGoldenTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
        if (verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity >= 0
                ? xVelocity - 64
                : xVelocity + 64))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-(projectile.Velocity.Y.RawValue >> 1))));
            if (TorizoMotionDefinitions.HasSettled(projectile.Velocity.Y))
            {
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
                projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbFloorImpact));
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 24))));
    }

    /// <summary>Counts down the egg's hatch timer while bouncing it off terrain with horizontal drag and gravity.</summary>
    internal static void RunGoldenTorizoEggPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        NativeWordCounterStep hatch = NativeCountdown.FromRaw(projectile.Variable1).Decrement();
        projectile.Variable1 = hatch.Value;
        if (hatch.IsNegative)
        {
            // The hatch timer steps the sleeping list past its sleep.
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileAnimations.AfterSleep(RunningTorizoProjectileProgram(projectile))));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight)
                ? (ushort)256
                : unchecked((ushort)-256)));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));
            projectile.Variable0 = (ushort)((TorizoPostureFlags)projectile.Variable0 ^ TorizoPostureFlags.FacingRight);
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false) &&
            unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity + (xVelocity < 0 ? 32 : -32)))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.Y.RawValue))));
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Accelerates the hatched egg sideways and changes to its fall phase when it hits a wall.</summary>
    internal static void RunGoldenTorizoEggHorizontalCharge(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.PreInstruction =
                (ushort)EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall;
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight) ? 48 : -48)))));
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break program.</summary>
    internal static void RunGoldenTorizoEggFall(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight) ? TorizoProjectileProgram.EggBreakRight : TorizoProjectileProgram.EggBreakLeft));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
    }

    /// <summary>Keeps the held super missile beside Golden Torizo's hand, clearing it if its owner is gone.</summary>
    internal void RunGoldenTorizoSuperMissilePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        TorizoEnemyState? state = GoldenTorizo;
        if (state is null)
        {
            projectile.Clear();
            return;
        }

        RoomEnemySlot torizo = state.Slot;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole +
            (FacesRight(torizo) ? 32 : -32))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
    }

    /// <summary>Moves the thrown super missile until wall or downward floor impact, applying gravity in flight.</summary>
    internal static void RunGoldenTorizoSuperMissileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool impact = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true);
        if (!impact)
        {
            bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
            impact = verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0;
        }

        if (impact)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SuperMissileImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Aims the thrown missile toward or away from Samus using the native up-zero angle convention.</summary>
    internal static void SetGoldenTorizoSuperMissileVelocity(
        RoomEnemyProjectileSlot projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        byte angle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - projectile.Position.Y.Whole))));
        angle = awayFromSamus
            ? unchecked((byte)(angle | TorizoMotionDefinitions.AwayHalfTurn))
            : unchecked((byte)(angle & TorizoMotionDefinitions.TowardHalfTurn));

        // Preserve the native up-zero angle convention: X is sine, Y is
        // negative cosine. Starting both reads at sine rotates the shot.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle)))));
    }

    /// <summary>Selects wall or floor impact artwork when the eye beam strikes terrain.</summary>
    internal static void RunGoldenTorizoEyeBeamPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.EyeBeamWallImpact));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.EyeBeamFloorImpact));
        }
    }

    /// <summary>
    /// Ports instruction <c>$86:AB8A</c>. Its two inline header operands are Bomb Torizo's area-zero orb and Golden
    /// Torizo's nonzero-area orb, which the cue carries as its typed meaning. This runtime already identifies the
    /// concrete loaded variant, which is the exact room-scoped equivalent of consulting the global area byte.
    /// </summary>
    internal void RequestTorizoChozoOrbDrop(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Kind is not (
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
        {
            throw new InvalidOperationException(
                $"Projectile {projectile.Kind} reached the Torizo-orb drop.");
        }

        EnemyDefinitionId header = GoldenTorizo is not null
            ? EnemyDefinitionId.GoldenTorizoOrb
            : EnemyDefinitionId.BombTorizoOrb;

        RoomEnemyDefinition definition = EnemyPopulationLoader.ResolveRoomEnemyDefinition(_room.LoadedBus, header);
        _torizoOrbDropRequests.Add(new TorizoOrbDropRequest());
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            projectile.Position.Whole,
            definition.ItemDropChancesPointer);
    }
    /// <summary>
    /// Allocates the room-graphics enemy projectile requested by PLM instruction
    /// <c>$84:D357</c>, then runs initializer <c>$86:A764</c> against the publishing PLM's
    /// block coordinates.
    /// </summary>
    public RoomEnemyProjectileSlot? SpawnBombTorizoStatueBreakingProjectile(
        BombTorizoStatueProjectileRequest request)
    {
        _room.EnsureLoaded();
        if (request.DefinitionPointer !=
            BombTorizoStatueFragmentDefinitions.ProjectileDefinition)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Bomb Torizo statue projectile must use definition $A993, not " +
                $"${request.DefinitionPointer:X4}.");
        }
        BombTorizoStatueFragmentDefinition definition =
            BombTorizoStatueFragmentDefinitions.ForParameter(request.Parameter);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null; // SpawnEprojInner silently drops work when all eighteen slots fill.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            graphicsIndex: 0);

        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Program));
        projectile.Position = projectile.Position.WithWhole(request.PlmBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(definition.YVelocity));
        projectile.Variable1 = definition.Acceleration;
        return projectile;
    }

    /// <summary>
    /// Ports pre-instruction <c>$86:A8EF</c>, including its authentic unindexed read from
    /// physical slot zero's variable-F word when applying acceleration.
    /// </summary>
    internal void RunBombTorizoStatueBreakingPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool collided = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0 || !collided)
        {
            // The initializer writes $0010 to every fragment's own F word, but A8EF's ROM
            // instruction omits X and reads `$1B23` (physical slot zero) literally. The
            // retail quirk normally leaves these first eight actors at constant speed;
            // retaining it matters if another projectile already occupies slot zero.
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
                projectile.Velocity.Y.RawValue + _projectiles.EnemyProjectiles[0].Variable1))));
            if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(TorizoMotionDefinitions.RunawaySpeed));
            return;
        }

        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.BombTorizoStatueFragment_Stopped;
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetBombTorizoRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>
    /// Handles flash/awakening guards and Golden Torizo missile catches before shared
    /// shot damage; returns true when the hit was consumed by a special reaction.
    /// </summary>
    internal bool ResolveTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemySlot enemy = c.Enemy;
        bool isBombTorizo = enemy.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = !isBombTorizo;
        TorizoEnemyState torizoState = RequireBombTorizoState(enemy);
        if (c.HitboxShotAi == EnemyInteractionCallback.TorizoStandUpSitDownShot)
        {
            // `$AA:C9C2` returns immediately in area zero. In nonzero areas it calls `$AA:D658`, which admits common
            // no-death shot damage only while both the flash timer and native `toriz_var_04` are zero. Crucially it
            // never enters `$AA:D667`, so missiles are not caught, Supers are not reflected, and parameter-two
            // counterattack bits are not armed through these transitional body rectangles.
            if (isBombTorizo || !enemy.FlashTimer.IsZero || torizoState.ShotGuard != 0)
            {
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                return true;
            }
        }
        if (!enemy.FlashTimer.IsZero || isBombTorizo && torizoState.ShotGuard != 0)
        {
            // Both callbacks return during a damage flash. Bomb Torizo also rejects shots while its awakening guard
            // is set; Golden Torizo's nonzero guard instead routes through common damage below.
            c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
            return true;
        }

        // A caught Super bypasses family selection, but still reaches the native stun write before common damage.
        // Animation lock and the transitional hitbox callback bypass that write instead.
        if (c.HitboxShotAi == EnemyInteractionCallback.BombTorizoShot && isGoldenTorizo &&
            torizoState.ShotGuard == 0 &&
            HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
            SetBehavior(enemy, TorizoBehaviorFlags.Stunned);

        if (c.HitboxShotAi == EnemyInteractionCallback.BombTorizoShot &&
            isGoldenTorizo && torizoState.ShotGuard == 0 &&
            !HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
        {
            torizoState.CapturedProjectileFamily = (ushort)c.Family;
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                c.Projectile.Direction = new SamusProjectileDirectionWord(c.Projectile.Direction).WithoutCollisionLifecycleState();
                torizoState.Function = TorizoFunction.Idle;
                enemy.InstructionTimer = NativeCountdown.Start(1);
                RequireBombTorizoState(enemy).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(FacesRight(enemy)
                    ? TorizoBodyProgram.GoldenDodgeTurningRight
                    : TorizoBodyProgram.GoldenDodgeTurningLeft);
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (c.Samus is not null && BombTorizoFunction12IsNonNegative(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    SetBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile);
                    torizoState.Function = TorizoFunction.Idle;
                    c.Projectile.Direction = new SamusProjectileDirectionWord(c.Projectile.Direction).WithCollisionLifecycleState();
                    enemy.InstructionTimer = NativeCountdown.Start(1);
                    RequireBombTorizoState(enemy).BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(Posture(enemy).HasFlag(TorizoPostureFlags.OppositeLegForward)
                        ? FacesRight(enemy)
                            ? TorizoBodyProgram.GoldenCaughtSuperFacingRightLeftLegFwd
                            : TorizoBodyProgram.GoldenCaughtSuperFacingLeftRightLegFwd
                        : FacesRight(enemy)
                            ? TorizoBodyProgram.GoldenCaughtSuperFacingRightRightLegFwd
                            : TorizoBodyProgram.GoldenCaughtSuperFacingLeftLeftLegFwd);
                    return true;
                }
            }
            else
            {
                // Beams and other common-damage families arm the high-health counterattack branch before entering
                // normal shot AI.
                SetBehavior(enemy, TorizoBehaviorFlags.Stunned);
            }
        }
        return false;
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastBombTorizoMusicRequest?.Command, LastBombTorizoMusicRequest?.Delay);
    }

    /// <summary>
    /// Registers Torizo_Hurt for each variant; the common instruction interpreter still
    /// advances after the hurt callback, matching <c>$A0:8FF7</c>.
    /// </summary>
    internal void RegisterBombTorizoHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            (slot, _) => ApplyBombTorizoHurt(slot));

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindTorizoHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(enemy.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
            enemy.Definition.Bank == TorizoCollisionDefinitions.Bank))
            return null;

        foreach (GoldenTorizoCollisionComponent component in
                 TorizoCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (GoldenTorizoCollisionHitbox hitbox in
                     TorizoCollisionDefinitions.HitboxesAt(component.HitboxList))
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                return true;
            }
        }
        return false;
    
    }
}
