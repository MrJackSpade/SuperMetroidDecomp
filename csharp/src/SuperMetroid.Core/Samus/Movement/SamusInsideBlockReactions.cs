using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Body-overlap scroll, conveyor and sand reactions from Samus's inside-block detection.</summary>
public static class SamusInsideBlockReactions
{
    /// <summary>Half a pixel in fraction steps: quicksand keeps only the part of the base speed's fraction below it.</summary>
    private const int HalfPixelFraction = 32768;

    /// <summary>Samples bottom, center, and top in order.</summary>
    internal static void PrepareFrame(RoomLevelData level, SamusState samus,
        AreaId area, bool areaBossDefeated = false, ISamusRoomObjects? roomObjects = null)
    {
        var body = samus.Kinematics;
        body.SandCollisionArea = area;
        SamusInsideBlockSamplePoints.Visit(body.Position.Y.Whole, body.Radius.Y, (y, point) =>
            Visit(y, point == SamusInsideBlockPoint.Bottom, point == SamusInsideBlockPoint.Center));

        void Visit(int y, bool bottomPoint, bool centerPoint)
        {
            var block = level.GetCollisionBlockOrEdgeSolid(new(CoordinateAxis.Wrap(body.Position.X.Whole >> 4), CoordinateAxis.Wrap(y >> 4)));
            if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block) ||
                block.CollisionType != RoomCollisionType.SpecialAir)
                return;
            if (!block.Behavior.Reaction.IsAreaSpecific)
            {
                // Admits only inside-block sample one (the center).
                // Feet/head contacts belong to the separate movement scans. Keeping
                // this center wake-up lets a lower trigger win after the torso clears
                // an upper trigger, without changing room-object slot priority.
                if (centerPoint && block.Behavior.Reaction == BlockReactionDefinitions.ScrollTrigger &&
                    (roomObjects is null || !roomObjects.Scrolls.TryNotifyScrollTouch(level.BlockAt(block.Index))))
                    throw new InvalidOperationException($"Inside scroll trigger block {block.Index} has no active room-object owner.");
                // The normal table contains conveyors; area-table entries below own
                // sand. Both must be visited in the same bottom/center/top order.
                ApplyConveyor(block.Behavior.Reaction);
                return;
            }
            SpecialAirInsideReaction reaction = SpecialAirReactions.InsideOf(area, block.Behavior.Reaction);
            // A sand reaction takes a room-object place before it acts; with every place taken it does nothing.
            if (QuicksandReactions.PlacesObject(reaction) && roomObjects is not null &&
                !roomObjects.Quicksand.TrySpawnQuicksandReaction(level.BlockAt(block.Index), reaction))
            {
                return;
            }
            switch (reaction)
            {
                case SpecialAirInsideReaction.Nothing:
                case SpecialAirInsideReaction.IcePhysics:
                    // Neither inside reaction changes Samus's motion here.
                    break;
                case SpecialAirInsideReaction.BrinstarFloorPlant:
                    RequireRoomObjects().SamusEater.TrySpawnSamusEater(level, block, SamusEaterObjectDefinitions.Floor, samus.RoomView);
                    break;
                case SpecialAirInsideReaction.BrinstarCeilingPlant:
                    RequireRoomObjects().SamusEater.TrySpawnSamusEater(level, block, SamusEaterObjectDefinitions.Ceiling, samus.RoomView);
                    break;
                case SpecialAirInsideReaction.QuicksandSurface:
                    // The game's setup cancels running momentum even for center/top samples,
                    // but preserves the lower fractional bits rather than zeroing base X.
                    var speed = samus.HorizontalSpeed;
                    speed.HasRunningMomentum = false;
                    speed.SpeedBoostCounter = default;
                    speed.EchoSoundFlag = 0;
                    speed.ExtraRunSpeed = default;
                    speed.BaseSpeed = new UnsignedSixteenSixteen(0, speed.BaseSpeed.Fraction % HalfPixelFraction);
                    if (!bottomPoint) return;
                    QuicksandSurfacePhysics physics =
                        QuicksandReactions.SurfacePhysics(
                            samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit));
                    // Indexes a four-entry table with the direction's low two bits;
                    // its fourth entry repeats the grounded handler, but the vertical direction only ever
                    // holds the three defined directions.
                    switch (body.YDirection)
                    {
                        case SamusVerticalDirection.None:
                            body.YSpeed = default;
                            SetExtra(physics.StationaryDisplacement);
                            break;
                        case SamusVerticalDirection.Up:
                            SignedSixteenSixteen limit = physics.UpwardSpeedLimit;
                            // The game compares the speed and limit in 256ths of a pixel, then
                            // replaces the speed's whole pixels and fraction with the limit's.
                            if (body.YSpeed.InTwoFiftySixths > limit.InTwoFiftySixths)
                            {
                                body.YSpeed = UnsignedSixteenSixteen.FromDisplacement(limit);
                            }
                            SetExtra(physics.MovingDisplacement);
                            break;
                        case SamusVerticalDirection.Down:
                            SetExtra(physics.MovingDisplacement);
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Undefined {nameof(SamusVerticalDirection)} {body.YDirection}.");
                    }
                    break;
                case SpecialAirInsideReaction.SubmergingQuicksand:
                    SetExtra(QuicksandDefinitions.SubmergingSinkDisplacement);
                    break;
                case SpecialAirInsideReaction.SlowSandfall:
                    SetExtra(QuicksandDefinitions.SlowSandfallDisplacement);
                    break;
                case SpecialAirInsideReaction.FastSandfall:
                    SetExtra(QuicksandDefinitions.FastSandfallDisplacement);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SpecialAirInsideReaction)} {reaction}.");
            }
        }
        ISamusRoomObjects RequireRoomObjects() =>
            roomObjects ?? throw new InvalidOperationException("Samus Eater reaction requires the room-object owner.");
        void ApplyConveyor(BlockReaction reaction)
        {
            // The common special-air reactions include the conveyors; only they react here.
            if (!ConveyorBlockDefinitions.Conveyors.TryGetValue(reaction, out ConveyorBlock conveyor)) return;
            bool rightward = conveyor.Direction == HorizontalDirection.Right;
            samus.HorizontalSpeed.SelectNormalAirSpeedTable();
            if (conveyor.Condition == ConveyorCarryCondition.OnFootWithPower &&
                ((area == AreaId.WreckedShip && !areaBossDefeated) || body.YSpeed.Whole != 0))
                return;
            // The game checks only whole Y speed: fractional vertical motion does
            // not suppress carry. This replaces external X displacement, never adds to it.
            body.ExtraDisplacement = body.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(
                rightward ? ConveyorBlockDefinitions.RightwardCarry : ConveyorBlockDefinitions.LeftwardCarry, 0));
        }
        void SetExtra(SignedSixteenSixteen displacement)
        {
            body.ExtraDisplacement = body.ExtraDisplacement.WithY(displacement);
        }
    }

    /// <summary>
    /// Runs the sand reactions among the special-air collision reactions. Landing on sand is separate from being
    /// blocked: downward movement grounds on the quicksand surface without discarding its accepted sinking displacement,
    /// and pose-clearance probes read only whether the block stops them.
    /// </summary>
    /// <returns>Whether the block lets Samus's movement pass or stops it.</returns>
    /// <param name="contact">Samus's body and contact damage.</param>
    /// <param name="terrain">The room's level, which places the sand block a reaction sits on, and its objects, where a sand
    /// reaction takes a place.</param>
    /// <param name="block">The special-air block the collision scan reached.</param>
    /// <param name="axis">The axis the scan moves along.</param>
    /// <param name="displacement">The scan's accepted displacement, which sand can limit.</param>
    /// <param name="surfaceContact">Whether Samus touched the sand's surface.</param>
    /// <param name="blockReactionDirection">The direction the reaction sees, or null for the scan's own.</param>
    internal static SpecialAirCollisionOutcome ReactCollision(SamusCollisionContact contact,
        SamusTerrain terrain, RoomCollisionBlock block, Axis axis, ref SignedSixteenSixteen displacement, out bool surfaceContact,
        SamusCollisionDirection? blockReactionDirection = null)
    {
        RoomLevelData level = terrain.Level;
        ISamusRoomObjects? roomObjects = terrain.RoomObjects;
        SamusKinematicsState body = contact.Body;
        surfaceContact = false;
        if (!block.Behavior.Reaction.IsAreaSpecific) return SpecialAirCollisionOutcome.PassesThrough;
        SpecialAirCollisionReaction reaction = SpecialAirReactions.CollisionOf(body.SandCollisionArea, block.Behavior.Reaction);
        // A sand reaction takes a room-object place before it acts; with every place taken Samus passes as through air.
        if (QuicksandReactions.PlacesObject(reaction) && roomObjects is not null &&
            !roomObjects.Quicksand.TrySpawnQuicksandReaction(level.BlockAt(block.Index), reaction))
        {
            return SpecialAirCollisionOutcome.PassesThrough;
        }
        if (reaction == SpecialAirCollisionReaction.SubmergingQuicksand)
        {
            body.YSpeed = default;
            body.YAcceleration = default;
            return SpecialAirCollisionOutcome.PassesThrough;
        }
        if (reaction != SpecialAirCollisionReaction.QuicksandSurface) return SpecialAirCollisionOutcome.PassesThrough;
        SamusCollisionDirection reactionDirection = blockReactionDirection ??
            (axis == Axis.Vertical
                ? displacement.IsNegative ? SamusCollisionDirection.Up : SamusCollisionDirection.Down
                : displacement.IsNegative ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
        // The surface reacts to vertical scans and to the pose-observation probe; ordinary horizontal movement passes
        // through it untouched.
        if (reactionDirection is SamusCollisionDirection.Left or SamusCollisionDirection.Right) return SpecialAirCollisionOutcome.PassesThrough;
        // A rising Samus passes through; a stationary one lands only on an actual downward collision; a falling one lands
        // even during pose-observation probes. The grounded vertical direction never reaches the surface.
        switch (body.YDirection)
        {
            case SamusVerticalDirection.Up:
                return SpecialAirCollisionOutcome.PassesThrough;
            case SamusVerticalDirection.None:
                if (reactionDirection != SamusCollisionDirection.Down) return SpecialAirCollisionOutcome.PassesThrough;
                break;
            case SamusVerticalDirection.Down:
                break;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(SamusVerticalDirection)} {body.YDirection}.");
        }
        if (contact.ContactDamage == SamusContactDamageIndex.SpeedBooster)
        {
            displacement = SignedSixteenSixteen.Zero;
            return SpecialAirCollisionOutcome.Blocks;
        }
        // A stationary Samus's probe is compared with the surface limit and limited to it in its near reading alone,
        // in 256ths of a pixel below 256 whole pixels (SignedSixteenSixteen.NearTwoFiftySixths); the limit, a multiple
        // of a sixteenth of a pixel below one pixel, is exactly its own near reading.
        if (body.YDirection == SamusVerticalDirection.None &&
            displacement.NearTwoFiftySixths > QuicksandDefinitions.StationarySurfaceProbeLimit.NearTwoFiftySixths)
            displacement = displacement.WithNearTwoFiftySixths(QuicksandDefinitions.StationarySurfaceProbeLimit.NearTwoFiftySixths);
        surfaceContact = true;
        return SpecialAirCollisionOutcome.PassesThrough;
    }
}
