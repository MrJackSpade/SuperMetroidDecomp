using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's tail: the segment chain, its flight and pogo controllers, whip aiming,
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
                for (int index = 0; index < state.Tail.Segments.Length; index++)
                    TickRidleyTailSegment(state.Tail, index);
            }
            else
                TickRidleyPogoTail(place, state, samus);
        }

        // The tail composition always composes positions, but only the selected tail controller
        // updates offsets. With no tail action, and on the stagger and stop returns, the offsets stay.
        RidleyTailSegment first = state.Tail.Segments[0];
        Position16<RoomPixels> tailRoot = place.Position.Whole + new Offset16<RoomPixels>(0, 16);
        first.Position = state.Flight.FacingDirection switch
        {
            RidleyFacing.Left => tailRoot + new Offset16<RoomPixels>(32, 0) + first.Offset,
            RidleyFacing.Turning => (tailRoot + first.Offset).WithX(place.Position.X.Whole),
            RidleyFacing.Right => tailRoot + new Offset16<RoomPixels>(-32, 0) + first.Offset,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(RidleyFacing)} {state.Flight.FacingDirection}."),
        };

        for (int index = 1; index < state.Tail.Segments.Length; index++)
        {
            RidleyTailSegment previous = state.Tail.Segments[index - 1];
            RidleyTailSegment current = state.Tail.Segments[index];
            Position16<RoomPixels> chained = previous.Position + current.Offset;
            current.Position = state.Flight.FacingDirection == RidleyFacing.Turning
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
        bool allActive = state.Tail.Segments.All(segment => segment.Active);
        if (allActive)
        {
            bool targetsIdle = state.Tail.ClockwiseWhipTarget is null && state.Tail.CounterClockwiseWhipTarget is null;
            if (state.Tail.WhipRequested && targetsIdle)
            {
                AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: 0);
            }
            else if (state.Tail.IdleWhipEnabled)
            {
                // Read the existing seed; this branch never advances the RNG.
                // An idle random whip with no request pending aims one whole turn short of a requested one.
                if (RidleyRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()))
                    AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: state.Tail.WhipRequested ? 0 : -1);
                else if (CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) < 128)
                    AimRidleyTailWhip(state, samus, samusProjectiles, turnsBeyond: 0);
            }

            state.Tail.WhipRequested = false;
        }

        if (state.Tail.Segments.Any(segment => segment.Active))
            return;

        // Once a wave/whip has propagated through all seven parts, neutral AI starts the
        // next wave at the base and retires both old targets. Samus's X half selects the
        // exact one- or two-unit angular delta the neutral tail uses.
        state.Tail.Segments[0].Active = true;
        state.Tail.ClockwiseWhipTarget = null;
        state.Tail.CounterClockwiseWhipTarget = null;
        state.Tail.AngleDelta = samus.Position.X.Whole < 112 ? 1 : 2;
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

        RidleyTailSegment root = state.Tail.Segments[0];
        Position16<RoomPixels> aim = samus.Position.Whole + new Offset16<RoomPixels>(0, 24);
        RidleyTailSegment tip = state.Tail.Segments[6];
        SamusProjectile? missile = RidleyTailMissileTarget.Target(
            samusProjectiles.Pool.ShotCount.Count,
            samusProjectiles.Shots,
            projectile => projectile.Type.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile &&
                CoordinateAxis.Distance(projectile.Position.X.Whole, tip.Position.X) - projectile.Radius.X < 64 &&
                CoordinateAxis.Distance(projectile.Position.Y.Whole, tip.Position.Y) - projectile.Radius.Y < 64);
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
                state.Tail.AngleDelta = 8;
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
                state.Tail.AngleDelta = 8;
            }
        }
    }

    /// <summary>
    /// Advances one joint's stagger angle and active whip direction toward the shared target. In both directions a step
    /// that lands exactly on the limit or target counts as reaching it.
    /// </summary>
    internal static void TickRidleyTailSegment(RidleyTailState tail, int index)
    {
        RidleyTailSegment segment = tail.Segments[index];
        if (segment.Active)
        {
            if (segment.StaggerAngle < tail.IdealInterSegmentAngle)
            {
                segment.StaggerAngle += tail.AngleDelta;
                return;
            }

            if (segment.StaggerAngle is not null)
            {
                segment.StaggerAngle = null;
                if (index + 1 < tail.Segments.Length)
                {
                    tail.Segments[index + 1].Active = true;
                    tail.Segments[index + 1].Clockwise = segment.Clockwise;
                }
            }

            if (segment.Clockwise)
            {
                RidleyTailAngle next = segment.Angle.TurnedClockwise(tail.AngleDelta);
                if (tail.ClockwiseWhipTarget is { } clockwiseTarget)
                {
                    segment.TargetDistance = RidleyTailDefinitions.WhipDistance;
                    if (next <= clockwiseTarget)
                    {
                        if (index == 0 || !tail.Segments[index - 1].Active)
                        {
                            segment.Angle = clockwiseTarget;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.Clockwise = false;
                        segment.Angle = tail.MinimumClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = next;
                    }
                }
                else
                {
                    if (next <= tail.MinimumClockwiseAngle)
                    {
                        segment.Clockwise = false;
                        segment.Angle = tail.MinimumClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = next;
                    }
                }
            }
            else
            {
                RidleyTailAngle next = segment.Angle.TurnedCounterclockwise(tail.AngleDelta);
                if (tail.CounterClockwiseWhipTarget is { } counterClockwiseTarget)
                {
                    segment.TargetDistance = RidleyTailDefinitions.WhipDistance;
                    if (next >= counterClockwiseTarget)
                    {
                        if (index == 0 || !tail.Segments[index - 1].Active)
                        {
                            segment.Angle = counterClockwiseTarget;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.Clockwise = true;
                        segment.Angle = tail.MaximumCounterClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = next;
                    }
                }
                else
                {
                    if (next >= tail.MaximumCounterClockwiseAngle)
                    {
                        segment.Clockwise = true;
                        segment.Angle = tail.MaximumCounterClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = next;
                    }
                }
            }
        }

        UpdateRidleyTailSegmentOffset(tail, index);
    }

    /// <summary>
    /// Converts one segment's current polar angle/distance into the relative Cartesian
    /// offset the tail composition consumes. This is deliberately independent of the segment's active
    /// flag: inactive tail pieces keep their established pose rather than collapsing.
    /// </summary>
    private static void UpdateRidleyTailSegmentOffset(RidleyTailState tail, int index)
    {
        RidleyTailSegment segment = tail.Segments[index];
        TrigonometryAngle angle = segment.Angle.Direction;
        if (index != 0)
            angle = angle.TurnedBy(tail.Segments[index - 1].Angle.Direction);
        int distanceInPixels = segment.Distance.WholePixels;
        // Each segment's offset is its rotated distance in whole pixels, rounded down:
        // a negative fractional pixel rounds toward the more negative whole pixel.
        segment.Offset = new Offset16<RoomPixels>(
            CoordinateAxis.AsSigned(EightEight.Multiply(distanceInPixels, Trigonometry.SignedSine(angle))),
            CoordinateAxis.AsSigned(EightEight.Multiply(distanceInPixels, Trigonometry.SignedSine(angle.TurnedBy(TrigonometryAngle.QuarterTurn)))));
    }

    /// <summary>Ends a joint's active whip and flips its movement direction for the next activation.</summary>
    private static void DeactivateRidleyTailSegment(RidleyTailSegment segment)
    {
        segment.Active = false;
        segment.StaggerAngle = 0;
        segment.Clockwise = !segment.Clockwise;
    }

    /// <summary>Runs the six extend/shrink steps.</summary>
    internal static void UpdateRidleyTailDistances(RidleyTailState tail)
    {
        for (int index = 1; index < tail.Segments.Length; index++)
        {
            RidleyTailSegment segment = tail.Segments[index];
            if (segment.TargetDistance is { } target)
            {
                if (target < segment.Distance)
                    segment.TargetDistance = null;
                // Every whip maximum is under 25 pixels, so the extended length stays far inside its range.
                segment.Distance = SignedEightEight.Min(
                    RidleyTailReach.MaximumDistance(index),
                    segment.Distance + tail.ExtensionSpeed);
            }
            else if (segment.Distance > RidleyTailReach.RestDistance(index))
            {
                // Every rest length is at least two pixels, so a shrinking link never passes zero.
                segment.Distance -= RidleyTailDefinitions.ShrinkStep;
            }
        }
    }

    /// <summary>Creates the seven-link tail at its cataloged rest lengths and sequential initial angles.</summary>
    internal static RidleyTailSegment[] CreateInitialRidleyTailSegments()
    {
        // Each successive link begins one ideal angular separation beyond its predecessor.
        // The chosen base/shaft/tip lengths remain required inputs in the tail catalog.
        var segments = new RidleyTailSegment[7];
        for (int index = 0; index < segments.Length; index++)
        {
            segments[index] = new RidleyTailSegment
            {
                Clockwise = true,
                Distance = RidleyTailReach.RestDistance(index),
                Angle = RidleyTailReach.InitialAngle(index),
                StaggerAngle = 17,
            };
        }
        return segments;
    }

    /// <summary>
    /// Randomly updates the tail's curliness: when the live random number falls in its roll's top outcome (sampled, not
    /// advanced), the ideal inter-segment tail angle becomes its curliness roll plus eight, so 8..23.
    /// </summary>
    internal void RandomlyUpdateCeresRidleyTailCurliness(RidleyTailState tail)
    {
        int random = _room.Random.Read();
        if (!RidleyRandomDefinitions.TailCurlinessChanges.IsTakenBy(random))
            return;
        tail.IdealInterSegmentAngle = RidleyRandomDefinitions.TailCurliness.Of(random) + 8;
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
        RidleyTailSegment tip = state.Tail.Segments[^1];
        int dx = CoordinateAxis.Distance(samus.Position.X.Whole, tip.Position.X);
        int dy = CoordinateAxis.Distance(samus.Position.Y.Whole, tip.Position.Y);
        if (dx >= samus.Kinematics.Radius.X + 14 || dy >= samus.Kinematics.Radius.Y + 14)
            return false;
        EnemyCollision.ApplyNormalEnemyTouchDamage(samus, state.Tail.Damage, tip.Position.X);
        return true;
    }

    /// <summary>Probes the tip and four inner joints at their downward offsets for solid room blocks.</summary>
    internal static bool RidleyTailTouchesTerrain(
        RidleyTailState tail,
        RoomLevelData level)
    {
        // The tail's floor check samples the tip and the next four joints with the exact +16/+18 Y probes.
        int tipIndex = tail.Segments.Length - 1;
        for (int index = tipIndex; index >= 2; index--)
        {
            RidleyTailSegment segment = tail.Segments[index];
            Position16<RoomPixels> probe = segment.Position + new Offset16<RoomPixels>(0, index == tipIndex ? 16 : 18);
            if (RoomGridConversions.PixelToBlock(probe.X) >= level.Size.X || RoomGridConversions.PixelToBlock(probe.Y) >= level.Size.Y)
                continue;
            if (level.GetCollisionBlockAtPixel(probe).CollisionType != RoomCollisionType.Air)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Mirrors the tail. A facing step restores the seven rest distances before
    /// reflecting angles about straight down and setting clockwise movement. The offsets
    /// remain untouched until the following tail controller update.
    /// </summary>
    internal static void MirrorRidleyTail(RidleyTailState tail)
    {
        for (int index = 0; index < tail.Segments.Length; index++)
        {
            RidleyTailSegment segment = tail.Segments[index];
            segment.Distance = RidleyTailReach.RestDistance(index);
            segment.Angle = segment.Angle.Mirrored();
            segment.Clockwise = true;
        }
    }

    /// <summary>
    /// Resolves shots against the tail. Only the final two solved tail joints are live in retail code: a
    /// radius-14 tip followed by a radius-10 inner segment. A hit is snapped to the joint,
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
                tail.Segments[^1],
                radius: 14,
                projectiles))
        {
            return;
        }

        _ = TryBlockProjectileAtRidleyTailJoint(
            tail.Segments[^2],
            radius: 10,
            projectiles);
    }

    /// <summary>Consumes the first colliding ordinary beam or missile at one armored joint and spawns its impact effect.</summary>
    private bool TryBlockProjectileAtRidleyTailJoint(
        RidleyTailSegment joint,
        int radius,
        SamusProjectileSystem projectiles)
    {
        // The projectile rectangle check scans the five ordinary places in ascending
        // order. It admits beams, missiles and Super Missiles and rejects bombs, Power Bombs
        // and shots already converted to explosions.
        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            if (!projectile.HasEnemyCollisionPayload)
            {
                continue;
            }

            int xDistance = CoordinateAxis.Distance(projectile.Position.X.Whole, joint.Position.X);
            int yDistance = CoordinateAxis.Distance(projectile.Position.Y.Whole, joint.Position.Y);
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
            MiscDustAnimation dustVariant = missile ? MiscDustAnimation.DudShotTinyExplosion : MiscDustAnimation.Smoke;
            _projectiles.SpawnDustOrExplosion(joint.Position, dustVariant);
            if (missile)
                _projectiles.QueueDudSound(SoundRequestPriority.Normal);
            return true;
        }

        return false;
    }

    /// <summary>Live pogo tail controllers.</summary>
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
            state.Tail.AngleDelta = action == RidleyTailAction.Stab ? 3 : 8;
            state.Tail.ClockwiseWhipTarget = state.Flight.FacingDirection == RidleyFacing.Left ? null : RidleyTailDefinitions.DownAngle;
            state.Tail.CounterClockwiseWhipTarget = state.Flight.FacingDirection == RidleyFacing.Left ? RidleyTailDefinitions.DownAngle : null;
        }
        HandleRidleyPogoTailControl(place, state, samus, action == RidleyTailAction.StabSetup);
        for (int index = 0; index < state.Tail.Segments.Length; index++)
            TickRidleyTailSegment(state.Tail, index);
        if (pointed && !state.Tail.Segments.Any(segment => segment.Active))
            state.Tail.Action = RidleyTailAction.Pogo;
        // The setup/stab wrappers overwrite the result of the shared pointed-tail step.
        if (action == RidleyTailAction.PogoSetup)
            state.Tail.Action = RidleyTailAction.PointDown;
        else if (action == RidleyTailAction.Stab)
            state.Tail.Action = RidleyTailAction.Stab;
    }

    /// <summary>Triggers pogo whips from random/proximity gates and extends the tail downward as Ridley descends.</summary>
    private void HandleRidleyPogoTailControl(RoomEnemyPlace place, RidleyEncounterState state, SamusState samus, bool stabbing)
    {
        if (state.Tail.Segments.All(segment => segment.Active) &&
            (RidleyRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()) ||
             CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) < 128) &&
            state.Tail.ClockwiseWhipTarget is null && state.Tail.CounterClockwiseWhipTarget is null)
        {
            state.Tail.ClockwiseWhipTarget = RidleyTailDefinitions.PogoWhipAngle;
            state.Tail.AngleDelta = 8;
            return;
        }
        if (state.Tail.Segments.Any(segment => segment.Active)) return;
        if (!state.Flight.Velocity.Y.IsNegative)
        {
            state.Tail.Action = stabbing ? RidleyTailAction.Stab : RidleyTailAction.StabSetup;
            if (stabbing)
            {
                foreach (var segment in state.Tail.Segments)
                {
                    segment.TargetDistance = RidleyTailDefinitions.StabDistance;
                    segment.Angle = RidleyTailDefinitions.DownAngle;
                }
                state.Tail.Segments[0].Active = true;
            }
        }
        state.Tail.ClockwiseWhipTarget = null;
        state.Tail.CounterClockwiseWhipTarget = null;
        // The pogo tail's activation timer is initialized to zero and never assigned
        // a nonzero value; both pogo tail setups therefore write zero here.
        state.Tail.AngleDelta = 0;
    }
}
