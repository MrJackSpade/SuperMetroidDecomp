using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's setup and movement: walking, jumps, falls and air transitions under gravity.</summary>
[RoomLifetime]
internal sealed partial class TorizoMovement
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves body movement against room blocks and slopes.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Receives the impact quake when a falling or jumping statue lands.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The medium up-and-down foreground shake a landing statue starts.</summary>
    private static readonly EarthquakeShake LandingShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks);
    /// <summary>Bomb and Golden Torizo's body, which shows the Torizo's colouring and drips its low-health drool.</summary>
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
    /// Sets up Crateria's Bomb Torizo or Norfair's Golden Torizo as he spawns; which Torizo he is, not the room's area,
    /// chooses his values.
    /// </summary>
    /// <returns>The state the Torizo holds from its setup.</returns>
    internal TorizoEnemyState InitializeTorizo(RoomEnemyPlace torizo, TorizoVariant variant, SamusState samus, GameplayAction controllerInput)
    {
        var state = new TorizoEnemyState(variant)
        {
            // Every Torizo placement starts it standing, unstunned and whole, its face and gut not yet blown up.
            Posture = TorizoPosture.Initial,
        };


        // Each area keeps its own Torizo defeat, which both encounters read: a Torizo already beaten in this area
        // never appears.
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaTorizo))
        {
            torizo.Presence = torizo.Presence.With(EnemyPresence.Deleted);
            return state;
        }

        TorizoInitializationDefinition definition = variant.Start;
        torizo.Position = torizo.Position.WithWhole(definition.Position);
        state.BodyPlayback = TorizoAnimations.Start(definition.Animation);
        torizo.Processing = torizo.Processing.With(definition.Processing);
        torizo.CollidesByFrameHitboxes = true;
        torizo.Radius = definition.Radius;
        torizo.AnimationClock.Restart();
        torizo.Shading = state.Colours.Shading;
        torizo.Frame = definition.InitialFrame;

        state.Motion = TorizoBodyMotion.HoldingStill;
        state.Velocity = PixelVelocity<RoomPixels>.Zero;
        state.FallAtRestingSpeed();

        // Both statues show the colours they share with their own body colours over them (#1512).
        TorizoBody.ShowLook(torizo, state, variant.StatueColouring);
        // A Torizo with a secret code grants its loadout as it appears while the player holds its chord.
        if (variant.SecretCode is { } code && controllerInput == code.Chord)
            code.GrantTo(samus);
        return state;
    }

    /// <summary>Interrupts Bomb Torizo's fight at its gut and face health thresholds; otherwise its body moves by its motion.</summary>
    internal void RunBombTorizoActiveState(
        RoomEnemyOccupant occupant, RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        _body.MaybeSpawnLowHealthDrool(occupant, torizo, state);

        // Until its gut and face are blown up, which its death also does, health thresholds interrupt the current
        // authored animation with the gut's and the face's blow-ups.
        if (!state.GutBlownUp &&
            torizo.Health < TorizoDecisionDefinitions.GutBlowUpHealth)
        {
            state.InterruptedPlayback = state.BodyPlayback;
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoGutBlowingUp));
            return;
        }

        if (!state.FaceBlownUp &&
            torizo.Health < TorizoDecisionDefinitions.FaceBlowUpHealth)
        {
            // The face's blow-up ends in the faceless turn away from the way Bomb Torizo faces (#2028).
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.BombTorizoFaceBlowingUp));
            return;
        }

        MoveBody(torizo, state, level);
    }

    /// <summary>Moves Bomb or Golden Torizo's body by the motion its animation started, or holds it still (#1931).</summary>
    internal void MoveBody(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level) =>
        MotionSteps[state.Motion](torizo, state, level);

    /// <summary>How the body moves in each motion (#2553): not at all while holding still, a walk, gravity while standing, or a leap.</summary>
    [field: NonSerialized]
    private IdentityTable<TorizoBodyMotion, Action<RoomEnemyPlace, TorizoEnemyState, RoomLevelData>> MotionSteps => field ??= new(
        new Dictionary<TorizoBodyMotion, Action<RoomEnemyPlace, TorizoEnemyState, RoomLevelData>>
        {
            [TorizoBodyMotion.HoldingStill] = static (_, _, _) => { },
            [TorizoBodyMotion.Walking] = Walk,
            [TorizoBodyMotion.Standing] = ApplyTorizoGravity,
            [TorizoBodyMotion.Leaping] = Leap,
        });

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
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(state.Landing));
            state.FallAtRestingSpeed();
            _earthquake.Start(LandingShake, TorizoMotionDefinitions.LandingQuakeUpdates);
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + state.JumpAcceleration);
    }

    /// <summary>
    /// Walks on (#1931): once its wait to turn runs out the Torizo turns toward Samus, and while the ground under it has given
    /// way it starts its fall. Both statues walk alike; the turn is the turn about of the walk it now walks
    /// (<see cref="TorizoEnemyState.Walker"/>), faceless for a faceless Bomb Torizo.
    /// </summary>
    private void Walk(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomLevelData level)
    {
        if (state.TurnTowardSamus is { } turn && turn.StepRunsOut())
        {
            state.TurnTowardSamus = null;
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(state.Walker.TurnAbout(state)));
            return;
        }

        if (!_terrain.MoveEnemyVertically(level, torizo, SignedSixteenSixteen.FromWholePixels(TorizoGroundProbeFall.Pixels(state.Velocity.X))))
        {
            // The fall swaps to the in-air animation while the downward probe stays clear; its leaping motion lands it.
            TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoFacing.FacesRight(state)
                ? TorizoAnimation.FacingRightFalling
                : TorizoAnimation.FacingLeftFalling));
            // The fall starts at the resting speed, so a Torizo whose ground returns at once lands without a quake.
            state.Velocity = PixelVelocity<RoomPixels>.Zero;
            state.FallAtRestingSpeed();
        }
    }
}
