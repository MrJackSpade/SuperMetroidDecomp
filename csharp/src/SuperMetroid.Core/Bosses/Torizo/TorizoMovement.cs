using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;
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
    /// <summary>The medium up-and-down foreground shake a landing statue starts.</summary>
    private static readonly EarthquakeShake LandingShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.Foreground);
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, animation start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the phase their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoMovement(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, TorizoBody body, TorizoEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _body = body;
        _access = access;
    }

    /// <summary>
    /// Runs the shared Torizo initializer for the Crateria Bomb and Norfair
    /// Golden definitions. The concrete definition selects the values, so the room's area
    /// cannot become a second authority for actor identity.
    /// </summary>
    internal void InitializeBombTorizo(RoomEnemySlot torizo, SamusState? samus, ControllerButton controllerInput)
    {
        bool isGolden = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        var state = new TorizoEnemyState(torizo, isGolden)
        {
            // Every Torizo population entry starts it standing, with no behaviour latch set.
            Posture = TorizoPostureFlags.None,
            Behavior = 0,
        };
        _access.TorizoState = state;

        // The area's Torizo defeat is shared by the Bomb and Golden Torizo encounters, but it is stored
        // per area. The room loader supplies the current area's accessor, keeping the actor
        // independent from save-memory layout and avoiding a room-number special case.
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaTorizo))
        {
            torizo.Presence = torizo.Presence.With(EnemyPresence.Deleted);
            return;
        }

        TorizoInitializationDefinition definition = isGolden
            ? TorizoInitializationDefinitions.Golden : TorizoInitializationDefinitions.Bomb;
        torizo.Position = torizo.Position.WithWhole(definition.Position);
        state.BodyPlayback = TorizoAnimations.Start(definition.Animation);
        torizo.Processing = torizo.Processing.With(definition.Processing);
        torizo.CollidesByFrameHitboxes = true;
        torizo.Radius = definition.Radius;
        torizo.AnimationClock.Restart();
        torizo.Shading = state.Colours.Shading;
        torizo.Frame = EnemyFrameId.BombTorizoDormant;

        state.Phase = TorizoPhase.Falling;
        state.FrameBehavior = TorizoFrameBehavior.Idle;
        state.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256);

        // Init writes the shared colours' targets and then selects the encounter-specific body
        // colours. This runtime exposes the live colours rather than separate current/target
        // buffers, so install those final visible colours directly in their owners.
        // Both statues draw in the colours they share with their own body colours over them (#1512).
        if (isGolden)
        {
            TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.GoldenInitial));
            if (controllerInput == GoldenTorizoCodeDefinitions.ControllerChord)
            {
                if (samus is null)
                    throw new InvalidOperationException("The Golden Torizo code requires the live Samus owner.");
                // Initialization overwrites both the collected and equipped items, even if
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
            TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.BombStatue));
    }

    /// <summary>Interrupts active AI at the head and core health thresholds before normal frame behaviours resume.</summary>
    internal void RunBombTorizoActiveState(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        _body.MaybeSpawnBombTorizoLowHealthDrool(torizo);

        // Two of its latches are the death sequence's. Before they are set,
        // health thresholds interrupt the current authored animation with the core/head bursts.
        if (!_body.HasBehavior(torizo, TorizoBehaviorFlags.GutBlownUp) &&
            torizo.Health < BombTorizoBehavior.BombTorizoHeadExplosionHealth)
        {
            state.InterruptedPlayback = state.BodyPlayback;
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoBodySegment.SpecialCallableBlowUpBombTorizosGut));
            return;
        }

        if (!_body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp) &&
            torizo.Health < BombTorizoBehavior.BombTorizoCoreExplosionHealth)
        {
            state.ReturnPlayback = TorizoAnimations.Start(_access.FacesRight(torizo)
                ? TorizoBodySegment.FacingLeftFacelessTurningLeft
                : TorizoBodySegment.FacingRightFacelessTurningRight);
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoBodySegment.CallableBlowUpBombTorizosFace));
            return;
        }

        RunBombTorizoFrameBehavior(torizo, state, level);
    }

    /// <summary>Dispatches the movement behaviour installed by the current Bomb or Golden animation.</summary>
    internal void RunBombTorizoFrameBehavior(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData? level)
    {
        switch (state.FrameBehavior)
        {
            case TorizoFrameBehavior.Idle:
                return;
            case TorizoFrameBehavior.AirTransition:
                RunBombTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoFrameBehavior.Gravity:
                ApplyBombTorizoGravity(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoFrameBehavior.Jump:
                RunBombTorizoJump(torizo, state, RequireBombTorizoLevel(level));
                return;
            case TorizoFrameBehavior.GoldenAirTransition:
                RunGoldenTorizoAirTransition(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            case TorizoFrameBehavior.GoldenGravity:
                ApplyBombTorizoGravity(
                    torizo,
                    state,
                    RequireBombTorizoLevel(level));
                return;
            default:
                throw new InvalidDataException(
                    $"Bomb Torizo has no frame behaviour {state.FrameBehavior}.");
        }
    }

    /// <summary>Moves the statue vertically and starts a landing quake when downward motion hits terrain.</summary>
    internal void ApplyBombTorizoGravity(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.Velocity.Y.ToSixteenSixteenDelta();
        if (_terrain.MoveEnemyVertically(level, torizo, displacement))
        {
            int velocity = state.Velocity.Y.InTwoHundredFiftySixthsOfPixel;
            if (velocity is >= 0 and not 256)
            {
                _earthquake.Start(LandingShake, 32);
                state.Velocity = state.Velocity.WithY(SignedEightEight.FromPixels(1));
            }
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + TorizoMotionDefinitions.StatueGravity);
    }

    /// <summary>Advances a jump across slopes; landing restores the saved animation and triggers the impact quake.</summary>
    private void RunBombTorizoJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            torizo,
            state.Velocity.X.ToSixteenSixteenDelta());
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);

        if (_terrain.MoveEnemyVertically(
                level,
                torizo,
                state.Velocity.Y.ToSixteenSixteenDelta()))
        {
            TorizoBody.StartTorizoBodyAnimation(torizo, state, state.ReturnPlayback);
            state.Velocity = state.Velocity.WithY(SignedEightEight.FromPixels(1));
            _earthquake.Start(LandingShake, 32);
            return;
        }

        state.Velocity += state.JumpAcceleration;
    }

    /// <summary>Waits for Bomb Torizo's landing timer or selects the airborne animation while a downward probe is clear.</summary>
    private void RunBombTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        bool alternateLanding = _body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
        if (state.AirTransitionTimer != 0)
        {
            if (--state.AirTransitionTimer == 0)
            {
                TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(alternateLanding
                    ? (_access.FacesRight(torizo)
                        ? TorizoBodySegment.FacingLeftFacelessTurningLeft
                        : TorizoBodySegment.FacingRightFacelessTurningRight)
                    : (_access.FacesRight(torizo)
                        ? TorizoBodySegment.FacingLeftTurningLeft
                        : TorizoBodySegment.FacingRightTurningRight)));
                return;
            }
        }

        if (!_terrain.MoveEnemyVertically(level, torizo, SignedSixteenSixteen.FromWholePixels(TorizoGroundProbeFall.Pixels(state.Velocity.X))))
        {
            // Bomb Torizo's fall swaps to the authored in-air animation while the downward probe remains
            // clear. Landing itself is handled by the animation-installed gravity/jump behaviour.
            TorizoBody.StartTorizoBodyAnimation(torizo, _access.RequireBombTorizoState(torizo), TorizoAnimations.Start(_access.FacesRight(torizo)
                ? TorizoBodySegment.FacingRightFalling0
                : TorizoBodySegment.FacingLeftFalling0));
            state.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256);
        }
    }

    /// <summary>Uses Golden Torizo's timed facing transition and landing animations during an airborne probe.</summary>
    private void RunGoldenTorizoAirTransition(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        // Golden Torizo's fall is the Golden analogue of Bomb Torizo's. Its only behavioral difference is the
        // sixteen-frame facing transition and Golden landing animations selected when it expires.
        if (state.AirTransitionTimer != 0)
        {
            if (--state.AirTransitionTimer == 0)
            {
                TorizoBody.StartTorizoBodyAnimation(torizo, _access.RequireBombTorizoState(torizo), TorizoAnimations.Start(_access.FacesRight(torizo)
                    ? TorizoBodySegment.GoldenTurningLeft
                    : TorizoBodySegment.GoldenTurningRight));
                return;
            }
        }

        if (!_terrain.MoveEnemyVertically(level, torizo, SignedSixteenSixteen.FromWholePixels(TorizoGroundProbeFall.Pixels(state.Velocity.X))))
        {
            TorizoBody.StartTorizoBodyAnimation(torizo, _access.RequireBombTorizoState(torizo), TorizoAnimations.Start(_access.FacesRight(torizo)
                ? TorizoBodySegment.FacingRightFalling0
                : TorizoBodySegment.FacingLeftFalling0));
            state.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256);
        }
    }

    /// <summary>Requires a collision layer for movement behaviours that cannot run outside a loaded room.</summary>
    internal static RoomLevelData RequireBombTorizoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Bomb Torizo movement requires the active room collision layer.");

    /// <summary>Runs BombTorizo's initialization AI.</summary>
    internal void RunBombTorizoInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBombTorizo(slot, surroundings.Samus, surroundings.ControllerInput);
    }
}
