using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Body-overlap scroll, conveyor and sand reactions from bank-$94 BlockInsideDetection.</summary>
public static class SamusInsideBlockReactions
{
    /// <summary>Samples bottom, center, and top in native order ($94:9B60).</summary>
    public static void PrepareFrame(RoomLevelData level, SamusState samus,
        AreaId area, bool areaBossDefeated = false, RoomPlmSystem? plms = null)
    {
        var body = samus.Kinematics;
        body.SandCollisionArea = area;
        SamusInsideBlockSamplePoints.Visit(body.YPosition, body.YRadius, (y, point) =>
            Visit(y, point == SamusInsideBlockPoint.Bottom, point == SamusInsideBlockPoint.Center));

        void Visit(ushort y, bool bottomPoint, bool centerPoint)
        {
            var block = level.GetCollisionBlockOrPrefilledSolid(body.XPosition >> 4, y >> 4);
            if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block) ||
                block.CollisionType != RoomCollisionType.SpecialAir)
                return;
            if (!block.Bts.UsesAreaReactionTable)
            {
                // $94:9956 admits only inside-block sample one (the center).
                // Feet/head contacts belong to the separate movement scans. Keeping
                // this center wake-up lets a lower trigger win after the torso clears
                // an upper trigger, without changing native PLM slot priority.
                if (centerPoint && block.Bts == RoomBlockBehaviorValues.ScrollTrigger &&
                    (plms is null || !plms.TryNotifyScrollTouch(block.Index)))
                    throw new InvalidOperationException($"Inside scroll trigger block {block.Index} has no active PLM owner.");
                // The normal table contains conveyors; area-table entries below own
                // sand. Both must be visited in the same bottom/center/top order.
                ApplyConveyor(block.Behavior);
                return;
            }
            SpecialAirReactionDefinition areaReaction =
                SpecialAirReactionDefinitions.ResolveInside(
                    area,
                    block.Bts.AreaReactionIndex);
            PlmHeaderId header = areaReaction.HeaderPointer;
            if (header == 0) return;
            bool hasCompiledReaction = QuicksandDefinitions.TryGetReaction(
                header,
                out QuicksandReactionDefinition reaction);
            SpecialAirReactionSetup setup = hasCompiledReaction
                ? reaction.SetupPointer
                : areaReaction.SetupPointer;
            if (hasCompiledReaction && plms is not null &&
                !plms.TrySpawnQuicksandReaction(block.Index, header))
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
                        .TrySpawnSamusEater(level, block, header, samus);
                    break;
                case SpecialAirReactionSetup.QuicksandSurface:
                    // Native setup cancels running momentum even for center/top samples,
                    // but preserves the lower fractional bits rather than zeroing base X.
                    var speed = samus.HorizontalSpeed;
                    speed.HasRunningMomentum = false;
                    speed.SpeedBoostCounter = 0;
                    speed.EchoSoundFlag = 0;
                    speed.ExtraRunSpeed = speed.ExtraRunSubspeed = speed.BaseSpeed = 0;
                    speed.BaseSubspeed &= 0x7fff;
                    if (!bottomPoint) return;
                    QuicksandSurfacePhysics physics =
                        QuicksandDefinitions.SurfacePhysics(
                            (samus.EquippedItems &
                                (ushort)SamusEquipmentFlags.GravitySuit) != 0);
                    switch (body.YDirection & 3)
                    {
                        case 0:
                        case 3:
                            body.YSpeed = body.YSubspeed = 0;
                            SetExtra(physics.StationaryDisplacement << 8);
                            break;
                        case 1:
                            ushort limit = physics.UpwardSpeedLimit;
                            // The original compares the middle word of the 16.16 speed.
                            if ((ushort)(body.VerticalSpeedFixed >> 8) > limit)
                            {
                                body.YSpeed = (ushort)(limit >> 8);
                                body.YSubspeed = unchecked((ushort)(limit << 8));
                            }
                            SetExtra(physics.MovingDisplacement << 8);
                            break;
                        case 2:
                            SetExtra(physics.MovingDisplacement << 8);
                            break;
                    }
                    break;
                case SpecialAirReactionSetup.SubmergingQuicksand:
                    SetExtra(QuicksandRomData.SubmergingDisplacement);
                    break;
                case SpecialAirReactionSetup.SandFallsSlow:
                    SetExtra(QuicksandRomData.SlowFallsDisplacement);
                    break;
                case SpecialAirReactionSetup.SandFallsFast:
                    SetExtra(QuicksandRomData.FastFallsDisplacement);
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
        void ApplyConveyor(byte bts)
        {
            // The normal special-air table shares its BTS space; only conveyor entries react here.
            if (!Enum.IsDefined((ConveyorBlockBts)bts)) return;
            var conveyor = (ConveyorBlockBts)bts;
            (bool groundedOnly, bool rightward) = conveyor switch
            {
                ConveyorBlockBts.GroundedRight => (true, true),
                ConveyorBlockBts.GroundedLeft => (true, false),
                ConveyorBlockBts.UnconditionalRight => (false, true),
                ConveyorBlockBts.UnconditionalLeft => (false, false),
                _ => throw new InvalidOperationException($"Undefined {nameof(ConveyorBlockBts)} {(int)conveyor}."),
            };
            samus.HorizontalSpeed.SelectNormalAirSpeedTable();
            if (groundedOnly && ((area == AreaId.WreckedShip && !areaBossDefeated) || body.YSpeed != 0))
                return;
            // The original checks only whole Y speed: fractional vertical motion does
            // not suppress carry. This replaces external X displacement, never adds to it.
            body.ExtraXSubdisplacement = 0;
            body.ExtraXDisplacement = rightward
                ? ConveyorBlockRomData.RightDisplacement : ConveyorBlockRomData.LeftDisplacement;
        }
        void SetExtra(int displacement)
        {
            body.ExtraYDisplacement = (ushort)(displacement >> 16);
            body.ExtraYSubdisplacement = unchecked((ushort)displacement);
        }
    }

    /// <summary>
    /// Executes the sand subset of the area-specific collision PLM table. Contact is
    /// separate from carry: native downward movement grounds on sand without discarding
    /// its accepted sinking displacement. Pose-clearance probes consume carry only.
    /// </summary>
    public static bool ReactCollision(SamusKinematicsState body,
        RoomCollisionBlock block, bool vertical, ref int displacement, out bool surfaceContact,
        SamusCollisionDirection? blockReactionDirection = null,
        RoomPlmSystem? plms = null)
    {
        surfaceContact = false;
        if (!block.Bts.UsesAreaReactionTable) return false;
        SpecialAirReactionDefinition areaReaction =
            SpecialAirReactionDefinitions.ResolveCollision(
                body.SandCollisionArea,
                block.Bts.AreaReactionIndex);
        PlmHeaderId header = areaReaction.HeaderPointer;
        if (header == 0) return false;
        bool hasCompiledReaction = QuicksandDefinitions.TryGetReaction(
            header,
            out QuicksandReactionDefinition reaction);
        SpecialAirReactionSetup setup = hasCompiledReaction
            ? reaction.SetupPointer
            : areaReaction.SetupPointer;
        if (hasCompiledReaction && plms is not null &&
            !plms.TrySpawnQuicksandReaction(block.Index, header))
        {
            return false;
        }
        if (setup == SpecialAirReactionSetup.SubmergingQuicksandCollision)
        {
            body.YSpeed = body.YSubspeed = body.YAcceleration = body.YSubacceleration = 0;
            return false;
        }
        if (setup != SpecialAirReactionSetup.QuicksandSurfaceCollision) return false;
        int direction = body.YDirection & 3;
        SamusCollisionDirection reactionDirection = blockReactionDirection ??
            (vertical
                ? displacement < 0 ? SamusCollisionDirection.Up : SamusCollisionDirection.Down
                : displacement < 0 ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
        // The native bit-one direction test accepts vertical scans AND direction $F.
        // Ordinary horizontal movement rejects the surface callback entirely.
        if (reactionDirection is SamusCollisionDirection.Left or SamusCollisionDirection.Right) return false;
        // Stationary/unused vertical states require actual downward collision. Falling
        // remains eligible even during direction-$F pose checks, exactly as B4C4 branches.
        if (direction == 1 || (direction != 2 && reactionDirection != SamusCollisionDirection.Down)) return false;
        if (body.CollisionContactDamageIndex == 1)
        {
            displacement = 0;
            return true;
        }
        if (direction != 2 && (ushort)((uint)displacement >> 8) > (QuicksandRomData.SurfaceProbeLimit >> 8))
            displacement = (displacement & unchecked((int)0xff0000ff)) | QuicksandRomData.SurfaceProbeLimit;
        surfaceContact = true;
        return false;
    }
}
