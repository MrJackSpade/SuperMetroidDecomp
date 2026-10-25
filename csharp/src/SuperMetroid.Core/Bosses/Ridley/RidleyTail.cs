using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's bank-$A6 tail: the segment chain, its flight and pogo controllers, whip aiming,
/// terrain probing, Samus contact, and the projectile-blocking joints shared by the Ceres and
/// Lower Norfair encounters.
/// </summary>
[RoomLifetime]
internal sealed class RidleyTail
{
    /// <summary>Exposes the five ordinary Samus projectile slots checked by Ceres whip aiming.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Replaces shots blocked by tail joints with cartridge dust or dud effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies the current Ridley body and tail state for post-solve Samus contact.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Provides the live random word sampled by Ceres curl and pogo whip decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Ridley's body in physical enemy slot zero for tail contact gates.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Connects tail kinematics, collision, and encounter control to shared room and projectile state.</summary>
    internal RidleyTail(EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, RidleyEncounter ridleyEncounter, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _frameContext = frameContext;
        _projectiles = projectiles;
        _ridleyEncounter = ridleyEncounter;
        _room = room;
        _slotTable = slotTable;
    }

    /// <summary>
    /// Ports the neutral-tail controller at $A6:CBC0-$CCBC, the segment update at $D09F,
    /// and the position/distance chains at $CEBA/$CF5A. The retail routine deliberately
    /// adds the preceding segment's low-byte angle when deriving every later offset;
    /// retaining that documented anomaly is why the tail curls and rotates at the same
    /// rate as the cartridge.
    /// </summary>
    internal void TickRidleyTail(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (state.TailSegments.Length != 7)
            throw new InvalidDataException("Ceres Ridley requires seven native tail segments.");

        if (state.TailFunctionIndex != RidleyTailFunction.None)
        {
            state.TailMinimumClockwiseAngle = RidleyTailDefinitions.MinimumClockwise(state.FacingDirection);
            state.TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.MaximumCounterClockwise(state.FacingDirection);

            if (state.TailFunctionIndex == RidleyTailFunction.Neutral)
            {
                HandleCeresRidleyNeutralTailControl(slot, state, samus);
                for (int index = 0; index < state.TailSegments.Length; index++)
                    TickRidleyTailSegment(state, index);
            }
            else
                TickRidleyPogoTail(slot, state, samus);
        }

        // $CAF5 always composes positions, but only the selected tail controller
        // updates offsets. Function zero and stagger/stop returns retain them.
        RidleyTailSegment first = state.TailSegments[0];
        ushort firstY = unchecked((ushort)(slot.Position.Y.Whole + first.Offset.Y + 16));
        ushort firstX = state.FacingDirection switch
        {
            RidleyFacing.Left => unchecked((ushort)(slot.Position.X.Whole + 32 + first.Offset.X)),
            RidleyFacing.Turning => slot.Position.X.Whole,
            RidleyFacing.Right => unchecked((ushort)(slot.Position.X.Whole - 32 + first.Offset.X)),
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(RidleyFacing)} {(int)state.FacingDirection}."),
        };
        first.Position = new Position16<RoomPixels>(firstX, firstY);

        for (int index = 1; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment previous = state.TailSegments[index - 1];
            RidleyTailSegment current = state.TailSegments[index];
            Position16<RoomPixels> chained = previous.Position + current.Offset;
            current.Position = state.FacingDirection == RidleyFacing.Turning
                ? chained.WithX(slot.Position.X.Whole)
                : chained;
        }

        UpdateRidleyTailDistances(state);
        if (samus is not null)
            ResolveRidleyTailSamusContact(samus);
    }

    /// <summary>
    /// Replays <c>HandleRidleyTailWhip</c> and <c>HandleRidleyTailFlingTrigger</c>. Ceres
    /// disables random/proximity flings only during the swoop, then sets a one-shot request
    /// at its lowest point so the tail snaps toward Samus as Ridley charges back upward.
    /// </summary>
    internal void HandleCeresRidleyNeutralTailControl(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        bool allActive = state.TailSegments.All(segment => segment.Active);
        if (allActive)
        {
            bool targetsIdle = state.ClockwiseWhipTarget is null && state.CounterClockwiseWhipTarget is null;
            if (state.TailWhipRequest != 0 && targetsIdle)
            {
                AimCeresRidleyTailWhip(state, samus, unchecked(new SnesByteAngle((byte)(state.TailWhipRequest - 1))));
            }
            else if (state.IdleTailWhipEnabled)
            {
                // Read the existing seed; this branch never advances the cartridge RNG.
                if ((SnesLittleEndian.LowByte(_room.Random.Read())) >= 240)
                    AimCeresRidleyTailWhip(state, samus, unchecked(new SnesByteAngle((byte)(state.TailWhipRequest - 1))));
                else if (samus is not null && Math.Abs((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 128)
                    AimCeresRidleyTailWhip(state, samus, additionalAngle: new SnesByteAngle(0));
            }

            state.TailWhipRequest = 0;
        }

        if (state.TailSegments.Any(segment => segment.Active))
            return;

        // Once a wave/whip has propagated through all seven parts, neutral AI starts the
        // next wave at the base and retires both old targets. Samus's X half selects the
        // exact one- or two-unit angular delta used by `$A6:CC6B-$CC79`.
        state.TailSegments[0].Active = true;
        state.ClockwiseWhipTarget = null;
        state.CounterClockwiseWhipTarget = null;
        state.TailAngleDelta = samus is not null && samus.Position.X.Whole < 112 ? 1 : 2;
    }

    /// <summary>Aims the Ceres tail at Samus or a nearby missile, clamping targets that would pass through Ridley's body.</summary>
    internal void AimCeresRidleyTailWhip(
        RidleyEnemyState state,
        SamusState? samus,
        SnesByteAngle additionalAngle)
    {
        // Facing direction one is the front-facing transition art. `$A6:D1A4` returns
        // without installing a target in that state, and fight mode zero likewise blocks
        // the attack before liftoff.
        if (samus is null || state.FacingDirection == RidleyFacing.Turning || state.FightMode == RidleyFightMode.NotFighting)
            return;

        RidleyTailSegment root = state.TailSegments[0];
        ushort targetX = samus.Position.X.Whole;
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole + 24));
        // $D242 scans only the five ordinary projectile slots, first match wins.
        if (_frameContext.SamusProjectiles is { } projectiles && projectiles.SlotTable.ProjectileCounter > 0)
        {
            RidleyTailSegment tip = state.TailSegments[6];
            foreach (var projectile in projectiles.Slots.Take(5))
            {
                if (projectile.PackedType.Family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)) continue;
                if (Math.Abs((short)(projectile.Position.X.Whole - tip.Position.X)) - projectile.Radius.X >= 64 ||
                    Math.Abs((short)(projectile.Position.Y.Whole - tip.Position.Y)) - projectile.Radius.Y >= 64) continue;
                targetX = projectile.Position.X.Whole;
                targetY = projectile.Position.Y.Whole;
                break;
            }
        }
        byte targetByte = EnemyMath.CalculateBearing(
            new(unchecked((short)(targetX - root.Position.X)), unchecked((short)(targetY - root.Position.Y)))).ToTableAngle().AddTableUnits(SnesByteAngle.QuarterTurn.RawValue).RawValue;
        ushort additional = additionalAngle.ToAngle().RawValue;

        if (state.FacingDirection == RidleyFacing.Left)
        {
            // Facing-left Ridley cannot aim through his own body. Angles inside the
            // forbidden forward interval clamp to $E8 before bank-$3F expansion.
            if (targetByte is >= RidleyTailDefinitions.ForbiddenForwardStart and < RidleyTailDefinitions.ForbiddenForwardEnd)
                targetByte = RidleyTailDefinitions.ForbiddenForwardEnd;
            ushort target = unchecked((ushort)(RidleyTailDefinitions.LeftWhipTargetBase + targetByte - additional));
            if (target < root.Angle)
            {
                state.ClockwiseWhipTarget = target;
                state.TailAngleDelta = 8;
            }
        }
        else
        {
            if (targetByte is >= RidleyTailDefinitions.ForbiddenForwardStart and < RidleyTailDefinitions.ForbiddenForwardEnd)
                targetByte = RidleyTailDefinitions.ForbiddenForwardStart;
            ushort target = unchecked((ushort)(RidleyTailDefinitions.RightWhipTargetBase + targetByte + additional));
            if (target >= root.Angle)
            {
                state.CounterClockwiseWhipTarget = target;
                state.TailAngleDelta = 8;
            }
        }
    }

    /// <summary>Advances one joint's stagger angle and active whip direction toward the shared target.</summary>
    internal static void TickRidleyTailSegment(RidleyEnemyState state, int index)
    {
        RidleyTailSegment segment = state.TailSegments[index];
        if (segment.Active)
        {
            if (segment.StaggerAngle < state.IdealInterSegmentTailAngle)
            {
                segment.StaggerAngle += state.TailAngleDelta;
                return;
            }

            if (segment.StaggerAngle != RidleyTailDefinitions.StaggerPropagated)
            {
                segment.StaggerAngle = RidleyTailDefinitions.StaggerPropagated;
                if (index + 1 < state.TailSegments.Length)
                {
                    state.TailSegments[index + 1].Active = true;
                    state.TailSegments[index + 1].Clockwise = segment.Clockwise;
                }
            }

            if (segment.Clockwise)
            {
                if (state.ClockwiseWhipTarget is int clockwiseTarget)
                {
                    segment.TargetDistance = RidleyTailDefinitions.WhipDistance;
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < clockwiseTarget)
                    {
                        if (index == 0 || !state.TailSegments[index - 1].Active)
                        {
                            segment.Angle = clockwiseTarget;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.Clockwise = false;
                        segment.Angle = state.TailMinimumClockwiseAngle.RawValue;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)(next + 1));
                    }
                }
                else
                {
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < state.TailMinimumClockwiseAngle.RawValue)
                    {
                        segment.Clockwise = false;
                        segment.Angle = state.TailMinimumClockwiseAngle.RawValue;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)(next + 1));
                    }
                }
            }
            else
            {
                int next = segment.Angle + state.TailAngleDelta;
                if (state.CounterClockwiseWhipTarget is int counterClockwiseTarget)
                {
                    segment.TargetDistance = RidleyTailDefinitions.WhipDistance;
                    if (next >= counterClockwiseTarget)
                    {
                        if (index == 0 || !state.TailSegments[index - 1].Active)
                        {
                            segment.Angle = counterClockwiseTarget;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.Clockwise = true;
                        segment.Angle = state.TailMaximumCounterClockwiseAngle.RawValue;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
                else
                {
                    if (next >= state.TailMaximumCounterClockwiseAngle.RawValue)
                    {
                        segment.Clockwise = true;
                        segment.Angle = state.TailMaximumCounterClockwiseAngle.RawValue;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
            }
        }

        UpdateRidleyTailSegmentOffset(state, index);
    }

    /// <summary>
    /// Converts one segment's current polar angle/distance into the relative Cartesian
    /// offset consumed by $A6:CEBA. This is deliberately independent of the segment's active
    /// flag: inactive tail pieces keep their established pose rather than collapsing.
    /// </summary>
    private static void UpdateRidleyTailSegmentOffset(RidleyEnemyState state, int index)
    {
        RidleyTailSegment segment = state.TailSegments[index];
        byte angle = unchecked((byte)segment.Angle);
        if (index != 0)
            angle = unchecked((byte)(angle + state.TailSegments[index - 1].Angle));
        int distanceInPixels = SnesEightEight.WholeOf(segment.Distance);
        // $A9:C46C reads the middle/high bytes of the signed Mode 7 product.
        // An arithmetic shift preserves its floor for negative fractional pixels.
        segment.Offset = new Offset16<RoomPixels>(
            unchecked((short)(SnesEightEight.Multiply(distanceInPixels, EnemyTrigonometryTables.SignedSine(new SnesByteAngle(angle))))),
            unchecked((short)(SnesEightEight.Multiply(distanceInPixels, EnemyTrigonometryTables.SignedSine(unchecked(new SnesByteAngle((byte)(angle + 64))))))));
    }

    /// <summary>Ends a joint's active whip and flips its movement direction for the next activation.</summary>
    private static void DeactivateRidleyTailSegment(RidleyTailSegment segment)
    {
        segment.Active = false;
        segment.StaggerAngle = 0;
        segment.Clockwise = !segment.Clockwise;
    }

    /// <summary>Ports the six extend/shrink records at <c>$A6:CF5A-$D09E</c>.</summary>
    internal static void UpdateRidleyTailDistances(RidleyEnemyState state)
    {
        for (int index = 1; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            if (segment.TargetDistance != 0)
            {
                if (segment.TargetDistance < segment.Distance)
                    segment.TargetDistance = 0;
                segment.Distance = Math.Min(
                    RidleyTailDefinitions.MaximumDistance(index),
                    segment.Distance + state.TailExtensionSpeed);
            }
            else if (segment.Distance > RidleyTailDefinitions.RestDistance(index))
            {
                // Half a pixel per update; every rest length is at least two pixels, so a shrinking link never passes zero.
                segment.Distance -= 128;
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
                Distance = RidleyTailDefinitions.RestDistance(index),
                Angle = RidleyTailDefinitions.InitialAngle(index),
                StaggerAngle = 17,
            };
        }
        return segments;
    }

    /// <summary>
    /// <c>RandomlyUpdateRidleyTailCurliness</c> ($A6:A2BD): when the live RNG word is at
    /// least $FF00 (sampled, not advanced), the ideal inter-segment tail angle becomes its
    /// low nibble plus seven plus the compare's set carry, so $08..$17.
    /// </summary>
    internal void RandomlyUpdateCeresRidleyTailCurliness(RidleyEnemyState state)
    {
        int random = _room.Random.Read();
        if (SnesLittleEndian.HighByte(random) != byte.MaxValue)
            return;
        state.IdealInterSegmentTailAngle = random % 16 + 7 + 1;
    }

    /// <summary>Applies tail-tip contact damage after joint positions are solved, unless Ridley carries Samus or collision is disabled.</summary>
    internal bool ResolveRidleyTailSamusContact(SamusState samus)
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        RidleyEnemyState? state = _ridleyEncounter.Ridley;
        // $A6:CAF5 checks the newly solved tail, independently of the earlier
        // extended-body contact pass, and never damages Samus while carrying her.
        if (state is null || state.GrabState != RidleyGrabState.Released || !samus.InvincibilityTimer.IsZero ||
            body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            !state.MovementAnimationEnabled || state.TailSegments.Length != 7)
            return false;
        RidleyTailSegment tip = state.TailSegments[6];
        int dx = Math.Abs(unchecked((short)(samus.Position.X.Whole - tip.Position.X)));
        int dy = Math.Abs(unchecked((short)(samus.Position.Y.Whole - tip.Position.Y)));
        if (dx >= samus.Kinematics.Radius.X + 14 || dy >= samus.Kinematics.Radius.Y + 14)
            return false;
        EnemyCollision.ApplyNormalEnemyTouchDamage(samus, checked((ushort)state.TailDamage), tip.Position.X);
        return true;
    }

    /// <summary>Probes the tip and four inner joints at their native downward offsets for solid room blocks.</summary>
    internal static bool RidleyTailTouchesTerrain(
        RidleyEnemyState state,
        RoomLevelData? level)
    {
        if (level is null || state.TailSegments.Length != 7)
            return false;

        // $B7E7 samples the tip and the next four joints with the exact +16/+18 Y probes.
        int tipIndex = state.TailSegments.Length - 1;
        for (int index = tipIndex; index >= 2; index--)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            ushort x = segment.Position.X;
            ushort y = unchecked((ushort)(segment.Position.Y + (index == tipIndex ? 16 : 18)));
            if ((x >> 4) >= level.Size.X || (y >> 4) >= level.Size.Y)
                continue;
            if (level.GetCollisionBlockAtPixel(new Position16<RoomPixels>(x, y)).CollisionType != 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Ports $A6:D3F9. A facing instruction restores the seven rest distances before
    /// reflecting angles around $8000 and setting clockwise movement. The offset words
    /// remain untouched until the following tail controller update.
    /// </summary>
    internal static void MirrorRidleyTail(RidleyEnemyState state)
    {
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            segment.Distance = RidleyTailDefinitions.RestDistance(index);
            segment.Angle = unchecked((ushort)(RidleyTailDefinitions.MirrorAxisAngle.RawValue - segment.Angle));
            segment.Clockwise = true;
        }
    }

    /// <summary>
    /// Ports $A6:E088. Only the final two solved tail joints are live in retail code: a
    /// radius-14 tip followed by a radius-10 inner segment. A hit is snapped to the joint,
    /// marked consumed, and replaced by the cartridge's dust/dud actor; Ridley's health is
    /// never consulted because the tail is armor rather than another body hitbox.
    /// </summary>
    internal void ResolveRidleyTailProjectileHits(
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusProjectileSystem projectiles)
    {
        if (body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            state.TailSegments.Length != 7)
        {
            return;
        }

        if (TryBlockProjectileAtRidleyTailJoint(
                state.TailSegments[6],
                radius: 14,
                projectiles))
        {
            return;
        }

        _ = TryBlockProjectileAtRidleyTailJoint(
            state.TailSegments[5],
            radius: 10,
            projectiles);
    }

    /// <summary>Consumes the first colliding ordinary beam or missile slot at one armored joint and spawns its impact effect.</summary>
    private bool TryBlockProjectileAtRidleyTailJoint(
        RidleyTailSegment joint,
        ushort radius,
        SamusProjectileSystem projectiles)
    {
        // CheckForProjectileCollisionWithRectangle scans the five ordinary native slots in
        // ascending order. Its signed-type gate admits active beam/missile/super records and
        // rejects bomb, power-bomb, and already-converted impact families.
        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            int typeNibble = SamusProjectileFamilies.IndexOf(projectile.PackedType.Raw);
            if (!projectile.HasEnemyCollisionPayload ||
                unchecked((short)projectile.Type) >= 0 ||
                typeNibble >= 3)
            {
                continue;
            }

            int xDistance = Math.Abs(unchecked((short)(projectile.Position.X.Whole - joint.Position.X)));
            int yDistance = Math.Abs(unchecked((short)(projectile.Position.Y.Whole - joint.Position.Y)));
            if (xDistance >= projectile.Radius.X + radius ||
                yDistance >= projectile.Radius.Y + radius)
            {
                continue;
            }

            projectile.Position = projectile.Position.WithWhole(joint.Position);
            projectile.Direction = projectile.PackedDirection.WithCollisionLifecycleState();

            // Missile family one gets the tiny dud and library-one sound $3D. Beams and
            // supers use the larger smoke variant $0C, exactly as $A6:E126 selects it.
            MiscDustAnimation dustVariant = typeNibble == 1 ? MiscDustAnimation.DudShotTinyExplosion : MiscDustAnimation.Smoke;
            _projectiles.SpawnRoomGraphicsDustExplosion(joint.Position, dustVariant);
            if (typeNibble == 1)
                _projectiles.LastEnemyProjectileDudSoundEffect = Library1Sound.DudShot;
            return true;
        }

        return false;
    }

    /// <summary>Live pogo tail controllers $A6:CB33..CBD3 and $CD24..CE64.</summary>
    internal void TickRidleyPogoTail(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        RidleyTailFunction function = state.TailFunctionIndex;
        bool pointed = function is RidleyTailFunction.PogoSetup or RidleyTailFunction.PointDown or RidleyTailFunction.Stab;
        if (!pointed && function is not (RidleyTailFunction.Pogo or RidleyTailFunction.StabSetup))
            throw new InvalidDataException($"Ridley tail function {function} is not translated.");

        state.TailMinimumClockwiseAngle = RidleyTailDefinitions.MinimumClockwise(state.FacingDirection);
        state.TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.MaximumCounterClockwise(state.FacingDirection);
        if (pointed)
        {
            state.TailAngleDelta = function == RidleyTailFunction.Stab ? 3 : 8;
            state.ClockwiseWhipTarget = state.FacingDirection == RidleyFacing.Left ? null : RidleyTailDefinitions.DownAngle.RawValue;
            state.CounterClockwiseWhipTarget = state.FacingDirection == RidleyFacing.Left ? RidleyTailDefinitions.DownAngle.RawValue : null;
        }
        HandleRidleyPogoTailControl(slot, state, samus, function == RidleyTailFunction.StabSetup);
        for (int index = 0; index < state.TailSegments.Length; index++)
            TickRidleyTailSegment(state, index);
        if (pointed && !state.TailSegments.Any(segment => segment.Active))
            state.TailFunctionIndex = RidleyTailFunction.Pogo;
        // The setup/stab wrappers overwrite the result of the shared pointed-tail routine.
        if (function == RidleyTailFunction.PogoSetup)
            state.TailFunctionIndex = RidleyTailFunction.PointDown;
        else if (function == RidleyTailFunction.Stab)
            state.TailFunctionIndex = RidleyTailFunction.Stab;
    }

    /// <summary>Triggers pogo whips from random/proximity gates and extends the tail downward as Ridley descends.</summary>
    private void HandleRidleyPogoTailControl(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, bool stabbing)
    {
        if (state.TailSegments.All(segment => segment.Active) &&
            ((SnesLittleEndian.LowByte(_room.Random.Read())) >= 240 ||
             samus is not null && Math.Abs((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 128) &&
            state.ClockwiseWhipTarget is null && state.CounterClockwiseWhipTarget is null)
        {
            state.ClockwiseWhipTarget = RidleyTailDefinitions.PogoWhipAngle.RawValue;
            state.TailAngleDelta = 8;
            return;
        }
        if (state.TailSegments.Any(segment => segment.Active)) return;
        if (state.Velocity.Y.SignedRawValue >= 0)
        {
            state.TailFunctionIndex = stabbing ? RidleyTailFunction.Stab : RidleyTailFunction.StabSetup;
            if (stabbing)
            {
                foreach (var segment in state.TailSegments)
                {
                    segment.TargetDistance = RidleyTailDefinitions.StabDistance;
                    segment.Angle = RidleyTailDefinitions.DownAngle.RawValue;
                }
                state.TailSegments[0].Active = true;
            }
        }
        state.ClockwiseWhipTarget = null;
        state.CounterClockwiseWhipTarget = null;
        // Native pogoTailActivationTimer is initialized to zero and never assigned
        // a nonzero value by any live routine; CD90/CE61 therefore write zero here.
        state.TailAngleDelta = 0;
    }
}
