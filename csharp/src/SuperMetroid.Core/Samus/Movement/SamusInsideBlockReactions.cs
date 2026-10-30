using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Body-overlap scroll, conveyor and sand reactions from bank-$94 BlockInsideDetection.</summary>
public static class SamusInsideBlockReactions
{
    /// <summary>Samples bottom, center, and top in native order ($94:9B60).</summary>
    internal static void PrepareFrame(RoomLevelData level, SamusState samus,
        AreaId area, bool areaBossDefeated = false, ISamusRoomPlms? plms = null)
    {
        var body = samus.Kinematics;
        body.SandCollisionArea = area;
        SamusInsideBlockSamplePoints.Visit(body.Position.Y.Whole, body.Radius.Y, (y, point) =>
            Visit(y, point == SamusInsideBlockPoint.Bottom, point == SamusInsideBlockPoint.Center));

        void Visit(int y, bool bottomPoint, bool centerPoint)
        {
            var block = level.GetCollisionBlockOrEdgeSolid(new(unchecked((ushort)(body.Position.X.Whole >> 4)), unchecked((ushort)(y >> 4))));
            if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block) ||
                block.CollisionType != RoomCollisionType.SpecialAir)
                return;
            if (!block.Behavior.UsesAreaReactionTable)
            {
                // $94:9956 admits only inside-block sample one (the center).
                // Feet/head contacts belong to the separate movement scans. Keeping
                // this center wake-up lets a lower trigger win after the torso clears
                // an upper trigger, without changing native PLM slot priority.
                if (centerPoint && block.Behavior == RoomBlockBehaviorValues.ScrollTrigger &&
                    (plms is null || !plms.Scrolls.TryNotifyScrollTouch(block.Index)))
                    throw new InvalidOperationException($"Inside scroll trigger block {block.Index} has no active PLM owner.");
                // The normal table contains conveyors; area-table entries below own
                // sand. Both must be visited in the same bottom/center/top order.
                ApplyConveyor(block.Behavior.Value);
                return;
            }
            SpecialAirReactionDefinition areaReaction =
                SpecialAirReactions.ResolveInside(
                    area,
                    block.Behavior.AreaReactionIndex);
            PlmHeaderId header = areaReaction.Header;
            if (header == PlmHeaderId.None) return;
            bool hasCompiledReaction = QuicksandReactions.TryGetReaction(
                header,
                out QuicksandReactionDefinition reaction);
            SpecialAirReactionSetup setup = hasCompiledReaction
                ? reaction.Setup
                : areaReaction.Setup;
            if (hasCompiledReaction && plms is not null &&
                !plms.Quicksand.TrySpawnQuicksandReaction(block.Index, header))
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
                    (plms ?? throw new InvalidOperationException("Samus Eater reaction requires the room PLM owner."))
                        .SamusEater.TrySpawnSamusEater(level, block, header, samus.RoomView);
                    break;
                case SpecialAirReactionSetup.QuicksandSurface:
                    // Native setup cancels running momentum even for center/top samples,
                    // but preserves the lower fractional bits rather than zeroing base X.
                    var speed = samus.HorizontalSpeed;
                    speed.HasRunningMomentum = false;
                    speed.SpeedBoostCounter = default;
                    speed.EchoSoundFlag = 0;
                    speed.ExtraRunSpeed = default;
                    speed.BaseSpeed = new UnsignedSixteenSixteen(0, speed.BaseSpeed.Fraction & (ushort)short.MaxValue);
                    if (!bottomPoint) return;
                    QuicksandSurfacePhysics physics =
                        QuicksandReactions.SurfacePhysics(
                            samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit));
                    // $84:B433 indexes a four-entry table with the direction's low two bits;
                    // its fourth entry repeats the grounded handler, but $0B36 only ever
                    // holds the three defined directions.
                    switch (body.YDirection)
                    {
                        case SamusVerticalDirection.None:
                            body.YSpeed = default;
                            SetExtra(physics.StationaryDisplacement);
                            break;
                        case SamusVerticalDirection.Up:
                            SignedSixteenSixteen limit = physics.UpwardSpeedLimit;
                            // The original compares the middle words of the 16.16 speed and limit, then
                            // replaces the speed's whole pixels and fraction with the limit's.
                            if ((ushort)(EightEight.WholeOf(body.VerticalSpeedFixed)) > EightEight.WholeOf(limit.RawValue))
                            {
                                body.YSpeed = new UnsignedSixteenSixteen(limit.WholeWord, limit.Fraction);
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
        void ApplyConveyor(int behavior)
        {
            // The normal special-air table shares its behaviour space; only conveyor entries react here.
            ConveyorBlockBts? carrier = behavior switch
            {
                8 => ConveyorBlockBts.GroundedRight,
                9 => ConveyorBlockBts.GroundedLeft,
                10 => ConveyorBlockBts.UnconditionalRight,
                11 => ConveyorBlockBts.UnconditionalLeft,
                _ => null,
            };
            if (carrier is not { } conveyor) return;
            bool groundedOnly = conveyor is ConveyorBlockBts.GroundedRight or ConveyorBlockBts.GroundedLeft;
            bool rightward = conveyor is ConveyorBlockBts.GroundedRight or ConveyorBlockBts.UnconditionalRight;
            samus.HorizontalSpeed.SelectNormalAirSpeedTable();
            if (groundedOnly && ((area == AreaId.WreckedShip && !areaBossDefeated) || body.YSpeed.Whole != 0))
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
    /// Executes the sand subset of the area-specific collision PLM table. Contact is
    /// separate from carry: native downward movement grounds on sand without discarding
    /// its accepted sinking displacement. Pose-clearance probes consume carry only.
    /// </summary>
    internal static bool ReactCollision(SamusCollisionContact contact,
        RoomCollisionBlock block, Axis axis, ref int displacement, out bool surfaceContact,
        SamusCollisionDirection? blockReactionDirection = null,
        ISamusRoomPlms? plms = null)
    {
        SamusKinematicsState body = contact.Body;
        surfaceContact = false;
        if (!block.Behavior.UsesAreaReactionTable) return false;
        SpecialAirReactionDefinition areaReaction =
            SpecialAirReactions.ResolveCollision(
                body.SandCollisionArea,
                block.Behavior.AreaReactionIndex);
        PlmHeaderId header = areaReaction.Header;
        if (header == PlmHeaderId.None) return false;
        bool hasCompiledReaction = QuicksandReactions.TryGetReaction(
            header,
            out QuicksandReactionDefinition reaction);
        SpecialAirReactionSetup setup = hasCompiledReaction
            ? reaction.Setup
            : areaReaction.Setup;
        if (hasCompiledReaction && plms is not null &&
            !plms.Quicksand.TrySpawnQuicksandReaction(block.Index, header))
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
                ? displacement < 0 ? SamusCollisionDirection.Up : SamusCollisionDirection.Down
                : displacement < 0 ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
        // The native bit-one direction test accepts vertical scans AND direction $F.
        // Ordinary horizontal movement rejects the surface callback entirely.
        if (reactionDirection is SamusCollisionDirection.Left or SamusCollisionDirection.Right) return false;
        // $84:B4E4 dispatches on the direction (its fourth, grounded entry is never
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
            displacement = 0;
            return true;
        }
        if (body.YDirection == SamusVerticalDirection.None && (ushort)(EightEight.WholeOf((uint)displacement)) > EightEight.WholeOf(QuicksandDefinitions.StationarySurfaceProbeLimit.RawValue))
            displacement = (displacement & ~(ushort.MaxValue << 8)) | QuicksandDefinitions.StationarySurfaceProbeLimit.RawValue;
        surfaceContact = true;
        return false;
    }
}
