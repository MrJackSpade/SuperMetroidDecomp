using SuperMetroid.Core.Samus.Colours;
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
    internal static SamusProjectileType PowerBombType => SamusProjectileType.Of(SamusProjectileFamily.PowerBomb);

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
    /// free-place search. A placement counts one only below the place count, a spread sets the count whole, the Baby
    /// Metroid's drain holds it whole and clears it (<see cref="DrainHoldsBombCount"/>), and a bomb that ends takes one
    /// away, staying at zero when the count was already cleared.
    /// </summary>
    public int BombCounter
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, PlaceCount, value);
    }

    /// <summary>Holds the bomb count full for one update of the Baby Metroid's drain (<see cref="DrainHoldsBombCount"/>).</summary>
    internal void HoldCountForDrain() => BombCounter = DrainHoldsBombCount.Held;

    /// <summary>Clears the bomb count as the Baby Metroid's drain lets Samus go (<see cref="DrainHoldsBombCount"/>).</summary>
    internal void ReleaseCountAfterDrain() => BombCounter = DrainHoldsBombCount.Released;

    /// <summary>
    /// The cooldown shared by every Samus projectile: every shot, bomb and boss starts it, a bomb's reservation lengthens
    /// it (<see cref="MorphBallBombDefinitions.ReservationCooldownUpdates"/>), X-ray holds it, and it is counted once each
    /// update and stops once over, so whether it has run out gates another shot and, while a bomb is out, another bomb.
    /// </summary>
    private readonly UpdateCountdown _cooldown = UpdateCountdown.Ended();

    /// <summary>The updates left on the shared projectile cooldown, none once it has run out.</summary>
    public int CooldownTimer => _cooldown.UpdatesLeft;

    /// <summary>
    /// Replaces the shared projectile cooldown from another Samus-projectile producer.
    /// </summary>
    /// <remarks>
    /// Beams, missiles, power bombs and normal bombs do not own separate cooldowns; they all read and write the same
    /// one. Ordinary projectiles live in a separate class, but that boundary must not create a second clock.
    /// </remarks>
    /// <param name="value">The cooldown's updates, zero or more.</param>
    internal void SetSharedCooldown(int value) => _cooldown.Restart(value);

    /// <summary>
    /// Creates one room's bombs, whose sounds join <paramref name="audioRequests"/> (#2057) and whose Power Bomb blasts glow
    /// in <paramref name="artwork"/>'s glow colours (#2049, #2397).
    /// </summary>
    /// <param name="audioRequests">The update's audio requests the bombs' sounds join: the runtime's, or a cinematic's.</param>
    /// <param name="artwork">
    /// The Samus artwork whose projectile frames the bombs draw and whose glow colours their blasts show, rebound with the
    /// host's artwork.
    /// </param>
    internal SamusBombProjectileSystem(GameplaySoundRequests audioRequests, SamusArtworkBinding artwork)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        _artwork = Ensure.NotNull(artwork);
        PowerBombExplosion = new SamusPowerBombExplosionState();
    }

    /// <summary>The update's audio requests the bombs' sounds join as they are made.</summary>
    internal GameplaySoundRequests AudioRequests { get; }

    /// <summary>
    /// Advances the Power Bomb explosion's shape independently of Samus's input phase.
    /// Scripted carries suppress the input phase but do not stop the blast, which advances right after the liquid wave
    /// and statue descent.
    /// </summary>
    public void AdvancePowerBombExplosion(
        SamusState samus,
        GameplayAction controllerInput,
        SuperMetroidGameState gameState = SuperMetroidGameState.MainGameplay)
    {
        Ensure.NotNull(samus);
        // Each update advances the blast with the liquid wave and statue descent, before gameplay reaches Samus's
        // frame. A power bomb spawned later in her input phase consequently receives
        // its first radius update on the next frame, not on its fuse-expiration frame.
        bool crystalFlashBlastWasRunning = PowerBombExplosion.Blast.Phase is
            PowerBombExplosionPhase.CrystalFlashExplosion or
            PowerBombExplosionPhase.CrystalFlashAfterglow;
        bool powerBombCleanup = PowerBombExplosion.StepFrame(_artwork.PowerBombGlowColors);
        if (powerBombCleanup && !crystalFlashBlastWasRunning)
        {
            // The explosion offers Crystal Flash only if Samus has remained on the exact bomb
            // origin. Failure (including moving one pixel) releases the power-bomb lock immediately.
            bool crystalFlashStarted =
                samus.Position.X.Whole == PowerBombExplosion.Position.X &&
                samus.Position.Y.Whole == PowerBombExplosion.Position.Y &&
                samus.CrystalFlash.TryBegin(samus, controllerInput);
            if (!crystalFlashStarted)
                PowerBombExplosion.ReleasePowerBombLock();
        }
        // The explosion resumes Samus's sounds at the end of every cleanup, including the
        // Crystal Flash blast's own.
        if (powerBombCleanup)
            ResumeSoundsAfterPowerBombExplosion(samus, gameState);
    }

    /// <summary>
    /// Runs the bomb-owned portion of the input phase, followed by the enemy
    /// overlap pass that the main gameplay loop invokes before the movement phase: <see cref="TickCooldownAndPlaceBombs"/>,
    /// then <see cref="HandleBombs"/>.
    /// </summary>
    internal void StepFrame(SamusTerrain terrain, SamusState samus, GameplayAction controllerInput, GameplayAction controllerNewInput, BombJumpOverlapPublication overlap = BombJumpOverlapPublication.Immediate, PowerBombExplosionStep explosion = PowerBombExplosionStep.Advance)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        if (explosion == PowerBombExplosionStep.Advance)
            AdvancePowerBombExplosion(samus, controllerInput);
        _ = TickCooldownAndPlaceBombs(samus, controllerInput, controllerNewInput);
        HandleBombs(terrain, samus, overlap);
    }

    /// <summary>
    /// Runs the bombs' share of Samus's HUD item step, which the game runs before it handles any projectile: the shared
    /// cooldown, then the morph ball's bomb producer.
    /// </summary>
    /// <returns>Whether the producer consumed the beam charge.</returns>
    public bool TickCooldownAndPlaceBombs(SamusState samus, GameplayAction controllerInput, GameplayAction controllerNewInput)
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
    internal void HandleBombs(SamusTerrain terrain, SamusState samus, BombJumpOverlapPublication overlap)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        // Bombs run in update order. X-ray may have frozen time in this same HUD pass: the cooldown above still runs, but
        // no projectile is handled after admission.
        foreach (SamusBomb bomb in InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (bomb.Animation is null || bomb.Occupant is not { } placed)
                continue;

            bool wasNormalBomb = placed.Type.Family == SamusProjectileFamily.Bomb;
            bool bombExplosionStarted = AdvanceBomb(
                terrain.Level,
                placed,
                terrain.RoomObjects,
                samus.Kinematics);
            if (bombExplosionStarted && wasNormalBomb)
            {
                // The fuse requests this sound at normal priority for every normal bomb
                // whose fuse expires. Keep the append-only shape because several spread
                // bombs can expire during one Samus projectile pass.
                AudioRequests.QueueSound(EnemySound.BombExplosion, SoundRequestPriority.Normal);
            }

            // A bomb its own update ended this pass shows nothing more.
            if (bomb.Occupant is not { IsAnimated: true } live)
                continue;

            AdvanceProjectileAnimation(live);
        }

        // Main gameplay runs the bomb overlap check after the input phase (which placed/updated the
        // bombs) and before the movement phase moves Samus. Store only the direction here. The
        // same frame's hit-interruption phase can arm the bomb jump's movement.
        // Runtime defers this until the humanoid shot producer has also run: a
        // shot this frame may suppress overlap before the bomb direction is stored.
        if (overlap == BombJumpOverlapPublication.Immediate)
            PublishBombJumpOverlap(samus);
    }

    /// <summary>Runs the bomb subset of the post-input-phase Samus/projectile interaction pass.</summary>
    /// <param name="samus">Samus, whom a bomb's blast may launch.</param>
    /// <param name="shotsSuppressInteraction">Whether a shot just fired keeps Samus from interacting with her own projectiles.</param>
    public void ResolveSamusOverlap(SamusState samus, bool shotsSuppressInteraction)
    {
        Ensure.NotNull(samus);
        if (!shotsSuppressInteraction && samus.HorizontalSpeed.BodyAttack == SamusBodyAttack.None)
            PublishBombJumpOverlap(samus);
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
            // oval, drawn apart from them.
            if (bomb.Animation is null || bomb.Occupant is not { } placed ||
                placed.Type.Family is not (SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb))
                continue;
            SamusProjectileFamily family = placed.Type.Family;

            // A Power Bomb's own picture disappears on the update its fuse runs out, as its blast begins; a bomb's
            // explosion goes on showing its pictures after its fuse has run out.
            if (family == SamusProjectileFamily.PowerBomb && bomb.HasBurnedOut)
                continue;

            // The bomb is published wherever it lies; the display clips it to the view (#1541).
            Offset16<ScreenPixels> place = bomb.Position.Whole.RelativeToCamera(cameraPosition);

            // A frame showing no composition draws nothing (#2558), and so does a composition the sheet of the beams the bomb
            // is pictured with holds as showing nothing (#1513, #1586).
            if (bomb.Sprite is not { } sprite)
                continue;
            SpriteSheet sheet = artwork.SheetOf(bomb.PicturedBeams);
            string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
            if (sheet.ShowsNothing(frame))
                continue;
            SpriteSheetFrame image = sheet.Frame(frame);
            sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusBomb), image, place));
        }
    }

    /// <summary>Ends Samus's bombs and clears their two aggregate counters.</summary>
    public void Reset()
    {
        foreach (SamusBomb bomb in _bombs)
            bomb.End();
        BombCounter = 0;
        _cooldown.End();
        PowerBombExplosion.Reset();
    }

    /// <summary>
    /// Resumes Samus's sounds. In main gameplay a spinning Samus re-queues her spin
    /// sound through the spin sound's selection; otherwise a flare that has started the charging sound
    /// (<see cref="BeamChargeDefinitions.ChargeSoundStartCounter"/>) resumes the charge loop.
    /// </summary>
    private static void ResumeSoundsAfterPowerBombExplosion(
        SamusState samus,
        SuperMetroidGameState gameState)
    {
        if (gameState != SuperMetroidGameState.MainGameplay)
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

    /// <summary>
    /// Ticks the shared shot cooldown, or holds it at <see cref="SamusProjectileCooldownDefinitions.FrozenTime"/> during
    /// X-Ray time freeze.
    /// </summary>
    private void StepCooldown(bool timeIsFrozen)
    {
        // While time is frozen the cooldown is held instead of counting
        // down, so the first shot after X-ray waits out the full delay.
        if (timeIsFrozen)
        {
            _cooldown.Restart(SamusProjectileCooldownDefinitions.FrozenTime);
            return;
        }

        _cooldown.Step();
    }

    /// <summary>Applies held/new Shoot admission, ammo and equipment gates, then initializes a normal or power bomb in the lowest free bomb place.</summary>
    internal void TryPlaceBomb(
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
            return;

        bool placingPowerBomb = samus.SelectedHudItem == HudItem.PowerBomb;
        if (placingPowerBomb && PowerBombExplosion.IsArmed)
            return;

        // Normal bombs require the Bomb item. A selected Power Bomb skips this equipment
        // check and goes straight to the reservation.
        if (!placingPowerBomb && !samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs))
            return;

        if (!TryReserveBomb(controllerNewInput, shoot))
        {
            // A rejected reservation (no new Shoot press, every bomb place in use, or the
            // cooldown running) also cancels the beam charge, so a partial charge carried
            // into the ball does not outlive it. The equipment and armed-Power-Bomb checks
            // reject before the reservation and keep the charge. The runtime publishes the
            // cancellation's shade and sound.
            rejectedPlacementClearedCharge = samus.BeamCharge.IsCharging;
            return;
        }

        // HUD selection cannot normally point at an empty ammo class. The game's ordering
        // still holds: the reservation has already counted the bomb when
        // Power Bombs are selected with none left.
        if (placingPowerBomb && samus.PowerBombs == 0)
            return;

        if (placingPowerBomb)
        {
            // A count Crystal Flash drained below zero still places Power Bombs, each taking it further below; only the
            // 32768th from below zero wraps it to the largest count (CountsRunBelowZero), a wrap play can reach.
            samus.PowerBombs = CountsRunBelowZero.DecrementWrapping(samus.PowerBombs);
            PowerBombExplosion.Arm();
        }

        SamusBomb bomb = Occupy(FindFreePlace());
        SamusBombOccupant placed = Place(bomb, placingPowerBomb ? PowerBombType : NormalBombType);
        placed.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        placed.Position = placed.Position.WithWhole(samus.Position.Whole);
        placed.Fuse = UpdateCountdown.Lasting(MorphBallBombDefinitions.InitialBombTimer);
        InitializeBomb(placed);
        _cooldown.Restart(placingPowerBomb
            ? MorphBallBombDefinitions.PowerBombCooldown
            : MorphBallBombDefinitions.NormalBombCooldown);

        if (placingPowerBomb)
        {
            // The auto-cancel flag always wins. Otherwise consuming the last round clears
            // the Power Bomb selection so the next Shoot edge returns to the beam/normal-bomb path.
            if (samus.AutoCancelledItem is not null)
            {
                samus.SelectedHudItem = HudItem.Beam;
                samus.AutoCancelledItem = null;
            }
            else if (samus.PowerBombs == 0)
            {
                samus.SelectedHudItem = HudItem.Beam;
            }
        }
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
            !samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter) ||
            BombCounter != 0)
        {
            return BombSpreadAdmission.NotApplicable;
        }

        if ((controllerInput & GameplayAction.Down) != GameplayAction.None &&
            samus.BombSpreadCharge.IsRunning)
        {
            samus.BombSpreadCharge.Count();
            return BombSpreadAdmission.Charging;
        }

        SpawnBombSpread(samus);
        return BombSpreadAdmission.Spawned;
    }

    /// <summary>Installs five bombs with each place's own launch and clears Samus's stored charge.</summary>
    internal void SpawnBombSpread(SamusState samus)
    {
        // One bomb for each authored launch, from the spread's leftmost place to its rightmost.
        int place = 0;
        foreach (BombSpreadLaunchDefinition launch in SamusBombSpreadLaunchDefinitions.LeftToRight)
        {
            SamusBomb bomb = Occupy(place++);
            SamusBombOccupant placed = Place(bomb, NormalBombType);
            placed.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
            placed.SpreadFlight = SamusBombSpreadLaunches.Launch(launch, samus.BombSpreadCharge.Age);
            placed.Position = placed.Position.WithWhole(samus.Position.Whole);
            InitializeBomb(placed);
            placed.Fuse = UpdateCountdown.For(launch.FuseTimer);
        }

        BombCounter = PlaceCount;
        _cooldown.Restart(MorphBallBombDefinitions.NormalBombCooldown);
        samus.BombSpreadCharge = UpdateCountdown.Lasting(MorphBallBombDefinitions.DownChargeTimeoutUpdates);
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
            (BombCounter >= PlaceCount || _cooldown.IsRunning))
        {
            return false;
        }

        // Reserving a bomb counts it and lengthens the cooldown by one update before a free
        // place is chosen; once the bomb starts, placement replaces the cooldown with the
        // bomb's own.
        _cooldown.Lengthen(MorphBallBombDefinitions.ReservationCooldownUpdates);
        BombCounter++;

        return true;
    }

    /// <summary>The lowest place no bomb holds.</summary>
    /// <exception cref="InvalidOperationException">
    /// Every place holds a bomb. The placing count admits a bomb only below the place count and counts every bomb until it
    /// ends; only the drain's release clears it while bombs may be out (<see cref="DrainHoldsBombCount"/>), and none is
    /// then. A full set of places therefore means the count fell out of step with the bombs, where the game would reuse
    /// the last place.
    /// </exception>
    private int FindFreePlace()
    {
        for (int place = 0; place < PlaceCount; place++)
        {
            if (!_bombs[place].IsOccupied)
                return place;
        }

        throw new InvalidOperationException(
            $"A bomb was admitted with all {PlaceCount} places held: the placing count ({BombCounter}) is out of step with the bombs out.");
    }

    /// <summary>Takes <paramref name="bomb"/>'s place with a bomb of <paramref name="type"/> and the damage its family's bomb definition gives.</summary>
    internal static SamusBombOccupant Place(SamusBomb bomb, SamusProjectileType type) =>
        bomb.Occupy(type, SamusProjectileDamage.Read(SamusProjectileSelection.NonBeam(type.Family)));

    /// <summary>Starts a bomb's animation, from the bomb definition of its family.</summary>
    private static void InitializeBomb(
        SamusBombOccupant placed)
    {
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(placed.Type.Family);
        placed.Animation = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(kind));
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
        SamusBombOccupant placed,
        ISamusRoomObjects roomObjects,
        SamusKinematicsState samusKinematics)
    {
        SamusBomb bomb = placed.Bomb;
        // A bomb that struck an enemy is cleared before its fuse runs.
        if (bomb.IsSpent)
        {
            ClearProjectile(bomb);
            return false;
        }

        SamusProjectileFamily typeFamily = placed.Type.Family;
        if (typeFamily is not (SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb))
        {
            // Bombs are only normal bombs and Power Bombs. No producer places another family;
            // seeing one means a bomb was mutated inconsistently.
            throw new InvalidDataException(
                $"The bomb in place {bomb.Place} has invalid projectile family {typeFamily}.");
        }

        bool explosionStarted = false;
        bool powerBombBoundaryReacts = false;
        if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            PowerBombFuseStep fuse = SamusPowerBombFuse.Step(bomb.Fuse, PowerBombExplosion.IsArmed);
            powerBombBoundaryReacts = fuse.BoundaryReacts;
            if (fuse.FastAnimation)
                SamusPowerBombFuse.SwitchToFastAnimation(bomb.Animation);
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
        else if (!bomb.HasBurnedOut)
        {
            bool burnedOut = bomb.Fuse.Count();
            if (bomb.Fuse.UpdatesLeft == SamusPowerBombFuseData.FastAnimationTimer)
            {
                // The fuse switches the bomb to the same frame of its fast animation, so its
                // four-frame cycle keeps its place.
                SamusPowerBombFuse.SwitchToFastAnimation(bomb.Animation);
            }
            else if (burnedOut)
            {
                // The fuse running out starts the bomb's explosion animation, which shows its first picture next.
                placed.Animation = SamusProjectilePlayback.Start(SamusProjectileSegment.BombExplosion);
                explosionStarted = true;
            }
        }

        // A burned-out spread bomb flies no further and never turns, even with its centre in a solid block (#2466): the
        // game still runs the spread's collision test on it, but with the fuse out that test sets off the explosion's
        // cross below and reports no collision, so the wall turn never moves it.
        if (bomb.SpreadFlight is { } flight && !bomb.HasBurnedOut)
            MoveBombSpread(level, placed, flight, samusKinematics);

        if (typeFamily == SamusProjectileFamily.Bomb &&
            bomb.HasBurnedOut &&
            !bomb.HasReactedToTerrain)
        {
            // As soon as a normal bomb's fuse reads zero its explosion reacts with the
            // five-block cross around it, exactly once.
            placed.HasReactedToTerrain = true;
            ReactToBombExplosionCross(level, placed, roomObjects);
        }
        else if (typeFamily == SamusProjectileFamily.PowerBomb)
        {
            // The update the explosion starts on touches no terrain (PowerBombFuseStep.BoundaryReacts). Every later frame
            // scans the newly reached rectangle border using the explosion's shared radii in whole pixels.
            if (powerBombBoundaryReacts && PowerBombExplosion.IsArmed)
            {
                ReactToPowerBombBoundary(
                    level,
                    PowerBombExplosion.Position,
                    PowerBombExplosion.Blast.ExplosionRadius,
                    roomObjects,
                    placed.Type);
            }

        }

        return explosionStarted;
    }

    /// <summary>
    /// Flies a spread bomb one update: Samus's gravity accelerates it, a ceiling stops its rise, a floor bounces it back
    /// up at its launch velocity and a wall reverses its horizontal travel. A bomb that meets a ceiling or floor moves no
    /// further that update.
    /// </summary>
    private static void MoveBombSpread(
        RoomLevelData level,
        SamusBombOccupant bomb,
        BombSpreadFlight flight,
        SamusKinematicsState samus)
    {
        flight.Accelerate(samus.YAcceleration.AsDisplacement());

        FixedPosition<RoomPixels> beforeVertical = bomb.Position;
        bomb.Position = bomb.Position.AdvanceY(flight.VerticalVelocity);
        if (BombSpreadCollides(level, bomb.Bomb))
        {
            bomb.Position = beforeVertical;
            if (flight.VerticalVelocity.IsNegative)
            {
                // A ceiling also takes the bomb's vertical extent away for its later collisions.
                flight.StopRiseAtCeiling();
                bomb.Radius = bomb.Radius.WithY(0);
            }
            else
            {
                flight.BounceOffFloor();
            }
            return;
        }

        FixedPosition<RoomPixels> beforeHorizontal = bomb.Position;
        // The velocity moves the bomb, its fraction passing into the whole pixels in either direction.
        bomb.Position = bomb.Position.AdvanceX(flight.HorizontalVelocity);
        if (!BombSpreadCollides(level, bomb.Bomb))
            return;

        bomb.Position = beforeHorizontal;
        flight.ReverseAtWall();
    }

    /// <summary>
    /// Tests the bomb center against room bounds, the block its cell stands for (<see cref="ContinuedBlocks"/>), solid blocks
    /// and the game's non-square slope heights.
    /// </summary>
    internal static bool BombSpreadCollides(
        RoomLevelData level,
        SamusBomb bomb)
    {
        int blockX = RoomGridConversions.PixelToBlock(bomb.Position.X.Whole);
        int blockY = RoomGridConversions.PixelToBlock(bomb.Position.Y.Whole);
        if (blockX >= level.Size.X || blockY >= level.Size.Y)
            return true;

        RoomCollisionBlock block = level.GetCollisionBlock(new(blockX, blockY));
        if (!level.TryReachBlockStoodFor(ref block))
            return false;

        if (block.CollisionType != RoomCollisionType.Slope)
        {
            return !RoomCollisionTypes.Of(block.CollisionType).BombsPass;
        }

        if (!block.Behavior.Slope.IsNonSquare)
            return true;

        // The bomb collides where it lies at or below the slope's surface, the surface mirrored with the slope.
        int xWithinBlock = RoomGridConversions.OffsetWithinBlock(bomb.Position.X.Whole, mirrored: block.Behavior.Slope.FlipsHorizontally);
        int yWithinBlock = RoomGridConversions.OffsetWithinBlock(bomb.Position.Y.Whole, mirrored: block.Behavior.Slope.FlipsVertically);
        int height = block.Behavior.Slope.Shape.HeightAt(xWithinBlock);
        return height <= yWithinBlock;
    }

    /// <summary>
    /// The power-bomb boundary's reactions, shared by every bomb's update that runs them: each block of the boundary
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
    internal static List<Position16<RoomBlocks>> PowerBombBoundaryBlocks(
        RoomLevelData level,
        Position16<RoomPixels> explosion,
        PowerBombRadius explosionRadius)
    {
        int horizontalRadius = explosionRadius.WholePixels;
        int verticalRadius = PowerBombBlastOval.VerticalRadius(horizontalRadius);

        // The blast's reach is clipped to the room: before its first pixel and after its last block. A blast just left
        // of or above the room reaches past the room's edge (CoordinateAxis states the wrap), so its far edge is first
        // held to the last room coordinate a block can be read at; the room's own clip then leaves that edge before the
        // near one. A blast both left of and above the room so visits no edge at all.
        int left = RoomGridConversions.PixelToBlock(Math.Max(0, explosion.X - horizontalRadius));
        int right = Math.Min(
            level.Size.X - 1,
            RoomGridConversions.PixelToBlock(Math.Min(RoomGridConversions.LastPixel, explosion.X + horizontalRadius)));
        int top = RoomGridConversions.PixelToBlock(Math.Max(0, explosion.Y - verticalRadius));
        int bottom = Math.Min(
            level.Size.Y - 1,
            RoomGridConversions.PixelToBlock(Math.Min(RoomGridConversions.LastPixel, explosion.Y + verticalRadius)));

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
        SamusBombOccupant placed,
        ISamusRoomObjects roomObjects)
    {
        SamusBomb bomb = placed.Bomb;
        int centerX = RoomGridConversions.PixelToBlock(bomb.Position.X.Whole);
        int centerY = RoomGridConversions.PixelToBlock(bomb.Position.Y.Whole);
        foreach (Offset16<RoomBlocks> member in MorphBallBombDefinitions.ExplosionCross)
        {
            int x = centerX + member.X;
            int y = centerY + member.Y;
            if (x < 0 || x >= level.Size.X || y < 0 || y >= level.Size.Y)
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
                placed.Type);
        }
    }

    /// <summary>
    /// Reacts to the block a bombed cell stands for (<see cref="ContinuedBlocks"/>): its item, door, special-block or ordinary
    /// reaction, including synchronous room-object changes.
    /// </summary>
    internal static void ReactToBombedBlock(
        RoomLevelData level,
        Position16<RoomBlocks> bombedBlock,
        ISamusRoomObjects roomObjects,
        SamusProjectileType projectileType)
    {
        int x = bombedBlock.X;
        int y = bombedBlock.Y;
        RoomCollisionBlock visitedBlock = level.GetCollisionBlock(bombedBlock);

        // A bombed cell reacts as the block it stands for, at that block; one that stands for nothing is air.
        RoomCollisionBlock block = visitedBlock;
        if (!level.TryReachBlockStoodFor(ref block))
            return;

        // A bombed item block, special or shootable, tells the item room object at it of the hit, and no other reaction
        // runs.
        if (block.CollisionType == RoomCollisionType.SpecialBlock && block.Behavior.Special == SpecialBlockReaction.CollectibleTrigger ||
            block.CollisionType == RoomCollisionType.ShootableBlock && block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            roomObjects.CollectibleObjects.NotifyCollectibleProjectileHit(block.At);
            return;
        }

        // A bombed solid or shootable block holding a resident room object's trigger tells that room object the
        // projectile's type. Coloured doors are such blocks, and so is the intact n00b tube, a solid block its intact
        // picture makes shootable.
        if (block.CollisionType is RoomCollisionType.SolidBlock or RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
        {
            // A block no object holds any more (the already-broken n00b tube's origin) takes the
            // bomb and nothing happens.
            roomObjects.ProjectileTriggers.NotifyResidentProjectileHit(block.At, BlockStrike.By(projectileType));
            return;
        }

        // The four ordinary blue-door cap orientations lead to the blue door setup whether the collision came from the
        // shot or the bomb step. Normal bombs use the bomb projectile family and are accepted, so a bomb placed beside a
        // blue door opens it; the Power Bomb family is rejected inside the shared setup.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.TryGetBlueDoorOrientation(out _))
        {
            roomObjects.DoorObjects.SpawnBlueDoorOpening(
                level,
                block.At,
                block.Behavior,
                BlockStrike.By(projectileType));
            return;
        }

        // The bomb reaction sends these types to reactions that only report pass or stop. They
        // spawn no room object and do not alter the room's terrain or behaviours: the bomb's visit is recorded
        // and nothing else happens.
        if (RoomCollisionTypes.Of(block.CollisionType).BombLeavesAlone)
            return;

        if (block.CollisionType is RoomCollisionType.BombableAir or RoomCollisionType.BombableBlock)
        {
            // Both bombable-air and bombable-solid reactions use the bomb-block reactions; an inert block neither
            // spawns a room object nor changes the terrain.
            if (block.Behavior.Bombable == BombableReaction.Inert)
                return;
            if (block.Behavior.Bombable is null)
            {
                // Every bombable block names its bomb reaction; the import refuses one that does not.
                throw new InvalidDataException(
                    $"Bombable block {block.Position} has behaviour ({block.Behavior}) with no " +
                    "bomb-block reaction.");
            }
            // Spawn is synchronous: an accepted bomb-block reaction applies its temporary solid-block
            // or air collision before the bomb proceeds to its next border/cross member.
            roomObjects.BlockReactions.SpawnBombReactionBlock(
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
                roomObjects.Gates.SpawnDownwardGateTrigger(
                    level,
                    block.At,
                    block.Behavior,
                    BlockStrike.By(projectileType));
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
                BlockStrike.By(projectileType),
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
            return;
        }

        if (block.CollisionType == RoomCollisionType.SpecialBlock)
        {
            roomObjects.BlockReactions.SpawnBombedSpecialBlock(
                level,
                block.At,
                block.Behavior,
                projectileType);
            return;
        }

        // Every collision type is handled above; a continuing cell either reached the block it stands for or returned
        // before this step, so reaching here means a collision type was added without a bomb reaction.
        throw new InvalidDataException(
            $"Bombed block reaction reached impossible type {block.CollisionType}/" +
            $"behaviour ({block.Behavior}) at ({x},{y}).");
    }

    /// <summary>
    /// Plays the bomb's animation one update on: each frame it shows anew publishes its picture and radius, and an animation
    /// that ends deletes the bomb.
    /// </summary>
    /// <exception cref="InvalidOperationException">The bomb has ended and has no animation to play.</exception>
    /// <exception cref="InvalidDataException">The animation shows a still picture, which no bomb animation does.</exception>
    internal void AdvanceProjectileAnimation(
        SamusBombOccupant bomb)
    {
        AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> animation = bomb.Animation
            ?? throw new InvalidOperationException("An inactive bomb has no animation to play.");
        if (SamusProjectilePlayback.StepToNewFrame(animation) is { } frame)
        {
            // A bomb's animations show no still picture: every frame lasts its updates.
            if (animation.IsResting)
                throw new InvalidDataException($"Bomb frame {frame} is a still picture.");
            bomb.Sprite = _artwork.ProjectileFrames.Resolve(frame);
            bomb.Radius = SamusProjectileFrameDefinitions.Radius(frame);
            return;
        }
        if (animation.HasStopped)
            ClearProjectile(bomb.Bomb);
    }

    /// <summary>Scans bombs from the first place up and publishes the last overlapping bomb's Samus launch direction.</summary>
    private void PublishBombJumpOverlap(SamusState samus)
    {
        // The bomb-jump scan runs from the first place up. Only live placed bombs that have not
        // struck an enemy reach the geometric test; spread bombs never launch Samus.
        for (int place = 0; place < PlaceCount; place++)
        {
            SamusBomb bomb = _bombs[place];
            if (!bomb.IsActive ||
                bomb.Occupant is not { } placed ||
                bomb.IsBombSpread ||
                placed.Type.Family.IsExplosion ||
                bomb.IsSpent)
            {
                continue;
            }

            // A placed bomb stays where Samus laid it, at her place in the room, and departing on an elevator clears the
            // bombs, so she and the bomb both lie in the room and their distance is plain.
            int xDistance = Math.Abs(bomb.Position.X.Whole - samus.Position.X.Whole);
            int yDistance = Math.Abs(bomb.Position.Y.Whole - samus.Position.Y.Whole);
            if (xDistance >= bomb.Radius.X + samus.Kinematics.Radius.X ||
                yDistance >= bomb.Radius.Y + samus.Kinematics.Radius.Y)
            {
                continue;
            }

            SamusProjectileFamily typeFamily = placed.Type.Family;
            if (typeFamily is not (
                    SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb) ||
                bomb.Fuse.UpdatesLeft != MorphBallBombDefinitions.BombJumpExplosionCount)
                continue;

            // Samus's X against the bomb's: equal is straight; Samus left of the bomb launches left;
            // Samus right of the bomb launches right. Later overlapping bombs, in place order, overwrite
            // earlier ones.
            SamusBombJumpDirection publishedDirection = samus.Position.X.Whole == bomb.Position.X.Whole
                ? SamusBombJumpDirection.Straight
                : samus.Position.X.Whole < bomb.Position.X.Whole
                    ? SamusBombJumpDirection.Left
                    : SamusBombJumpDirection.Right;
            SamusPoseMetadata.PublishBombJumpDirection(samus, publishedDirection);
        }
    }

    /// <summary>Deletes a bomb, ending it, and decrements the bomb count, clamping a count below zero to zero.</summary>
    private void ClearProjectile(SamusBomb bomb)
    {
        bool wasActive = bomb.IsActive;
        bomb.End();
        if (!wasActive)
            return;

        // A count the drain's release cleared while bombs were out stays at zero as they end (DrainHoldsBombCount).
        BombCounter = Math.Max(0, BombCounter - 1);
    }

}
