using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby, and the movement integration they share.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFlight
{
    /// <summary>Sets up the enemies Ridley's flight spawns and runs their initialization.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues Ceres Ridley's starting music.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Finds the free place the turning-floor wall enemy takes when Ridley spawns it.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The room's view of Ridley, which Ceres Ridley attaches himself to and whose escape progress he resets.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Shows Ridley's colours over the room once he has begun his encounter.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFlight(CeresRidleyFlightDependencies dependencies)
    {
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _places = dependencies.Places;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _presentation = dependencies.Presentation;
    }

    /// <summary>
    /// Sets Ceres Ridley up as he spawns and begins his encounter, which his body holds from then on (#2231); Norfair
    /// Ridley sets himself up. A Ceres Ridley whose boss is already dead never spawns: the room's state once its boss is
    /// dead places no Ridley, only its door.
    /// </summary>
    internal void InitializeCeresRidley(RoomEnemyPlace place)
    {
        // Ceres Ridley starts with a fresh encounter, below, and the minimap stays off, which the room's load sees to once
        // the enemies are loaded; Ceres has no arena to mark explored.
        place.CollidesByFrameHitboxes = true;

        // He starts stopping the Plasma Beam and ignoring Samus.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Position = place.Position.WithWhole(CeresRidleyArenaDefinitions.Spawn);
        _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;

        var ridley = new CeresRidleyState(place.Position.Whole);
        place.SetSpeciesState(ridley);
        _ridleyEncounter.BeginCeres(place, ridley);
        RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.Standing);
        // Ceres Ridley's setup ends by stopping the music.
        _audio.RequestRoomEnemyStartMusic(MusicRequest.Stop);

        // He arrives with the door's, container's and Baby's colours, his own hidden in black for his eye and body fades.
        ridley.Colours.ShowCeresArrival();
        _presentation.BossShadings.Show(BossShadingLayer.Ridley, ridley.Colours);
    }

    /// <summary>Ceres Ridley's lift-off starts his rise toward his hover.</summary>
    /// <param name="state">Ridley's state.</param>
    internal static void LiftOff(CeresRidleyState state) => Ensure.NotNull(state).Phase = CeresRidleyPhase.LiftoffAccelerating;

    /// <summary>
    /// Ceres Ridley cuts his fireballing short when Samus has under 30 energy: his hover lasts only the cut's updates
    /// (<see cref="CeresRidleyFlightDefinitions.FireballingCutShortUpdates"/>), and he
    /// continues in the fireballing's end for the way he faces, which nothing in the fireballing turns.
    /// </summary>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whose energy decides the cut.</param>
    internal static RidleyBodySegment? CutFireballingShort(CeresRidleyState state, SamusState samus)
    {
        if (Ensure.NotNull(samus).Health >= CeresRidleyFightDefinitions.SamusLowEnergy)
            return null;
        Ensure.NotNull(state).PhaseTimer.LastFor(CeresRidleyFlightDefinitions.FireballingCutShortUpdates);
        return RidleyAnimations.FireballingEnd.For(state.Flight.FacingDirection);
    }

    /// <summary>Slows Ridley's rise by five sixty-fourths of a pixel an update until he climbs past line 80, where he starts hovering.</summary>
    internal static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // On the update the rise starts slowing, his speed takes both the rise's -16 and the slowing's +20.
        // The rise carries Ridley past line 80, 32 pixels above where the slowing starts, within a few updates.
        state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y + CeresRidleyFlightDefinitions.LiftoffSlowingStep);
        if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.HoverLineY)
        {
            BeginHover(state);
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Starts his hover, which waits its updates outside the hover box before he attacks anyway.</summary>
    internal static void BeginHover(CeresRidleyState state)
    {
        state.Phase = CeresRidleyPhase.Hovering;
        state.HoverWait.LastFor(CeresRidleyFlightDefinitions.HoverWaitUpdates);
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    internal void TickCeresRidleyHover(
        RoomEnemyPlace place,
        CeresRidleyState state,
        SamusState samus)
    {
        // Ceres Ridley never loses health. Each shot that hits him counts here, and the fight ends in the fake
        // retreat once 100 have.
        if (state.HitCounter >= CeresRidleyFightDefinitions.HitsToDriveOff)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Phase = CeresRidleyPhase.FakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(place, state);
            return;
        }

        // The fight's other end is Samus's energy falling below 30: the real retreat starts and takes its first
        // acceleration in this same update.
        if (samus.Health < CeresRidleyFightDefinitions.SamusLowEnergy)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(place, state);
            return;
        }

        // He returns to his hover: he accelerates toward the hover point with the hover's inertia.
        AccelerateRidleyToward(place, state.Flight, target: CeresRidleyArenaDefinitions.HoverPoint, divisor: CeresRidleyFlightDefinitions.HoverDivisor);
        if (!RidleyFlight.IsWithinRectangle(place, CeresRidleyArenaDefinitions.HoverPoint, CeresRidleyArenaDefinitions.HoverReach))
        {
            // The hover waits only while Ridley is still outside the hover box, for at most its wait.
            if (state.HoverWait.StepKeepsRunning())
                return;
        }

        state.Phase = CeresRidleyAttackDefinitions.Attack.Choose(_room.Random.Read());
        // A move into his fireball position counts its wait above the hover line from the choice.
        state.FireballMoveWait.LastFor(CeresRidleyFlightDefinitions.FireballMoveGiveUpUpdates);
    }

    /// <summary>Steers Ridley into his fireball position and installs the mouth/fireball animation.</summary>
    internal void TickCeresRidleyFireballMove(RoomEnemyPlace place, CeresRidleyState state)
    {
        state.Flight.Velocity = state.Flight.Velocity.WithY(
            state.Flight.Velocity.Y.WithSpeedAtLeast(CeresRidleyFlightDefinitions.FireballMoveMinimumSpeed));
        AccelerateRidleyToward(place, state.Flight, new Position16<RoomPixels>(place.Position.X.Whole, CeresRidleyArenaDefinitions.FireballMoveTargetY), CeresRidleyFlightDefinitions.HoverDivisor);

        int height = RidleyFlight.SignedPlace(place).Y;
        if (height < CeresRidleyArenaDefinitions.HoverLineY)
        {
            if (state.FireballMoveWait.StepRunsOut())
                state.Phase = CeresRidleyPhase.SwoopSetup;
            return;
        }
        if (height >= CeresRidleyArenaDefinitions.FireballLineY)
            return;

        state.FireballBasePosition = place.Position.Whole;
        // The fireball phase starts his mouth-and-fireball animation, whose cues aim and spawn the fireballs.
        RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.Fireballing);
        state.Phase = CeresRidleyPhase.FireballShooting;
        state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.FireballingUpdates);
        TickCeresRidleyFireballShooting(place, state);
    }

    /// <summary>Selects the next fireball target from the base position and a jitter drawn from the current random number.</summary>
    internal void TickCeresRidleyFireballShooting(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The aim reads the room's current random number without drawing a new one.
        int random = _room.Random.Read();
        int jitter = RidleyRandomDefinitions.FireballJitter.Of(random).Toward(BossRandomRolls.SideOf(random));
        Position16<RoomPixels> target = CeresRidleyFireballTarget.Around(state.FireballBasePosition, jitter);
        AccelerateRidleyToward(place, state.Flight, target, CeresRidleyFlightDefinitions.HoverDivisor);
        if (state.PhaseTimer.StepRunsOut())
            BeginHover(state);
    }

    /// <summary>Advances the attack lunge until its position threshold hands control to the swoop.</summary>
    internal static void TickCeresRidleyLunge(
        RoomEnemyPlace place,
        CeresRidleyState state,
        SamusState samus)
    {
        Position16<RoomPixels> target = samus.Position.Y.Whole < CeresRidleyArenaDefinitions.LungeSamusHighLineY
            ? samus.Position.Whole.WithY(CeresRidleyArenaDefinitions.LungeHighTargetY)
            : samus.Position.Whole - CeresRidleyArenaDefinitions.LungeAboveSamus;
        AccelerateRidleyToward(place, state.Flight, target, CeresRidleyFlightDefinitions.LungeDivisor);
        bool reached = RidleyFlight.IsWithinRectangle(place, target, CeresRidleyArenaDefinitions.LungeReach);
        bool ranOut = state.PhaseTimer.StepRunsOut();
        if (reached || ranOut)
            BeginHover(state);
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration.</summary>
    internal static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.SwoopStart, CeresRidleyFlightDefinitions.SweepDivisor);
        if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.SwoopStartLineY)
        {
            state.Phase = CeresRidleyPhase.SwoopDescendingAimingDown;
            state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.SwoopDiveUpdates);
            state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    internal static void TickCeresRidleySwoopPhase(
        CeresRidleyState state,
        TurnStep turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed,
        CeresRidleyPhase nextPhase,
        int nextUpdates)
    {
        RidleyFlight.UpdateSwoopVelocity(state.Flight, turn, targetAngle, targetSpeed);
        if (state.PhaseTimer.StepRunsOut())
        {
            state.Phase = nextPhase;
            state.PhaseTimer.LastFor(nextUpdates);
        }
    }

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(CeresRidleyState state)
    {
        state.Phase = CeresRidleyPhase.RealRetreatRising;
        state.Flight.MovementMinimum = state.Flight.MovementMinimum.WithY(CeresRidleyArenaDefinitions.RetreatMovementTop);
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    internal static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // Ridley steers toward the fake retreat's point. Only the fake retreat after 100 hits takes the Baby back; the retreat
        // at Samus's low energy leaves without it.
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.FakeRetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (RidleyFlight.SignedPlace(place).X >= CeresRidleyArenaDefinitions.FakeRetreatPoint.X)
            state.Phase = CeresRidleyPhase.FakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=32 before releasing the Baby into its independent fall.</summary>
    internal static void TickCeresRidleyFakeRetreatRising(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // Ridley rises past Y=32 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent fall and the wait
        // before Ridley changes to the retrieval pose.
        state.Flight.MovementMinimum = state.Flight.MovementMinimum.WithY(CeresRidleyArenaDefinitions.RetreatMovementTop);
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.RetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.BabyDropLineY)
        {
            state.Baby.Action = CeresBabyAction.Drop;
            state.Phase = CeresRidleyPhase.WaitBeforeRetrievingBaby;
            state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.BabyRetrievalDelayUpdates);
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    internal static void TickCeresRidleyRetrieveBaby(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The Baby grab flies Ridley's hand toward the falling Baby. The collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(place, state.Flight, state.Baby.Position.Whole - CeresRidleyArenaDefinitions.RetrievalHandAboveBaby, CeresRidleyFlightDefinitions.RetrievalDivisor);

        Offset16<RoomPixels> fromHand = state.Baby.Position.Whole - (place.Position.Whole + CeresRidleyArenaDefinitions.BabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= CeresRidleyArenaDefinitions.BabyGraspReach || Math.Abs(fromHand.Y) >= CeresRidleyArenaDefinitions.BabyGraspReach)
        {
            return;
        }

        state.Baby.Action = CeresBabyAction.CarriedInFeet;
        state.Flight.Velocity = state.Flight.Velocity.WithY(CeresRidleyFlightDefinitions.RetreatStartSpeed);
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the getaway delay.</summary>
    internal void TickCeresRidleyRetreat(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The retreat accelerates toward the off-screen point, and the check against the line comes before his
        // movement. Once he is above the retreat point the delay starts, its first update the same update.
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.RetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.RetreatPoint.Y)
        {
            state.Phase = CeresRidleyPhase.RetreatDelay;
            state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.RetreatDelayUpdates);
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary>Ends the retreat's delay: places the two wall actors, stops Ridley and recolours the chamber.</summary>
    internal void TickCeresRidleyRetreatDelay(CeresRidleyState state)
    {
        if (state.PhaseTimer.StepKeepsRunning())
            return;

        // The retreat places the two wall actors before the getaway begins. The getaway scene shows the chamber's walls
        // itself (#1545); the actors draw nothing, but they take two enemy places.
        SpawnCeresRidleyGetawayWalls();
        state.Flight.Velocity = PixelVelocity<RoomPixels>.Zero;
        state.Tail.Action = RidleyTailAction.None;
        state.Phase = CeresRidleyPhase.PublishEscapeHandoff;

        // The retreat recolours the chamber: its walls and the scenery that shares the retreat colours. Ridley keeps
        // his own colours (#1544).
        state.Colours.ShowRetreat();
    }

    /// <summary>
    /// Steers both axes of his velocity toward <paramref name="target"/>, where it lies from the room's origin, which
    /// may be above the chamber, at a negative height.
    /// </summary>
    internal static void AccelerateRidleyToward(
        RoomEnemyPlace place,
        RidleyFlightState flight,
        Offset16<RoomPixels> target,
        int divisor) =>
        AccelerateRidleyToward(place, flight, Position16<RoomPixels>.Zero + target, divisor);

    /// <summary>Steers both axes of his velocity toward a target, raising a step that divides to zero to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemyPlace place,
        RidleyFlightState flight,
        Position16<RoomPixels> target,
        int divisor)
    {
        // Division truncates like the game's quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        divisor = RidleyInertia.Divisor(divisor);

        Position16<RoomPixels> at = place.Position.Whole;
        Offset16<RoomPixels> fromTarget = at - target;
        flight.Velocity = new PixelVelocity<RoomPixels>(
            AccelerateAxis(new SeekingVelocityRun(flight.Velocity.X, at.X, target.X), fromTarget.X, divisor),
            AccelerateAxis(new SeekingVelocityRun(flight.Velocity.Y, at.Y, target.Y), fromTarget.Y, divisor));
    }

    /// <summary>
    /// One axis of Ridley's seek (Y and X): one run of steps toward the target, three while reversing
    /// and one otherwise, in which a step that crosses zero makes the next one a unit larger
    /// (<see cref="SeekingVelocityRun"/>). The velocity stays within a few thousand units of rest (each update clamps it
    /// to five pixels and a step is at most the arena's width), so the clamps compare signed velocities.
    /// </summary>
    /// <param name="run">The run of steps from the axis velocity toward the target from Ridley's place on the axis.</param>
    /// <param name="difference">Ridley's displacement from the target on the axis.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    private static SignedEightEight AccelerateAxis(SeekingVelocityRun run, int difference, int divisor)
    {
        SignedEightEight velocity = run.Velocity;
        if (difference == 0)
            return velocity;
        // The distance divided by the inertia, at most 32768; a step that divides to zero is raised to one.
        SignedEightEight step = SeekingVelocityRun.StepFor(Math.Max(1, Math.Abs(difference) / divisor));

        if (difference < 0)
        {
            if (velocity.IsNegative)
            {
                run.Raise(step);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, RidleyInertiaDefinitions.MaximumVelocity);
        }
        if (!velocity.IsNegative)
        {
            run.Lower(step);
            run.Lower(step);
        }
        run.Lower(step);
        return SignedEightEight.Max(run.Velocity, RidleyInertiaDefinitions.MinimumVelocity);
    }

    /// <summary>
    /// Places the getaway's two wall actors at the chamber's walls (#1545). They draw nothing: the getaway scene shows the
    /// walls (<see cref="CeresEscape.ChamberWalls"/>). They are kept because gameplay can observe them: each takes
    /// the first free enemy place and extends the room's filled places, and on the update after this one they are
    /// touchable, before their first animation step makes them ignore Samus and her weapons.
    /// </summary>
    private void SpawnCeresRidleyGetawayWalls()
    {
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.LeftWall, HorizontalDirection.Left);
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.RightWall, HorizontalDirection.Right);
    }

    /// <summary>Initializes one wall actor at its place, on its side of the chamber, drawing nothing.</summary>
    private void SpawnCeresRidleyGetawayWall(Position16<RoomPixels> position, HorizontalDirection side)
    {
        RoomEnemyPlace wall = _places.FindFreePlace() ??
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy place for a turning-floor wall.");

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(EnemyDefinitionId.CeresGetawayChamberWall);
        RoomEnemyPlacement population = new(
            EnemyDefinitionId.CeresGetawayChamberWall,
            Position: position,
            Contact: EnemyContact.None,
            Processing: EnemyProcessing.Animates | EnemyProcessing.OffScreen,
            Presence: EnemyPresence.Hidden,
            CollidesByFrameHitboxes: false,
            SpawnSettings: new CeresGetawayChamberWallSpawnSettings(side));
        _lifecycle.InitializePlaceFromDefinition(wall, population, definition);
        _lifecycle.SetUpSpawnedEnemy(wall);
        _places.CoverSpawn(wall);
    }
}
