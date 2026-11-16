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
    /// Sets up the Ceres branch of Ridley's initializer and begins his encounter, which his body holds from then on (#2231);
    /// Norfair Ridley's branch is his own part's. The initializer's branch for a Ceres Ridley whose boss is already dead
    /// never runs: the room's state once its boss is dead places no Ridley, only its door.
    /// </summary>
    internal CeresRidleyState InitializeCeresRidley(RoomEnemyPlace place)
    {
        // Ceres Ridley's initialization clears the boss's working state, which the fresh encounter below holds, and
        // disables the minimap, which the room loader does once the enemies are loaded; Ceres has no arena to mark explored.
        place.CollidesByFrameHitboxes = true;

        // The initializer sets both of Ridley's contact properties: he stops the Plasma Beam and ignores Samus.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Position = place.Position.WithWhole(CeresRidleyArenaDefinitions.Spawn);
        _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;

        var ridley = new CeresRidleyState(place.Position.Whole);
        place.SetSpeciesState(ridley);
        _ridleyEncounter.BeginCeres(place, ridley);
        RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.Initial);
        // Ceres Ridley's initialization ends by queuing music zero (stop).
        _audio.RequestRoomEnemyStartMusic(MusicRequest.Stop);

        // He arrives with the door's, container's and Baby's colours, his own hidden in black for his eye and body fades.
        ridley.Colours.ShowCeresArrival();
        _presentation.BossShadings.Show(BossShadingLayer.Ridley, ridley.Colours);
        return ridley;
    }

    /// <summary>Ceres Ridley's lift-off starts his rise toward his hover.</summary>
    /// <param name="state">Ridley's state.</param>
    internal static void LiftOff(CeresRidleyState state) => Ensure.NotNull(state).Phase = CeresRidleyPhase.LiftoffAccelerating;

    /// <summary>
    /// Ceres Ridley cuts his fireballing short when Samus has under 30 energy: he stores eight in his phase timer, cutting
    /// the fireball hover short, and continues in the fireballing's end for the way he faces, which nothing in the
    /// fireballing turns.
    /// </summary>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whose energy decides the cut.</param>
    internal static RidleyBodySegment? CutFireballingShort(CeresRidleyState state, SamusState samus)
    {
        if (Ensure.NotNull(samus).Health >= CeresRidleyFightDefinitions.SamusLowEnergy)
            return null;
        Ensure.NotNull(state).PhaseTimer.Count = CeresRidleyFlightDefinitions.FireballingCutShort;
        return RidleyAnimations.FireballingEnd.For(state.Flight.FacingDirection);
    }

    /// <summary>Applies the crossing-frame -16 then +20 velocity changes before shared Ridley motion.</summary>
    internal static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // The fallthrough from the rise deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        // The rise carries Ridley past line 80, 32 pixels above where the slowing starts, within a few updates.
        state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y + CeresRidleyFlightDefinitions.LiftoffSlowingStep);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.HoverLineY) < 0)
        {
            state.Phase = CeresRidleyPhase.Hovering;
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    internal void TickCeresRidleyHover(
        RoomEnemyPlace place,
        CeresRidleyState state,
        SamusState samus)
    {
        // Ceres Ridley never loses the ordinary enemy health. His shot AI increments
        // this separate counter and the fight treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= CeresRidleyFightDefinitions.HitsToDriveOff)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Phase = CeresRidleyPhase.FakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(place, state);
            return;
        }

        // The fight's other end is Samus's energy falling below 30.
        // The low-energy check jumps into the real retreat, so its first acceleration runs this call.
        if (samus.Health < CeresRidleyFightDefinitions.SamusLowEnergy)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(place, state);
            return;
        }

        // The shared cooldown/return-to-hover movement: accelerate toward the hover point with the hover's divisor.
        AccelerateRidleyToward(place, state.Flight, target: CeresRidleyArenaDefinitions.HoverPoint, divisor: CeresRidleyFlightDefinitions.HoverDivisor);
        if (!RidleyFlight.IsWithinRectangle(place, CeresRidleyArenaDefinitions.HoverPoint, CeresRidleyArenaDefinitions.HoverReach))
        {
            // The hover waits only while Ridley is still outside the hover box, for at most its wait limit.
            state.HoverCounter++;
            if (state.HoverCounter < CeresRidleyFlightDefinitions.HoverWaitLimit)
                return;
        }

        state.Phase = CeresRidleyAttackDefinitions.SelectAttack(_room.Random.Read());
        state.HoverCounter = 0;
    }

    /// <summary>Steers Ridley into his fireball position and installs the mouth/fireball animation.</summary>
    internal void TickCeresRidleyFireballMove(RoomEnemyPlace place, CeresRidleyState state)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed always has a magnitude.
        SignedEightEight verticalVelocity = state.Flight.Velocity.Y;
        state.Flight.Velocity = state.Flight.Velocity.WithY(SignedEightEight.Max(CeresRidleyFlightDefinitions.FireballMoveMinimumSpeed, verticalVelocity.Magnitude())
            .AlongDirectionOf(verticalVelocity));
        AccelerateRidleyToward(place, state.Flight, new(place.Position.X.Whole, CeresRidleyArenaDefinitions.FireballMoveTargetY), CeresRidleyFlightDefinitions.HoverDivisor);

        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.HoverLineY) < 0)
        {
            state.HoverCounter++;
            if (state.HoverCounter >= CeresRidleyFlightDefinitions.FireballMoveGiveUp)
                state.Phase = CeresRidleyPhase.SwoopSetup;
            return;
        }
        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.FireballLineY) >= 0)
            return;

        state.FireballBasePosition = place.Position.Whole;
        // The fireball phase starts the complete retail mouth/fireball animation.
        // Its fireball cues calculate and spawn actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.Fireballing);
        state.Phase = CeresRidleyPhase.FireballShooting;
        state.PhaseTimer.Count = CeresRidleyFlightDefinitions.FireballingUpdates;
        TickCeresRidleyFireballShooting(place, state);
    }

    /// <summary>Selects the next fireball target from the base position and a jitter drawn from the current random number.</summary>
    internal void TickCeresRidleyFireballShooting(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The attack choice reads RandomNumberSeed without generating a number.
        int random = _room.Random.Read();
        int jitter = BossRandomRolls.InLowerHalf(random)
            ? RidleyRandomDefinitions.FireballJitter.Of(random)
            : -RidleyRandomDefinitions.FireballJitter.Of(random);
        Position16<RoomPixels> target = CeresRidleyFireballTarget.Around(state.FireballBasePosition, jitter);
        AccelerateRidleyToward(place, state.Flight, target, CeresRidleyFlightDefinitions.HoverDivisor);
        if (state.PhaseTimer.StepRunsOut())
        {
            state.HoverCounter = 0;
            state.Phase = CeresRidleyPhase.Hovering;
        }
    }

    /// <summary>Advances the attack lunge until its position threshold hands control to the swoop.</summary>
    internal static void TickCeresRidleyLunge(
        RoomEnemyPlace place,
        CeresRidleyState state,
        SamusState samus)
    {
        Position16<RoomPixels> target = CoordinateAxis.Difference(samus.Position.Y.Whole, CeresRidleyArenaDefinitions.LungeSamusHighLineY) < 0
            ? samus.Position.Whole.WithY(CeresRidleyArenaDefinitions.LungeHighTargetY)
            : samus.Position.Whole - CeresRidleyArenaDefinitions.LungeAboveSamus;
        AccelerateRidleyToward(place, state.Flight, target, CeresRidleyFlightDefinitions.LungeDivisor);
        bool reached = RidleyFlight.IsWithinRectangle(place, target, CeresRidleyArenaDefinitions.LungeReach);
        bool ranOut = state.PhaseTimer.StepRunsOut();
        if (reached || ranOut)
        {
            state.HoverCounter = 0;
            state.Phase = CeresRidleyPhase.Hovering;
        }
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration.</summary>
    internal static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.SwoopStart, CeresRidleyFlightDefinitions.SweepDivisor);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.SwoopStartLineY) < 0)
        {
            state.Phase = CeresRidleyPhase.SwoopDescendingAimingDown;
            state.PhaseTimer.Count = CeresRidleyFlightDefinitions.SwoopDiveUpdates;
            state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    internal static void TickCeresRidleySwoopPhase(
        CeresRidleyState state,
        RidleySwoopTurn turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed,
        CeresRidleyPhase nextPhase,
        int nextTimer)
    {
        RidleyFlight.UpdateSwoopVelocity(state.Flight, turn, targetAngle, targetSpeed);
        if (state.PhaseTimer.StepRunsOut())
        {
            state.Phase = nextPhase;
            state.PhaseTimer.Count = nextTimer;
        }
    }

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(CeresRidleyState state)
    {
        state.Phase = CeresRidleyPhase.RealRetreatRising;
        state.Flight.MovementMinimum = state.Flight.MovementMinimum.WithY(CoordinateAxis.Wrap(CeresRidleyArenaDefinitions.RetreatMovementTop));
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    internal static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // The shared Ridley phase steers toward the fake retreat's point. Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.FakeRetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (CoordinateAxis.Difference(place.Position.X.Whole, CeresRidleyArenaDefinitions.FakeRetreatPoint.X) >= 0)
            state.Phase = CeresRidleyPhase.FakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=32 before releasing the Baby into its independent fall.</summary>
    internal static void TickCeresRidleyFakeRetreatRising(
        RoomEnemyPlace place,
        CeresRidleyState state)
    {
        // Ridley rises past Y=32 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent fall and a
        // countdown of 21, which runs out on its 22nd step, before Ridley changes to the retrieval pose.
        state.Flight.MovementMinimum = state.Flight.MovementMinimum.WithY(CoordinateAxis.Wrap(CeresRidleyArenaDefinitions.RetreatMovementTop));
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.RetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.BabyDropLineY) < 0)
        {
            state.Baby.Action = CeresBabyAction.Drop;
            state.Phase = CeresRidleyPhase.WaitBeforeRetrievingBaby;
            state.PhaseTimer.Count = CeresRidleyFlightDefinitions.BabyRetrievalDelay;
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    internal static void TickCeresRidleyRetrieveBaby(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The Baby grab flies Ridley's hand toward the falling Baby. The collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(place, state.Flight, state.Baby.Position.Whole - CeresRidleyArenaDefinitions.RetrievalHandAboveBaby, CeresRidleyFlightDefinitions.RetrievalDivisor);

        Offset16<RoomPixels> fromHand = state.Baby.Position.Whole - (place.Position.Whole + CeresRidleyBabyScene.CeresBabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= CeresRidleyArenaDefinitions.BabyGraspReach || Math.Abs(fromHand.Y) >= CeresRidleyArenaDefinitions.BabyGraspReach)
        {
            return;
        }

        state.Baby.Action = CeresBabyAction.CarriedInFeet;
        state.Flight.Velocity = state.Flight.Velocity.WithY(CeresRidleyFlightDefinitions.RetreatStartSpeed);
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the getaway delay, a countdown of 64.</summary>
    internal void TickCeresRidleyRetreat(RoomEnemyPlace place, CeresRidleyState state)
    {
        // The retreat accelerates toward the off-screen point. The above-the-line check
        // is performed before the common movement pass; once the origin is above the retreat point
        // the delay is installed and falls through into its first step, so it runs out on its 65th step.
        AccelerateRidleyToward(place, state.Flight, CeresRidleyArenaDefinitions.RetreatPoint, CeresRidleyFlightDefinitions.SweepDivisor);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, CeresRidleyArenaDefinitions.RetreatPoint.Y) < 0)
        {
            state.Phase = CeresRidleyPhase.RetreatDelay;
            state.PhaseTimer.Count = CeresRidleyFlightDefinitions.RetreatDelay;
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary>Ends the retreat's delay: places the two wall actors, stops Ridley and recolours the chamber.</summary>
    internal void TickCeresRidleyRetreatDelay(CeresRidleyState state)
    {
        if (state.PhaseTimer.StepKeepsRunning())
            return;

        // The retreat places the two wall actors before the getaway begins. The getaway scene shows the chamber's walls
        // itself (#1545); the actors draw nothing, but they take two enemy places (SpawnCeresRidleyGetawayWalls).
        SpawnCeresRidleyGetawayWalls();
        state.Flight.Velocity = EightEightVector<RoomPixels>.Zero;
        state.Tail.Action = RidleyTailAction.None;
        state.Phase = CeresRidleyPhase.PublishEscapeHandoff;

        // The retreat recolours the chamber: its walls and the scenery that shares the retreat colours. The game also copied those
        // colours into Ridley's own sprite colours; his sprites here keep their own colours (#1544).
        state.Colours.ShowRetreat();
    }

    /// <summary>Steers both 1/256-pixel velocity axes toward a target, raising a step that divides to zero to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemyPlace place,
        RidleyFlightState flight,
        Position16<RoomPixels> target,
        int divisor)
    {
        // Division truncates like the game's quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        divisor = RidleyInertia.Divisor(divisor);

        flight.Velocity = new EightEightVector<RoomPixels>(
            AccelerateAxis(flight.Velocity.X, place.Position.X.Whole, target.X, divisor),
            AccelerateAxis(flight.Velocity.Y, place.Position.Y.Whole, target.Y, divisor));
    }

    /// <summary>
    /// One axis of Ridley's seek (Y and X): one run of steps toward the target, three while reversing
    /// and one otherwise, in which a step that crosses zero makes the next one a unit larger
    /// (<see cref="SeekingVelocityRun"/>). The velocity stays within a few thousand units of rest (each call clamps it
    /// to five pixels and a step is at most the arena's width), so the clamps compare signed velocities.
    /// </summary>
    /// <param name="velocity">The axis velocity.</param>
    /// <param name="position">Ridley's whole coordinate on the axis, as his position holds it.</param>
    /// <param name="target">The target's whole coordinate on the axis, as its position holds it.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    private static SignedEightEight AccelerateAxis(SignedEightEight velocity, int position, int target, int divisor)
    {
        int difference = CoordinateAxis.Difference(position, target);
        if (difference == 0)
            return velocity;
        // The distance divided by the inertia, at most 32768; a step that divides to zero is raised to one.
        int step = Math.Max(1, Math.Abs(difference) / divisor);
        var run = new SeekingVelocityRun(velocity, position, target);

        if (difference < 0)
        {
            if (velocity.IsNegative)
            {
                run.Raise(step);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, CeresRidleyFlightDefinitions.MaximumSeekSpeed);
        }
        if (!velocity.IsNegative)
        {
            run.Lower(step);
            run.Lower(step);
        }
        run.Lower(step);
        return SignedEightEight.Max(run.Velocity, CeresRidleyFlightDefinitions.MinimumSeekSpeed);
    }

    /// <summary>
    /// Places the getaway's two wall actors at the chamber's walls (#1545). They draw nothing: the getaway scene shows the
    /// walls (<see cref="CeresEscape.ChamberWalls"/>). They are kept because gameplay can observe them: each takes
    /// the first free enemy place and extends the room's filled places, and on the update after this one they are
    /// touchable, before their first animation step makes them ignore Samus and her weapons. Evidence: the original's
    /// retreat spawns the two door-family enemies, whose animations start with the intangibility step.
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
        _lifecycle.RunInitializationAi(wall, EnemySpawnSurroundings.None);
        _places.CoverSpawn(wall);
    }
}
