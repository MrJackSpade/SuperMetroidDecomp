using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
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
        if (!slot.HasEnemyCollisionPayload)
            return;

        if (slot.PackedType.Family == SamusProjectileFamily.SuperMissile)
        {
            // The collision walkers write the enemy-hit quake before dispatching the enemy's
            // shot callback. This remains observable even when gold Ninja armor ignores a
            // fresh, not-yet-linked Super Missile.
            RequestSuperMissileEarthquake(SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType);
        }

        if (markCollisionState)
            slot.Direction = slot.PackedDirection.WithCollisionLifecycleState();
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
        if (!slot.HasEnemyCollisionPayload)
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
                ? SamusProjectileRomData.Beams.ChargedDataPointers
                : SamusProjectileRomData.Beams.UnchargedDataPointers;
            dataPointerIndex = slot.PackedType.BeamCombinationIndex;
        }
        else if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            dataPointerTable = SamusProjectileRomData.NonBeam.DataPointers;
            dataPointerIndex = slot.PackedType.FamilyValue >> 8;
        }
        else
        {
            // The native reflection producer only selects beams and the two missile
            // families. Reaching this method with a bomb/explosion payload is corrupt
            // scheduler state rather than an unimplemented reflection behavior.
            throw new InvalidDataException(
                $"Projectile family ${slot.PackedType.FamilyValue:X3} is not reflectable.");
        }

        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(dataPointerTable + dataPointerIndex * 2);
        int data = SamusProjectileRomData.Banks.Projectile | dataPointer;
        slot.Damage = SamusProjectileDamageDefinitions.Read(data);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            AddWithinBank(data, 2 + slot.PackedDirection.DirectionIndex * 2));
        slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(slot.InstructionPointer + 5)));
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
        int slotIndex,
        bool blocksPlasmaBeam = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        if ((uint)slotIndex >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        SamusProjectileSlot slot = _slots[slotIndex];
        if (!slot.HasEnemyCollisionPayload)
            return false;

        if (slot.PreInstruction is SamusProjectilePreInstruction.IceCombo or
            SamusProjectilePreInstruction.IceComboOutward or SamusProjectilePreInstruction.WaveCombo or
            SamusProjectilePreInstruction.SpazerCombo or SamusProjectilePreInstruction.SpazerComboFalling or
            SamusProjectilePreInstruction.PlasmaCombo)
        {
            // Enemy collision only marks the native direction word. These particles own
            // hit deletion (and Spazer's paired deletion) in their next pre-instruction;
            // replacing that handler with a beam explosion loses their family lifecycle.
            // Plasma normally pierces, unless the target explicitly stops Plasma beams.
            ApplyEnemyCollisionPrelude(slotIndex,
                blocksPlasmaBeam || (slot.PackedType.BeamCombinationIndex & (int)SamusBeamFlags.Plasma) == 0);
            return true;
        }

        if (slot.PackedDirection.HasLowByteLifecycleState)
            return false;

        switch (slot.PackedType.Family)
        {
            case SamusProjectileFamily.Beam:
                // Native enemy collision marks direction bit $10 only when the
                // target blocks Plasma or the shot lacks Plasma. Ordinary penetrating
                // beams, not just special beam-combo particles, retain their lifecycle.
                if (!blocksPlasmaBeam &&
                    (slot.PackedType.BeamCombinationIndex & (int)SamusBeamFlags.Plasma) != 0)
                    return true;
                // $A0:9A07 only marks the shot; enemy hits never reach Kill_Projectile's sound.
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

    /// <summary>Allocates a free trail pair and initializes its animation lists and offsets for the projectile's current frame.</summary>
    /// <param name="bus">Address space used to read direction-specific trail coordinates.</param>
    /// <param name="projectile">Beam or missile whose family, direction, and installed instruction frame select the trail.</param>
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
        trail.Left.InstructionPointer = ProjectileTrailDefinitions.ReadSelector(
            SamusProjectileRomData.Trails.LeftInstructionPointers + pointerIndex * 2);
        trail.Right.InstructionPointer = ProjectileTrailDefinitions.ReadSelector(
            SamusProjectileRomData.Trails.RightInstructionPointers + pointerIndex * 2);

        // Retail reads the previously installed record, even when the animation timer
        // will expire later this frame. Reading ahead advances Spazer's trail spread early.
        ushort animationFrame = GetTrailAnimationFrame(projectile);
        int direction = projectile.PackedDirection.DirectionIndex;
        int familyTable = (projectile.Type & 0x0020) != 0
            ? SamusProjectileRomData.Trails.SpazerSbaOffsetFamilies
            : projectile.PackedType.IsChargedBeam
                ? SamusProjectileRomData.Trails.ChargedOffsetFamilies
                : SamusProjectileRomData.Trails.UnchargedOffsetFamilies;
        ushort directionTable = ProjectileTrailCoordinateDefinitions.ReadFamilyPointer(
            familyTable + projectile.PackedType.BeamCombinationIndex * 2);
        ushort offsetList = ProjectileTrailCoordinateDefinitions.ReadDirectionPointer(
            bus,
            SamusProjectileRomData.Banks.PaletteAndTrailData |
                unchecked((ushort)(directionTable + direction * 2)));
        int offsets = SamusProjectileRomData.Banks.PaletteAndTrailData |
            unchecked((ushort)(offsetList + animationFrame * 4));

        // Native performs WORD reads and consumes their high bytes after XBA.
        // Reflection can expose the word preceding a freshly installed animation
        // list, sending this lookup into reserved B-bus space. Preserve the actual
        // operand-driven open bus there instead of changing trail timing or hiding
        // every unsupported hardware read behind a zero fallback.
        ushort y = (ushort)offsets;
        byte leftY = (byte)(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 0, y) >> 8);
        byte leftX = (byte)(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 0, unchecked((ushort)(y - 1))) >> 8);
        byte rightY = (byte)(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 2, y) >> 8);
        byte rightX = (byte)(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 1, y) >> 8);
        trail.Left.XPosition = AddSignedOffset(projectile.XPosition, leftX, -4);
        trail.Left.YPosition = AddSignedOffset(projectile.YPosition, leftY, -4);
        trail.Right.XPosition = AddSignedOffset(projectile.XPosition, rightX, -4);
        trail.Right.YPosition = AddSignedOffset(projectile.YPosition, rightY, -4);
    }

    /// <summary>Reads the frame word adjacent to the currently installed projectile instruction cursor.</summary>
    /// <param name="projectile">Projectile providing the bank-$93 instruction pointer.</param>
    /// <returns>The word used to select this update's trail spread.</returns>
    private static ushort GetTrailAnimationFrame(SamusProjectileSlot projectile)
    {
        // $93:81D8-$81E3 unconditionally reads (instruction pointer - 8) + 6.
        // The upstream C port's timer-one lookahead is absent from the pinned ROM.
        ushort frameAddress = unchecked((ushort)(projectile.InstructionPointer - 2));
        return SamusProjectileInstructionDefinitions.ReadWord(SamusProjectileRomData.Banks.Projectile | frameAddress);
    }

    /// <summary>Adds a signed byte displacement and fixed adjustment to a wrapping room coordinate.</summary>
    /// <param name="origin">Whole-pixel coordinate before the offset.</param>
    /// <param name="encodedOffset">Two's-complement byte displacement read from the trail table.</param>
    /// <param name="constant">Additional pixel adjustment for the trail anchor.</param>
    /// <returns>The adjusted 16-bit room coordinate.</returns>
    private static ushort AddSignedOffset(ushort origin, byte encodedOffset, int constant) =>
        unchecked((ushort)(origin + unchecked((sbyte)encodedOffset) + constant));

    /// <summary>Advances one trail-side instruction stream unless time is frozen, then draws it only when its full screen coordinates are visible.</summary>
    /// <param name="bus">Address space supplying the side's movement-list records.</param>
    /// <param name="oam">Frame OAM buffer receiving an in-viewport trail sprite.</param>
    /// <param name="pair">Both trail sides, since movement opcodes may alter their sibling's coordinates.</param>
    /// <param name="layer1X">Horizontal camera origin used for the native unsigned visibility test.</param>
    /// <param name="layer1Y">Vertical camera origin used for the native unsigned visibility test.</param>
    /// <param name="timeIsFrozen"><see langword="true"/> preserves instruction timers while still allowing the current side to draw.</param>
    /// <param name="isLeft">Selects which instruction stream and OAM position to process.</param>
    /// <param name="artwork">Optional installed trail art used to resolve the current tile attributes.</param>
    private static void HandleTrailSideAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileTrailSlot pair,
        ushort layer1X,
        ushort layer1Y,
        bool timeIsFrozen,
        bool isLeft,
        Assets.ProjectileTrailCatalog? artwork)
    {
        SamusProjectileTrailSide side = isLeft ? pair.Left : pair.Right;
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
                    ushort instructionOrTimer = ProjectileTrailProgramDefinitions.Read(
                        bus,
                        SamusProjectileRomData.Banks.Movement | pointer);
                    if ((instructionOrTimer & 0x8000) == 0)
                    {
                        side.InstructionTimer = instructionOrTimer;
                        if (instructionOrTimer == 0)
                            return;

                        side.TileNumberAttributes = artwork is not null ? artwork.Resolve(pointer) :
                            ProjectileTrailProgramDefinitions.Read(bus,
                                SamusProjectileRomData.Banks.Movement |
                                unchecked((ushort)(pointer + 2)));
                        side.InstructionPointer = unchecked((ushort)(pointer + 4));
                        break;
                    }

                    // Bank $90 stores executable instruction addresses inline. Each handler
                    // returns to the parser with X already advanced past its one-word opcode.
                    pointer = unchecked((ushort)(pointer + 2));
                    switch (instructionOrTimer)
                    {
                        // The opcode names its destination array. Both streams dispatch
                        // the same handlers, including writes to their sibling's position.
                        case SamusProjectileRomData.Trails.MoveLeftDown:
                            pair.Left.YPosition = unchecked((ushort)(pair.Left.YPosition + 1));
                            break;
                        case SamusProjectileRomData.Trails.MoveRightDown:
                            pair.Right.YPosition = unchecked((ushort)(pair.Right.YPosition + 1));
                            break;
                        case SamusProjectileRomData.Trails.MoveLeftUp:
                            pair.Left.YPosition = unchecked((ushort)(pair.Left.YPosition - 1));
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
            artwork is null ? side.TileNumberAttributes : artwork.ResolveCurrent(side.InstructionPointer, side.TileNumberAttributes));
    }

    /// <summary>Applies the projectile's signed fixed-point X velocity, then scans the leading vertical span for shot reactions.</summary>
    /// <param name="level">Room collision geometry traversed by the projectile.</param>
    /// <param name="slot">Projectile position, radius, velocity, and family used by the scan.</param>
    /// <param name="roomPlms">Optional owner for shootable-block and resident-PLM side effects.</param>
    /// <param name="waveBeam"><see langword="true"/> applies Wave's non-stopping reaction semantics while still invoking block effects.</param>
    /// <returns><see langword="true"/> when ordinary shot collision should stop movement; Wave collisions are consumed separately.</returns>
    private static bool MoveHorizontally(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms,
        bool waveBeam = false)
    {
        (slot.XPosition, slot.XSubposition) = AddVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity);

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
    private bool RunInitialBeamCollision(
        ISnesAddressSpace bus,
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
        slot.XVelocity = slot.PackedDirection.Direction == SamusProjectileDirection.Left
            ? (short)-1
            : (short)0;
        slot.YVelocity = 0;

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
            KillBeamOnBlockCollision(bus, slot, powerBomb);
            return true;
        }

        // Wave collision deliberately discards carry after retaining every block/PLM side
        // effect. Its caller will overwrite the temporary zero/-1 speeds immediately.
        return false;
    }

    /// <summary>Applies the projectile's signed fixed-point Y velocity, then scans the leading horizontal span for shot reactions.</summary>
    /// <param name="level">Room collision geometry traversed by the projectile.</param>
    /// <param name="slot">Projectile position, radius, velocity, and family used by the scan.</param>
    /// <param name="roomPlms">Optional owner for shootable-block and resident-PLM side effects.</param>
    /// <param name="waveBeam"><see langword="true"/> applies Wave's non-stopping reaction semantics while still invoking block effects.</param>
    /// <returns><see langword="true"/> when ordinary shot collision should stop movement; Wave collisions are consumed separately.</returns>
    private static bool MoveVertically(
        RoomLevelData level,
        SamusProjectileSlot slot,
        RoomPlmSystem? roomPlms,
        bool waveBeam = false)
    {
        (slot.YPosition, slot.YSubposition) = AddVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity);

        if (waveBeam)
        {
            ScanVerticalWaveShotReactions(level, slot, roomPlms);
            return false;
        }
        return ScanVerticalShotReactions(level, slot, roomPlms);
    }

    /// <summary>Scans every block touched by the projectile's horizontal leading edge and aggregates their collision reactions.</summary>
    /// <param name="level">Room collision geometry and block lookup.</param>
    /// <param name="slot">Projectile bounds and horizontal travel direction.</param>
    /// <param name="roomPlms">Optional PLM owner receiving projectile-trigger side effects.</param>
    /// <returns>Whether the scan's collision counter reached its stopping condition.</returns>
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
        int span = bottomBlock - topBlock;
        for (int blockY = topBlock; blockY <= bottomBlock; blockY++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
            var context = new ShotSpan(span, bottomBlock - blockY, targetX);
            if (!RunShotReaction(level, slot, block, roomPlms, context, out bool endSpan))
                everyBlockSolid = false;
            if (endSpan) return true;
        }
        return everyBlockSolid;
    }

    /// <summary>Scans every block touched by the projectile's vertical leading edge and aggregates their collision reactions.</summary>
    /// <param name="level">Room collision geometry and block lookup.</param>
    /// <param name="slot">Projectile bounds and vertical travel direction.</param>
    /// <param name="roomPlms">Optional PLM owner receiving projectile-trigger side effects.</param>
    /// <returns>Whether the scan's collision counter reached its stopping condition.</returns>
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
        int span = rightBlock - leftBlock;
        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
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
    /// <param name="Span">Total number of blocks spanned minus one for the current leading edge.</param>
    /// <param name="Remaining">Blocks still to scan after the current block, matching the native counter's remaining-minus-one value.</param>
    /// <param name="TargetBoundary">Whole-pixel edge coordinate used to select the leading quarter-cell.</param>
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
            bool topEdgeInBottomHalf = (unchecked((ushort)(slot.YPosition - slot.YRadius)) & 8) != 0;
            bool bottomEdgeInBottomHalf = (unchecked((ushort)(slot.YPosition + slot.YRadius - 1)) & 8) != 0;
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
        bool leftEdgeInRightHalf = (unchecked((ushort)(slot.XPosition - slot.XRadius)) & 8) != 0;
        bool rightEdgeInRightHalf = (unchecked((ushort)(slot.XPosition + slot.XRadius - 1)) & 8) != 0;
        if (span.Span == 0)
            return (!leftEdgeInRightHalf && Solid(rowCell)) || (rightEdgeInRightHalf && Solid(rowCell ^ 1));
        if (span.Remaining == 0)
            return rightEdgeInRightHalf ? Solid(rowCell) || Solid(rowCell ^ 1) : Solid(rowCell);
        if (span.Remaining == span.Span && leftEdgeInRightHalf)
            return Solid(rowCell ^ 1);
        return Solid(rowCell) || Solid(rowCell ^ 1);
    }

    /// <summary>Resolves one touched block's shot behavior, including extension links, slope quadrants, and PLM-trigger reactions.</summary>
    /// <param name="level">Room geometry used to follow extension blocks and interpret slopes.</param>
    /// <param name="slot">Projectile family, bounds, and position used by reaction-specific rules.</param>
    /// <param name="block">Currently touched collision block, possibly an extension of a reaction origin.</param>
    /// <param name="roomPlms">Optional PLM owner for collectible, resident, and shootable triggers.</param>
    /// <param name="span">Counters and leading-edge coordinate for the scan containing this block.</param>
    /// <param name="endSpan">Receives whether a special surface or spawned gate ends further scanning.</param>
    /// <param name="horizontalMovement"><see langword="true"/> selects horizontal leading-edge geometry; otherwise use vertical geometry.</param>
    /// <returns>Whether this block reports collision carry to the caller.</returns>
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
            int centerBlock = horizontalMovement ? slot.YPosition >> 4 : slot.XPosition >> 4;
            int slopeBlock = horizontalMovement ? block.Index / level.WidthInBlocks : block.Index % level.WidthInBlocks;
            if (centerBlock != slopeBlock) return false;

            int x = slot.XPosition & 15;
            int y = slot.YPosition & 15;
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
            _ = roomPlms?.TryNotifyCollectibleProjectileHit(block.Index, slot.Type);
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
                endSpan = roomPlms?.TrySpawnProjectileShotBlock(
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

    /// <summary>Follows signed horizontal or vertical BTS extension links to their origin, rejecting cycles beyond the supported chain depth.</summary>
    /// <param name="level">Room layer providing block indices and width for vertical offsets.</param>
    /// <param name="initialBlock">First block encountered by the projectile scan.</param>
    /// <returns>The reaction-origin block, or <see langword="null"/> when a zero-offset extension has no reaction origin.</returns>
    /// <exception cref="InvalidDataException">A link leaves the room layer or the chain exceeds sixteen links.</exception>
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
                    targetIndex = block.Index + (offset * level.WidthInBlocks);
                    break;

                default:
                    return block;
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

    /// <summary>Dispatches shootable-block BTS values to their specialized PLM setup when the current projectile is eligible.</summary>
    /// <param name="level">Room data needed by gate and shot-block setup routines.</param>
    /// <param name="slot">Projectile whose packed type is passed to the selected reaction owner.</param>
    /// <param name="block">Shootable block or air cell whose BTS selects a reaction.</param>
    /// <param name="roomPlms">PLM owner; absence skips allocation while preserving the collision scan.</param>
    private static void TrySpawnShootableReaction(
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
            roomPlms.TrySpawnDownwardGateTrigger(level, block.Index, block.Bts, slot.PackedType);
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
            roomPlms.TrySpawnBlueDoorOpening(
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
            roomPlms.TrySpawnProjectileShotBlock(
                level,
                block.Index,
                block.Bts,
                slot.Type,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
        }
    }

    /// <summary>Publishes a projectile hit to the PLM already resident at a shared-trigger block.</summary>
    /// <param name="roomPlms">Active resident-PLM owner required to process the hit.</param>
    /// <param name="block">Resolved trigger-origin block identifying the resident PLM.</param>
    /// <param name="projectileType">Packed projectile family and beam flags passed to the resident handler.</param>
    /// <exception cref="InvalidOperationException">There is no active PLM owner for the resident-trigger hit.</exception>
    private static void NotifyResidentProjectileHit(
        RoomPlmSystem? roomPlms,
        RoomCollisionBlock block,
        SamusProjectileTypeWord projectileType)
    {
        if (roomPlms is not null &&
            roomPlms.TryNotifyResidentProjectileHit(block.Index, projectileType))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Projectile hit resident-trigger block {block.Index} with no active PLM owner.");
    }

    /// <summary>Adds a signed 8.8 projectile velocity to a whole/subpixel position with native 16.16 wrapping.</summary>
    /// <param name="position">Whole-pixel coordinate before movement.</param>
    /// <param name="subposition">Fractional coordinate before movement.</param>
    /// <param name="velocity">Signed 8.8 velocity word.</param>
    /// <returns>The updated whole and fractional position words.</returns>
    private static (ushort Position, ushort Subposition) AddVelocity(
        ushort position,
        ushort subposition,
        short velocity)
    {
        // Bank $94 overlaps DP $12/$13 so an 8.8 velocity becomes a signed 16.16 delta by
        // shifting it left eight. Performing the addition as one wrapped 32-bit quantity is
        // exactly equivalent to its low-word ADC followed by sign-word ADC.
        SnesFixedPosition result = SnesSignedEightEight
            .FromSignedRaw(velocity)
            .ToSixteenSixteenDelta()
            .AddTo(position, subposition);
        return (result.Whole, result.Fraction);
    }

    /// <summary>
    /// $90:AE06 Kill_Projectile for a beam, as bank $94's block collision calls it: the
    /// explosion's library-two $0C queues ($93:80EF, no cinematic test) before the swap.
    /// </summary>
    private void KillBeamOnBlockCollision(ISnesAddressSpace bus, SamusProjectileSlot slot,
        SamusPowerBombExplosionState powerBomb)
    {
        RequestBeamImpactSound(powerBomb);
        KillBeam(bus, slot);
    }

    /// <summary>Moves a beam-impact anchor to its leading edge and replaces its payload with the bank-$93 beam explosion.</summary>
    /// <param name="bus">Address space supplied by the collision caller; the compiled beam-explosion selector requires no live bus read.</param>
    /// <param name="slot">Beam slot to convert in place.</param>
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
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.NonBeam.BeamExplosionInstructionPointer);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;
    }

    /// <summary>Converts an ordinary missile impact to its explosion, clears unsupported families, and applies shared impact effects.</summary>
    /// <param name="bus">Address space supplied by the impact caller; this conversion uses the compiled explosion selectors.</param>
    /// <param name="slot">Projectile slot receiving the explosion or being cleared.</param>
    /// <param name="sharedProjectiles">Shared bomb/explosion owner for impact audio and missile cooldown updates.</param>
    private void KillMissile(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        // The native kill dispatcher clears families beyond missiles immediately. A Super
        // Missile's supplemental probe can collide again after becoming an explosion;
        // restarting its animation here would extend both its lifetime and damage.
        if (slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb)
        {
            ClearProjectile(slot);
            return;
        }

        // Missile point collisions already supply the impact coordinate. Only the beam
        // branch of the native dispatcher applies a leading-edge radius adjustment.

        // Impact audio is separate from launch audio and can also originate in enemy
        // collision before the projectile movement owner executes.
        RequestMissileImpactSound(sharedProjectiles.PowerBombExplosion);
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
            RequestSuperMissileEarthquake(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType);
        }

        // Only cooldowns 21+ are shortened to 20. A normal missile begins at ten, so ordinary
        // wall impact does not extend or replace its remaining fire delay.
        if (sharedProjectiles.CooldownTimer >= 21)
            sharedProjectiles.SetSharedCooldown(20);
    }

}
