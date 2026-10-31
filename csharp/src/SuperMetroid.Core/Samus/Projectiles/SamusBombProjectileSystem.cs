using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The five bomb slots shared by bank-$90 projectile logic, bank-$93 instruction lists,
/// bank-$94 block reactions, and bank-$A0 Samus/projectile overlap handling.
/// </summary>
/// <remarks>
/// Slot order: Samus has <see cref="SlotCount"/> bomb slots, each with a fixed ordinal. A placed bomb takes the lowest
/// free ordinal, a bomb spread fills every slot in ordinal order, and the bomb update and drawing run in update order,
/// from the highest ordinal down. The bomb-jump scan alone runs in ordinal order, so the highest overlapping bomb decides
/// Samus's launch.
/// </remarks>
[RoomLifetime]
public sealed class SamusBombProjectileSystem
{
    /// <summary>Shared installed timed-frame sprite choices for the bomb half of the projectile pool.</summary>
    [field: NonSerialized]
    public SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings? FrameBindings { get; set; }
    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int SlotCount = MorphBallBombDefinitions.SlotCount;

    /// <summary>A placed Morph Ball bomb.</summary>
    public static SamusProjectileType NormalBombType => SamusProjectileType.Of(SamusProjectileFamily.Bomb);

    /// <summary>A placed Power Bomb.</summary>
    public static SamusProjectileType PowerBombType => SamusProjectileType.Of(SamusProjectileFamily.PowerBomb);

    /// <summary>The bomb slots by ordinal.</summary>
    private readonly SamusBombProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount).Select(index => new SamusBombProjectileSlot(index)).ToArray();

    /// <summary>The bomb slots by ordinal.</summary>
    public IReadOnlyList<SamusBombProjectileSlot> Slots => _slots;

    /// <summary>The bomb slots in update order, highest ordinal first.</summary>
    private IEnumerable<SamusBombProjectileSlot> InUpdateOrder
    {
        get
        {
            for (int ordinal = _slots.Length - 1; ordinal >= 0; ordinal--)
                yield return _slots[ordinal];
        }
    }

    /// <summary>Bank-$88 owner of the flash, damaging radius, and armed flag.</summary>
    public SamusPowerBombExplosionState PowerBombExplosion { get; } = new();

    /// <summary>How many bombs Samus has out by the placing routines' count; kept separately from the free-slot search.</summary>
    public int BombCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The cooldown shared by every Samus projectile. Its low byte gates another bomb while one is active.</summary>
    public WrappingCountdown CooldownTimer { get; private set; }

    /// <summary>
    /// Replaces the shared projectile cooldown from another Samus-projectile producer.
    /// </summary>
    /// <remarks>
    /// Beams, missiles, power bombs and normal bombs do not own separate cooldowns; they all read and write the same
    /// one. Ordinary projectiles live in a separate class, but that boundary must not create a second clock.
    /// </remarks>
    internal void SetSharedCooldown(int value) => CooldownTimer = WrappingCountdown.FromRaw(value);

    /// <summary>
    /// Replaces the bomb count from an enemy-owned Samus interaction. Shitroid writes
    /// five while it drains Samus to suppress projectile allocation, then clears the word
    /// at the terminal one-energy transition without disturbing the shared cooldown.
    /// </summary>
    internal void SetSharedBombCounter(int value) => BombCounter = value;

    /// <summary>Observable result of the most recent translated alpha/interaction pass.</summary>
    public BombProjectileFrameResult LastFrameResult { get; private set; }

    /// <summary>
    /// Sound guard after the per-line effect update but before bomb pre-instructions. Native
    /// humanoid weapon production precedes those instructions; the split projectile
    /// owners must retain this earlier value when a bomb activates later in alpha.
    /// </summary>
    public bool SoundSuppressedBeforeProjectileHandling { get; private set; }

    /// <summary>
    /// Advances the Power Bomb explosion's shape independently of Samus's alpha/input handler.
    /// Scripted carries suppress alpha but do not stop the per-line effect phase.
    /// </summary>
    public void AdvancePowerBombExplosion(
        SamusState samus,
        ControllerButton controllerInput,
        SuperMetroid.Core.Progression.SuperMetroidGameState gameState = SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay)
    {
        Ensure.NotNull(samus);
        // Each update runs the per-line display effects before gameplay reaches Samus's
        // frame handler. A power bomb spawned later by alpha consequently receives
        // its first radius update on the next frame, not on its fuse-expiration frame.
        bool crystalFlashWindowWasActive = PowerBombExplosion.Phase is
            PowerBombExplosionPhase.CrystalFlashExplosion or
            PowerBombExplosionPhase.CrystalFlashAfterglow;
        bool powerBombCleanup = PowerBombExplosion.StepFrame();
        if (powerBombCleanup && !crystalFlashWindowWasActive)
        {
            // $88:8B4E offers Crystal Flash only if Samus has remained on the exact bomb
            // origin. Failure (including moving one pixel) releases $0CEA immediately.
            bool crystalFlashStarted =
                samus.Position.X.Whole == PowerBombExplosion.Position.X &&
                samus.Position.Y.Whole == PowerBombExplosion.Position.Y &&
                samus.CrystalFlash.TryBegin(
                    samus,
                    controllerInput,
                    ControllerButton.X,
                    inputCheck: CrystalFlashInputCheck.Required);
            if (!crystalFlashStarted)
                PowerBombExplosion.ReleaseFlag();
        }
        // $88:8B78 runs Samus command $1E at the end of every cleanup, including the
        // Crystal Flash window's own.
        if (powerBombCleanup)
            ResumeSoundsAfterPowerBombExplosion(samus, gameState);

        SoundSuppressedBeforeProjectileHandling = PowerBombExplosion.IsActive;
    }

    /// <summary>
    /// Runs the bomb-owned portion of Samus frame-handler alpha, followed by the bank-$A0
    /// overlap pass that the main gameplay loop invokes before movement-handler beta.
    /// </summary>
    public BombProjectileFrameResult StepFrame(SamusTerrain terrain, SamusState samus, ControllerButton controllerInput, ControllerButton controllerNewInput, BombJumpOverlapPublication overlap = BombJumpOverlapPublication.Immediate, PowerBombExplosionStep explosion = PowerBombExplosionStep.Advance)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (explosion == PowerBombExplosionStep.Advance)
            AdvancePowerBombExplosion(samus, controllerInput);

        // $90:AC1C runs before the movement-type-specific HUD handler. A value of one
        // therefore reaches zero in time for a new Shoot edge during this same frame.
        // The forward-facing branch at $90:DCE3/$90:DCE8 skips this call as well
        // as the HUD producer, but must still run the existing projectile slots below.
        // Command-zero alpha calls HandleProjectile directly, skipping the HUD
        // dispatcher that owns cooldown and new bomb placement.
        if (samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose))
            StepCooldown(samus.Xray.TimeIsFrozen);

        bool beamChargeConsumed = false;
        var soundRequests = new List<SoundRequest>();
        if (samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked && SamusPoseMetadata.IsStableBallPose(samus.Pose))
        {
            BombSpreadAdmission spread = HandleBombSpreadInput(samus, controllerInput);
            beamChargeConsumed = spread is
                BombSpreadAdmission.Spawned or BombSpreadAdmission.ChargeCancelled;
            if (spread == BombSpreadAdmission.NotApplicable)
            {
                TryPlaceBomb(
                    samus,
                    controllerInput,
                    controllerNewInput,
                    out bool rejectedPlacementClearedCharge);
                beamChargeConsumed |= rejectedPlacementClearedCharge;
            }
            if (beamChargeConsumed)
            {
                soundRequests.Add(new SoundRequest(
                    Library1Sound.Silence,
                    Priority: SoundRequestPriority.High,
                    SoundSuppressed: PowerBombExplosion.IsActive));
            }
        }

        bool explosionStarted = false;
        bool projectileDeleted = false;
        var blockReactions = new List<BombBlockReaction>();

        // Bombs run in update order. X-ray may have frozen time in this same HUD pass: the cooldown above still runs, but
        // no projectile is handled after admission.
        foreach (SamusBombProjectileSlot slot in InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (slot.Cursor is null)
                continue;

            bool wasNormalBomb = slot.Type.Family == SamusProjectileFamily.Bomb;
            // Spawn queues its startup cue before setting the active bit. Preserve
            // that earlier guard separately from sounds produced after the pre-instruction.
            bool soundSuppressedBeforePreInstruction = PowerBombExplosion.IsActive;
            bool slotExplosionStarted = RunBombPreInstruction(
                terrain.Level,
                slot,
                blockReactions,
                terrain.Plms,
                samus.LiquidPhysics.AreaIndex,
                samus.Kinematics);
            explosionStarted |= slotExplosionStarted;
            if (slotExplosionStarted && wasNormalBomb)
            {
                // `$90:C128` requests this sound at normal priority for every normal bomb
                // whose fuse expires. Keep the append-only shape because several spread
                // bombs can expire during one Samus projectile pass.
                soundRequests.Add(new SoundRequest(
                    Library2Sound.BombExplosion,
                    Priority: SoundRequestPriority.Normal,
                    SoundSuppressed: PowerBombExplosion.IsActive));
            }
            else if (slotExplosionStarted)
            {
                // The explosion requests its sound at essential priority
                // immediately before it marks the explosion active and
                // starts its two per-line effects. Publish it from the same frame that
                // Spawn installs the explosion's owner.
                soundRequests.Add(new SoundRequest(
                    Library1Sound.PowerBombExplosion,
                    Priority: SoundRequestPriority.Essential,
                    SoundSuppressed: soundSuppressedBeforePreInstruction));
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

            projectileDeleted |= RunProjectileInstructionHandler(slot);
        }

        // GameState_8 invokes $A0:9785 after frame-handler alpha (which placed/updated the
        // bombs) and before beta moves Samus. Store only the low direction byte here. The
        // same frame's hit-interruption phase can arm the bomb movement handler.
        // Runtime defers this until the humanoid shot producer has also run: a
        // shot this frame may suppress overlap before the bomb direction is stored.
        SamusBombJumpDirection publishedDirection = overlap == BombJumpOverlapPublication.Deferred ? SamusBombJumpDirection.None : PublishBombJumpOverlap(samus);

        LastFrameResult = new BombProjectileFrameResult(
            publishedDirection,
            beamChargeConsumed,
            soundRequests.ToArray());
        return LastFrameResult;
    }

    /// <summary>Runs the bomb subset of the post-alpha Samus/projectile interaction pass.</summary>
    public void ResolveSamusOverlap(SamusState samus, int projectileInvincibilityTimer)
    {
        Ensure.NotNull(samus);
        SamusBombJumpDirection direction = projectileInvincibilityTimer == 0 && samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.None
            ? PublishBombJumpOverlap(samus) : SamusBombJumpDirection.None;
        LastFrameResult = LastFrameResult with { PublishedBombJumpDirection = direction };
    }

    /// <summary>
    /// Draws the placed bombs and Power Bombs, in update order.
    /// </summary>
    public void Draw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusBombProjectileSlot slot in InUpdateOrder)
        {
            // Type $0300 power bombs and type $0500 normal bombs both use the same bank-$93
            // timed-spritemap interpreter. The large power-bomb flash itself is a per-line
            // color-math window and is composed separately from the sprites.
            SamusProjectileFamily family = slot.Type.Family;
            if (slot.Cursor is null ||
                family is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
                continue;

            // `$93:835F-$8364` suppresses family `$0300` as soon as its shared
            // projectile-variable word reaches zero. `$90:C157` publishes that zero on
            // the detonation frame, so the small placed-bomb OBJ must disappear exactly
            // when the large explosion effect begins. Normal bomb explosions keep
            // drawing their bank-$93 spritemaps with a zero BombTimer and are unaffected.
            if (family == SamusProjectileFamily.PowerBomb && slot.BombTimer.IsZero)
                continue;

            // $93:837F admits X in [-48,304). Y is admitted only when the high byte of
            // the wrapped room-relative word is zero, i.e. [0,255]. OAM itself clips the
            // bottom 32 scanlines outside this runtime's 224-line rendered viewport.
            int screenX = unchecked((short)(slot.Position.X.Whole - layer1.X));
            int screenY = unchecked((ushort)(slot.Position.Y.Whole - layer1.Y));
            if (screenX < -48 || screenX >= 304 || ScreenConversions.IsOutsideSpriteRange(screenY))
                continue;

            SpriteSheetFrame image = artwork.Sheet.Frame(ProjectileSheetFrames.ProjectileFrame(
                slot.Sprite ?? throw new InvalidOperationException("A drawn bomb shows a composition.")));
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusBomb, slot.Index), image,
                Position16<ScreenPixels>.Of(unchecked((ushort)screenX), screenY), SpriteMirror.None));
        }
    }

    /// <summary>Clears the five bomb slots and their two aggregate counters.</summary>
    public void Reset()
    {
        foreach (SamusBombProjectileSlot slot in _slots)
            slot.ClearFields();
        BombCounter = 0;
        CooldownTimer = WrappingCountdown.Start(0);
        PowerBombExplosion.Reset();
        LastFrameResult = default;
    }

    /// <summary>
    /// Samus command $1E ($90:F4A2). In main gameplay a spinning Samus re-queues her spin
    /// sound through command $1C; otherwise a flare of at least $10 resumes the charge loop.
    /// </summary>
    private static void ResumeSoundsAfterPowerBombExplosion(
        SamusState samus,
        SuperMetroid.Core.Progression.SuperMetroidGameState gameState)
    {
        if (gameState != SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay)
            return;
        if (SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            if (SamusSpinSoundCommand.Select(samus) is { } spinSound)
                samus.LiquidPhysics.QueueMovementSound(spinSound, priority: SoundRequestPriority.High);
            return;
        }
        if (samus.ProjectileFlareCounter >= ChargeSoundFlareThreshold)
            samus.LiquidPhysics.QueueMovementSound(Library1Sound.ResumeChargingBeam, priority: SoundRequestPriority.High);
    }

    /// <summary>$90:F4C2: the flare count from which a charging beam's loop is audible.</summary>
    private const ushort ChargeSoundFlareThreshold = 16;

    /// <summary>Ticks the shared shot cooldown, or holds it at $20 during X-Ray time freeze.</summary>
    private void StepCooldown(bool timeIsFrozen)
    {
        // $90:AC32: while time is frozen the cooldown is held at $20 instead of counting
        // down, so the first shot after X-ray waits out the full delay.
        if (timeIsFrozen)
        {
            CooldownTimer = WrappingCountdown.Start(SamusProjectileCooldownDefinitions.FrozenTime);
            return;
        }

        if (CooldownTimer.IsZero)
            return;

        // $90:AC1C also clamps already-negative/crossed-negative values to zero. Normal
        // bomb cooldowns are small positive words, but retaining the signed edge keeps a
        // debugger mutation from producing a permanently locked firing state.
        if (CooldownTimer.IsNegative)
        {
            CooldownTimer = WrappingCountdown.Start(0);
            return;
        }

        CooldownTimer = CooldownTimer.Decrement().Remaining;
        if (CooldownTimer.IsNegative)
            CooldownTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Applies held/new Shoot admission, ammo and equipment gates, then initializes a normal or power bomb in the lowest free bomb slot.</summary>
    internal int? TryPlaceBomb(
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        out bool rejectedPlacementClearedCharge)
    {
        rejectedPlacementClearedCharge = false;
        // The outer $90:BF9D test uses held Shoot. Both item branches eventually call
        // helper two, which separately insists on the newly-pressed bit.
        const ControllerButton shoot = ControllerButton.X;
        if ((controllerInput & shoot) == ControllerButton.None)
            return null;

        bool placingPowerBomb = samus.SelectedHudItem == HudItem.PowerBomb;
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
            samus.PowerBombs = SamusQuantity.DecrementWord(samus.PowerBombs);
            PowerBombExplosion.Arm();
        }

        int slotIndex = FindFreeBombSlot();
        SamusBombProjectileSlot slot = _slots[slotIndex];
        slot.ClearFields();
        slot.Type = placingPowerBomb ? PowerBombType : NormalBombType;
        slot.Position = slot.Position.WithWhole(samus.Position.Whole);
        slot.BombTimer = WrappingCountdown.Start(MorphBallBombDefinitions.InitialBombTimer);
        InitializeBomb(slot);
        CooldownTimer = WrappingCountdown.FromRaw(placingPowerBomb
            ? MorphBallBombDefinitions.PowerBombCooldown
            : MorphBallBombDefinitions.NormalBombCooldown);

        if (placingPowerBomb)
        {
            // The auto-cancel flag always wins. Otherwise consuming the last round clears
            // item index three so the next Shoot edge returns to the beam/normal-bomb path.
            if (samus.AutoCancelHudItemIndex != HudItem.Beam)
            {
                samus.SelectedHudItem = HudItem.Beam;
                samus.AutoCancelHudItemIndex = HudItem.Beam;
            }
            else if (samus.PowerBombs == 0)
            {
                samus.SelectedHudItem = HudItem.Beam;
            }
        }

        return slotIndex;
    }

    /// <summary>Consumes Morph Ball charge input and either continues its Down hold or emits all five spread bombs.</summary>
    private BombSpreadAdmission HandleBombSpreadInput(
        SamusState samus,
        ControllerButton controllerInput)
    {
        bool shootHeld = (controllerInput & ControllerButton.X) != ControllerButton.None;
        if (!shootHeld)
        {
            // The outer Morph-Ball handler cancels charge before testing HUD selection
            // ($90:BFA0-$BFC4). Releasing Shoot must also cancel with Power Bombs selected;
            // only held Shoot reaches their branch. Runtime consumes the palette/SFX bridge.
            return samus.ProjectileFlareCounter != 0
                ? BombSpreadAdmission.ChargeCancelled
                : BombSpreadAdmission.NotApplicable;
        }

        // Held Shoot with Power Bomb selected bypasses the ordinary spread helper.
        if (samus.SelectedHudItem == HudItem.PowerBomb)
            return BombSpreadAdmission.NotApplicable;

        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs) ||
            samus.ProjectileFlareCounter < MorphBallBombDefinitions.RequiredChargeFrames ||
            BombCounter != 0)
        {
            return BombSpreadAdmission.NotApplicable;
        }

        if ((controllerInput & ControllerButton.Down) != ControllerButton.None &&
            samus.BombSpreadChargeTimeoutCounter % MorphBallBombDefinitions.DownChargeTimeoutCycle <
                MorphBallBombDefinitions.DownChargeTimeoutUpdates)
        {
            samus.BombSpreadChargeTimeoutCounter =
                unchecked((ushort)(samus.BombSpreadChargeTimeoutCounter + 1));
            return BombSpreadAdmission.Charging;
        }

        SpawnBombSpread(samus);
        return BombSpreadAdmission.Spawned;
    }

    /// <summary>Installs five bank-$93 projectiles with slot-specific launch values and clears Samus's stored charge.</summary>
    internal void SpawnBombSpread(SamusState samus)
    {
        int verticalModifier =
            (samus.BombSpreadChargeTimeoutCounter >> 6) & 3;
        for (int index = 0; index < SlotCount; index++)
        {
            SamusBombProjectileSlot slot = _slots[index];
            slot.ClearFields();
            slot.Type = NormalBombType;
            slot.IsBombSpread = true;
            slot.Position = slot.Position.WithWhole(samus.Position.Whole);
            InitializeBomb(slot);

            BombSpreadLaunchDefinition launch = SamusBombSpreadLaunches.ForSlot(index);
            slot.BombTimer = WrappingCountdown.FromRaw(launch.FuseTimer);
            slot.BombSpreadHorizontal = launch.XVelocity;
            // The held charge adds up to three whole pixels of upward launch speed; the negated whole word
            // wraps exactly as the cartridge's sixteen-bit subtraction from zero does.
            slot.BombSpreadLaunchVelocity = SignedSixteenSixteen.FromParts(
                unchecked((short)-(launch.YSpeed.Whole + verticalModifier)), launch.YSpeed.Fraction);
            slot.BombSpreadVerticalVelocity = slot.BombSpreadLaunchVelocity;
        }

        BombCounter = SlotCount;
        CooldownTimer = WrappingCountdown.Start(MorphBallBombDefinitions.NormalBombCooldown);
        samus.BombSpreadChargeTimeoutCounter = 0;
        samus.ProjectileFlareCounter = 0;
    }

    /// <summary>Requires a fresh Shoot edge and available bomb capacity, testing only the cooldown's low byte after the first bomb.</summary>
    private bool TryReserveBombSlot(ControllerButton controllerNewInput, ControllerButton shoot)
    {
        if ((controllerNewInput & shoot) == ControllerButton.None)
            return false;

        // $90:C0E7 allows the first bomb regardless of cooldown. With any active bomb,
        // five slots or a nonzero LOW cooldown byte reject the edge. The high byte is not
        // part of this test; that oddity is intentional 65C816 SEP-width behavior.
        if (BombCounter != 0 &&
            (BombCounter >= SlotCount || CooldownTimer.EightBitCount != 0))
        {
            return false;
        }

        // Helper two increments both values before the caller searches for storage. The
        // caller immediately replaces cooldown with table entry five ($10) after init.
        CooldownTimer = CooldownTimer.AddFrames(1);
        BombCounter = WrappingCounter.IncrementWord(BombCounter);

        return true;
    }

    /// <summary>Finds the lowest free bomb slot, taking the last slot when an inconsistent count admitted a bomb with none free.</summary>
    private int FindFreeBombSlot()
    {
        int slotIndex = 0;
        while (!_slots[slotIndex].Type.IsNone)
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

    /// <summary>Starts a bomb: its damage and its program, from the compiled non-beam projectile data for the slot's family.</summary>
    internal static void InitializeBomb(
        SamusBombProjectileSlot slot)
    {
        // The non-beam selector is read at the family's row.
        SamusProjectileHeader header = SamusProjectileSelection.Select(SamusProjectileSelectorTable.NonBeam, slot.Type.Family.TableRow);

        slot.Damage = SamusProjectileDamage.Read(header);
        if (unchecked((short)slot.Damage) < 0)
            throw new InvalidDataException($"Bomb header {header} has crash-marker damage ${slot.Damage:X4}.");

        slot.Cursor = SamusProjectilePrograms.Start(SamusProjectileHeaders.Program(header, 0));
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Advances one bomb fuse or spread trajectory, starts explosions and visits terrain on the native trigger frame.</summary>
    private bool RunBombPreInstruction(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> blockReactions,
        ISamusRoomPlms? roomPlms,
        AreaId areaIndex,
        SamusKinematicsState samusKinematics)
    {
        // A bomb that struck an enemy is cleared before its fuse runs.
        if (slot.IsSpent)
        {
            ClearProjectile(slot);
            return false;
        }

        SamusProjectileFamily typeFamily = slot.Type.Family;
        if (typeFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            // Bomb slots hold only normal bombs and Power Bombs. No retail producer installs another family in them;
            // seeing one means the slot model was mutated inconsistently.
            throw new InvalidDataException(
                $"Bomb slot {slot.Index} has invalid projectile family {typeFamily}.");
        }

        bool explosionStarted = false;
        if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            PowerBombFuseStep fuse = SamusPowerBombFuse.Step(slot.BombTimer, PowerBombExplosion.IsArmed);
            slot.BombTimer = fuse.Timer;
            if (fuse.FastAnimation)
                slot.Cursor = SamusPowerBombFuse.FastAnimation(slot.Cursor);
            if (fuse.DeleteProjectile)
            {
                ClearProjectile(slot);
                return false;
            }
            if (fuse.SpawnExplosion)
            {
                PowerBombExplosion.Spawn(slot.Position.Whole);
                explosionStarted = true;
            }
        }
        else if (!slot.BombTimer.IsZero)
        {
            slot.BombTimer = slot.BombTimer.Decrement().Remaining;
            if (slot.BombTimer.IsAt(15))
            {
                // The slow and fast lists have identical four-frame layouts and differ by
                // $1C bytes. Adding to the live next-instruction pointer preserves phase.
                slot.Cursor = SamusPowerBombFuse.FastAnimation(slot.Cursor);
            }
            else if (slot.BombTimer.IsZero)
            {
                // $93:814E reads the pointer word embedded in the bomb-explosion data
                // record at $93:8683 and resets the instruction timer to one.
                slot.Cursor = SamusProjectilePrograms.Start(SamusProjectileProgram.BombExplosion);
                slot.InstructionTimer = WrappingCountdown.Start(1);
                explosionStarted = true;
            }
        }

        if (slot.IsBombSpread && !slot.BombTimer.IsZero)
            MoveBombSpread(level, slot, samusKinematics);

        if (typeFamily == SamusProjectileFamily.Bomb &&
            slot.BombTimer.IsZero &&
            !slot.HasReactedToTerrain)
        {
            // As soon as a normal bomb's fuse reads zero its explosion reacts with the
            // five-block cross around it, exactly once.
            slot.HasReactedToTerrain = true;
            CollectBlockExplosionReactions(level, slot, blockReactions, roomPlms, areaIndex);
        }
        else if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            // Collision mode three first turns the fuse-expiration sentinel $FFFF into
            // zero without touching terrain. Every later frame scans the newly reached
            // rectangle border using the high bytes of bank-$88's shared radii.
            if (slot.BombTimer.IsNegative)
            {
                slot.BombTimer = WrappingCountdown.Start(0);
            }
            else if (slot.BombTimer.IsZero && PowerBombExplosion.IsArmed)
            {
                CollectPowerBombBoundaryReactions(
                    level,
                    PowerBombExplosion.Position,
                    PowerBombExplosion.ExplosionRadius,
                    blockReactions,
                    roomPlms,
                    areaIndex,
                    slot.Type);
            }

        }

        return explosionStarted;
    }

    /// <summary>Applies Samus gravity to a spread bomb, reversing horizontal travel at walls and restoring launch speed on a floor bounce.</summary>
    private static void MoveBombSpread(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        SamusKinematicsState samus)
    {
        // Gravity adds with the fraction carrying into the whole word and the whole word wrapping at sixteen bits,
        // which is the signed 16.16 value's wrapping addition.
        slot.BombSpreadVerticalVelocity += SignedSixteenSixteen.FromParts(
            unchecked((short)samus.YAcceleration.Whole), samus.YAcceleration.Fraction);

        int previousY = slot.Position.Y.Whole;
        int previousYSub = slot.Position.Y.Fraction;
        slot.Position = slot.Position.AdvanceY(slot.BombSpreadVerticalVelocity);
        if (BombSpreadCollides(level, slot))
        {
            slot.Position = slot.Position.WithWholeY(previousY).WithFractionY(previousYSub);
            if (slot.BombSpreadVerticalVelocity.WholePixels < 0)
            {
                // A ceiling clears only the whole word; the fraction keeps accumulating gravity.
                slot.BombSpreadVerticalVelocity = slot.BombSpreadVerticalVelocity.WithWholePixels(0);
                slot.Radius = slot.Radius.WithY(0);
            }
            else
            {
                slot.BombSpreadVerticalVelocity = slot.BombSpreadLaunchVelocity;
            }
            return;
        }

        int previousX = slot.Position.X.Whole;
        int previousXSub = slot.Position.X.Fraction;
        // Rightward travel adds the speed with the fraction carrying into the whole word; leftward travel subtracts it
        // with the borrow. Both wrap at sixteen whole bits, the signed 16.16 value's wrapping addition.
        slot.Position = slot.Position.AdvanceX(slot.BombSpreadHorizontal.Displacement);
        if (!BombSpreadCollides(level, slot))
            return;

        slot.Position = slot.Position.WithWholeX(previousX).WithFractionX(previousXSub);
        slot.BombSpreadHorizontal = slot.BombSpreadHorizontal.Reversed();
    }

    /// <summary>Tests the bomb center against room bounds, extension parents, solid tiles and the cartridge's non-square slope heights.</summary>
    internal static bool BombSpreadCollides(
        RoomLevelData level,
        SamusBombProjectileSlot slot)
    {
        int blockX = slot.Position.X.Whole >> 4;
        int blockY = slot.Position.Y.Whole >> 4;
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return true;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
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

        if (!block.Behavior.IsNonSquareSlope)
            return true;

        // `$94:A58F` mirrors X before indexing the slope-height row, then compares the
        // resulting height with Y within the block. Equality counts as collision.
        int xWithinBlock = (block.Behavior.SlopeFlipsHorizontally
            ? slot.Position.X.Whole ^ 15
            : slot.Position.X.Whole) & 15;
        int yWithinBlock = slot.Position.Y.Whole & 15;
        if (block.Behavior.SlopeFlipsVertically)
            yWithinBlock ^= 15;
        int height = SlopeHeights.Read(block.Behavior.SlopeShape, xWithinBlock);
        return height <= yWithinBlock;
    }

    /// <summary>
    /// $94:A06A boundary traversal, shared by every callback that dispatches to it.
    /// Coordinates and radius belong to the global explosion, not the calling slot.
    /// </summary>
    internal static void CollectPowerBombBoundaryReactions(
        RoomLevelData level,
        Position16<RoomPixels> explosion,
        PowerBombRadius explosionRadius,
        List<BombBlockReaction> reactions,
        ISamusRoomPlms? roomPlms,
        AreaId areaIndex,
        SamusProjectileType projectileType)
    {
        int horizontalRadius = explosionRadius.WholePixels;
        int verticalRadius = (3 * horizontalRadius) >> 2;

        int left = Math.Max(0, explosion.X - horizontalRadius) >> 4;
        int right = Math.Min(
            level.Size.X - 1,
            (explosion.X + horizontalRadius) >> 4);
        int top = Math.Max(0, explosion.Y - verticalRadius) >> 4;
        int bottom = Math.Min(
            level.Size.Y - 1,
            (explosion.Y + verticalRadius) >> 4);

        // $94:A0F4/$A11A visit all four inclusive edges in this exact order. Corners are
        // intentionally visited twice; synchronous PLM terrain mutation means the second
        // visit can observe a different collision type than the first.
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(checked((ushort)x), checked((ushort)top)), reactions, roomPlms, areaIndex, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(checked((ushort)left), checked((ushort)y)), reactions, roomPlms, areaIndex, projectileType);
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(checked((ushort)x), checked((ushort)bottom)), reactions, roomPlms, areaIndex, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(checked((ushort)right), checked((ushort)y)), reactions, roomPlms, areaIndex, projectileType);
    }

    /// <summary>Visits the normal bomb's five-block cross in native order when its fuse reaches zero.</summary>
    private static void CollectBlockExplosionReactions(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> reactions,
        ISamusRoomPlms? roomPlms,
        AreaId areaIndex)
    {
        int centerX = slot.Position.X.Whole >> 4;
        int centerY = slot.Position.Y.Whole >> 4;
        // The centre, then above, right, left and below, in the native reaction order.
        Offset16<RoomBlocks>[] cross = [new(0, 0), new(0, -1), new(1, 0), new(-1, 0), new(0, 1)];

        foreach (Offset16<RoomBlocks> member in cross)
        {
            int x = centerX + member.X;
            int y = centerY + member.Y;
            if ((uint)x >= level.Size.X || (uint)y >= level.Size.Y)
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
                new Position16<RoomBlocks>(checked((ushort)x), checked((ushort)y)),
                reactions,
                roomPlms,
                areaIndex,
                slot.Type);
        }
    }

    /// <summary>Resolves a bombed tile through extension, item, door and special-block dispatch, including synchronous PLM changes.</summary>
    internal static void CollectSingleBombedBlockReaction(
        RoomLevelData level,
        Position16<RoomBlocks> bombedBlock,
        List<BombBlockReaction> reactions,
        ISamusRoomPlms? roomPlms,
        AreaId areaIndex,
        SamusProjectileType projectileType)
    {
        int x = bombedBlock.X;
        int y = bombedBlock.Y;
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(bombedBlock);
        reactions.Add(new BombBlockReaction());

        // `$94:9411/$9447` do not react to an extension block directly. A nonzero signed
        // behaviour redirects CurrentBlockIndex horizontally (type $5) or by whole room rows
        // (type $D), then rewinds the dispatcher return address so the resolved parent
        // is dispatched again. A zero-behaviour extension is simply air for this reaction.
        RoomCollisionBlock block = visitedBlock;
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return;

        // Item collision behaviour $45 routes to the already-loaded item object rather than
        // indexing the ordinary bomb/special-block tables. Visible type-$B frames need no
        // additional response; concealed type-$C/orb frames publish the generic trigger.
        if (block.Behavior == RoomBlockBehaviorValues.CollectibleTrigger &&
            block.CollisionType is RoomCollisionType.SpecialBlock or RoomCollisionType.ShootableBlock)
        {
            _ = roomPlms?.CollectiblePlms.TryNotifyCollectibleProjectileHit(block.Index);
            return;
        }

        // `$94:9F2E` selects temporary PLM $C83E for both type-$8 special collision and
        // type-$C shootable collision with behaviour $44. Setup $84:C7E2 deletes that temporary
        // slot, finds the resident actor at the same block, and publishes this projectile
        // word. The n00b tube begins as type $8 but its intact draw changes the live origin
        // to type $C, so narrowing this seam to colored doors strands the progression PLM.
        if (block.CollisionType is RoomCollisionType.SolidBlock or RoomCollisionType.ShootableBlock &&
            block.Behavior == RoomBlockBehaviorValues.ResidentPlmProjectileTrigger)
        {
            if (roomPlms is null ||
                !roomPlms.ProjectileTriggers.TryNotifyResidentProjectileHit(block.Index, projectileType))
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
        // private door behaviours as ordinary table indices both crashed on `$41` and lost
        // the cartridge behavior that allows a bomb placed beside a blue door to open it.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.TryGetBlueDoorOrientation(out _))
        {
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed blue-door cap at ({x},{y}) requires a room PLM owner.");
            }
            roomPlms.DoorPlms.TrySpawnBlueDoorOpening(
                level,
                block.Index,
                block.Behavior,
                projectileType);
            return;
        }

        // $94:A052 dispatches these types to immediate clear/set-carry routines. They
        // spawn no PLM and do not alter the room's terrain or behaviours, so recording the visit is
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
            // behaviour takes the native duplicate/area-dependent early return and therefore
            // neither allocates a PLM nor mutates terrain.
            if (block.Behavior.UsesAreaReactionTable)
                return;
            if (!block.Behavior.IsNormalReactionIndex(16))
            {
                // $94:A012 is a literal sixteen-word table. Retail room data must keep a
                // nonnegative bombable behaviour within that table; a larger value would make
                // the 65C816 read unrelated following ROM as a PLM header.
                throw new InvalidDataException(
                    $"Bombable block {block.Index} has behaviour ${block.Behavior.Value:X2} outside " +
                    "the native $94:A012 reaction table.");
            }
            if (roomPlms is null)
            {
                // Gameplay always owns the room's PLM pool. Null is supported only for
                // nonreactive synthetic fixtures, so reaching a producer without it is a
                // caller-composition error rather than untranslated cartridge behavior.
                throw new InvalidOperationException(
                    $"Bombed block reaction type {block.CollisionType}/behaviour ${block.Behavior.Value:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            // Spawn is synchronous: accepted behaviour 0..7 applies CEDA's temporary type-$8
            // or type-$0 word before the caller proceeds to its next border/cross member.
            roomPlms.BlockReactions.TrySpawnBombReactionBlock(
                level,
                block.Index,
                block.Behavior,
                projectileType);
            return;
        }

        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            // Gate shot blocks reuse shootable collision with private behaviour $46-$4D. They
            // select temporary trigger PLMs rather than the ordinary 0..F bomb table.
            if (block.CollisionType == RoomCollisionType.ShootableBlock &&
                block.Behavior.TryGetDownwardGateTrigger(out _))
            {
                if (roomPlms is null)
                {
                    throw new InvalidOperationException(
                        $"Bombed downward gate trigger at ({x},{y}) requires a room PLM owner.");
                }
                roomPlms.Gates.TrySpawnDownwardGateTrigger(
                    level,
                    block.Index,
                    block.Behavior,
                    projectileType);
                return;
            }

            // Type-$4 shootable air treats negative behaviour as a duplicate and returns.
            // Type-$C instead indexes one of eight area tables; every retail entry is
            // PLMEntries_nothing, but Spawn_PLM still consumes a slot for one pass.
            if (block.CollisionType == RoomCollisionType.ShootableAir &&
                block.Behavior.UsesAreaReactionTable)
                return;
            if (!block.Behavior.UsesAreaReactionTable &&
                block.Behavior != EscapeAnimalWallDefinitions.ReactionBehavior &&
                !block.Behavior.IsNormalReactionIndex(16) &&
                !block.Behavior.IsShootableCollisionProbe)
            {
                throw new InvalidDataException(
                    $"Shootable block {block.Index} has behaviour ${block.Behavior.Value:X2} outside " +
                    "the translated native bomb-reaction table range.");
            }
            if (block.Behavior.UsesAreaReactionTable &&
                !block.Behavior.IsAreaReactionIndex(8))
            {
                throw new InvalidDataException(
                    $"Area-dependent shootable block {block.Index} has behaviour " +
                    $"${block.Behavior.Value:X2} outside its eight-entry native table.");
            }
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed shootable type {block.CollisionType}/behaviour ${block.Behavior.Value:X2} " +
                    $"at ({x},{y}) requires a room PLM owner.");
            }

            roomPlms.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                block.Index,
                block.Behavior,
                projectileType,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
            return;
        }

        if (block.CollisionType == RoomCollisionType.SpecialBlock)
        {
            if (roomPlms is null)
            {
                throw new InvalidOperationException(
                    $"Bombed special block behaviour ${block.Behavior.Value:X2} at ({x},{y}) " +
                    "requires a room PLM owner.");
            }

            roomPlms.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                block.Index,
                block.Behavior,
                areaIndex,
                projectileType);
            return;
        }

        // Collision types are a four-bit field and every value 0..15 is handled above;
        // type 5/D either resolves to its parent or returned before this dispatcher. Keep
        // the guard for corrupted packed words, but do not label it missing behavior.
        throw new InvalidDataException(
            $"Bombed block reaction reached impossible type {block.CollisionType}/" +
            $"behaviour ${block.Behavior.Value:X2} at ({x},{y}).");
    }

    /// <summary>Ticks the bank-$93 bomb instruction list, including timed maps, delete and goto opcodes.</summary>
    internal bool RunProjectileInstructionHandler(
        SamusBombProjectileSlot slot)
    {
        // $93:81F2 is a wrapping 16-bit DEC. A legitimate initialized timer is always at
        // least one, but preserving wrap gives corrupt-state inspection the same behavior.
        slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
        if (!slot.InstructionTimer.IsZero)
            return false;

        AnimationProgramCursor<SamusProjectileProgram> cursor = slot.Cursor
            ?? throw new InvalidOperationException("An inactive bomb has no list to run.");
        if (SamusProjectilePrograms.RunToNextRecord(ref cursor, out int duration) is not { } frame)
        {
            ClearProjectile(slot);
            return true;
        }
        if (duration == 0)
            throw new InvalidDataException($"Bomb record {frame.Record} has zero duration.");
        SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(frame);
        slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)duration);
        slot.Sprite = FrameBindings?.Resolve(frame) ?? record.Sprite;
        slot.Radius = record.Radius;
        slot.Cursor = cursor;
        return false;
    }

    /// <summary>Scans bombs in ascending slot order and publishes the last overlapping bomb's Samus launch direction.</summary>
    private SamusBombJumpDirection PublishBombJumpOverlap(SamusState samus)
    {
        SamusBombJumpDirection publishedDirection = SamusBombJumpDirection.None;

        // The bomb-jump scan runs in ascending slot order. Only damage-bearing placed
        // bombs that have not struck an enemy reach the geometric test; spread bombs never launch Samus.
        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            SamusBombProjectileSlot slot = _slots[slotIndex];
            if (slot.Damage == 0 ||
                slot.IsBombSpread ||
                slot.Type.Family.IsExplosion ||
                slot.IsSpent)
            {
                continue;
            }

            int xDistance = Math.Abs((short)(slot.Position.X.Whole - samus.Position.X.Whole));
            int yDistance = Math.Abs((short)(slot.Position.Y.Whole - samus.Position.Y.Whole));
            if (xDistance >= slot.Radius.X + samus.Kinematics.Radius.X ||
                yDistance >= slot.Radius.Y + samus.Kinematics.Radius.Y)
            {
                continue;
            }

            SamusProjectileFamily typeFamily = slot.Type.Family;
            if (typeFamily is not (
                    SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb) ||
                !slot.BombTimer.IsAt(8))
                continue;

            // CMP SamusX,bombX: equal is straight; Samus left of the bomb launches left;
            // Samus right of the bomb launches right. Later overlapping slots overwrite
            // earlier ones exactly as the ascending assembly loop does.
            publishedDirection = samus.Position.X.Whole == slot.Position.X.Whole
                ? SamusBombJumpDirection.Straight
                : unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
                    ? SamusBombJumpDirection.Left
                    : SamusBombJumpDirection.Right;
            SamusPoseMetadata.PublishBombJumpDirection(samus, publishedDirection);
        }

        return publishedDirection;
    }

    /// <summary>Clears a bomb slot and decrements the bomb count, clamping a signed underflow to zero.</summary>
    private void ClearProjectile(SamusBombProjectileSlot slot)
    {
        bool wasActive = slot.IsActive;
        slot.ClearFields();
        if (!wasActive)
            return;

        // Consistent state cannot underflow, but the clamp is real.
        CountdownStep remaining = WrappingCounter.Decrement(BombCounter);
        BombCounter = remaining.IsNegative ? (ushort)0 : remaining.Value;
    }

}
