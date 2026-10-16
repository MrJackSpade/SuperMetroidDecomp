using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Firing beams, missiles and the Hyper Beam: charge cancellation, spawn position and velocity, the interaction-immunity timer $18AC, and the charged and combo launch paths.</summary>
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
    /// Applies <c>Projectile_Func7_Shinespark</c>'s flare teardown at $90:CFFA.
    /// Unlike HUD selection, the cartridge clears the live charge and its animation
    /// without rewriting the previous-frame sampling word.
    /// </summary>
    internal void CancelChargeForShinespark()
    {
        _charge.FlareCounter = 0;
        _animation.ClearFlareAnimationState();
    }

    /// <summary>
    /// Native Samus command four ($90:F19B/$90:F19E): consume Pseudo Screw charge,
    /// clear its visible flare and restore the normal suit palette. Unlike HUD
    /// cancellation, this does not rewrite the previous-charge sampling word.
    /// </summary>
    public void ConsumePseudoScrewCharge(SnesCgram cgram, SamusState samus)
    {
        _charge.SamusChargePaletteIndex = 0;
        _charge.FlareCounter = 0;
        samus.ProjectileFlareCounter = 0;
        _animation.ClearFlareAnimationState();
        ProjectileAnimation.LoadNormalSuitPalette(cgram, samus);
    }

    /// <summary>Clears the charge counter, its prior sample, and all flare animation layers.</summary>
    private void CancelCharge()
    {
        _charge.FlareCounter = 0;
        _charge.PreviousBeamChargeCounter = 0;
        _animation.ClearFlareAnimationState();
    }

    /// <summary>Handles held Shoot, release, pose-transition release, Hyper Beam, and special attack timing.</summary>
    internal (int? Slot, ushort Sound, byte MaximumQueued) HandleBeamInput(
        RoomLevelData level,
        SamusState samus,
        SnesButton controllerInput,
        SnesButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        const SnesButton shoot = SnesButton.X;
        _charge.PreviousBeamChargeCounter = _charge.FlareCounter;

        // `$90:B80D` gives the Hyper flag priority over Charge equipment. It still uses
        // held Shoot rather than a new edge, with its own 21-frame cooldown preventing a
        // desktop-held button from allocating more frequently than the cartridge.
        if (samus.HyperBeamEnabled)
        {
            (int? slot, ushort sound) = (controllerInput & shoot) != 0
                ? TryFireHyperBeam(level, samus, sharedProjectiles, roomPlms)
                : (null, 0);
            return (slot, sound, sound == 0 ? (byte)0 : (byte)15);
        }

        // Retail inventory normally owns twelve low-nibble combinations, but the native
        // routine never validates that domain before indexing its adjacent tables. Invalid
        // equipment combinations are observable cartridge behavior used by advanced beam
        // glitches, so this producer must preserve the raw four-bit index.
        SamusBeamLoadoutWord beamLoadout = samus.EquippedBeams;

        bool chargeEquipped = beamLoadout.HasAny(SamusBeamFlags.Charge);
        bool held = (controllerInput & shoot) != 0;
        if (!chargeEquipped)
        {
            _charge.FlareCounter = 0;
            _animation.ClearFlareAnimationState();
            (int? slot, ushort sound) = held
                ? TryFireBeam(
                    level,
                    samus,
                    controllerNewInput,
                    sharedProjectiles,
                    roomPlms,
                    charged: false)
                : (null, 0);
            return (slot, sound, sound == 0 ? (byte)0 : (byte)15);
        }

        if (samus.PoseTransitionShotDirection != 0)
        {
            // `$90:B82D-$B83A` forces release before testing held Shoot. This is why a
            // charged beam can leave the OLD gun direction while a normal-jump or moonwalk
            // transition installs new body art. Values below 60 release an ordinary beam;
            // values 60+ release the charged family through the same allocation gate.
            bool forcedChargedRelease =
                _charge.FlareCounter >= SamusProjectileRomData.Beams.FullyChargedCounter;
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
            if (_charge.FlareCounter < SamusProjectileRomData.Beams.SpecialAttackCounter)
            {
                _charge.FlareCounter++;
                if (_charge.FlareCounter == 1)
                {
                    _animation.ClearFlareAnimationState();
                    (int? slot, ushort sound) = TryFireBeam(
                        level,
                        samus,
                        controllerNewInput,
                        sharedProjectiles,
                        roomPlms,
                        charged: false);
                    return (slot, sound, sound == 0 ? (byte)0 : (byte)15);
                }

                // `$90:BAFC` owns both flare graphics and their audio. Its draw-time call
                // queues library-one sequence `$08` exactly once at counter sixteen. The
                // C# renderer is intentionally side-effect free, so publish the same command
                // here, on the alpha pass that advances the counter to sixteen.
                if (_charge.FlareCounter == SamusProjectileRomData.Beams.ChargeSoundStartCounter)
                {
                    return (
                        null,
                        SoundEffectLibrary1Sounds.ChargeBeamStart.Value,
                        MaximumQueued: 9);
                }
            }
            else if (_combos.TryActivateCombo(samus, sharedProjectiles, out ushort comboSound))
            {
                // Native tries FireSBA only on the call after charge reaches 120.
                // Successful activation clears flare animation and restores the suit;
                // failed family dispatch leaves charge intact, including its ammo debit.
                _charge.FlareCounter = 0;
                _animation.ClearFlareAnimationState();
                samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
                return (null, comboSound, 6);
            }
            return (null, 0, 0);
        }

        if (_charge.FlareCounter == 0)
            return (null, 0, 0);

        bool releaseCharged =
            _charge.FlareCounter >= SamusProjectileRomData.Beams.FullyChargedCounter;
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
    /// successful shot's own Max15 sequence replaces the charging sound. If allocation or
    /// muzzle initialization rejects a release after counter sixteen, the cartridge queues
    /// library-one `$02` instead so the sustained charge cannot remain audible.
    /// </summary>
    private (int? Slot, ushort Sound, byte MaximumQueued) CompleteBeamRelease(
        (int? Slot, ushort Sound) firing)
    {
        // FireChargedBeam's success path queues the indexed sound value even when an
        // out-of-domain beam combination reads zero from the adjacent table. Allocation,
        // not a nonzero sound command, distinguishes that success from the shared failure
        // tail which cancels an audible charge. Murder Beam is the observable index-15
        // case: it allocates a persistent projectile while requesting no new sound.
        if (firing.Slot is not null)
            return (firing.Slot, firing.Sound, MaximumQueued: 15);

        return _charge.PreviousBeamChargeCounter >=
            SamusProjectileRomData.Beams.ChargeSoundStartCounter
            ? (null, SoundEffectLibrary1Sounds.CancelAll.Value, MaximumQueued: 15)
            : (null, 0, 0);
    }

    /// <summary>Attempts allocation and muzzle setup for an ordinary or charged beam shot.</summary>
    internal (int? Slot, ushort Sound) TryFireBeam(
        RoomLevelData level,
        SamusState samus,
        SnesButton controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms,
        bool charged)
    {
        const SnesButton shoot = SnesButton.X;

        // `$90:B823/$90:B986` converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, callers attempt a shot every held
        // frame and cooldown `$0CCC` supplies the native rate limit; charged releases reach
        // the same gate once.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            (SnesLittleEndian.LowByte(sharedProjectiles.CooldownTimer.RawValue)) != 0)
        {
            return (null, 0);
        }

        // `$90:AC39` writes one before allocating storage. A corrupt counter/free-slot
        // disagreement is therefore intentionally observable rather than silently repaired.
        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter = unchecked((ushort)(_slotTable.ProjectileCounter + 1));

        int slotIndex = Array.FindIndex(_slotTable._slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
        {
            // Consistent state cannot arrive here. Match the producer's defensive rollback
            // so a debugger mutation does not leave the count permanently above five.
            _slotTable.ProjectileCounter = NativeWordCounter.Decrement(_slotTable.ProjectileCounter).Value;
            return (null, 0);
        }

        // The fire routines overwrite only the words they initialize. Words such as the
        // subpixels keep the slot's previous occupant until velocity setup, so the left
        // muzzle check's -1 speed ($90:BDA7) can carry back into the same whole pixel.
        SamusProjectileSlot slot = _slotTable._slots[slotIndex];

        byte direction = samus.PoseTransitionShotDirection != 0
            ? unchecked((byte)samus.PoseTransitionShotDirection)
            : samus.PoseMetadata.ReadShotDirection();

        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
        {
            _slotTable.ProjectileCounter = NativeWordCounter.Decrement(_slotTable.ProjectileCounter).Value;
            return (null, 0);
        }

        slot.Direction = direction;
        InitializePosition(samus, slot);
        slot.TrailTimer = NativeCountdown.Start(4);
        slot.Type = SamusProjectileTypeWord.CreateBeam(samus.EquippedBeams, charged);
        ProjectileInvincibilityTimer = NativeCountdown.Start(10);

        // The four low bits are a direct ROM table index, not four independent animation
        // layers composed by the host. In particular, Spazer's familiar three streaks live
        // in one bank-$93 spritemap selected by one data pointer and occupy one projectile
        // slot. This is why no synthetic child projectiles are created here.
        SamusBeamCombination beamType = slot.PackedType.BeamCombination;

        // `$93:8000` indexes a data-table pointer by beam type, stores damage, chooses the
        // direction-specific list, samples its initial radii, and arms a one-frame timer.
        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            (charged
                ? SamusProjectileRomData.Beams.ChargedDataPointers
                : SamusProjectileRomData.Beams.UnchargedDataPointers) + beamType.TableIndex * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        SamusProjectileProgram? program = SamusProjectileHeaderDefinitions.Program(dataPointer, direction);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = NativeCountdown.Start(1);

        // $90:B8ED checks Charge equipment and both input-edge latches before
        // selecting ordinary cooldown. Only uncharged auto-fire without those
        // conditions uses the longer held-fire delay.
        byte cooldown = charged
            ? SamusProjectileCooldownDefinitions.ReadByte(
                SamusProjectileRomData.Beams.UnchargedCooldowns +
                SamusProjectileRomData.Beams.ChargedRowOffset + beamType.TableIndex)
            : samus.EquippedBeams.HasAny(SamusBeamFlags.Charge) || (controllerNewInput & shoot) != 0
                ? SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + beamType.TableIndex)
                : SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.AutoFireCooldowns + beamType.TableIndex);
        sharedProjectiles.SetSharedCooldown(cooldown);

        ushort sound = SamusProjectileSoundRoutingDefinitions.Resolve(charged, beamType);
        if (charged)
            _charge.ChargedShotGlowTimer = NativeCountdown.Start(4);

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
                SamusBeamCallbackDefinitions.Resolve(charged, beamType);
            slot.PreInstruction = callback.Translated ?? throw new NotSupportedException(
                $"Beam callback $90:{callback.NativePointer:X4} is not translated.");
            InitializePowerBeamVelocity(samus.ProjectileInheritance, slot);
        }
        return (slotIndex, sound);
    }

    /// <summary>Allocates charged-Plasma art with Hyper damage, Wave collision, and a 21-frame cooldown.</summary>
    internal (int? Slot, ushort Sound) TryFireHyperBeam(
        RoomLevelData level,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        // `$90:AC39` is shared with normal beams: five counted ordinary slots and a nonzero
        // low cooldown byte reject firing before any slot fields are touched.
        if (_slotTable.ProjectileCounter >= SamusProjectileSystem.SlotCount ||
            (SnesLittleEndian.LowByte(sharedProjectiles.CooldownTimer.RawValue)) != 0)
        {
            return (null, 0);
        }

        sharedProjectiles.SetSharedCooldown(1);
        _slotTable.ProjectileCounter = unchecked((ushort)(_slotTable.ProjectileCounter + 1));
        int slotIndex = Array.FindIndex(_slotTable._slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
        {
            _slotTable.ProjectileCounter = NativeWordCounter.Decrement(_slotTable.ProjectileCounter).Value;
            return (null, 0);
        }

        // As for ordinary beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slotTable._slots[slotIndex];
        byte direction = samus.PoseMetadata.ReadShotDirection();
        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
        {
            _slotTable.ProjectileCounter = NativeWordCounter.Decrement(_slotTable.ProjectileCounter).Value;
            return (null, 0);
        }
        slot.Direction = direction;

        InitializePosition(samus, slot);
        ProjectileInvincibilityTimer = NativeCountdown.Start(10);

        // The literal `$9018` is charged Plasma for bank-$93 data/art purposes even though
        // acquisition equips `$1009` Wave+Plasma tiles/palette. InitializeProjectile first
        // reads charged table entry eight; `$90:BD21` then deliberately replaces its damage
        // with 1000 before the first instruction record executes.
        slot.Type = SamusProjectileTypeWord.CreateBeam((SamusBeamFlags.Charge | SamusBeamFlags.Plasma), charged: true);
        const SamusBeamCombination hyperBeamType = SamusBeamCombination.Plasma;
        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.Beams.ChargedDataPointers + hyperBeamType.TableIndex * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        SamusProjectileProgram? program = SamusProjectileHeaderDefinitions.Program(dataPointer, slot.PackedDirection.DirectionIndex);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
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
        _charge.ChargedShotGlowTimer = NativeCountdown.Start((1 << 15) | 20);
        _charge._flareFrames[0] = 29;
        _charge._flareFrames[1] = 5;
        _charge._flareFrames[2] = 5;
        _charge._flareTimers[0] = _charge._flareTimers[1] = _charge._flareTimers[2] = NativeCountdown.Start(3);
        _charge.FlareCounter = 1 << 15;

        ushort sound = SamusProjectileSoundRoutingDefinitions.Resolve(
            charged: true, hyperBeamType);
        return (slotIndex, sound);
    }

    /// <summary>Consumes a current or retained Shoot edge, ammo, and cooldown to launch a missile.</summary>
    internal (int? Slot, ushort Sound) TryFireMissile(
        SamusState samus,
        SnesButton controllerNewInput,
        SnesButton controllerPreviousNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const SnesButton shoot = SnesButton.X;

        // `$90:BE65-$BE72` accepts either the current new-press word or the previous filtered
        // drawing word. Live gameplay retains that edge through the next alpha, allowing
        // a press one frame before cooldown expires to fire on the following frame.
        // Bank-$91 demo playback supplies the analogous scripted edge. Cooldown still
        // owns admission, so the delayed copy cannot manufacture held-button auto-fire.
        if (((controllerNewInput | controllerPreviousNewInput) & shoot) == 0)
            return (null, 0);

        // Admission reserves room for a Super Missile's later collision link. Ammo/free-
        // slot rejection rolls back the native count, but does not undo its cooldown write.
        bool isSuperMissile = samus.SelectedHudItem == 2;
        ushort ammo = isSuperMissile ? samus.SuperMissiles : samus.Missiles;
        int admissionLimit = isSuperMissile ? SamusProjectileRomData.NonBeam.SuperMissileAdmissionLimit : SamusProjectileSystem.SlotCount;
        if (_slotTable.ProjectileCounter >= admissionLimit ||
            (SnesLittleEndian.LowByte(sharedProjectiles.CooldownTimer.RawValue)) != 0)
        {
            return (null, 0);
        }

        sharedProjectiles.SetSharedCooldown(1);
        if (ammo == 0)
            return (null, 0);

        int slotIndex = Array.FindIndex(_slotTable._slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
            return (null, 0);

        // As for beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slotTable._slots[slotIndex];
        byte direction = samus.PoseMetadata.ReadShotDirection();
        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
            return (null, 0);
        slot.Direction = direction;

        InitializePosition(samus, slot);
        _slotTable.ProjectileCounter = unchecked((ushort)(_slotTable.ProjectileCounter + 1));
        ProjectileInvincibilityTimer = NativeCountdown.Start(20);
        if (isSuperMissile)
            samus.SuperMissiles = NativeWordCounter.Decrement(samus.SuperMissiles).Value;
        else
            samus.Missiles = NativeWordCounter.Decrement(samus.Missiles).Value;
        slot.TrailTimer = NativeCountdown.Start(4);
        slot.Type = (ushort)(SamusProjectileTypeWord.LiveMarker | (ushort)(isSuperMissile ? SamusProjectileFamily.SuperMissile : SamusProjectileFamily.Missile));
        slot.Variable = 0;

        // The producer first calls the generic velocity initializer with base speed zero.
        // Missile_Func1 replaces that zero with `$0100` during this same frame's alpha pass;
        // keeping both calls makes the one-frame ignition transition directly inspectable.
        InitializeDirectionalVelocity(samus.ProjectileInheritance, slot, baseSpeed: 0);

        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.NonBeam.DataPointers + samus.SelectedHudItem * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        SamusProjectileProgram? program = SamusProjectileHeaderDefinitions.Program(dataPointer, slot.PackedDirection.DirectionIndex);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.Radius = SamusProjectilePrograms.InitialRadius(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.PreInstruction = isSuperMissile
            ? SamusProjectilePreInstruction.SuperMissile
            : SamusProjectilePreInstruction.Missile;

        // Retail constants embedded beside the bank-$90 producer: missile sound library-one
        // effect three and ten-frame shared cooldown. A Select press made while Item Cancel
        // is held records a one-shot auto-cancel index; a successful missile consumes that
        // request before the ordinary empty-ammo fallback. Murder Beam uses this exact path
        // to begin charging safely while the missile cooldown suppresses its unsafe initial
        // uncharged shot.
        sharedProjectiles.SetSharedCooldown(isSuperMissile ? (ushort)20 : (ushort)10);
        if (samus.AutoCancelHudItemIndex != 0)
        {
            samus.SelectedHudItem = 0;
            samus.AutoCancelHudItemIndex = 0;
        }
        else if ((isSuperMissile ? samus.SuperMissiles : samus.Missiles) == 0)
            samus.SelectedHudItem = 0;
        return (slotIndex, isSuperMissile ? (ushort)4 : (ushort)3);
    }

    /// <summary>Places the muzzle using pose height and the running or ordinary directional origin table.</summary>
    internal static void InitializePosition(
        SamusState samus,
        SamusProjectileSlot slot)
    {
        byte poseYOffset = SamusPoseProjectileOriginDefinitions.ReadYOffset(samus.Pose);
        SamusMovementType movementType = samus.PoseMetadata.ReadMovementType();

        // `$90:BA94` uses the running/moonwalk origin table for movement type one and for
        // the two diagonally-up moonwalk poses $75/$76. Every other pose uses the default.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimUpRightPose;
        var origin = SamusProjectileOriginDefinitions.Read(runningOrigin, slot.Direction);
        slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + origin.X)), unchecked((ushort)(samus.Position.Y.Whole + origin.Y - poseYOffset))));
    }

    /// <summary>Selects beam speed from its combination and diagonal direction before applying inherited velocity.</summary>
    internal static void InitializePowerBeamVelocity(
        ProjectileInheritanceRecords inheritance,
        SamusProjectileSlot slot)
    {
        byte direction = slot.PackedDirection.DirectionIndex;
        bool diagonal = direction is 1 or 3 or 6 or 8;
        int rowOffset = slot.PackedType.BeamCombination.TableIndex * SamusProjectileRomData.Beams.InitialSpeedRowBytes;
        short speed = unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
            (diagonal
                ? SamusProjectileRomData.Beams.DiagonalSpeeds
                : SamusProjectileRomData.Beams.HorizontalVerticalSpeeds) + rowOffset));

        InitializeDirectionalVelocity(inheritance, slot, speed);
    }

    /// <summary>Clears projectile position fractions and combines base speed with Samus' inherited motion.</summary>
    internal static void InitializeDirectionalVelocity(
        ProjectileInheritanceRecords inheritance,
        SamusProjectileSlot slot,
        short baseSpeed)
    {
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.Velocity = inheritance.ReadVelocity(slot.Direction, baseSpeed);
    }

    /// <summary>
    /// WRAM <c>$18AC</c>; humanoid shots temporarily suppress Samus/projectile
    /// interaction, including bomb jumps. Decremented by the shared gameplay tail.
    /// </summary>
    public NativeCountdown ProjectileInvincibilityTimer { get; internal set; }

    /// <summary>Decrements the projectile-owned word of native DecrementSamusTimers.</summary>
    public void DecrementInteractionTimer()
    {
        ProjectileInvincibilityTimer = ProjectileInvincibilityTimer.DecrementToZero();
    }
}
