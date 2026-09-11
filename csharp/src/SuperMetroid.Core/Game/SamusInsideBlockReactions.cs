using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>Body-overlap scroll, conveyor and sand reactions from bank-$94 BlockInsideDetection.</summary>
public static class SamusInsideBlockReactions
{
    /// <summary>Samples bottom, center, and top in native order, visiting each block row only once.</summary>
    public static void PrepareFrame(ISnesAddressSpace bus, RoomLevelData level, SamusState samus,
        AreaId area, bool areaBossDefeated = false, RoomPlmSystem? plms = null)
    {
        var body = samus.Kinematics;
        body.SandCollisionArea = area;
        ushort bottom = unchecked((ushort)(body.YPosition + body.YRadius - 1));
        ushort top = unchecked((ushort)(body.YPosition - body.YRadius));
        Visit(bottom, true, false);
        if ((bottom >> 4) != (body.YPosition >> 4)) Visit(body.YPosition, false, true);
        if ((top >> 4) != (bottom >> 4) && (top >> 4) != (body.YPosition >> 4)) Visit(top, false, false);

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
            ushort table = Word(QuicksandRomData.InsideAreaTables + AreaIds.ToIndex(area) * 2);
            ushort header = Word(QuicksandRomData.CollisionBank | unchecked((ushort)(table + block.Bts.AreaReactionIndex * 2)));
            if (header == 0) return;
            ushort setup = Word(QuicksandRomData.PlmBank | header);
            switch (setup)
            {
                case SamusEaterPlmRomData.FloorSetup:
                case SamusEaterPlmRomData.CeilingSetup:
                    (plms ?? throw new InvalidOperationException("Samus Eater reaction requires the room PLM owner."))
                        .TrySpawnSamusEater(bus, level, block, header,
                            setup == SamusEaterPlmRomData.CeilingSetup, samus);
                    break;
                case QuicksandRomData.SurfaceSetup:
                    // Native setup cancels running momentum even for center/top samples,
                    // but preserves the lower fractional bits rather than zeroing base X.
                    var speed = samus.HorizontalSpeed;
                    speed.HasRunningMomentum = false;
                    speed.SpeedBoostCounter = 0;
                    speed.EchoSoundRequested = false;
                    speed.ExtraRunSpeed = speed.ExtraRunSubspeed = speed.BaseSpeed = 0;
                    speed.BaseSubspeed &= 0x7fff;
                    if (!bottomPoint) return;
                    int suitIndex = (samus.EquippedItems & (ushort)SamusEquipmentFlags.GravitySuit) != 0 ? 2 : 0;
                    switch (body.YDirection & 3)
                    {
                        case 0:
                        case 3:
                            body.YSpeed = body.YSubspeed = 0;
                            SetExtra(Word(QuicksandRomData.StationarySurfaceDisplacement + suitIndex) << 8);
                            break;
                        case 1:
                            ushort limit = Word(QuicksandRomData.SurfaceJumpLimit + suitIndex);
                            // The original compares the middle word of the 16.16 speed.
                            if ((ushort)(body.VerticalSpeedFixed >> 8) > limit)
                            {
                                body.YSpeed = (ushort)(limit >> 8);
                                body.YSubspeed = unchecked((ushort)(limit << 8));
                            }
                            SetExtra(Word(QuicksandRomData.MovingSurfaceDisplacement + suitIndex) << 8);
                            break;
                        case 2:
                            SetExtra(Word(QuicksandRomData.MovingSurfaceDisplacement + suitIndex) << 8);
                            break;
                    }
                    break;
                case QuicksandRomData.SubmergingSetup:
                    SetExtra(QuicksandRomData.SubmergingDisplacement);
                    break;
                case QuicksandRomData.SlowFallsSetup:
                    SetExtra(QuicksandRomData.SlowFallsDisplacement);
                    break;
                case QuicksandRomData.FastFallsSetup:
                    SetExtra(QuicksandRomData.FastFallsDisplacement);
                    break;
            }
        }
        ushort Word(int address) => RomDataReader.ReadWordFixedBank(bus, address);
        void ApplyConveyor(byte bts)
        {
            bool groundedOnly = bts is ConveyorBlockRomData.GroundedRight or ConveyorBlockRomData.GroundedLeft;
            bool always = bts is ConveyorBlockRomData.UnconditionalRight or ConveyorBlockRomData.UnconditionalLeft;
            if (!groundedOnly && !always) return;
            samus.HorizontalSpeed.SelectNormalAirSpeedTable();
            if (groundedOnly && ((area == AreaId.WreckedShip && !areaBossDefeated) || body.YSpeed != 0))
                return;
            // The original checks only whole Y speed: fractional vertical motion does
            // not suppress carry. This replaces external X displacement, never adds to it.
            body.ExtraXSubdisplacement = 0;
            body.ExtraXDisplacement = bts is ConveyorBlockRomData.GroundedRight or ConveyorBlockRomData.UnconditionalRight
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
    public static bool ReactCollision(ISnesAddressSpace bus, SamusKinematicsState body,
        RoomCollisionBlock block, bool vertical, ref int displacement, out bool surfaceContact,
        SamusCollisionDirection? blockReactionDirection = null)
    {
        surfaceContact = false;
        if (!block.Bts.UsesAreaReactionTable) return false;
        ushort table = RomDataReader.ReadWordFixedBank(bus,
            QuicksandRomData.CollisionAreaTables + AreaIds.ToIndex(body.SandCollisionArea) * 2);
        ushort header = RomDataReader.ReadWordFixedBank(bus, QuicksandRomData.CollisionBank |
            unchecked((ushort)(table + block.Bts.AreaReactionIndex * 2)));
        if (header == 0) return false;
        ushort setup = RomDataReader.ReadWordFixedBank(bus, QuicksandRomData.PlmBank | header);
        if (setup == QuicksandRomData.SubmergingCollision)
        {
            body.YSpeed = body.YSubspeed = body.YAcceleration = body.YSubacceleration = 0;
            return false;
        }
        if (setup != QuicksandRomData.SurfaceCollision) return false;
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
