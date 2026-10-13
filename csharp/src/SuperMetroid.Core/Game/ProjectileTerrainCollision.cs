using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>Projectile movement against room terrain: the initial beam collision, horizontal and vertical block scans, shot-block reactions (including wave beams through door bands) and killing a beam or missile on contact.</summary>
internal sealed class ProjectileTerrainCollision
{
    private readonly ProjectileAnimation _animation;
    private readonly ProjectileEarthquakes _earthquakes;
    private readonly ProjectileImpactAudio _impactAudio;

    internal ProjectileTerrainCollision(ProjectileAnimation animation, ProjectileEarthquakes earthquakes, ProjectileImpactAudio impactAudio)
    {
        _animation = animation;
        _earthquakes = earthquakes;
        _impactAudio = impactAudio;
    }

    internal static bool MoveHorizontally(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms,
        bool waveBeam = false)
    {
        slot.Position = slot.Position.AdvanceX(EightEightDelta(slot.Velocity.X.SignedRawValue));

        if (waveBeam)
        {
            ScanHorizontalWaveShotReactions(level, slot, roomPlms);
            return false;
        }
        return ScanHorizontalShotReactions(level, slot, roomPlms);
    }

    /// <summary>
    /// Runs the producer-owned collision call made before a new beam receives its speed.
    /// </summary>
    internal bool RunInitialBeamCollision(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms,
        bool waveBeam,
        SamusPowerBombExplosionState powerBomb)
    {
        // The producer explicitly stores zero in both 8.8 speed words. Direction seven is
        // the sole exception: `$90:BDA4/$BDF2` writes -1 before the horizontal scan so the
        // amount-sign branch samples the left radius. Diagonal-left directions retain zero,
        // including the cartridge's counterintuitive right-edge initial horizontal probe.
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(slot.PackedDirection.Direction == SamusProjectileDirection.Left
            ? (short)-1
            : (short)0));
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(0));

        SamusProjectileDirection direction = slot.PackedDirection.Direction;
        bool collided = false;
        if (direction is
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.Right or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.Left or
            SamusProjectileDirection.UpLeft)
        {
            bool horizontalReaction = MoveHorizontally(level, slot, roomPlms, waveBeam);

            // The Wave-beam dispatcher deliberately clears carry after publishing a block
            // reaction. Preserve that contract here instead of allowing our shared scanner's
            // Boolean return to suppress the diagonal vertical probe. Ordinary beams retain
            // the reaction result because their producer aborts immediately on carry set.
            collided = !waveBeam && horizontalReaction;
        }

        // Diagonals call vertical collision only when the no-Wave horizontal call returned
        // carry clear. Wave's native horizontal routine always returns clear even when it
        // publishes a block reaction, so it necessarily reaches this second axis as well.
        bool diagonal = direction is
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.UpLeft;
        if (!collided && (diagonal || direction is
            SamusProjectileDirection.UpFacingRight or
            SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or
            SamusProjectileDirection.UpFacingLeft))
        {
            bool verticalReaction = MoveVertically(level, slot, roomPlms, waveBeam);
            collided = !waveBeam && verticalReaction;
        }

        if (!waveBeam && collided)
        {
            KillBeamOnBlockCollision(slot, powerBomb);
            return true;
        }

        // Wave collision deliberately discards carry after retaining every block/PLM side
        // effect. Its caller will overwrite the temporary zero/-1 speeds immediately.
        return false;
    }

    internal static bool MoveVertically(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms,
        bool waveBeam = false)
    {
        slot.Position = slot.Position.AdvanceY(EightEightDelta(slot.Velocity.Y.SignedRawValue));

        if (waveBeam)
        {
            ScanVerticalWaveShotReactions(level, slot, roomPlms);
            return false;
        }
        return ScanVerticalShotReactions(level, slot, roomPlms);
    }

    internal static bool ScanHorizontalShotReactions(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        int topBlock = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y)) >> 4;
        int bottomBlock = unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1)) >> 4;
        int targetX = slot.Velocity.X.SignedRawValue < 0
            ? unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X))
            : unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1));
        int blockX = targetX >> 4;

        if ((uint)blockX >= (uint)level.Size.X ||
            (uint)topBlock >= (uint)level.Size.Y ||
            (uint)bottomBlock >= (uint)level.Size.Y)
        {
            return false;
        }

        // `$94:A1B5` decrements its target counter only for carry-set reactions. A tall
        // projectile grazing air beside one solid block therefore keeps moving; every
        // touched block still gets its side effect before that aggregate result is tested.
        bool everyBlockSolid = true;
        int span = bottomBlock - topBlock;
        for (int blockY = topBlock; blockY <= bottomBlock; blockY++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
            var context = new ShotSpan(span, bottomBlock - blockY, targetX);
            if (!RunShotReaction(level, slot, block, roomPlms, context, out bool endSpan))
                everyBlockSolid = false;
            if (endSpan) return true;
        }
        return everyBlockSolid;
    }

    internal static bool ScanVerticalShotReactions(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms)
    {
        int leftBlock = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X)) >> 4;
        int rightBlock = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1)) >> 4;
        int targetY = slot.Velocity.Y.SignedRawValue < 0
            ? unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y))
            : unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int blockY = targetY >> 4;

        if ((uint)blockY >= (uint)level.Size.Y ||
            (uint)leftBlock >= (uint)level.Size.X ||
            (uint)rightBlock >= (uint)level.Size.X)
        {
            return false;
        }

        bool everyBlockSolid = true;
        int span = rightBlock - leftBlock;
        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
            var context = new ShotSpan(span, rightBlock - blockX, targetY);
            if (!RunShotReaction(level, slot, block, roomPlms, context, out bool endSpan, horizontalMovement: false))
                everyBlockSolid = false;
            if (endSpan) return true;
        }
        return everyBlockSolid;
    }

    /// <summary>
    /// A block span's scan state: $1A (blocks spanned minus one), $26 (blocks still to check
    /// minus one; the first, top or left, block has $26 = $1A) and $1C (target boundary).
    /// </summary>
    private readonly record struct ShotSpan(int Span, int Remaining, int TargetBoundary);

    /// <summary>
    /// $94:A66A (horizontal) and $94:A71A (vertical) for a beam: the quarter cells of a
    /// square slope that the beam's edges reach in this block. A hit returns carry like a
    /// solid block; unlike a non-square slope it does not end the span.
    /// </summary>
    private static bool SquareSlopeShotReaction(SamusProjectileSlot slot, RoomCollisionBlock block,
        ShotSpan span, bool horizontalMovement)
    {
        if (slot.PackedType.IsFamily(SamusProjectileFamily.Missile) ||
            slot.PackedType.IsFamily(SamusProjectileFamily.SuperMissile))
        {
            // Missiles take the point test in RunMissilePreInstruction; the native missile
            // branch ($1E set) has no caller in this port.
            throw new InvalidOperationException("Missile reached the beam square-slope shot reaction.");
        }

        // Cell index bits: 0 = right half, 1 = bottom half; BTS bit 6 / bit 7 flip them.
        int flips = (block.Bts.SlopeFlipsHorizontally ? 1 : 0) | (block.Bts.SlopeFlipsVertically ? 2 : 0);
        int baseIndex = block.Bts.SlopeShape * 4;
        bool Solid(int cell) => SquareSlopeDefinitions.ReadSamusQuadrant(baseIndex + cell) != 0;

        if (horizontalMovement)
        {
            // The leading edge picks the column half; the scan then tests top/bottom rows.
            int cell = flips ^ ((span.TargetBoundary & 8) >> 3);
            bool topEdgeInBottomHalf = (unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y)) & 8) != 0;
            bool bottomEdgeInBottomHalf = (unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1)) & 8) != 0;
            if (span.Span == 0)
                return (!topEdgeInBottomHalf && Solid(cell)) || (bottomEdgeInBottomHalf && Solid(cell ^ 2));
            if (span.Remaining == 0)
                return bottomEdgeInBottomHalf ? Solid(cell) || Solid(cell ^ 2) : Solid(cell);
            if (span.Remaining == span.Span && topEdgeInBottomHalf)
                return Solid(cell ^ 2);
            return Solid(cell) || Solid(cell ^ 2);
        }

        // The leading edge picks the row half; the scan then tests left/right columns.
        int rowCell = flips ^ ((span.TargetBoundary & 8) >> 2);
        bool leftEdgeInRightHalf = (unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X)) & 8) != 0;
        bool rightEdgeInRightHalf = (unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1)) & 8) != 0;
        if (span.Span == 0)
            return (!leftEdgeInRightHalf && Solid(rowCell)) || (rightEdgeInRightHalf && Solid(rowCell ^ 1));
        if (span.Remaining == 0)
            return rightEdgeInRightHalf ? Solid(rowCell) || Solid(rowCell ^ 1) : Solid(rowCell);
        if (span.Remaining == span.Span && leftEdgeInRightHalf)
            return Solid(rowCell ^ 1);
        return Solid(rowCell) || Solid(rowCell ^ 1);
    }

    private static bool RunShotReaction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomCollisionBlock block,
        RoomPlmSystem? roomPlms,
        ShotSpan span,
        out bool endSpan,
        bool horizontalMovement = true)
    {
        endSpan = false;
        // `$94:9411/$9447` are shared by shot, bomb, grapple, collision, and inside
        // dispatch. Door caps deliberately put the shootable origin at one end and type-
        // `$D` extension words across the other three cells. Resolve those signed BTS
        // links before selecting a reaction; otherwise a centered horizontal beam merely
        // explodes against the middle extension and can never reach BTS `$40..$43`.
        RoomCollisionBlock? resolvedBlock = ResolveShotReactionExtension(level, block);
        if (resolvedBlock is null)
            return false;
        block = resolvedBlock.Value;
        if (block.CollisionType == RoomCollisionType.Slope && block.Bts.SlopeShape < 5)
            return SquareSlopeShotReaction(slot, block, span, horizontalMovement);
        if (block.CollisionType == RoomCollisionType.Slope && block.Bts.SlopeShape >= 5)
        {
            // $94:A147/A15E dispatch non-square slopes through the projectile's
            // perpendicular center block, not every row/column touched by its radius.
            int centerBlock = horizontalMovement ? slot.Position.Y.Whole >> 4 : slot.Position.X.Whole >> 4;
            int slopeBlock = horizontalMovement ? block.Index / level.Size.X : block.Index % level.Size.X;
            if (centerBlock != slopeBlock) return false;

            int x = slot.Position.X.Whole & 15;
            int y = slot.Position.Y.Whole & 15;
            if (block.Bts.SlopeFlipsHorizontally) x ^= 15;
            if (block.Bts.SlopeFlipsVertically) y ^= 15;
            bool hit = SlopeHeightDefinitions.Read(block.Bts.SlopeShape, x) <= y;
            // $94:A5DC/A5DE clears both scan counters on a surface hit. This overrides
            // earlier air rows and ends the span; Wave still discards collision carry.
            endSpan = hit;
            return hit;
        }

        // Chozo orbs and concealed item blocks are type-$C/BTS-$45. Their special
        // reaction does not use the ordinary BTS 0..F shot-block table: header $EED3
        // finds and triggers the already-loaded permanent-item PLM at this origin.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Bts == RoomBlockBehaviorValues.CollectibleTrigger)
        {
            _ = roomPlms?.CollectiblePlms.TryNotifyCollectibleProjectileHit(block.Index, slot.Type);
            return true;
        }

        // Both type-$8 and type-$C BTS-$44 collision routes allocate generic trigger $C83E,
        // which publishes the live projectile word to the resident PLM at this block.
        if (block.CollisionType == RoomCollisionType.SolidBlock &&
            block.Bts == RoomBlockBehaviorValues.ResidentPlmProjectileTrigger)
        {
            NotifyResidentProjectileHit(roomPlms, block, slot.Type);
            return true;
        }

        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            if (block.Bts.IsShootableCollisionProbe)
            {
                // Gate setup ends the caller's span and forces its collision counter.
                // A full PLM pool skips setup entirely, so it must not end the scan.
                endSpan = roomPlms?.BlockReactions.TrySpawnProjectileShotBlock(
                    level, block.Index, block.Bts, slot.Type,
                    block.CollisionType == RoomCollisionType.ShootableBlock) == true;
                return block.CollisionType == RoomCollisionType.ShootableBlock;
            }
            TrySpawnShootableReaction(level, slot, block, roomPlms);
            return block.CollisionType == RoomCollisionType.ShootableBlock;
        }

        // `$94:A175/$A195` return carry for 8/B/E directly. Door ($9), spike ($A), and
        // bombable ($F) also collide; their actor/weapon-gated setup remains a later slice.
        return block.CollisionType is >= RoomCollisionType.SolidBlock and <= RoomCollisionType.BombableBlock;
    }

    private static RoomCollisionBlock? ResolveShotReactionExtension(
        RoomLevelData level,
        RoomCollisionBlock initialBlock)
    {
        RoomCollisionBlock block = initialBlock;
        for (int linkCount = 0; linkCount < 16; linkCount++)
        {
            int offset = block.Bts.ExtensionOffset;
            int targetIndex;
            switch (block.CollisionType)
            {
                case RoomCollisionType.HorizontalExtension:
                    if (offset == 0)
                        return null;
                    targetIndex = block.Index + offset;
                    break;

                case RoomCollisionType.VerticalExtension:
                    if (offset == 0)
                        return null;
                    targetIndex = block.Index + (offset * level.Size.X);
                    break;

                case RoomCollisionType.Air:
                case RoomCollisionType.Slope:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnusedAir:
                case RoomCollisionType.BombableAir:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpikeBlock:
                case RoomCollisionType.SpecialBlock:
                case RoomCollisionType.ShootableBlock:
                case RoomCollisionType.GrappleBlock:
                case RoomCollisionType.BombableBlock:
                    return block;
                default:
                    throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}.");
            }

            if ((uint)targetIndex >= (uint)level.ForegroundEntries.Length)
            {
                throw new InvalidDataException(
                    $"Projectile extension at block {block.Index} resolved outside the " +
                    $"{level.ForegroundEntries.Length}-entry room layer.");
            }
            block = level.GetCollisionBlockByIndex(targetIndex);
        }

        throw new InvalidDataException(
            $"Projectile extension chain from block {initialBlock.Index} did not terminate.");
    }

    internal static void TrySpawnShootableReaction(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomCollisionBlock block,
        RoomPlmSystem? roomPlms)
    {
        if (roomPlms is null)
            return;

        // Downward gate BTS $46-$4D bypasses the normal 16-entry shot-block table and
        // allocates one of eight temporary trigger PLMs. Dispatch it before the table-range
        // validation below; those BTS bytes are intentionally outside that ordinary domain.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Bts.TryGetDownwardGateTrigger(out _))
        {
            roomPlms.Gates.TrySpawnDownwardGateTrigger(level, block.Index, block.Bts, slot.PackedType);
            return;
        }

        // The intact n00b tube, colored doors, eye doors, grey doors, and other resident
        // actors all share type-$C/BTS-$44. The resident PLM determines the accepted family
        // after generic trigger $C83E publishes the projectile word.
        if (block.Bts == RoomBlockBehaviorValues.ResidentPlmProjectileTrigger)
        {
            NotifyResidentProjectileHit(roomPlms, block, slot.Type);
            return;
        }

        // `$94:9EA6[40..43]` selects the four blue-door entry PLMs. These are not
        // ordinary breakable blocks: setup changes the cap origin to type $8 and the
        // cartridge list opens all four blocks over eighteen frames. Keep this dispatch
        // beside the general table lookup so every beam/missile/bomb collision reaches the
        // same bank-$84 owner and power bombs retain Setup_BlueDoor's rejection behavior.
        if (block.Bts.TryGetBlueDoorOrientation(out _))
        {
            roomPlms.DoorPlms.TrySpawnBlueDoorOpening(
                level,
                block.Index,
                block.Bts,
                slot.Type);
            return;
        }

        // Nonnegative BTS 0..F indexes the ordinary retail `$94:9EA6` entries, and $10 is
        // the table's deliberate `$84:B974` collision-probe/no-op PLM. Negative BTS
        // 0..7 is also admitted so shootable-solid can preserve its native area-table no-op
        // slot; shootable-air's method exits without allocating. The RoomPlm owner performs
        // the power-bomb/Super-Missile family checks for entries 8..B synchronously, just as
        // each bank-$84 setup sees the current native projectile type during Spawn_PLM.
        bool translatedBehavior = block.Bts.IsAreaReactionIndex(8) ||
            block.Bts.Value == EscapeAnimalPlmRomData.ReactionBts ||
            block.Bts.IsNormalReactionIndex(16) ||
            block.Bts.IsShootableCollisionProbe;
        if (translatedBehavior)
        {
            roomPlms.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                block.Index,
                block.Bts,
                slot.Type,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
        }
    }

    private static void NotifyResidentProjectileHit(
        RoomPlmSystem? roomPlms,
        RoomCollisionBlock block,
        SamusProjectileTypeWord projectileType)
    {
        if (roomPlms is not null &&
            roomPlms.ProjectileTriggers.TryNotifyResidentProjectileHit(block.Index, projectileType))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Projectile hit resident-trigger block {block.Index} with no active PLM owner.");
    }

    /// <summary>
    /// Bank $94 overlaps DP $12/$13 so an 8.8 velocity word becomes a signed 16.16 delta by
    /// shifting it left eight. Advancing by it as one wrapped 32-bit quantity is exactly
    /// equivalent to its low-word ADC followed by sign-word ADC.
    /// </summary>
    internal static SnesSignedSixteenSixteen EightEightDelta(short velocity) =>
        SnesSignedEightEight.FromSignedRaw(velocity).ToSixteenSixteenDelta();

    /// <summary>
    /// $90:AE06 Kill_Projectile for a beam, as bank $94's block collision calls it: the
    /// explosion's library-two $0C queues ($93:80EF, no cinematic test) before the swap.
    /// </summary>
    internal void KillBeamOnBlockCollision(SamusProjectileSlot slot,
        SamusPowerBombExplosionState powerBomb)
    {
        _impactAudio.RequestBeamImpactSound(powerBomb);
        KillBeam(slot);
    }

    internal static void KillBeam(SamusProjectileSlot slot)
    {
        // `$90:AE3A` moves the explosion anchor to the leading edge before bank $93 swaps
        // the instruction list. Diagonals adjust both axes in their respective signs.
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        if (direction is 1 or 2 or 3)
            slot.Position += new Offset16<RoomPixels>(unchecked((short)slot.Radius.X), 0);
        else if (direction is 6 or 7 or 8)
            slot.Position -= new Offset16<RoomPixels>(unchecked((short)slot.Radius.X), 0);

        if (direction is 0 or 1 or 8 or 9)
            slot.Position -= new Offset16<RoomPixels>(0, unchecked((short)slot.Radius.Y));
        else if (direction is 3 or 4 or 5 or 6)
            slot.Position += new Offset16<RoomPixels>(0, unchecked((short)slot.Radius.Y));

        slot.Type = slot.PackedType.WithFamily(SamusProjectileFamily.BeamExplosion);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.NonBeam.BeamExplosionInstructionPointer);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;
    }

    internal void KillMissile(
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        // The native kill dispatcher clears families beyond missiles immediately. A Super
        // Missile's supplemental probe can collide again after becoming an explosion;
        // restarting its animation here would extend both its lifetime and damage.
        if (slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb)
        {
            _animation.ClearProjectile(slot);
            return;
        }

        // Missile point collisions already supply the impact coordinate. Only the beam
        // branch of the native dispatcher applies a leading-edge radius adjustment.

        // Impact audio is separate from launch audio and can also originate in enemy
        // collision before the projectile movement owner executes.
        _impactAudio.RequestMissileImpactSound(sharedProjectiles.PowerBombExplosion);
        bool wasSuperMissile = slot.PackedType.IsFamily(SamusProjectileFamily.SuperMissile);
        slot.Type = slot.PackedType.WithFamily(SamusProjectileFamily.MissileExplosion);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            wasSuperMissile
                ? SamusProjectileRomData.NonBeam.SuperMissileExplosionInstructionPointer
                : SamusProjectileRomData.NonBeam.MissileExplosionInstructionPointer);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;

        if (wasSuperMissile)
        {
            // The impact writes the shared room quake immediately; later native producers
            // can replace it before the frame's shake handler consumes the request.
            _earthquakes.RequestSuperMissileEarthquake(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType);
        }

        // Only cooldowns 21+ are shortened to 20. A normal missile begins at ten, so ordinary
        // wall impact does not extend or replace its remaining fire delay.
        if (sharedProjectiles.CooldownTimer >= 21)
            sharedProjectiles.SetSharedCooldown(20);
    }
    /// <summary>Preserves bank $94:A352's byte indexing and chained-ADC carry.</summary>
    internal static void ScanHorizontalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort top = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y));
        ushort bottom = unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int remaining = unchecked((ushort)(bottom - (top & 0xfff0))) >> 4;
        int span = remaining;
        ushort targetX = unchecked((ushort)(slot.Velocity.X.SignedRawValue < 0
            ? slot.Position.X.Whole - slot.Radius.X : slot.Position.X.Whole + slot.Radius.X - 1));
        // XBA sets N/Z from its resulting LOW BYTE even with a 16-bit accumulator.
        // BMI therefore tests the original coordinate's high-byte sign, not bit 7
        // of its pixel offset. Testing the latter disables Wave reactions throughout
        // the lower half of every screen, including ordinary blue-door caps.
        if (remaining >= 16 || unchecked((short)slot.Position.Y.Whole) < 0 ||
            (SnesEightEight.WholeOf(slot.Position.Y.Whole)) >= (level.Size.Y + 15) / 16 ||
            (SnesLittleEndian.HighByte(targetX)) >= (level.Size.X + 15) / 16)
            return;

        int product = unchecked((byte)(top >> 4)) * unchecked((byte)level.Size.X);
        ushort byteOffset = unchecked((ushort)((product + (targetX >> 4)) * 2));
        do
        {
            int index = byteOffset >> 1;
            if (index < level.ForegroundEntries.Length)
            {
                RoomCollisionBlock block = level.GetCollisionBlockByIndex(index);
                if ((byteOffset & 1) != 0)
                {
                    if (index + 1 >= level.ForegroundEntries.Length)
                        throw new NotSupportedException("Odd Wave collision read crosses the modeled level-data allocation.");
                    ushort next = level.GetCollisionBlockByIndex(index + 1).LevelWord;
                    // Dispatch consumes the unaligned word; BTS and PLM setup
                    // still use floor(byteOffset / 2), including aligned writes.
                    block = block with { LevelWord = SnesLittleEndian.Word(SnesLittleEndian.HighByte(block.LevelWord), unchecked((byte)next)) };
                }
                RunShotReaction(level, slot, block, roomPlms, new ShotSpan(span, remaining, targetX), out bool endSpan);
                // Wave discards collision carry, but still honors PLM scan termination.
                if (endSpan) return;
            }

            // There is only one CLC before the two ADCs. Do not simplify this
            // to offset += width * 2: the first carry causes ceiling odd reads.
            int first = byteOffset + level.Size.X;
            byteOffset = unchecked((ushort)((ushort)first + level.Size.X + (first > ushort.MaxValue ? 1 : 0)));
        } while (--remaining >= 0);
    }

    /// <summary>
    /// Preserves bank $94:A3E4's linear tile addressing across the room's side edges.
    /// </summary>
    internal static void ScanVerticalWaveShotReactions(
        RoomLevelData level, SamusProjectileSlot slot, RoomPlmSystem? roomPlms)
    {
        ushort left = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X));
        ushort right = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1));
        int remaining = unchecked((ushort)(right - (left & 0xfff0))) >> 4;
        if (remaining >= 16) return;
        int span = remaining;
        ushort targetY = unchecked((ushort)(slot.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole - slot.Radius.Y
            : slot.Position.Y.Whole + slot.Radius.Y - 1));
        int centerScreenX = SnesEightEight.WholeOf(slot.Position.X.Whole);
        if (centerScreenX >= (level.Size.X + 15) / 16 ||
            (SnesLittleEndian.HighByte(targetY)) >= (level.Size.Y + 15) / 16)
            return;

        // The hardware multiplier consumes bytes. The following addition and
        // byte-offset shift consume words. X is deliberately not clipped to the
        // room width: right-edge spans reach the following row; left underflow
        // starts 4095 tiles forward while the center remains inside the room.
        // The native block reaction dispatcher bounds the resulting allocation
        // offset, not the horizontal extent of this individual tile span.
        int rowOffset = unchecked((byte)(targetY >> 4)) * level.Size.X;
        ushort byteOffset = unchecked((ushort)((rowOffset + (left >> 4)) * 2));
        do
        {
            int index = byteOffset >> 1;
            if (index < level.Size.X * level.Size.Y)
            {
                RunShotReaction(level, slot, level.GetCollisionBlock(new(unchecked((ushort)(index % level.Size.X)), unchecked((ushort)(index / level.Size.X)))), roomPlms,
                    new ShotSpan(span, remaining, targetY), out bool endSpan, horizontalMovement: false);
                if (endSpan) return;
            }
            byteOffset = unchecked((ushort)(byteOffset + 2));
        } while (--remaining >= 0);
    }
}
