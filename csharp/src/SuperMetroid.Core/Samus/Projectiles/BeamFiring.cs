using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Firing beams, missiles and the Hyper Beam: charge cancellation, spawn position and velocity, the interaction-immunity timer $18AC, and the charged and combo launch paths.</summary>
[RoomLifetime]
internal sealed class BeamFiring
{
    /// <summary>Clears charge flares and initializes launched beam animation.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Tracks held Shoot duration, charge effects, and Hyper Beam glow.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Handles the special beam attack after the charge counter reaches 120.</summary>
    private readonly BeamCombos _combos;
    /// <summary>Owns the five counted projectile slots and their native counter.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Runs initial muzzle collision against room terrain and PLMs.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;

    /// <summary>Connects firing to charge, animation, slot allocation, combo, and terrain systems.</summary>
    internal BeamFiring(ProjectileAnimation animation, BeamChargeState charge, BeamCombos combos, ProjectileSlotTable slotTable, ProjectileTerrainCollision terrainCollision)
    {
        _animation = animation;
        _charge = charge;
        _combos = combos;
        _slotTable = slotTable;
        _terrainCollision = terrainCollision;
    }
    /// <summary>
    /// Applies the shared side effect of all six <c>SwitchToHudHandler_*</c> routines:
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
    /// Native Samus command four ($90:F19B/$90:F19E): consume Pseudo Screw charge,
    /// clear its visible flare and restore the normal suit palette. Unlike HUD
    /// cancellation, this does not rewrite the previous-charge sampling word.
    /// </summary>
    public void ConsumePseudoScrewCharge(SamusColours colours, SamusState samus)
    {
        _charge.ChargePaletteStep = 0;
        _charge.FlareCounter = 0;
        samus.ProjectileFlareCounter = 0;
        _animation.ClearFlareAnimationState();
        ProjectileAnimation.LoadNormalSuitPalette(colours, samus);
    }

    /// <summary>Clears the charge counter, its prior sample, and all flare animation layers.</summary>
    private void CancelCharge()
    {
        _charge.FlareCounter = 0;
        _charge.PreviousBeamChargeCounter = 0;
        _animation.ClearFlareAnimationState();
    }

    /// <summary>Handles held Shoot, release, pose-transition release, Hyper Beam, and special attack timing.</summary>
    internal BeamFiringResult HandleBeamInput(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomPlms? roomPlms)
    {
        const ControllerButton shoot = ControllerButton.X;
        _charge.PreviousBeamChargeCounter = _charge.FlareCounter;

        // `$90:B80D` gives the Hyper flag priority over Charge equipment. It still uses
        // held Shoot rather than a new edge, with its own 21-frame cooldown preventing a
        // desktop-held button from allocating more frequently than the cartridge.
        if (samus.HyperBeamEnabled)
        {
            (int? slot, SoundEffectId? sound) = (controllerInput & shoot) != ControllerButton.None
                ? TryFireHyperBeam(level, samus, sharedProjectiles, roomPlms)
                : new BeamShot(null, null);
            return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        bool chargeEquipped = samus.EquippedBeams.HasAny(SamusBeamFlags.Charge);
        bool held = (controllerInput & shoot) != ControllerButton.None;
        if (!chargeEquipped)
        {
            _charge.FlareCounter = 0;
            _animation.ClearFlareAnimationState();
            (int? slot, SoundEffectId? sound) = held
                ? TryFireBeam(
                    level,
                    samus,
                    controllerNewInput,
                    sharedProjectiles,
                    roomPlms,
                    charged: false)
                : new BeamShot(null, null);
            return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
        }

        if (samus.PendingTransitionShot is not null)
        {
            // `$90:B82D-$B83A` forces release before testing held Shoot. This is why a
            // charged beam can leave the OLD gun direction while a normal-jump or moonwalk
            // transition installs new body art. Values below 60 release an ordinary beam;
            // values 60+ release the charged family through the same allocation gate.
            bool forcedChargedRelease =
                _charge.FlareCounter >= BeamChargeDefinitions.FullyChargedCounter;
            _charge.FlareCounter = 0;
            _animation.ClearFlareAnimationState();
            return CompleteBeamRelease(TryFireBeam(
                level,
                samus,
                controllerNewInput,
                sharedProjectiles,
                roomPlms,
                charged: forcedChargedRelease));
        }

        if (held)
        {
            // `$90:B843` increments through 120 and fires one ordinary shot on the first
            // held frame. Charge Beam therefore changes sustained-fire semantics rather
            // than suppressing the familiar initial power shot.
            if (_charge.FlareCounter < BeamChargeDefinitions.SpecialAttackCounter)
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
                        roomPlms,
                        charged: false);
                    return new(slot, sound, sound is null ? null : SoundRequestPriority.Essential);
                }

                // `$90:BAFC` owns both flare graphics and their audio. Its draw-time call
                // queues library-one sequence `$08` exactly once at counter sixteen. The
                // C# renderer is intentionally side-effect free, so publish the same command
                // here, on the alpha pass that advances the counter to sixteen.
                if (_charge.FlareCounter == BeamChargeDefinitions.ChargeSoundStartCounter)
                {
                    return new(null, Library1Sound.ChargingBeam, Priority: SoundRequestPriority.High);
                }
            }
            else if (_combos.TryActivateCombo(samus, sharedProjectiles, out SoundEffectId? comboSound))
            {
                // Native tries FireSBA only on the call after charge reaches 120.
                // Successful activation clears flare animation and restores the suit;
                // failed family dispatch leaves charge intact, including its ammo debit.
                _charge.FlareCounter = 0;
                _animation.ClearFlareAnimationState();
                samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
                return new(null, comboSound, SoundRequestPriority.Normal);
            }
            return new(null, null, null);
        }

        if (_charge.FlareCounter == 0)
            return new(null, null, null);

        bool releaseCharged =
            _charge.FlareCounter >= BeamChargeDefinitions.FullyChargedCounter;
        _charge.FlareCounter = 0;
        _animation.ClearFlareAnimationState();
        return CompleteBeamRelease(TryFireBeam(
            level,
            samus,
            controllerNewInput,
            sharedProjectiles,
            roomPlms,
            charged: releaseCharged));
    }

    /// <summary>
    /// Applies the shared tail of <c>FireUnchargedBeam</c>/<c>FireChargedBeam</c>. A
    /// successful shot's own essential-priority sound replaces the charging sound. If allocation or
    /// muzzle initialization rejects a release after counter sixteen, the cartridge queues
    /// library-one `$02` instead so the sustained charge cannot remain audible.
    /// </summary>
    private BeamFiringResult CompleteBeamRelease(
        BeamShot firing)
    {
        // FireChargedBeam's success path queues the indexed sound value even when an
        // out-of-domain beam combination reads zero from the adjacent table. Allocation,
        // not a nonzero sound command, distinguishes that success from the shared failure
        // tail which cancels an audible charge. Murder Beam is the observable index-15
        // case: it allocates a persistent projectile while requesting no new sound.
        if (firing.Slot is not null)
            return new(firing.Slot, firing.Sound, Priority: SoundRequestPriority.Essential);

        return _charge.PreviousBeamChargeCounter >=
            BeamChargeDefinitions.ChargeSoundStartCounter
            ? new(null, Library1Sound.Silence, Priority: SoundRequestPriority.Essential)
            : new(null, null, null);
    }


    /// <summary>Attempts allocation and muzzle setup for an ordinary or charged beam shot.</summary>
    internal BeamShot TryFireBeam(
        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomPlms? roomPlms,
        bool charged)
    {
        const ControllerButton shoot = ControllerButton.X;

        // `$90:B823/$90:B986` converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, callers attempt a shot every held
        // frame and cooldown `$0CCC` supplies the native rate limit; charged releases reach
        // the same gate once.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            sharedProjectiles.CooldownTimer.EightBitCount != 0)
        {
            return new BeamShot(null, null);
        }

        // `$90:AC39` writes one before allocating storage. A corrupt counter/free-slot
        // disagreement is therefore intentionally observable rather than silently repaired.
        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter = WrappingCounter.IncrementWord(_slotTable.ProjectileCounter);

        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
        {
            // Consistent state cannot arrive here. Match the producer's defensive rollback
            // so a debugger mutation does not leave the count permanently above five.
            _slotTable.ProjectileCounter = WrappingCounter.DecrementWord(_slotTable.ProjectileCounter);
            return new BeamShot(null, null);
        }

        // The fire routines overwrite only the words they initialize. Words such as the
        // subpixels keep the slot's previous occupant until velocity setup, so the left
        // muzzle check's -1 speed ($90:BDA7) can carry back into the same whole pixel.
        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];

        int direction = samus.PendingTransitionShot?.Direction ?? SamusPoseMetadata.ReadShotDirection(samus);

        if (SamusProjectileDirectionCodec.TryFromAimIndex(direction) is not { } aim)
        {
            _slotTable.ProjectileCounter = WrappingCounter.DecrementWord(_slotTable.ProjectileCounter);
            return new BeamShot(null, null);
        }

        slot.Direction = aim;
        slot.IsSpent = false;
        InitializePosition(samus, slot);
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.Type = SamusProjectileType.Beam(SamusBeamCombinations.Of(samus.EquippedBeams), charged);
        ProjectileInvincibilityTimer = WrappingCountdown.Start(10);

        // The combination selects one projectile, not four independent animation layers
        // composed by the host. In particular, Spazer's familiar three streaks are one
        // composition in one projectile slot, so no child projectiles are created here.
        SamusBeamCombination beamType = slot.Type.BeamCombination;

        // `$93:8000` indexes a data-table pointer by beam type, stores damage, chooses the
        // direction-specific list, samples its initial radii, and arms a one-frame timer.
        SamusProjectileHeader header = SamusProjectileSelection.Select(
            charged ? SamusProjectileSelectorTable.ChargedBeam : SamusProjectileSelectorTable.UnchargedBeam, beamType.TableIndex);
        slot.Damage = SamusProjectileDamage.Read(header);
        SamusProjectileProgram? program = SamusProjectileHeaders.Program(header, direction);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);

        // $90:B8ED checks Charge equipment and both input-edge latches before
        // selecting ordinary cooldown. Only uncharged auto-fire without those
        // conditions uses the longer held-fire delay.
        int cooldown = charged
            ? SamusProjectileCooldowns.ReadByte(
                SamusFiringTableDefinitions.UnchargedCooldowns +
                SamusFiringTableDefinitions.ChargedRowOffset + beamType.TableIndex)
            : samus.EquippedBeams.HasAny(SamusBeamFlags.Charge) || (controllerNewInput & shoot) != ControllerButton.None
                ? SamusProjectileCooldowns.ReadByte(SamusFiringTableDefinitions.UnchargedCooldowns + beamType.TableIndex)
                : SamusProjectileCooldowns.ReadByte(SamusFiringTableDefinitions.AutoFireCooldowns + beamType.TableIndex);
        sharedProjectiles.SetSharedCooldown(cooldown);

        SoundEffectId? sound = SamusProjectileSoundRouting.Resolve(charged, beamType);
        if (charged)
            _charge.ChargedShotGlowTimer = WrappingCountdown.Start(4);

        // FireUnchargedBeam and FireChargedBeam explicitly zero both speed words and call
        // CheckBeamCollByDir/WaveBeam_CheckColl before installing the pre-instruction and
        // initial speed. This is what lets a muzzle already overlapping a door cap trigger
        // its bank-$94 shot reaction instead of moving its leading edge beyond the cap.
        bool initialImpact = _terrainCollision.RunInitialBeamCollision(
            level,
            slot,
            roomPlms,
            waveBeam: beamType.HasWave,
            sharedProjectiles.PowerBombExplosion);
        if (!initialImpact)
        {
            // The cartridge installs the callback only after muzzle collision. Do not
            // infer it from Wave bits: the table is the dispatcher, including overreads.
            SamusBeamCallbackDefinition callback =
                SamusBeamCallbacks.Resolve(charged, beamType);
            slot.PreInstruction = callback.Translated ?? throw new NotSupportedException(
                $"Beam callback {callback.Callback} is not translated.");
            InitializePowerBeamVelocity(samus.ProjectileInheritance, slot);
        }
        return new BeamShot(slotIndex, sound);
    }

    /// <summary>Allocates charged-Plasma art with Hyper damage, Wave collision, and a 21-frame cooldown.</summary>
    internal BeamShot TryFireHyperBeam(
        RoomLevelData level,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomPlms? roomPlms)
    {
        // `$90:AC39` is shared with normal beams: five counted ordinary slots and a nonzero
        // low cooldown byte reject firing before any slot fields are touched.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            sharedProjectiles.CooldownTimer.EightBitCount != 0)
        {
            return new BeamShot(null, null);
        }

        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter = WrappingCounter.IncrementWord(_slotTable.ProjectileCounter);
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
        {
            _slotTable.ProjectileCounter = WrappingCounter.DecrementWord(_slotTable.ProjectileCounter);
            return new BeamShot(null, null);
        }

        // As for ordinary beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];
        int direction = SamusPoseMetadata.ReadShotDirection(samus);
        if (SamusProjectileDirectionCodec.TryFromAimIndex(direction) is not { } aim)
        {
            _slotTable.ProjectileCounter = WrappingCounter.DecrementWord(_slotTable.ProjectileCounter);
            return new BeamShot(null, null);
        }
        slot.Direction = aim;
        slot.IsSpent = false;

        InitializePosition(samus, slot);
        ProjectileInvincibilityTimer = WrappingCountdown.Start(10);

        // The Hyper Beam is a charged Plasma beam for its projectile data and art even though
        // acquisition equips Wave and Plasma tiles and palette. It first reads the charged
        // Plasma entry; `$90:BD21` then deliberately replaces its damage with 1000 before the
        // first instruction record executes.
        slot.Type = SamusProjectileType.Beam(SamusBeamCombination.Plasma, charged: true);
        const SamusBeamCombination hyperBeamType = SamusBeamCombination.Plasma;
        SamusProjectileHeader header = SamusProjectileSelection.Select(
            SamusProjectileSelectorTable.ChargedBeam, hyperBeamType.TableIndex);
        slot.Damage = SamusProjectileDamage.Read(header);
        SamusProjectileProgram? program = SamusProjectileHeaders.Program(header, slot.Direction.ToAimIndex());
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Damage = 1000;

        // Hyper uses WaveBeam_CheckColl at this same producer seam. It publishes door and
        // shootable-block reactions but, like every Wave family, ignores solid carry and
        // survives to receive its normal movement state.
        _ = _terrainCollision.RunInitialBeamCollision(
            level,
            slot,
            roomPlms,
            waveBeam: true,
            sharedProjectiles.PowerBombExplosion);
        slot.PreInstruction = SamusProjectilePreInstruction.HyperBeam;
        InitializePowerBeamVelocity(samus.ProjectileInheritance, slot);

        // These are literal stores at `$90:BD29-$BD52`. `$8014` is not an ordinary four-call
        // glow timer: its sign bit identifies Hyper's palette phase to the later consumer.
        sharedProjectiles.SetSharedCooldown(21);
        _charge.ChargedShotGlowTimer = WrappingCountdown.Start((1 << 15) | 20);
        _charge.StartHyperBeamFlare();
        _charge.FlareCounter = 1 << 15;

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

        // `$90:BE65-$BE72` accepts either the current new-press word or the previous filtered
        // drawing word. Live gameplay retains that edge through the next alpha, allowing
        // a press one frame before cooldown expires to fire on the following frame.
        // Bank-$91 demo playback supplies the analogous scripted edge. Cooldown still
        // owns admission, so the delayed copy cannot manufacture held-button auto-fire.
        if (((controllerNewInput | controllerPreviousNewInput) & shoot) == ControllerButton.None)
            return new BeamShot(null, null);

        // Admission reserves room for a Super Missile's later collision link. Ammo/free-
        // slot rejection rolls back the native count, but does not undo its cooldown write.
        bool isSuperMissile = samus.SelectedHudItem == HudItem.SuperMissile;
        int ammo = isSuperMissile ? samus.SuperMissiles : samus.Missiles;
        int admissionLimit = isSuperMissile ? SuperMissileDefinitions.AdmissionLimit : SamusProjectileSystem.SlotCount;
        if (_slotTable.ProjectileCounter >= admissionLimit ||
            sharedProjectiles.CooldownTimer.EightBitCount != 0)
        {
            return new BeamShot(null, null);
        }

        sharedProjectiles.SetSharedCooldown(1);
        if (ammo == 0)
            return new BeamShot(null, null);

        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            return new BeamShot(null, null);

        // As for beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slotTable.Slots[slotIndex];
        int direction = SamusPoseMetadata.ReadShotDirection(samus);
        if (SamusProjectileDirectionCodec.TryFromAimIndex(direction) is not { } aim)
            return new BeamShot(null, null);
        slot.Direction = aim;
        slot.IsSpent = false;

        InitializePosition(samus, slot);
        _slotTable.ProjectileCounter = WrappingCounter.IncrementWord(_slotTable.ProjectileCounter);
        ProjectileInvincibilityTimer = WrappingCountdown.Start(20);
        if (isSuperMissile)
            samus.SuperMissiles = SamusQuantity.DecrementWord(samus.SuperMissiles);
        else
            samus.Missiles = SamusQuantity.DecrementWord(samus.Missiles);
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.Type = SamusProjectileType.Of(isSuperMissile ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile);
        slot.MissileIgnition = SamusMissileIgnition.Pending;
        slot.SuperMissileLinkSlot = 0;

        // The producer first calls the generic velocity initializer with base speed zero.
        // Ignition replaces that zero with one pixel per update during this same frame's alpha pass;
        // keeping both calls makes the one-frame ignition transition directly inspectable.
        InitializeDirectionalVelocity(samus.ProjectileInheritance, slot, baseSpeed: 0);

        SamusProjectileHeader header = SamusProjectileSelection.Select(
            SamusProjectileSelectorTable.NonBeam, samus.SelectedHudItem.ToWord());
        slot.Damage = SamusProjectileDamage.Read(header);
        SamusProjectileProgram? program = SamusProjectileHeaders.Program(header, slot.Direction.ToAimIndex());
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.PreInstruction = isSuperMissile
            ? SamusProjectilePreInstruction.SuperMissile
            : SamusProjectilePreInstruction.Missile;

        // Retail constants embedded beside the bank-$90 producer: missile sound library-one
        // effect three and ten-frame shared cooldown. A Select press made while Item Cancel
        // is held records a one-shot auto-cancel index; a successful missile consumes that
        // request before the ordinary empty-ammo fallback. Murder Beam uses this exact path
        // to begin charging safely while the missile cooldown suppresses its unsafe initial
        // uncharged shot.
        sharedProjectiles.SetSharedCooldown(isSuperMissile ? 20 : 10);
        if (samus.AutoCancelHudItemIndex != HudItem.Beam)
        {
            samus.SelectedHudItem = HudItem.Beam;
            samus.AutoCancelHudItemIndex = HudItem.Beam;
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
        SamusProjectileSlot slot)
    {
        int poseYOffset = SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        // `$90:BA94` uses the running/moonwalk origin table for movement type one and for
        // the two diagonally-up moonwalk poses $75/$76. Every other pose uses the default.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
        var origin = SamusMuzzleOrigins.Read(runningOrigin, slot.Direction.ToAimIndex());
        slot.Position = slot.Position.WithWhole(new(CoordinateAxis.Wrap(samus.Position.X.Whole + origin.X), CoordinateAxis.Wrap(samus.Position.Y.Whole + origin.Y - poseYOffset)));
    }

    /// <summary>Selects beam speed from its combination and diagonal direction before applying inherited velocity.</summary>
    internal static void InitializePowerBeamVelocity(
        ProjectileInheritanceRecords inheritance,
        SamusProjectileSlot slot)
    {
        int direction = slot.Direction.ToAimIndex();
        bool diagonal = direction is 1 or 3 or 6 or 8;
        int rowOffset = slot.Type.BeamCombination.TableIndex * SamusFiringTableDefinitions.InitialSpeedRowBytes;
        int speed = WordWrap.Signed(SamusProjectileAccelerations.ReadWord(
            (diagonal
                ? SamusFiringTableDefinitions.DiagonalSpeeds
                : SamusFiringTableDefinitions.HorizontalVerticalSpeeds) + rowOffset));

        InitializeDirectionalVelocity(inheritance, slot, speed);
    }

    /// <summary>Clears projectile position fractions and combines base speed with Samus' inherited motion.</summary>
    internal static void InitializeDirectionalVelocity(
        ProjectileInheritanceRecords inheritance,
        SamusProjectileSlot slot,
        int baseSpeed)
    {
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.Velocity = inheritance.ReadVelocity(slot.Direction, baseSpeed);
    }

    /// <summary>
    /// WRAM <c>$18AC</c>; humanoid shots temporarily suppress Samus/projectile
    /// interaction, including bomb jumps. Decremented by the shared gameplay tail.
    /// </summary>
    public WrappingCountdown ProjectileInvincibilityTimer { get; internal set; }

    /// <summary>Decrements the projectile-owned word of native DecrementSamusTimers.</summary>
    public void DecrementInteractionTimer()
    {
        ProjectileInvincibilityTimer = ProjectileInvincibilityTimer.DecrementToZero();
    }
}
