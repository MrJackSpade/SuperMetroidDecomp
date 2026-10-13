using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Ridley's bank-$A6 tail: the segment chain, its flight and pogo controllers, whip aiming,
/// terrain probing, Samus contact, and the projectile-blocking joints shared by the Ceres and
/// Lower Norfair encounters.
/// </summary>
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
            RidleyFacing.Left => unchecked((ushort)(slot.Position.X.Whole + 0x0020 + first.Offset.X)),
            RidleyFacing.Turning => slot.Position.X.Whole,
            RidleyFacing.Right => unchecked((ushort)(slot.Position.X.Whole - 0x0020 + first.Offset.X)),
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
            bool targetsIdle = state.ClockwiseWhipTargetIdle && state.CounterClockwiseWhipTargetIdle;
            if (state.TailWhipRequest != 0 && targetsIdle)
            {
                AimCeresRidleyTailWhip(state, samus, unchecked((byte)(state.TailWhipRequest - 1)));
            }
            else if (state.IdleTailWhipEnabled != 0)
            {
                // Read the existing seed; this branch never advances the cartridge RNG.
                if ((SnesLittleEndian.LowByte(_room.Random.Read())) >= 0xf0)
                    AimCeresRidleyTailWhip(state, samus, unchecked((byte)(state.TailWhipRequest - 1)));
                else if (samus is not null && Math.Abs((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 128)
                    AimCeresRidleyTailWhip(state, samus, additionalAngle: 0);
            }

            state.TailWhipRequest = 0;
        }

        if (state.TailSegments.Any(segment => segment.Active))
            return;

        // Once a wave/whip has propagated through all seven parts, neutral AI starts the
        // next wave at the base and retires both old targets. Samus's X half selects the
        // exact one- or two-unit angular delta used by `$A6:CC6B-$CC79`.
        state.TailSegments[0].Active = true;
        state.TailWhipTargetClockwiseAngle = 0xffff;
        state.TailWhipTargetCounterClockwiseAngle = 0xffff;
        state.TailAngleDelta = samus is not null && samus.Position.X.Whole < 0x0070
            ? (ushort)1
            : (ushort)2;
    }

    /// <summary>Aims the Ceres tail at Samus or a nearby missile, clamping targets that would pass through Ridley's body.</summary>
    internal void AimCeresRidleyTailWhip(
        RidleyEnemyState state,
        SamusState? samus,
        byte additionalAngle)
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
        if (_frameContext._samusProjectilesForEnemyFrame is { } projectiles && projectiles.SlotTable.ProjectileCounter > 0)
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
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(targetX - root.Position.X)), unchecked((short)(targetY - root.Position.Y))));
        byte targetByte = unchecked((byte)-(cartridgeAngle - 0x80));
        ushort additional = SnesAngle.FromTableIndex(unchecked((byte)additionalAngle)).RawValue;

        if (state.FacingDirection == RidleyFacing.Left)
        {
            // Facing-left Ridley cannot aim through his own body. Angles inside the
            // forbidden forward interval clamp to $E8 before bank-$3F expansion.
            if (targetByte is >= 0x18 and < 0xe8)
                targetByte = 0xe8;
            ushort target = unchecked((ushort)(0x3f00 + targetByte - additional));
            if (target < root.Angle)
            {
                state.TailWhipTargetClockwiseAngle = target;
                state.TailAngleDelta = 8;
            }
        }
        else
        {
            if (targetByte is >= 0x18 and < 0xe8)
                targetByte = 0x18;
            ushort target = unchecked((ushort)(0x4000 + targetByte + additional));
            if (target >= root.Angle)
            {
                state.TailWhipTargetCounterClockwiseAngle = target;
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
                segment.StaggerAngle = unchecked((ushort)(
                    segment.StaggerAngle + state.TailAngleDelta));
                return;
            }

            if (segment.StaggerAngle != 0xffff)
            {
                segment.StaggerAngle = 0xffff;
                if (index + 1 < state.TailSegments.Length)
                {
                    state.TailSegments[index + 1].Active = true;
                    state.TailSegments[index + 1].MovementDirection = segment.MovementDirection;
                }
            }

            if ((segment.MovementDirection & 0x8000) != 0)
            {
                bool whipping = !state.ClockwiseWhipTargetIdle;
                if (whipping)
                {
                    segment.TargetDistance = 0x0c00;
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < state.TailWhipTargetClockwiseAngle)
                    {
                        if (index == 0 || !state.TailSegments[index - 1].Active)
                        {
                            segment.Angle = state.TailWhipTargetClockwiseAngle;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.MovementDirection = 0;
                        segment.Angle = state.TailMinimumClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)(next + 1));
                    }
                }
                else
                {
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < state.TailMinimumClockwiseAngle)
                    {
                        segment.MovementDirection = 0;
                        segment.Angle = state.TailMinimumClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)(next + 1));
                    }
                }
            }
            else
            {
                bool whipping = !state.CounterClockwiseWhipTargetIdle;
                int next = segment.Angle + state.TailAngleDelta;
                if (whipping)
                {
                    segment.TargetDistance = 0x0c00;
                    if (next >= state.TailWhipTargetCounterClockwiseAngle)
                    {
                        if (index == 0 || !state.TailSegments[index - 1].Active)
                        {
                            segment.Angle = state.TailWhipTargetCounterClockwiseAngle;
                            DeactivateRidleyTailSegment(segment);
                            return;
                        }
                        segment.MovementDirection = 0x8000;
                        segment.Angle = state.TailMaximumCounterClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
                else
                {
                    if (next >= state.TailMaximumCounterClockwiseAngle)
                    {
                        segment.MovementDirection = 0x8000;
                        segment.Angle = state.TailMaximumCounterClockwiseAngle;
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
        ushort distanceInPixels = unchecked((ushort)(SnesLittleEndian.HighByte(segment.Distance)));
        // $A9:C46C reads the middle/high bytes of the signed Mode 7 product.
        // An arithmetic shift preserves its floor for negative fractional pixels.
        segment.Offset = new Offset16<RoomPixels>(
            unchecked((short)(SnesEightEight.Multiply(distanceInPixels, EnemyTrigonometryTables.SignedSine(angle)))),
            unchecked((short)(SnesEightEight.Multiply(distanceInPixels, EnemyTrigonometryTables.SignedSine(unchecked((byte)(angle + 64)))))));
    }

    /// <summary>Ends a joint's active whip and flips its movement direction for the next activation.</summary>
    private static void DeactivateRidleyTailSegment(RidleyTailSegment segment)
    {
        segment.Active = false;
        segment.StaggerAngle = 0;
        segment.MovementDirection ^= 0x8000;
    }

    /// <summary>Ports the six extend/shrink records at <c>$A6:CF5A-$D09E</c>.</summary>
    internal static void UpdateRidleyTailDistances(RidleyEnemyState state)
    {
        ushort[] maximumDistances = [0, 0x1800, 0x1800, 0x1600, 0x1600, 0x1200, 0x0500];
        for (int index = 1; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            if (segment.TargetDistance != 0)
            {
                if (segment.TargetDistance < segment.Distance)
                    segment.TargetDistance = 0;
                segment.Distance = unchecked((ushort)Math.Min(
                    maximumDistances[index],
                    segment.Distance + state.TailExtensionSpeed));
            }
            else if (segment.Distance > RidleyTailDefinitions.RestDistance(index))
            {
                segment.Distance = unchecked((ushort)(segment.Distance - 0x0080));
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
                MovementDirection = 0x8000,
                Distance = RidleyTailDefinitions.RestDistance(index),
                Angle = RidleyTailDefinitions.InitialAngle(index),
                StaggerAngle = 0x0011,
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
        ushort random = _room.Random.Read();
        if (random < 0xff00)
            return;
        state.IdealInterSegmentTailAngle = (ushort)((random & 0x000f) + 7 + 1);
    }

    /// <summary>Applies tail-tip contact damage after joint positions are solved, unless Ridley carries Samus or collision is disabled.</summary>
    internal bool ResolveRidleyTailSamusContact(SamusState samus)
    {
        RoomEnemySlot body = _slotTable._slots[0];
        RidleyEnemyState? state = _ridleyEncounter.Ridley;
        // $A6:CAF5 checks the newly solved tail, independently of the earlier
        // extended-body contact pass, and never damages Samus while carrying her.
        if (state is null || state.GrabState != 0 || !samus.InvincibilityTimer.IsZero ||
            body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            state.MovementAnimationEnabled == 0 || state.TailSegments.Length != 7)
            return false;
        RidleyTailSegment tip = state.TailSegments[6];
        int dx = Math.Abs(unchecked((short)(samus.Position.X.Whole - tip.Position.X)));
        int dy = Math.Abs(unchecked((short)(samus.Position.Y.Whole - tip.Position.Y)));
        if (dx >= samus.Kinematics.Radius.X + 14 || dy >= samus.Kinematics.Radius.Y + 14)
            return false;
        EnemyCollision.ApplyNormalEnemyTouchDamage(samus, state.TailDamage, tip.Position.X);
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
            segment.Angle = unchecked((ushort)(0x8000 - segment.Angle));
            segment.MovementDirection |= 0x8000;
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
            int typeNibble = SamusProjectileFamilies.IndexOf(projectile.Type);
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
            projectile.Direction = unchecked((ushort)(projectile.Direction | 0x0010));

            // Missile family one gets the tiny dud and library-one sound $3D. Beams and
            // supers use the larger smoke variant $0C, exactly as $A6:E126 selects it.
            MiscDustAnimation dustVariant = typeNibble == 1 ? MiscDustAnimation.DudShotTinyExplosion : MiscDustAnimation.Smoke;
            _projectiles.SpawnRoomGraphicsDustExplosion(joint.Position, dustVariant);
            if (typeNibble == 1)
                _projectiles.LastEnemyProjectileDudSoundEffect = SoundEffectLibrary1Sounds.DudShot.Value;
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
            state.TailAngleDelta = function == RidleyTailFunction.Stab ? (ushort)3 : (ushort)8;
            state.TailWhipTargetClockwiseAngle = state.FacingDirection == RidleyFacing.Left ? ushort.MaxValue : RidleyTailDefinitions.DownAngle;
            state.TailWhipTargetCounterClockwiseAngle = state.FacingDirection == RidleyFacing.Left ? RidleyTailDefinitions.DownAngle : ushort.MaxValue;
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
            ((SnesLittleEndian.LowByte(_room.Random.Read())) >= 0xf0 ||
             samus is not null && Math.Abs((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 128) &&
            state.ClockwiseWhipTargetIdle && state.CounterClockwiseWhipTargetIdle)
        {
            state.TailWhipTargetClockwiseAngle = RidleyTailDefinitions.PogoWhipAngle;
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
                    segment.Angle = RidleyTailDefinitions.DownAngle;
                }
                state.TailSegments[0].Active = true;
            }
        }
        state.TailWhipTargetClockwiseAngle = ushort.MaxValue;
        state.TailWhipTargetCounterClockwiseAngle = ushort.MaxValue;
        // Native pogoTailActivationTimer is initialized to zero and never assigned
        // a nonzero value by any live routine; CD90/CE61 therefore write zero here.
        state.TailAngleDelta = 0;
    }
}
