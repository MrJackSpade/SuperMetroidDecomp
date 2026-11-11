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
            SpecialAirReactionDefinition areaReaction =
                SpecialAirReactions.ResolveInside(
                    area,
                    block.Behavior.Reaction.Entry);
            RoomObjectHeaderId header = areaReaction.Header;
            if (header == RoomObjectHeaderId.None) return;
            bool hasCompiledReaction = QuicksandReactions.TryGetReaction(
                header,
                out QuicksandReactionDefinition reaction);
            SpecialAirReactionSetup setup = hasCompiledReaction
                ? reaction.Setup
                : areaReaction.Setup;
            if (hasCompiledReaction && roomObjects is not null &&
                !roomObjects.Quicksand.TrySpawnQuicksandReaction(level.BlockAt(block.Index), header))
            {
                return;
            }
            switch (setup)
            {
                case SpecialAirReactionSetup.Nothing:
                case SpecialAirReactionSetup.IcePhysics:
                    // Neither inside reaction changes Samus's motion here.
                    break;
                case SpecialAirReactionSetup.BrinstarFloorPlant:
                case SpecialAirReactionSetup.BrinstarCeilingPlant:
                    (roomObjects ?? throw new InvalidOperationException("Samus Eater reaction requires the room-object owner."))
                        .SamusEater.TrySpawnSamusEater(level, block, header, samus.RoomView);
                    break;
                case SpecialAirReactionSetup.QuicksandSurface:
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
                            // The original compares the speed and limit in 256ths of a pixel, then
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
                case SpecialAirReactionSetup.SubmergingQuicksand:
                    SetExtra(QuicksandDefinitions.SubmergingSinkDisplacement);
                    break;
                case SpecialAirReactionSetup.SandFallsSlow:
                    SetExtra(QuicksandDefinitions.SlowSandfallDisplacement);
                    break;
                case SpecialAirReactionSetup.SandFallsFast:
                    SetExtra(QuicksandDefinitions.FastSandfallDisplacement);
                    break;
                case SpecialAirReactionSetup.ClearCarry:
                case SpecialAirReactionSetup.QuicksandSurfaceCollision:
                case SpecialAirReactionSetup.SubmergingQuicksandCollision:
                case SpecialAirReactionSetup.SandFallsCollision:
                case SpecialAirReactionSetup.SpeedBlock:
                case SpecialAirReactionSetup.LowerNorfairChozoHand:
                case SpecialAirReactionSetup.WreckedShipChozoHand:
                    throw new InvalidOperationException(
                        $"Collision-only setup {setup} reached the inside-block reaction.");
                default:
                    throw new InvalidOperationException($"Undefined special-air setup {setup}.");
            }
        }
        void ApplyConveyor(BlockReaction reaction)
        {
            // The common special-air reactions include the conveyors; only they react here.
            if (!ConveyorBlockDefinitions.Conveyors.TryGetValue(reaction, out ConveyorBlock conveyor)) return;
            bool rightward = conveyor.Direction == HorizontalDirection.Right;
            samus.HorizontalSpeed.SelectNormalAirSpeedTable();
            if (conveyor.Condition == ConveyorCarryCondition.OnFootWithPower &&
                ((area == AreaId.WreckedShip && !areaBossDefeated) || body.YSpeed.Whole != 0))
                return;
            // The original checks only whole Y speed: fractional vertical motion does
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
    /// Executes the sand subset of the area-specific collision room-object table. Contact is
    /// separate from carry: downward movement grounds on sand without discarding
    /// its accepted sinking displacement. Pose-clearance probes consume carry only.
    /// </summary>
    /// <param name="contact">Samus's body and contact damage.</param>
    /// <param name="level">The room's level, which places the sand block a reaction sits on.</param>
    /// <param name="block">The special-air block the collision scan reached.</param>
    /// <param name="axis">The axis the scan moves along.</param>
    /// <param name="displacement">The scan's accepted displacement, which sand can limit.</param>
    /// <param name="surfaceContact">Whether Samus touched the sand's surface.</param>
    /// <param name="blockReactionDirection">The direction the reaction sees, or null for the scan's own.</param>
    /// <param name="roomObjects">The room's objects, where a sand reaction takes a place; null runs no spawn.</param>
    internal static bool ReactCollision(SamusCollisionContact contact,
        RoomLevelData level, RoomCollisionBlock block, Axis axis, ref SignedSixteenSixteen displacement, out bool surfaceContact,
        SamusCollisionDirection? blockReactionDirection = null,
        ISamusRoomObjects? roomObjects = null)
    {
        SamusKinematicsState body = contact.Body;
        surfaceContact = false;
        if (!block.Behavior.Reaction.IsAreaSpecific) return false;
        SpecialAirReactionDefinition areaReaction =
            SpecialAirReactions.ResolveCollision(
                body.SandCollisionArea,
                block.Behavior.Reaction.Entry);
        RoomObjectHeaderId header = areaReaction.Header;
        if (header == RoomObjectHeaderId.None) return false;
        bool hasCompiledReaction = QuicksandReactions.TryGetReaction(
            header,
            out QuicksandReactionDefinition reaction);
        SpecialAirReactionSetup setup = hasCompiledReaction
            ? reaction.Setup
            : areaReaction.Setup;
        if (hasCompiledReaction && roomObjects is not null &&
            !roomObjects.Quicksand.TrySpawnQuicksandReaction(level.BlockAt(block.Index), header))
        {
            return false;
        }
        if (setup == SpecialAirReactionSetup.SubmergingQuicksandCollision)
        {
            body.YSpeed = default;
            body.YAcceleration = default;
            return false;
        }
        if (setup != SpecialAirReactionSetup.QuicksandSurfaceCollision) return false;
        SamusCollisionDirection reactionDirection = blockReactionDirection ??
            (axis == Axis.Vertical
                ? displacement.IsNegative ? SamusCollisionDirection.Up : SamusCollisionDirection.Down
                : displacement.IsNegative ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
        // The bit-one direction test accepts vertical scans AND direction $F.
        // Ordinary horizontal movement rejects the surface callback entirely.
        if (reactionDirection is SamusCollisionDirection.Left or SamusCollisionDirection.Right) return false;
        // Dispatches on the direction (its fourth, grounded entry is never
        // reached). Rising clears carry; stationary requires actual downward collision;
        // falling remains eligible even during direction-$F pose checks.
        switch (body.YDirection)
        {
            case SamusVerticalDirection.Up:
                return false;
            case SamusVerticalDirection.None:
                if (reactionDirection != SamusCollisionDirection.Down) return false;
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
            return true;
        }
        // A stationary Samus's probe is compared with the surface limit and limited to it in its near reading alone,
        // in 256ths of a pixel below 256 whole pixels (SignedSixteenSixteen.NearTwoFiftySixths); the limit, a multiple
        // of a sixteenth of a pixel below one pixel, is exactly its own near reading.
        if (body.YDirection == SamusVerticalDirection.None &&
            displacement.NearTwoFiftySixths > QuicksandDefinitions.StationarySurfaceProbeLimit.NearTwoFiftySixths)
            displacement = displacement.WithNearTwoFiftySixths(QuicksandDefinitions.StationarySurfaceProbeLimit.NearTwoFiftySixths);
        surfaceContact = true;
        return false;
    }
}
