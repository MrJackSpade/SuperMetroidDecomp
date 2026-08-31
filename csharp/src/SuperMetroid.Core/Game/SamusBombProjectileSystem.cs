using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five bomb slots shared by bank-$90 projectile logic, bank-$93 instruction lists,
/// bank-$94 block reactions, and bank-$A0 Samus/projectile overlap handling.
/// </summary>
/// <remarks>
/// The cartridge does not have a tidy C-style <c>Bomb</c> structure. Bomb slots are the
/// upper five indices of several ten-word projectile arrays. Consequently, for example,
/// <c>projectile_variables[5]</c> at WRAM $0C86 is also named <c>bomb_timers[0]</c> by
/// routines that index from the bomb half of the allocation. This port gives each slot
/// semantic fields, but the comments below retain every important alias and source seam.
/// </remarks>
public sealed class SamusBombProjectileSystem
{
    /// <summary>Number of physical bomb slots at projectile byte indices $0A-$12.</summary>
    public const int SlotCount = 5;

    /// <summary>Equipped-item bit checked by <c>HudSelectionHandler_MorphBall_Helper</c>.</summary>
    /// <summary>Normal-bomb projectile type written by $90:BF9D.</summary>
    public const ushort NormalBombType = 0x0500;

    /// <summary>Power-bomb projectile type formed from HUD item index three.</summary>
    public const ushort PowerBombType = 0x0300;

    private const int NonBeamProjectileDataPointerTable = 0x9383f1;
    private const int BombExplosionInstructionPointerAddress = 0x938683;
    private const ushort ProjectileInstructionDelete = 0x822f;
    private const ushort ProjectileInstructionGoto = 0x8239;
    private const ushort NormalBombCooldown = 0x0010;
    private const ushort PowerBombCooldown = 0x0028;
    private const ushort InitialBombTimer = 60;

    private readonly SamusBombProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount).Select(index => new SamusBombProjectileSlot(index)).ToArray();

    /// <summary>The five slots in the same low-to-high order as WRAM $0C86-$0C8E.</summary>
    public IReadOnlyList<SamusBombProjectileSlot> Slots => _slots;

    /// <summary>Bank-$88 owner of the flash, damaging radius, and armed flag.</summary>
    public SamusPowerBombExplosionState PowerBombExplosion { get; } = new();

    /// <summary>WRAM $0CD2, maintained independently from slot scans by the original.</summary>
    public ushort BombCounter { get; private set; }

    /// <summary>WRAM $0CCC. Its low byte gates another bomb while one is active.</summary>
    public ushort CooldownTimer { get; private set; }

    /// <summary>
    /// Replaces WRAM <c>$0CCC</c> from another Samus-projectile producer.
    /// </summary>
    /// <remarks>
    /// Beams, missiles, power bombs, and normal bombs do not own separate cooldowns in the
    /// cartridge. They all read and write the same word. Ordinary projectiles live in a
    /// separate translated class because the native slot arrays split cleanly at byte index
    /// <c>$0A</c>, but that host boundary must not accidentally create a second clock.
    /// </remarks>
    internal void SetSharedCooldown(ushort value) => CooldownTimer = value;

    /// <summary>
    /// Replaces WRAM <c>$0CD2</c> from an enemy-owned Samus interaction. Shitroid writes
    /// five while it drains Samus to suppress projectile allocation, then clears the word
    /// at the terminal one-energy transition without disturbing the shared cooldown.
    /// </summary>
    internal void SetSharedBombCounter(ushort value) => BombCounter = value;

    /// <summary>Observable result of the most recent translated alpha/interaction pass.</summary>
    public BombProjectileFrameResult LastFrameResult { get; private set; }

    /// <summary>
    /// Runs the bomb-owned portion of Samus frame-handler alpha, followed by the bank-$A0
    /// overlap pass that the main gameplay loop invokes before movement-handler beta.
    /// </summary>
    public BombProjectileFrameResult StepFrame(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        RoomPlmSystem? roomPlms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);

        // RunOneFrameOfGame invokes HdmaObjectHandler before GameState_8 reaches Samus's
        // frame handler. A power bomb spawned later in this method consequently receives
        // its first radius update on the next frame, not on its fuse-expiration frame.
        bool crystalFlashWindowWasActive = PowerBombExplosion.Phase is
            PowerBombExplosionPhase.CrystalFlashExplosion or
            PowerBombExplosionPhase.CrystalFlashAfterglow;
        bool powerBombCleanup = PowerBombExplosion.StepFrame(bus);
        if (powerBombCleanup && !crystalFlashWindowWasActive)
        {
            // $88:8B4E offers Crystal Flash only if Samus has remained on the exact bomb
            // origin. Failure (including moving one pixel) releases $0CEA immediately.
            bool crystalFlashStarted =
                samus.XPosition == PowerBombExplosion.XPosition &&
                samus.YPosition == PowerBombExplosion.YPosition &&
                samus.CrystalFlash.TryBegin(
                    bus,
                    samus,
                    controllerInput,
                    (ushort)SnesButton.X,
                    skipInputCheck: false);
            if (!crystalFlashStarted)
                PowerBombExplosion.ReleaseFlag();
        }

        // $90:AC1C runs before the movement-type-specific HUD handler. A value of one
        // therefore reaches zero in time for a new Shoot edge during this same frame.
        StepCooldown();

        int? placedSlot = null;
        if (SamusState.IsStableBallPose(samus.Pose))
        {
            placedSlot = TryPlaceBomb(
                bus,
                samus,
                controllerInput,
                controllerNewInput);
        }

        bool explosionStarted = false;
        bool projectileDeleted = false;
        var blockReactions = new List<BombBlockReaction>();

        // HandleProjectile starts at projectile byte index $12 and walks downward. Those
        // are bomb slots four through zero after removing the ordinary-projectile half.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusBombProjectileSlot slot = _slots[slotIndex];
            if (slot.InstructionPointer == 0)
                continue;

            bool slotExplosionStarted = RunBombPreInstruction(
                bus,
                level,
                slot,
                blockReactions,
                roomPlms,
                samus.LiquidPhysics.AreaIndex);
            explosionStarted |= slotExplosionStarted;

            // The native loop still calls $93:81E9 after a pre-instruction clears a slot.
            // Its cleared timer underflows and returns without reading pointer zero. An
            // inactive host slot has the same externally visible result, so skip the
            // otherwise meaningless decrement rather than pretending bank $93:0000 ran.
            if (!slot.IsActive)
            {
                projectileDeleted = true;
                continue;
            }

            projectileDeleted |= RunProjectileInstructionHandler(bus, slot);
        }

        // GameState_8 invokes $A0:9785 after frame-handler alpha (which placed/updated the
        // bombs) and before beta moves Samus. Store only the low direction byte here. The
        // next alpha pass will run $90:DE78/$90:DF99 and add command bit $0800.
        byte publishedDirection = PublishBombJumpOverlap(samus);

        LastFrameResult = new BombProjectileFrameResult(
            placedSlot,
            explosionStarted,
            projectileDeleted,
            publishedDirection,
            blockReactions.ToArray());
        return LastFrameResult;
    }

    /// <summary>
    /// Draws $93:834D's bomb-slot subset in descending physical-slot order.
    /// </summary>
    public void Draw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusBombProjectileSlot slot = _slots[slotIndex];
            // Type $0300 power bombs and type $0500 normal bombs both use the same bank-$93
            // timed-spritemap interpreter. The large power-bomb flash itself is an HDMA
            // color-math window and is composed separately from OAM.
            ushort family = (ushort)(slot.Type & 0x0f00);
            if (slot.InstructionPointer == 0 || (family != PowerBombType && family != NormalBombType))
                continue;

            // $93:837F admits X in [-48,304). Y is admitted only when the high byte of
            // the wrapped room-relative word is zero, i.e. [0,255]. OAM itself clips the
            // bottom 32 scanlines outside this runtime's 224-line rendered viewport.
            short screenX = unchecked((short)(slot.XPosition - layer1X));
            ushort screenY = unchecked((ushort)(slot.YPosition - layer1Y));
            if (screenX < -48 || screenX >= 304 || (screenY & 0xff00) != 0)
                continue;

            oam.AddProjectileSpritemap(
                bus,
                slot.SpritemapPointer,
                unchecked((ushort)screenX),
                screenY);
        }
    }

    /// <summary>Clears the five bomb slots and their two aggregate counters.</summary>
    public void Reset()
    {
        foreach (SamusBombProjectileSlot slot in _slots)
            slot.ClearFields();
        BombCounter = 0;
        CooldownTimer = 0;
        PowerBombExplosion.Reset();
        LastFrameResult = default;
    }

    private void StepCooldown()
    {
        if (CooldownTimer == 0)
            return;

        // $90:AC1C also clamps already-negative/crossed-negative values to zero. Normal
        // bomb cooldowns are small positive words, but retaining the signed edge keeps a
        // debugger mutation from producing a permanently locked firing state.
        if ((CooldownTimer & 0x8000) != 0)
        {
            CooldownTimer = 0;
            return;
        }

        CooldownTimer = unchecked((ushort)(CooldownTimer - 1));
        if ((CooldownTimer & 0x8000) != 0)
            CooldownTimer = 0;
    }

    private int? TryPlaceBomb(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput)
    {
        // The outer $90:BF9D test uses held Shoot. Both item branches eventually call
        // helper two, which separately insists on the newly-pressed bit.
        const ushort shoot = (ushort)SnesButton.X;
        if ((controllerInput & shoot) == 0)
            return null;

        bool placingPowerBomb = samus.SelectedHudItem == 3;
        if (placingPowerBomb && PowerBombExplosion.IsArmed)
            return null;

        // Normal bombs require the Bomb item. The selected-power-bomb branch in the ROM
        // deliberately bypasses this equipment check and calls helper two directly.
        if (!placingPowerBomb && !samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            return null;

        if (!TryReserveBombSlot(controllerNewInput, shoot))
            return null;

        // Retail HUD selection cannot normally point at an empty ammo class. Preserve the
        // native ordering nonetheless: helper two has already incremented the aggregate
        // counter if a debugger forces selected item three with zero power bombs.
        if (placingPowerBomb && samus.PowerBombs == 0)
            return null;

        if (placingPowerBomb)
        {
            samus.PowerBombs = unchecked((ushort)(samus.PowerBombs - 1));
            PowerBombExplosion.Arm();
        }

        int slotIndex = FindFreeBombSlot();
        SamusBombProjectileSlot slot = _slots[slotIndex];
        slot.ClearFields();
        slot.Type = placingPowerBomb ? PowerBombType : NormalBombType;
        slot.Direction = 0;
        slot.XPosition = samus.XPosition;
        slot.YPosition = samus.YPosition;
        slot.BombTimer = InitialBombTimer;
        InitializeBombFromRom(bus, slot);
        CooldownTimer = placingPowerBomb ? PowerBombCooldown : NormalBombCooldown;

        if (placingPowerBomb)
        {
            // The auto-cancel flag always wins. Otherwise consuming the last round clears
            // item index three so the next Shoot edge returns to the beam/normal-bomb path.
            if (samus.AutoCancelHudItemIndex != 0)
            {
                samus.SelectedHudItem = 0;
                samus.AutoCancelHudItemIndex = 0;
            }
            else if (samus.PowerBombs == 0)
            {
                samus.SelectedHudItem = 0;
            }
        }

        return slotIndex;
    }

    private bool TryReserveBombSlot(ushort controllerNewInput, ushort shoot)
    {
        if ((controllerNewInput & shoot) == 0)
            return false;

        // $90:C0E7 allows the first bomb regardless of cooldown. With any active bomb,
        // five slots or a nonzero LOW cooldown byte reject the edge. The high byte is not
        // part of this test; that oddity is intentional 65C816 SEP-width behavior.
        if (BombCounter != 0 &&
            (BombCounter >= SlotCount || (CooldownTimer & 0x00ff) != 0))
        {
            return false;
        }

        // Helper two increments both values before the caller searches for storage. The
        // caller immediately replaces cooldown with table entry five ($10) after init.
        CooldownTimer = unchecked((ushort)(CooldownTimer + 1));
        BombCounter = unchecked((ushort)(BombCounter + 1));

        return true;
    }

    private int FindFreeBombSlot()
    {
        int slotIndex = 0;
        while (_slots[slotIndex].Type != 0)
        {
            slotIndex++;
            if (slotIndex >= SlotCount)
            {
                // This is the assembly's defensive/corrupt-state fallback: decrement X
                // once and reuse the last slot. Consistent BombCounter state never needs it.
                slotIndex = SlotCount - 1;
                break;
            }
        }
        return slotIndex;
    }

    private static void InitializeBombFromRom(
        ISnesAddressSpace bus,
        SamusBombProjectileSlot slot)
    {
        // $93:80A6 reads the HIGH byte of type, masks its low nibble, and indexes the
        // non-beam pointer table. Type $0500 therefore selects word five -> $8675.
        int projectileTypeIndex = (slot.Type >> 8) & 0x000f;
        ushort dataPointer = ReadWord(
            bus,
            AddWithinBank(NonBeamProjectileDataPointerTable, projectileTypeIndex * 2));
        int dataAddress = 0x930000 | dataPointer;

        slot.Damage = ReadWord(bus, dataAddress);
        if ((slot.Damage & 0x8000) != 0)
            throw new InvalidDataException($"Bomb data at $93:{dataPointer:X4} has crash-marker damage ${slot.Damage:X4}.");

        slot.InstructionPointer = ReadWord(bus, AddWithinBank(dataAddress, 2));
        slot.InstructionTimer = 1;
    }

    private bool RunBombPreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> blockReactions,
        RoomPlmSystem? roomPlms,
        byte areaIndex)
    {
        // Direction high nibble is a generic projectile kill request. Normal placed bombs
        // leave direction zero for their lifetime, but debugger state can exercise it.
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return false;
        }

        ushort typeFamily = (ushort)(slot.Type & 0x0f00);
        if (typeFamily != NormalBombType && typeFamily != PowerBombType)
        {
            throw new NotSupportedException(
                $"Bomb slot {slot.Index} has untranslated projectile family ${typeFamily:X4}.");
        }

        bool explosionStarted = false;
        if (slot.BombTimer != 0)
        {
            slot.BombTimer = unchecked((ushort)(slot.BombTimer - 1));
            if (slot.BombTimer == 15)
            {
                // The slow and fast lists have identical four-frame layouts and differ by
                // $1C bytes. Adding to the live next-instruction pointer preserves phase.
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 0x001c));
            }
            else if (slot.BombTimer == 0)
            {
                if (typeFamily == NormalBombType)
                {
                    // $93:814E reads the pointer word embedded in the bomb-explosion data
                    // record at $93:8683 and resets the instruction timer to one.
                    slot.InstructionPointer = ReadWord(bus, BombExplosionInstructionPointerAddress);
                    slot.InstructionTimer = 1;
                }
                else
                {
                    // $90:C157 copies the projectile center to the global HDMA owner and
                    // leaves the bank-$93 power-bomb animation on its fast looping list.
                    PowerBombExplosion.Spawn(slot.XPosition, slot.YPosition);
                    slot.BombTimer = 0xffff;
                }
                explosionStarted = true;
            }
        }

        if (typeFamily == NormalBombType && slot.BombTimer == 0 && (slot.Type & 0x0001) == 0)
        {
            // Normal bomb type five maps through $94:9C73 to collision mode two. As soon
            // as timer zero is visible, $94:9CF4 sets type bit zero and reacts to a
            // five-block cross exactly once.
            slot.Type |= 0x0001;
            CollectBlockExplosionReactions(level, slot, blockReactions, roomPlms, areaIndex);
        }
        else if (typeFamily == PowerBombType)
        {
            // Collision mode three first turns the fuse-expiration sentinel $FFFF into
            // zero without touching terrain. Every later frame scans the newly reached
            // rectangle border using the high bytes of bank-$88's shared radii.
            if ((slot.BombTimer & 0x8000) != 0)
            {
                slot.BombTimer = 0;
            }
            else if (slot.BombTimer == 0 && PowerBombExplosion.IsArmed)
            {
                CollectPowerBombBoundaryReactions(
                    level,
                    blockReactions,
                    roomPlms,
                    areaIndex,
                    slot.Type);
            }

            // Once cleanup clears $0CEA, $90:C157 deletes the otherwise immortal looping
            // projectile. Crystal Flash deliberately retains the flag and slot instead.
            if (slot.BombTimer == 0 && !PowerBombExplosion.IsArmed)
                ClearProjectile(slot);
        }

        return explosionStarted;
    }

    private void CollectPowerBombBoundaryReactions(
        RoomLevelData level,
        List<BombBlockReaction> reactions,
        RoomPlmSystem? roomPlms,
        byte areaIndex,
        ushort projectileType)
    {
        int horizontalRadius = PowerBombExplosion.ExplosionRadius >> 8;
        int verticalRadius = (3 * horizontalRadius) >> 2;

        int left = Math.Max(0, PowerBombExplosion.XPosition - horizontalRadius) >> 4;
        int right = Math.Min(
            level.WidthInBlocks - 1,
            (PowerBombExplosion.XPosition + horizontalRadius) >> 4);
        int top = Math.Max(0, PowerBombExplosion.YPosition - verticalRadius) >> 4;
        int bottom = Math.Min(
            level.HeightInBlocks - 1,
            (PowerBombExplosion.YPosition + verticalRadius) >> 4);

        // $94:A0F4/$A11A visit all four inclusive edges in this exact order. Corners are
        // intentionally visited twice; synchronous PLM terrain mutation means the second
        // visit can observe a different collision type than the first.
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, x, top, reactions, roomPlms, areaIndex, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, left, y, reactions, roomPlms, areaIndex, projectileType);
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, x, bottom, reactions, roomPlms, areaIndex, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, right, y, reactions, roomPlms, areaIndex, projectileType);
    }

    private static void CollectBlockExplosionReactions(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> reactions,
        RoomPlmSystem? roomPlms,
        byte areaIndex)
    {
        int centerX = slot.XPosition >> 4;
        int centerY = slot.YPosition >> 4;
        (int X, int Y)[] cross =
        [
            (centerX, centerY),
            (centerX, centerY - 1),
            (centerX + 1, centerY),
            (centerX - 1, centerY),
            (centerX, centerY + 1),
        ];

        foreach ((int x, int y) in cross)
        {
            if ((uint)x >= (uint)level.WidthInBlocks || (uint)y >= (uint)level.HeightInBlocks)
            {
                throw new NotSupportedException(
                    $"Bomb explosion cross reaches outside translated room storage at block ({x},{y}).");
            }

            CollectSingleBombedBlockReaction(
                level,
                x,
                y,
                reactions,
                roomPlms,
                areaIndex,
                slot.Type);
        }
    }

    private static void CollectSingleBombedBlockReaction(
        RoomLevelData level,
        int x,
        int y,
        List<BombBlockReaction> reactions,
        RoomPlmSystem? roomPlms,
        byte areaIndex,
        ushort projectileType)
    {
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(x, y);
        reactions.Add(new BombBlockReaction(
            x,
            y,
            visitedBlock.CollisionType,
            visitedBlock.Behavior));

        // `$94:9411/$9447` do not react to an extension block directly. A nonzero signed
        // BTS redirects CurrentBlockIndex horizontally (type $5) or by whole room rows
        // (type $D), then rewinds the dispatcher return address so the resolved parent
        // is dispatched again. A zero-BTS extension is simply air for this reaction.
        RoomCollisionBlock block = visitedBlock;
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return;

        // Item collision BTS $45 routes to the already-loaded item object rather than
        // indexing the ordinary bomb/special-block tables. Visible type-$B frames need no
        // additional response; concealed type-$C/orb frames publish the generic trigger.
        if (block.Behavior == 0x45 && block.CollisionType is 11 or 12)
        {
            _ = roomPlms?.TryNotifyCollectibleProjectileHit(block.Index, projectileType);
            return;
        }

        // $94:A052 dispatches these types to immediate clear/set-carry routines. They
        // spawn no PLM and do not alter the level/BTS arrays, so recording the visit is
        // the complete observable effect for this runtime.
        if (block.CollisionType is 0 or 1 or 2 or 3 or 6 or 8 or 9 or 10 or 14)
            return;

        if (block.CollisionType is 7 or 15)
        {
            // Both bombable-air and bombable-solid handlers use `$94:A012`. Negative
            // BTS takes the native duplicate/area-dependent early return and therefore
            // neither allocates a PLM nor mutates terrain.
            if ((block.Behavior & 0x80) != 0)
                return;
            if (block.Behavior > 15)
            {
                throw new NotSupportedException(
                    $"Bombable block {block.Index} has BTS ${block.Behavior:X2} outside " +
                    "the native $94:A012 reaction table.");
            }
            if (roomPlms is null)
            {
                throw new NotSupportedException(
                    $"Bombed block reaction type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            // Spawn is synchronous: accepted BTS 0..7 applies CEDA's temporary type-$8
            // or type-$0 word before the caller proceeds to its next border/cross member.
            roomPlms.TrySpawnBombReactionBlock(
                level,
                block.Index,
                block.Behavior,
                projectileType);
            return;
        }

        if (block.CollisionType is 4 or 12)
        {
            // Type-$4 shootable air treats negative BTS as a duplicate and returns.
            // Type-$C instead indexes one of eight area tables; every retail entry is
            // PLMEntries_nothing, but Spawn_PLM still consumes a slot for one pass.
            if (block.CollisionType == 4 && (block.Behavior & 0x80) != 0)
                return;
            if ((block.Behavior & 0x80) == 0 && block.Behavior > 15)
            {
                throw new NotSupportedException(
                    $"Shootable block {block.Index} has BTS ${block.Behavior:X2} outside " +
                    "the translated normal-bomb table range.");
            }
            if ((block.Behavior & 0x80) != 0 && (block.Behavior & 0x7f) > 7)
            {
                throw new NotSupportedException(
                    $"Area-dependent shootable block {block.Index} has BTS " +
                    $"${block.Behavior:X2} outside its eight-entry native table.");
            }
            if (roomPlms is null)
            {
                throw new NotSupportedException(
                    $"Bombed shootable type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            roomPlms.TrySpawnBombedShootableBlock(
                level,
                block.Index,
                block.Behavior,
                projectileType);
            return;
        }

        if (block.CollisionType == 11)
        {
            if (roomPlms is null)
            {
                throw new NotSupportedException(
                    $"Bombed special block BTS ${block.Behavior:X2} at ({x},{y}) " +
                    "requires a room PLM owner.");
            }

            roomPlms.TrySpawnBombedSpecialBlock(
                level,
                block.Index,
                block.Behavior,
                areaIndex,
                projectileType);
            return;
        }

        throw new NotSupportedException(
            $"Bombed block reaction type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
            $"at ({x},{y}) requires the untranslated bank-$84 PLM pipeline.");
    }

    private bool RunProjectileInstructionHandler(
        ISnesAddressSpace bus,
        SamusBombProjectileSlot slot)
    {
        // $93:81F2 is a wrapping 16-bit DEC. A legitimate initialized timer is always at
        // least one, but preserving wrap gives corrupt-state inspection the same behavior.
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (slot.InstructionTimer != 0)
            return false;

        ushort pointer = slot.InstructionPointer;
        for (int operationCount = 0; operationCount < 16; operationCount++)
        {
            ushort durationOrOpcode = ReadWord(bus, 0x930000 | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                    throw new InvalidDataException($"Projectile instruction at $93:{pointer:X4} has zero duration.");

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(bus, 0x930000 | unchecked((ushort)(pointer + 2)));
                slot.XRadius = bus.ReadByte(0x930000 | unchecked((ushort)(pointer + 4)));
                slot.YRadius = bus.ReadByte(0x930000 | unchecked((ushort)(pointer + 5)));
                slot.InstructionPointer = unchecked((ushort)(pointer + 8));
                return false;
            }

            switch (durationOrOpcode)
            {
                case ProjectileInstructionDelete:
                    ClearProjectile(slot);
                    return true;

                case ProjectileInstructionGoto:
                    // The handler increments Y past the opcode before the instruction
                    // routine reads [[Y]]. Its target is another bank-$93 16-bit pointer.
                    pointer = ReadWord(bus, 0x930000 | unchecked((ushort)(pointer + 2)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Bomb projectile opcode $93:{durationOrOpcode:X4} is not translated.");
            }
        }

        throw new InvalidDataException("Bomb projectile instruction list did not reach a timed frame within 16 operations.");
    }

    private byte PublishBombJumpOverlap(SamusState samus)
    {
        byte publishedDirection = 0;

        // $A0:97BF scans ascending projectile indices. Only damage-bearing, unreflected,
        // pre-explosion projectile types below $0700 reach the geometric test.
        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            SamusBombProjectileSlot slot = _slots[slotIndex];
            if (slot.Damage == 0 ||
                (slot.Type & 0x8000) != 0 ||
                (slot.Type & 0x0f00) >= 0x0700 ||
                (slot.Direction & 0x0010) != 0)
            {
                continue;
            }

            int xDistance = Math.Abs((short)(slot.XPosition - samus.XPosition));
            int yDistance = Math.Abs((short)(slot.YPosition - samus.YPosition));
            if (xDistance >= slot.XRadius + samus.Kinematics.XRadius ||
                yDistance >= slot.YRadius + samus.Kinematics.YRadius)
            {
                continue;
            }

            ushort typeFamily = (ushort)(slot.Type & 0xff00);
            if ((typeFamily != 0x0300 && typeFamily != 0x0500) || slot.BombTimer != 8)
                continue;

            // CMP SamusX,bombX: equal is straight; Samus left of the bomb launches left;
            // Samus right of the bomb launches right. Later overlapping slots overwrite
            // earlier ones exactly as the ascending assembly loop does.
            publishedDirection = samus.XPosition == slot.XPosition
                ? (byte)2
                : unchecked((short)(samus.XPosition - slot.XPosition)) < 0
                    ? (byte)1
                    : (byte)3;
            samus.PublishBombJumpDirection(publishedDirection);
        }

        return publishedDirection;
    }

    private void ClearProjectile(SamusBombProjectileSlot slot)
    {
        bool wasActive = slot.IsActive;
        slot.ClearFields();
        if (!wasActive)
            return;

        // ClearProjectile decrements the aggregate for byte indices >= $0A and clamps a
        // signed underflow to zero. Consistent state cannot underflow, but the clamp is real.
        BombCounter = unchecked((ushort)(BombCounter - 1));
        if ((BombCounter & 0x8000) != 0)
            BombCounter = 0;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

}

/// <summary>One semantic view over a physical WRAM bomb slot.</summary>
public sealed class SamusBombProjectileSlot
{
    internal SamusBombProjectileSlot(int index) => Index = index;

    /// <summary>Logical bomb index zero through four; physical projectile index is 5+Index.</summary>
    public int Index { get; }

    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort Direction { get; internal set; }
    public ushort Type { get; internal set; }
    public ushort Damage { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }

    /// <summary>
    /// The word called <c>projectile_variables[5+Index]</c> by bank $90 and
    /// <c>bomb_timers[Index]</c> by bank $A0 due to the original overlapping arrays.
    /// </summary>
    public ushort BombTimer { get; internal set; }

    /// <summary>Instruction pointer is the native active-slot sentinel.</summary>
    public bool IsActive => InstructionPointer != 0;

    /// <summary>
    /// True after a normal bomb selects `$93:A06B`, or while a Power Bomb's timer-zero
    /// slot owns the expanding bank-$88 terrain scan, and before native-style deletion.
    /// </summary>
    public bool IsExploding => IsActive && BombTimer == 0;

    internal void ClearFields()
    {
        XPosition = 0;
        YPosition = 0;
        Direction = 0;
        Type = 0;
        Damage = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
        XRadius = 0;
        YRadius = 0;
        BombTimer = 0;
    }
}

/// <summary>One frame's debugger-visible bomb lifecycle transitions.</summary>
public readonly record struct BombProjectileFrameResult(
    int? PlacedSlot,
    bool ExplosionStarted,
    bool ProjectileDeleted,
    byte PublishedBombJumpDirection,
    IReadOnlyList<BombBlockReaction>? BlockReactions);

/// <summary>
/// One block visited by `$94:9CF4`'s normal-bomb cross or `$94:9D68`'s Power Bomb border.
/// </summary>
public readonly record struct BombBlockReaction(
    int BlockX,
    int BlockY,
    byte CollisionType,
    byte Behavior);
