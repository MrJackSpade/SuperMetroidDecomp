using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Beam, missile, super-missile, slope, and linked-projectile motion.
/// </summary>
public sealed partial class SamusProjectileSystem
{
    private bool RunNoWaveBeamPreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            ClearProjectile(slot);
            return false;
        }

        // `$90:AF00` allocates a persistent trail before applying acceleration or movement.
        // Thus the detached 8x8 OBJ records the beam's old world position and visibly falls
        // behind it. Failure to preserve this order makes the trail sit inside the projectile.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }

        int directionOffset = slot.PackedDirection.DirectionIndex * 2;
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + directionOffset))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + directionOffset))));

        SamusProjectileDirection direction = slot.PackedDirection.Direction;
        bool collided = direction switch
        {
            SamusProjectileDirection.UpFacingRight or
            SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or
            SamusProjectileDirection.UpFacingLeft => MoveVertically(level, slot, roomPlms),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left =>
                MoveHorizontally(level, slot, roomPlms),
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.UpLeft =>
                MoveHorizontally(level, slot, roomPlms) ||
                MoveVertically(level, slot, roomPlms),
            _ => throw new InvalidDataException(
                $"Beam slot contains invalid direction ${slot.PackedDirection.DirectionIndex:X2}."),
        };

        if (collided)
        {
            KillBeam(bus, slot);
            return true;
        }

        DeleteIfOutsideMovementWindow(slot, layer1X, layer1Y);

        return false;
    }

    private void RunWaveBeamPreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            ClearProjectile(slot);
            return;
        }

        // `$90:B0C3` and `$90:B0E4` differ only in the value reloaded after a trail timer
        // expires. The producer always starts at four; an uncharged low-family wave becomes
        // three only after its first emission. Spawning precedes acceleration and movement,
        // so the trail samples the old world position just like the no-wave family.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = slot.PreInstruction ==
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                    ? (ushort)3
                    : (ushort)4;
            SpawnTrail(bus, slot);
        }

        RunWaveBeamShared(bus, level, slot, layer1X, layer1Y, roomPlms);
    }

    private void RunHyperBeamPreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            ClearProjectile(slot);
            return;
        }

        // `$90:B159` falls directly into the shared Wave movement. Hyper deliberately has
        // no projectile-trail timer or SpawnProjectileTrail call; its long beam spritemap
        // and the separately counting muzzle flare provide the complete native presentation.
        RunWaveBeamShared(bus, level, slot, layer1X, layer1Y, roomPlms);
    }

    private void RunWaveBeamShared(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        RoomPlmSystem? roomPlms)
    {
        int direction = slot.PackedDirection.DirectionIndex;
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + direction * 2))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + direction * 2))));

        // `$94:A352/$A3E4` advance the same 16.16 positions and scan every block touched by
        // the projectile radii, but deliberately return carry clear unconditionally. That is
        // the defining Wave Beam behavior: block reactions may run, yet terrain never kills
        // or clips the shot. The span scanners below now publish ordinary shootable-block
        // PLMs while intentionally discarding their carry result, matching this fallthrough.
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            (slot.XPosition, slot.XSubposition) = AddVelocity(
                slot.XPosition,
                slot.XSubposition,
                slot.XVelocity);
            ScanHorizontalShotReactions(level, slot, roomPlms);
        }
        if (direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            (slot.YPosition, slot.YSubposition) = AddVelocity(
                slot.YPosition,
                slot.YSubposition,
                slot.YVelocity);
            ScanVerticalShotReactions(level, slot, roomPlms);
        }

        DeleteIfOutsideMovementWindow(slot, layer1X, layer1Y);
    }

    private bool RunMissilePreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            ClearProjectile(slot);
            return false;
        }

        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }

        int direction = slot.PackedDirection.DirectionIndex;

        // Missile pre-instruction `$90:AF8F-$AFA0` first applies the same small directional
        // acceleration as beams. On the ignition frame Missile_Func1 below replaces velocity,
        // so this addition is intentionally overwritten; subsequent frames retain it.
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + direction * 2))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + direction * 2))));

        if ((slot.Variable & 0xff00) == 0)
        {
            // `$90:C301` is the literal `$0100` ignition increment. Crossing into a nonzero
            // high byte re-runs `$90:B1F3` with that word as base 8.8 speed. A normal missile
            // crosses on its first alpha pass and therefore begins at exactly one pixel/frame.
            slot.Variable = unchecked((ushort)(slot.Variable + 0x0100));
            if ((slot.Variable & 0xff00) != 0)
                InitializeDirectionalVelocity(slot, unchecked((short)slot.Variable));
        }
        else
        {
            int acceleration = MissileAccelerations + direction * 4;
            slot.XVelocity = unchecked((short)(slot.XVelocity +
                unchecked((short)ReadWord(bus, acceleration))));
            slot.YVelocity = unchecked((short)(slot.YVelocity +
                unchecked((short)ReadWord(bus, acceleration + 2))));
        }

        bool collided = direction switch
        {
            0 or 4 or 5 or 9 => MoveMissileVertically(bus, level, slot, roomPlms),
            2 or 7 => MoveMissileHorizontally(bus, level, slot, roomPlms),
            1 or 3 or 6 or 8 =>
                MoveMissileHorizontally(bus, level, slot, roomPlms) ||
                MoveMissileVertically(bus, level, slot, roomPlms),
            _ => throw new InvalidDataException(
                $"Missile slot contains invalid direction ${direction:X2}."),
        };
        if (collided)
        {
            KillMissile(bus, slot, sharedProjectiles);
            return true;
        }

        DeleteIfOutsideMovementWindow(slot, layer1X, layer1Y);
        return false;
    }

    private bool RunSuperMissilePreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            ClearProjectile(slot);
            ClearAllSuperMissileLinks();
            return false;
        }

        // `$90:AFF3` begins with the producer's value four but reloads two after every
        // expiry. Super Missiles therefore emit twice as frequently as ordinary missiles;
        // both families still select the same `$B5A1` art through pointer entries $20/$21.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 2;
            SpawnTrail(bus, slot);
        }

        int direction = slot.PackedDirection.DirectionIndex;
        if ((slot.Variable & 0xff00) == 0)
        {
            // The shared missile accelerator crosses `$0100` on its first alpha pass. Only
            // the Super family immediately allocates `$90:BF46`'s invisible collision link;
            // the link's native byte index is retained in the variable's low byte.
            slot.Variable = unchecked((ushort)(slot.Variable + 0x0100));
            if ((slot.Variable & 0xff00) != 0)
            {
                InitializeDirectionalVelocity(slot, unchecked((short)slot.Variable));
                SpawnSuperMissileLink(bus, samus, slot);
            }
        }
        else
        {
            int acceleration = SuperMissileAccelerations + direction * 4;
            slot.XVelocity = unchecked((short)(slot.XVelocity +
                unchecked((short)ReadWord(bus, acceleration))));
            slot.YVelocity = unchecked((short)(slot.YVelocity +
                unchecked((short)ReadWord(bus, acceleration + 2))));
        }

        bool collided = false;
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileHorizontally(bus, level, slot, roomPlms);
            if (collided)
                KillMissile(bus, slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                bus, level, slot, vertical: false, sharedProjectiles, roomPlms);
        }
        if (!collided && direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileVertically(bus, level, slot, roomPlms);
            if (collided)
                KillMissile(bus, slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                bus, level, slot, vertical: true, sharedProjectiles, roomPlms);
        }

        if (DeleteIfOutsideMovementWindow(slot, layer1X, layer1Y))
        {
            ClearAllSuperMissileLinks();
        }
        return collided;
    }

    /// <summary>
    /// Applies the shared bank-$90 beam/missile deletion window after movement. The SNES
    /// routine allows a 64-pixel apron above and left of the viewport and extends through
    /// coordinate 319 on the other axes; centralizing those native bounds keeps projectile
    /// families from drifting apart during later translation.
    /// </summary>
    /// <returns>True when the projectile was outside the native movement window and cleared.</returns>
    private bool DeleteIfOutsideMovementWindow(
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        const int MinimumVisibleCoordinate = -64;
        const int MaximumVisibleCoordinateExclusive = 320;

        short screenX = unchecked((short)(slot.XPosition - layer1X));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        bool outside =
            screenX < MinimumVisibleCoordinate ||
            screenX >= MaximumVisibleCoordinateExclusive ||
            screenY < MinimumVisibleCoordinate ||
            screenY >= MaximumVisibleCoordinateExclusive;
        if (!outside)
            return false;

        ClearProjectile(slot);
        return true;
    }

    private void SpawnSuperMissileLink(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusProjectileSlot owner)
    {
        SamusProjectileSlot? link = _slots.FirstOrDefault(candidate => candidate.Damage == 0);
        if (link is null)
            return;

        link.ClearFields();
        link.Type = 0x8200;
        link.Direction = owner.Direction;
        link.XPosition = owner.XPosition;
        link.YPosition = owner.YPosition;

        // `$90:BF78` deliberately calls the ordinary muzzle initializer again after copying
        // the owner's coordinates. At ignition both positions are equivalent; retaining the
        // call matters for moving/transition poses whose cartridge origin tables can change.
        InitializePosition(bus, samus, link);
        ushort dataPointer = ReadWord(bus, SuperMissileLinkDataPointers + 4);
        link.Damage = ReadWord(bus, 0x930000 | dataPointer);
        link.InstructionPointer = ReadWord(bus, 0x930000 | unchecked((ushort)(dataPointer + 2)));
        link.InstructionTimer = 1;
        link.PreInstruction = SamusProjectilePreInstruction.SuperMissileLink;

        owner.Variable = unchecked((ushort)((owner.Variable & 0xff00) + link.NativeByteIndex));
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));
    }

    private void RunSuperMissileLinkPreInstruction(SamusProjectileSlot link)
    {
        // `$90:B075` leaves an ordinary link completely stationary. A high direction nibble
        // is a deletion signal and clears every `$x200` family slot, including its owner.
        if (!link.PackedDirection.HasLowByteLifecycleState)
            return;
        ClearProjectile(link);
        ClearAllSuperMissileLinks();
    }

    private void UpdateSuperMissileLinkAxis(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot owner,
        bool vertical,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if ((owner.Variable & 0xff00) == 0)
            return;

        int linkIndex = (owner.Variable & 0x00ff) >> 1;
        if ((uint)linkIndex >= (uint)_slots.Length)
            return;
        SamusProjectileSlot link = _slots[linkIndex];
        if (!link.IsActive)
            return;

        // A primary collision has already converted the owner to `$0800`. Both slow and fast
        // native branches then clear the invisible link rather than allowing a second quake.
        if (owner.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))
        {
            ClearProjectile(link);
            return;
        }

        short velocity = vertical ? owner.YVelocity : owner.XVelocity;
        int wholeMagnitude = (Math.Abs((int)velocity) & 0xff00) >> 8;
        ushort ownerPosition = vertical ? owner.YPosition : owner.XPosition;
        ushort linkPosition = ownerPosition;
        if (wholeMagnitude >= 11)
        {
            int offset = wholeMagnitude - 10;
            linkPosition = unchecked((ushort)(ownerPosition + (velocity < 0 ? offset : -offset)));
        }

        if (vertical)
            link.YPosition = linkPosition;
        else
            link.XPosition = linkPosition;

        // At <11 px/frame the link only follows the main center. At higher speeds it samples
        // exactly ten pixels beyond the previous position to close the point-collision gap.
        if (wholeMagnitude >= 11 && MissilePointReaction(
                bus,
                level,
                link,
                horizontalMovement: !vertical,
                roomPlms: roomPlms))
            KillMissile(bus, link, sharedProjectiles);
    }

    private void ClearAllSuperMissileLinks()
    {
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot candidate = _slots[slotIndex];
            if (candidate.PackedType.HasPlainFamilyPayload(SamusProjectileFamily.SuperMissile))
                ClearProjectile(candidate);
        }
    }

    private static bool MoveMissileHorizontally(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        (slot.XPosition, slot.XSubposition) = AddVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity);

        // `$94:A46F` tests the projectile center, not its radius-spanning leading edge. Room
        // width is stored in 256-pixel screens; coordinates in/past the first out-of-room
        // high byte skip reaction and are left for the later 64-pixel viewport deletion.
        int roomWidthInScreens = (level.WidthInBlocks + 15) >> 4;
        if ((slot.XPosition >> 8) >= roomWidthInScreens)
            return false;
        return MissilePointReaction(
            bus, level, slot, horizontalMovement: true, roomPlms: roomPlms);
    }

    private static bool MoveMissileVertically(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        (slot.YPosition, slot.YSubposition) = AddVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity);

        int roomHeightInScreens = (level.HeightInBlocks + 15) >> 4;
        if ((slot.YPosition >> 8) >= roomHeightInScreens)
            return false;
        return MissilePointReaction(
            bus, level, slot, horizontalMovement: false, roomPlms: roomPlms);
    }

    private static bool MissilePointReaction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        bool horizontalMovement,
        RoomPlmSystem? roomPlms)
    {
        int blockX = slot.XPosition >> 4;
        int blockY = slot.YPosition >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);

        // `$94:9D9F/$9DCA` return the dispatcher to its caller with N set, causing the same
        // point reaction to run again on the signed horizontal/vertical parent. A zero-BTS
        // extension is transparent. Reuse the already translated level resolver so missiles
        // and the broad Samus body scans cannot disagree about chained extensions.
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return false;
        if (block.CollisionType == 12 && block.Behavior == 0x45)
        {
            _ = roomPlms?.TryNotifyCollectibleProjectileHit(block.Index, slot.Type);
            return true;
        }
        if (block.CollisionType is 4 or 12)
        {
            // `$94:9E55/$9E73` run the bank-$84 spawn before returning the collision
            // nibble's normal carry: type four remains pass-through; type C is solid.
            TrySpawnShootableReaction(level, slot, block, roomPlms);
            return block.CollisionType == 12;
        }

        if (block.CollisionType is 7 or 15)
        {
            // `$94:9FD6/$9FF4` always indexes the bomb-block PLM table for nonnegative BTS.
            // A missile or Super Missile still performs Spawn_PLM, but setup `$84:CEDA`
            // immediately clears the new slot because its family is neither `$0500` nor
            // `$0300`; no level word, timer, sound, or persistent PLM survives that call.
            // The reaction's carry is independent of setup: bombable air passes through,
            // while bombable solid destroys the missile.
            return block.CollisionType == 15;
        }
        return block.CollisionType switch
        {
            // The point reaction dispatch treats these categories as transparent air.
            0 or 2 or 3 or 6 => false,

            // These categories return carry immediately and therefore kill the missile.
            8 or 9 or 10 or 11 or 14 => true,

            // `$94:A147/$A15E` divide slope BTS values into the five square definitions and
            // the remaining 27 pixel-height definitions. Both paths use the missile center;
            // the direction only changes which half of a square definition is sampled.
            1 => MissileSlopePointReaction(bus, block, slot, horizontalMovement),

            // CollisionType is the high nibble of a room word, so the cases above are
            // exhaustive after extension redispatch. Preserve an explicit corruption guard
            // in case that representation ever changes without silently inventing carry.
            _ => throw new InvalidDataException(
                $"Invalid missile collision type ${block.CollisionType:X2} at block {block.Index}."),
        };
    }

    private static bool MissileSlopePointReaction(
        ISnesAddressSpace bus,
        RoomCollisionBlock block,
        SamusProjectileSlot slot,
        bool horizontalMovement)
    {
        int slopeShape = block.Behavior & 0x1f;
        if (slopeShape >= 5)
        {
            // `$94:A58F` mirrors the projectile's within-block coordinate before indexing
            // the cartridge table. BTS bit 6 flips X and bit 7 flips Y. A table height at or
            // above the mirrored Y point (the original uses signed `height - y <= 0`) is
            // solid. Values can reach 20 for overhanging shapes, so do not mask to a nibble.
            int xInBlock = slot.XPosition & 0x000f;
            if ((block.Behavior & 0x40) != 0)
                xInBlock ^= 0x000f;

            int yInBlock = slot.YPosition & 0x000f;
            if ((block.Behavior & 0x80) != 0)
                yInBlock ^= 0x000f;

            int height = bus.ReadByte(
                NonSquareSlopeDefinitions + slopeShape * 16 + xInBlock) & 0x1f;
            return height <= yInBlock;
        }

        // `$94:A66A/$A71A` treat each square slope as four 8x8 quadrants. The top two BTS
        // bits select the base flip, while the projectile half along the movement axis and
        // then the perpendicular axis select the exact quadrant. Each ROM byte is either
        // `$00` (air) or `$80` (solid).
        int quadrant = slopeShape * 4 + (block.Behavior >> 6);
        if (horizontalMovement)
        {
            quadrant ^= (slot.XPosition & 8) >> 3;
            if ((slot.YPosition & 8) != 0)
                quadrant ^= 2;
        }
        else
        {
            quadrant ^= (slot.YPosition & 8) >> 2;
            if ((slot.XPosition & 8) != 0)
                quadrant ^= 1;
        }

        return bus.ReadByte(SquareSlopeDefinitions + quadrant) != 0;
    }

}
