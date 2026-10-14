using SuperMetroid.Core.Frontend;
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

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BombTorizoBehavior : IEnemySpecies
{
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

    /// <summary>Instruction reached when the first body-break health threshold interrupts active AI.</summary>
    private const ushort BombTorizoLowHealthInterruptInstruction = 0xb0e5;
    /// <summary>Instruction reached for the later core-break recovery sequence.</summary>
    private const ushort BombTorizoLowHealthRecoveryInstruction = 0xb155;
    /// <summary>Entry into the death animation after damage reduces health to zero.</summary>
    private const ushort BombTorizoDeathInstruction = 0xb1c8;
    /// <summary>Initial extended spritemap shared by the two statue definitions.</summary>
    private const ushort BombTorizoInitialExtendedSpritemap = 0x87d0;
    /// <summary>Native awakening guard value that rejects Bomb Torizo hits until the list clears it.</summary>
    private const ushort BombTorizoShotGuardValue = 0x7777;

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
            if ((torizo.Parameter2 & 0x1000) == 0)
                state.CapturedProjectileFamily = SamusBombProjectileSystem.NormalBombType;
            torizo.Parameter2 |= GoldenTorizoBehavioralProperties.Stunned;
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

    /// <summary>Last library-two sound requested by the current Torizo frame.</summary>
    public ushort? LastBombTorizoSoundEffect { get; private set; }

    /// <summary>Last delayed music request emitted by awakening/death bytecode.</summary>
    public BombTorizoMusicRequest? LastBombTorizoMusicRequest { get; private set; }

    /// <summary>Discards the previous statue, orb requests, and frame-level sound/music publications on room load.</summary>
    internal void ResetBombTorizoRoomState()
    {
        _torizoState = null;
        _torizoOrbDropRequests.Clear();
        LastBombTorizoSoundEffect = null;
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>
    /// Ports shared <c>Torizo_Init</c> at $AA:C87F for the area-zero Bomb and area-two
    /// Golden definitions. The cartridge indexes parallel two-word tables by area; using
    /// the concrete definition here is equivalent and prevents an unrelated host area enum
    /// from becoming a second authority for actor identity.
    /// </summary>
    internal void InitializeBombTorizo(RoomEnemySlot torizo, SamusState? samus, ushort controllerInput)
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
        torizo.CurrentInstruction = definition.Instruction;
        torizo.Properties = unchecked((ushort)(torizo.Properties | definition.PropertyMask));
        torizo.ExtraProperties = torizo.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        torizo.Radius = definition.Radius;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        torizo.Timer = NativeCountdown.Start(0);
        torizo.PaletteIndex = 0;
        torizo.SpritemapPointer = BombTorizoInitialExtendedSpritemap;

        state.Function = TorizoFunction.Falling;
        state.PreInstruction = TorizoPreInstruction.Idle;
        state.Velocity = new EightEightVector<RoomPixels>(0, 0x0100);

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
        int selector = (SnesLittleEndian.HighByte(health)) & 0x78;
        if ((selector & 0x40) != 0)
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
        LastBombTorizoSoundEffect = null;
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
                torizo.CurrentInstruction = unchecked((ushort)(torizo.CurrentInstruction + 2));
                torizo.InstructionTimer = NativeCountdown.Start(1);
                state.AwakeningReleased = true;
                return;

            case TorizoFunction.Active:
                RunBombTorizoActiveState(torizo, state, level);
                return;

            case TorizoFunction.GoldenWaitForSamus:
                // $AA:D5C2 advances the list operand only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus is not null && samus.Position.Y.Whole > 0x0140 && samus.Position.X.Whole > 0x0170)
                {
                    torizo.CurrentInstruction = unchecked((ushort)(torizo.CurrentInstruction + 2));
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
        if ((torizo.Parameter2 & 0x8000) == 0 &&
            torizo.Health < BombTorizoHeadExplosionHealth)
        {
            state.InterruptedInstruction = torizo.CurrentInstruction;
            torizo.CurrentInstruction = BombTorizoLowHealthInterruptInstruction;
            torizo.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        if ((torizo.Parameter2 & 0x4000) == 0 &&
            torizo.Health < BombTorizoCoreExplosionHealth)
        {
            state.ReturnInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? TorizoInstructionLists.AlternateLandingFacingRight
                : TorizoInstructionLists.AlternateLandingFacingLeft;
            torizo.CurrentInstruction = BombTorizoLowHealthRecoveryInstruction;
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
            if (velocity is >= 0 and not 0x0100)
            {
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = NativeCountdown.Start(32);
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x0100));
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
            torizo.CurrentInstruction = state.ReturnInstruction;
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x0100));
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
        bool alternateLanding = (torizo.Parameter2 & 0x4000) != 0;
        if (!state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = state.AirTransitionTimer.Decrement().Remaining;
            if (state.AirTransitionTimer.IsZero)
            {
                torizo.CurrentInstruction = alternateLanding
                    ? ((torizo.Parameter1 & 0x8000) != 0
                        ? TorizoInstructionLists.AlternateLandingFacingRight
                        : TorizoInstructionLists.AlternateLandingFacingLeft)
                    : ((torizo.Parameter1 & 0x8000) != 0
                        ? TorizoInstructionLists.LandingFacingRight
                        : TorizoInstructionLists.LandingFacingLeft);
                torizo.InstructionTimer = NativeCountdown.Start(1);
                return;
            }
        }

        int fallingSpeed = Math.Min(Math.Abs(state.Velocity.X.SignedRawValue) + 1, 15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SnesSixteenSixteen.FromWhole(fallingSpeed)))
        {
            // $AA:C752 swaps to the authored in-air list while the downward probe remains
            // clear. Landing itself is handled by the list-installed gravity/jump callback.
            torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? TorizoInstructionLists.AirborneFacingRight
                : TorizoInstructionLists.AirborneFacingLeft;
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = new EightEightVector<RoomPixels>(0, 0x0100);
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
                torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                    ? TorizoInstructionLists.GoldenLandingFacingRight
                    : TorizoInstructionLists.GoldenLandingFacingLeft;
                torizo.InstructionTimer = NativeCountdown.Start(1);
                return;
            }
        }

        int fallingSpeed = Math.Min(
            Math.Abs(state.Velocity.X.SignedRawValue) + 1,
            15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SnesSixteenSixteen.FromWhole(fallingSpeed)))
        {
            torizo.CurrentInstruction = (torizo.Parameter1 & 0x8000) != 0
                ? TorizoInstructionLists.AirborneFacingRight
                : TorizoInstructionLists.AirborneFacingLeft;
            torizo.InstructionTimer = NativeCountdown.Start(1);
            state.Velocity = new EightEightVector<RoomPixels>(0, 0x0100);
        }
    }

    /// <summary>Uses the room RNG to emit drool only while a living Torizo is below the head-break threshold.</summary>
    private void MaybeSpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo)
    {
        if (torizo.Health is 0 or >= BombTorizoHeadExplosionHealth)
            return;
        if (_room.Random.Read() is ushort random &&
            (random & 0x8142) == 0)
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
        torizo.CurrentInstruction = BombTorizoDeathInstruction;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0xc000));
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
            xMask: state.IsGolden ? (ushort)0x00ff : (ushort)0x007f,
            yBase: state.IsGolden ? (ushort)288 : (ushort)96,
            yMask: 0x3f00);
        state.ItemDropRequested = true;
        LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
            MusicCommand.SelectTrack(3),
            MusicCommandDelay.EightFrames);
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
    internal void RegisterBombTorizoBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            c => ResolveTorizoNormalBomb(c.Enemy, c.Bomb, c.SelectedShotAiPointer));
    }

    /// <summary>Installs movement and bytecode handlers for statue fragments, orbs, attacks, and reflected missiles.</summary>
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

        table.RegisterInstruction(EnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                TorizoEnemyState state = GoldenTorizo ??
                    throw new InvalidOperationException(
                        "Golden Torizo eye-beam bytecode has no owning Torizo state.");
                cursor = (state.AttackFlags & 0x8000) == 0
                    ? EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(
                        projectile,
                        unchecked((ushort)(cursor + 2)))
                    : unchecked((ushort)(cursor + 4));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        table.RegisterInstruction(EnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                RequestTorizoChozoOrbDrop(projectile, cursor);
                cursor = unchecked((ushort)(cursor + 6));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        EnemyProjectileInstructionHandler aimSuperMissile =
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Golden Torizo super-missile aiming requires the active Samus actor.");
                }
                SetGoldenTorizoSuperMissileVelocity(
                    projectile,
                    samus,
                    awayFromSamus: word == (ushort)EnemyProjectileInstruction.AimSuperMissile_Leftwards);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            };
        table.RegisterInstruction(EnemyProjectileInstruction.AimSuperMissile_Rightwards, null, aimSuperMissile);
        table.RegisterInstruction(EnemyProjectileInstruction.AimSuperMissile_Leftwards, null, aimSuperMissile);
    }

    /// <summary>Maps both Torizo definitions to the shared native initialization routine.</summary>
    internal void RegisterBombTorizoInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Torizo, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBombTorizo(slot, samus, controllerInput);
        });
    }

    /// <summary>Registers variant-specific main callbacks while retaining the shared state dispatcher.</summary>
    internal void RegisterBombTorizoMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_BombTorizo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo, (slot, frame, nmiFrameCounter8) =>
        {
            RunBombTorizoMain(slot, RequireBombTorizoState(slot), frame.Samus, frame.Level);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_GoldenTorizo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo, (slot, frame, nmiFrameCounter8) =>
        {
            RunGoldenTorizoMain(
                slot,
                RequireBombTorizoState(slot),
                frame.Samus,
                frame.Level);
        });
    }

    /// <summary>Feeds room, Samus, input, and NMI state to the statue-owned instruction interpreter.</summary>
    internal void RegisterBombTorizoOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
            TryProcessBombTorizoInstruction(slot, new TorizoInstructionInputs { Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounterLow = unchecked((byte)_frameContext._enemyFrameNmiFrameCounter) }, word, ref cursor, out yieldInterpreter));
    }

    /// <summary>Validates extended hitbox callbacks, handles catches, and defers death to Torizo's own sequence.</summary>
    internal void RegisterBombTorizoShotReactions(EnemyAiTable table)
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
    internal void RegisterBombTorizoTouch(EnemyAiTable table)
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
    /// <summary>
    /// Dispatches Bomb Torizo's bank-$AA instruction callbacks. <paramref name="cursor"/>
    /// points at the opcode on entry and at the next opcode/frame/branch target on return.
    /// A true <paramref name="pauseInterpreter"/> reproduces callbacks that return a null
    /// instruction pointer after scheduling a multi-frame delay in the common enemy slot.
    /// <see cref="TorizoInstructionInputs.FrameCounterLow"/> is the low byte of <c>NMI_FrameCounter</c> ($05B6),
    /// which both attack choosers ($AA:C5B9, $AA:D53B) add; only its bit 3 survives.
    /// </summary>
    internal bool TryProcessBombTorizoInstruction(RoomEnemySlot torizo, TorizoInstructionInputs inputs, ushort opcode, ref ushort cursor, out bool pauseInterpreter)
    {
        pauseInterpreter = false;
        if (torizo.EnemyDefinitionPointer is not (
                EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo))
            return false;

        TorizoEnemyState state = RequireBombTorizoState(torizo);
        // A large fraction of bank-$AA callbacks have no operands. Reading the
        // following word before dispatch also reads the next list after a
        // return, and makes a cartridge-free run depend on an unrelated list.
        // Capture this address before any branch mutates the ref cursor.
        ushort operandAddress = unchecked((ushort)(cursor + 2));
        ushort operand0 = 0;
        bool operand0Read = false;
        ushort ReadOperand0()
        {
            if (!operand0Read)
            {
                operand0 = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(torizo, operandAddress);
                operand0Read = true;
            }
            return operand0;
        }

        if (!Enum.IsDefined((TorizoInstruction)opcode))
            return false;
        switch ((TorizoInstruction)opcode)
        {
            case TorizoInstruction.Instruction_CommonAA_Enemy0FB2_InY:
                state.PreInstruction = ClosedNativeWords.Decode<TorizoPreInstruction>(ReadOperand0(), "Torizo pre-instruction");
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_CommonAA3_SetEnemy0FB2ToRTS:
                state.PreInstruction = TorizoPreInstruction.Idle;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_FunctionInY:
                state.Function = ClosedNativeWords.Decode<TorizoFunction>(ReadOperand0(), "Torizo function");
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x8000));
                for (int piece = 0; piece < 6; piece++)
                    SpawnBombTorizoLowHealthDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_MarkBombTorizoFaceBlownUp:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetAsVisible:
                torizo.Properties = torizo.Properties.Without(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetAsInvisible:
                torizo.Properties = torizo.Properties.With(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToBlack:
                SetTorizoPaletteTarget(state, new Bgr555[32]);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops:
                FinishBombTorizoDeath(state);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange:
                GetTorizoPaletteTransition(state).Step(_presentation.Cgram, TorizoPaletteDefinitions.BodyPaletteMask);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Normal);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.RTL_AAC2C8:
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetAnimationLock:
                state.ShotGuard = BombTorizoShotGuardValue;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_ClearAnimationLock:
                state.ShotGuard = 0;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden:
            {
                ushort operand1 = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                if ((torizo.Parameter2 & 0x4000) != 0)
                    cursor = ReadOperand0();
                else if (state.IsGolden)
                    cursor = operand1;
                else
                    cursor = unchecked((ushort)(cursor + 6)); // Bomb variant skips both targets.
                return true;
            }

            case TorizoInstruction.Instruction_Torizo_LinkInstructionInY:
                state.ReturnInstruction = ReadOperand0();
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_Return:
                cursor = state.ReturnInstruction;
                return true;

            case TorizoInstruction.Instruction_Torizo_GotoGutExplosionLinkInstruction:
                cursor = state.InterruptedInstruction;
                return true;

            case TorizoInstruction.Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames:
                for (int explosion = 0; explosion < 6; explosion++)
                    SpawnBombTorizoLowHealthExplosion(torizo, ReadOperand0());
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 4));
                torizo.FlashTimer = NativeCountdown.Start(40);
                torizo.InstructionTimer = NativeCountdown.Start(40);
                pauseInterpreter = true;
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame:
                SpawnBombTorizoDeathExplosion(torizo);
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 2));
                torizo.FlashTimer = NativeCountdown.Start(1);
                torizo.InstructionTimer = NativeCountdown.Start(1);
                pauseInterpreter = true;
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnTorizoLandingDustClouds:
                SpawnBombTorizoLandingDust(torizo, rightFoot: true);
                SpawnBombTorizoLandingDust(torizo, rightFoot: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow:
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    SpawnBombTorizoInitialDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetTorizoTurningAroundFlag:
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithLeftFootState:
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 & 0x1fff));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetSteppedRightWithRightFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x8000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x2000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SetSteppedRightWithLeftFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0xa000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY:
                ApplyBombTorizoMapOffset(torizo, ReadOperand0(), subtract: false);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY:
                ApplyBombTorizoMapOffset(torizo, ReadOperand0(), subtract: true);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY:
                cursor = ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, RequireBombTorizoLevel(inputs.Level), cursor, new TorizoWalkStep { TableOffset = ReadOperand0(), CollisionFacingRight = 0xb962, CollisionFacingLeft = 0xbdd8 });
                return true;

            case TorizoInstruction.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY:
                cursor = ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, RequireBombTorizoLevel(inputs.Level), cursor, new TorizoWalkStep { TableOffset = ReadOperand0(), CollisionFacingRight = 0xbd0e, CollisionFacingLeft = 0xc188 });
                return true;

            case TorizoInstruction.Instruction_Torizo_GotoY_IfRising:
                cursor = state.Velocity.Y.SignedRawValue < 0
                    ? ReadOperand0()
                    : unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront:
                cursor = SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels:
                cursor = SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    inputs.Samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack:
            {
                if (inputs.Samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                ushort operand1 = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = inputs.Samus.Missiles < 5 ||
                    ((inputs.FrameCounterLow + (inputs.Samus.Position.X.Whole & 1) + (inputs.Samus.Position.X.Whole >> 1)) & 8) != 0;
                cursor = chooseFirst ? ReadOperand0() : operand1;
                return true;
            }

            case TorizoInstruction.Instruction_Torizo_SpawnBombTorizosChozoOrbs:
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY:
                SpawnBombTorizoSonicBoom(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY:
                SpawnGoldenTorizoSonicBoom(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY:
                SpawnBombTorizoExplosiveSwipe(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_Torizo_PlayShotTorizoSFX:
                LastBombTorizoSoundEffect = 0x0027;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.ShotTorizo, maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX:
                LastBombTorizoSoundEffect = 0x004b;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.TorizoFootsteps, maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_GotoY_IfNotHitGround:
                cursor = torizo.Position.Y.Whole == 375
                    ? unchecked((ushort)(cursor + 4))
                    : ReadOperand0();
                return true;

            case TorizoInstruction.Instruction_Torizo_LoadGoldenTorizoPalettes:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Golden);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                torizo.Radius = new(18, 48);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 & ~0x1000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_SpawnGoldenTorizoEgg:
                SpawnGoldenTorizoEgg(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_EyeBeamAttack_0:
                cursor = _projectiles.EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? ReadOperand0()
                    : unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_DisableEyeBeamExplosions:
                state.AttackFlags = unchecked((ushort)(state.AttackFlags & ~0x8000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_EnableEyeBeamExplosions:
                state.AttackFlags = unchecked((ushort)(state.AttackFlags | 0x8000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_UnmarkStunned:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 &
                    ~GoldenTorizoBehavioralProperties.Stunned));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_QueueEggReleasedSFX:
                LastBombTorizoSoundEffect = 0x0034;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.GoldenTorizoEggReleased, maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_QueueLaserSFX:
                LastBombTorizoSoundEffect = 0x0067;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.GoldenTorizoLaser, maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_Torizo_QueueSonicBoomSFX:
                LastBombTorizoSoundEffect = 0x0048;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.TorizoSonicBoom, maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_SpawnSuperMissile:
                SpawnGoldenTorizoSuperMissile(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo:
                cursor = SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_SpawnEyeBeam:
                SpawnGoldenTorizoEyeBeam(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstruction.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo:
                cursor = SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789:
                if (torizo.Health > 0x0788 || (_room.Random.Generate() & 0x0102) != 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.DecisionCounter = 0;
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = ReadOperand0();
                }
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31:
                if (torizo.Health <= 0x2a30 ||
                    (torizo.Parameter2 & GoldenTorizoBehavioralProperties.Stunned) == 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = ReadOperand0();
                }
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel:
                cursor = SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.ControllerInput,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_SpawnChozoOrbs:
                SpawnGoldenTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels:
                cursor = SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    inputs.Samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(inputs.Samus);
                ushort operand1 = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = activeSamus.Missiles < 0x20 ||
                    ((inputs.FrameCounterLow + (activeSamus.Position.X.Whole & 1) +
                        (activeSamus.Position.X.Whole >> 1)) & 8) != 0;
                cursor = chooseFirst ? ReadOperand0() : operand1;
                return true;
            }

            case TorizoInstruction.Instruction_GoldenTorizo_WalkingMovement_IndexInY:
                cursor = ProcessGoldenTorizoWalkInstruction(
                    torizo,
                    state,
                    inputs.Samus,
                    RequireBombTorizoLevel(inputs.Level),
                    cursor,
                    ReadOperand0());
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoInstruction)} {opcode:X4}.");
        }
    }

    /// <summary>Branches to the grounded-ball response only when Samus is four to 39 pixels away behind the statue.</summary>
    private static ushort SelectGoldenTorizoMorphBallBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        bool isGroundedBall = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose;
        if (BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 4 || distance >= 0x28 || !isGroundedBall)
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        return target;
    }

    /// <summary>Uses the room RNG to select a forward-facing attack at medium horizontal range.</summary>
    private ushort SelectGoldenTorizoMediumRangeBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (!BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 0x20 || distance >= 0x60 || (_room.Random.Generate() & 0x0110) != 0)
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.ReturnInstruction = unchecked((ushort)(cursor + 4));
        return target;
    }

    /// <summary>Launches the forward leap when distant Samus movement or sustained space jumping breaks the wait.</summary>
    private ushort SelectGoldenTorizoSpaceJumpCounter(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort controllerInput,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (distance < 0x70 || !BombTorizoFunction12IsNonNegative(torizo, samus))
            return unchecked((ushort)(cursor + 4));
        SnesButton heldButtons = SnesButtons.FromRaw(
            controllerInput,
            "Golden Torizo jump decision");
        if (state.SamusSpaceJumpFrames <= 0x0168 &&
            (!heldButtons.HasAny(SnesButtons.HorizontalDirections) ||
                (_room.Random.Generate() & 0x0101) == 0))
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return target;
    }

    /// <summary>Starts a backward jump after enough decisions or when Samus closes inside 32 pixels.</summary>
    private static ushort SelectGoldenTorizoRepeatedJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (state.DecisionCounter < 8 &&
            (distance >= 0x20 || !BombTorizoFunction12IsNonNegative(torizo, samus)))
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoBackwardJump(torizo, state);
        return target;
    }

    /// <summary>Moves by the Golden walk table, branches on wall collision, and arms a sixteen-frame air transition near Samus.</summary>
    internal ushort ProcessGoldenTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort cursor,
        ushort tableOffset)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(GoldenTorizoWalkDefinitions.Velocity(tableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = NativeCountdown.Start(0);
            return (torizo.Parameter1 & 0x8000) != 0
                ? (ushort)0xd203
                : (ushort)0xd2bf;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = NativeCountdown.Start(16);
        }
        return unchecked((ushort)(cursor + 4));
    }

    /// <summary>Sets the facing-dependent forward leap velocity and restarts its instruction timer.</summary>
    private static void StartGoldenTorizoForwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            (torizo.Parameter1 & 0x8000) != 0 ? (short)512 : (short)-512, -1472);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Sets the faster reverse leap velocity used by Golden Torizo's repeated-jump branch.</summary>
    private static void StartGoldenTorizoBackwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            (torizo.Parameter1 & 0x8000) != 0 ? (short)-768 : (short)768, -1152);
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
    internal ushort ProcessBombTorizoWalkInstruction(RoomEnemySlot torizo, TorizoEnemyState state, SamusState? samus, RoomLevelData level, ushort cursor, TorizoWalkStep step)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(BombTorizoMovementDefinitions.WalkVelocity(step.TableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = NativeCountdown.Start(0);
            return (torizo.Parameter1 & 0x8000) != 0
                ? step.CollisionFacingRight
                : step.CollisionFacingLeft;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = NativeCountdown.Start(72);
        }
        return unchecked((ushort)(cursor + 4));
    }

    /// <summary>Saves the next instruction and branches when Samus is within 56 pixels behind the statue.</summary>
    private static ushort SelectBombTorizoCloseBehindBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 0x38)
            return unchecked((ushort)(cursor + 4));
        if (BombTorizoParameterMatchesSamusDelta(torizo, samus))
            return unchecked((ushort)(cursor + 4));
        state.ReturnInstruction = unchecked((ushort)(cursor + 4));
        return target;
    }

    /// <summary>Launches the reverse jump when Samus is within 32 pixels in the selected facing half-plane.</summary>
    private static ushort SelectBombTorizoCloseFrontJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 0x20 ||
            !BombTorizoFunction12IsNonNegative(torizo, samus))
        {
            return unchecked((ushort)(cursor + 4));
        }

        // $AA:C22D launches away from the currently faced direction at 3 px/frame and
        // starts with -4.5 px/frame vertical speed, adding 40/256 px/frame each tick.
        state.Velocity = new EightEightVector<RoomPixels>(
            (torizo.Parameter1 & 0x8000) != 0 ? (short)-768 : (short)768, -1152);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = NativeCountdown.Start(1);
        return target;
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

        ushort random;
        if (kind == RoomEnemyProjectileKind.BombTorizoLowHealthDrool)
        {
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolInstructionProgramDefinitions.SelectLowHealthInitialProgram(
                    _room.Random.Generate());
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
            int yWithOffset = jitteredY + 0xfffb;
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)yWithOffset));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((random & 0x001f) + 0x30 + SnesCarry.OutOfWord(yWithOffset)))));
            // $86:A68D-$A6C3: the X offset's ADC carries out of CLC; ADC X. Every branch
            // stores a zero X velocity.
            int jitteredX = (_room.Random.Generate() & 3) + torizo.Position.X.Whole;
            int carry = SnesCarry.OutOfWord(jitteredX);
            projectile.Position = projectile.Position.WithWholeX((torizo.Parameter1 & 0x4000) != 0
                ? unchecked((ushort)jitteredX)
                : (torizo.Parameter1 & 0x8000) != 0
                    ? unchecked((ushort)(jitteredX + 8 + carry))
                    : unchecked((ushort)(jitteredX + 0xfff8 + carry)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(torizo.Position.Y.Whole - 5)));

        // The recurring low-health drool chooses a random direction ($86:A5F9-$A62C). The
        // turning branch's word index is random & $1FE; the X velocity is the sign-extended
        // sine and the Y velocity the negative cosine of that angle.
        int angle;
        if ((torizo.Parameter1 & 0x4000) != 0)
        {
            angle = (random & 0x01fe) >> 1;
        }
        else
        {
            int baseAngle = (torizo.Parameter1 & 0x8000) != 0 ? 32 : 224;
            angle = unchecked((byte)(baseAngle + (random & 0x000f) - 8));
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedSine(unchecked((byte)angle)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle))));
        projectile.Position = projectile.Position.WithWholeX((torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(torizo.Position.X.Whole + 8))
            : unchecked((ushort)(torizo.Position.X.Whole - 8)));
    }

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

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            ((torizo.Parameter1 & 0x8000) != 0 ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the facing-adjusted table offset and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, ushort parameter)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttackDefinitions.LowHealthExplosion(
                parameter,
                facingRight: (torizo.Parameter1 & 0x8000) != 0);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphicsIndex: 0);
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
                facingRight: (torizo.Parameter1 & 0x8000) != 0));
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
        if ((torizo.Parameter1 & 0x8000) != 0)
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole + 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(624));
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredRight;
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole - 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-624)));
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.FiredLeft;
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
                facingRight: (torizo.Parameter1 & 0x8000) != 0));
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

        // $86:B001 contains an authentic bug: it masks the literal opcode byte $E2,
        // not a random value, producing a fixed 66-frame timer. Preserve that behavior.
        projectile.Variable1 = (0x00e2 & 0x001f) + 64;
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
        bool facingRight = (torizo.Parameter1 & 0x8000) != 0;
        projectile.Variable0 = unchecked((ushort)(torizo.SlotIndex * 2));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (facingRight ? 30 : -30))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
        projectile.InstructionPointer =
            GoldenTorizoProjectileDefinitions.GetReflectedSuperMissileInstruction(facingRight);
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
        bool facingRight = (torizo.Parameter1 & 0x8000) != 0;
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight));

        int angle = ((_room.Random.Generate() & 0x001e) - 16 + 192 + (facingRight ? 0 : 128)) & 0x01ff;
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
        projectile.InstructionPointer = definition.InstructionList;
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
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (rightFoot ? 24 : -24))), unchecked((ushort)(torizo.Position.Y.Whole + 48))));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void RunBombTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                TorizoChozoOrbInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & 0xfff0) + 6)));
            projectile.InstructionPointer =
                TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 18))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
            projectile.Clear();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact program.</summary>
    internal static void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.InstructionPointer =
                TorizoSonicBoomInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            projectile.Variable0 = projectile.Position.X.Whole;
            projectile.Variable1 = projectile.Position.Y.Whole;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (unchecked((short)projectile.Velocity.X.RawValue) < 0 ? -16 : 16)))));
        if ((projectile.Velocity.X.RawValue & 0xf000) == 0x1000)
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
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolProgram.WallImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
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
            projectile.InstructionPointer =
                (ushort)BombTorizoDroolProgram.FloorImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
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
            if ((projectile.Velocity.Y.RawValue & 0xff80) == 0xff80)
            {
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    (projectile.Position.Y.Whole & 0xfff0) + 6)));
                projectile.InstructionPointer =
                    TorizoChozoOrbInstructionProgramDefinitions.FloorImpact;
                projectile.InstructionTimer = NativeCountdown.Start(1);
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
        projectile.Variable1 = unchecked((ushort)(projectile.Variable1 - 1));
        if (unchecked((short)projectile.Variable1) < 0)
        {
            projectile.InstructionPointer = unchecked((ushort)(projectile.InstructionPointer + 2));
            projectile.InstructionTimer = NativeCountdown.Start(1);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight((projectile.Variable0 & 0x8000) != 0
                ? (ushort)256
                : unchecked((ushort)-256)));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));
            projectile.Variable0 ^= 0x8000;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false) &&
            unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity + (xVelocity < 0 ? 32 : -32)))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.Y.RawValue))));
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
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
            ((projectile.Variable0 & 0x8000) != 0 ? 48 : -48)))));
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break program.</summary>
    internal static void RunGoldenTorizoEggFall(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.InstructionPointer = (projectile.Variable0 & 0x8000) != 0
                ? GoldenTorizoEggInstructionProgramDefinitions.BreakRight
                : GoldenTorizoEggInstructionProgramDefinitions.BreakLeft;
            projectile.InstructionTimer = NativeCountdown.Start(1);
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
            ((torizo.Parameter1 & 0x8000) != 0 ? 32 : -32))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
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
            projectile.InstructionPointer =
                GoldenTorizoSuperMissileInstructionProgramDefinitions.Impact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
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
            ? unchecked((byte)(angle | 0x80))
            : unchecked((byte)(angle & 0x7f));

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
            projectile.InstructionPointer =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.WallImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & 0xfff0) + 6)));
            projectile.InstructionPointer =
                GoldenTorizoEyeBeamInstructionProgramDefinitions.FloorImpact;
            projectile.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>
    /// Ports instruction <c>$86:AB8A</c>. Its two inline header operands are Bomb Torizo's
    /// area-zero orb and Golden Torizo's nonzero-area orb. This runtime already identifies
    /// the concrete loaded variant from definition <c>$EEFF/$EF7F</c>, which is the exact
    /// room-scoped equivalent of consulting the global area byte.
    /// </summary>
    internal void RequestTorizoChozoOrbDrop(
        RoomEnemyProjectileSlot projectile,
        ushort instructionCursor)
    {
        if (projectile.Kind is not (
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
        {
            throw new InvalidOperationException(
                $"Projectile {projectile.Kind} reached Torizo-orb drop opcode $AB8A.");
        }

        bool golden = GoldenTorizo is not null;
        ushort operand = EnemyProjectileEngine.ReadEnemyProjectileInstructionMechanicsWord(
            projectile,
            unchecked((ushort)(instructionCursor + (golden ? 4 : 2))));
        EnemyDefinitionId expectedHeader = golden
            ? EnemyDefinitionId.GoldenTorizoOrb
            : EnemyDefinitionId.BombTorizoOrb;
        if (operand != (ushort)expectedHeader)
        {
            throw new InvalidDataException(
                $"Torizo-orb drop operand selected header ${operand:X4}; expected " +
                $"${(ushort)expectedHeader:X4} for golden={golden}.");
        }
        EnemyDefinitionId header = expectedHeader;

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

        projectile.InstructionPointer = definition.InstructionList;
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
            if ((projectile.Velocity.Y.RawValue & 0xf000) == 0x1000)
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0x1000));
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
        LastBombTorizoSoundEffect = null;
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
            (enemy.Parameter2 & TorizoBehaviorBits.CaughtSuperMissile) != 0)
            enemy.Parameter2 |= TorizoBehaviorBits.Counterattack;

        if (c.HitboxShotAi == EnemyInteractionCallback.BombTorizoShot &&
            isGoldenTorizo && torizoState.ShotGuard == 0 &&
            (enemy.Parameter2 & 0x1000) == 0)
        {
            torizoState.CapturedProjectileFamily = (ushort)c.Family;
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                c.Projectile.Direction &= 0xffef;
                torizoState.Function = TorizoFunction.Idle;
                enemy.InstructionTimer = NativeCountdown.Start(1);
                enemy.CurrentInstruction = (enemy.Parameter1 & 0x8000) != 0
                    ? TorizoInstructionLists.CaughtMissileFacingRight
                    : TorizoInstructionLists.CaughtMissileFacingLeft;
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (c.Samus is not null && BombTorizoFunction12IsNonNegative(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    enemy.Parameter2 |= 0x1000;
                    torizoState.Function = TorizoFunction.Idle;
                    c.Projectile.Direction |= 0x0010;
                    enemy.InstructionTimer = NativeCountdown.Start(1);
                    enemy.CurrentInstruction = (enemy.Parameter1 & 0x2000) != 0
                        ? (enemy.Parameter1 & 0x8000) != 0
                            ? TorizoInstructionLists.CaughtSuperFacingRightLeftLegForward
                            : TorizoInstructionLists.CaughtSuperFacingLeftRightLegForward
                        : (enemy.Parameter1 & 0x8000) != 0
                            ? TorizoInstructionLists.CaughtSuperFacingRightRightLegForward
                            : TorizoInstructionLists.CaughtSuperFacingLeftLeftLegForward;
                    return true;
                }
            }
            else
            {
                // Beams and other common-damage families arm the high-health counterattack branch before entering
                // normal shot AI.
                enemy.Parameter2 |= 0x2000;
            }
        }
        return false;
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastBombTorizoMusicRequest?.Command, LastBombTorizoMusicRequest?.Delay);
    }

    /// <summary>Registers the statue-fragment bytecode sound, consuming its inline queue-limit operand.</summary>
    internal void RegisterStatueBreakingSoundInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterInstruction(EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                EnemySoundRequest sound = BombTorizoStatueInstructionProgramDefinitions.ReleaseSound(cursor);
                _audio.QueueEnemySound(sound.SoundEffect, sound.MaximumQueued);
                cursor = unchecked((ushort)(cursor + 3));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }

    /// <summary>
    /// Registers Torizo_Hurt for each variant; the common instruction interpreter still
    /// advances after the hurt callback, matching <c>$A0:8FF7</c>.
    /// </summary>
    internal void RegisterBombTorizoHurtAi(EnemyAiTable table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            (slot, _) => ApplyBombTorizoHurt(slot));
}
