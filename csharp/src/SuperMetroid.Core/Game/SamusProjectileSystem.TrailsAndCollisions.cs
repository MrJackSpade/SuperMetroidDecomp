using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Trail allocation/animation plus terrain reaction and impact handling.
/// </summary>
public sealed partial class SamusProjectileSystem
{
    /// <summary>
    /// Applies the projectile-owned side effects that `$A0:9CC8-$9CD6` performs before any
    /// enemy callback which returns without entering ordinary impact conversion. This is
    /// shared by extended hitboxes and radius-based custom reactions such as shutters.
    /// </summary>
    public void ApplyEnemyCollisionPrelude(int slotIndex, bool markCollisionState)
    {
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slots[slotIndex];
        if (!slot.IsActive)
            return;

        if (slot.PackedType.Family == SamusProjectileFamily.SuperMissile)
        {
            // The multibox collision walker writes the same global quake as a normal Super
            // Missile impact before dispatching the hitbox callback. This remains observable
            // even when gold Ninja armor ignores a fresh, not-yet-linked Super Missile.
            EarthquakeType = 20;
            EarthquakeTimer = 30;
        }

        if (markCollisionState)
            slot.Direction = unchecked((ushort)(slot.Direction | 0x0010));
    }

    /// <summary>
    /// Compatibility name retained for callers whose geometry is specifically an extended
    /// spritemap. The behavior is the same bank-$A0 projectile prelude, not a separate fix.
    /// </summary>
    public void ApplyExtendedEnemyCollisionPrelude(int slotIndex, bool markCollisionState) =>
        ApplyEnemyCollisionPrelude(slotIndex, markCollisionState);

    /// <summary>
    /// Ports <c>ProjectileReflection</c> at `$90:BE00` after an enemy callback has replaced
    /// the projectile direction. Reflection preserves the projectile family/type and world
    /// position, but reconstructs its bank-$93 damage, direction-specific animation, radii,
    /// and family pre-instruction exactly as the native routine does.
    /// </summary>
    public void ReflectFromEnemy(ISnesAddressSpace bus, int slotIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slots[slotIndex];
        if (!slot.IsActive)
            return;

        SamusProjectileFamily family = slot.PackedType.Family;
        if (family == SamusProjectileFamily.SuperMissile)
        {
            // A live Super Missile stores its invisible collision-link byte index in the
            // variable's low byte. `$90:BE17` clears that exact slot before repurposing the
            // owner; it does not broadly delete unrelated Super Missile records.
            int linkIndex = (slot.Variable & 0x00ff) >> 1;
            if ((uint)linkIndex < SlotCount && linkIndex != slotIndex && _slots[linkIndex].IsActive)
                ClearProjectile(_slots[linkIndex]);
        }

        if (family == SamusProjectileFamily.Beam)
        {
            // Beam reflection alone reinitializes velocity immediately. Missile families
            // retain their old velocity until variable `$00F0` crosses into `$01F0` in the
            // next Missile_Func1 call, at which point the new direction takes effect.
            InitializePowerBeamVelocity(bus, slot);
        }

        int dataPointerTable;
        int dataPointerIndex;
        if (family == SamusProjectileFamily.Beam)
        {
            dataPointerTable = slot.PackedType.IsChargedBeam
                ? ChargedBeamDataPointers
                : UnchargedBeamDataPointers;
            dataPointerIndex = slot.PackedType.BeamCombinationIndex;
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            dataPointerTable = NonBeamProjectileDataPointers;
            dataPointerIndex = slot.PackedType.FamilyValue >> 8;
        }
        else
        {
            throw new NotSupportedException(
                $"Projectile family ${slot.PackedType.FamilyValue:X3} cannot be reflected.");
        }

        ushort dataPointer = ReadWord(bus, dataPointerTable + dataPointerIndex * 2);
        int data = 0x930000 | dataPointer;
        slot.Damage = ReadWord(bus, data);
        slot.InstructionPointer = ReadWord(
            bus,
            AddWithinBank(data, 2 + slot.PackedDirection.DirectionIndex * 2));
        slot.XRadius = bus.ReadByte(
            0x930000 | unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = bus.ReadByte(
            0x930000 | unchecked((ushort)(slot.InstructionPointer + 5)));
        slot.InstructionTimer = 1;

        if (family == SamusProjectileFamily.Missile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.Missile;
            slot.Variable = 240;
        }
        else if (family == SamusProjectileFamily.SuperMissile)
        {
            slot.PreInstruction = SamusProjectilePreInstruction.SuperMissile;
            slot.Variable = 240;
        }
        else
        {
            slot.PreInstruction = (slot.PackedType.BeamCombinationIndex & 1) == 0
                ? SamusProjectilePreInstruction.NoWaveBeam
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;
        }
    }

    /// <summary>
    /// Applies intro Mother Brain's external projectile test at <c>$8B:B78A..B7BA</c>.
    /// </summary>
    /// <remarks>
    /// The cinematic scans native byte indices $08 down through $00 for the first ordinary
    /// missile (<c>type &amp; $0FFF == $0100</c>) and only kills it once its signed X coordinate
    /// is below $54. This is not terrain collision, so the actor owns the predicate while
    /// the projectile system owns the exact bank-$90 impact conversion.
    /// </remarks>
    public bool TryImpactIntroMotherBrainMissile(
        ISnesAddressSpace bus,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = _slots[slotIndex];
            if ((slot.Type & 0x0fff) != 0x0100)
                continue;

            if (unchecked((short)(slot.XPosition - 0x0054)) >= 0)
                return false;

            KillMissile(bus, slot, sharedProjectiles);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts one live beam or missile into its ordinary bank-$93 impact animation after
    /// an enemy-owned overlap test accepts it. Enemy shot AI owns damage/counters; this
    /// projectile owner alone mutates the parallel slot arrays and shared cooldown.
    /// </summary>
    public bool TryStartEnemyImpact(
        ISnesAddressSpace bus,
        SamusBombProjectileSystem sharedProjectiles,
        int slotIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slots[slotIndex];
        if (!slot.IsActive || slot.PackedDirection.HasLowByteLifecycleState)
            return false;

        switch (slot.PackedType.Family)
        {
            case SamusProjectileFamily.Beam:
                KillBeam(bus, slot);
                return true;
            case SamusProjectileFamily.Missile:
            case SamusProjectileFamily.SuperMissile:
                KillMissile(bus, slot, sharedProjectiles);
                return true;
            default:
                // Existing explosion, bomb, and unknown family slots cannot repeatedly
                // increment an enemy hit counter merely because their art still overlaps.
                return false;
        }
    }

    private void SpawnTrail(ISnesAddressSpace bus, SamusProjectileSlot projectile)
    {
        int pointerIndex;
        if (projectile.PackedType.IsFamily(SamusProjectileFamily.Beam))
        {
            // Beam indices retain charge/SBA bits in the low six bits. Charged plain power
            // therefore selects entry $10, not ordinary-power entry zero.
            pointerIndex = projectile.Type & 0x003f;
        }
        else
        {
            // Missiles and supers map families $01/$02 to table entries $20/$21. Other
            // projectile families return carry set and do not consume a trail slot.
            int family = projectile.PackedType.FamilyValue >> 8;
            if (family >= 3)
                return;
            pointerIndex = family + 0x001f;
        }

        SamusProjectileTrailSlot? trail = null;
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            // The original tests only the left timer. A right stream can still be alive in
            // malformed/debug-edited state and will be overwritten when the left sentinel is
            // clear; retain that asymmetric allocation contract rather than being "safer".
            if (_trailSlots[slotIndex].Left.InstructionTimer == 0)
            {
                trail = _trailSlots[slotIndex];
                break;
            }
        }
        if (trail is null)
            return;

        trail.Left.InstructionTimer = 1;
        trail.Right.InstructionTimer = 1;
        trail.Left.InstructionPointer = ReadWord(bus, TrailLeftInstructionPointers + pointerIndex * 2);
        trail.Right.InstructionPointer = ReadWord(bus, TrailRightInstructionPointers + pointerIndex * 2);

        // `$93:81D1` returns the animation field that is current at this exact pre-instruction
        // instant. When the timer is one and the upcoming word is a normal record, that means
        // the upcoming field; otherwise it means the record eight bytes behind the pointer.
        ushort animationFrame = GetTrailAnimationFrame(bus, projectile);
        int direction = projectile.PackedDirection.DirectionIndex;
        int familyTable = (projectile.Type & 0x0020) != 0
            ? SpazerSbaTrailOffsetFamilies
            : projectile.PackedType.IsChargedBeam
                ? ChargedTrailOffsetFamilies
                : UnchargedTrailOffsetFamilies;
        ushort directionTable = ReadWord(
            bus,
            familyTable + projectile.PackedType.BeamCombinationIndex * 2);
        ushort offsetList = ReadWord(
            bus,
            0x9b0000 | unchecked((ushort)(directionTable + direction * 2)));
        int offsets = 0x9b0000 | unchecked((ushort)(offsetList + animationFrame * 4));

        // All four bytes are signed offsets. The final minus four converts a beam-centered
        // point to the upper-left origin of the raw 8x8 trail OBJ, exactly as `$9B:A3CC`.
        trail.Left.XPosition = AddSignedOffset(projectile.XPosition, bus.ReadByte(offsets), -4);
        trail.Left.YPosition = AddSignedOffset(projectile.YPosition, bus.ReadByte(offsets + 1), -4);
        trail.Right.XPosition = AddSignedOffset(projectile.XPosition, bus.ReadByte(offsets + 2), -4);
        trail.Right.YPosition = AddSignedOffset(projectile.YPosition, bus.ReadByte(offsets + 3), -4);
    }

    private static ushort GetTrailAnimationFrame(
        ISnesAddressSpace bus,
        SamusProjectileSlot projectile)
    {
        ushort pointer = projectile.InstructionPointer;
        ushort upcomingWord = ReadWord(bus, 0x930000 | pointer);
        int recordDelta = projectile.InstructionTimer == 1 && (upcomingWord & 0x8000) == 0
            ? 0
            : -8;
        ushort frameAddress = unchecked((ushort)(pointer + recordDelta + 6));
        return ReadWord(bus, 0x930000 | frameAddress);
    }

    private static ushort AddSignedOffset(ushort origin, byte encodedOffset, int constant) =>
        unchecked((ushort)(origin + unchecked((sbyte)encodedOffset) + constant));

    private static void HandleTrailSideAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileTrailSide side,
        ushort layer1X,
        ushort layer1Y,
        bool timeIsFrozen,
        bool isLeft)
    {
        if (side.InstructionTimer == 0)
            return;

        if (!timeIsFrozen)
        {
            side.InstructionTimer = unchecked((ushort)(side.InstructionTimer - 1));
            if (side.InstructionTimer == 0)
            {
                ushort pointer = side.InstructionPointer;
                while (true)
                {
                    ushort instructionOrTimer = ReadWord(bus, 0x900000 | pointer);
                    if ((instructionOrTimer & 0x8000) == 0)
                    {
                        side.InstructionTimer = instructionOrTimer;
                        if (instructionOrTimer == 0)
                            return;

                        side.TileNumberAttributes = ReadWord(
                            bus,
                            0x900000 | unchecked((ushort)(pointer + 2)));
                        side.InstructionPointer = unchecked((ushort)(pointer + 4));
                        break;
                    }

                    // Bank $90 stores executable instruction addresses inline. Each handler
                    // returns to the parser with X already advanced past its one-word opcode.
                    pointer = unchecked((ushort)(pointer + 2));
                    switch (instructionOrTimer)
                    {
                        case MoveLeftTrailDown when isLeft:
                        case MoveRightTrailDown when !isLeft:
                            side.YPosition = unchecked((ushort)(side.YPosition + 1));
                            break;
                        case MoveLeftTrailUp when isLeft:
                            side.YPosition = unchecked((ushort)(side.YPosition - 1));
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unsupported {(isLeft ? "left" : "right")} projectile-trail " +
                                $"instruction ${instructionOrTimer:X4} at $90:{unchecked((ushort)(pointer - 2)):X4}.");
                    }
                }
            }
        }

        // `$90:B6F4/$B703` require both complete 16-bit camera-relative coordinates to have
        // a zero high byte. Unlike the generic spritemap path, negative or 256+ coordinates
        // are skipped rather than wrapped or parked.
        ushort screenX = unchecked((ushort)(side.XPosition - layer1X));
        ushort screenY = unchecked((ushort)(side.YPosition - layer1Y));
        if ((screenX & 0xff00) != 0 || (screenY & 0xff00) != 0)
            return;

        oam.AddProjectileTrailSprite(
            unchecked((byte)screenX),
            unchecked((byte)screenY),
            side.TileNumberAttributes);
    }

    private static bool MoveHorizontally(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        (slot.XPosition, slot.XSubposition) = AddVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity);

        return ScanHorizontalShotReactions(level, slot, roomPlms);
    }

    private static bool MoveVertically(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        (slot.YPosition, slot.YSubposition) = AddVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity);

        return ScanVerticalShotReactions(level, slot, roomPlms);
    }

    private static bool ScanHorizontalShotReactions(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        int topBlock = unchecked((ushort)(slot.YPosition - slot.YRadius)) >> 4;
        int bottomBlock = unchecked((ushort)(slot.YPosition + slot.YRadius - 1)) >> 4;
        int targetX = slot.XVelocity < 0
            ? unchecked((ushort)(slot.XPosition - slot.XRadius))
            : unchecked((ushort)(slot.XPosition + slot.XRadius - 1));
        int blockX = targetX >> 4;

        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)topBlock >= (uint)level.HeightInBlocks ||
            (uint)bottomBlock >= (uint)level.HeightInBlocks)
        {
            return false;
        }

        // `$94:A1B5` decrements its target counter only for carry-set reactions. A tall
        // projectile grazing air beside one solid block therefore keeps moving; every
        // touched block still gets its side effect before that aggregate result is tested.
        bool everyBlockSolid = true;
        for (int blockY = topBlock; blockY <= bottomBlock; blockY++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
            if (!RunShotReaction(level, slot, block, roomPlms))
                everyBlockSolid = false;
        }
        return everyBlockSolid;
    }

    private static bool ScanVerticalShotReactions(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        int leftBlock = unchecked((ushort)(slot.XPosition - slot.XRadius)) >> 4;
        int rightBlock = unchecked((ushort)(slot.XPosition + slot.XRadius - 1)) >> 4;
        int targetY = slot.YVelocity < 0
            ? unchecked((ushort)(slot.YPosition - slot.YRadius))
            : unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int blockY = targetY >> 4;

        if ((uint)blockY >= (uint)level.HeightInBlocks ||
            (uint)leftBlock >= (uint)level.WidthInBlocks ||
            (uint)rightBlock >= (uint)level.WidthInBlocks)
        {
            return false;
        }

        bool everyBlockSolid = true;
        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
            if (!RunShotReaction(level, slot, block, roomPlms))
                everyBlockSolid = false;
        }
        return everyBlockSolid;
    }

    private static bool RunShotReaction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomCollisionBlock block,
        RoomPlmSystem? roomPlms)
    {
        // Special-block collision first searches the loaded PLM pool for the object whose
        // origin matches this block. Mother Brain's type-$8/BTS-$44 glass consumes the live
        // projectile type through its pre-instruction on the later PLM-handler seam; it does
        // not allocate a second reaction PLM or mutate the shot-block tables below.
        if (block.CollisionType == 8 && block.Behavior == 0x44)
            _ = roomPlms?.TryNotifyProjectileHit(block.Index, slot.Type);

        if (block.CollisionType is 4 or 12)
        {
            TrySpawnShootableReaction(level, slot, block, roomPlms);
            return block.CollisionType == 12;
        }

        // `$94:A175/$A195` return carry for 8/B/E directly. Door ($9), spike ($A), and
        // bombable ($F) also collide; their actor/weapon-gated setup remains a later slice.
        return block.CollisionType is >= 8 and <= 15;
    }

    private static void TrySpawnShootableReaction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomCollisionBlock block,
        RoomPlmSystem? roomPlms)
    {
        if (roomPlms is null)
            return;

        // Nonnegative BTS 0..F indexes the complete retail `$94:9EA6` table. Negative BTS
        // 0..7 is also admitted so shootable-solid can preserve its native area-table no-op
        // slot; shootable-air's method exits without allocating. The RoomPlm owner performs
        // the power-bomb/Super-Missile family checks for entries 8..B synchronously, just as
        // each bank-$84 setup sees the current native projectile type during Spawn_PLM.
        bool translatedBehavior = (block.Behavior & 0x80) != 0
            ? (block.Behavior & 0x7f) <= 7
            : block.Behavior <= 15;
        if (translatedBehavior)
        {
            roomPlms.TrySpawnProjectileShotBlock(
                level,
                block.Index,
                block.Behavior,
                slot.Type,
                solidBlock: block.CollisionType == 12);
        }
    }

    private static (ushort Position, ushort Subposition) AddVelocity(
        ushort position,
        ushort subposition,
        short velocity)
    {
        // Bank $94 overlaps DP $12/$13 so an 8.8 velocity becomes a signed 16.16 delta by
        // shifting it left eight. Performing the addition as one wrapped 32-bit quantity is
        // exactly equivalent to its low-word ADC followed by sign-word ADC.
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = unchecked((uint)(fixedPosition + ((int)velocity << 8)));
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private static void KillBeam(ISnesAddressSpace bus, SamusProjectileSlot slot)
    {
        // `$90:AE3A` moves the explosion anchor to the leading edge before bank $93 swaps
        // the instruction list. Diagonals adjust both axes in their respective signs.
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        if (direction is 1 or 2 or 3)
            slot.XPosition = unchecked((ushort)(slot.XPosition + slot.XRadius));
        else if (direction is 6 or 7 or 8)
            slot.XPosition = unchecked((ushort)(slot.XPosition - slot.XRadius));

        if (direction is 0 or 1 or 8 or 9)
            slot.YPosition = unchecked((ushort)(slot.YPosition - slot.YRadius));
        else if (direction is 3 or 4 or 5 or 6)
            slot.YPosition = unchecked((ushort)(slot.YPosition + slot.YRadius));

        slot.Type = slot.PackedType.WithFamily(SamusProjectileFamily.BeamExplosion);
        slot.InstructionPointer = ReadWord(bus, BeamExplosionInstructionPointerAddress);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;
    }

    private void KillMissile(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        // The shared `$90:AE3A` leading-edge correction runs for beams and missiles alike.
        // Missiles use point collision while travelling, but their explosion is deliberately
        // anchored one current animation radius farther in the fired direction.
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        if (direction is 1 or 2 or 3)
            slot.XPosition = unchecked((ushort)(slot.XPosition + slot.XRadius));
        else if (direction is 6 or 7 or 8)
            slot.XPosition = unchecked((ushort)(slot.XPosition - slot.XRadius));

        if (direction is 0 or 1 or 8 or 9)
            slot.YPosition = unchecked((ushort)(slot.YPosition - slot.YRadius));
        else if (direction is 3 or 4 or 5 or 6)
            slot.YPosition = unchecked((ushort)(slot.YPosition + slot.YRadius));

        // `$93:80CF` queues library-two sound seven, converts non-beams to family `$0800`,
        // selects `$86:7F`, and leaves the slot counted until its delete opcode. Sound-library
        // two has no public frame-result channel yet; every stateful effect is retained here.
        bool wasSuperMissile = slot.PackedType.IsFamily(SamusProjectileFamily.SuperMissile);
        slot.Type = slot.PackedType.WithFamily(SamusProjectileFamily.MissileExplosion);
        slot.InstructionPointer = ReadWord(
            bus,
            wasSuperMissile
                ? SuperMissileExplosionInstructionPointerAddress
                : MissileExplosionInstructionPointerAddress);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;

        if (wasSuperMissile)
        {
            // `$93:8125-$812E` is presentation state, but it is authored by the projectile
            // impact itself: quake type $14 for thirty frames. The screen-offset consumer is
            // still separate, so expose the exact words for debugger watches and integration.
            EarthquakeType = 20;
            EarthquakeTimer = 30;
        }

        // Only cooldowns 21+ are shortened to 20. A normal missile begins at ten, so ordinary
        // wall impact does not extend or replace its remaining fire delay.
        if (sharedProjectiles.CooldownTimer >= 21)
            sharedProjectiles.SetSharedCooldown(20);
    }

}
