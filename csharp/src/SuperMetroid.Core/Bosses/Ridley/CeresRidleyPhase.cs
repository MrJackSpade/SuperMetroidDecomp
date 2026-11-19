using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases Ceres Ridley runs once his lift-off ends the reveal (#2227): his rise, his hover and its attacks, his fake
/// retreat after his hundredth hit (he rises, drops the Baby and swoops to catch it), his real retreat with the Baby and
/// his handoff to the station's escape (<see cref="CeresEscape"/>). The state stores the phase, and each phase runs itself on
/// his encounter (#2571), so his encounter runs its current phase with no switch over phase identities.
/// </summary>
public sealed class CeresRidleyPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the phase for one update of his encounter.</summary>
    private readonly Action<CeresRidleyBehavior, RoomEnemyPlace, CeresRidleyState, SamusState> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update of his encounter.</param>
    /// <param name="handsTheRoomToTheEscape">Whether the escape owns the room while the phase runs.</param>
    private CeresRidleyPhase(string name, Action<CeresRidleyBehavior, RoomEnemyPlace, CeresRidleyState, SamusState> run,
        bool handsTheRoomToTheEscape = false)
    {
        _name = name;
        _run = run;
        HandsTheRoomToTheEscape = handsTheRoomToTheEscape;
    }

    /// <summary>Accelerates the body upward during liftoff.</summary>
    public static CeresRidleyPhase LiftoffAccelerating { get; } = new(nameof(LiftoffAccelerating), (_, place, state, _) =>
    {
        // The rising step speeds his rise by a sixteenth of a pixel an update before the common movement pass. Ridley rises from rest
        // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
        state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y - CeresRidleyFlightDefinitions.LiftoffClimbStep);
        if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.LiftoffSlowingLineY)
        {
            state.Phase = CeresRidleyPhase.LiftoffDecelerating;
            CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(place, state);
        }
    });

    /// <summary>Decelerates liftoff into the hovering phase.</summary>
    public static CeresRidleyPhase LiftoffDecelerating { get; } = new(nameof(LiftoffDecelerating), (_, place, state, _) => CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(place, state));

    /// <summary>Runs hovering and chooses its next scripted attack.</summary>
    public static CeresRidleyPhase Hovering { get; } = new(nameof(Hovering), (ridley, place, state, samus) => ridley.Flight.TickCeresRidleyHover(place, state, samus));

    /// <summary>Moves the body to its fireball firing position.</summary>
    public static CeresRidleyPhase FireballMoveToPosition { get; } = new(nameof(FireballMoveToPosition), (ridley, place, state, _) => ridley.Flight.TickCeresRidleyFireballMove(place, state));

    /// <summary>Maintains the fireball attack before returning to hover.</summary>
    public static CeresRidleyPhase FireballShooting { get; } = new(nameof(FireballShooting), (ridley, place, state, _) => ridley.Flight.TickCeresRidleyFireballShooting(place, state));

    /// <summary>Initializes the lunge movement and phase timer.</summary>
    public static CeresRidleyPhase LungeSetup { get; } = new(nameof(LungeSetup), (_, place, state, samus) =>
    {
        RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.CeresLunge);
        state.Phase = CeresRidleyPhase.LungeMain;
        state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.LungeUpdates);
        CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
    });

    /// <summary>Advances the lunge until recovery.</summary>
    public static CeresRidleyPhase LungeMain { get; } = new(nameof(LungeMain), (_, place, state, samus) => CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus));

    /// <summary>Initializes the downward-aiming swoop.</summary>
    public static CeresRidleyPhase SwoopSetup { get; } = new(nameof(SwoopSetup), (_, place, state, _) =>
    {
        state.Phase = CeresRidleyPhase.SwoopMoveToPosition;
        state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.SwoopDiveUpdates);
        state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
        state.Tail.IdleWhipEnabled = false;
        CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
    });

    /// <summary>Steers Ridley to the swoop start position.</summary>
    public static CeresRidleyPhase SwoopMoveToPosition { get; } = new(nameof(SwoopMoveToPosition), (_, place, state, _) => CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state));

    /// <summary>Descends through the downward-aiming portion of the swoop.</summary>
    public static CeresRidleyPhase SwoopDescendingAimingDown { get; } = new(nameof(SwoopDescendingAimingDown), (_, _, state, _) =>
    {
        CeresRidleyFlight.TickCeresRidleySwoopPhase(
            state,
            turn: CeresRidleyFlightDefinitions.SwoopDiveTurn,
            targetAngle: CeresRidleyFlightDefinitions.SwoopDiveAim,
            targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
            nextPhase: CeresRidleyPhase.SwoopDescendingAimingLeft,
            nextUpdates: CeresRidleyFlightDefinitions.SwoopSweepUpdates);
    });

    /// <summary>Turns the descending swoop toward a leftward aim.</summary>
    public static CeresRidleyPhase SwoopDescendingAimingLeft { get; } = new(nameof(SwoopDescendingAimingLeft), (_, _, state, _) =>
    {
        CeresRidleyFlight.TickCeresRidleySwoopPhase(
            state,
            turn: CeresRidleyFlightDefinitions.SwoopSweepTurn,
            targetAngle: CeresRidleyFlightDefinitions.SwoopSweepAim,
            targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
            nextPhase: CeresRidleyPhase.SwoopAscendingAimingUp,
            nextUpdates: CeresRidleyFlightDefinitions.SwoopClimbUpdates);
        if (state.Phase == CeresRidleyPhase.SwoopAscendingAimingUp)
        {
            // At the bottom of the swoop his tail flings once, aimed: the neutral tail takes the order later
            // this update.
            state.Tail.Whip.Give();
        }
    });

    /// <summary>Starts the upward-aiming ascent of the swoop.</summary>
    public static CeresRidleyPhase SwoopAscendingAimingUp { get; } = new(nameof(SwoopAscendingAimingUp), (_, _, state, _) =>
    {
        CeresRidleyFlight.TickCeresRidleySwoopPhase(
            state,
            turn: CeresRidleyFlightDefinitions.SwoopClimbTurn,
            targetAngle: RotationAngle.Zero - RidleyFlight.SwoopClimbTarget,
            targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
            nextPhase: CeresRidleyPhase.SwoopAscendingAimingUpFaster,
            nextUpdates: CeresRidleyFlightDefinitions.SwoopFinalClimbUpdates);
    });

    /// <summary>Continues the faster upward-aiming swoop ascent.</summary>
    public static CeresRidleyPhase SwoopAscendingAimingUpFaster { get; } = new(nameof(SwoopAscendingAimingUpFaster), (_, _, state, _) =>
    {
        RidleyFlight.UpdateSwoopVelocity(state.Flight, turn: CeresRidleyFlightDefinitions.SwoopFinalClimbTurn, targetAngle: RotationAngle.Zero - RidleyFlight.SwoopClimbTarget,
            targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed);
        if (state.PhaseTimer.StepRunsOut())
        {
            CeresRidleyFlight.BeginHover(state);
            state.Tail.IdleWhipEnabled = true;
        }
    });

    /// <summary>Steers Ridley to the start of his fake retreat after his hundredth hit.</summary>
    public static CeresRidleyPhase FakeRetreatMoveToPosition { get; } = new(nameof(FakeRetreatMoveToPosition), (_, place, state, _) => CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(place, state));

    /// <summary>Raises Ridley until he lets the Baby fall.</summary>
    public static CeresRidleyPhase FakeRetreatRising { get; } = new(nameof(FakeRetreatRising), (_, place, state, _) => CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(place, state));

    /// <summary>Starts the Baby's fall and waits before the retrieval animation.</summary>
    public static CeresRidleyPhase WaitBeforeRetrievingBaby { get; } = new(nameof(WaitBeforeRetrievingBaby), (_, place, state, _) =>
    {
        if (state.PhaseTimer.StepRunsOut())
        {
            RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.RetrieveBabyMetroid);
            state.Phase = CeresRidleyPhase.RetrieveBaby;
            CeresRidleyFlight.TickCeresRidleyRetrieveBaby(place, state);
        }
    });

    /// <summary>Steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    public static CeresRidleyPhase RetrieveBaby { get; } = new(nameof(RetrieveBaby), (_, place, state, _) => CeresRidleyFlight.TickCeresRidleyRetrieveBaby(place, state));

    /// <summary>Raises Ridley beyond the room for the real getaway.</summary>
    public static CeresRidleyPhase RealRetreatRising { get; } = new(nameof(RealRetreatRising), (ridley, place, state, _) => ridley.Flight.TickCeresRidleyRetreat(place, state));

    /// <summary>Prepares retreat colours and walls while waiting to publish the getaway.</summary>
    public static CeresRidleyPhase RetreatDelay { get; } = new(nameof(RetreatDelay), (ridley, _, state, _) => ridley.Flight.TickCeresRidleyRetreatDelay(state));

    /// <summary>Hides Ridley and begins the station's escape with his turning-floor getaway.</summary>
    public static CeresRidleyPhase PublishEscapeHandoff { get; } = new(nameof(PublishEscapeHandoff), (ridley, place, state, _) =>
    {
        // The getaway handoff leaves Ridley with nothing to do and publishes his escape. The turning floor's
        // view shows his departure, so his body is hidden.
        state.Phase = CeresRidleyPhase.Escaped;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        ridley.Escape.BeginGetaway();
    });

    /// <summary>
    /// He has got away (#2228): the station's escape owns the room, and his AI runs the escape's step in his place in the
    /// update (nothing during the getaway, then the self-destruct warning and its alarm).
    /// </summary>
    public static CeresRidleyPhase Escaped { get; } = new(nameof(Escaped), (ridley, place, _, _) =>
    {
        // The escape runs its own step here, in his place in the update: nothing while the room main flies the
        // getaway, then the self-destruct warning and its alarm.
        ridley.Escape.RunRidleysStep(place.FrameCounter);
    }, handsTheRoomToTheEscape: true);

    /// <summary>
    /// Whether the station's escape owns the room while the phase runs, so neither his movement nor the Baby follows it.
    /// </summary>
    internal bool HandsTheRoomToTheEscape { get; }

    /// <summary>Runs the phase for one update of Ceres Ridley's encounter.</summary>
    /// <param name="ridley">His encounter's behaviour, whose flight and escape the phases run.</param>
    /// <param name="place">His body.</param>
    /// <param name="state">His encounter.</param>
    /// <param name="samus">Samus, whom his attacks aim at.</param>
    internal void Run(CeresRidleyBehavior ridley, RoomEnemyPlace place, CeresRidleyState state, SamusState samus) =>
        _run(ridley, place, state, samus);

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
