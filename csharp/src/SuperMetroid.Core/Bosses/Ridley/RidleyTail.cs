using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's tail: the segment chain, its flight and pogo swings, whip aiming,
/// terrain probing, Samus contact, and the projectile-blocking joints shared by the Ceres and
/// Lower Norfair encounters.
/// </summary>
[RoomLifetime]
internal sealed class RidleyTail
{
    /// <summary>Replaces shots blocked by tail joints with dust or dud effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Provides the live random number sampled by Ceres curl and pogo whip decisions.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Connects tail kinematics and collision to shared room and projectile state.</summary>
    internal RidleyTail(EnemyProjectileEngine projectiles, EnemyRoomBinding room)
    {
        _projectiles = projectiles;
        _room = room;
    }

    /// <summary>
    /// Moves the tail: the neutral tail's swing, the segment update,
    /// and the position/distance chains. The swing deliberately
    /// adds the preceding segment's direction when deriving every later offset;
    /// retaining that documented anomaly is why the tail curls and rotates at the game's
    /// rate.
    /// </summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the tail follows and whips at.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles, a missile among which the Ceres whip may aim at instead.</param>
    internal void TickRidleyTail(
        RoomEnemyPlace place,
        RidleyEncounterState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        if (state.Tail.Action != RidleyTailAction.None)
        {
            state.Tail.MinimumClockwiseAngle = RidleyTailReach.MinimumClockwise(state.Flight.FacingDirection);
            state.Tail.MaximumCounterClockwiseAngle = RidleyTailReach.MaximumCounterClockwise(state.Flight.FacingDirection);

            if (state.Tail.Action == RidleyTailAction.Neutral)
            {
                HandleRidleyNeutralTailControl(place, state, samus, samusProjectiles);
                foreach (RidleyTailLink link in state.Tail.Links)
                    TickRidleyTailLink(state.Tail, link);
            }
            else
                TickRidleyPogoTail(place, state, samus);
        }

        // The tail composition always composes positions, but only the selected tail action
        // updates offsets. With no tail action, and on the stagger and stop returns, the offsets stay.
        RidleyTailLink tailBase = state.Tail.Base;
        Position16<RoomPixels> tailRoot = place.Position.Whole + RidleyTailDefinitions.HipBelowBody;
        tailBase.Position = state.Flight.FacingDirection switch
        {
            RidleyFacing.Left => tailRoot + RidleyTailDefinitions.HipBehindBodyFacingLeft + tailBase.Offset,
            RidleyFacing.Turning => (tailRoot + tailBase.Offset).WithX(place.Position.X.Whole),
            RidleyFacing.Right => tailRoot + RidleyTailDefinitions.HipBehindBodyFacingLeft.MirroredX() + tailBase.Offset,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(RidleyFacing)} {state.Flight.FacingDirection}."),
        };

        for (RidleyTailLink previous = tailBase; previous.Next is { } link; previous = link)
        {
            Position16<RoomPixels> chained = previous.Position + link.Offset;
            link.Position = state.Flight.FacingDirection == RidleyFacing.Turning
                ? chained.WithX(place.Position.X.Whole)
                : chained;
        }

        UpdateRidleyTailDistances(state.Tail);
        ResolveRidleyTailSamusContact(place, state, samus);
    }

    /// <summary>
    /// Runs Ridley's tail whip and its fling trigger. Ceres
    /// disables random/proximity flings only during the swoop, then sets a one-shot request
    /// at its lowest point so the tail snaps toward Samus as Ridley charges back upward.
    /// </summary>
    internal void HandleRidleyNeutralTailControl(
        RoomEnemyPlace place,
        RidleyEncounterState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        bool allActive = state.Tail.Links.All(link => link.Active);
        if (allActive)
        {
            bool targetsIdle = state.Tail.ClockwiseWhipTarget is null && state.Tail.CounterClockwiseWhipTarget is null;
            if (state.Tail.WhipRequested && targetsIdle)
            {
                AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: 0);
            }
            else if (state.Tail.IdleWhipEnabled)
            {
                // An idle whip reads the random number as it stands and never advances it.
                // An idle random whip with no request pending aims one whole turn short of a requested one.
                if (RidleyRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()))
                    AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: state.Tail.WhipRequested ? 0 : -1);
                else if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < RidleyTailDefinitions.WhipRange)
                    AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: 0);
            }

            state.Tail.WhipRequested = false;
        }

        if (state.Tail.Links.Any(link => link.Active))
            return;

        // Once a wave/whip has propagated through every link, neutral AI starts the next wave at the base and retires
        // both old targets. Where Samus stands selects the slow or fast neutral swing.
        state.Tail.Base.Active = true;
        state.Tail.ClockwiseWhipTarget = null;
        state.Tail.CounterClockwiseWhipTarget = null;
        state.Tail.SwingStep = samus.Position.X.Whole < RidleyTailDefinitions.FastNeutralSwingFromX
            ? RidleyTailDefinitions.SlowNeutralSwing
            : RidleyTailDefinitions.FastNeutralSwing;
    }

    /// <summary>
    /// Aims the Ceres tail at Samus or a nearby missile, clamping targets that would pass through Ridley's body. The
    /// target's direction is counted in the turn ending at straight down for a left-facing (clockwise) whip and in the
    /// turn starting there for a right-facing (counterclockwise) one, then moved <paramref name="turnsBeyond"/> whole
    /// turns further along the whip's swing; a negative count lies that many turns short. Only the order against the
    /// root's angle and the turns the whip sweeps change: the target still points the same way.
    /// </summary>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the whip aims at.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles; a missile near the tail tip draws the whip instead.</param>
    /// <param name="turnsBeyond">The whole turns the target lies past the whip's swing, or short of it when negative.</param>
    internal static void AimRidleyTailWhip(
        RidleyEncounterState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles,
        int turnsBeyond)
    {
        // Ridley turning to face the screen aims no whip, and the fight's opening mode likewise blocks the whip before
        // liftoff.
        if (state.Flight.FacingDirection == RidleyFacing.Turning || state.FightMode == RidleyFightMode.NotFighting)
            return;

        RidleyTailLink root = state.Tail.Base;
        Position16<RoomPixels> aim = samus.Position.Whole + RidleyTailDefinitions.WhipAimOnSamus;
        RidleyTailLink tip = state.Tail.Tip;
        SamusProjectile? missile = RidleyTailMissileTarget.Target(
            samusProjectiles.Pool.ShotCount.Count,
            samusProjectiles.Shots,
            projectile => projectile.Type.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile &&
                Math.Abs((projectile.Position.Whole - tip.Position).X) - projectile.Radius.X < RidleyTailDefinitions.MissileWhipReach &&
                Math.Abs((projectile.Position.Whole - tip.Position).Y) - projectile.Radius.Y < RidleyTailDefinitions.MissileWhipReach);
        if (missile is not null)
            aim = missile.Position.Whole;
        TrigonometryAngle targetDirection = EnemyMath.CalculateBearing(aim - root.Position).ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);

        if (state.Flight.FacingDirection == RidleyFacing.Left)
        {
            // Facing-left Ridley cannot aim through his own body. Directions inside the
            // forbidden forward interval clamp to its end.
            if (RidleyTailReach.AimsThroughBody(targetDirection))
                targetDirection = RidleyTailDefinitions.ForbiddenForwardEnd;
            RidleyTailAngle target = RidleyTailDefinitions.LeftWhipTargetBase
                .TurnedByWholeTurns(-turnsBeyond)
                .TurnedCounterclockwise(targetDirection);
            if (target < root.Angle)
            {
                state.Tail.ClockwiseWhipTarget = target;
                state.Tail.SwingStep = RidleyTailDefinitions.WhipSwing;
            }
        }
        else
        {
            if (RidleyTailReach.AimsThroughBody(targetDirection))
                targetDirection = RidleyTailDefinitions.ForbiddenForwardStart;
            RidleyTailAngle target = RidleyTailDefinitions.RightWhipTargetBase
                .TurnedByWholeTurns(turnsBeyond)
                .TurnedCounterclockwise(targetDirection);
            if (target >= root.Angle)
            {
                state.Tail.CounterClockwiseWhipTarget = target;
                state.Tail.SwingStep = RidleyTailDefinitions.WhipSwing;
            }
        }
    }

    /// <summary>
    /// Advances one link's stagger and swing toward the shared target. In both directions a step that lands exactly on
    /// the limit or target counts as reaching it. A link that has swung its spacing starts the next link swinging its way.
    /// </summary>
    internal static void TickRidleyTailLink(RidleyTailState tail, RidleyTailLink link)
    {
        if (link.Active)
        {
            if (link.Stagger < tail.IdealLinkSpacing)
            {
                link.Stagger += tail.SwingStep;
                return;
            }

            if (link.Stagger is not null)
            {
                link.Stagger = null;
                if (link.Next is { } next)
                {
                    next.Active = true;
                    next.SwingDirection = link.SwingDirection;
                }
            }

            RotationDirection direction = link.SwingDirection;
            RidleyTailAngle swung = link.Angle.Turned(direction, tail.SwingStep);
            RidleyTailAngle limit = tail.SwingLimit(direction);
            if (tail.WhipTarget(direction) is { } target)
            {
                link.TargetDistance = RidleyTailDefinitions.WhipDistance;
                if (swung.Reaches(target, direction))
                {
                    // The leading link of the whip stops on its target; a link whose predecessor still swings turns
                    // back from the limit instead.
                    if (link.Previous is not { Active: true })
                    {
                        link.Angle = target;
                        DeactivateRidleyTailLink(link);
                        return;
                    }
                    link.SwingDirection = Reversed(direction);
                    link.Angle = limit;
                }
                else
                {
                    link.Angle = swung;
                }
            }
            else if (swung.Reaches(limit, direction))
            {
                link.SwingDirection = Reversed(direction);
                link.Angle = limit;
            }
            else
            {
                link.Angle = swung;
            }
        }

        UpdateRidleyTailLinkOffset(link);
    }

    /// <summary>The other way round from <paramref name="direction"/>.</summary>
    private static RotationDirection Reversed(RotationDirection direction) =>
        direction == RotationDirection.Clockwise ? RotationDirection.Counterclockwise : RotationDirection.Clockwise;

    /// <summary>
    /// Converts one link's current polar angle/distance into the relative Cartesian offset the tail composition consumes.
    /// This is deliberately independent of the link's active flag: inactive links keep their established pose rather
    /// than collapsing.
    /// </summary>
    private static void UpdateRidleyTailLinkOffset(RidleyTailLink link)
    {
        TrigonometryAngle angle = link.PointingDirection;
        int distanceInPixels = link.Distance.WholePixels;
        // Each link's offset is its rotated distance in whole pixels, rounded down:
        // a negative fractional pixel rounds toward the more negative whole pixel.
        link.Offset = new Offset16<RoomPixels>(
            EightEight.Multiply(distanceInPixels, Trigonometry.SignedSine(angle)),
            EightEight.Multiply(distanceInPixels, Trigonometry.SignedSine(angle.TurnedBy(TrigonometryAngle.QuarterTurn))));
    }

    /// <summary>Ends a link's active whip and flips its swing for the next activation.</summary>
    private static void DeactivateRidleyTailLink(RidleyTailLink link)
    {
        link.Active = false;
        link.Stagger = RidleyTailTurn.None;
        link.SwingDirection = Reversed(link.SwingDirection);
    }

    /// <summary>
    /// Extends each shaft link and the tip toward its target distance, up to its longest, or shrinks one longer than its
    /// rest length back toward it. The base holds its length and takes no part.
    /// </summary>
    internal static void UpdateRidleyTailDistances(RidleyTailState tail)
    {
        foreach (RidleyTailLink link in tail.Links.Skip(1))
        {
            if (link.TargetDistance is { } target)
            {
                if (target < link.Distance)
                    link.TargetDistance = null;
                // Every link's longest is under 25 pixels, so the extended length stays far inside its range.
                link.Distance = SignedEightEight.Min(link.LongestDistance, link.Distance + tail.ExtensionSpeed);
            }
            else if (link.Distance > link.RestDistance)
            {
                // Every rest length is at least two pixels, so a shrinking link never passes zero.
                link.Distance -= RidleyTailDefinitions.ShrinkStep;
            }
        }
    }

    /// <summary>
    /// Randomly updates the tail's curliness: when the live random number falls in its roll's top outcome (sampled, not
    /// advanced), the ideal link spacing becomes the least curly spacing plus the curliness roll, so eight through 23 steps.
    /// </summary>
    internal void RandomlyUpdateCeresRidleyTailCurliness(RidleyTailState tail)
    {
        int random = _room.Random.Read();
        if (!RidleyRandomDefinitions.TailCurlinessChanges.IsTakenBy(random))
            return;
        tail.IdealLinkSpacing = RidleyTailDefinitions.LeastCurlyLinkSpacing + RidleyTailTurn.OfSteps(RidleyRandomDefinitions.TailCurliness.Of(random));
    }

    /// <summary>Applies tail-tip contact damage after joint positions are solved, unless Ridley carries Samus or collision is disabled.</summary>
    /// <param name="body">Ridley's body, whose contact decides whether the tail meets Samus.</param>
    /// <param name="state">Ridley's encounter.</param>
    /// <param name="samus">Samus.</param>
    internal static bool ResolveRidleyTailSamusContact(RoomEnemyPlace body, RidleyEncounterState state, SamusState samus)
    {
        // The tail composition checks the newly solved tail, independently of the earlier
        // extended-body contact pass, and never damages Samus while carrying her.
        if (state.CarriesSamus || !samus.InvincibilityTimer.IsZero ||
            body.Contact.HasAny(EnemyContact.IgnoresSamus) ||
            !state.MovementAnimationEnabled)
            return false;
        RidleyTailLink tip = state.Tail.Tip;
        // Ceres Ridley's retreat carries his tail above his chamber, so the tip is measured by its displacement from Samus.
        Offset16<RoomPixels> fromTip = samus.Position.Whole - tip.Position;
        int dx = Math.Abs(fromTip.X);
        int dy = Math.Abs(fromTip.Y);
        if (dx >= samus.Kinematics.Radius.X + RidleyTailDefinitions.TipTouchReach || dy >= samus.Kinematics.Radius.Y + RidleyTailDefinitions.TipTouchReach)
            return false;
        EnemyCollision.ApplyNormalEnemyTouchDamage(samus, state.Tail.Damage, tip.Position.X);
        return true;
    }

    /// <summary>
    /// Probes the tip and then the four shaft links nearest it, from the tip inward, at their downward offsets for solid
    /// room blocks.
    /// </summary>
    internal static bool RidleyTailTouchesTerrain(
        RidleyTailState tail,
        RoomLevelData level)
    {
        // The tail's floor check samples the tip 16 pixels below it and each shaft link 18 pixels below it.
        if (ProbeTouchesTerrain(tail.Tip.Position + RidleyTailDefinitions.TipFloorProbe, level))
            return true;
        foreach (RidleyTailLink link in tail.Shaft.Reverse().Take(RidleyTailDefinitions.FloorProbingShaftLinks))
        {
            if (ProbeTouchesTerrain(link.Position + RidleyTailDefinitions.ShaftFloorProbe, level))
                return true;
        }
        return false;
    }

    /// <summary>Whether <paramref name="probe"/> lies on a solid block inside the room; a probe outside the room touches nothing.</summary>
    private static bool ProbeTouchesTerrain(Position16<RoomPixels> probe, RoomLevelData level) =>
        RoomGridConversions.PixelToBlock(probe.X) < level.Size.X && RoomGridConversions.PixelToBlock(probe.Y) < level.Size.Y &&
        level.GetCollisionBlockAtPixel(probe).CollisionType != RoomCollisionType.Air;

    /// <summary>
    /// Mirrors the tail. A facing step restores every link's rest length before reflecting its angle about straight down
    /// and setting it swinging clockwise. The offsets remain untouched until the tail's following update.
    /// </summary>
    internal static void MirrorRidleyTail(RidleyTailState tail)
    {
        foreach (RidleyTailLink link in tail.Links)
        {
            link.Distance = link.RestDistance;
            link.Angle = link.Angle.Mirrored();
            link.SwingDirection = RotationDirection.Clockwise;
        }
    }

    /// <summary>
    /// Resolves shots against the tail. Only the tip and the inner joint block shots: a radius-14 tip followed by a
    /// radius-10 inner joint. A hit is snapped to the joint,
    /// marked consumed, and replaced by the game's dust/dud actor; Ridley's health is
    /// never consulted because the tail is armor rather than another body hitbox.
    /// </summary>
    internal void ResolveRidleyTailProjectileHits(
        RoomEnemyPlace body,
        RidleyTailState tail,
        SamusProjectileSystem projectiles)
    {
        if (body.Contact.HasAny(EnemyContact.IgnoresSamus))
            return;

        if (TryBlockProjectileAtRidleyTailJoint(
                tail.Tip,
                radius: RidleyTailDefinitions.TipArmourReach,
                projectiles))
        {
            return;
        }

        _ = TryBlockProjectileAtRidleyTailJoint(
            tail.InnerJoint,
            radius: RidleyTailDefinitions.InnerJointArmourReach,
            projectiles);
    }

    /// <summary>Consumes the first colliding ordinary beam or missile at one armored joint and spawns its impact effect.</summary>
    private bool TryBlockProjectileAtRidleyTailJoint(
        RidleyTailLink joint,
        int radius,
        SamusProjectileSystem projectiles)
    {
        // Samus's shots are checked in shot-place order: beams, missiles and Super Missiles count; bombs, Power Bombs
        // and shots already exploding do not.
        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            if (!projectile.HasEnemyCollisionPayload)
            {
                continue;
            }

            Offset16<RoomPixels> fromJoint = projectile.Position.Whole - joint.Position;
            int xDistance = Math.Abs(fromJoint.X);
            int yDistance = Math.Abs(fromJoint.Y);
            if (xDistance >= projectile.Radius.X + radius ||
                yDistance >= projectile.Radius.Y + radius)
            {
                continue;
            }

            projectile.Position = projectile.Position.WithWhole(joint.Position);
            projectile.IsSpent = true;

            // A missile gets the tiny dud and the dud-shot sound. Beams and supers
            // use the larger smoke variant, exactly as the tail's shot reaction selects it.
            bool missile = projectile.Type.Family == SamusProjectileFamily.Missile;
            DustExplosionAnimation dustVariant = missile ? DustExplosionAnimation.DudShotTinyExplosion : DustExplosionAnimation.Smoke;
            _projectiles.SpawnDustOrExplosion(joint.Position, dustVariant);
            if (missile)
                _projectiles.QueueDudSound(SoundRequestPriority.Normal);
            return true;
        }

        return false;
    }

    /// <summary>Runs the live pogo tail.</summary>
    internal void TickRidleyPogoTail(RoomEnemyPlace place, RidleyEncounterState state, SamusState samus)
    {
        RidleyTailAction action = state.Tail.Action;
        bool pointed = action is RidleyTailAction.PogoSetup or RidleyTailAction.PointDown or RidleyTailAction.Stab;
        if (!pointed && action is not (RidleyTailAction.Pogo or RidleyTailAction.StabSetup))
            throw new InvalidDataException($"Ridley tail has no behaviour for action {action}.");

        state.Tail.MinimumClockwiseAngle = RidleyTailReach.MinimumClockwise(state.Flight.FacingDirection);
        state.Tail.MaximumCounterClockwiseAngle = RidleyTailReach.MaximumCounterClockwise(state.Flight.FacingDirection);
        if (pointed)
        {
            state.Tail.SwingStep = action == RidleyTailAction.Stab ? RidleyTailDefinitions.StabSwing : RidleyTailDefinitions.WhipSwing;
            state.Tail.ClockwiseWhipTarget = state.Flight.FacingDirection == RidleyFacing.Left ? null : RidleyTailDefinitions.DownAngle;
            state.Tail.CounterClockwiseWhipTarget = state.Flight.FacingDirection == RidleyFacing.Left ? RidleyTailDefinitions.DownAngle : null;
        }
        HandleRidleyPogoTailControl(place, state, samus, action == RidleyTailAction.StabSetup);
        foreach (RidleyTailLink link in state.Tail.Links)
            TickRidleyTailLink(state.Tail, link);
<<<<<<< HEAD
        if (pointed && !state.Tail.Links.Any(link => link.Active))
            state.Tail.Action = RidleyTailAction.Pogo;
        // The tail's setup and stab take precedence over the pointed tail's choice.
||||||| ccc5bcd718
        if (pointed && !state.Tail.Links.Any(link => link.Active))
            state.Tail.Action = RidleyTailAction.Pogo;
        // The tail's setup and stab overwrite the result of the shared pointed-tail step.
=======
        // The tail's setup points it down next and a stab keeps stabbing, whatever the step did; a tail pointing down
        // becomes a pogo once none of its links is active.
>>>>>>> worktree-agent-a263abbbd85c36f1d
        if (action == RidleyTailAction.PogoSetup)
            state.Tail.Action = RidleyTailAction.PointDown;
        else if (action == RidleyTailAction.Stab)
            state.Tail.Action = RidleyTailAction.Stab;
        else if (action == RidleyTailAction.PointDown && !state.Tail.Links.Any(link => link.Active))
            state.Tail.Action = RidleyTailAction.Pogo;
    }

    /// <summary>Triggers pogo whips from random/proximity gates and extends the tail downward as Ridley descends.</summary>
    private void HandleRidleyPogoTailControl(RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, bool stabbing)
    {
        if (state.Tail.Links.All(link => link.Active) &&
            (RidleyRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()) ||
             Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < RidleyTailDefinitions.WhipRange) &&
            state.Tail.ClockwiseWhipTarget is null && state.Tail.CounterClockwiseWhipTarget is null)
        {
            state.Tail.ClockwiseWhipTarget = RidleyTailDefinitions.PogoWhipAngle;
            state.Tail.SwingStep = RidleyTailDefinitions.WhipSwing;
            return;
        }
        if (state.Tail.Links.Any(link => link.Active)) return;
        if (!state.Flight.Velocity.Y.IsNegative)
        {
            state.Tail.Action = stabbing ? RidleyTailAction.Stab : RidleyTailAction.StabSetup;
            if (stabbing)
            {
                foreach (RidleyTailLink link in state.Tail.Links)
                {
                    link.TargetDistance = RidleyTailDefinitions.StabDistance;
                    link.Angle = RidleyTailDefinitions.DownAngle;
                }
                state.Tail.Base.Active = true;
            }
        }
        state.Tail.ClockwiseWhipTarget = null;
        state.Tail.CounterClockwiseWhipTarget = null;
        state.Tail.SwingStep = RidleyTailDefinitions.PogoSwing;
    }
}
