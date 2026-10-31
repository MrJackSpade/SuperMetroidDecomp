using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's setup and movement: walking, jumps, falls and air transitions under gravity.</summary>
[RoomLifetime]
internal sealed class TorizoMovement
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves body movement against room blocks and slopes.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Receives the impact quake when a falling or jumping statue lands.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Exposes the two live CGRAM rows occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, program start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoMovement(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemyPresentation presentation, TorizoBody body, TorizoEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _presentation = presentation;
        _body = body;
        _access = access;
    }

    /// <summary>
    /// Ports shared <c>Torizo_Init</c> at $AA:C87F for the area-zero Bomb and area-two
    /// Golden definitions. The cartridge indexes parallel two-word tables by area; using
    /// the concrete definition here is equivalent and prevents an unrelated host area enum
    /// from becoming a second authority for actor identity.
    /// </summary>
    internal void InitializeBombTorizo(RoomEnemySlot torizo, SamusState? samus, ControllerButton controllerInput)
    {
        bool isGolden = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        var state = new TorizoEnemyState(torizo, isGolden)
        {
            // Every Torizo population entry starts it standing, with no behaviour latch set.
            Posture = TorizoPostureFlags.None,
            PostureParameterLowBits = 0,
            Behavior = 0,
        };
        _access.TorizoState = state;

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
        torizo.Properties = torizo.Properties.With(definition.PropertyMask);
        torizo.ExtraProperties = torizo.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        torizo.Radius = definition.Radius;
        torizo.InstructionTimer = WrappingCountdown.Start(1);
        torizo.Timer = WrappingCountdown.Start(0);
        torizo.Colouring = SharedSpriteColouring.Common;
        torizo.Frame = EnemyFrameId.BombTorizoDormant;

        state.Function = TorizoFunction.Falling;
        state.PreInstruction = TorizoPreInstruction.Idle;
        state.Velocity = new EightEightVector<RoomPixels>(0, 256);

        // Init writes the shared colours' targets and then selects the encounter-specific body
        // colours. This runtime exposes the live colours rather than separate current/target
        // buffers, so install those final visible colours directly in their owners.
        LoadTorizoSharedColours();
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

    /// <summary>Loads the orb, flame and statue-shading colours both statue variants share.</summary>
    private void LoadTorizoSharedColours() =>
        _presentation.BossColours().TorizoShared.CopyFrom(TorizoPaletteDefinitions.Shared);

    /// <summary>Installs Bomb Torizo's initial body and belly colours.</summary>
    private void LoadBombTorizoPalette() =>
        _body.LoadTorizoBodyColours(TorizoPaletteDefinitions.BombInitial);

    /// <summary>
    /// Installs Golden Torizo's initial body colors before hurt frames begin using the
    /// health-indexed bank-$84 gradient.
    /// </summary>
    private void LoadGoldenTorizoBasePalette() =>
        _body.LoadTorizoBodyColours(TorizoPaletteDefinitions.GoldenInitial);

    /// <summary>Interrupts active AI at the head and core health thresholds before normal pre-instructions resume.</summary>
    internal void RunBombTorizoActiveState(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        _body.MaybeSpawnBombTorizoLowHealthDrool(torizo);

        // The two high parameter-two bits are death-sequence latches. Before they are set,
        // health thresholds interrupt the current authored list with the core/head bursts.
        if (!_body.HasBehavior(torizo, TorizoBehaviorFlags.GutBlownUp) &&
            torizo.Health < BombTorizoBehavior.BombTorizoHeadExplosionHealth)
        {
            state.InterruptedCursor = state.BodyProgram;
            TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.SpecialCallableBlowUpBombTorizosGut));
            return;
        }

        if (!_body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp) &&
            torizo.Health < BombTorizoBehavior.BombTorizoCoreExplosionHealth)
        {
            state.ReturnCursor = AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                ? TorizoBodyProgram.FacingLeftFacelessTurningLeft
                : TorizoBodyProgram.FacingRightFacelessTurningRight);
            TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.CallableBlowUpBombTorizosFace));
            return;
        }

        RunBombTorizoPreInstruction(torizo, state, level);
    }

    /// <summary>Dispatches the movement callback installed by the current Bomb or Golden instruction list.</summary>
    internal void RunBombTorizoPreInstruction(
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
                    $"Bomb Torizo pre-instruction {state.PreInstruction} is not translated.");
        }
    }

    /// <summary>Moves the statue vertically and starts a landing quake when downward motion hits terrain.</summary>
    internal void ApplyBombTorizoGravity(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        int displacement = SixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue);
        if (_terrain.MoveEnemyVertically(level, torizo, displacement))
        {
            int velocity = state.Velocity.Y.SignedRawValue;
            if (velocity is >= 0 and not 256)
            {
                _earthquake.EarthquakeType = 4;
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
                state.Velocity = state.Velocity.WithY(new SignedEightEight(256));
            }
            return;
        }

        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + 40))));
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
            SixteenSixteen.FromEightEight(state.Velocity.X.SignedRawValue));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);

        if (_terrain.MoveEnemyVertically(
                level,
                torizo,
                SixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue)))
        {
            TorizoBody.StartTorizoBodyProgram(torizo, state, state.ReturnCursor);
            state.Velocity = state.Velocity.WithY(new SignedEightEight(256));
            _earthquake.EarthquakeType = 4;
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
            return;
        }

        state.Velocity += state.JumpAcceleration;
    }

    /// <summary>Waits for Bomb Torizo's landing timer or selects the airborne list while a downward probe is clear.</summary>
    private void RunBombTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        bool alternateLanding = _body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
        if (!state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = state.AirTransitionTimer.Decrement().Remaining;
            if (state.AirTransitionTimer.IsZero)
            {
                TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(alternateLanding
                    ? (_access.FacesRight(torizo)
                        ? TorizoBodyProgram.FacingLeftFacelessTurningLeft
                        : TorizoBodyProgram.FacingRightFacelessTurningRight)
                    : (_access.FacesRight(torizo)
                        ? TorizoBodyProgram.FacingLeftTurningLeft
                        : TorizoBodyProgram.FacingRightTurningRight)));
                return;
            }
        }

        int fallingSpeed = Math.Min(Math.Abs(state.Velocity.X.SignedRawValue) + 1, 15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SixteenSixteen.FromWhole(fallingSpeed)))
        {
            // $AA:C752 swaps to the authored in-air list while the downward probe remains
            // clear. Landing itself is handled by the list-installed gravity/jump callback.
            TorizoBody.StartTorizoBodyProgram(torizo, _access.RequireBombTorizoState(torizo), AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                ? TorizoBodyProgram.FacingRightFalling0
                : TorizoBodyProgram.FacingLeftFalling0));
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
                TorizoBody.StartTorizoBodyProgram(torizo, _access.RequireBombTorizoState(torizo), AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                    ? TorizoBodyProgram.GoldenTurningLeft
                    : TorizoBodyProgram.GoldenTurningRight));
                return;
            }
        }

        int fallingSpeed = Math.Min(
            Math.Abs(state.Velocity.X.SignedRawValue) + 1,
            15);
        if (!_terrain.MoveEnemyVertically(level, torizo, SixteenSixteen.FromWhole(fallingSpeed)))
        {
            TorizoBody.StartTorizoBodyProgram(torizo, _access.RequireBombTorizoState(torizo), AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                ? TorizoBodyProgram.FacingRightFalling0
                : TorizoBodyProgram.FacingLeftFalling0));
            state.Velocity = new EightEightVector<RoomPixels>(0, 256);
        }
    }

    /// <summary>Requires a collision layer for movement callbacks that cannot run outside a loaded room.</summary>
    internal static RoomLevelData RequireBombTorizoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Bomb Torizo movement requires the active room collision layer.");

    /// <summary>Ports BombTorizo's initialization AI.</summary>
    internal void RunBombTorizoInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBombTorizo(slot, surroundings.Samus, surroundings.ControllerInput);
    }
}
