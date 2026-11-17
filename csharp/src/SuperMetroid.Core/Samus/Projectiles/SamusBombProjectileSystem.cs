using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's bombs, which projectile logic, animations, block reactions and Samus/projectile overlap handling share.
/// </summary>
/// <remarks>
/// Place order: Samus has <see cref="PlaceCount"/> bomb places. A placed bomb takes the lowest free place, a bomb spread
/// fills every place from the first, and the bomb update and drawing run in update order, from the last place down. The
/// bomb-jump scan alone runs from the first place up, so the last overlapping bomb in place order decides Samus's launch.
/// </remarks>
[RoomLifetime]
public sealed class SamusBombProjectileSystem
{
    /// <summary>The Samus artwork whose installed projectile frames each bomb's frames draw.</summary>
    private readonly SamusArtworkBinding _artwork;

    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int PlaceCount = MorphBallBombDefinitions.PlaceCount;

    /// <summary>A placed Morph Ball bomb.</summary>
    public static SamusProjectileType NormalBombType => SamusProjectileType.Of(SamusProjectileFamily.Bomb);

    /// <summary>A placed Power Bomb.</summary>
    public static SamusProjectileType PowerBombType => SamusProjectileType.Of(SamusProjectileFamily.PowerBomb);

    /// <summary>Each bomb place's bomb, live or the last one that ended there, in place order.</summary>
    private readonly SamusBomb[] _bombs =
        Enumerable.Range(0, PlaceCount).Select(place => new SamusBomb(place)).ToArray();

    /// <summary>Each bomb place's bomb, live or the last one that ended there, in place order.</summary>
    public IReadOnlyList<SamusBomb> Bombs => _bombs;

    /// <summary>Creates the new bomb of place <paramref name="place"/>, ending the one there.</summary>
    /// <param name="place">The bomb place, from zero.</param>
    /// <returns>The new bomb, at rest at the room's origin with nothing set.</returns>
    private SamusBomb Occupy(int place)
    {
        _bombs[place].End();
        var bomb = new SamusBomb(place);
        _bombs[place] = bomb;
        return bomb;
    }

    /// <summary>
    /// Samus's first bomb: the bomb in the first place, which a bomb placed while none is out takes and a bomb
    /// spread fills first. Kraid's shot passes, which count one past Samus's shots, reach it (<see cref="KraidShotPassReach"/>).
    /// </summary>
    public SamusBomb FirstBomb => _bombs[0];

    /// <summary>The bombs in update order, from the last place to the first.</summary>
    private IEnumerable<SamusBomb> InUpdateOrder
    {
        get
        {
            for (int place = _bombs.Length - 1; place >= 0; place--)
                yield return _bombs[place];
        }
    }

    /// <summary>Owner of the flash, damaging radius, and armed flag.</summary>
    public SamusPowerBombExplosionState PowerBombExplosion { get; }


    /// <summary>
    /// How many bombs Samus has out by the placing count, 0 through the bomb places' count (#1627); kept separately from the
    /// free-place search. A placement counts one only below the place count, a spread or the Shitroid's drain sets the count
    /// whole, and a bomb that ends takes one away, staying at zero when the count was already cleared.
    /// </summary>
    public int BombCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, PlaceCount, value);
    }

    /// <summary>
    /// The updates left on the cooldown shared by every Samus projectile, zero or more: every shot, bomb and boss sets it,
    /// a bomb's reservation lengthens it by one, and it counts down to zero and stops there, so whether it has run out
    /// gates another shot and, while a bomb is out, another bomb.
    /// </summary>
    public int CooldownTimer
    {
        get;
        private set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// Replaces the shared projectile cooldown from another Samus-projectile producer.
    /// </summary>
    /// <remarks>
    /// Beams, missiles, power bombs and normal bombs do not own separate cooldowns; they all read and write the same
    /// one. Ordinary projectiles live in a separate class, but that boundary must not create a second clock.
    /// </remarks>
    /// <param name="value">The cooldown's updates, zero or more.</param>
    internal void SetSharedCooldown(int value) => CooldownTimer = value;

    /// <summary>
    /// Replaces the bomb count from an enemy-owned Samus interaction. Shitroid writes
    /// five while it drains Samus to suppress projectile allocation, then clears the count
    /// at the terminal one-energy transition without disturbing the shared cooldown.
    /// </summary>
    internal void SetSharedBombCounter(int value) => BombCounter = value;

    /// <summary>
    /// Creates one room's bombs, whose sounds join <paramref name="audioRequests"/> (#2057) and whose Power Bomb blasts glow
    /// in <paramref name="powerBombGlowColors"/> (#2049).
    /// </summary>
    /// <param name="audioRequests">The update's audio requests the bombs' sounds join: the runtime's, or a cinematic's.</param>
    /// <param name="powerBombGlowColors">The installed Power Bomb glow colours.</param>
    /// <param name="artwork">The Samus artwork whose projectile frames the bombs draw, rebound with the host's artwork.</param>
    internal SamusBombProjectileSystem(GameplayAudioRequests audioRequests,
        InstalledPowerBombGlowColors powerBombGlowColors, SamusArtworkBinding artwork)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        _artwork = Ensure.NotNull(artwork);
        PowerBombExplosion = new SamusPowerBombExplosionState(powerBombGlowColors);
    }

    /// <summary>The update's audio requests the bombs' sounds join as they are made.</summary>
    internal GameplayAudioRequests AudioRequests { get; }

    /// <summary>
    /// Advances the Power Bomb explosion's shape independently of Samus's input phase.
    /// Scripted carries suppress the input phase but do not stop the blast, which advances right after the liquid wave
    /// and statue descent.
    /// </summary>
    public void AdvancePowerBombExplosion(
        SamusState samus,
        GameplayAction controllerInput,
        SuperMetroid.Core.Progression.SuperMetroidGameState gameState = SuperMetroid.Core.Progression.SuperMetroidGameState.MainGameplay)
    {
        Ensure.NotNull(samus);
        // Each update advances the blast with the liquid wave and statue descent, before gameplay reaches Samus's
        // frame. A power bomb spawned later in her input phase consequently receives
        // its first radius update on the next frame, not on its fuse-expiration frame.
        bool crystalFlashWindowWasActive = PowerBombExplosion.Blast.Phase is
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
                samus.CrystalFlash.TryBegin(samus, controllerInput);
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
    internal void StepFrame(SamusTerrain terrain, SamusState samus, GameplayAction controllerInput, GameplayAction controllerNewInput, BombJumpOverlapPublication overlap = BombJumpOverlapPublication.Immediate, PowerBombExplosionStep explosion = PowerBombExplosionStep.Advance)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (explosion == PowerBombExplosionStep.Advance)
            AdvancePowerBombExplosion(samus, controllerInput);
        _ = RunHudProducer(samus, controllerInput, controllerNewInput);
        _ = HandleBombs(terrain, samus, overlap);
    }

    /// <summary>
    /// Runs the bombs' share of Samus's HUD item step, which the game runs before it handles any projectile: the shared
    /// cooldown, then the morph ball's bomb producer.
    /// </summary>
    /// <returns>Whether the producer consumed the beam charge.</returns>
    public bool RunHudProducer(SamusState samus, GameplayAction controllerInput, GameplayAction controllerNewInput)
    {
        Ensure.NotNull(samus);

        // The cooldown runs before the movement type's own HUD step. A value of one
        // therefore reaches zero in time for a new Shoot edge during this same frame.
        // A forward-facing pose skips the cooldown and the placing of bombs, but the bombs already out still run below.
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
    /// Handles the bombs in update order, the bombs' share of the game's projectile handling, which runs after
    /// every HUD producer and before the shots (<see cref="SamusProjectileSystem.HandleShots"/>).
    /// </summary>
    /// <returns>The bomb jump direction published, or none when <paramref name="overlap"/> defers it.</returns>
    internal SamusBombJumpDirection HandleBombs(SamusTerrain terrain, SamusState samus, BombJumpOverlapPublication overlap)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        bool explosionStarted = false;
        bool projectileDeleted = false;

        // Bombs run in update order. X-ray may have frozen time in this same HUD pass: the cooldown above still runs, but
        // no projectile is handled after admission.
        foreach (SamusBomb bomb in InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (bomb.Playback is null)
                continue;

            bool wasNormalBomb = bomb.Type.Family == SamusProjectileFamily.Bomb;
            bool bombExplosionStarted = AdvanceBomb(
                terrain.Level,
                bomb,
                terrain.RoomObjects,
                samus.Kinematics);
            explosionStarted |= bombExplosionStarted;
            if (bombExplosionStarted && wasNormalBomb)
            {
                // The fuse requests this sound at normal priority for every normal bomb
                // whose fuse expires. Keep the append-only shape because several spread
                // bombs can expire during one Samus projectile pass.
                AudioRequests.QueueSound(EnemySound.BombExplosion, SoundRequestPriority.Normal);
            }

            // The loop still steps the animation after a frame behaviour ends a bomb.
            // Its cleared timer passes zero and returns without reading an empty animation. An
            // inactive projectile has the same externally visible result, so skip the
            // otherwise meaningless decrement rather than pretending an empty animation ran.
            if (!bomb.IsActive)
            {
                projectileDeleted = true;
                continue;
            }

            projectileDeleted |= AdvanceProjectileAnimation(bomb);
        }

        // Main gameplay runs the bomb overlap check after the input phase (which placed/updated the
        // bombs) and before the movement phase moves Samus. Store only the direction here. The
        // same frame's hit-interruption phase can arm the bomb jump's movement.
        // Runtime defers this until the humanoid shot producer has also run: a
        // shot this frame may suppress overlap before the bomb direction is stored.
        SamusBombJumpDirection publishedDirection = overlap == BombJumpOverlapPublication.Deferred ? SamusBombJumpDirection.None : PublishBombJumpOverlap(samus);

        return publishedDirection;
    }

    /// <summary>Runs the bomb subset of the post-input-phase Samus/projectile interaction pass.</summary>
    public void ResolveSamusOverlap(SamusState samus, int projectileInvincibilityTimer)
    {
        Ensure.NotNull(samus);
        if (projectileInvincibilityTimer == 0 && samus.HorizontalSpeed.BodyAttack == SamusBodyAttack.None)
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

        foreach (SamusBomb bomb in InUpdateOrder)
        {
            // Bombs and Power Bombs show their animations' pictures; a Power Bomb's large flash is its blast's glowing
            // window, drawn apart from them.
            SamusProjectileFamily family = bomb.Type.Family;
            if (bomb.Playback is null ||
                family is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
                continue;

            // A Power Bomb's own picture disappears on the update its fuse runs out, as its blast begins; a bomb's
            // explosion goes on showing its pictures after its fuse has run out.
            if (family == SamusProjectileFamily.PowerBomb && bomb.BombTimer.IsZero)
                continue;

            // The bomb is published wherever it lies; the display clips it to the view (#1541).
            Offset16<ScreenPixels> place = bomb.Position.Whole.RelativeToCamera(cameraPosition);

            ProjectileSpriteId sprite = bomb.Sprite ?? throw new InvalidOperationException("A drawn bomb shows a composition.");
            // The empty composition draws nothing, and so does a composition the sheet of the beams the bomb is pictured
            // with holds as showing nothing (#1513, #1586).
            if (sprite.DrawsNothing)
                continue;
            SpriteSheet sheet = artwork.SheetOf(bomb.PicturedBeams);
            string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
            if (sheet.ShowsNothing(frame))
                continue;
            SpriteSheetFrame image = sheet.Frame(frame);
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusBomb), image,
                place, SpriteMirror.None));
        }
    }

    /// <summary>Ends Samus's bombs and clears their two aggregate counters.</summary>
    public void Reset()
    {
        foreach (SamusBomb bomb in _bombs)
            bomb.End();
        BombCounter = 0;
        CooldownTimer = 0;
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
                samus.AudioRequests.QueueSound(spinSound, priority: SoundRequestPriority.High);
            return;
        }
        if (samus.BeamCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
            samus.AudioRequests.QueueSound(SamusSound.ResumeChargingBeam, priority: SoundRequestPriority.High);
    }

    /// <summary>Ticks the shared shot cooldown, or holds it at 32 during X-Ray time freeze.</summary>
    private void StepCooldown(bool timeIsFrozen)
    {
        // While time is frozen the cooldown is held at 32 instead of counting
        // down, so the first shot after X-ray waits out the full delay.
        if (timeIsFrozen)
        {
            CooldownTimer = SamusProjectileCooldownDefinitions.FrozenTime;
            return;
        }

        // Every cooldown set is a few dozen updates at most, so the count only ever runs down to zero and stops.
        if (CooldownTimer > 0)
            CooldownTimer--;
    }

    /// <summary>Applies held/new Shoot admission, ammo and equipment gates, then initializes a normal or power bomb in the lowest free bomb place.</summary>
    internal int? TryPlaceBomb(
        SamusState samus,
        GameplayAction controllerInput,
        GameplayAction controllerNewInput,
        out bool rejectedPlacementClearedCharge)
    {
        rejectedPlacementClearedCharge = false;
        // Placement starts while Shoot is held; reserving the bomb, for either kind, then
        // also needs Shoot newly pressed.
        const GameplayAction shoot = GameplayAction.Shoot;
        if ((controllerInput & shoot) == GameplayAction.None)
            return null;

        bool placingPowerBomb = samus.SelectedHudItem == HudItem.PowerBomb;
        if (placingPowerBomb && PowerBombExplosion.IsArmed)
            return null;

        // Normal bombs require the Bomb item. A selected Power Bomb skips this equipment
        // check and goes straight to the reservation.
        if (!placingPowerBomb && !samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            return null;

        if (!TryReserveBomb(controllerNewInput, shoot))
        {
            // A rejected reservation (no new Shoot press, every bomb place in use, or the
            // cooldown running) also cancels the beam charge, so a partial charge carried
            // into the ball does not outlive it. The equipment and armed-Power-Bomb checks
            // reject before the reservation and keep the charge. The runtime publishes the
            // cancellation's shade and sound.
            rejectedPlacementClearedCharge = samus.BeamCharge.IsCharging;
            return null;
        }

        // HUD selection cannot normally point at an empty ammo class. Preserve the
        // game's ordering nonetheless: the reservation has already counted the bomb when
        // Power Bombs are selected with none left.
        if (placingPowerBomb && samus.PowerBombs == 0)
            return null;

        if (placingPowerBomb)
        {
            // A count Crystal Flash drained below zero still places Power Bombs, each taking it further below; only the
            // 32768th from below zero wraps it to the largest count (CountsRunBelowZero), a wrap play can reach.
            samus.PowerBombs = CountsRunBelowZero.DecrementWrapping(samus.PowerBombs);
            PowerBombExplosion.Arm();
        }

        int place = FindFreePlace();
        SamusBomb bomb = Occupy(place);
        bomb.Type = placingPowerBomb ? PowerBombType : NormalBombType;
        bomb.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        bomb.Position = bomb.Position.WithWhole(samus.Position.Whole);
        bomb.BombTimer = WrappingCountdown.Start(MorphBallBombDefinitions.InitialBombTimer);
        InitializeBomb(bomb);
        CooldownTimer = placingPowerBomb
            ? MorphBallBombDefinitions.PowerBombCooldown
            : MorphBallBombDefinitions.NormalBombCooldown;

        if (placingPowerBomb)
        {
            // The auto-cancel flag always wins. Otherwise consuming the last round clears
            // the Power Bomb selection so the next Shoot edge returns to the beam/normal-bomb path.
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

        return place;
    }

    /// <summary>Consumes Morph Ball charge input and either continues its Down hold or emits all five spread bombs.</summary>
    private BombSpreadAdmission HandleBombSpreadInput(
        SamusState samus,
        GameplayAction controllerInput)
    {
        bool shootHeld = (controllerInput & GameplayAction.Shoot) != GameplayAction.None;
        if (!shootHeld)
        {
            // The Morph Ball step cancels a charge before it reads the HUD selection: releasing Shoot cancels it even
            // with Power Bombs selected, and only with Shoot held do the Power Bombs come into it. The Morph Ball update
            // applies the cancelled charge's shade and sound.
            return samus.BeamCharge.IsCharging
                ? BombSpreadAdmission.ChargeCancelled
                : BombSpreadAdmission.NotApplicable;
        }

        // Held Shoot with Power Bomb selected bypasses the ordinary bomb spread.
        if (samus.SelectedHudItem == HudItem.PowerBomb)
            return BombSpreadAdmission.NotApplicable;

        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs) ||
            !samus.BeamCharge.HasCharged(MorphBallBombDefinitions.RequiredChargeFrames) ||
            BombCounter != 0)
        {
            return BombSpreadAdmission.NotApplicable;
        }

        if ((controllerInput & GameplayAction.Down) != GameplayAction.None &&
            samus.BombSpreadChargeTimeoutCounter < MorphBallBombDefinitions.DownChargeTimeoutUpdates)
        {
            samus.BombSpreadChargeTimeoutCounter++;
            return BombSpreadAdmission.Charging;
        }

        SpawnBombSpread(samus);
        return BombSpreadAdmission.Spawned;
    }

    /// <summary>Installs five bombs with each place's own launch and clears Samus's stored charge.</summary>
    internal void SpawnBombSpread(SamusState samus)
    {
        int verticalModifier =
            samus.BombSpreadChargeTimeoutCounter / MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel;
        for (int place = 0; place < PlaceCount; place++)
        {
            SamusBomb bomb = Occupy(place);
            bomb.Type = NormalBombType;
            bomb.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
            bomb.IsBombSpread = true;
            bomb.Position = bomb.Position.WithWhole(samus.Position.Whole);
            InitializeBomb(bomb);

            BombSpreadLaunchDefinition launch = SamusBombSpreadLaunches.ForPlace(place);
            bomb.BombTimer = WrappingCountdown.Start(launch.FuseTimer);
            bomb.BombSpreadHorizontalVelocity = launch.XVelocity;
            // The held charge adds up to three whole pixels of upward launch speed.
            bomb.BombSpreadLaunchVelocity = SignedSixteenSixteen.FromParts(
                -(launch.YSpeed.Whole + verticalModifier), launch.YSpeed.Fraction);
            bomb.BombSpreadVerticalVelocity = bomb.BombSpreadLaunchVelocity;
        }

        BombCounter = PlaceCount;
        CooldownTimer = MorphBallBombDefinitions.NormalBombCooldown;
        samus.BombSpreadChargeTimeoutCounter = 0;
        samus.BeamCharge.ClearCount();
    }

    /// <summary>Requires a fresh Shoot edge and available bomb capacity, testing the cooldown after the first bomb.</summary>
    private bool TryReserveBomb(GameplayAction controllerNewInput, GameplayAction shoot)
    {
        if ((controllerNewInput & shoot) == GameplayAction.None)
            return false;

        // The first bomb is allowed regardless of cooldown. With any active bomb,
        // five bombs out or a running cooldown reject the edge.
        if (BombCounter != 0 &&
            (BombCounter >= PlaceCount || CooldownTimer != 0))
        {
            return false;
        }

        // Reserving a bomb counts it and lengthens the cooldown by one update before a free
        // place is chosen; once the bomb starts, placement replaces the cooldown with the
        // bomb's own.
        CooldownTimer++;
        BombCounter++;

        return true;
    }

    /// <summary>The lowest place no bomb holds.</summary>
    /// <exception cref="InvalidOperationException">
    /// Every place holds a bomb. The placing count admits a bomb only below the place count and counts every bomb until it
    /// ends; only the Shitroid clears it while bombs may be out, and it does so as it releases a Samus its drain has
    /// left lying since before any bomb she placed could still be burning. A full set of places therefore means the
    /// count fell out of step with the bombs, where the game would reuse the last place.
    /// </exception>
    private int FindFreePlace()
    {
        for (int place = 0; place < PlaceCount; place++)
        {
            if (_bombs[place].Type.IsNone)
                return place;
        }

        throw new InvalidOperationException(
            $"A bomb was admitted with all {PlaceCount} places held: the placing count ({BombCounter}) is out of step with the bombs out.");
    }

    /// <summary>Starts a bomb: its damage and its animation, from the compiled non-beam projectile data for the bomb's family.</summary>
    internal static void InitializeBomb(
        SamusBomb bomb)
    {
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(bomb.Type.Family);

        bomb.Damage = SamusProjectileDamage.Read(kind);

        bomb.Playback = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(kind));
        bomb.FrameHold = WrappingCountdown.Start(1);
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
    private bool AdvanceBomb(
        RoomLevelData level,
        SamusBomb bomb,
        ISamusRoomObjects roomObjects,
        SamusKinematicsState samusKinematics)
    {
        // A bomb that struck an enemy is cleared before its fuse runs.
        if (bomb.IsSpent)
        {
            ClearProjectile(bomb);
            return false;
        }

        SamusProjectileFamily typeFamily = bomb.Type.Family;
        if (typeFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            // Bombs are only normal bombs and Power Bombs. No producer places another family;
            // seeing one means a bomb was mutated inconsistently.
            throw new InvalidDataException(
                $"The bomb in place {bomb.Place} has invalid projectile family {typeFamily}.");
        }

        bool explosionStarted = false;
        if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            PowerBombFuseStep fuse = SamusPowerBombFuse.Step(bomb.BombTimer, PowerBombExplosion.IsArmed);
            bomb.BombTimer = fuse.Timer;
            if (fuse.FastAnimation)
                bomb.Playback = SamusPowerBombFuse.FastAnimation(bomb.Playback);
            if (fuse.DeleteProjectile)
            {
                ClearProjectile(bomb);
                return false;
            }
            if (fuse.SpawnExplosion)
            {
                SpawnPowerBombExplosion(bomb.Position.Whole);
                explosionStarted = true;
            }
        }
        else if (!bomb.BombTimer.IsZero)
        {
            bomb.BombTimer = bomb.BombTimer.Decrement().Remaining;
            if (bomb.BombTimer.IsAt(SamusPowerBombFuseData.FastAnimationTimer))
            {
                // The slow and fast animations have identical four-frame layouts and differ by
                // a fixed distance. Moving the live animation by it preserves phase.
                bomb.Playback = SamusPowerBombFuse.FastAnimation(bomb.Playback);
            }
            else if (bomb.BombTimer.IsZero)
            {
                // The fuse running out starts the bomb's explosion animation, which shows its first picture next.
                bomb.Playback = SamusProjectilePlayback.Start(SamusProjectileSegment.BombExplosion);
                bomb.FrameHold = WrappingCountdown.Start(1);
                explosionStarted = true;
            }
        }

        if (bomb.IsBombSpread && !bomb.BombTimer.IsZero)
            MoveBombSpread(level, bomb, samusKinematics);

        if (typeFamily == SamusProjectileFamily.Bomb &&
            bomb.BombTimer.IsZero &&
            !bomb.HasReactedToTerrain)
        {
            // As soon as a normal bomb's fuse reads zero its explosion reacts with the
            // five-block cross around it, exactly once.
            bomb.HasReactedToTerrain = true;
            ReactToBombExplosionCross(level, bomb, roomObjects);
        }
        else if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            // The update the explosion starts on touches no terrain. Every later frame scans the
            // newly reached rectangle border using the explosion's shared radii in whole pixels.
            if (!explosionStarted && bomb.BombTimer.IsZero && PowerBombExplosion.IsArmed)
            {
                ReactToPowerBombBoundary(
                    level,
                    PowerBombExplosion.Position,
                    PowerBombExplosion.Blast.ExplosionRadius,
                    roomObjects,
                    bomb.Type);
            }

        }

        return explosionStarted;
    }

    /// <summary>Applies Samus gravity to a spread bomb, reversing horizontal travel at walls and restoring launch speed on a floor bounce.</summary>
    private static void MoveBombSpread(
        RoomLevelData level,
        SamusBomb bomb,
        SamusKinematicsState samus)
    {
        // Gravity adds with the fraction carrying into the whole pixels.
        bomb.BombSpreadVerticalVelocity += samus.YAcceleration.AsDisplacement();

        int previousY = bomb.Position.Y.Whole;
        int previousYSub = bomb.Position.Y.Fraction;
        bomb.Position = bomb.Position.AdvanceY(bomb.BombSpreadVerticalVelocity);
        if (BombSpreadCollides(level, bomb))
        {
            bomb.Position = bomb.Position.WithWholeY(previousY).WithFractionY(previousYSub);
            if (bomb.BombSpreadVerticalVelocity.WholePixels < 0)
            {
                // A ceiling clears only the speed's whole pixels; the fraction keeps accumulating gravity.
                bomb.BombSpreadVerticalVelocity = bomb.BombSpreadVerticalVelocity.WithWholePixels(0);
                bomb.Radius = bomb.Radius.WithY(0);
            }
            else
            {
                bomb.BombSpreadVerticalVelocity = bomb.BombSpreadLaunchVelocity;
            }
            return;
        }

        int previousX = bomb.Position.X.Whole;
        int previousXSub = bomb.Position.X.Fraction;
        // The velocity moves the bomb, its fraction passing into the whole pixels in either direction.
        bomb.Position = bomb.Position.AdvanceX(bomb.BombSpreadHorizontalVelocity);
        if (!BombSpreadCollides(level, bomb))
            return;

        bomb.Position = bomb.Position.WithWholeX(previousX).WithFractionX(previousXSub);
        bomb.BombSpreadHorizontalVelocity = -bomb.BombSpreadHorizontalVelocity;
    }

    /// <summary>Tests the bomb center against room bounds, extension parents, solid blocks and the game's non-square slope heights.</summary>
    internal static bool BombSpreadCollides(
        RoomLevelData level,
        SamusBomb bomb)
    {
        int blockX = RoomGridConversions.PixelToBlock(bomb.Position.X.Whole);
        int blockY = RoomGridConversions.PixelToBlock(bomb.Position.Y.Whole);
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
        int xWithinBlock = RoomGridConversions.OffsetWithinBlock(bomb.Position.X.Whole, mirrored: block.Behavior.Slope.FlipsHorizontally);
        int yWithinBlock = RoomGridConversions.OffsetWithinBlock(bomb.Position.Y.Whole, mirrored: block.Behavior.Slope.FlipsVertically);
        int height = block.Behavior.Slope.Shape.HeightAt(xWithinBlock);
        return height <= yWithinBlock;
    }

    /// <summary>
    /// The power-bomb boundary's reactions, shared by every frame behaviour that runs them: each block of the boundary
    /// (<see cref="PowerBombBoundaryBlocks"/>) reacts in turn. Coordinates and radius belong to the global explosion, not
    /// the calling bomb.
    /// </summary>
    internal static void ReactToPowerBombBoundary(
        RoomLevelData level,
        Position16<RoomPixels> explosion,
        PowerBombRadius explosionRadius,
        ISamusRoomObjects roomObjects,
        SamusProjectileType projectileType)
    {
        foreach (Position16<RoomBlocks> block in PowerBombBoundaryBlocks(level, explosion, explosionRadius))
            ReactToBombedBlock(level, block, roomObjects, projectileType);
    }

    /// <summary>
    /// The blocks a Power Bomb blast's boundary reaches this update, in the order they react: the top edge left to right,
    /// the left edge top to bottom, the bottom edge left to right and the right edge top to bottom, each inclusive, so
    /// every corner is reached twice. The edges are those of the blast's reach clipped to the room, fixed before the first
    /// block reacts.
    /// </summary>
    internal static IReadOnlyList<Position16<RoomBlocks>> PowerBombBoundaryBlocks(
        RoomLevelData level,
        Position16<RoomPixels> explosion,
        PowerBombRadius explosionRadius)
    {
        int horizontalRadius = explosionRadius.WholePixels;
        int verticalRadius = PowerBombBlastOval.VerticalRadius(horizontalRadius);

        // The blast's reach is clipped to the room: before its first pixel and after its last block. A blast just left of
        // or above the room holds a coordinate near 65535, and its far edge would pass 65535, which PixelToBlock rejects,
        // so that edge is first held to the last room coordinate a block can be read at; the room's own clip then leaves
        // that edge before the near one. A blast both left of and above the room so visits no edge at all.
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
        var blocks = new List<Position16<RoomBlocks>>();
        for (int x = left; x <= right; x++)
            blocks.Add(new Position16<RoomBlocks>(x, top));
        for (int y = top; y <= bottom; y++)
            blocks.Add(new Position16<RoomBlocks>(left, y));
        for (int x = left; x <= right; x++)
            blocks.Add(new Position16<RoomBlocks>(x, bottom));
        for (int y = top; y <= bottom; y++)
            blocks.Add(new Position16<RoomBlocks>(right, y));
        return blocks;
    }

    /// <summary>Visits the normal bomb's five-block cross in order when its fuse reaches zero.</summary>
    private static void ReactToBombExplosionCross(
        RoomLevelData level,
        SamusBomb bomb,
        ISamusRoomObjects roomObjects)
    {
        int centerX = RoomGridConversions.PixelToBlock(bomb.Position.X.Whole);
        int centerY = RoomGridConversions.PixelToBlock(bomb.Position.Y.Whole);
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
                // out-of-range cross member therefore identifies malformed room or bomb
                // state rather than another bomb reaction.
                throw new InvalidDataException(
                    $"Bomb explosion cross reaches outside room storage at block ({x},{y}).");
            }

            ReactToBombedBlock(
                level,
                new Position16<RoomBlocks>(x, y),
                roomObjects,
                bomb.Type);
        }
    }

    /// <summary>Resolves a bombed block through extension, item, door and special-block reactions, including synchronous room-object changes.</summary>
    internal static void ReactToBombedBlock(
        RoomLevelData level,
        Position16<RoomBlocks> bombedBlock,
        ISamusRoomObjects roomObjects,
        SamusProjectileType projectileType)
    {
        int x = bombedBlock.X;
        int y = bombedBlock.Y;
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(bombedBlock);

        // A bombed extension block reacts as the block it leads to, across or by whole rows; one that leads nowhere is air.
        RoomCollisionBlock block = visitedBlock;
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return;

        // A bombed item block, special or shootable, tells the item room object at it of the hit, and no other reaction
        // runs.
        if (block.CollisionType == RoomCollisionType.SpecialBlock && block.Behavior.Special == SpecialBlockReaction.CollectibleTrigger ||
            block.CollisionType == RoomCollisionType.ShootableBlock && block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            _ = roomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(block.At);
            return;
        }

        // A bombed solid or shootable block holding a resident room object's trigger tells that room object the
        // projectile's type. Coloured doors are such blocks, and so is the intact n00b tube, a solid block its intact
        // picture makes shootable.
        if (block.CollisionType is RoomCollisionType.SolidBlock or RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
        {
            if (!roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, projectileType))
            {
                throw new InvalidOperationException(
                    $"Bombed resident-trigger block {block.Position} has no active room-object owner.");
            }
            return;
        }

        // The four ordinary blue-door cap orientations lead to the blue door setup whether the collision came from the
        // shot or the bomb step. Normal bombs use the bomb projectile family and are accepted, so a bomb placed beside a
        // blue door opens it; the Power Bomb family is rejected inside the shared setup.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.TryGetBlueDoorOrientation(out _))
        {
            roomObjects.DoorObjects.TrySpawnBlueDoorOpening(
                level,
                block.At,
                block.Behavior,
                projectileType);
            return;
        }

        // The bomb reaction sends these types to reactions that only report pass or stop. They
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
            // Both bombable-air and bombable-solid reactions use the bomb-block reactions. An inert
            // block returns early and therefore neither allocates a room object nor mutates terrain.
            if (block.Behavior.Bombable == BombableReaction.Inert)
                return;
            if (block.Behavior.Bombable is null)
            {
                // A bombable block with no bomb reaction would make the game read unrelated data as a
                // room-object definition; room data holds none, and the import refuses one.
                throw new InvalidDataException(
                    $"Bombable block {block.Position} has behaviour ({block.Behavior}) with no " +
                    "bomb-block reaction.");
            }
            // Spawn is synchronous: an accepted bomb-block reaction applies its temporary solid-block
            // or air collision before the bomb proceeds to its next border/cross member.
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
            // select temporary trigger room objects rather than the ordinary bomb reactions.
            if (block.CollisionType == RoomCollisionType.ShootableBlock &&
                block.Behavior.TryGetDownwardGateTrigger(out _))
            {
                roomObjects.Gates.TrySpawnDownwardGateTrigger(
                    level,
                    block.At,
                    block.Behavior,
                    projectileType);
                return;
            }

            // Shootable air passes a bomb by when its reaction sets off only in a shootable block. A
            // shootable block with that reaction still spawns the empty room object, which takes a
            // room-object place for one pass.
            if (block.CollisionType == RoomCollisionType.ShootableAir &&
                block.Behavior.Shootable is { } shootable && shootable.SetsOffOnlyInSolidBlocks)
                return;
            if (block.Behavior.Shootable is not (ShootableReaction.NothingWhenSolid or ShootableReaction.AnimalEscape or
                    ShootableReaction.CollisionProbe) &&
                !ShotBlockReactions.IsWeaponReaction(block.Behavior.Shootable))
            {
                throw new InvalidDataException(
                    $"Shootable block {block.Position} has behaviour ({block.Behavior}) with no " +
                    "reaction a bomb sets off.");
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
            roomObjects.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                block.At,
                block.Behavior,
                projectileType);
            return;
        }

        // Every collision type is handled above; a horizontal or vertical extension either
        // resolves to its parent or returned before this step, so reaching here means a
        // collision type was added without a bomb reaction.
        throw new InvalidDataException(
            $"Bombed block reaction reached impossible type {block.CollisionType}/" +
            $"behaviour ({block.Behavior}) at ({x},{y}).");
    }

    /// <summary>Ticks the bomb animation, including timed frames, delete and jump steps.</summary>
    internal bool AdvanceProjectileAnimation(
        SamusBomb bomb)
    {
        // Every frame holds the bomb for at least one update: placement starts the count at one and a frame of no
        // duration is rejected below, so the count reaches zero here and never passes it.
        bomb.FrameHold = bomb.FrameHold.Decrement().Remaining;
        if (!bomb.FrameHold.IsZero)
            return false;

        AnimationPlayback<SamusProjectileSegment> playback = bomb.Playback
            ?? throw new InvalidOperationException("An inactive bomb has no list to run.");
        if (SamusProjectilePlayback.PlayToNextFrame(ref playback, out int duration) is not { } frame)
        {
            ClearProjectile(bomb);
            return true;
        }
        if (duration == 0)
            throw new InvalidDataException($"Bomb frame {frame} has zero duration.");
        bomb.FrameHold = WrappingCountdown.Start(duration);
        bomb.Sprite = _artwork.ProjectileFrames.Resolve(frame);
        bomb.Radius = SamusProjectileFrameDefinitions.Radius(frame);
        bomb.Playback = playback;
        return false;
    }

    /// <summary>Scans bombs from the first place up and publishes the last overlapping bomb's Samus launch direction.</summary>
    private SamusBombJumpDirection PublishBombJumpOverlap(SamusState samus)
    {
        SamusBombJumpDirection publishedDirection = SamusBombJumpDirection.None;

        // The bomb-jump scan runs from the first place up. Only live placed bombs that have not
        // struck an enemy reach the geometric test; spread bombs never launch Samus.
        for (int place = 0; place < PlaceCount; place++)
        {
            SamusBomb bomb = _bombs[place];
            if (!bomb.IsActive ||
                bomb.IsBombSpread ||
                bomb.Type.Family.IsExplosion ||
                bomb.IsSpent)
            {
                continue;
            }

            int xDistance = CoordinateAxis.Distance(bomb.Position.X.Whole, samus.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(bomb.Position.Y.Whole, samus.Position.Y.Whole);
            if (xDistance >= bomb.Radius.X + samus.Kinematics.Radius.X ||
                yDistance >= bomb.Radius.Y + samus.Kinematics.Radius.Y)
            {
                continue;
            }

            SamusProjectileFamily typeFamily = bomb.Type.Family;
            if (typeFamily is not (
                    SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb) ||
                !bomb.BombTimer.IsAt(MorphBallBombDefinitions.BombJumpExplosionCount))
                continue;

            // Samus's X against the bomb's: equal is straight; Samus left of the bomb launches left;
            // Samus right of the bomb launches right. Later overlapping bombs, in place order, overwrite
            // earlier ones.
            publishedDirection = samus.Position.X.Whole == bomb.Position.X.Whole
                ? SamusBombJumpDirection.Straight
                : CoordinateAxis.Difference(samus.Position.X.Whole, bomb.Position.X.Whole) < 0
                    ? SamusBombJumpDirection.Left
                    : SamusBombJumpDirection.Right;
            SamusPoseMetadata.PublishBombJumpDirection(samus, publishedDirection);
        }

        return publishedDirection;
    }

    /// <summary>Deletes a bomb, ending it, and decrements the bomb count, clamping a count below zero to zero.</summary>
    private void ClearProjectile(SamusBomb bomb)
    {
        bool wasActive = bomb.IsActive;
        bomb.End();
        if (!wasActive)
            return;

        // A count the Shitroid's drain cleared while bombs were out stays at zero as they end.
        BombCounter = Math.Max(0, BombCounter - 1);
    }

}
