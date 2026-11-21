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
    /// <summary>Tracks held Shoot duration, charge effects, the charge flare and Hyper Beam glow.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Handles the special beam attack after the charge counter reaches <see cref="BeamChargeDefinitions.SpecialAttackCounter"/>.</summary>
    private readonly BeamCombos _combos;
    /// <summary>Owns the shot places (<see cref="SamusProjectileSystem.PlaceCount"/>), which a fired projectile takes, and the shot count.</summary>
    private readonly SamusShotPool _pool;
    /// <summary>Runs initial muzzle collision against room terrain and room objects.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>The speeds a Spazer and Plasma beam launches at and the cooldowns it waits.</summary>
    private readonly UnauthoredBeamFiring<SamusBeamCombination> _unauthoredFiring;

    /// <summary>Connects firing to charge, projectile allocation, combo, and terrain systems.</summary>
    internal BeamFiring(BeamChargeState charge, BeamCombos combos, SamusShotPool pool,
        ProjectileTerrainCollision terrainCollision, UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
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
        _charge.ClearFlare();
        SamusSuitShade.Show(colours, samus.EquippedItems);
    }

    /// <summary>
    /// Clears the charge, its prior sample, and all flare animation layers: every HUD weapon selection does so before the
    /// newly selected weapon producer runs, and so do the Morph Ball's charge cancellation and a completed bomb spread.
    /// </summary>
    public void CancelCharge()
    {
        _charge.ClearCount();
        _charge.PreviousCharge = BeamChargeLevel.None;
        _charge.ClearFlare();
    }

    /// <summary>Handles held Shoot, release, pose-transition release, Hyper Beam, and special attack timing.</summary>
    internal BeamFiringResult HandleBeamInput(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        GameplayAction controllerNewInput,
        SamusBombProjectileSystem samusBombs)
    {
        const GameplayAction shoot = GameplayAction.Shoot;
        _charge.PreviousCharge = _charge.Level;

        // Firing gives the Hyper Beam priority over Charge equipment. It fires on held Shoot rather than a new press,
        // and its own cooldown (SamusProjectileCooldownDefinitions.HyperBeamShot) paces a held button.
        if (samus.HyperBeamEnabled)
        {
            SoundEffectId? sound = (controllerInput & shoot) != GameplayAction.None
                ? TryFireHyperBeam(terrain, samus, samusBombs)?.Sound
                : null;
            return BeamFiringResult.Of(sound, SoundRequestPriority.Essential);
        }

        bool chargeEquipped = samus.EquippedBeams.HasAny(SamusBeamFlags.Charge);
        bool held = (controllerInput & shoot) != GameplayAction.None;
        if (!chargeEquipped)
        {
            _charge.ClearCount();
            _charge.ClearFlare();
            SoundEffectId? sound = held
                ? TryFireBeam(
                    terrain,
                    samus,
                    controllerNewInput,
                    samusBombs,
                    charged: false)?.Sound
                : null;
            return BeamFiringResult.Of(sound, SoundRequestPriority.Essential);
        }

        if (samus.TransitionShot.Handed is not null)
        {
            // Charging forces release before testing held Shoot. This is why a
            // charged beam can leave the OLD gun direction while a normal-jump or moonwalk
            // transition changes her pose. A charge short of full releases an ordinary beam;
            // a full charge releases the charged family through the same allocation gate.
            bool forcedChargedRelease =
                _charge.HasCharged(BeamChargeDefinitions.FullyChargedCounter);
            _charge.ClearCount();
            _charge.ClearFlare();
            return CompleteBeamRelease(samus, TryFireBeam(
                terrain,
                samus,
                controllerNewInput,
                samusBombs,
                charged: forcedChargedRelease));
        }

        if (held)
        {
            // Charging counts up to the special-attack clamp and fires one ordinary shot on the first
            // held frame. Charge Beam therefore changes sustained-fire semantics rather
            // than suppressing the familiar initial power shot.
            if (!_charge.HasCharged(BeamChargeDefinitions.SpecialAttackCounter))
            {
                _charge.CountChargeUpdate();
                if (_charge.UpdatesHeld == 1)
                {
                    _charge.ClearFlare();
                    SoundEffectId? sound = TryFireBeam(
                        terrain,
                        samus,
                        controllerNewInput,
                        samusBombs,
                        charged: false)?.Sound;
                    return BeamFiringResult.Of(sound, SoundRequestPriority.Essential);
                }

                // The charging sound starts once, on the input phase that advances the flare count to
                // BeamChargeDefinitions.ChargeSoundStartCounter; drawing the flare queues nothing.
                if (_charge.UpdatesHeld == BeamChargeDefinitions.ChargeSoundStartCounter)
                {
                    return BeamFiringResult.Of(SamusSound.ChargingBeam, SoundRequestPriority.High);
                }
            }
            else if (_combos.TryActivateCombo(samus, samusBombs, out SoundEffectId? comboSound))
            {
                // The game tries the special beam attack only on the update after the charge reaches its clamp.
                // Successful activation clears flare animation and restores the suit;
                // failed family selection leaves charge intact, including its ammo debit.
                _charge.ClearCount();
                _charge.ClearFlare();
                samus.HorizontalSpeed.ShowSuitShadeAtColourStep();
                return BeamFiringResult.Of(comboSound, SoundRequestPriority.Normal);
            }
            return BeamFiringResult.Silent;
        }

        if (!_charge.IsCharging)
            return BeamFiringResult.Silent;

        bool releaseCharged =
            _charge.HasCharged(BeamChargeDefinitions.FullyChargedCounter);
        _charge.ClearCount();
        _charge.ClearFlare();
        return CompleteBeamRelease(samus, TryFireBeam(
            terrain,
            samus,
            controllerNewInput,
            samusBombs,
            charged: releaseCharged));
    }

    /// <summary>
    /// Finishes firing a beam, uncharged or charged. A
    /// successful shot's own essential-priority sound replaces the charging sound. If admission or
    /// muzzle initialization rejects a release once the charging sound has started
    /// (<see cref="BeamChargeDefinitions.ChargeSoundStartCounter"/>), the game queues
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
            return BeamFiringResult.Of(fired.Sound, SoundRequestPriority.Essential);

        if (!_charge.PreviousCharge.HasCharged(BeamChargeDefinitions.ChargeSoundStartCounter))
            return BeamFiringResult.Silent;
        samus.ChargingSound.Cancel();
        return BeamFiringResult.Of(SamusSoundStop.Any, SoundRequestPriority.Essential);
    }

    /// <summary>Attempts allocation and muzzle setup for an ordinary or charged beam shot.</summary>
    internal BeamShot? TryFireBeam(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerNewInput,
        SamusBombProjectileSystem samusBombs,
        bool charged)
    {
        const GameplayAction shoot = GameplayAction.Shoot;

        // Both release paths converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, Samus attempts a shot every held
        // frame and the beam cooldown supplies the rate limit; charged releases reach
        // the same gate once.
        if (_pool.ShotCount.Count >= SamusProjectileSystem.PlaceCount ||
            samusBombs.CooldownTimer != 0)
        {
            return null;
        }

        // A fire attempt sets the shared cooldown before it takes a place, so a shot count that disagrees
        // with the free places shows rather than being repaired.
        samusBombs.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        _pool.ShotCount.CountShot();

        // The shot count can read lower than the places held: a special attack sets it to its particle count whatever
        // the fifth place holds. With every place held the shot takes the last place over the shot there (#2468).
        int place = _pool.PlaceForBeam();

        SamusProjectileDirection? direction = samus.TransitionShot.Handed is { } transitionShot
            ? transitionShot.Aim
            : SamusPoseMetadata.ReadAimDirection(samus);

        if (direction is not { } aim)
        {
            // A pose that cannot shoot fires nothing; a transition shot always names an aim (SamusTransitionShot.FromPose).
            _pool.ShotCount.TakeBackRefusedShot();
            return null;
        }

        // The shot starts from the horizontal fraction of a pixel its place hands on (LeftBeamStartFromLastShot): the left
        // muzzle check's -1 speed can carry it back into the same whole pixel before the velocity setup clears it. Over a
        // held place it also starts from the Power Bomb fuse the shot there leaves (BeamFiredOverLastShot).
        // The projectile setup selects the combination's projectile kind, whose damage the shot deals, from the shot's type.
        SamusProjectileType firedType = SamusProjectileType.Beam(SamusBeamCombinations.Of(samus.EquippedBeams), charged);
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(firedType.BeamCombination, charged);
        SamusProjectileOccupant held = _pool.OccupyWithBeam(place, firedType, SamusProjectileDamage.Read(kind));
        held.Direction = aim;
        InitializePosition(samus, held);
        held.Trail = UpdateCountdown.Lasting(ProjectileTimingDefinitions.TrailInterval);
        held.PicturedBeams = held.Type.BeamCombination;
        InteractionImmunity = UpdateCountdown.Lasting(ProjectileTimingDefinitions.BeamInteractionImmunity);
        // The shot cancels a pending resume of the charging sound where it requests its own sound, before the projectile
        // is set up, so the next post-draw sound pass does not resume the charge after it (#1689).
        samus.ChargingSound.Cancel();

        // The combination selects one projectile: Spazer's three streaks are one picture of one projectile, so no
        // further projectiles are created here.
        SamusBeamCombination beamType = held.Type.BeamCombination;

        // The projectile setup chooses the direction-specific animation, samples its initial radii, and arms a one-frame
        // timer.
        SamusProjectileSegment? segment = SamusProjectileSelection.BeamShotAnimation(beamType, charged, aim);
        held.Animation = SamusProjectilePlayback.Start(segment);
        held.Radius = SamusProjectilePlayback.InitialRadius(segment);

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
        samusBombs.SetSharedCooldown(cooldown);

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(charged, beamType);
        if (charged)
            _charge.ChargedShotGlow = ChargedShotGlow.StartChargedShotFlash();

        // Uncharged and charged firing explicitly zero both speeds and call
        // the directional or Wave beam collision check before installing the shot's behaviour and
        // initial speed. This is what lets a muzzle already overlapping a door cap trigger
        // its shot reaction instead of moving its leading edge beyond the cap.
        bool initialImpact = _terrainCollision.RunInitialBeamCollision(
            terrain,
            held,
            waveBeam: beamType.HasWave);
        if (!initialImpact)
        {
            // The shot's behaviour starts only after muzzle collision, and each combination names its own, the Spazer
            // and Plasma ones included, apart from the Wave component. A Spazer and Plasma combination whose shots'
            // effect lies outside the emulation scope limit (UnauthoredBeamShotBehaviors) is refused here (#1500).
            SamusBeamShotBehaviors.Resolve(charged, beamType).Fire(held);
            InitializePowerBeamVelocity(samus.ProjectileInheritance, held, _unauthoredFiring);
        }
        return new BeamShot(sound);
    }

    /// <summary>Allocates charged-Plasma art with Hyper damage, Wave collision, and the Hyper Beam's cooldown (<see cref="SamusProjectileCooldownDefinitions.HyperBeamShot"/>).</summary>
    internal BeamShot? TryFireHyperBeam(
        SamusTerrain terrain,
        SamusState samus,
        SamusBombProjectileSystem samusBombs)
    {
        // Admission is shared with normal beams: a full shot count and a running cooldown reject firing before any
        // projectile is created.
        if (_pool.ShotCount.Count >= SamusProjectileSystem.PlaceCount ||
            samusBombs.CooldownTimer != 0)
        {
            return null;
        }

        samusBombs.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        _pool.ShotCount.CountShot();
        // As for ordinary beams, with every place held the shot takes the last place over the shot there (#2468).
        int place = _pool.PlaceForBeam();

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
        {
            _pool.ShotCount.TakeBackRefusedShot();
            return null;
        }

        // As for ordinary beams, the shot starts from the fraction of a pixel its place hands on (LeftBeamStartFromLastShot)
        // and the Power Bomb fuse a shot there leaves (BeamFiredOverLastShot).
        // The Hyper Beam is a charged Plasma beam for its projectile data and animation even though
        // acquisition equips Wave and Plasma, whose pictures its shots show; its damage is its own.
        SamusProjectileOccupant held = _pool.OccupyWithBeam(place,
            SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true), SamusProjectileDamageDefinitions.HyperBeamDamage);
        held.Direction = aim;

        InitializePosition(samus, held);
        InteractionImmunity = UpdateCountdown.Lasting(ProjectileTimingDefinitions.BeamInteractionImmunity);
        // As for ordinary beams, the shot cancels a pending resume of the charging sound where it requests its own (#1689).
        samus.ChargingSound.Cancel();

        held.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);
        const SamusBeamCombination hyperBeamType = SamusBeamCombination.Plasma;
        SamusProjectileKind kind = SamusProjectileSelection.BeamShot(hyperBeamType, charged: true);
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, held.Direction);
        held.Animation = SamusProjectilePlayback.Start(segment);
        held.Radius = SamusProjectilePlayback.InitialRadius(segment);

        // Hyper uses the Wave beam collision check at this same producer seam. It publishes door and
        // shootable-block reactions but, like every Wave family, is not stopped by solid blocks and
        // survives to receive its normal movement state.
        _ = _terrainCollision.RunInitialBeamCollision(
            terrain,
            held,
            waveBeam: true);
        held.Install(SamusShotBehaviors.HyperBeam, kindState: null);
        InitializePowerBeamVelocity(samus.ProjectileInheritance, held, _unauthoredFiring);

        // The shot starts the Hyper Beam's own suit glow, and its flare in place of the charge.
        samusBombs.SetSharedCooldown(SamusProjectileCooldownDefinitions.HyperBeamShot);
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
        SamusBombProjectileSystem samusBombs)
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
            samusBombs.CooldownTimer != 0)
        {
            return null;
        }

        samusBombs.SetSharedCooldown(SamusProjectileCooldownDefinitions.FireAttempt);
        if (ammo == 0)
            return null;

        if (_pool.FindFreePlace() is not { } place)
            return null;

        if (SamusPoseMetadata.ReadAimDirection(samus) is not { } aim)
            return null;
        // The selected HUD item chose the missile family above; the family selects the projectile kind, whose damage the
        // missile deals.
        SamusProjectileType firedType = SamusProjectileType.Of(isSuperMissile ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile);
        SamusProjectileKind kind = SamusProjectileSelection.Missile(isSuperMissile);
        SamusProjectileOccupant held = _pool.Occupy(place, firedType, SamusProjectileDamage.Read(kind));
        held.Direction = aim;

        InitializePosition(samus, held);
        _pool.ShotCount.CountShot();
        InteractionImmunity = UpdateCountdown.Lasting(ProjectileTimingDefinitions.MissileInteractionImmunity);
        // A count Crystal Flash drained below zero still fires, each shot taking it further below; only the 32768th shot
        // from below zero wraps it to the largest count (CountsRunBelowZero), a wrap play can reach.
        if (isSuperMissile)
            samus.SuperMissiles = CountsRunBelowZero.DecrementWrapping(samus.SuperMissiles);
        else
            samus.Missiles = CountsRunBelowZero.DecrementWrapping(samus.Missiles);
        held.Trail = UpdateCountdown.Lasting(ProjectileTimingDefinitions.TrailInterval);
        held.PicturedBeams = SamusBeamCombinations.Of(samus.EquippedBeams);

        // The missile's velocity is set when it ignites, on its first update in this same input phase
        // (ProjectileMotion.IgniteMissile), before anything reads it.
        SamusProjectileSegment segment = SamusProjectileKinds.Animation(kind, held.Direction);
        held.Animation = SamusProjectilePlayback.Start(segment);
        held.Radius = SamusProjectilePlayback.InitialRadius(segment);
        if (isSuperMissile)
        {
            held.Install(SamusShotBehaviors.SuperMissile,
                new SuperMissileShotState(new SamusMissileIgnition.Pending(), UnlinkedSuperMissileLink.Place));
        }
        else
            held.Install(SamusShotBehaviors.Missile, new MissileShotState(new SamusMissileIgnition.Pending()));

        // The missile sound and the missile's shared cooldown. An item-selection press made while item cancel
        // is held asks for the selection to return to the beam after one shot; a successful
        // missile returns it, ending that request, before the ordinary empty-ammo fallback. Murder Beam uses this exact path
        // to begin charging safely while the missile cooldown suppresses its unsafe initial
        // uncharged shot.
        samusBombs.SetSharedCooldown(isSuperMissile
            ? SamusProjectileCooldownDefinitions.SuperMissileShot
            : SamusProjectileCooldownDefinitions.MissileShot);
        if (samus.AutoCancelledItem is not null)
        {
            samus.SelectedHudItem = HudItem.Beam;
            samus.AutoCancelledItem = null;
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
        SamusProjectileOccupant projectile)
    {
        int poseYOffset = SamusPoseTraits.Of(samus.Pose).ProjectileHeight;
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        // Running and the two moonwalks aiming diagonally up take the running origins; every other pose takes the
        // ordinary ones.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.MoonwalkAimUp);
        var origin = SamusMuzzleOrigins.Read(runningOrigin, projectile.Direction);
        // Samus's centre lies at least 21 pixels from the room's left edge, which every row she reaches closes with terrain
        // or a door, and no muzzle lies more than 19 left of her, so the muzzle lies in the room across. A room open at the
        // top (the Landing Site's sky) lets her rise until her top meets the room's, and a shot aimed up then leaves the
        // muzzle above the room's top, a height OffRoomPlaces gives it.
        projectile.Position = projectile.Position.WithWhole(new(samus.Position.X.Whole + origin.X, OffRoomPlaces.Place(samus.Position.Y.Whole + origin.Y - poseYOffset)));
    }

    /// <summary>
    /// Selects beam speed from its combination and diagonal direction before applying inherited velocity. Every
    /// combination launches at the same authored pair; a Spazer and Plasma combination launches at the pair
    /// <paramref name="unauthoredFiring"/> decoded for it.
    /// </summary>
    internal static void InitializePowerBeamVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectileOccupant held,
        UnauthoredBeamFiring<SamusBeamCombination> unauthoredFiring)
    {
        Ensure.NotNull(held);
        Ensure.NotNull(unauthoredFiring);
        bool diagonal = SamusProjectileDirectionDefinitions.Axes(held.Direction).IsDiagonal;
        SamusBeamCombination beams = held.Type.BeamCombination;
        SignedEightEight authoredSpeed = diagonal ? SamusProjectileMotionDefinitions.DiagonalSpeed : SamusProjectileMotionDefinitions.CardinalSpeed;
        SignedEightEight speed = beams.CanBeEquipped ? authoredSpeed : unauthoredFiring.Speed(beams, diagonal);

        InitializeDirectionalVelocity(inheritance, held, speed);
    }

    /// <summary>Clears projectile position fractions and combines base speed with Samus' inherited motion.</summary>
    internal static void InitializeDirectionalVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectileOccupant projectile,
        SignedEightEight baseSpeed)
    {
        projectile.Position = projectile.Position.WithoutFractions();
        projectile.Velocity = LaunchVelocity(inheritance, projectile.Direction, baseSpeed);
    }

    /// <summary>
    /// The launch velocity of a shot fired in <paramref name="direction"/> at <paramref name="baseSpeed"/>, in pixels
    /// per update: the base speed along the direction plus the motion it inherits from Samus.
    /// </summary>
    internal static PixelVelocity<RoomPixels> LaunchVelocity(
        ProjectileLaunchInheritance inheritance,
        SamusProjectileDirection direction,
        SignedEightEight baseSpeed)
    {
        Ensure.NotNull(inheritance);
        SamusProjectileDirectionAxes axes = SamusProjectileDirectionDefinitions.Axes(direction);
        SignedEightEight x = axes.Horizontal switch
        {
            HorizontalDirection.Right => inheritance.InheritedVelocity(HorizontalDirection.Right) + baseSpeed,
            HorizontalDirection.Left => inheritance.InheritedVelocity(HorizontalDirection.Left) - baseSpeed,
            null => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined horizontal direction {axes.Horizontal}."),
        };
        SignedEightEight y = axes.Vertical switch
        {
            // The rising launch velocity and the base speed combine as wrapping velocities.
            VerticalDirection.Up => inheritance.RisingLaunchVelocity() - baseSpeed,
            VerticalDirection.Down => inheritance.InheritedVelocity(VerticalDirection.Down) + baseSpeed,
            null => SignedEightEight.Zero,
            _ => throw new InvalidOperationException($"Undefined vertical direction {axes.Vertical}."),
        };
        return new(x, y);
    }

    /// <summary>
    /// The updates a shot just fired keeps Samus from interacting with her own projectiles, bomb jumps included (#2529):
    /// firing a beam or a missile sets it, and it is counted down to none once each gameplay update, after Samus and the
    /// enemies have moved.
    /// </summary>
    internal UpdateCountdown InteractionImmunity { private get; set; } = UpdateCountdown.Ended();

    /// <summary>Whether a shot just fired keeps Samus from interacting with her own projectiles.</summary>
    public bool SuppressesInteraction => InteractionImmunity.IsRunning;

    /// <summary>Counts one update of the interaction immunity; an immunity that has run out stays over.</summary>
    public void CountInteractionImmunityDown() => InteractionImmunity.Step();
}
