using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Body-overlap scroll, conveyor and sand reactions from Samus's inside-block detection.</summary>
public static partial class SamusInsideBlockReactions
{
    /// <summary>Samples bottom, center, and top in order.</summary>
    /// <param name="level">The room's terrain.</param>
    /// <param name="samus">Samus, whose body the reactions sample and move.</param>
    /// <param name="roomHasPower">Whether the room has the power its on-foot conveyors need to run, as the room supplies it.</param>
    /// <param name="roomObjects">The room's objects the reactions wake.</param>
    internal static void PrepareFrame(
        RoomLevelData level,
        SamusState samus,
        bool roomHasPower,
        ISamusRoomObjects roomObjects)
    {
        var body = samus.Kinematics;
        SamusInsideBlockSamplePoints.Visit(body.Position.Y.Whole, body.Radius.Y, Visit);

        void Visit(int y, SamusInsideBlockPoint point)
        {
            var block = level.GetCollisionBlockOrEdgeSolid(new(RoomGridConversions.PixelToBlock(body.Position.X.Whole), RoomGridConversions.PixelToBlock(y)));
            if (block.Position is null || !level.TryReachBlockStoodFor(ref block) ||
                block.CollisionType != RoomCollisionType.SpecialAir)
                return;
            // A scroll trigger wakes only from the inside-block sample at Samus's centre.
            // Feet/head contacts belong to the separate movement scans. Keeping
            // this center wake-up lets a lower trigger win after the torso clears
            // an upper trigger, without changing the room-object update order.
            if (point == SamusInsideBlockPoint.Center && block.Behavior.SpecialAir == SpecialAirReaction.ScrollTrigger &&
                !roomObjects.Scrolls.TryNotifyScrollTouch(block.At))
                throw new InvalidOperationException($"Inside scroll trigger block {block.Position} has no active room-object owner.");
            // The conveyors carry Samus the same in every area; the reactions the area gives meaning
            // own sand and plants. Both are visited in the same bottom/center/top order.
            if (block.Behavior.SpecialAir is { } conveyed && ConveyorBlockDefinitions.Conveyors.TryGetValue(conveyed, out ConveyorBlock conveyor))
            {
                ApplyConveyor(conveyor);
                return;
            }
            SpecialAirInsideReaction reaction = SpecialAirReactions.InsideOf(block.Behavior.SpecialAir);
            // A sand reaction takes a room-object place before it acts; with every place taken it does nothing.
            if (QuicksandReactions.PlacesObject(reaction) &&
                !roomObjects.Quicksand.TrySpawnQuicksandReaction(block.At, reaction))
            {
                return;
            }
            if (reaction.Plant is { } plant)
                roomObjects.SamusEater.TrySpawnSamusEater(level, block, plant, samus.RoomView);
            else if (reaction.IsQuicksandSurface)
            {
                // The game's setup cancels running momentum even for center/top samples,
                // but keeps the base speed's fraction of a pixel rather than stopping it.
                var speed = samus.HorizontalSpeed;
                speed.HasRunningMomentum = false;
                speed.SpeedBoostCounter = SamusSpeedBoostCounter.NotBuilding;
                speed.EchoSoundPlaying = false;
                speed.ExtraRunSpeed = default;
                speed.BaseSpeed = QuicksandSurfaceSpeedLeftover.Leftover(speed.BaseSpeed);
                if (point != SamusInsideBlockPoint.Bottom) return;
                QuicksandSurfacePhysics physics =
                    QuicksandReactions.SurfacePhysics(
                        samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit));
                // One reaction per vertical direction; a still Samus takes the grounded reaction.
                switch (body.VerticalVelocity.Direction)
                {
                    case SamusVerticalDirection.None:
                        body.VerticalVelocity = body.VerticalVelocity.Stopped();
                        SetExtra(physics.StationaryDisplacement);
                        break;
                    case SamusVerticalDirection.Up:
                        SignedSixteenSixteen limit = physics.UpwardSpeedLimit;
                        // A speed past the limit takes the limit's whole pixels and fraction.
                        if (body.VerticalVelocity.Speed.IsFasterInTwoFiftySixthsThan(limit))
                        {
                            body.VerticalVelocity = body.VerticalVelocity.WithSpeed(UnsignedSixteenSixteen.FromDisplacement(limit));
                        }
                        SetExtra(physics.MovingDisplacement);
                        break;
                    case SamusVerticalDirection.Down:
                        SetExtra(physics.MovingDisplacement);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Undefined {nameof(SamusVerticalDirection)} {body.VerticalVelocity.Direction}.");
                }
            }
            else if (reaction.Sinking is { } sinking)
                SetExtra(sinking);
        }
        void ApplyConveyor(ConveyorBlock conveyor)
        {
            bool rightward = conveyor.Direction == HorizontalDirection.Right;
            samus.HorizontalSpeed.SelectAirSpeeds();
            if (conveyor.Condition == ConveyorCarryCondition.OnFootWithPower &&
                (!roomHasPower || body.VerticalVelocity.Speed.Whole != 0))
                return;
            // Only whole pixels of Y speed stop the carry: fractional vertical motion does
            // not. This replaces external X displacement, never adds to it.
            body.ExtraDisplacement = body.ExtraDisplacement.WithX(rightward ? ConveyorBlockDefinitions.RightwardCarry : ConveyorBlockDefinitions.LeftwardCarry);
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
    /// <returns>Whether the block lets Samus's movement pass, lets it pass landing on the sand's surface, or stops it.</returns>
    /// <param name="contact">Samus's body and contact damage.</param>
    /// <param name="terrain">The room's level, which places the sand block a reaction sits on, and its objects, where a sand
    /// reaction takes a place.</param>
    /// <param name="block">The special-air block the collision scan reached.</param>
    /// <param name="axis">The axis the scan moves along.</param>
    /// <param name="displacement">The scan's accepted displacement, which sand can limit.</param>
    /// <param name="observes">Whether the scan only observes, so the surface sees no direction of travel.</param>
    internal static SpecialAirCollisionOutcome ReactCollision(
        SamusCollisionContact contact,
        SamusTerrain terrain,
        RoomCollisionBlock block,
        Axis axis,
        ref SignedSixteenSixteen displacement,
        bool observes = false)
    {
        RoomLevelData level = terrain.Level;
        ISamusRoomObjects roomObjects = terrain.RoomObjects;
        SamusKinematicsState body = contact.Body;
        SpecialAirCollisionReaction reaction = SpecialAirReactions.CollisionOf(block.Behavior.SpecialAir);
        // A sand reaction takes a room-object place before it acts; with every place taken Samus passes as through air.
        if (QuicksandReactions.PlacesObject(reaction) &&
            !roomObjects.Quicksand.TrySpawnQuicksandReaction(block.At, reaction))
        {
            return SpecialAirCollisionOutcome.PassesThrough;
        }
        if (reaction.StopsFall)
        {
            body.VerticalVelocity = body.VerticalVelocity.Stopped();
            body.YAcceleration = default;
            return SpecialAirCollisionOutcome.PassesThrough;
        }
        if (!reaction.IsQuicksandSurface) return SpecialAirCollisionOutcome.PassesThrough;
        // The surface reacts to vertical scans and to observing probes; ordinary horizontal movement passes through it
        // untouched.
        if (!observes && axis == Axis.Horizontal) return SpecialAirCollisionOutcome.PassesThrough;
        // An observing probe moves in no direction; a vertical scan moves down unless it rises.
        bool movesDown = !observes && !displacement.IsNegative;
        // A rising Samus passes through; a stationary one lands only on an actual downward collision; a falling one lands
        // even during pose-observation probes. The grounded vertical direction never reaches the surface.
        switch (body.VerticalVelocity.Direction)
        {
            case SamusVerticalDirection.Up:
                return SpecialAirCollisionOutcome.PassesThrough;
            case SamusVerticalDirection.None:
                if (!movesDown) return SpecialAirCollisionOutcome.PassesThrough;
                break;
            case SamusVerticalDirection.Down:
                break;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(SamusVerticalDirection)} {body.VerticalVelocity.Direction}.");
        }
        if (contact.ContactDamage == SamusBodyAttack.SpeedBooster)
        {
            displacement = SignedSixteenSixteen.Zero;
            return SpecialAirCollisionOutcome.Blocks;
        }
        // The probe here moves down (only a scan's own downward displacement reaches this for a stationary Samus; the
        // pose-observation probe passed through above) and by far less than 256 pixels.
        if (body.VerticalVelocity.HasNoDirection)
            displacement = HeldAtQuicksandSurface(displacement);
        return SpecialAirCollisionOutcome.LandsOnSurface;
    }

    /// <summary>
    /// A stationary Samus's downward <paramref name="probe"/> held at the quicksand surface (#2555): sunk no further than
    /// <see cref="QuicksandDefinitions.StationarySurfaceProbeLimit"/>, compared at the precision of the game's speeds, the
    /// probe's finer fraction kept as it was (<see cref="SignedSixteenSixteen.WithTwoFiftySixthsAtMost"/>).
    /// </summary>
    private static SignedSixteenSixteen HeldAtQuicksandSurface(SignedSixteenSixteen probe) =>
        probe.WithTwoFiftySixthsAtMost(QuicksandDefinitions.StationarySurfaceProbeLimit);
}
