using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Audio;
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
    public const int SlotCount = SamusBombSpreadRomData.SlotCount;

    /// <summary>Normal-bomb projectile type written by $90:BF9D.</summary>
    public const ushort NormalBombType = SamusBombSpreadRomData.NormalBombType;

    /// <summary>Power-bomb projectile type formed from HUD item index three.</summary>
    public const ushort PowerBombType = SamusBombSpreadRomData.PowerBombType;

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
        RoomPlmSystem? roomPlms = null,
        bool deferSamusOverlap = false)
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
        // The forward-facing branch at $90:DCE3/$90:DCE8 skips this call as well
        // as the HUD producer, but must still run the existing projectile slots below.
        if (!SamusState.IsForwardFacingPose(samus.Pose))
            StepCooldown();

        int? placedSlot = null;
        bool bombSpreadStarted = false;
        bool beamChargeConsumed = false;
        var soundRequests = new List<SamusSoundRequest>();
        if (SamusState.IsStableBallPose(samus.Pose))
        {
            BombSpreadAdmission spread = HandleBombSpreadInput(bus, samus, controllerInput);
            bombSpreadStarted = spread == BombSpreadAdmission.Spawned;
            beamChargeConsumed = spread is
                BombSpreadAdmission.Spawned or BombSpreadAdmission.ChargeCancelled;
            if (spread == BombSpreadAdmission.NotApplicable)
            {
                placedSlot = TryPlaceBomb(
                    bus,
                    samus,
                    controllerInput,
                    controllerNewInput,
                    out bool rejectedPlacementClearedCharge);
                beamChargeConsumed |= rejectedPlacementClearedCharge;
            }
            if (beamChargeConsumed)
            {
                soundRequests.Add(new SamusSoundRequest(
                    SoundEffectLibrary1Sounds.CancelAll,
                    MaximumQueued: 9));
            }
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

            bool wasNormalBomb = slot.PackedType.Family == SamusProjectileFamily.Bomb;
            bool slotExplosionStarted = RunBombPreInstruction(
                bus,
                level,
                slot,
                blockReactions,
                roomPlms,
                samus.LiquidPhysics.AreaIndex,
                samus.Kinematics);
            explosionStarted |= slotExplosionStarted;
            if (slotExplosionStarted && wasNormalBomb)
            {
                // `$90:C128` publishes this exact Max6 request for every normal bomb
                // whose fuse expires. Keep the append-only shape because several spread
                // bombs can expire during one Samus projectile pass.
                soundRequests.Add(new SamusSoundRequest(
                    SoundEffectLibrary2Sounds.BombExplosion,
                    MaximumQueued: 6));
            }
            else if (slotExplosionStarted)
            {
                // `$88:8AA9-$8AAC` queues library-one sound one through the ordinary
                // Max15 entry immediately before setting explosion status `$8000` and
                // allocating the two HDMA objects. Publish it from the same frame that
                // Spawn installs the semantic bank-$88 owner.
                soundRequests.Add(new SamusSoundRequest(
                    SoundEffectLibrary1Sounds.PowerBombExplosion,
                    MaximumQueued: 15));
            }

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
        // same frame's hit-interruption phase can arm the bomb movement handler.
        // Runtime defers this until the humanoid shot producer has also run: a
        // shot this frame may suppress overlap before the bomb direction is stored.
        byte publishedDirection = deferSamusOverlap ? (byte)0 : PublishBombJumpOverlap(samus);

        LastFrameResult = new BombProjectileFrameResult(
            placedSlot,
            explosionStarted,
            projectileDeleted,
            publishedDirection,
            blockReactions.ToArray(),
            bombSpreadStarted,
            beamChargeConsumed,
            soundRequests.ToArray());
        return LastFrameResult;
    }

    /// <summary>Runs the bomb subset of the post-alpha Samus/projectile interaction pass.</summary>
    public void ResolveSamusOverlap(SamusState samus, ushort projectileInvincibilityTimer)
    {
        ArgumentNullException.ThrowIfNull(samus);
        byte direction = projectileInvincibilityTimer == 0 && samus.HorizontalSpeed.ContactDamageIndex == 0
            ? PublishBombJumpOverlap(samus) : (byte)0;
        LastFrameResult = LastFrameResult with { PublishedBombJumpDirection = direction };
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
            SamusProjectileFamily family = slot.PackedType.Family;
            if (slot.InstructionPointer == 0 ||
                family is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
                continue;

            // `$93:835F-$8364` suppresses family `$0300` as soon as its shared
            // projectile-variable word reaches zero. `$90:C157` publishes that zero on
            // the detonation frame, so the small placed-bomb OBJ must disappear exactly
            // when bank $88 begins the large HDMA explosion. Normal bomb explosions keep
            // drawing their bank-$93 spritemaps with a zero BombTimer and are unaffected.
            if (family == SamusProjectileFamily.PowerBomb && slot.BombTimer == 0)
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

    internal int? TryPlaceBomb(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        out bool rejectedPlacementClearedCharge)
    {
        rejectedPlacementClearedCharge = false;
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
        {
            // Helper two also owns charge cancellation when it rejects placement
            // (no new Shoot edge, active-slot limit or cooldown). Merely returning
            // no bomb leaves a carried partial charge alive beyond its native lifetime.
            // Earlier equipment/power-bomb guards do not execute helper two and must
            // preserve charge. Publish through the existing runtime palette/SFX bridge.
            rejectedPlacementClearedCharge = samus.ProjectileFlareCounter != 0;
            return null;
        }

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
        slot.BombTimer = SamusBombSpreadRomData.InitialBombTimer;
        InitializeBombFromRom(bus, slot);
        CooldownTimer = placingPowerBomb
            ? SamusBombSpreadRomData.PowerBombCooldown
            : SamusBombSpreadRomData.NormalBombCooldown;

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

    private BombSpreadAdmission HandleBombSpreadInput(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput)
    {
        // The power-bomb HUD branch bypasses `$90:C0AB` entirely.
        if (samus.SelectedHudItem == 3)
            return BombSpreadAdmission.NotApplicable;

        bool shootHeld = (controllerInput & (ushort)SnesButton.X) != 0;
        if (!shootHeld)
        {
            // `$90:BF75` cancels a carried humanoid charge when the Morph-Ball handler
            // observes Shoot released. The palette/SFX bridge is consumed by runtime.
            return samus.ProjectileFlareCounter != 0
                ? BombSpreadAdmission.ChargeCancelled
                : BombSpreadAdmission.NotApplicable;
        }

        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs) ||
            samus.ProjectileFlareCounter < SamusBombSpreadRomData.RequiredChargeFrames ||
            BombCounter != 0)
        {
            return BombSpreadAdmission.NotApplicable;
        }

        if ((controllerInput & (ushort)SnesButton.Down) != 0 &&
            (samus.BombSpreadChargeTimeoutCounter &
                SamusBombSpreadRomData.DownChargeTimeoutMask) <
                SamusBombSpreadRomData.DownChargeTimeoutMask)
        {
            samus.BombSpreadChargeTimeoutCounter =
                unchecked((ushort)(samus.BombSpreadChargeTimeoutCounter + 1));
            return BombSpreadAdmission.Charging;
        }

        SpawnBombSpread(bus, samus);
        return BombSpreadAdmission.Spawned;
    }

    private void SpawnBombSpread(ISnesAddressSpace bus, SamusState samus)
    {
        int verticalModifier =
            (samus.BombSpreadChargeTimeoutCounter >> 6) & 0x0003;
        for (int index = 0; index < SlotCount; index++)
        {
            SamusBombProjectileSlot slot = _slots[index];
            slot.ClearFields();
            slot.Type = SamusBombSpreadRomData.BombSpreadType;
            slot.IsBombSpread = true;
            slot.XPosition = samus.XPosition;
            slot.YPosition = samus.YPosition;
            InitializeBombFromRom(bus, slot);

            int tableOffset = index * 2;
            slot.BombTimer = ReadWord(
                bus,
                SamusBombSpreadRomData.FuseTimers + tableOffset);
            slot.BombSpreadXVelocity = ReadWord(
                bus,
                SamusBombSpreadRomData.XVelocities + tableOffset);
            slot.BombSpreadInitialYSubvelocity = ReadWord(
                bus,
                SamusBombSpreadRomData.YSubspeeds + tableOffset);
            slot.BombSpreadYSubvelocity = slot.BombSpreadInitialYSubvelocity;
            ushort wholeYSpeed = unchecked((ushort)(ReadWord(
                bus,
                SamusBombSpreadRomData.YSpeeds + tableOffset) + verticalModifier));
            slot.BombSpreadYVelocity = unchecked((ushort)(0 - wholeYSpeed));
            slot.BombSpreadBounceYVelocity = slot.BombSpreadYVelocity;
        }

        BombCounter = SlotCount;
        CooldownTimer = SamusBombSpreadRomData.NormalBombCooldown;
        samus.BombSpreadChargeTimeoutCounter = 0;
        samus.ProjectileFlareCounter = 0;
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
            AddWithinBank(SamusProjectileRomData.NonBeam.DataPointers, projectileTypeIndex * 2));
        int dataAddress = SamusProjectileRomData.Banks.Projectile | dataPointer;

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
        AreaId areaIndex,
        SamusKinematicsState samusKinematics)
    {
        // Direction high nibble is a generic projectile kill request. Normal placed bombs
        // leave direction zero for their lifetime, but debugger state can exercise it.
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return false;
        }

        SamusProjectileFamily typeFamily = slot.PackedType.Family;
        if (typeFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            // This class owns only physical projectile slots five through nine after
            // $90:BF9D/$C157 have selected normal- or Power-Bomb pre-instructions. No
            // retail producer installs another family in these semantic slots; seeing
            // one means the host-side slot model was mutated inconsistently.
            throw new InvalidDataException(
                $"Bomb slot {slot.Index} has invalid projectile family ${(ushort)typeFamily:X4}.");
        }

        bool explosionStarted = false;
        if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            PowerBombFuseStep fuse = SamusPowerBombFuse.Step(
                slot.BombTimer, slot.InstructionPointer, PowerBombExplosion.Flag);
            slot.BombTimer = fuse.Timer;
            slot.InstructionPointer = fuse.InstructionPointer;
            if (fuse.DeleteProjectile)
            {
                ClearProjectile(slot);
                return false;
            }
            if (fuse.SpawnExplosion)
            {
                PowerBombExplosion.Spawn(slot.XPosition, slot.YPosition);
                explosionStarted = true;
            }
        }
        else if (slot.BombTimer != 0)
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
                // $93:814E reads the pointer word embedded in the bomb-explosion data
                // record at $93:8683 and resets the instruction timer to one.
                slot.InstructionPointer = ReadWord(
                    bus,
                    SamusProjectileRomData.NonBeam.BombExplosionInstructionPointer);
                slot.InstructionTimer = 1;
                explosionStarted = true;
            }
        }

        if (slot.IsBombSpread && slot.BombTimer != 0)
            MoveBombSpread(bus, level, slot, samusKinematics);

        if (typeFamily == SamusProjectileFamily.Bomb &&
            slot.BombTimer == 0 &&
            (slot.Type & 0x0001) == 0)
        {
            // Normal bomb type five maps through $94:9C73 to collision mode two. As soon
            // as timer zero is visible, $94:9CF4 sets type bit zero and reacts to a
            // five-block cross exactly once.
            slot.Type |= 0x0001;
            CollectBlockExplosionReactions(level, slot, blockReactions, roomPlms, areaIndex);
        }
        else if (typeFamily == SamusProjectileFamily.PowerBomb)
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
                    PowerBombExplosion.XPosition,
                    PowerBombExplosion.YPosition,
                    PowerBombExplosion.ExplosionRadius,
                    blockReactions,
                    roomPlms,
                    areaIndex,
                    slot.Type);
            }

        }

        return explosionStarted;
    }

    private static void MoveBombSpread(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        SamusKinematicsState samus)
    {
        (slot.BombSpreadYVelocity, slot.BombSpreadYSubvelocity) = AddFixedWords(
            slot.BombSpreadYVelocity,
            slot.BombSpreadYSubvelocity,
            samus.YAcceleration,
            samus.YSubacceleration);

        ushort previousY = slot.YPosition;
        ushort previousYSub = slot.YSubposition;
        (slot.YPosition, slot.YSubposition) = AddFixedWords(
            slot.YPosition,
            slot.YSubposition,
            slot.BombSpreadYVelocity,
            slot.BombSpreadYSubvelocity);
        if (BombSpreadCollides(bus, level, slot))
        {
            slot.YPosition = previousY;
            slot.YSubposition = previousYSub;
            if ((slot.BombSpreadYVelocity & 0x8000) != 0)
            {
                slot.BombSpreadYVelocity = 0;
                slot.YRadius = 0;
            }
            else
            {
                slot.BombSpreadYSubvelocity = slot.BombSpreadInitialYSubvelocity;
                slot.BombSpreadYVelocity = slot.BombSpreadBounceYVelocity;
            }
            return;
        }

        ushort previousX = slot.XPosition;
        ushort previousXSub = slot.XSubposition;
        MoveBombSpreadHorizontally(slot);
        if (!BombSpreadCollides(bus, level, slot))
            return;

        slot.XPosition = previousX;
        slot.XSubposition = previousXSub;
        slot.BombSpreadXVelocity ^= 0x8000;
    }

    private static void MoveBombSpreadHorizontally(SamusBombProjectileSlot slot)
    {
        ushort swapped = unchecked((ushort)(
            (slot.BombSpreadXVelocity << 8) |
            (slot.BombSpreadXVelocity >> 8)));
        ushort subvelocity = unchecked((ushort)(swapped & 0xff00));
        ushort wholeMagnitude = unchecked((ushort)(swapped & 0x007f));
        bool movingLeft = (swapped & 0x0080) != 0;

        uint fraction = movingLeft
            ? unchecked((uint)slot.XSubposition - subvelocity)
            : (uint)slot.XSubposition + subvelocity;
        slot.XSubposition = unchecked((ushort)fraction);
        int carryOrBorrow = movingLeft
            ? (fraction > ushort.MaxValue ? 1 : 0)
            : (int)(fraction >> 16);
        slot.XPosition = movingLeft
            ? unchecked((ushort)(slot.XPosition - wholeMagnitude - carryOrBorrow))
            : unchecked((ushort)(slot.XPosition + wholeMagnitude + carryOrBorrow));
    }

    private static (ushort Whole, ushort Fraction) AddFixedWords(
        ushort whole,
        ushort fraction,
        ushort addWhole,
        ushort addFraction)
    {
        uint fractionalSum = (uint)fraction + addFraction;
        fraction = unchecked((ushort)fractionalSum);
        whole = unchecked((ushort)(whole + addWhole + (fractionalSum >> 16)));
        return (whole, fraction);
    }

    private static bool BombSpreadCollides(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusBombProjectileSlot slot)
    {
        int blockX = slot.XPosition >> 4;
        int blockY = slot.YPosition >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return true;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return false;

        if (block.CollisionType != RoomCollisionType.Slope)
        {
            return block.CollisionType is not (
                RoomCollisionType.Air or
                RoomCollisionType.SpikeAir or
                RoomCollisionType.SpecialAir or
                RoomCollisionType.UnusedAir or
                RoomCollisionType.BombableAir);
        }

        if (!block.Bts.IsNonSquareSlope)
            return true;

        // `$94:A58F` mirrors X before indexing the slope-height row, then compares the
        // resulting height with Y within the block. Equality counts as collision.
        int xWithinBlock = (block.Bts.SlopeFlipsHorizontally
            ? slot.XPosition ^ 0x000f
            : slot.XPosition) & 0x000f;
        int yWithinBlock = slot.YPosition & 0x000f;
        if (block.Bts.SlopeFlipsVertically)
            yWithinBlock ^= 0x000f;
        int height = bus.ReadByte(
            SamusBombSpreadRomData.NonSquareSlopeHeights +
            block.Bts.SlopeShape * 16 + xWithinBlock) &
            SamusBombSpreadRomData.NonSquareSlopeHeightMask;
        return height <= yWithinBlock;
    }

    /// <summary>
    /// $94:A06A boundary traversal, shared by every callback that dispatches to it.
    /// Coordinates and radius belong to the global explosion, not the calling slot.
    /// </summary>
    public static void CollectPowerBombBoundaryReactions(
        RoomLevelData level,
        ushort explosionX,
        ushort explosionY,
        ushort explosionRadius,
        List<BombBlockReaction> reactions,
        RoomPlmSystem? roomPlms,
        AreaId areaIndex,
        ushort projectileType)
    {
        int horizontalRadius = explosionRadius >> 8;
        int verticalRadius = (3 * horizontalRadius) >> 2;

        int left = Math.Max(0, explosionX - horizontalRadius) >> 4;
        int right = Math.Min(
            level.WidthInBlocks - 1,
            (explosionX + horizontalRadius) >> 4);
        int top = Math.Max(0, explosionY - verticalRadius) >> 4;
        int bottom = Math.Min(
            level.HeightInBlocks - 1,
            (explosionY + verticalRadius) >> 4);

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
        AreaId areaIndex)
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
                // $94:9CAC first admits the bomb centre through the room's scroll bounds,
                // and valid rooms surround reachable bomb centres with an edge wall. An
                // out-of-range cross member therefore identifies malformed room/caller
                // state rather than another bomb reaction routine.
                throw new InvalidDataException(
                    $"Bomb explosion cross reaches outside room storage at block ({x},{y}).");
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

    internal static void CollectSingleBombedBlockReaction(
        RoomLevelData level,
        int x,
        int y,
        List<BombBlockReaction> reactions,
        RoomPlmSystem? roomPlms,
        AreaId areaIndex,
        ushort projectileType)
    {
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(x, y);
        reactions.Add(new BombBlockReaction(
            x,
            y,
            visitedBlock.CollisionType,
            visitedBlock.Bts));

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
        if (block.Bts == RoomBlockBehaviorValues.CollectibleTrigger &&
            block.CollisionType is RoomCollisionType.SpecialBlock or RoomCollisionType.ShootableBlock)
        {
            _ = roomPlms?.TryNotifyCollectibleProjectileHit(block.Index, projectileType);
            return;
        }

        // `$94:9F2E` selects temporary PLM $C83E for both type-$8 special collision and
        // type-$C shootable collision with BTS $44. Setup $84:C7E2 deletes that temporary
        // slot, finds the resident actor at the same block, and publishes this projectile
        // word. The n00b tube begins as type $8 but its intact draw changes the live origin
        // to type $C, so narrowing this seam to colored doors strands the progression PLM.
        if (block.CollisionType is RoomCollisionType.SolidBlock or RoomCollisionType.ShootableBlock &&
            block.Bts == RoomBlockBehaviorValues.ResidentPlmProjectileTrigger)
        {
            if (roomPlms is null ||
                !roomPlms.TryNotifyResidentProjectileHit(block.Index, projectileType))
            {
                throw new InvalidOperationException(
                    $"Bombed resident-trigger block {block.Index} has no active PLM owner.");
            }
            return;
        }

        // `$94:9EA6[40..43]` routes the four ordinary blue-door cap orientations to
        // Setup_BlueDoor regardless of whether the collision came from the shot or bomb
        // dispatcher. Normal bombs use projectile family `$0500` and are accepted; Power
        // Bomb family `$0300` is rejected inside the shared bank-$84 setup. Treating these
        // private door BTS bytes as ordinary table indices both crashed on `$41` and lost
        // the cartridge behavior that allows a bomb placed beside a blue door to open it.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Bts.TryGetBlueDoorOrientation(out _))
        {
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed blue-door cap at ({x},{y}) requires a room PLM owner.");
            }
            roomPlms.TrySpawnBlueDoorOpening(
                level,
                block.Index,
                block.Bts,
                projectileType);
            return;
        }

        // $94:A052 dispatches these types to immediate clear/set-carry routines. They
        // spawn no PLM and do not alter the level/BTS arrays, so recording the visit is
        // the complete observable effect for this runtime.
        if (block.CollisionType is
            RoomCollisionType.Air or
            RoomCollisionType.Slope or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.UnusedAir or
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.GrappleBlock)
            return;

        if (block.CollisionType is RoomCollisionType.BombableAir or RoomCollisionType.BombableBlock)
        {
            // Both bombable-air and bombable-solid handlers use `$94:A012`. Negative
            // BTS takes the native duplicate/area-dependent early return and therefore
            // neither allocates a PLM nor mutates terrain.
            if (block.Bts.UsesAreaReactionTable)
                return;
            if (!block.Bts.IsNormalReactionIndex(16))
            {
                // $94:A012 is a literal sixteen-word table. Retail room data must keep a
                // nonnegative bombable BTS within that table; a larger value would make
                // the 65C816 read unrelated following ROM as a PLM header.
                throw new InvalidDataException(
                    $"Bombable block {block.Index} has BTS ${block.Behavior:X2} outside " +
                    "the native $94:A012 reaction table.");
            }
            if (roomPlms is null)
            {
                // Gameplay always owns the room's PLM pool. Null is supported only for
                // nonreactive synthetic fixtures, so reaching a producer without it is a
                // caller-composition error rather than untranslated cartridge behavior.
                throw new InvalidOperationException(
                    $"Bombed block reaction type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            // Spawn is synchronous: accepted BTS 0..7 applies CEDA's temporary type-$8
            // or type-$0 word before the caller proceeds to its next border/cross member.
            roomPlms.TrySpawnBombReactionBlock(
                level,
                block.Index,
                block.Bts,
                projectileType);
            return;
        }

        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            // Gate shot blocks reuse shootable collision with private BTS $46-$4D. They
            // select temporary trigger PLMs rather than the ordinary 0..F bomb table.
            if (block.CollisionType == RoomCollisionType.ShootableBlock &&
                block.Bts.TryGetDownwardGateTrigger(out _))
            {
                if (roomPlms is null)
                {
                    throw new InvalidOperationException(
                        $"Bombed downward gate trigger at ({x},{y}) requires a room PLM owner.");
                }
                roomPlms.TrySpawnDownwardGateTrigger(
                    level,
                    block.Index,
                    block.Bts,
                    projectileType);
                return;
            }

            // Type-$4 shootable air treats negative BTS as a duplicate and returns.
            // Type-$C instead indexes one of eight area tables; every retail entry is
            // PLMEntries_nothing, but Spawn_PLM still consumes a slot for one pass.
            if (block.CollisionType == RoomCollisionType.ShootableAir &&
                block.Bts.UsesAreaReactionTable)
                return;
            if (!block.Bts.UsesAreaReactionTable &&
                block.Bts.Value != EscapeAnimalPlmRomData.ReactionBts &&
                !block.Bts.IsNormalReactionIndex(16) &&
                !block.Bts.IsShootableCollisionProbe)
            {
                throw new InvalidDataException(
                    $"Shootable block {block.Index} has BTS ${block.Behavior:X2} outside " +
                    "the translated native bomb-reaction table range.");
            }
            if (block.Bts.UsesAreaReactionTable &&
                !block.Bts.IsAreaReactionIndex(8))
            {
                throw new InvalidDataException(
                    $"Area-dependent shootable block {block.Index} has BTS " +
                    $"${block.Behavior:X2} outside its eight-entry native table.");
            }
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed shootable type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            roomPlms.TrySpawnProjectileShotBlock(
                level,
                block.Index,
                block.Bts,
                projectileType,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
            return;
        }

        if (block.CollisionType == RoomCollisionType.SpecialBlock)
        {
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed special block BTS ${block.Behavior:X2} at ({x},{y}) " +
                    "requires a room PLM owner.");
            }

            roomPlms.TrySpawnBombedSpecialBlock(
                level,
                block.Index,
                block.Bts,
                areaIndex,
                projectileType);
            return;
        }

        // Collision types are a four-bit field and every value 0..15 is handled above;
        // type 5/D either resolves to its parent or returned before this dispatcher. Keep
        // the guard for corrupted packed words, but do not label it missing behavior.
        throw new InvalidDataException(
            $"Bombed block reaction reached impossible type ${block.CollisionType:X1}/" +
            $"BTS ${block.Behavior:X2} at ({x},{y}).");
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
            ushort durationOrOpcode = ReadWord(
                bus,
                SamusProjectileRomData.Banks.Projectile | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                    throw new InvalidDataException($"Projectile instruction at $93:{pointer:X4} has zero duration.");

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(
                    bus,
                    SamusProjectileRomData.Banks.Projectile |
                        unchecked((ushort)(pointer + 2)));
                slot.XRadius = bus.ReadByte(
                    (int)new SnesAddress(
                        SamusProjectileRomData.Banks.ProjectileNumber,
                        unchecked((ushort)(pointer + 4))));
                slot.YRadius = bus.ReadByte(
                    (int)new SnesAddress(
                        SamusProjectileRomData.Banks.ProjectileNumber,
                        unchecked((ushort)(pointer + 5))));
                slot.InstructionPointer = unchecked((ushort)(pointer + 8));
                return false;
            }

            switch (durationOrOpcode)
            {
                case SamusProjectileRomData.Instructions.Delete:
                    ClearProjectile(slot);
                    return true;

                case SamusProjectileRomData.Instructions.GoTo:
                    // The handler increments Y past the opcode before the instruction
                    // routine reads [[Y]]. Its target is another bank-$93 16-bit pointer.
                    pointer = ReadWord(
                        bus,
                        SamusProjectileRomData.Banks.Projectile |
                            unchecked((ushort)(pointer + 2)));
                    break;

                default:
                    // The complete referenced bank-$93 projectile interpreter exposes
                    // delete ($822F) and goto ($8239); its only third routine ($8240) is
                    // unreferenced data excluded from the retail build. A different word
                    // in an active bomb list is consequently corrupt ROM/list state.
                    throw new InvalidDataException(
                        $"Bomb projectile list contains invalid opcode $93:{durationOrOpcode:X4}.");
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
                slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.BeamExplosion ||
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

            SamusProjectileFamily typeFamily = slot.PackedType.Family;
            if (typeFamily is not (
                    SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb) ||
                slot.BombTimer != 8)
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
    /// <summary>Lossless semantic view of the native projectile family/type word.</summary>
    public SamusProjectileTypeWord PackedType => new(Type);
    public ushort Damage { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }

    /// <summary>Fractional X position at WRAM <c>$0B64+slot</c>.</summary>
    public ushort XSubposition { get; internal set; }

    /// <summary>Fractional Y position at WRAM <c>$0B78+slot</c>.</summary>
    public ushort YSubposition { get; internal set; }

    /// <summary>Whether bank-$90 installed <c>ProjectilePreInstruction_BombSpread</c>.</summary>
    public bool IsBombSpread { get; internal set; }

    /// <summary>Native direction/magnitude X-velocity word from $90:D8D9.</summary>
    public ushort BombSpreadXVelocity { get; internal set; }

    /// <summary>Whole signed Y velocity used by the spread pre-instruction.</summary>
    public ushort BombSpreadYVelocity { get; internal set; }

    /// <summary>Fractional Y velocity used by the spread pre-instruction.</summary>
    public ushort BombSpreadYSubvelocity { get; internal set; }

    /// <summary>ROM-authored fractional Y velocity restored on a floor bounce.</summary>
    public ushort BombSpreadInitialYSubvelocity { get; internal set; }

    /// <summary>Initial negative whole Y velocity restored on a floor bounce.</summary>
    public ushort BombSpreadBounceYVelocity { get; internal set; }

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
        XSubposition = 0;
        YSubposition = 0;
        IsBombSpread = false;
        BombSpreadXVelocity = 0;
        BombSpreadYVelocity = 0;
        BombSpreadYSubvelocity = 0;
        BombSpreadInitialYSubvelocity = 0;
        BombSpreadBounceYVelocity = 0;
    }
}

/// <summary>One frame's debugger-visible bomb lifecycle transitions.</summary>
public readonly record struct BombProjectileFrameResult(
    int? PlacedSlot,
    bool ExplosionStarted,
    bool ProjectileDeleted,
    byte PublishedBombJumpDirection,
    IReadOnlyList<BombBlockReaction>? BlockReactions,
    bool BombSpreadStarted = false,
    bool BeamChargeConsumed = false,
    IReadOnlyList<SamusSoundRequest>? SoundRequests = null);

internal enum BombSpreadAdmission
{
    NotApplicable,
    Charging,
    Spawned,
    ChargeCancelled,
}

/// <summary>
/// One block visited by `$94:9CF4`'s normal-bomb cross or `$94:9D68`'s Power Bomb border.
/// </summary>
public readonly record struct BombBlockReaction(
    int BlockX,
    int BlockY,
    RoomCollisionType CollisionType,
    RoomBlockBehavior Behavior);
