using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's arrival and fight: hovering, swoops, the wall sweep, the pogo attack and grabbing Samus.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyCombat
{
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes tail-impact tremors to the room earthquake state.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues roars, tail impacts, and other enemy sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The room's view of Ridley's encounters, which he records beginning his in.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    private readonly RidleyTail _ridleyTail;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>Shows Ridley's colours over the room once he has begun his encounter.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyCombat(NorfairRidleyCombatDependencies dependencies)
    {
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _ridleyTail = dependencies.RidleyTail;
        _ridleyProjectiles = dependencies.RidleyProjectiles;
        _roomFx = dependencies.RoomFx;
        _presentation = dependencies.Presentation;
    }

    /// <summary>
    /// Sets Lower Norfair Ridley up and begins his encounter, which his body holds from then on
    /// (#2231). A defeated Ridley is deleted before his encounter or his colours begin, as every defeated boss is, and
    /// begins none; a live fight begins with Ridley below the visible arena.
    /// </summary>
    internal void InitializeNorfairRidley(RoomEnemyPlace place)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }
        place.CollidesByFrameHitboxes = true;

        // Ridley's body stops a Plasma Beam from passing through him.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Position = place.Position.WithWhole(NorfairRidleyArenaDefinitions.Spawn);

        var ridley = new NorfairRidleyState { MovementAnimationEnabled = true };
        place.SetSpeciesState(ridley);
        _ridleyEncounter.BeginLowerNorfair();
        RidleyFlight.StartAnimation(ridley.Flight, place, RidleyAnimations.Standing);

        // He arrives in his own colours, his body and tail hidden in black, and his arena hidden for its reveal.
        ridley.Colours.ShowNorfairArrival();
        _presentation.BossShadings.Show(BossShadingLayer.Ridley, ridley.Colours);
    }

    /// <summary>Lower Norfair Ridley's lift-off hands control to the fight: he flies into the arena.</summary>
    /// <param name="state">Ridley's state.</param>
    internal static void LiftOff(NorfairRidleyState state) => Ensure.NotNull(state).Phase = NorfairRidleyPhase.EnterArena;

    /// <summary>
    /// Plays one update of the arena reveal Ridley holds his roar through (#2409): a Norfair-only fade of
    /// <see cref="NorfairRidleyRevealDefinitions.RevealStepCount"/> colour steps, one every third update. The update after
    /// the last step's raises the battle's liquid and ends the hold.
    /// </summary>
    /// <returns>Whether the reveal ended this update, so he takes off.</returns>
    internal bool TickNorfairRidleyArenaReveal(NorfairRidleyState state)
    {
        if (state.ArenaReveal.StepToNewFrame() is { } image)
            state.Colours.ShowArenaReveal(image);
        if (!state.ArenaReveal.IsResting)
            return false;

        RidleyLiquidMotion.Publish(_roomFx, RidleyLiquidDefinitions.BattleHeight,
            RidleyLiquidDefinitions.RiseVelocity, RidleyLiquidDefinitions.RiseDelay);
        return true;
    }

    /// <summary>Steers Ridley to the facing-dependent hover point until timeout or arrival.</summary>
    internal static void TickNorfairRidleyHover(RoomEnemyPlace place, NorfairRidleyState state)
    {
        if (state.PhaseTimer.StepRunsOut())
        {
            state.Phase = NorfairRidleyPhase.SelectAttack;
            return;
        }

        Position16<RoomPixels> target = new(RidleyMovementTargets.Hover.For(state.Flight.FacingDirection), NorfairRidleyArenaDefinitions.FlightLineY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state.Flight,
            target,
            ReadRidleyHealthMovementDivisor(state));
        if (RidleyFlight.IsWithinRectangle(place, target, NorfairRidleyArenaDefinitions.FlightTargetReach))
            state.Phase = NorfairRidleyPhase.SelectAttack;
    }

    /// <summary>Moves to the high side point and initializes the swoop angle timer.</summary>
    internal static void TickNorfairRidleySwoopMoveToStart(RoomEnemyPlace place, NorfairRidleyState state)
    {
        Position16<RoomPixels> target = new(RidleyMovementTargets.SwoopStart.For(state.Flight.FacingDirection), NorfairRidleyArenaDefinitions.SwoopStartY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, target, divisor: NorfairRidleyMotionDefinitions.SwoopStartDivisor);
        if (!RidleyFlight.IsWithinRectangle(place, target, NorfairRidleyArenaDefinitions.FlightTargetReach))
            return;

        state.Phase = NorfairRidleyPhase.SwoopAimDown;
        state.SwoopPhaseTimer.LastFor(NorfairRidleyMotionDefinitions.SwoopDiveUpdates);
        state.Flight.SwoopHeading = RotationAngle.Zero;
    }

    /// <summary>Advances the swoop velocity arc and changes phase, for <paramref name="nextUpdates"/>, as its countdown runs out.</summary>
    internal static bool TickNorfairRidleySwoopPhase(
        NorfairRidleyState state,
        TurnStep turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed,
        NorfairRidleyPhase nextPhase,
        int nextUpdates)
    {
        RidleyFlight.UpdateSwoopVelocity(state.Flight, turn, targetAngle, targetSpeed);
        if (state.SwoopPhaseTimer.StepKeepsRunning())
            return false;

        state.Phase = nextPhase;
        state.SwoopPhaseTimer.LastFor(nextUpdates);
        return true;
    }

    /// <summary>
    /// Runs the wall sweep: Ridley sweeps between the walls behind and ahead of him at Samus's height while she spin
    /// jumps; any other movement starts the pogo attack at once.
    /// </summary>
    internal void TickNorfairRidleyWallSweep(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        RidleyWallSweepTarget wall)
    {
        RidleyFacing facing = state.Flight.FacingDirection;
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state.Flight,
            new Position16<RoomPixels>(
                (wall == RidleyWallSweepTarget.WallBehind ? RidleyMovementTargets.WallBehind : RidleyMovementTargets.WallAhead).For(facing),
                int.Min(samus.Position.Y.Whole, NorfairRidleyArenaDefinitions.PogoZoneTopY)),
            ReadRidleyHealthMovementDivisor(state));
        state.Tail.Whip.Give();

        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyPogoAttack(state);
            return;
        }

        // The fireball choice uses the current random number; it does not draw a new one.
        if (RidleyRandomDefinitions.WallSweepFireballs.IsTakenBy(_room.Random.Read()) && !state.Flight.Roaring && state.Flight.FacingDirection != RidleyFacing.Turning)
            RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.Fireballing);

        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.Phase = wall == RidleyWallSweepTarget.WallBehind
            ? NorfairRidleyPhase.SweepToWallAhead
            : NorfairRidleyPhase.SweepToWallBehind;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.WallSweepLegUpdates);
        SelectNorfairRidleyFacingAnimation(place, state);
    }

    /// <summary>Resets tail extension and starts the pogo attack by moving Ridley to its side of the arena.</summary>
    internal static void BeginNorfairRidleyPogoAttack(NorfairRidleyState state)
    {
        state.Tail.ExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
        state.Tail.IdealLinkSpacing = RidleyTailDefinitions.StandardLinkSpacing;
        state.Tail.Action = RidleyTailAction.Neutral;
        state.Phase = NorfairRidleyPhase.PogoMoveToSide;
    }

    /// <summary>Moves Ridley toward the facing-dependent side before descending to the pogo height.</summary>
    internal void TickNorfairRidleyPogoMoveToSide(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus)
    {
        if (place.Position.Y.Whole < NorfairRidleyArenaDefinitions.PogoHeightY)
        {
            SelectNorfairRidleyFacingAnimation(place, state);
            state.Phase = NorfairRidleyPhase.PogoMoveToHeight;
            state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.PogoSettleUpdates);
            TickNorfairRidleyPogoMoveToHeight(place, state, samus);
            return;
        }

        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place, state.Flight, new Position16<RoomPixels>(RidleyMovementTargets.PogoSide.For(state.Flight.FacingDirection), NorfairRidleyArenaDefinitions.PogoHeightY), divisor: NorfairRidleyMotionDefinitions.PogoSetupDivisor);
    }

    /// <summary>Settles at the pogo's height, sets the pogo tail, and starts the pogo's random time limit.</summary>
    internal void TickNorfairRidleyPogoMoveToHeight(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, place.Position.Whole.WithY(NorfairRidleyArenaDefinitions.PogoHeightY), divisor: NorfairRidleyMotionDefinitions.PogoSetupDivisor);
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.Tail.Action = RidleyTailAction.PogoSetup;
        _ridleyTail.TickRidleyPogoTail(place, state, samus);
        // The first bounce heads the way his settling left him moving, rightward when it left him still.
        state.PogoBounce = state.Flight.Velocity.X.IsNegative ? HorizontalDirection.Left : HorizontalDirection.Right;
        InitializeNorfairRidleyPogoVelocity(state);
        state.Phase = NorfairRidleyPhase.PogoFall;
        state.PhaseTimer.LastFor(RidleyRandomDefinitions.PogoTimer.Of(_room.Random.Read()) + NorfairRidleyMotionDefinitions.PogoLeastUpdates);
    }

    /// <summary>Checks claw grab, accelerates downward, and bounces the tail on terrain contact.</summary>
    internal void TickNorfairRidleyPogoFall(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (SamusMovementUsesRidleyGrab(samus) &&
            RidleyClawOverlapsSamus(place, state, samus, NorfairRidleyArenaDefinitions.PogoClawReach))
        {
            // Every acceleration clamps Ridley's speed to a few pixels an update, so it always has a magnitude.
            state.Flight.Velocity = state.Flight.Velocity.WithY(-SignedEightEight.Max(NorfairRidleyMotionDefinitions.GrabLiftMinimumSpeed, state.Flight.Velocity.Y.Magnitude()));
            state.Tail.Action = RidleyTailAction.Neutral;
            state.Tail.SwingStep = RidleyTailDefinitions.SlowNeutralSwing;
            BeginNorfairRidleyGrab(place, state, samus);
            return;
        }

        // The fall is capped at the pogo's fastest fall.
        state.Flight.Velocity = state.Flight.Velocity.WithY(SignedEightEight.Min(state.Flight.Velocity.Y + state.PogoDownwardAcceleration, NorfairRidleyMotionDefinitions.PogoMaximumFallSpeed));

        if (!RidleyTail.RidleyTailTouchesTerrain(state.Tail, level))
            return;

        RidleyTailLink tip = state.Tail.Tip;
        _ridleyProjectiles.SpawnRidleyDust(tip.Position + NorfairRidleyArenaDefinitions.PogoDustBelowTip, DustExplosionAnimation.SmallDustCloud);
        _audio.QueueEnemySound(EnemySound.Quake, priority: SoundRequestPriority.Normal);
        _earthquake.Start(NorfairRidleyArenaDefinitions.PogoBounceQuake, NorfairRidleyArenaDefinitions.PogoBounceQuakeUpdates);
        SetRidleyPogoHorizontalDirection(place, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        foreach (RidleyTailLink link in state.Tail.Links)
        {
            link.Distance = link.RestDistance;
            link.TargetDistance = RidleyTailDefinitions.BounceDistance;
        }
        state.Tail.Action = RidleyTailAction.Pogo;
        state.PogoBounceCount++;
        if (state.PogoBounceCount >= NorfairRidleyMotionDefinitions.PogoBouncesPerFireballing)
        {
            state.PogoBounceCount = 0;
            if (state.Flight.FacingDirection != RidleyFacing.Turning)
                RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.Fireballing);
        }
        state.Phase = NorfairRidleyPhase.PogoRise;
    }

    /// <summary>Rises after a pogo bounce or returns to attack selection.</summary>
    internal static void TickNorfairRidleyPogoRise(
        NorfairRidleyState state,
        SamusState samus)
    {
        if (samus.Position.Y.Whole < NorfairRidleyArenaDefinitions.PogoZoneTopY || state.PhaseTimer.StepRunsOut())
        {
            state.Tail.Action = RidleyTailAction.Neutral;
            state.Tail.SwingStep = RidleyTailDefinitions.SlowNeutralSwing;
            state.Phase = NorfairRidleyPhase.SelectAttack;
            return;
        }

        // The rise slows toward rest and stops at its peak.
        SignedEightEight nextVertical = state.Flight.Velocity.Y + state.PogoUpwardAcceleration;
        if (!nextVertical.IsNegative)
        {
            state.Flight.Velocity = state.Flight.Velocity.WithY(SignedEightEight.Zero);
            state.Phase = NorfairRidleyPhase.PogoFall;
        }
        else
        {
            state.Flight.Velocity = state.Flight.Velocity.WithY(nextVertical);
        }
    }

    /// <summary>
    /// Launches a pogo bounce: one of the health band's four launches, chosen by the current random number, its sideways
    /// speed taken toward the bounce's side (<see cref="NorfairRidleyState.PogoBounce"/>).
    /// </summary>
    private void InitializeNorfairRidleyPogoVelocity(NorfairRidleyState state)
    {
        // The pogo choice samples the current random number; it does not advance shared RNG.
        RidleyPogoDefinition definition = state.HealthBand.PogoLaunch.Choose(_room.Random.Read());
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.Flight.Velocity = new(definition.Velocity.X.Toward(state.PogoBounce), definition.Velocity.Y);
    }

    /// <summary>Lunges toward a grabbable Samus, ending on passage, boundary, height, or claw contact.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the lunge grabs.</param>
    /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which a missed lunge dodges.</param>
    internal static void TickNorfairRidleyGrabApproach(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        SamusPowerBombExplosionState powerBomb)
    {
        if (!SamusMovementUsesRidleyGrab(samus))
        {
            HandleNorfairRidleyMissedLunge(place, state, powerBomb);
            return;
        }

        // The lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        int deltaX = place.Position.X.Whole - samus.Position.X.Whole;
        bool passedSamus = state.Flight.FacingDirection == RidleyFacing.Right ? deltaX >= 0 : deltaX < 0;
        if (state.Flight.HitRoomBoundary ||
            (passedSamus && Math.Abs(deltaX) >= RidleyLungeDefinitions.PassDistance) ||
            place.Position.Y.Whole + RidleyClawOffsets.Below.For(RidleyClawHeight.Standing) >= samus.Position.Y.Whole)
        {
            HandleNorfairRidleyMissedLunge(place, state, powerBomb);
            return;
        }

        Offset16<RoomPixels> besideSamus = state.Flight.FacingDirection != RidleyFacing.Left
            ? NorfairRidleyArenaDefinitions.LungeAimBesideSamus
            : NorfairRidleyArenaDefinitions.LungeAimBesideSamus.MirroredX();
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state.Flight,
            samus.Position.Whole + besideSamus,
            state.HealthBand.GrabDivisor);

        if (RidleyClawOverlapsSamus(place, state, samus, NorfairRidleyArenaDefinitions.LungeClawReach))
        {
            // The grabbing lunge reverses its vertical speed before the carry accelerates.
            state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y.Negated());
            BeginNorfairRidleyGrab(place, state, samus);
        }
    }

    /// <summary>Locks Samus to the claw and enters carry or the zero-health death approach.</summary>
    private static void BeginNorfairRidleyGrab(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus)
    {
        state.GrabOffset = samus.Position.Whole - NorfairRidleyClaw(place, state);
        state.GrabState = RidleyGrabState.CarryingSamus;
        samus.Control.LockStationary();
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        if (place.Health == 0)
        {
            StartNorfairRidleyDeathSequence(place, state);
            // At zero health the grab moves him on toward his death spot in this same update.
            TickNorfairRidleyMoveToDeathSpot(place, state);
            return;
        }
        // The carry starts on the grabbing update.
        BeginNorfairRidleyCarry(place, state);
    }

    /// <summary>Chooses a facing-dependent carry anchor and starts Ridley's move, a countdown whose first step is this update.</summary>
    internal static void BeginNorfairRidleyCarry(RoomEnemyPlace place, NorfairRidleyState state)
    {
        state.Target = new Position16<RoomPixels>(
            RidleyMovementTargets.CarryAnchor.For(state.Flight.FacingDirection),
            place.Position.Y.Whole < NorfairRidleyArenaDefinitions.CarryHighLineY
                ? NorfairRidleyArenaDefinitions.FlightLineY
                : place.Position.Y.Whole - NorfairRidleyArenaDefinitions.CarryLift);
        state.Phase = NorfairRidleyPhase.CarryMoveToAnchor;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.CarryToAnchorUpdates);
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, state.Target, NorfairRidleyMotionDefinitions.CarryDivisor);
        state.PhaseTimer.StepRunsOut();
    }

    /// <summary>Decays Samus's grab offset and positions her at Ridley's moving claw.</summary>
    internal static void UpdateNorfairRidleyGrabbedSamus(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus)
    {
        state.GrabOffset = new Offset16<RoomPixels>(
            DecayRidleyGrabOffset(state.GrabOffset.X),
            DecayRidleyGrabOffset(state.GrabOffset.Y));
        samus.Position = samus.Position.WithWhole(NorfairRidleyClaw(place, state) + state.GrabOffset);
    }

    /// <summary>Moves one signed grab-offset component its decay toward zero.</summary>
    private static int DecayRidleyGrabOffset(int offset)
    {
        if (offset > 0)
            return Math.Max(0, offset - NorfairRidleyMotionDefinitions.GrabOffsetDecay);
        if (offset < 0)
            return Math.Min(0, offset + NorfairRidleyMotionDefinitions.GrabOffsetDecay);
        return 0;
    }

    /// <summary>Tests Samus's radius against the facing and foot-frame adjusted claw point.</summary>
    private static bool RidleyClawOverlapsSamus(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        Extent16<RoomPixels> radius)
    {
        Offset16<RoomPixels> fromClaw = samus.Position.Whole - NorfairRidleyClaw(place, state);
        return Math.Abs(fromClaw.X) < samus.Kinematics.Radius.X + radius.X &&
            Math.Abs(fromClaw.Y) < samus.Kinematics.Radius.Y + radius.Y;
    }

    /// <summary>Ridley's claw: his body's place moved by the claw offsets for the way he faces and his feet's height.</summary>
    private static Position16<RoomPixels> NorfairRidleyClaw(RoomEnemyPlace place, NorfairRidleyState state) =>
        place.Position.Whole + new Offset16<RoomPixels>(
            RidleyClawOffsets.Across.For(state.Flight.FacingDirection),
            RidleyClawOffsets.Below.For(state.Flight.ClawHeight));

    /// <summary>Starts a turn only when Ridley faces away from the half of his arena he lies in.</summary>
    internal static void SelectNorfairRidleyFacingAnimation(RoomEnemyPlace place, NorfairRidleyState state)
    {
        if (state.Flight.FacingDirection == RidleyFacing.Turning)
            return;
        // Keep an inward facing actor's current animation and timer untouched.
        bool rightHalf = place.Position.X.Whole >= NorfairRidleyArenaDefinitions.CentreX;
        if (state.Flight.FacingDirection == RidleyFacing.Left ? rightHalf : !rightHalf)
            return;
        RidleyFlight.StartAnimationAfter(
            state.Flight,
            place,
            state.Flight.FacingDirection == RidleyFacing.Left
                ? RidleyBodySegment.TurnFromLeftToRight
                : RidleyBodySegment.TurnFromRightToLeft,
            NorfairRidleyMotionDefinitions.TurnStartUpdates);
    }

    /// <summary>The hover acceleration divisor of Ridley's health band.</summary>
    private static int ReadRidleyHealthMovementDivisor(NorfairRidleyState state) => state.HealthBand.HoverDivisor;

    /// <summary>Checks the validated pose movement type against Ridley's grabbable states.</summary>
    internal static bool SamusMovementUsesRidleyGrab(SamusState samus)
    {
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
        return RidleyGrabs.CanGrab(movement);
    }

    /// <summary>
    /// Lunges when a power bomb is active and Ridley is not holding Samus, run before the phase on
    /// every main and even hurt frame. It reads the power-bomb state, which is set as soon as a power bomb is
    /// laid, so Ridley lunges in the same frame Samus lays one.
    /// </summary>
    internal static void PrepareNorfairRidleyCombatFrame(
        NorfairRidleyState state,
        SamusBombProjectileSystem samusBombs)
    {
        if (state.FightMode is RidleyFightMode.NotFighting or RidleyFightMode.Dying or RidleyFightMode.PowerBombDodge)
            return;
        if (!samusBombs.PowerBombExplosion.IsArmed || state.GrabState != RidleyGrabState.Released)
            return;

        // The power-bomb lunge resets both tail controls before the lunge.
        state.Tail.Action = RidleyTailAction.Neutral;
        state.Tail.SwingStep = RidleyTailDefinitions.SlowNeutralSwing;
        state.Phase = NorfairRidleyPhase.GrabApproach;
    }

    /// <summary>On zero health, makes Ridley intangible and enters the Samus-release death state.</summary>
    private static void StartNorfairRidleyDeathSequence(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        if (body.Health != 0 || state.FightMode == RidleyFightMode.Dying)
            return;

        state.FightMode = RidleyFightMode.Dying;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Phase = NorfairRidleyPhase.MoveToDeathSpot;
    }

    /// <summary>
    /// Keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the small rectangle centered on his death spot. Reaching it goes straight on to
    /// the roar setup instead of spending an extra transition frame.
    /// </summary>
    internal static void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state.Flight, NorfairRidleyArenaDefinitions.DeathSpot,
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (RidleyFlight.IsWithinRectangle(body, NorfairRidleyArenaDefinitions.DeathSpot, NorfairRidleyArenaDefinitions.DeathSpotReach))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Selects the death-roar animation and arms its move's countdown.</summary>
    internal static void BeginNorfairRidleyDeathRoar(
        RoomEnemyPlace body,
        NorfairRidleyState state)
    {
        RidleyFlight.StartAnimation(state.Flight, body, RidleyAnimations.DeathRoar);
        state.Phase = NorfairRidleyPhase.DeathRoar;
        state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.DeathRoarUpdates);
    }

    /// <summary>Handles a missed lunge: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private static void HandleNorfairRidleyMissedLunge(RoomEnemyPlace place, NorfairRidleyState state, SamusPowerBombExplosionState powerBomb)
    {
        if (state.ZeroHealthLungeCount >= RidleyLungeDefinitions.DeathLungeLimit)
        {
            StartNorfairRidleyDeathSequence(place, state);
            BeginNorfairRidleyDeathRoar(place, state);
            return;
        }
        if (powerBomb.IsArmed)
        {
            state.Phase = NorfairRidleyPhase.PowerBombDodge;
            TickNorfairRidleyPowerBombDodge(place, state, powerBomb);
            return;
        }
        state.Phase = NorfairRidleyPhase.HoverSetup;
        state.Tail.Whip.Give();
    }

    /// <summary>Recovers from a missed lunge with a Power Bomb out.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="bomb">The power bomb explosion of the pass's bombs, which Ridley dodges while it is armed.</param>
    internal static void TickNorfairRidleyPowerBombDodge(RoomEnemyPlace place, NorfairRidleyState state, SamusPowerBombExplosionState bomb)
    {
        if (!bomb.IsArmed)
        {
            state.FightMode = RidleyFightMode.Fighting;
            state.Phase = state.GrabState == RidleyGrabState.Released
                ? NorfairRidleyPhase.SelectAttack : NorfairRidleyPhase.CarrySetup;
            return;
        }
        state.FightMode = RidleyFightMode.PowerBombDodge;
        Position16<RoomPixels> target = new(
            bomb.Position.X >= RidleyLungeDefinitions.DodgeSplitX
                ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX,
            bomb.Position.Y >= RidleyLungeDefinitions.DodgeSplitY
                ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, target, ReadRidleyHealthMovementDivisor(state));
    }

    /// <summary>
    /// Chooses the next bounce's side from position and the current seed: a bounce that left him no sideways speed heads
    /// for the arena's centre, one past his movement bounds heads back inside them, and otherwise the seed decides whether
    /// the bounce heads toward Samus or away.
    /// </summary>
    private void SetRidleyPogoHorizontalDirection(RoomEnemyPlace place, NorfairRidleyState state, SamusState samus)
    {
        int x = place.Position.X.Whole;
        if (state.Flight.Velocity.X.IsZero)
            state.PogoBounce = x < NorfairRidleyArenaDefinitions.CentreX ? HorizontalDirection.Right : HorizontalDirection.Left;
        bool reverse;
        if (x < state.Flight.MovementMinimum.X) reverse = state.PogoBounce == HorizontalDirection.Left;
        else if (x >= state.Flight.MovementMaximum.X) reverse = state.PogoBounce == HorizontalDirection.Right;
        else
        {
            HorizontalDirection towardSamus = x < samus.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left;
            bool headsAway = RidleyRandomDefinitions.PogoBounceHeadsAwayFromSamus.IsTakenBy(_room.Random.Read());
            reverse = state.PogoBounce == towardSamus ? headsAway : !headsAway;
        }
        if (reverse) state.PogoBounce = state.PogoBounce.Reversed();
    }
}
