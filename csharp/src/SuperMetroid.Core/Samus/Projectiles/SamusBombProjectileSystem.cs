using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The five bomb slots shared by projectile logic, animations,
/// block reactions, and Samus/projectile overlap handling.
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
    /// <summary>The installed composition each projectile animation frame draws, which a bomb's frames need bound.</summary>
    public SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings? FrameBindings { get; set; }
    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int SlotCount = MorphBallBombDefinitions.SlotCount;

    /// <summary>A placed Morph Ball bomb.</summary>
    public static SamusProjectileType NormalBombType => SamusProjectileType.Of(SamusProjectileFamily.Bomb);

    /// <summary>A placed Power Bomb.</summary>
    public static SamusProjectileType PowerBombType => SamusProjectileType.Of(SamusProjectileFamily.PowerBomb);

    /// <summary>Each bomb place's bomb, live or the last one that ended there, in place order.</summary>
    private readonly SamusBombProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount).Select(index => new SamusBombProjectileSlot(index)).ToArray();

    /// <summary>Each bomb place's bomb, live or the last one that ended there, in place order.</summary>
    public IReadOnlyList<SamusBombProjectileSlot> Slots => _slots;

    /// <summary>Creates the new bomb of place <paramref name="index"/>, ending the one there.</summary>
    /// <param name="index">The zero-based bomb place.</param>
    /// <returns>The new bomb, at rest at the room's origin with nothing set.</returns>
    private SamusBombProjectileSlot Occupy(int index)
    {
        _slots[index].End();
        var bomb = new SamusBombProjectileSlot(index);
        _slots[index] = bomb;
        return bomb;
    }

    /// <summary>
    /// Samus's first bomb: the bomb with the lowest ordinal, which a bomb placed while none is out takes and a bomb
    /// spread fills first. Kraid's shot passes, which count one past Samus's shots, reach it (<see cref="KraidShotPassReach"/>).
    /// </summary>
    public SamusBombProjectileSlot FirstBomb => _slots[0];

    /// <summary>The bomb slots in update order, highest ordinal first.</summary>
    private IEnumerable<SamusBombProjectileSlot> InUpdateOrder
    {
        get
        {
            for (int ordinal = _slots.Length - 1; ordinal >= 0; ordinal--)
                yield return _slots[ordinal];
        }
    }

    /// <summary>Owner of the flash, damaging radius, and armed flag.</summary>
    public SamusPowerBombExplosionState PowerBombExplosion { get; } = new();

    /// <summary>
    /// How many bombs Samus has out by the placing count, 0 through the bomb slots' count (#1627); kept separately from the
    /// free-slot search. A placement counts one only below the slot count, a spread or the Shitroid's drain sets the count
    /// whole, and a bomb that ends takes one away, staying at zero when the count was already cleared.
    /// </summary>
    public int BombCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, SlotCount, value);
    }

    /// <summary>
    /// The cooldown shared by every Samus projectile, 0 through <see cref="SamusProjectileCooldownDefinitions.LongestCooldown"/>
    /// frames: every shot, bomb and boss sets it no longer, and it only counts down from there or steps from zero to one, so
    /// whether it has run out gates another shot and, while a bomb is out, another bomb.
    /// </summary>
    public WrappingCountdown CooldownTimer { get; private set; }

    /// <summary>
    /// Replaces the shared projectile cooldown from another Samus-projectile producer.
    /// </summary>
    /// <remarks>
    /// Beams, missiles, power bombs and normal bombs do not own separate cooldowns; they all read and write the same
    /// one. Ordinary projectiles live in a separate class, but that boundary must not create a second clock.
    /// </remarks>
    /// <param name="value">The cooldown's frames, 0 through <see cref="SamusProjectileCooldownDefinitions.LongestCooldown"/>.</param>
    internal void SetSharedCooldown(int value) =>
        CooldownTimer = WrappingCountdown.Start(Ensure.BetweenInclusive(0, SamusProjectileCooldownDefinitions.LongestCooldown, value));

    /// <summary>
    /// Replaces the bomb count from an enemy-owned Samus interaction. Shitroid writes
    /// five while it drains Samus to suppress projectile allocation, then clears the count
    /// at the terminal one-energy transition without disturbing the shared cooldown.
    /// </summary>
    internal void SetSharedBombCounter(int value) => BombCounter = value;

    /// <summary>
    /// The update's audio requests the bombs' sounds join as they are made: the runtime's, once it points the room's
    /// systems at it; the bombs' own record before that.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();

    /// <summary>Points the bombs' sounds at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => AudioRequests = Ensure.NotNull(requests);

    /// <summary>
    /// Advances the Power Bomb explosion's shape independently of Samus's alpha/input handler.
    /// Scripted carries suppress the input phase but do not stop the per-line effect phase.
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
            // The explosion offers Crystal Flash only if Samus has remained on the exact bomb
            // origin. Failure (including moving one pixel) releases the power-bomb lock immediately.
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
        // The explosion resumes Samus's sounds at the end of every cleanup, including the
        // Crystal Flash window's own.
        if (powerBombCleanup)
            ResumeSoundsAfterPowerBombExplosion(samus, gameState);
    }

    /// <summary>
    /// Runs the bomb-owned portion of the input phase, followed by the enemy
    /// overlap pass that the main gameplay loop invokes before the movement phase: <see cref="RunHudProducer"/>,
    /// then <see cref="HandleBombs"/>.
    /// </summary>
    public void StepFrame(SamusTerrain terrain, SamusState samus, ControllerButton controllerInput, ControllerButton controllerNewInput, BombJumpOverlapPublication overlap = BombJumpOverlapPublication.Immediate, PowerBombExplosionStep explosion = PowerBombExplosionStep.Advance)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (explosion == PowerBombExplosionStep.Advance)
            AdvancePowerBombExplosion(samus, controllerInput);
        _ = RunHudProducer(samus, controllerInput, controllerNewInput);
        _ = HandleBombs(terrain, samus, overlap);
    }

    /// <summary>
    /// Runs the bombs' share of Samus's HUD handler, which the game runs before it handles any projectile: the shared
    /// cooldown, then the morph ball's bomb producer.
    /// </summary>
    /// <returns>Whether the producer consumed the beam charge.</returns>
    public bool RunHudProducer(SamusState samus, ControllerButton controllerInput, ControllerButton controllerNewInput)
    {
        Ensure.NotNull(samus);

        // The cooldown runs before the movement-type-specific HUD handler. A value of one
        // therefore reaches zero in time for a new Shoot edge during this same frame.
        // The forward-facing branch skips this call as well
        // as the HUD producer, but must still run the existing projectile slots below.
        // The stationary lock's input mode runs the projectile update directly, skipping the HUD
        // step that owns cooldown and new bomb placement.
        if (samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose))
            StepCooldown(samus.Xray.TimeIsFrozen);

        bool beamChargeConsumed = false;
        if (samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked && SamusPoseMetadata.IsStableBallPose(samus.Pose))
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
                AudioRequests.QueueSound(SamusSound.Silence, SoundRequestPriority.High);
        }
        return beamChargeConsumed;
    }

    /// <summary>
    /// Handles the bomb slots in update order, the bombs' share of the game's projectile handling, which runs after
    /// every HUD producer and before the shots (<see cref="SamusProjectileSystem.HandleShots"/>).
    /// </summary>
    /// <returns>The bomb jump direction published, or none when <paramref name="overlap"/> defers it.</returns>
    public SamusBombJumpDirection HandleBombs(SamusTerrain terrain, SamusState samus, BombJumpOverlapPublication overlap = BombJumpOverlapPublication.Immediate)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        bool explosionStarted = false;
        bool projectileDeleted = false;
        var blockReactions = new List<BombBlockReaction>();

        // Bombs run in update order. X-ray may have frozen time in this same HUD pass: the cooldown above still runs, but
        // no projectile is handled after admission.
        foreach (SamusBombProjectileSlot slot in InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (slot.Playback is null)
                continue;

            bool wasNormalBomb = slot.Type.Family == SamusProjectileFamily.Bomb;
            bool slotExplosionStarted = RunBombFrameBehavior(
                terrain.Level,
                slot,
                blockReactions,
                terrain.RoomObjects,
                samus.LiquidPhysics.Area,
                samus.Kinematics);
            explosionStarted |= slotExplosionStarted;
            if (slotExplosionStarted && wasNormalBomb)
            {
                // The fuse requests this sound at normal priority for every normal bomb
                // whose fuse expires. Keep the append-only shape because several spread
                // bombs can expire during one Samus projectile pass.
                AudioRequests.QueueSound(EnemySound.BombExplosion, SoundRequestPriority.Normal);
            }

            // The loop still steps the animation after a frame behaviour clears a slot.
            // Its cleared timer passes zero and returns without reading an empty animation. An
            // inactive projectile has the same externally visible result, so skip the
            // otherwise meaningless decrement rather than pretending an empty animation ran.
            if (!slot.IsActive)
            {
                projectileDeleted = true;
                continue;
            }

            projectileDeleted |= AdvanceProjectileAnimation(slot);
        }

        // Main gameplay runs the bomb overlap check after the input phase (which placed/updated the
        // bombs) and before the movement phase moves Samus. Store only the direction here. The
        // same frame's hit-interruption phase can arm the bomb movement handler.
        // Runtime defers this until the humanoid shot producer has also run: a
        // shot this frame may suppress overlap before the bomb direction is stored.
        SamusBombJumpDirection publishedDirection = overlap == BombJumpOverlapPublication.Deferred ? SamusBombJumpDirection.None : PublishBombJumpOverlap(samus);

        return publishedDirection;
    }

    /// <summary>Runs the bomb subset of the post-input-phase Samus/projectile interaction pass.</summary>
    public void ResolveSamusOverlap(SamusState samus, int projectileInvincibilityTimer)
    {
        Ensure.NotNull(samus);
        if (projectileInvincibilityTimer == 0 && samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.None)
            _ = PublishBombJumpOverlap(samus);
    }

    /// <summary>
    /// Draws the placed bombs and Power Bombs, in update order.
    /// </summary>
    public void Draw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusBombProjectileSlot slot in InUpdateOrder)
        {
            // Power bombs and normal bombs both use the same projectile
            // timed-frame animation. The large power-bomb flash itself is a glowing
            // window and is composed separately from the sprites.
            SamusProjectileFamily family = slot.Type.Family;
            if (slot.Playback is null ||
                family is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
                continue;

            // The projectile draw suppresses power bombs as soon as their shared
            // projectile value reaches zero. The fuse publishes that zero on
            // the detonation frame, so the small placed-bomb sprite must disappear exactly
            // when the large explosion effect begins. Normal bomb explosions keep
            // drawing their frames with a zero BombTimer and are unaffected.
            if (family == SamusProjectileFamily.PowerBomb && slot.BombTimer.IsZero)
                continue;

            // The bomb is published wherever it lies; the display clips it to the view (#1541).
            int screenX = CoordinateAxis.Difference(slot.Position.X.Whole, cameraPosition.X);
            int screenY = CoordinateAxis.Wrap(slot.Position.Y.Whole - cameraPosition.Y);

            ProjectileSpriteId sprite = slot.Sprite ?? throw new InvalidOperationException("A drawn bomb shows a composition.");
            // The empty composition draws nothing, and so does a composition the sheet of the beams the bomb is pictured
            // with holds as showing nothing (#1513, #1586).
            if (sprite.DrawsNothing)
                continue;
            SpriteSheet sheet = artwork.SheetOf(slot.PicturedBeams);
            string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
            if (sheet.ShowsNothing(frame))
                continue;
            SpriteSheetFrame image = sheet.Frame(frame);
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusBomb, slot.Index), image,
                Position16<ScreenPixels>.Of(CoordinateAxis.Wrap(screenX), screenY), SpriteMirror.None));
        }
    }

    /// <summary>Clears the five bomb slots and their two aggregate counters.</summary>
    public void Reset()
    {
        foreach (SamusBombProjectileSlot slot in _slots)
            slot.End();
        BombCounter = 0;
        CooldownTimer = WrappingCountdown.Start(0);
        PowerBombExplosion.Reset();
    }

    /// <summary>
    /// Resumes Samus's sounds. In main gameplay a spinning Samus re-queues her spin
    /// sound through the spin sound's selection; otherwise a flare of at least sixteen resumes the charge loop.
    /// </summary>
    private static void ResumeSoundsAfterPowerBombExplosion(
        SamusState samus,
        SuperMetroid.Core.Progression.SuperMetroidGameState gameState)
    {
        if (gameState != SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay)
            return;
        if (SamusPoseMetadata.ReadMovementType(samus) is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            if (SamusSpinSound.Select(samus) is { } spinSound)
                samus.LiquidPhysics.QueueMovementSound(spinSound, priority: SoundRequestPriority.High);
            return;
        }
        if (samus.BeamCharge.HasCharged(ChargeSoundFlareThreshold))
            samus.LiquidPhysics.QueueMovementSound(SamusSound.ResumeChargingBeam, priority: SoundRequestPriority.High);
    }

    /// <summary>The flare count from which a charging beam's loop is audible.</summary>
    private const int ChargeSoundFlareThreshold = 16;

    /// <summary>Ticks the shared shot cooldown, or holds it at 32 during X-Ray time freeze.</summary>
    private void StepCooldown(bool timeIsFrozen)
    {
        // While time is frozen the cooldown is held at 32 instead of counting
        // down, so the first shot after X-ray waits out the full delay.
        if (timeIsFrozen)
        {
            CooldownTimer = WrappingCountdown.Start(SamusProjectileCooldownDefinitions.FrozenTime);
            return;
        }

        if (CooldownTimer.IsZero)
            return;

        // The cooldown also clamps already-negative/crossed-negative values to zero. Normal
        // bomb cooldowns are small positive values, but retaining the signed edge keeps an
        // out-of-range cooldown from producing a permanently locked firing state.
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
        // Placement starts while Shoot is held; reserving the bomb, for either kind, then
        // also needs Shoot newly pressed.
        const ControllerButton shoot = ControllerButton.X;
        if ((controllerInput & shoot) == ControllerButton.None)
            return null;

        bool placingPowerBomb = samus.SelectedHudItem == HudItem.PowerBomb;
        if (placingPowerBomb && PowerBombExplosion.IsArmed)
            return null;

        // Normal bombs require the Bomb item. A selected Power Bomb skips this equipment
        // check and goes straight to the reservation.
        if (!placingPowerBomb && !samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            return null;

        if (!TryReserveBombSlot(controllerNewInput, shoot))
        {
            // A rejected reservation (no new Shoot press, every bomb slot in use, or the
            // cooldown running) also cancels the beam charge, so a partial charge carried
            // into the ball does not outlive it. The equipment and armed-Power-Bomb checks
            // reject before the reservation and keep the charge. The runtime publishes the
            // cancellation's palette and sound.
            rejectedPlacementClearedCharge = samus.BeamCharge.IsCharging;
            return null;
        }

        // Retail HUD selection cannot normally point at an empty ammo class. Preserve the
        // game's ordering nonetheless: the reservation has already counted the bomb when
        // Power Bombs are selected with none left.
        if (placingPowerBomb && samus.PowerBombs == 0)
            return null;

        if (placingPowerBomb)
        {
            samus.PowerBombs = CountsRunBelowZero.DecrementWrapping(samus.PowerBombs);
            PowerBombExplosion.Arm();
        }

        int slotIndex = FindFreeBombSlot();
        SamusBombProjectileSlot slot = Occupy(slotIndex);
        slot.Type = placingPowerBomb ? PowerBombType : NormalBombType;
        slot.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        slot.Position = slot.Position.WithWhole(samus.Position.Whole);
        slot.BombTimer = WrappingCountdown.Start(MorphBallBombDefinitions.InitialBombTimer);
        InitializeBomb(slot);
        CooldownTimer = WrappingCountdown.Start(placingPowerBomb
            ? MorphBallBombDefinitions.PowerBombCooldown
            : MorphBallBombDefinitions.NormalBombCooldown);

        if (placingPowerBomb)
        {
            // The auto-cancel flag always wins. Otherwise consuming the last round clears
            // item index three so the next Shoot edge returns to the beam/normal-bomb path.
            if (samus.AutoCancelledItem != HudItem.Beam)
            {
                samus.SelectedHudItem = HudItem.Beam;
                samus.AutoCancelledItem = HudItem.Beam;
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
            //. Releasing Shoot must also cancel with Power Bombs selected;
            // only held Shoot reaches their branch. Runtime consumes the palette/SFX bridge.
            return samus.BeamCharge.IsCharging
                ? BombSpreadAdmission.ChargeCancelled
                : BombSpreadAdmission.NotApplicable;
        }

        // Held Shoot with Power Bomb selected bypasses the ordinary spread helper.
        if (samus.SelectedHudItem == HudItem.PowerBomb)
            return BombSpreadAdmission.NotApplicable;

        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs) ||
            !samus.BeamCharge.HasCharged(MorphBallBombDefinitions.RequiredChargeFrames) ||
            BombCounter != 0)
        {
            return BombSpreadAdmission.NotApplicable;
        }

        if ((controllerInput & ControllerButton.Down) != ControllerButton.None &&
            samus.BombSpreadChargeTimeoutCounter < MorphBallBombDefinitions.DownChargeTimeoutUpdates)
        {
            samus.BombSpreadChargeTimeoutCounter++;
            return BombSpreadAdmission.Charging;
        }

        SpawnBombSpread(samus);
        return BombSpreadAdmission.Spawned;
    }

    /// <summary>Installs five projectiles with slot-specific launch values and clears Samus's stored charge.</summary>
    internal void SpawnBombSpread(SamusState samus)
    {
        int verticalModifier =
            samus.BombSpreadChargeTimeoutCounter / MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel;
        for (int index = 0; index < SlotCount; index++)
        {
            SamusBombProjectileSlot slot = Occupy(index);
            slot.Type = NormalBombType;
            slot.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
            slot.IsBombSpread = true;
            slot.Position = slot.Position.WithWhole(samus.Position.Whole);
            InitializeBomb(slot);

            BombSpreadLaunchDefinition launch = SamusBombSpreadLaunches.ForSlot(index);
            slot.BombTimer = WrappingCountdown.Start(launch.FuseTimer);
            slot.BombSpreadHorizontal = launch.XVelocity;
            // The held charge adds up to three whole pixels of upward launch speed.
            slot.BombSpreadLaunchVelocity = SignedSixteenSixteen.FromParts(
                -(launch.YSpeed.Whole + verticalModifier), launch.YSpeed.Fraction);
            slot.BombSpreadVerticalVelocity = slot.BombSpreadLaunchVelocity;
        }

        BombCounter = SlotCount;
        CooldownTimer = WrappingCountdown.Start(MorphBallBombDefinitions.NormalBombCooldown);
        samus.BombSpreadChargeTimeoutCounter = 0;
        samus.BeamCharge.ClearCount();
    }

    /// <summary>Requires a fresh Shoot edge and available bomb capacity, testing the cooldown after the first bomb.</summary>
    private bool TryReserveBombSlot(ControllerButton controllerNewInput, ControllerButton shoot)
    {
        if ((controllerNewInput & shoot) == ControllerButton.None)
            return false;

        // The first bomb is allowed regardless of cooldown. With any active bomb,
        // five slots or a running cooldown reject the edge.
        if (BombCounter != 0 &&
            (BombCounter >= SlotCount || !CooldownTimer.IsZero))
        {
            return false;
        }

        // Reserving a bomb counts it and lengthens the cooldown by one update before a free
        // slot is chosen; once the bomb starts, placement replaces the cooldown with the
        // bomb's own.
        CooldownTimer = CooldownTimer.AddFrames(1);
        BombCounter++;

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
                // This is the game's defensive/corrupt-state fallback: step the index back
                // once and reuse the last slot. Consistent BombCounter state never needs it.
                slotIndex = SlotCount - 1;
                break;
            }
        }
        return slotIndex;
    }

    /// <summary>Starts a bomb: its damage and its animation, from the compiled non-beam projectile data for the slot's family.</summary>
    internal static void InitializeBomb(
        SamusBombProjectileSlot slot)
    {
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(slot.Type.Family);

        slot.Damage = SamusProjectileDamage.Read(kind);

        slot.Playback = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(kind));
        slot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Starts the power bomb's explosion at <paramref name="position"/>: the explosion requests its sound at essential
    /// priority immediately before it marks itself active, so the sound passes the queues' power-bomb test before the
    /// explosion it announces turns later sounds away. A Power Bomb's fuse and the Chainsaw beam's borrowed fuse both
    /// start it here.
    /// </summary>
    internal void SpawnPowerBombExplosion(Position16<RoomPixels> position)
    {
        AudioRequests.QueueSound(SamusSound.PowerBombExplosion, SoundRequestPriority.Essential);
        PowerBombExplosion.Spawn(position);
    }

    /// <summary>Advances one bomb fuse or spread trajectory, starts explosions and visits terrain on the trigger frame.</summary>
    private bool RunBombFrameBehavior(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> blockReactions,
        ISamusRoomObjects? roomObjects,
        AreaId area,
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
                slot.Playback = SamusPowerBombFuse.FastAnimation(slot.Playback);
            if (fuse.DeleteProjectile)
            {
                ClearProjectile(slot);
                return false;
            }
            if (fuse.SpawnExplosion)
            {
                SpawnPowerBombExplosion(slot.Position.Whole);
                explosionStarted = true;
            }
        }
        else if (!slot.BombTimer.IsZero)
        {
            slot.BombTimer = slot.BombTimer.Decrement().Remaining;
            if (slot.BombTimer.IsAt(15))
            {
                // The slow and fast animations have identical four-frame layouts and differ by
                // a fixed distance. Moving the live animation by it preserves phase.
                slot.Playback = SamusPowerBombFuse.FastAnimation(slot.Playback);
            }
            else if (slot.BombTimer.IsZero)
            {
                // The explosion reads the animation embedded in the bomb-explosion data
                // record and resets the animation timer to one.
                slot.Playback = SamusProjectilePlayback.Start(SamusProjectileSegment.BombExplosion);
                slot.FrameHold = WrappingCountdown.Start(1);
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
            CollectBlockExplosionReactions(level, slot, blockReactions, roomObjects, area);
        }
        else if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            // The update the explosion starts on touches no terrain. Every later frame scans the
            // newly reached rectangle border using the explosion's shared radii in whole pixels.
            if (!explosionStarted && slot.BombTimer.IsZero && PowerBombExplosion.IsArmed)
            {
                CollectPowerBombBoundaryReactions(
                    level,
                    PowerBombExplosion.Position,
                    PowerBombExplosion.ExplosionRadius,
                    blockReactions,
                    roomObjects,
                    area,
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
        // Gravity adds with the fraction carrying into the whole pixels.
        slot.BombSpreadVerticalVelocity += SignedSixteenSixteen.FromParts(
            samus.YAcceleration.Whole, samus.YAcceleration.Fraction);

        int previousY = slot.Position.Y.Whole;
        int previousYSub = slot.Position.Y.Fraction;
        slot.Position = slot.Position.AdvanceY(slot.BombSpreadVerticalVelocity);
        if (BombSpreadCollides(level, slot))
        {
            slot.Position = slot.Position.WithWholeY(previousY).WithFractionY(previousYSub);
            if (slot.BombSpreadVerticalVelocity.WholePixels < 0)
            {
                // A ceiling clears only the speed's whole pixels; the fraction keeps accumulating gravity.
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
        // Rightward travel adds the speed with the fraction carrying into the whole pixels; leftward travel subtracts
        // it, borrowing from them. Both wrap after 65535 whole pixels, the signed 1/65536-pixel value's wrapping addition.
        slot.Position = slot.Position.AdvanceX(slot.BombSpreadHorizontal.Displacement);
        if (!BombSpreadCollides(level, slot))
            return;

        slot.Position = slot.Position.WithWholeX(previousX).WithFractionX(previousXSub);
        slot.BombSpreadHorizontal = slot.BombSpreadHorizontal.Reversed();
    }

    /// <summary>Tests the bomb center against room bounds, extension parents, solid blocks and the game's non-square slope heights.</summary>
    internal static bool BombSpreadCollides(
        RoomLevelData level,
        SamusBombProjectileSlot slot)
    {
        int blockX = RoomGridConversions.PixelToBlock(slot.Position.X.Whole);
        int blockY = RoomGridConversions.PixelToBlock(slot.Position.Y.Whole);
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return true;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return false;

        if (block.CollisionType != RoomCollisionType.Slope)
        {
            return block.CollisionType is not (
                RoomCollisionType.Air or
                RoomCollisionType.SpikeAir or
                RoomCollisionType.SpecialAir or
                RoomCollisionType.UnrevealedAir or
                RoomCollisionType.BombableAir);
        }

        if (!block.Behavior.Slope.IsNonSquare)
            return true;

        // Non-square slope collision mirrors X before indexing the slope-height row, then compares the
        // resulting height with Y within the block. Equality counts as collision.
        int xWithinBlock = RoomGridConversions.OffsetWithinBlock(slot.Position.X.Whole, mirrored: block.Behavior.Slope.FlipsHorizontally);
        int yWithinBlock = RoomGridConversions.OffsetWithinBlock(slot.Position.Y.Whole, mirrored: block.Behavior.Slope.FlipsVertically);
        int height = SlopeHeights.Read(block.Behavior.Slope.Shape, xWithinBlock);
        return height <= yWithinBlock;
    }

    /// <summary>
    /// The power-bomb boundary traversal, shared by every frame behaviour that dispatches to it.
    /// Coordinates and radius belong to the global explosion, not the calling slot.
    /// </summary>
    internal static void CollectPowerBombBoundaryReactions(
        RoomLevelData level,
        Position16<RoomPixels> explosion,
        PowerBombRadius explosionRadius,
        List<BombBlockReaction> reactions,
        ISamusRoomObjects? roomObjects,
        AreaId area,
        SamusProjectileType projectileType)
    {
        int horizontalRadius = explosionRadius.WholePixels;
        int verticalRadius = (3 * horizontalRadius) >> 2;

        // The blast's reach is clipped to the room: before its first pixel and, past the coordinate range, at its last pixel,
        // whose block lies at or beyond every room's last column or row.
        int left = RoomGridConversions.PixelToBlock(Math.Max(0, explosion.X - horizontalRadius));
        int right = Math.Min(
            level.Size.X - 1,
            RoomGridConversions.PixelToBlock(Math.Min(ushort.MaxValue, explosion.X + horizontalRadius)));
        int top = RoomGridConversions.PixelToBlock(Math.Max(0, explosion.Y - verticalRadius));
        int bottom = Math.Min(
            level.Size.Y - 1,
            RoomGridConversions.PixelToBlock(Math.Min(ushort.MaxValue, explosion.Y + verticalRadius)));

        // The traversal visits all four inclusive edges in this exact order. Corners are
        // intentionally visited twice; synchronous room-object terrain mutation means the second
        // visit can observe a different collision type than the first.
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(x, top), reactions, roomObjects, area, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(left, y), reactions, roomObjects, area, projectileType);
        for (int x = left; x <= right; x++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(x, bottom), reactions, roomObjects, area, projectileType);
        for (int y = top; y <= bottom; y++)
            CollectSingleBombedBlockReaction(level, new Position16<RoomBlocks>(right, y), reactions, roomObjects, area, projectileType);
    }

    /// <summary>Visits the normal bomb's five-block cross in order when its fuse reaches zero.</summary>
    private static void CollectBlockExplosionReactions(
        RoomLevelData level,
        SamusBombProjectileSlot slot,
        List<BombBlockReaction> reactions,
        ISamusRoomObjects? roomObjects,
        AreaId area)
    {
        int centerX = RoomGridConversions.PixelToBlock(slot.Position.X.Whole);
        int centerY = RoomGridConversions.PixelToBlock(slot.Position.Y.Whole);
        // The centre, then above, right, left and below, in the reaction order.
        Offset16<RoomBlocks>[] cross = [new(0, 0), new(0, -1), new(1, 0), new(-1, 0), new(0, 1)];

        foreach (Offset16<RoomBlocks> member in cross)
        {
            int x = centerX + member.X;
            int y = centerY + member.Y;
            if ((uint)x >= level.Size.X || (uint)y >= level.Size.Y)
            {
                // The bomb collision first admits the bomb centre through the room's scroll bounds,
                // and valid rooms surround reachable bomb centres with an edge wall. An
                // out-of-range cross member therefore identifies malformed room/caller
                // state rather than another bomb reaction.
                throw new InvalidDataException(
                    $"Bomb explosion cross reaches outside room storage at block ({x},{y}).");
            }

            CollectSingleBombedBlockReaction(
                level,
                new Position16<RoomBlocks>(x, y),
                reactions,
                roomObjects,
                area,
                slot.Type);
        }
    }

    /// <summary>Resolves a bombed block through extension, item, door and special-block dispatch, including synchronous room-object changes.</summary>
    internal static void CollectSingleBombedBlockReaction(
        RoomLevelData level,
        Position16<RoomBlocks> bombedBlock,
        List<BombBlockReaction> reactions,
        ISamusRoomObjects? roomObjects,
        AreaId area,
        SamusProjectileType projectileType)
    {
        int x = bombedBlock.X;
        int y = bombedBlock.Y;
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(bombedBlock);
        reactions.Add(new BombBlockReaction());

        // The block reactions do not react to an extension block directly. A nonzero signed
        // behaviour redirects the current block horizontally (a horizontal extension) or by whole room rows
        // (a vertical extension), then the dispatch restarts so the resolved parent
        // is dispatched again. A zero-behaviour extension is simply air for this reaction.
        RoomCollisionBlock block = visitedBlock;
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return;

        // The item-trigger collision behaviour routes to the already-loaded item object rather than
        // indexing the ordinary bomb/special-block tables. Visible special-block frames need no
        // additional response; concealed shootable/orb frames publish the generic trigger.
        if (block.CollisionType == RoomCollisionType.SpecialBlock && block.Behavior.Special == SpecialBlockReaction.CollectibleTrigger ||
            block.CollisionType == RoomCollisionType.ShootableBlock && block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            _ = roomObjects?.CollectibleObjects.TryNotifyCollectibleProjectileHit(block.At);
            return;
        }

        // The shot dispatch selects the temporary generic trigger room object for both solid-block collision and
        // shootable collision with the resident-object trigger behaviour. Its setup deletes that temporary
        // slot, finds the resident actor at the same block, and publishes this projectile
        // projectile. The n00b tube begins as a solid block but its intact draw changes the live origin
        // to a shootable block, so narrowing this seam to colored doors strands the progression room object.
        if (block.CollisionType is RoomCollisionType.SolidBlock or RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
        {
            if (roomObjects is null ||
                !roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, projectileType))
            {
                throw new InvalidOperationException(
                    $"Bombed resident-trigger block {block.Position} has no active room-object owner.");
            }
            return;
        }

        // The shot-block table's entries 40..43 route the four ordinary blue-door cap orientations to
        // the blue door setup regardless of whether the collision came from the shot or bomb
        // step. Normal bombs use the bomb projectile family and are accepted; the Power
        // Bomb family is rejected inside the shared setup. Treating these
        // private door behaviours as ordinary shot-block reactions both crashed on a blue door and lost
        // the game behavior that allows a bomb placed beside a blue door to open it.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.TryGetBlueDoorOrientation(out _))
        {
            if (roomObjects is null)
            {
                throw new InvalidOperationException(
                    $"Bombed blue-door cap at ({x},{y}) requires a room-object owner.");
            }
            roomObjects.DoorObjects.TrySpawnBlueDoorOpening(
                level,
                block.At,
                block.Behavior,
                projectileType);
            return;
        }

        // The bomb dispatch sends these types to reactions that only report pass or stop. They
        // spawn no room object and do not alter the room's terrain or behaviours, so recording the visit is
        // the complete observable effect for this runtime.
        if (block.CollisionType is
            RoomCollisionType.Air or
            RoomCollisionType.Slope or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.UnrevealedAir or
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.GrappleBlock)
            return;

        if (block.CollisionType is RoomCollisionType.BombableAir or RoomCollisionType.BombableBlock)
        {
            // Both bombable-air and bombable-solid handlers use the bomb-block reactions. An inert
            // block returns early and therefore neither allocates a room object nor mutates terrain.
            if (block.Behavior.Bombable == BombableReaction.Inert)
                return;
            if (block.Behavior.Bombable is null)
            {
                // A bombable block with no bomb reaction would make the game read unrelated data as a
                // room-object definition; retail room data holds none, and the import refuses one.
                throw new InvalidDataException(
                    $"Bombable block {block.Position} has behaviour ({block.Behavior}) with no " +
                    "bomb-block reaction.");
            }
            if (roomObjects is null)
            {
                // Gameplay always owns the room's room-object pool. Null is supported only for
                // nonreactive synthetic fixtures, so reaching a producer without it is a
                // caller-composition error rather than unsupported game behavior.
                throw new InvalidOperationException(
                    $"Bombed block reaction type {block.CollisionType}/behaviour ({block.Behavior}) " +
                    $"at ({x},{y}) requires a room-object owner.");
            }

            // Spawn is synchronous: accepted behaviour 0..7 applies the reaction's temporary solid-block
            // or air collision before the caller proceeds to its next border/cross member.
            roomObjects.BlockReactions.TrySpawnBombReactionBlock(
                level,
                block.At,
                block.Behavior,
                projectileType);
            return;
        }

        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            // Gate shot blocks reuse shootable collision with the private gate-trigger behaviours. They
            // select temporary trigger room objects rather than the ordinary 0..F bomb table.
            if (block.CollisionType == RoomCollisionType.ShootableBlock &&
                block.Behavior.TryGetDownwardGateTrigger(out _))
            {
                if (roomObjects is null)
                {
                    throw new InvalidOperationException(
                        $"Bombed downward gate trigger at ({x},{y}) requires a room-object owner.");
                }
                roomObjects.Gates.TrySpawnDownwardGateTrigger(
                    level,
                    block.At,
                    block.Behavior,
                    projectileType);
                return;
            }

            // Shootable air passes a bomb by when its reaction sets off only in a shootable block. A
            // shootable block with that reaction still spawns the empty room object, which consumes a
            // slot for one pass.
            if (block.CollisionType == RoomCollisionType.ShootableAir &&
                block.Behavior.Shootable is { } shootable && shootable.SetsOffOnlyInSolidBlocks)
                return;
            if (block.Behavior.Shootable is not (ShootableReaction.NothingWhenSolid or ShootableReaction.AnimalEscape or
                    ShootableReaction.CollisionProbe) &&
                !ShotBlockReactions.IsLeading(block.Behavior.Shootable))
            {
                throw new InvalidDataException(
                    $"Shootable block {block.Position} has behaviour ({block.Behavior}) with no " +
                    "reaction a bomb sets off.");
            }
            if (roomObjects is null)
            {
                throw new InvalidOperationException(
                    $"Bombed shootable type {block.CollisionType}/behaviour ({block.Behavior}) " +
                    $"at ({x},{y}) requires a room-object owner.");
            }

            roomObjects.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                block.At,
                block.Behavior,
                projectileType,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
            return;
        }

        if (block.CollisionType == RoomCollisionType.SpecialBlock)
        {
            if (roomObjects is null)
            {
                throw new InvalidOperationException(
                    $"Bombed special block behaviour ({block.Behavior}) at ({x},{y}) " +
                    "requires a room-object owner.");
            }

            roomObjects.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                block.At,
                block.Behavior,
                area,
                projectileType);
            return;
        }

        // Collision types are a four-bit field and every value 0..15 is handled above;
        // type 5/D either resolves to its parent or returned before this step. Keep
        // the guard for corrupted packed values, but do not label it missing behavior.
        throw new InvalidDataException(
            $"Bombed block reaction reached impossible type {block.CollisionType}/" +
            $"behaviour ({block.Behavior}) at ({x},{y}).");
    }

    /// <summary>Ticks the bomb animation, including timed frames, delete and jump steps.</summary>
    internal bool AdvanceProjectileAnimation(
        SamusBombProjectileSlot slot)
    {
        // The animation timer is a decrement wrapping at 65536. A legitimate initialized timer is always at
        // least one, but preserving wrap gives corrupt-state inspection the same behavior.
        slot.FrameHold = slot.FrameHold.Decrement().Remaining;
        if (!slot.FrameHold.IsZero)
            return false;

        AnimationPlayback<SamusProjectileSegment> playback = slot.Playback
            ?? throw new InvalidOperationException("An inactive bomb has no list to run.");
        if (SamusProjectilePlayback.PlayToNextFrame(ref playback, out int duration) is not { } frame)
        {
            ClearProjectile(slot);
            return true;
        }
        if (duration == 0)
            throw new InvalidDataException($"Bomb frame {frame} has zero duration.");
        SamusProjectileFrameGameplay gameplay = SamusProjectileFrameDefinitions.Get(frame);
        slot.FrameHold = WrappingCountdown.Start(duration);
        slot.Sprite = (FrameBindings ?? throw new InvalidOperationException(
            "Samus's bombs show their frames' installed compositions, which are not bound.")).Resolve(frame);
        slot.Radius = gameplay.Radius;
        slot.Playback = playback;
        return false;
    }

    /// <summary>Scans bombs in ascending slot order and publishes the last overlapping bomb's Samus launch direction.</summary>
    private SamusBombJumpDirection PublishBombJumpOverlap(SamusState samus)
    {
        SamusBombJumpDirection publishedDirection = SamusBombJumpDirection.None;

        // The bomb-jump scan runs in ascending slot order. Only occupied slots holding placed
        // bombs that have not struck an enemy reach the geometric test; spread bombs never launch Samus.
        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            SamusBombProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive ||
                slot.IsBombSpread ||
                slot.Type.Family.IsExplosion ||
                slot.IsSpent)
            {
                continue;
            }

            int xDistance = CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(slot.Position.Y.Whole, samus.Position.Y.Whole);
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

            // Samus's X against the bomb's: equal is straight; Samus left of the bomb launches left;
            // Samus right of the bomb launches right. Later overlapping slots overwrite
            // earlier ones exactly as the ascending assembly loop does.
            publishedDirection = samus.Position.X.Whole == slot.Position.X.Whole
                ? SamusBombJumpDirection.Straight
                : CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
                    ? SamusBombJumpDirection.Left
                    : SamusBombJumpDirection.Right;
            SamusPoseMetadata.PublishBombJumpDirection(samus, publishedDirection);
        }

        return publishedDirection;
    }

    /// <summary>Deletes a bomb, ending it, and decrements the bomb count, clamping a count below zero to zero.</summary>
    private void ClearProjectile(SamusBombProjectileSlot slot)
    {
        bool wasActive = slot.IsActive;
        slot.End();
        if (!wasActive)
            return;

        // A count the Shitroid's drain cleared while bombs were out stays at zero as they end.
        BombCounter = Math.Max(0, BombCounter - 1);
    }

}
