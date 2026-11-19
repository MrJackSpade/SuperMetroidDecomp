using SuperMetroid.Core.Samus.Colours;
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
    private readonly SamusShotPool _pool;
    /// <summary>Runs initial muzzle collision against room terrain and room objects.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>The speeds a Spazer and Plasma beam launches at and the cooldowns it waits.</summary>
    private readonly UnauthoredBeamFiring<SamusBeamCombination> _unauthoredFiring;

    /// <summary>Connects firing to charge, animation, projectile allocation, combo, and terrain systems.</summary>
    internal BeamFiring(ProjectileAnimation animation, BeamChargeState charge, BeamCombos combos, SamusShotPool pool,
        ProjectileTerrainCollision terrainCollision, UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        _animation = animation;
        _charge = charge;
        _combos = combos;
        _pool = pool;
        _terrainCollision = terrainCollision;
        _unauthoredFiring = Ensure.NotNull(unauthoredFiring);
    }
    /// <summary>
    /// The scripted charge end: consume Pseudo Screw charge,
    /// clear its visible flare and restore the normal suit colours. Unlike HUD
    /// cancellation, this does not rewrite the previous-charge sample.
    /// </summary>
    public void ConsumePseudoScrewCharge(SamusColours colours, SamusState samus)
    {
        _charge.RestartChargePulse();
        _charge.ClearCount();
        _animation.ClearFlareAnimationState();
        ProjectileAnimation.ShowSuitShade(colours, samus);
    }

    /// <summary>
    /// Clears the charge, its prior sample, and all flare animation layers: every HUD weapon selection does so before the
    /// newly selected weapon producer runs, and so do the Morph Ball's charge cancellation and a completed bomb spread.
    /// </summary>
    public void CancelCharge()
    {
        _charge.ClearCount();
        _charge.PreviousCharge = BeamChargeLevel.None;
        _animation.ClearFlareAnimationState();
    }

    /// <summary>Handles held Shoot, release, pose-transition release, Hyper Beam, and special attack timing.</summary>
    internal BeamFiringResult HandleBeamInput(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        GameplayAction controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        const GameplayAction shoot = GameplayAction.Shoot;
        _charge.PreviousCharge = _charge.Level;

        // Firing gives the Hyper Beam priority over Charge equipment. It fires on held Shoot rather than a new press,
        // and its own 21-frame cooldown paces a held button.
        if (samus.HyperBeamEnabled)
        {
            SoundEffectId? sound = (controllerInput & shoot) != GameplayAction.None
                ? TryFireHyperBeam(level, samus, sharedProjectiles, roomObjects)?.Sound
                : null;
            return new(sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        bool chargeEquipped = samus.EquippedBeams.HasAny(SamusBeamFlags.Charge);
        bool held = (controllerInput & shoot) != GameplayAction.None;
        if (!chargeEquipped)
        {
            _charge.ClearCount();
            _animation.ClearFlareAnimationState();
            SoundEffectId? sound = held
                ? TryFireBeam(
                    level,
                    samus,
                    controllerNewInput,
                    sharedProjectiles,
                    roomObjects,
                    charged: false)?.Sound
                : null;
            return new(sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        if (samus.PendingTransitionShot is not null)
        {
            // Charging forces release before testing held Shoot. This is why a
            // charged beam can leave the OLD gun direction while a normal-jump or moonwalk
            // transition changes her pose. Values below 60 release an ordinary beam;
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
                    SoundEffectId? sound = TryFireBeam(
                        level,
                        samus,
                        controllerNewInput,
                        sharedProjectiles,
                        roomObjects,
                        charged: false)?.Sound;
                    return new(sound, sound is null ? null : SoundRequestPriority.Essential);
                }

                // The charging sound starts once, on the input phase that advances the flare count to
                // BeamChargeDefinitions.ChargeSoundStartCounter; drawing the flare queues nothing.
                if (_charge.FlareCounter == BeamChargeDefinitions.ChargeSoundStartCounter)
                {
                    return new(SamusSound.ChargingBeam, Priority: SoundRequestPriority.High);
                }
            }
            else if (_combos.TryActivateCombo(samus, sharedProjectiles, out SoundEffectId? comboSound))
            {
                // The game tries the special beam attack only on the update after charge reaches 120.
                // Successful activation clears flare animation and restores the suit;
                // failed family selection leaves charge intact, including its ammo debit.
                _charge.ClearCount();
                _animation.ClearFlareAnimationState();
                samus.HorizontalSpeed.RequestSuitShadeRestore();
                return new(comboSound, SoundRequestPriority.Normal);
            }
            return new(null, null);
        }

        if (!_charge.IsCharging)
            return new(null, null);

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
    /// Finishes firing a beam, uncharged or charged. A
    /// successful shot's own essential-priority sound replaces the charging sound. If admission or
    /// muzzle initialization rejects a release after counter sixteen, the game queues
    /// the charge-silencing sound instead so the sustained charge cannot remain audible; that rejection first cancels a
    /// pending resume of the charging sound, as a shot does (#1689).
    /// </summary>
    private BeamFiringResult CompleteBeamRelease(
        SamusState samus,
        BeamShot? firing)
    {
        // Charged firing's success path queues the combination's charged sound even when the
        // combination has none. Allocation, not a sound request, distinguishes that success from
        // a failed release, which cancels an audible charge. Murder Beam is the observable
        // case: it allocates a persistent projectile while requesting no new sound.
        if (firing is { } fired)
            return new(fired.Sound, Priority: SoundRequestPriority.Essential);

        if (!_charge.PreviousCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
            return new(null, null);
        samus.ResumeChargingBeamSoundPending = false;
        return new(SamusSound.Silence, Priority: SoundRequestPriority.Essential);
    }

    /// <summary>Attempts allocation and muzzle setup for an ordinary or charged beam shot.</summary>
    internal BeamShot? TryFireBeam(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects,
        bool charged)
    {
        const GameplayAction shoot = GameplayAction.Shoot;

        // Both release paths converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, Samus attempts a shot every held
        // frame and the beam cooldown supplies the rate limit; charged releases reach
        // the same gate once.
        if (_pool.ShotCount.Count >= SamusProjectileSystem.PlaceCount ||
            sharedProjectiles.CooldownTimer != 0)
        {
            return null;
        }

        // A fire attempt sets the shared cooldown before it takes a place, so a shot count that disagrees
        // with the free places shows rather than being repaired.
        sharedProjectiles.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        _pool.ShotCount.CountShot();

        // The shot count can read lower than the places held: a special attack sets it to its particle count whatever
        // the fifth place holds. With every place held the shot takes the last place over the shot there (#2468).
        int place = _pool.PlaceForBeam();

        SamusProjectileDirection? direction = samus.PendingTransitionShot is { } transitionShot
            ? transitionShot.Aim
            : SamusPoseMetadata.ReadAimDirection(samus);

        if (direction is not { } aim)
        {
            // A pose that cannot shoot fires nothing; a transition shot always names an aim (SamusTransitionShot.FromPose).
            _pool.ShotCount.TakeBackRefusedShot();
            return null;
        }

        // The shot starts from the horizontal sub-pixel position its place hands on (LeftMuzzleSubpixelCarry): the left
        // muzzle check's -1 speed can carry it back into the same whole pixel before the velocity setup clears it. Over a
        // held place it also starts from the Power Bomb fuse the shot there leaves (BeamFiredOverLastShot).
        SamusProjectile projectile = _pool.OccupyWithBeam(place);
        projectile.Direction = aim;
        InitializePosition(samus, projectile);
        projectile.TrailUpdatesLeft = ProjectileTimingDefinitions.TrailInterval;
        projectile.Type = SamusProjectileType.Beam(SamusBeamCombinations.Of(samus.EquippedBeams), charged);
        projectile.PicturedBeams = projectile.Type.BeamCombination;
        ProjectileInvincibilityTimer = ProjectileTimingDefinitions.BeamInteractionImmunity;
        // The shot cancels a pending resume of the charging sound where it requests its own sound, before the projectile
        // is set up, so the next post-draw sound pass does not resume the charge after it (#1689).
        samus.ResumeChargingBeamSoundPending = false;

        // The combination selects one projectile: Spazer's three streaks are one picture of one projectile, so no
        // further projectiles are created here.
        SamusBeamCombination beamType = projectile.Type.BeamCombination;

        // The projectile setup selects the combination's projectile kind, stores damage, chooses the
        // direction-specific animation, samples its initial radii, and arms a one-frame timer.
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(beamType, charged);
        projectile.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment? segment = SamusProjectileSelection.BeamShotAnimation(beamType, charged, aim);
        projectile.Animation = SamusProjectilePlayback.Start(segment);
        projectile.Radius = SamusProjectilePlayback.InitialRadius(segment);

        // The cooldown selection checks Charge equipment and both input-edge latches before
        // selecting ordinary cooldown. Only uncharged auto-fire without those
        // conditions uses the longer held-fire delay.
        BeamShotCooldowns.Shot shot = charged ? BeamShotCooldowns.Shot.Charged
            : samus.EquippedBeams.HasAny(SamusBeamFlags.Charge) || (controllerNewInput & shoot) != GameplayAction.None
                ? BeamShotCooldowns.Shot.Uncharged
                : BeamShotCooldowns.Shot.HeldFire;
        // A Spazer and Plasma combination waits its own cooldowns (UnauthoredBeamFiring, #1506).
        int cooldown = beamType.CanBeEquipped
            ? SamusProjectileCooldowns.Read(beamType, shot)
            : _unauthoredFiring.Cooldown(beamType, shot);
        sharedProjectiles.SetSharedCooldown(cooldown);

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(charged, beamType);
        if (charged)
            _charge.ChargedShotGlow = ChargedShotGlow.StartChargedShotFlash();

        // Uncharged and charged firing explicitly zero both speeds and call
        // the directional or Wave beam collision check before installing the shot's behaviour and
        // initial speed. This is what lets a muzzle already overlapping a door cap trigger
        // its shot reaction instead of moving its leading edge beyond the cap.
        bool initialImpact = _terrainCollision.RunInitialBeamCollision(
            level,
            projectile,
            roomObjects,
            waveBeam: beamType.HasWave);
        if (!initialImpact)
        {
            // The game starts the shot's behaviour only after muzzle collision. Do not infer it from the Wave component:
            // each combination names its own, the Spazer and Plasma ones included. A Spazer and Plasma combination
            // whose shots run the game's live state or an unconnected region instead of the game's code
            // (UnauthoredBeamShotBehaviors) is refused here: only full memory and processor emulation could reproduce
            // that, which is outside the emulation scope limit (#1500).
            SamusShotBehavior behavior = SamusBeamShotBehaviors.Resolve(charged, beamType);
            behavior.AdmitFiring();
            projectile.Install(behavior, kindState: null);
            InitializePowerBeamVelocity(samus.ProjectileInheritance, projectile, _unauthoredFiring);
        }
        return new BeamShot(sound);
    }

    /// <summary>Allocates charged-Plasma art with Hyper damage, Wave collision, and a 21-frame cooldown.</summary>
    internal BeamShot? TryFireHyperBeam(
        RoomLevelData level,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        // Admission is shared with normal beams: five counted shots and a running cooldown reject firing before any
        // projectile is created.
        if (_pool.ShotCount.Count >= SamusProjectileSystem.PlaceCount ||
            sharedProjectiles.CooldownTimer != 0)
        {
            return null;
        }

        sharedProjectiles.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        _pool.ShotCount.CountShot();
        // As for ordinary beams, with every place held the shot takes the last place over the shot there (#2468).
        int place = _pool.PlaceForBeam();

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
        {
            _pool.ShotCount.TakeBackRefusedShot();
            return null;
        }

        // As for ordinary beams, the shot starts from the sub-pixel position its place hands on (LeftMuzzleSubpixelCarry)
        // and the Power Bomb fuse a shot there leaves (BeamFiredOverLastShot).
        SamusProjectile projectile = _pool.OccupyWithBeam(place);
        projectile.Direction = aim;

        InitializePosition(samus, projectile);
        ProjectileInvincibilityTimer = ProjectileTimingDefinitions.BeamInteractionImmunity;
        // As for ordinary beams, the shot cancels a pending resume of the charging sound where it requests its own (#1689).
        samus.ResumeChargingBeamSoundPending = false;

        // The Hyper Beam is a charged Plasma beam for its projectile data and animation even though
        // acquisition equips Wave and Plasma, whose pictures its shots show; its damage is its own.
        projectile.Type = SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true);
        projectile.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        const SamusBeamCombination hyperBeamType = SamusBeamCombination.Plasma;
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(hyperBeamType, charged: true);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, projectile.Direction);
        projectile.Animation = SamusProjectilePlayback.Start(segment);
        projectile.Radius = SamusProjectilePlayback.InitialRadius(segment);
        projectile.Damage = SamusProjectileDamageDefinitions.HyperBeamDamage;

        // Hyper uses the Wave beam collision check at this same producer seam. It publishes door and
        // shootable-block reactions but, like every Wave family, is not stopped by solid blocks and
        // survives to receive its normal movement state.
        _ = _terrainCollision.RunInitialBeamCollision(
            level,
            projectile,
            roomObjects,
            waveBeam: true);
        projectile.Install(SamusShotBehaviors.HyperBeam, kindState: null);
        InitializePowerBeamVelocity(samus.ProjectileInheritance, projectile, _unauthoredFiring);

        // The shot starts the Hyper Beam's own suit glow, and its flare in place of the charge.
        sharedProjectiles.SetSharedCooldown(SamusProjectileCooldownDefinitions.HyperBeamShot);
        _charge.ChargedShotGlow = ChargedShotGlow.StartHyperBeamGlow();
        _charge.StartHyperBeamFlare();

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(
            charged: true, hyperBeamType);
        return new BeamShot(sound);
    }

    /// <summary>Consumes a current or retained Shoot edge, ammo, and cooldown to launch a missile.</summary>
    internal BeamShot? TryFireMissile(
        SamusState samus,
        GameplayAction controllerNewInput,
        GameplayAction controllerPreviousNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const GameplayAction shoot = GameplayAction.Shoot;

        // The fire test accepts either the current new-press input or the previous filtered
        // drawing input. Live gameplay retains that edge through the next input phase, allowing
        // a press one frame before cooldown expires to fire on the following frame.
        // Demo playback supplies the analogous scripted edge. Cooldown still
        // owns admission, so the delayed press cannot manufacture held-button auto-fire.
        if (((controllerNewInput | controllerPreviousNewInput) & shoot) == GameplayAction.None)
            return null;

        // Admission reserves room for a Super Missile's later collision link. Ammo/free-
        // projectile rejection rolls back the count, but does not undo its cooldown write.
        bool isSuperMissile = samus.SelectedHudItem == HudItem.SuperMissile;
        int ammo = isSuperMissile ? samus.SuperMissiles : samus.Missiles;
        int admissionLimit = isSuperMissile ? SuperMissileDefinitions.AdmissionLimit : SamusProjectileSystem.PlaceCount;
        if (_pool.ShotCount.Count >= admissionLimit ||
            sharedProjectiles.CooldownTimer != 0)
        {
            return null;
        }

        sharedProjectiles.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        if (ammo == 0)
            return null;

        if (_pool.FindFreePlace() is not { } place)
            return null;

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
            return null;
        SamusProjectile projectile = _pool.Occupy(place);
        projectile.Direction = aim;

        InitializePosition(samus, projectile);
        _pool.ShotCount.CountShot();
        ProjectileInvincibilityTimer = ProjectileTimingDefinitions.MissileInteractionImmunity;
        // A count Crystal Flash drained below zero still fires, each shot taking it further below; only the 32768th shot
        // from below zero wraps it to the largest count (CountsRunBelowZero), a wrap play can reach.
        if (isSuperMissile)
            samus.SuperMissiles = CountsRunBelowZero.DecrementWrapping(samus.SuperMissiles);
        else
            samus.Missiles = CountsRunBelowZero.DecrementWrapping(samus.Missiles);
        projectile.TrailUpdatesLeft = ProjectileTimingDefinitions.TrailInterval;
        projectile.Type = SamusProjectileType.Of(isSuperMissile ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile);
        projectile.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);

        // The missile's velocity is set when it ignites, on its first update in this same input phase
        // (ProjectileMotion.IgniteMissile), before anything reads it.
        // The selected HUD item chose the missile family above; the family selects the projectile kind.
        SamusProjectileKind kind = SamusProjectileSelection.NonBeam(projectile.Type.Family);
        projectile.Damage = SamusProjectileDamage.Read(kind);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, projectile.Direction);
        projectile.Animation = SamusProjectilePlayback.Start(segment);
        projectile.Radius = SamusProjectilePlayback.InitialRadius(segment);
        if (isSuperMissile)
        {
            projectile.Install(SamusShotBehaviors.SuperMissile,
                new SuperMissileShotState(SamusMissileIgnition.Pending, UnlinkedSuperMissileLink.Place));
        }
        else
            projectile.Install(SamusShotBehaviors.Missile, new MissileShotState(SamusMissileIgnition.Pending));

        // The missile sound and the missile's shared cooldown. An item-selection press made while item cancel
        // is held asks for the selection to return to the beam after one shot; a successful
        // missile returns it, ending that request, before the ordinary empty-ammo fallback. Murder Beam uses this exact path
        // to begin charging safely while the missile cooldown suppresses its unsafe initial
        // uncharged shot.
        sharedProjectiles.SetSharedCooldown(isSuperMissile
            ? SamusProjectileCooldownDefinitions.SuperMissileShot
            : SamusProjectileCooldownDefinitions.MissileShot);
        if (samus.AutoCancelledItem != HudItem.Beam)
        {
            samus.SelectedHudItem = HudItem.Beam;
            samus.AutoCancelledItem = HudItem.Beam;
        }
        else if ((isSuperMissile ? samus.SuperMissiles : samus.Missiles) == 0)
            samus.SelectedHudItem = HudItem.Beam;
        return new BeamShot(isSuperMissile
            ? SamusProjectileSoundRoutingDefinitions.SuperMissileRequest
            : SamusProjectileSoundRoutingDefinitions.MissileRequest);
    }

    /// <summary>Places the muzzle at the pose's projectile height and the running or ordinary origin of its direction (<see cref="SamusMuzzleOrigins"/>).</summary>
    internal static void InitializePosition(
        SamusState samus,
        SamusProjectile projectile)
    {
        int poseYOffset = SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        // Running and the two moonwalks aiming diagonally up take the running origins; every other pose takes the
        // ordinary ones.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
        var origin = SamusMuzzleOrigins.Read(runningOrigin, projectile.Direction);
        // Samus's centre lies at least 21 pixels from the room's left edge, which every row she reaches closes with terrain
        // or a door, and no muzzle lies more than 19 left of her, so the muzzle lies in the room across. A room open at the
        // top (the Landing Site's sky) lets her rise until her top meets the room's, and a shot aimed up then leaves the
        // muzzle above the room's top, a height CoordinateAxis.Wrap gives it.
        projectile.Position = projectile.Position.WithWhole(new(samus.Position.X.Whole + origin.X, CoordinateAxis.Wrap(samus.Position.Y.Whole + origin.Y - poseYOffset)));
    }

    /// <summary>
    /// Selects beam speed from its combination and diagonal direction before applying inherited velocity. Every
    /// combination launches at the same authored pair; a Spazer and Plasma combination launches at the pair
    /// <paramref name="unauthoredFiring"/> decoded for it.
    /// </summary>
    internal static void InitializePowerBeamVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectile projectile,
        UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        Ensure.NotNull(unauthoredFiring);
        bool diagonal = projectile.Direction is SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft;
        SamusBeamCombination beams = projectile.Type.BeamCombination;
        SignedEightEight authoredSpeed = diagonal ? SamusProjectileMotionDefinitions.DiagonalSpeed : SamusProjectileMotionDefinitions.CardinalSpeed;
        // An unauthored combination's speed is the one its unauthored firing gives, which the launch adds as a wrapping velocity.
        SignedEightEight speed = beams.CanBeEquipped ? authoredSpeed : SignedEightEight.Wrapped(unauthoredFiring.Speed(beams, diagonal));

        InitializeDirectionalVelocity(inheritance, projectile, speed);
    }

    /// <summary>Clears projectile position fractions and combines base speed with Samus' inherited motion.</summary>
    internal static void InitializeDirectionalVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectile projectile,
        SignedEightEight baseSpeed)
    {
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.Velocity = LaunchVelocity(inheritance, projectile.Direction, baseSpeed);
    }

    /// <summary>
    /// The launch velocity of a shot fired in <paramref name="direction"/> at <paramref name="baseSpeed"/>, in signed 1/256
    /// pixels per update: the base speed along the direction plus the motion it inherits from Samus. Each axis wraps as
    /// a velocity does, so an unauthored speed that passes the end of the range continues from the other end.
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
    /// interaction, including bomb jumps. Counted down once each gameplay update, after Samus and the enemies have moved.
    /// </summary>
    public int ProjectileInvincibilityTimer { get; internal set; }

    /// <summary>Counts the projectile invincibility down by one update, stopping at zero.</summary>
    public void DecrementInteractionTimer()
    {
        ProjectileInvincibilityTimer = Math.Max(0, ProjectileInvincibilityTimer - 1);
    }
}
