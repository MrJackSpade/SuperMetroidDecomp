using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Per-frame projectile movement: the beam, wave, Hyper Beam, missile and Super Missile pre-instructions, Super Missile links, and the movement-window deletion check.</summary>
[RoomLifetime]
internal sealed class ProjectileMotion
{
    /// <summary>Clears shots that expire or leave the native movement window.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Allocates and removes the invisible Super Missile link slots.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Converts beam and missile block impacts into their family-specific outcomes.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>Emits beam and missile trails before their parent shots move.</summary>
    private readonly ProjectileTrails _trails;

    /// <summary>Connects projectile motion to animation lifetime, terrain reactions, link slots, and trails.</summary>
    internal ProjectileMotion(ProjectileAnimation animation, ProjectileSlotTable slotTable, ProjectileTerrainCollision terrainCollision, ProjectileTrails trails)
    {
        _animation = animation;
        _slotTable = slotTable;
        _terrainCollision = terrainCollision;
        _trails = trails;
    }
    /// <summary>Accelerates a non-Wave beam, emits its old-position trail, and resolves directional block collision.</summary>
    internal bool RunNoWaveBeamPreInstruction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        RoomPlmSystem? roomPlms,
        SamusPowerBombExplosionState powerBomb)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            _animation.ClearProjectile(slot);
            return false;
        }

        // `$90:AF00` allocates a persistent trail before applying acceleration or movement.
        // Thus the detached 8x8 OBJ records the beam's old world position and visibly falls
        // behind it. Failure to preserve this order makes the trail sit inside the projectile.
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (slot.TrailTimer.IsZero)
        {
            slot.TrailTimer = NativeCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }

        int directionOffset = slot.PackedDirection.DirectionIndex * 2;
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.XAccelerations + directionOffset))))));
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.YAccelerations + directionOffset))))));

        SamusProjectileDirection direction = slot.PackedDirection.Direction;
        bool collided = direction switch
        {
            SamusProjectileDirection.UpFacingRight or
            SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or
            SamusProjectileDirection.UpFacingLeft => ProjectileTerrainCollision.MoveVertically(level, slot, roomPlms),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left =>
                ProjectileTerrainCollision.MoveHorizontally(level, slot, roomPlms),
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.UpLeft =>
                ProjectileTerrainCollision.MoveHorizontally(level, slot, roomPlms) ||
                ProjectileTerrainCollision.MoveVertically(level, slot, roomPlms),
            _ => throw new InvalidDataException(
                $"Beam slot contains invalid direction ${slot.PackedDirection.DirectionIndex:X2}."),
        };

        if (collided)
            _terrainCollision.KillBeamOnBlockCollision(slot, powerBomb);

        // $90:AF00 checks the movement window after the direction handler even
        // when collision changed the beam into an explosion. KillBeam moves its
        // origin to the contact edge, which can put an otherwise retained shot
        // outside that window on this very frame.
        DeleteIfOutsideMovementWindow(slot, layer1);

        return collided;
    }

    /// <summary>Updates Wave's three- or four-frame trail cadence before its block-transparent movement.</summary>
    internal void RunWaveBeamPreInstruction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            _animation.ClearProjectile(slot);
            return;
        }

        // `$90:B0C3` and `$90:B0E4` differ only in the value reloaded after a trail timer
        // expires. The producer always starts at four; an uncharged low-family wave becomes
        // three only after its first emission. Spawning precedes acceleration and movement,
        // so the trail samples the old world position just like the no-wave family.
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (slot.TrailTimer.IsZero)
        {
            slot.TrailTimer = NativeCountdown.FromRaw(slot.PreInstruction ==
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                    ? (ushort)3
                    : (ushort)4);
            _trails.SpawnTrail(slot);
        }

        RunWaveBeamShared(level, slot, layer1, roomPlms);
    }

    /// <summary>Uses Wave's block-transparent movement for Hyper Beam without spawning projectile trails.</summary>
    internal void RunHyperBeamPreInstruction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            _animation.ClearProjectile(slot);
            return;
        }

        // `$90:B159` falls directly into the shared Wave movement. Hyper deliberately has
        // no projectile-trail timer or SpawnProjectileTrail call; its long beam spritemap
        // and the separately counting muzzle flare provide the complete native presentation.
        RunWaveBeamShared(level, slot, layer1, roomPlms);
    }

    /// <summary>Advances Wave or Hyper positions and notifies shootable blocks while ignoring terrain collision carry.</summary>
    private void RunWaveBeamShared(
        RoomLevelData level,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        RoomPlmSystem? roomPlms)
    {
        int direction = slot.PackedDirection.DirectionIndex;
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.XAccelerations + direction * 2))))));
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.YAccelerations + direction * 2))))));

        // `$94:A352/$A3E4` advance the same 16.16 positions and scan every block touched by
        // the projectile radii, but deliberately return carry clear unconditionally. That is
        // the defining Wave Beam behavior: block reactions may run, yet terrain never kills
        // or clips the shot. The span scanners below now publish ordinary shootable-block
        // PLMs while intentionally discarding their carry result, matching this fallthrough.
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            slot.Position = slot.Position.AdvanceX(ProjectileTerrainCollision.EightEightDelta(slot.Velocity.X.SignedRawValue));
            ProjectileTerrainCollision.ScanHorizontalWaveShotReactions(level, slot, roomPlms);
        }
        if (direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            slot.Position = slot.Position.AdvanceY(ProjectileTerrainCollision.EightEightDelta(slot.Velocity.Y.SignedRawValue));
            ProjectileTerrainCollision.ScanVerticalWaveShotReactions(level, slot, roomPlms);
        }

        DeleteIfOutsideMovementWindow(slot, layer1);
    }

    /// <summary>Handles ordinary missile ignition, acceleration, trail, point collision, and post-impact deletion.</summary>
    internal bool RunMissilePreInstruction(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            _animation.ClearProjectile(slot);
            return false;
        }

        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (slot.TrailTimer.IsZero)
        {
            slot.TrailTimer = NativeCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }

        int direction = slot.PackedDirection.DirectionIndex;

        // Missile pre-instruction `$90:AF8F-$AFA0` first applies the same small directional
        // acceleration as beams. On the ignition frame Missile_Func1 below replaces velocity,
        // so this addition is intentionally overwritten; subsequent frames retain it.
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.XAccelerations + direction * 2))))));
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue +
            unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.YAccelerations + direction * 2))))));

        if ((SnesLittleEndian.HighByteInPlace(slot.Variable)) == 0)
        {
            // `$90:C301` is the literal `$0100` ignition increment. Crossing into a nonzero
            // high byte re-runs `$90:B1F3` with that word as base 8.8 speed. A normal missile
            // crosses on its first alpha pass: one pixel/frame plus native inheritance.
            slot.Variable = unchecked((ushort)(slot.Variable + 256));
            if ((SnesLittleEndian.HighByteInPlace(slot.Variable)) != 0)
                BeamFiring.InitializeDirectionalVelocity(samus.ProjectileInheritance, slot, unchecked((short)slot.Variable));
        }
        else
        {
            int acceleration = SamusProjectileRomData.NonBeam.MissileAccelerations + direction * 4;
            slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue +
                unchecked((short)SamusProjectileMotionDefinitions.ReadWord(acceleration))))));
            slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue +
                unchecked((short)SamusProjectileMotionDefinitions.ReadWord(acceleration + 2))))));
        }

        bool collided = direction switch
        {
            0 or 4 or 5 or 9 => MoveMissileVertically(level, slot, roomPlms),
            2 or 7 => MoveMissileHorizontally(level, slot, roomPlms),
            1 or 3 or 6 or 8 =>
                MoveMissileHorizontally(level, slot, roomPlms) ||
                MoveMissileVertically(level, slot, roomPlms),
            _ => throw new InvalidDataException(
                $"Missile slot contains invalid direction ${direction:X2}."),
        };
        if (collided)
            _terrainCollision.KillMissile(slot, sharedProjectiles);

        // The native missile direction handler may create an explosion just
        // beyond the retention boundary. Its caller still runs the offscreen
        // check on that same frame, after impact sound/cooldown side effects.
        DeleteIfOutsideMovementWindow(slot, layer1);
        return collided;
    }

    /// <summary>Ignites a Super Missile with its invisible link and updates both point reactions after each movement axis.</summary>
    internal bool RunSuperMissilePreInstruction(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if (slot.PackedDirection.HasLowByteLifecycleState)
        {
            _animation.ClearProjectile(slot);
            ClearAllSuperMissileLinks();
            return false;
        }

        // `$90:AFF3` begins with the producer's value four but reloads two after every
        // expiry. Super Missiles therefore emit twice as frequently as ordinary missiles;
        // both families still select the same `$B5A1` art through pointer entries $20/$21.
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (slot.TrailTimer.IsZero)
        {
            slot.TrailTimer = NativeCountdown.Start(2);
            _trails.SpawnTrail(slot);
        }

        int direction = slot.PackedDirection.DirectionIndex;
        if ((SnesLittleEndian.HighByteInPlace(slot.Variable)) == 0)
        {
            // The shared missile accelerator crosses `$0100` on its first alpha pass. Only
            // the Super family immediately allocates `$90:BF46`'s invisible collision link;
            // the link's native byte index is retained in the variable's low byte.
            slot.Variable = unchecked((ushort)(slot.Variable + 256));
            if ((SnesLittleEndian.HighByteInPlace(slot.Variable)) != 0)
            {
                BeamFiring.InitializeDirectionalVelocity(samus.ProjectileInheritance, slot, unchecked((short)slot.Variable));
                SpawnSuperMissileLink(samus, slot);
            }
        }
        else
        {
            int acceleration =
                SamusProjectileRomData.NonBeam.SuperMissileAccelerations + direction * 4;
            slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue +
                unchecked((short)SamusProjectileMotionDefinitions.ReadWord(acceleration))))));
            slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue +
                unchecked((short)SamusProjectileMotionDefinitions.ReadWord(acceleration + 2))))));
        }

        bool collided = false;
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileHorizontally(level, slot, roomPlms);
            if (collided)
                _terrainCollision.KillMissile(slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                level, slot, vertical: false, sharedProjectiles, roomPlms);
        }
        if (!collided && direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileVertically(level, slot, roomPlms);
            if (collided)
                _terrainCollision.KillMissile(slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                level, slot, vertical: true, sharedProjectiles, roomPlms);
        }

        if (DeleteIfOutsideMovementWindow(slot, layer1))
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
        Position16<RoomPixels> layer1)
    {
        const int MinimumVisibleCoordinate = -64;
        const int MaximumVisibleCoordinateExclusive = 320;

        short screenX = unchecked((short)(slot.Position.X.Whole - layer1.X));
        short screenY = unchecked((short)(slot.Position.Y.Whole - layer1.Y));
        bool outside =
            screenX < MinimumVisibleCoordinate ||
            screenX >= MaximumVisibleCoordinateExclusive ||
            screenY < MinimumVisibleCoordinate ||
            screenY >= MaximumVisibleCoordinateExclusive;
        if (!outside)
            return false;

        _animation.ClearProjectile(slot);
        return true;
    }

    /// <summary>Allocates the invisible collision link at ignition and records its native byte index in the owner.</summary>
    internal void SpawnSuperMissileLink(
        SamusState samus,
        SamusProjectileSlot owner)
    {
        SamusProjectileSlot? link = _slotTable.Slots.FirstOrDefault(candidate => candidate.Damage == 0);
        if (link is null)
            return;

        link.ClearFields();
        link.Type = SamusProjectileTypeWord.LiveMarker | (ushort)SamusProjectileFamily.SuperMissile;
        link.Direction = owner.Direction;
        link.Position = link.Position.WithWhole(owner.Position.Whole);

        // `$90:BF78` deliberately calls the ordinary muzzle initializer again after copying
        // the owner's coordinates. At ignition both positions are equivalent; retaining the
        // call matters for moving/transition poses whose cartridge origin tables can change.
        BeamFiring.InitializePosition(samus, link);
        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.NonBeam.SuperMissileLinkDataPointers + 4);
        link.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        link.Cursor = SamusProjectilePrograms.Start(SamusProjectileHeaderDefinitions.Program(dataPointer, 0));
        link.InstructionTimer = NativeCountdown.Start(1);
        link.PreInstruction = SamusProjectilePreInstruction.SuperMissileLink;

        owner.Variable = unchecked((ushort)((SnesLittleEndian.HighByteInPlace(owner.Variable)) + link.NativeByteIndex));
        _slotTable.ProjectileCounter = unchecked((ushort)(_slotTable.ProjectileCounter + 1));
    }

    /// <summary>Keeps an ordinary link stationary or clears the whole Super Missile family on its deletion signal.</summary>
    internal void RunSuperMissileLinkPreInstruction(SamusProjectileSlot link)
    {
        // `$90:B075` leaves an ordinary link completely stationary. A high direction nibble
        // is a deletion signal and clears every `$x200` family slot, including its owner.
        if (!link.PackedDirection.HasLowByteLifecycleState)
            return;
        _animation.ClearProjectile(link);
        ClearAllSuperMissileLinks();
    }

    /// <summary>Places a fast link behind the owner for its extra point reaction, retaining impact checks on explosion frames.</summary>
    private void UpdateSuperMissileLinkAxis(
        RoomLevelData level,
        SamusProjectileSlot owner,
        bool vertical,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        if ((SnesLittleEndian.HighByteInPlace(owner.Variable)) == 0)
            return;

        int linkIndex = (SnesLittleEndian.LowByte(owner.Variable)) >> 1;
        if ((uint)linkIndex >= (uint)_slotTable.Slots.Count)
            return;
        SamusProjectileSlot link = _slotTable.Slots[linkIndex];
        if (!link.IsActive)
            return;

        short velocity = vertical ? owner.Velocity.Y.SignedRawValue : owner.Velocity.X.SignedRawValue;
        int wholeMagnitude = SnesEightEight.WholeOf(Math.Abs((int)velocity));
        bool ownerExploded = owner.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion);
        if (wholeMagnitude < 11)
        {
            // `$90:B494`: a slow link clears at once if the owner has already exploded,
            // otherwise it only follows the owner's center.
            if (ownerExploded)
            {
                _animation.ClearProjectile(link);
                return;
            }
            if (vertical)
                link.Position = link.Position.WithWholeY(owner.Position.Y.Whole);
            else
                link.Position = link.Position.WithWholeX(owner.Position.X.Whole);
            return;
        }

        // `$90:B438-$B467`: a fast link samples ten pixels beyond the owner's previous
        // position to close the point-collision gap, and does so even when the owner has
        // just exploded; only then is it cleared. A missile that skips a door cap and
        // strikes the door behind it still opens the cap through its link.
        int offset = wholeMagnitude - 10;
        ushort ownerPosition = vertical ? owner.Position.Y.Whole : owner.Position.X.Whole;
        ushort linkPosition = unchecked((ushort)(ownerPosition + (velocity < 0 ? offset : -offset)));
        if (vertical)
            link.Position = link.Position.WithWholeY(linkPosition);
        else
            link.Position = link.Position.WithWholeX(linkPosition);
        if (MissilePointReaction(
                level,
                link,
                horizontalMovement: !vertical,
                roomPlms: roomPlms))
            _terrainCollision.KillMissile(link, sharedProjectiles);
        if (ownerExploded)
            _animation.ClearProjectile(link);
    }

    /// <summary>Clears every active plain Super Missile family slot after owner or link termination.</summary>
    private void ClearAllSuperMissileLinks()
    {
        for (int slotIndex = SamusProjectileSystem.SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot candidate = _slotTable.Slots[slotIndex];
            if (candidate.PackedType.HasPlainFamilyPayload(SamusProjectileFamily.SuperMissile))
                _animation.ClearProjectile(candidate);
        }
    }

    /// <summary>Advances missile X and tests its center unless it has crossed the room's screen-width boundary.</summary>
    private static bool MoveMissileHorizontally(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        slot.Position = slot.Position.AdvanceX(ProjectileTerrainCollision.EightEightDelta(slot.Velocity.X.SignedRawValue));

        // `$94:A46F` tests the projectile center, not its radius-spanning leading edge. Room
        // width is stored in 256-pixel screens; coordinates in/past the first out-of-room
        // high byte skip reaction and are left for the later 64-pixel viewport deletion.
        int roomWidthInScreens = (level.Size.X + 15) >> 4;
        if ((SnesEightEight.WholeOf(slot.Position.X.Whole)) >= roomWidthInScreens)
            return false;
        return MissilePointReaction(
            level, slot, horizontalMovement: true, roomPlms: roomPlms);
    }

    /// <summary>Advances missile Y and tests its center unless it has crossed the room's screen-height boundary.</summary>
    private static bool MoveMissileVertically(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        slot.Position = slot.Position.AdvanceY(ProjectileTerrainCollision.EightEightDelta(slot.Velocity.Y.SignedRawValue));

        int roomHeightInScreens = (level.Size.Y + 15) >> 4;
        if ((SnesEightEight.WholeOf(slot.Position.Y.Whole)) >= roomHeightInScreens)
            return false;
        return MissilePointReaction(
            level, slot, horizontalMovement: false, roomPlms: roomPlms);
    }

    /// <summary>Resolves the missile center's extension chain, PLM reaction, and collision carry by block type.</summary>
    private static bool MissilePointReaction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        bool horizontalMovement,
        RoomPlmSystem? roomPlms)
    {
        int blockX = slot.Position.X.Whole >> 4;
        int blockY = slot.Position.Y.Whole >> 4;
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));

        // `$94:9D9F/$9DCA` return the dispatcher to its caller with N set, causing the same
        // point reaction to run again on the signed horizontal/vertical parent. A zero-BTS
        // extension is transparent. Reuse the already translated level resolver so missiles
        // and the broad Samus body scans cannot disagree about chained extensions.
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return false;
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Bts == RoomBlockBehaviorValues.CollectibleTrigger)
        {
            _ = roomPlms?.CollectiblePlms.TryNotifyCollectibleProjectileHit(block.Index);
            return true;
        }
        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            // `$94:9E55/$9E73` run the bank-$84 spawn before returning the collision
            // nibble's normal carry: type four remains pass-through; type C is solid.
            ProjectileTerrainCollision.TrySpawnShootableReaction(level, slot, block, roomPlms);
            return block.CollisionType == RoomCollisionType.ShootableBlock;
        }

        if (block.CollisionType is RoomCollisionType.BombableAir or RoomCollisionType.BombableBlock)
        {
            // `$94:9FD6/$9FF4` always indexes the bomb-block PLM table for nonnegative BTS.
            // A missile or Super Missile still performs Spawn_PLM, but setup `$84:CEDA`
            // immediately clears the new slot because its family is neither `$0500` nor
            // `$0300`; no level word, timer, sound, or persistent PLM survives that call.
            // The reaction's carry is independent of setup: bombable air passes through,
            // while bombable solid destroys the missile.
            return block.CollisionType == RoomCollisionType.BombableBlock;
        }
        return block.CollisionType switch
        {
            // The point reaction dispatch treats these categories as transparent air.
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.UnusedAir => false,

            // These categories return carry immediately and therefore kill the missile.
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.GrappleBlock => true,

            // `$94:A147/$A15E` divide slope BTS values into the five square definitions and
            // the remaining 27 pixel-height definitions. Both paths use the missile center;
            // the direction only changes which half of a square definition is sampled.
            RoomCollisionType.Slope =>
                MissileSlopePointReaction(block, slot, horizontalMovement),

            // CollisionType is the high nibble of a room word, so the cases above are
            // exhaustive after extension redispatch. Preserve an explicit corruption guard
            // in case that representation ever changes without silently inventing carry.
            RoomCollisionType.ShootableAir or
                RoomCollisionType.HorizontalExtension or
                RoomCollisionType.BombableAir or
                RoomCollisionType.ShootableBlock or
                RoomCollisionType.VerticalExtension or
                RoomCollisionType.BombableBlock => throw new InvalidDataException(
                $"Invalid missile collision type ${(int)block.CollisionType:X2} at block {block.Index}."),
            _ => throw new InvalidDataException(
                $"Invalid missile collision type ${(int)block.CollisionType:X2} at block {block.Index}."),
        };
    }

    /// <summary>Tests the missile center against mirrored height tables or the selected square-slope quadrant.</summary>
    internal static bool MissileSlopePointReaction(
        RoomCollisionBlock block,
        SamusProjectileSlot slot,
        bool horizontalMovement)
    {
        int slopeShape = block.Bts.SlopeShape;
        if (slopeShape >= 5)
        {
            // `$94:A58F` mirrors the projectile's within-block coordinate before indexing
            // the cartridge table. BTS bit 6 flips X and bit 7 flips Y. A table height at or
            // above the mirrored Y point (the original uses signed `height - y <= 0`) is
            // solid. Values can reach 20 for overhanging shapes, so do not mask to a nibble.
            int xInBlock = slot.Position.X.Whole & 15;
            if (block.Bts.SlopeFlipsHorizontally)
                xInBlock ^= 15;

            int yInBlock = slot.Position.Y.Whole & 15;
            if (block.Bts.SlopeFlipsVertically)
                yInBlock ^= 15;

            int height = SlopeHeightDefinitions.Read(slopeShape, xInBlock);
            return height <= yInBlock;
        }

        // `$94:A66A/$A71A` treat each square slope as four 8x8 quadrants. The top two BTS
        // bits select the base flip, while the projectile half along the movement axis and
        // then the perpendicular axis select the exact quadrant. Each ROM byte is either
        // `$00` (air) or `$80` (solid).
        int quadrant = slopeShape * 4 + block.Bts.SlopeOrientation;
        if (horizontalMovement)
        {
            quadrant ^= (slot.Position.X.Whole & 8) >> 3;
            if ((slot.Position.Y.Whole & 8) != 0)
                quadrant ^= 2;
        }
        else
        {
            quadrant ^= (slot.Position.Y.Whole & 8) >> 2;
            if ((slot.Position.X.Whole & 8) != 0)
                quadrant ^= 1;
        }

        return SquareSlopeDefinitions.ReadSamusQuadrant(quadrant) != 0;
    }
}
