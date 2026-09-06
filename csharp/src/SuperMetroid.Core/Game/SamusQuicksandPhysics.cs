using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>Sand-family inside-body reactions from BlockInsideDetection and bank-$84 setup.</summary>
public static class SamusQuicksandPhysics
{
    /// <summary>Samples bottom, center, and top in native order, visiting each block row only once.</summary>
    public static void PrepareFrame(ISnesAddressSpace bus, RoomLevelData level, SamusState samus, AreaId area)
    {
        var body = samus.Kinematics;
        body.SandCollisionArea = area;
        ushort bottom = unchecked((ushort)(body.YPosition + body.YRadius - 1));
        ushort top = unchecked((ushort)(body.YPosition - body.YRadius));
        Visit(bottom, true);
        if ((bottom >> 4) != (body.YPosition >> 4)) Visit(body.YPosition, false);
        if ((top >> 4) != (bottom >> 4) && (top >> 4) != (body.YPosition >> 4)) Visit(top, false);

        void Visit(ushort y, bool bottomPoint)
        {
            var block = level.GetCollisionBlockOrPrefilledSolid(body.XPosition >> 4, y >> 4);
            if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block) ||
                block.CollisionType != RoomCollisionType.SpecialAir || !block.Bts.UsesAreaReactionTable)
                return;
            ushort table = Word(QuicksandRomData.InsideAreaTables + AreaIds.ToIndex(area) * 2);
            ushort header = Word(QuicksandRomData.CollisionBank | unchecked((ushort)(table + block.Bts.AreaReactionIndex * 2)));
            if (header == 0) return;
            ushort setup = Word(QuicksandRomData.PlmBank | header);
            switch (setup)
            {
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
        RoomCollisionBlock block, bool vertical, ref int displacement, out bool surfaceContact)
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
        if (setup != QuicksandRomData.SurfaceCollision || !vertical) return false;
        int direction = body.YDirection & 3;
        if (direction == 1 || (direction != 2 && displacement < 0)) return false;
        if (body.SamusOwner?.HorizontalSpeed.ContactDamageIndex == 1)
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
