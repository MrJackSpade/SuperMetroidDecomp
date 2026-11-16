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
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, colours, animation start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoMovement(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, TorizoBody body)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _body = body;
    }

    /// <summary>
    /// Runs the shared Torizo initializer for the Crateria Bomb and Norfair
    /// Golden definitions. The concrete definition selects the values, so the room's area
    /// cannot become a second authority for actor identity.
    /// </summary>
    internal void InitializeTorizo(RoomEnemyPlace torizo, SamusState samus, GameplayAction controllerInput)
    {
        bool isGolden = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        var state = new TorizoEnemyState(isGolden)
        {
            // Every Torizo population entry starts it standing, with no behaviour latch set.
            Posture = TorizoPosture.Initial,
            Behavior = TorizoBehaviorFlags.None,
        };
        torizo.SetSpeciesState(state);

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
        torizo.Frame = definition.InitialFrame;

        state.Motion = null;
        state.Velocity = EightEightVector<RoomPixels>.Zero;
        state.FallAtRestingSpeed();

        // Init writes the shared colours' targets and then selects the encounter-specific body
        // colours. This runtime exposes the live colours rather than separate current/target
        // buffers, so install those final visible colours directly in their owners.
        // Both statues draw in the colours they share with their own body colours over them (#1512).
        if (isGolden)
        {
            TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.GoldenInitial));
            if (controllerInput == GoldenTorizoCodeDefinitions.ControllerChord)
            {
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

    /// <summary>Interrupts Bomb Torizo's fight at its gut and face health thresholds; otherwise its body moves by its motion.</summary>
    internal void RunBombTorizoActiveState(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        _body.MaybeSpawnLowHealthDrool(torizo);

        // Two of its latches are the death sequence's. Before they are set,
        // health thresholds interrupt the current authored animation with the core/head bursts.
        if (!TorizoBody.HasBehavior(torizo, TorizoBehaviorFlags.GutBlownUp) &&
            torizo.Health < TorizoDecisionDefinitions.GutBlowUpHealth)
        {
            state.InterruptedPlayback = state.BodyPlayback;
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoGutBlowingUp));
            return;
        }

        if (!TorizoBody.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp) &&
            torizo.Health < TorizoDecisionDefinitions.FaceBlowUpHealth)
        {
            // The face's blow-up ends in the faceless turn away from the way Bomb Torizo faces (#2028).
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoFaceBlowingUp));
            return;
        }

        MoveBody(torizo, state, level);
    }

    /// <summary>Moves Bomb or Golden Torizo's body by the motion its animation started, or holds it still with none (#1931).</summary>
    internal void MoveBody(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        switch (state.Motion)
        {
            case null:
                return;
            case TorizoBodyMotion.Walking:
                Walk(torizo, state, level);
                return;
            case TorizoBodyMotion.Standing:
                ApplyTorizoGravity(torizo, state, level);
                return;
            case TorizoBodyMotion.Leaping:
                Leap(torizo, state, level);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoBodyMotion)} {state.Motion}.");
        }
    }

    /// <summary>
    /// Moves the statue vertically. Meeting the ground while falling faster than it rests, it lands: a quake starts and it
    /// rests; meeting it at rest, or meeting the ceiling, changes nothing.
    /// </summary>
    internal void ApplyTorizoGravity(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.Velocity.Y.ToSixteenSixteenDelta();
        if (_terrain.MoveEnemyVertically(level, torizo, displacement))
        {
            if (!state.Velocity.Y.IsNegative && !state.FallsAtRestingSpeed)
            {
                _earthquake.Start(LandingShake, TorizoMotionDefinitions.LandingQuakeUpdates);
                state.FallAtRestingSpeed();
            }
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + TorizoMotionDefinitions.StatueGravity);
    }

    /// <summary>Advances a leap or fall across slopes; landing plays the animation it lands into and shakes the room.</summary>
    private void Leap(
        RoomEnemyPlace torizo,
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
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(state.Landing
                ?? throw new InvalidOperationException("A Torizo's leap lands it in no landing its jump or fall named.")));
            state.FallAtRestingSpeed();
            _earthquake.Start(LandingShake, TorizoMotionDefinitions.LandingQuakeUpdates);
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + state.JumpAcceleration);
    }

    /// <summary>
    /// Walks on (#1931): once the turn timer runs out the Torizo turns toward Samus, and while the ground under it has given
    /// way it starts its fall. Both statues walk alike; the turns are their definition's, and a faceless Bomb Torizo turns
    /// faceless.
    /// </summary>
    private void Walk(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        if (state.TurnTimer != 0 && --state.TurnTimer == 0)
        {
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TurnTowardSamus(torizo, state)));
            return;
        }

        if (!_terrain.MoveEnemyVertically(level, torizo, SignedSixteenSixteen.FromWholePixels(TorizoGroundProbeFall.Pixels(state.Velocity.X))))
        {
            // The fall swaps to the in-air animation while the downward probe stays clear; its leaping motion lands it.
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoFacing.FacesRight(torizo)
                ? TorizoAnimation.FacingRightFalling
                : TorizoAnimation.FacingLeftFalling));
            // The fall starts at the resting speed, so a Torizo whose ground returns at once lands without a quake.
            state.Velocity = EightEightVector<RoomPixels>.Zero;
            state.FallAtRestingSpeed();
        }
    }

    /// <summary>
    /// The animation a walking Torizo turns toward Samus with: a faceless Bomb Torizo's faceless turn, otherwise its
    /// definition's turn away from the way it faces.
    /// </summary>
    private static TorizoAnimation TurnTowardSamus(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        bool facesRight = TorizoFacing.FacesRight(torizo);
        if (TorizoBody.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp))
            return facesRight ? TorizoAnimation.FacelessTurningLeft : TorizoAnimation.FacelessTurningRight;
        TorizoInitializationDefinition definition = state.Definition;
        return facesRight ? definition.TurningLeft : definition.TurningRight;
    }

    /// <summary>Runs either Torizo's initialization AI.</summary>
    internal void RunTorizoInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeTorizo(place, surroundings.IntroducedSamus, surroundings.ControllerInput);
    }
}
