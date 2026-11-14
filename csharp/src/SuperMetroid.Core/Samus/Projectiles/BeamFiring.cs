using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Firing beams, missiles and the Hyper Beam: charge cancellation, spawn position and velocity, the interaction-immunity timer, and the charged and combo launch paths.</summary>
[RoomLifetime]
internal sealed class BeamFiring
{
    /// <summary>Clears charge flares and initializes launched beam animation.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Tracks held Shoot duration, charge effects, and Hyper Beam glow.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Handles the special beam attack after the charge counter reaches 120.</summary>
    private readonly BeamCombos _combos;
    /// <summary>Owns the five shot places, which a fired projectile takes, and the shot count.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Runs initial muzzle collision against room terrain and room objects.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>The speeds a Spazer and Plasma beam launches at and the cooldowns it waits.</summary>
    private readonly UnauthoredBeamFiring<SamusBeamCombination> _unauthoredFiring;

    /// <summary>Connects firing to charge, animation, slot allocation, combo, and terrain systems.</summary>
    internal BeamFiring(ProjectileAnimation animation, BeamChargeState charge, BeamCombos combos, ProjectileSlotTable slotTable,
        ProjectileTerrainCollision terrainCollision, UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        _animation = animation;
        _charge = charge;
        _combos = combos;
        _slotTable = slotTable;
        _terrainCollision = terrainCollision;
        _unauthoredFiring = Ensure.NotNull(unauthoredFiring);
    }
    /// <summary>
    /// Applies the shared side effect of all six HUD weapon selections:
    /// cancel beam charge and remove every live flare-animation layer before the newly
    /// selected weapon producer runs.
    /// </summary>
    public void CancelChargeForHudSelection()
        => CancelCharge();

    /// <summary>
    /// Applies the flare teardown shared by Morph-Ball charge cancellation and a completed
    /// bomb spread.
    /// </summary>
    public void CancelChargeForBombSpread()
        => CancelCharge();
    /// <summary>
    /// The scripted charge end: consume Pseudo Screw charge,
    /// clear its visible flare and restore the normal suit palette. Unlike HUD
    /// cancellation, this does not rewrite the previous-charge sample.
    /// </summary>
    public void ConsumePseudoScrewCharge(SamusColours colours, SamusState samus)
    {
        _charge.ChargePaletteStep = 0;
        _charge.ClearCount();
        _animation.ClearFlareAnimationState();
        ProjectileAnimation.LoadNormalSuitPalette(colours, samus);
    }

    /// <summary>Clears the charge, its prior sample, and all flare animation layers.</summary>
    private void CancelCharge()
    {
        _charge.ClearCount();
        _charge.PreviousCharge = BeamChargeLevel.None;
        _animation.ClearFlareAnimationState();
    }

    /// <summary>Handles held Shoot, release, pose-transition release, Hyper Beam, and special attack timing.</summary>
    internal BeamFiringResult HandleBeamInput(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects? roomObjects)
    {
        const ControllerButton shoot = ControllerButton.X;
        _charge.PreviousCharge = _charge.Level;

        // Firing gives the Hyper flag priority over Charge equipment. It still uses
        // held Shoot rather than a new edge, with its own 21-frame cooldown preventing a
        // desktop-held button from allocating more frequently than the game.
        if (samus.HyperBeamEnabled)
        {
            (int? slot, SoundEffectId? sound) = (controllerInput & shoot) != ControllerButton.None
                ? TryFireHyperBeam(level, samus, sharedProjectiles, roomObjects)
                : new BeamShot(null, null);
            return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        bool chargeEquipped = samus.EquippedBeams.HasAny(SamusBeamFlags.Charge);
        bool held = (controllerInput & shoot) != ControllerButton.None;
        if (!chargeEquipped)
        {
            _charge.ClearCount();
            _animation.ClearFlareAnimationState();
            (int? slot, SoundEffectId? sound) = held
                ? TryFireBeam(
                    level,
                    samus,
                    controllerNewInput,
                    sharedProjectiles,
                    roomObjects,
                    charged: false)
                : new BeamShot(null, null);
            return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        if (samus.PendingTransitionShot is not null)
        {
            // Charging forces release before testing held Shoot. This is why a
            // charged beam can leave the OLD gun direction while a normal-jump or moonwalk
            // transition installs new body art. Values below 60 release an ordinary beam;
            // values 60+ release the charged family through the same allocation gate.
            bool forcedChargedRelease =
                _charge.HasCharged(BeamChargeDefinitions.FullyChargedCounter);
            _charge.ClearCount();
            _animation.ClearFlareAnimationState();
            return CompleteBeamRelease(samus, TryFireBeam(
                level,
                samus,
                controllerNewInput,
                sharedProjectiles,
                roomObjects,
                charged: forcedChargedRelease));
        }

        if (held)
        {
            // Charging increments through 120 and fires one ordinary shot on the first
            // held frame. Charge Beam therefore changes sustained-fire semantics rather
            // than suppressing the familiar initial power shot.
            if (!_charge.HasCharged(BeamChargeDefinitions.SpecialAttackCounter))
            {
                _charge.FlareCounter++;
                if (_charge.FlareCounter == 1)
                {
                    _animation.ClearFlareAnimationState();
                    (int? slot, SoundEffectId? sound) = TryFireBeam(
                        level,
                        samus,
                        controllerNewInput,
                        sharedProjectiles,
                        roomObjects,
                        charged: false);
                    return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
                }

                // The flare draw owns both flare graphics and their audio. Its draw-time call
                // queues the charging sound exactly once at counter sixteen. The
                // C# renderer is intentionally side-effect free, so queue the same sound
                // here, on the input phase that advances the counter to sixteen.
                if (_charge.FlareCounter == BeamChargeDefinitions.ChargeSoundStartCounter)
                {
                    return new(null, SamusSound.ChargingBeam, Priority: SoundRequestPriority.High);
                }
            }
            else if (_combos.TryActivateCombo(samus, sharedProjectiles, out SoundEffectId? comboSound))
            {
                // The game tries the special beam attack only on the call after charge reaches 120.
                // Successful activation clears flare animation and restores the suit;
                // failed family selection leaves charge intact, including its ammo debit.
                _charge.ClearCount();
                _animation.ClearFlareAnimationState();
                samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
                return new(null, comboSound, SoundRequestPriority.Normal);
            }
            return new(null, null, null);
        }

        if (!_charge.IsCharging)
            return new(null, null, null);

        bool releaseCharged =
            _charge.HasCharged(BeamChargeDefinitions.FullyChargedCounter);
        _charge.ClearCount();
        _animation.ClearFlareAnimationState();
        return CompleteBeamRelease(samus, TryFireBeam(
            level,
            samus,
            controllerNewInput,
            sharedProjectiles,
            roomObjects,
            charged: releaseCharged));
    }

    /// <summary>
    /// Applies the shared tail of uncharged and charged beam firing. A
    /// successful shot's own essential-priority sound replaces the charging sound. If allocation or
    /// muzzle initialization rejects a release after counter sixteen, the game queues
    /// the charge-silencing sound instead so the sustained charge cannot remain audible; that rejection first cancels a
    /// pending resume of the charging sound, as a shot does (#1689).
    /// </summary>
    private BeamFiringResult CompleteBeamRelease(
        SamusState samus,
        BeamShot firing)
    {
        // Charged firing's success path queues the combination's charged sound even when the
        // combination has none. Allocation, not a sound request, distinguishes that success from
        // the shared failure tail which cancels an audible charge. Murder Beam is the observable
        // case: it allocates a persistent projectile while requesting no new sound.
        if (firing.Slot is not null)
            return new(firing.Slot, firing.Sound, Priority: SoundRequestPriority.Essential);

        if (!_charge.PreviousCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
            return new(null, null, null);
        samus.ResumeChargingBeamSoundPending = false;
        return new(null, SamusSound.Silence, Priority: SoundRequestPriority.Essential);
    }


    /// <summary>Attempts allocation and muzzle setup for an ordinary or charged beam shot.</summary>
    internal BeamShot TryFireBeam(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects? roomObjects,
        bool charged)
    {
        const ControllerButton shoot = ControllerButton.X;

        // Both release paths converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, callers attempt a shot every held
        // frame and the beam cooldown supplies the rate limit; charged releases reach
        // the same gate once.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            !sharedProjectiles.CooldownTimer.IsZero)
        {
            return new BeamShot(null, null);
        }

        // Allocation writes one before allocating storage. A corrupt counter/free-slot
        // disagreement is therefore intentionally observable rather than silently repaired.
        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter++;

        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
        {
            // Consistent state cannot arrive here. Match the producer's defensive rollback
            // so an inconsistent count does not stay permanently above five.
            _slotTable.ProjectileCounter--;
            return new BeamShot(null, null);
        }

        SamusProjectileDirection? direction = samus.PendingTransitionShot is { } transitionShot
            ? transitionShot.Aim
            : SamusPoseMetadata.ReadAimDirection(samus);

        if (direction is not { } aim)
        {
            // A pose that cannot shoot fires nothing. A transition shot is never refused by the game, which would fire it
            // with part of its value as the aim and its muzzle read past the origin tables, but no retail transition hands one
            // over: only the normal-jump initializer and the moonwalk transition publish one, and every normal-jump and
            // moonwalk pose's shot direction is one of the ten aims (#1506).
            if (samus.PendingTransitionShot is not null)
                throw new InvalidOperationException(
                    "A transition shot names no aim; no retail transition hands one over.");
            _slotTable.ProjectileCounter--;
            return new BeamShot(null, null);
        }

        // The shot starts from the horizontal sub-pixel position its place hands on (LeftMuzzleSubpixelCarry): the left
        // muzzle check's -1 speed can carry it back into the same whole pixel before the velocity setup clears it.
        SamusProjectile slot = _slotTable.Occupy(slotIndex);
        slot.Direction = aim;
        InitializePosition(samus, slot);
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.Type = SamusProjectileType.Beam(SamusBeamCombinations.Of(samus.EquippedBeams), charged);
        slot.PicturedBeams = slot.Type.BeamCombination;
        ProjectileInvincibilityTimer = 10;
        // The shot cancels a pending resume of the charging sound where it requests its own sound, before the projectile
        // is set up, so the next post-draw sound pass does not resume the charge after it (#1689).
        samus.ResumeChargingBeamSoundPending = false;

        // The combination selects one projectile, not four independent animation layers
        // composed by the host. In particular, Spazer's familiar three streaks are one
        // composition in one projectile slot, so no child projectiles are created here.
        SamusBeamCombination beamType = slot.Type.BeamCombination;

        // The projectile setup selects the combination's projectile kind, stores damage, chooses the
        // direction-specific animation, samples its initial radii, and arms a one-frame timer.
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(beamType, charged);
        slot.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment? segment = SamusProjectileSelection.BeamShotAnimation(beamType, charged, aim);
        slot.Playback = SamusProjectilePlayback.Start(segment);
        slot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        slot.FrameHold = WrappingCountdown.Start(1);

        // The cooldown selection checks Charge equipment and both input-edge latches before
        // selecting ordinary cooldown. Only uncharged auto-fire without those
        // conditions uses the longer held-fire delay.
        BeamShotCooldowns.Shot shot = charged ? BeamShotCooldowns.Shot.Charged
            : samus.EquippedBeams.HasAny(SamusBeamFlags.Charge) || (controllerNewInput & shoot) != ControllerButton.None
                ? BeamShotCooldowns.Shot.Uncharged
                : BeamShotCooldowns.Shot.HeldFire;
        // A Spazer and Plasma combination waits its own cooldowns (UnauthoredBeamFiring, #1506).
        int cooldown = beamType.IsRetail
            ? SamusProjectileCooldowns.Read(beamType, shot)
            : _unauthoredFiring.Cooldown(beamType, shot);
        sharedProjectiles.SetSharedCooldown(cooldown);

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(charged, beamType);
        if (charged)
            _charge.ChargedShotGlow = ChargedShotGlow.ChargedShotFlash;

        // Uncharged and charged firing explicitly zero both speeds and call
        // the directional or Wave beam collision check before installing the frame behaviour and
        // initial speed. This is what lets a muzzle already overlapping a door cap trigger
        // its shot reaction instead of moving its leading edge beyond the cap.
        bool initialImpact = _terrainCollision.RunInitialBeamCollision(
            level,
            slot,
            roomObjects,
            waveBeam: beamType.HasWave);
        if (!initialImpact)
        {
            // The game installs the frame behaviour only after muzzle collision. Do not infer it from
            // the Wave component: each combination names its own, the Spazer and Plasma ones included.
            SamusBeamFrameBehaviorDefinition definition =
                SamusBeamFrameBehaviors.Resolve(charged, beamType);
            // The frame behaviours with no model are Spazer and Plasma combinations that run the game's live state
            // or an unconnected region instead of the game's code (UnauthoredBeamFrameBehaviors). Only full memory
            // and processor emulation could reproduce that, so the model rejects the shot under the emulation scope
            // limit (#1500).
            slot.FrameBehavior = definition.FrameBehavior ?? throw new NotSupportedException(
                $"Beam frame behaviour {definition.Choice} runs whatever the game's live state or an unconnected region " +
                "holds; reproducing that needs full memory and processor emulation, which is outside the emulation " +
                "scope limit (#1500).");
            InitializePowerBeamVelocity(samus.ProjectileInheritance, slot, _unauthoredFiring);
        }
        return new BeamShot(slotIndex, sound);
    }

    /// <summary>Allocates charged-Plasma art with Hyper damage, Wave collision, and a 21-frame cooldown.</summary>
    internal BeamShot TryFireHyperBeam(
        RoomLevelData level,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects? roomObjects)
    {
        // Allocation is shared with normal beams: five counted shots and a running cooldown reject firing before any
        // projectile is created.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            !sharedProjectiles.CooldownTimer.IsZero)
        {
            return new BeamShot(null, null);
        }

        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter++;
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
        {
            _slotTable.ProjectileCounter--;
            return new BeamShot(null, null);
        }

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
        {
            _slotTable.ProjectileCounter--;
            return new BeamShot(null, null);
        }

        // As for ordinary beams, the shot starts from the sub-pixel position its place hands on (LeftMuzzleSubpixelCarry).
        SamusProjectile slot = _slotTable.Occupy(slotIndex);
        slot.Direction = aim;

        InitializePosition(samus, slot);
        ProjectileInvincibilityTimer = 10;
        // As for ordinary beams, the shot cancels a pending resume of the charging sound where it requests its own (#1689).
        samus.ResumeChargingBeamSoundPending = false;

        // The Hyper Beam is a charged Plasma beam for its projectile data and animation even though
        // acquisition equips Wave and Plasma, whose pictures its shots show. It first reads the charged
        // Plasma entry; Hyper's setup then deliberately replaces its damage with 1000 before the
        // first animation step runs.
        slot.Type = SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true);
        slot.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        const SamusBeamCombination hyperBeamType = SamusBeamCombination.Plasma;
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(hyperBeamType, charged: true);
        slot.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, slot.Direction);
        slot.Playback = SamusProjectilePlayback.Start(segment);
        slot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Damage = 1000;

        // Hyper uses the Wave beam collision check at this same producer seam. It publishes door and
        // shootable-block reactions but, like every Wave family, is not stopped by solid blocks and
        // survives to receive its normal movement state.
        _ = _terrainCollision.RunInitialBeamCollision(
            level,
            slot,
            roomObjects,
            waveBeam: true);
        slot.FrameBehavior = SamusProjectileFrameBehavior.HyperBeam;
        InitializePowerBeamVelocity(samus.ProjectileInheritance, slot, _unauthoredFiring);

        // The shot starts the Hyper Beam's own suit glow, and its flare in place of the charge.
        sharedProjectiles.SetSharedCooldown(21);
        _charge.ChargedShotGlow = ChargedShotGlow.HyperBeamGlow;
        _charge.StartHyperBeamFlare();

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(
            charged: true, hyperBeamType);
        return new BeamShot(slotIndex, sound);
    }

    /// <summary>Consumes a current or retained Shoot edge, ammo, and cooldown to launch a missile.</summary>
    internal BeamShot TryFireMissile(
        SamusState samus,
        ControllerButton controllerNewInput,
        ControllerButton controllerPreviousNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const ControllerButton shoot = ControllerButton.X;

        // The fire test accepts either the current new-press input or the previous filtered
        // drawing input. Live gameplay retains that edge through the next input phase, allowing
        // a press one frame before cooldown expires to fire on the following frame.
        // Demo playback supplies the analogous scripted edge. Cooldown still
        // owns admission, so the delayed copy cannot manufacture held-button auto-fire.
        if (((controllerNewInput | controllerPreviousNewInput) & shoot) == ControllerButton.None)
            return new BeamShot(null, null);

        // Admission reserves room for a Super Missile's later collision link. Ammo/free-
        // slot rejection rolls back the count, but does not undo its cooldown write.
        bool isSuperMissile = samus.SelectedHudItem == HudItem.SuperMissile;
        int ammo = isSuperMissile ? samus.SuperMissiles : samus.Missiles;
        int admissionLimit = isSuperMissile ? SuperMissileDefinitions.AdmissionLimit : SamusProjectileSystem.SlotCount;
        if (_slotTable.ProjectileCounter >= admissionLimit ||
            !sharedProjectiles.CooldownTimer.IsZero)
        {
            return new BeamShot(null, null);
        }

        sharedProjectiles.SetSharedCooldown(1);
        if (ammo == 0)
            return new BeamShot(null, null);

        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            return new BeamShot(null, null);

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
            return new BeamShot(null, null);
        SamusProjectile slot = _slotTable.Occupy(slotIndex);
        slot.Direction = aim;

        InitializePosition(samus, slot);
        _slotTable.ProjectileCounter++;
        ProjectileInvincibilityTimer = 20;
        if (isSuperMissile)
            samus.SuperMissiles = CountsRunBelowZero.DecrementWrapping(samus.SuperMissiles);
        else
            samus.Missiles = CountsRunBelowZero.DecrementWrapping(samus.Missiles);
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.Type = SamusProjectileType.Of(isSuperMissile ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile);
        slot.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        slot.MissileIgnition = SamusMissileIgnition.Pending;
        if (isSuperMissile)
            slot.SuperMissileLinkPlace = UnlinkedSuperMissileLink.Place;

        // The producer first calls the generic velocity initializer with base speed zero.
        // Ignition replaces that zero with one pixel per update during this same frame's alpha pass;
        // keeping both calls makes the one-frame ignition transition directly inspectable.
        InitializeDirectionalVelocity(samus.ProjectileInheritance, slot, baseSpeed: SignedEightEight.Zero);

        // The selected HUD item chose the missile family above; the family selects the projectile kind.
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(slot.Type.Family);
        slot.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, slot.Direction);
        slot.Playback = SamusProjectilePlayback.Start(segment);
        slot.Radius = SamusProjectilePlayback.InitialRadius(segment);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.FrameBehavior = isSuperMissile
            ? SamusProjectileFrameBehavior.SuperMissile
            : SamusProjectileFrameBehavior.Missile;

        // The missile sound and the ten-frame shared cooldown. A Select press made while Item Cancel
        // is held records a one-shot auto-cancel index; a successful missile consumes that
        // request before the ordinary empty-ammo fallback. Murder Beam uses this exact path
        // to begin charging safely while the missile cooldown suppresses its unsafe initial
        // uncharged shot.
        sharedProjectiles.SetSharedCooldown(isSuperMissile ? 20 : 10);
        if (samus.AutoCancelledItem != HudItem.Beam)
        {
            samus.SelectedHudItem = HudItem.Beam;
            samus.AutoCancelledItem = HudItem.Beam;
        }
        else if ((isSuperMissile ? samus.SuperMissiles : samus.Missiles) == 0)
            samus.SelectedHudItem = HudItem.Beam;
        return new BeamShot(slotIndex, isSuperMissile
            ? SamusProjectileSoundRoutingDefinitions.SuperMissileRequest
            : SamusProjectileSoundRoutingDefinitions.MissileRequest);
    }

    /// <summary>Places the muzzle using pose height and the running or ordinary directional origin table.</summary>
    internal static void InitializePosition(
        SamusState samus,
        SamusProjectile slot)
    {
        int poseYOffset = SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        // The muzzle origin uses the running/moonwalk origin table for SamusMovementType.Running and for
        // the two diagonally-up moonwalk MoonwalkAimUpLeftPose and MoonwalkAimUpRightPose. Every other pose uses the default.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
        var origin = SamusMuzzleOrigins.Read(runningOrigin, slot.Direction);
        slot.Position = slot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + origin.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + origin.Y - poseYOffset)));
    }

    /// <summary>
    /// Selects beam speed from its combination and diagonal direction before applying inherited velocity. Every retail
    /// combination launches at the same authored pair; a Spazer and Plasma combination launches at the pair
    /// <paramref name="unauthoredFiring"/> decoded for it.
    /// </summary>
    internal static void InitializePowerBeamVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectile slot,
        UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        Ensure.NotNull(unauthoredFiring);
        bool diagonal = slot.Direction is SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft;
        SamusBeamCombination beams = slot.Type.BeamCombination;
        SignedEightEight authoredSpeed = diagonal ? SamusProjectileMotionDefinitions.DiagonalSpeed : SamusProjectileMotionDefinitions.CardinalSpeed;
        // An unauthored combination's speed is its table's 1/256-pixel speed, which the launch adds as a wrapping velocity.
        SignedEightEight speed = beams.IsRetail ? authoredSpeed : SignedEightEight.Wrapped(unauthoredFiring.Speed(beams, diagonal));

        InitializeDirectionalVelocity(inheritance, slot, speed);
    }

    /// <summary>Clears projectile position fractions and combines base speed with Samus' inherited motion.</summary>
    internal static void InitializeDirectionalVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectile slot,
        SignedEightEight baseSpeed)
    {
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.Velocity = LaunchVelocity(inheritance, slot.Direction, baseSpeed);
    }

    /// <summary>
    /// The launch velocity of a shot fired in <paramref name="direction"/> at <paramref name="baseSpeed"/>, in signed 1/256
    /// pixels per update: the base speed along the direction plus the motion it inherits from Samus. Each axis wraps as
    /// a 1/256-pixel velocity, so an unauthored speed that passes the end of the range continues from the other end.
    /// </summary>
    internal static EightEightVector<RoomPixels> LaunchVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectileDirection direction,
        SignedEightEight baseSpeed)
    {
        Ensure.NotNull(inheritance);
        SignedEightEight x = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight =>
                inheritance.InheritedVelocity(HorizontalDirection.Right) + baseSpeed,
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft =>
                inheritance.InheritedVelocity(HorizontalDirection.Left) - baseSpeed,
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        };
        SignedEightEight y = direction switch
        {
            // The rising launch velocity and the base speed combine as wrapping velocities.
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or
                SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft =>
                inheritance.RisingLaunchVelocity() - baseSpeed,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.DownLeft =>
                inheritance.InheritedVelocity(VerticalDirection.Down) + baseSpeed,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        };
        return new(x, y);
    }

    /// <summary>
    /// The interaction-immunity timer; humanoid shots temporarily suppress Samus/projectile
    /// interaction, including bomb jumps. Decremented by the shared gameplay tail.
    /// </summary>
    public int ProjectileInvincibilityTimer { get; internal set; }

    /// <summary>Decrements the projectile-owned timer of Samus's timer countdown.</summary>
    public void DecrementInteractionTimer()
    {
        ProjectileInvincibilityTimer = Math.Max(0, ProjectileInvincibilityTimer - 1);
    }
}
