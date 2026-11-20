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

    /// <summary>Runs the phase for one update of his encounter, on the turn's behaviour, body, encounter and Samus.</summary>
    private readonly Action<CeresRidleyPhaseTurn> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update of his encounter.</param>
    private CeresRidleyPhase(string name, Action<CeresRidleyPhaseTurn> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>Accelerates the body upward during liftoff.</summary>
    public static CeresRidleyPhase LiftoffAccelerating { get; } = new(nameof(LiftoffAccelerating), turn => RunLiftoffAccelerating(turn.Place, turn.State));

    /// <summary>Decelerates liftoff into the hovering phase.</summary>
    public static CeresRidleyPhase LiftoffDecelerating { get; } = new(nameof(LiftoffDecelerating), turn => CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(turn.Place, turn.State));

    /// <summary>Runs hovering and chooses its next scripted attack.</summary>
    public static CeresRidleyPhase Hovering { get; } = new(nameof(Hovering), turn => turn.Ridley.Flight.TickCeresRidleyHover(turn.Place, turn.State, turn.Samus));

    /// <summary>Moves the body to its fireball firing position.</summary>
    public static CeresRidleyPhase FireballMoveToPosition { get; } = new(nameof(FireballMoveToPosition), turn => turn.Ridley.Flight.TickCeresRidleyFireballMove(turn.Place, turn.State));

    /// <summary>Maintains the fireball attack before returning to hover.</summary>
    public static CeresRidleyPhase FireballShooting { get; } = new(nameof(FireballShooting), turn => turn.Ridley.Flight.TickCeresRidleyFireballShooting(turn.Place, turn.State));

    /// <summary>Initializes the lunge movement and phase timer.</summary>
    public static CeresRidleyPhase LungeSetup { get; } = new(nameof(LungeSetup), turn => RunLungeSetup(turn.Place, turn.State, turn.Samus));

    /// <summary>Advances the lunge until recovery.</summary>
    public static CeresRidleyPhase LungeMain { get; } = new(nameof(LungeMain), turn => CeresRidleyFlight.TickCeresRidleyLunge(turn.Place, turn.State, turn.Samus));

    /// <summary>Initializes the downward-aiming swoop.</summary>
    public static CeresRidleyPhase SwoopSetup { get; } = new(nameof(SwoopSetup), turn => RunSwoopSetup(turn.Place, turn.State));

    /// <summary>Steers Ridley to the swoop start position.</summary>
    public static CeresRidleyPhase SwoopMoveToPosition { get; } = new(nameof(SwoopMoveToPosition), turn => CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(turn.Place, turn.State));

    /// <summary>Descends through the downward-aiming portion of the swoop.</summary>
    public static CeresRidleyPhase SwoopDescendingAimingDown { get; } = new(nameof(SwoopDescendingAimingDown), turn => RunSwoopDescendingAimingDown(turn.State));

    /// <summary>Turns the descending swoop toward a leftward aim.</summary>
    public static CeresRidleyPhase SwoopDescendingAimingLeft { get; } = new(nameof(SwoopDescendingAimingLeft), turn => RunSwoopDescendingAimingLeft(turn.State));

    /// <summary>Starts the upward-aiming ascent of the swoop.</summary>
    public static CeresRidleyPhase SwoopAscendingAimingUp { get; } = new(nameof(SwoopAscendingAimingUp), turn => RunSwoopAscendingAimingUp(turn.State));

    /// <summary>Continues the faster upward-aiming swoop ascent.</summary>
    public static CeresRidleyPhase SwoopAscendingAimingUpFaster { get; } = new(nameof(SwoopAscendingAimingUpFaster), turn => RunSwoopAscendingAimingUpFaster(turn.State));

    /// <summary>Steers Ridley to the start of his fake retreat after his hundredth hit.</summary>
    public static CeresRidleyPhase FakeRetreatMoveToPosition { get; } = new(nameof(FakeRetreatMoveToPosition), turn => CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(turn.Place, turn.State));

    /// <summary>Raises Ridley until he lets the Baby fall.</summary>
    public static CeresRidleyPhase FakeRetreatRising { get; } = new(nameof(FakeRetreatRising), turn => CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(turn.Place, turn.State));

    /// <summary>Starts the Baby's fall and waits before the retrieval animation.</summary>
    public static CeresRidleyPhase WaitBeforeRetrievingBaby { get; } = new(nameof(WaitBeforeRetrievingBaby), turn => RunWaitBeforeRetrievingBaby(turn.Place, turn.State));

    /// <summary>Steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    public static CeresRidleyPhase RetrieveBaby { get; } = new(nameof(RetrieveBaby), turn => CeresRidleyFlight.TickCeresRidleyRetrieveBaby(turn.Place, turn.State));

    /// <summary>Raises Ridley beyond the room for the real getaway.</summary>
    public static CeresRidleyPhase RealRetreatRising { get; } = new(nameof(RealRetreatRising), turn => turn.Ridley.Flight.TickCeresRidleyRetreat(turn.Place, turn.State));

    /// <summary>Prepares retreat colours and walls while waiting to publish the getaway.</summary>
    public static CeresRidleyPhase RetreatDelay { get; } = new(nameof(RetreatDelay), turn => turn.Ridley.Flight.TickCeresRidleyRetreatDelay(turn.State));

    /// <summary>Hides Ridley and begins the station's escape with his turning-floor getaway.</summary>
    public static CeresRidleyPhase PublishEscapeHandoff { get; } = new(nameof(PublishEscapeHandoff), turn => RunPublishEscapeHandoff(turn.Ridley, turn.Place, turn.State));

    /// <summary>
    /// He has got away (#2228): the station's escape owns the room, and his AI runs the escape's step in his place in the
    /// update (nothing during the getaway, then the self-destruct warning and its alarm).
    /// </summary>
    public static CeresRidleyPhase Escaped { get; } = new(nameof(Escaped), turn =>
    {
        // The escape runs its own step here, in his place in the update: nothing while the room main flies the
        // getaway, then the self-destruct warning and its alarm.
        turn.Ridley.Escape.RunRidleysStep(turn.Place.FrameCounter);
    });

    /// <summary>
    /// Whether the station's escape owns the room while the phase runs, so neither his movement nor the Baby follows it.
    /// </summary>
    internal bool HandsTheRoomToTheEscape => ReferenceEquals(this, Escaped);

    /// <summary>Runs the phase for one update of Ceres Ridley's encounter.</summary>
    /// <param name="ridley">His encounter's behaviour, whose flight and escape the phases run.</param>
    /// <param name="place">His body.</param>
    /// <param name="state">His encounter.</param>
    /// <param name="samus">Samus, whom his attacks aim at.</param>
    internal void Run(CeresRidleyBehavior ridley, RoomEnemyPlace place, CeresRidleyState state, SamusState samus) =>
        _run(new(ridley, place, state, samus));

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;

    /// <summary>Runs one update of <see cref="LiftoffAccelerating"/>.</summary>
    private static void RunLiftoffAccelerating(RoomEnemyPlace place, CeresRidleyState state)
    {
    // The rising step speeds his rise by a sixteenth of a pixel an update before the common movement pass. Ridley rises from rest
    // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
    state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y - CeresRidleyFlightDefinitions.LiftoffClimbStep);
    if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.LiftoffSlowingLineY)
    {
        state.Phase = CeresRidleyPhase.LiftoffDecelerating;
        CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(place, state);
    }
    }

    /// <summary>Runs one update of <see cref="LungeSetup"/>.</summary>
    private static void RunLungeSetup(RoomEnemyPlace place, CeresRidleyState state, SamusState samus)
    {
    RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.CeresLunge);
    state.Phase = CeresRidleyPhase.LungeMain;
    state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.LungeUpdates);
    CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
    }

    /// <summary>Runs one update of <see cref="SwoopSetup"/>.</summary>
    private static void RunSwoopSetup(RoomEnemyPlace place, CeresRidleyState state)
    {
    state.Phase = CeresRidleyPhase.SwoopMoveToPosition;
    state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.SwoopDiveUpdates);
    state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
    state.Tail.IdleWhipEnabled = false;
    CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
    }

    /// <summary>Runs one update of <see cref="SwoopDescendingAimingDown"/>.</summary>
    private static void RunSwoopDescendingAimingDown(CeresRidleyState state)
    {
    CeresRidleyFlight.TickCeresRidleySwoopPhase(
        state,
        turn: CeresRidleyFlightDefinitions.SwoopDiveTurn,
        targetAngle: CeresRidleyFlightDefinitions.SwoopDiveAim,
        targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
        nextPhase: CeresRidleyPhase.SwoopDescendingAimingLeft,
        nextUpdates: CeresRidleyFlightDefinitions.SwoopSweepUpdates);
    }

    /// <summary>Runs one update of <see cref="SwoopDescendingAimingLeft"/>.</summary>
    private static void RunSwoopDescendingAimingLeft(CeresRidleyState state)
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
    }

    /// <summary>Runs one update of <see cref="SwoopAscendingAimingUp"/>.</summary>
    private static void RunSwoopAscendingAimingUp(CeresRidleyState state)
    {
    CeresRidleyFlight.TickCeresRidleySwoopPhase(
        state,
        turn: CeresRidleyFlightDefinitions.SwoopClimbTurn,
        targetAngle: RotationAngle.Zero - RidleyFlightDefinitions.SwoopClimbTarget,
        targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
        nextPhase: CeresRidleyPhase.SwoopAscendingAimingUpFaster,
        nextUpdates: CeresRidleyFlightDefinitions.SwoopFinalClimbUpdates);
    }

    /// <summary>Runs one update of <see cref="SwoopAscendingAimingUpFaster"/>.</summary>
    private static void RunSwoopAscendingAimingUpFaster(CeresRidleyState state)
    {
    RidleyFlight.UpdateSwoopVelocity(state.Flight, turn: CeresRidleyFlightDefinitions.SwoopFinalClimbTurn, targetAngle: RotationAngle.Zero - RidleyFlightDefinitions.SwoopClimbTarget,
        targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed);
    if (state.PhaseTimer.StepRunsOut())
    {
        CeresRidleyFlight.BeginHover(state);
        state.Tail.IdleWhipEnabled = true;
    }
    }

    /// <summary>Runs one update of <see cref="WaitBeforeRetrievingBaby"/>.</summary>
    private static void RunWaitBeforeRetrievingBaby(RoomEnemyPlace place, CeresRidleyState state)
    {
    if (state.PhaseTimer.StepRunsOut())
    {
        RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.RetrieveBabyMetroid);
        state.Phase = CeresRidleyPhase.RetrieveBaby;
        CeresRidleyFlight.TickCeresRidleyRetrieveBaby(place, state);
    }
    }

    /// <summary>Runs one update of <see cref="PublishEscapeHandoff"/>.</summary>
    private static void RunPublishEscapeHandoff(CeresRidleyBehavior ridley, RoomEnemyPlace place, CeresRidleyState state)
    {
    // The getaway handoff leaves Ridley with nothing to do and publishes his escape. The turning floor's
    // view shows his departure, so his body is hidden.
    state.Phase = CeresRidleyPhase.Escaped;
    place.Presence = place.Presence.With(EnemyPresence.Hidden);
    ridley.Escape.BeginGetaway();
    }
}
