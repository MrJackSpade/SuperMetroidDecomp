using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Input admission, slot allocation, firing cooldowns, spawn positions, and initial velocities.
/// </summary>
public sealed partial class SamusProjectileSystem
{
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
        FlareCounter = 0;
        ClearFlareAnimationState();
    }

    /// <summary>
    /// Native Samus command four ($90:F19B/$90:F19E): consume Pseudo Screw charge,
    /// clear its visible flare and restore the normal suit palette. Unlike HUD
    /// cancellation, this does not rewrite the previous-charge sampling word.
    /// </summary>
    public void ConsumePseudoScrewCharge(ISnesAddressSpace bus, SnesCgram cgram, SamusState samus)
    {
        SamusChargePaletteIndex = 0;
        FlareCounter = 0;
        samus.ProjectileFlareCounter = 0;
        ClearFlareAnimationState();
        LoadNormalSuitPalette(bus, cgram, samus);
    }

    /// <summary>Clears the active beam-charge counters and all flare-animation layers.</summary>
    private void CancelCharge()
    {
        FlareCounter = 0;
        PreviousBeamChargeCounter = 0;
        ClearFlareAnimationState();
    }

    /// <summary>Applies held and edge-triggered fire input, advancing charge state or dispatching beam and Hyper shots.</summary>
    /// <param name="bus">Address space used for projectile data and Samus movement-state reads.</param>
    /// <param name="level">Room collision data used to resolve a beam's initial muzzle impact.</param>
    /// <param name="samus">Equipment, pose, and charge state updated or consumed by the selected firing path.</param>
    /// <param name="controllerInput">Current held-button word used for Shoot and Charge processing.</param>
    /// <param name="controllerNewInput">Fresh-button edge used by ordinary beam shot admission and cooldown selection.</param>
    /// <param name="sharedProjectiles">Shared cooldown and explosion state used by all projectile families.</param>
    /// <param name="roomPlms">Optional room PLM sink for beam impacts on doors or shootable blocks.</param>
    /// <returns>The allocated slot, sound request, and maximum queued sound duration; absent shots return a null slot.</returns>
    private (int? Slot, ushort Sound, byte MaximumQueued) HandleBeamInput(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        const ushort shoot = (ushort)SnesButton.X;
        PreviousBeamChargeCounter = FlareCounter;

        // `$90:B80D` gives the Hyper flag priority over Charge equipment. It still uses
        // held Shoot rather than a new edge, with its own 21-frame cooldown preventing a
        // desktop-held button from allocating more frequently than the cartridge.
        if (samus.HyperBeam != 0)
        {
            (int? slot, ushort sound) = (controllerInput & shoot) != 0
                ? TryFireHyperBeam(bus, level, samus, sharedProjectiles, roomPlms)
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
            FlareCounter = 0;
            ClearFlareAnimationState();
            (int? slot, ushort sound) = held
                ? TryFireBeam(
                    bus,
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
                FlareCounter >= SamusProjectileRomData.Beams.FullyChargedCounter;
            FlareCounter = 0;
            ClearFlareAnimationState();
            return CompleteBeamRelease(TryFireBeam(
                bus,
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
            if (FlareCounter < SamusProjectileRomData.Beams.SpecialAttackCounter)
            {
                FlareCounter++;
                if (FlareCounter == 1)
                {
                    ClearFlareAnimationState();
                    (int? slot, ushort sound) = TryFireBeam(
                        bus,
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
                if (FlareCounter == SamusProjectileRomData.Beams.ChargeSoundStartCounter)
                {
                    return (
                        null,
                        SoundEffectLibrary1Sounds.ChargeBeamStart.Value,
                        MaximumQueued: 9);
                }
            }
            else if (TryActivateCombo(bus, samus, sharedProjectiles, out ushort comboSound))
            {
                // Native tries FireSBA only on the call after charge reaches 120.
                // Successful activation clears flare animation and restores the suit;
                // failed family dispatch leaves charge intact, including its ammo debit.
                FlareCounter = 0;
                ClearFlareAnimationState();
                samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
                return (null, comboSound, 6);
            }
            return (null, 0, 0);
        }

        if (FlareCounter == 0)
            return (null, 0, 0);

        bool releaseCharged =
            FlareCounter >= SamusProjectileRomData.Beams.FullyChargedCounter;
        FlareCounter = 0;
        ClearFlareAnimationState();
        return CompleteBeamRelease(TryFireBeam(
            bus,
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

        return PreviousBeamChargeCounter >=
            SamusProjectileRomData.Beams.ChargeSoundStartCounter
            ? (null, SoundEffectLibrary1Sounds.CancelAll.Value, MaximumQueued: 15)
            : (null, 0, 0);
    }

    /// <summary>Allocates and initializes an ordinary or charged beam after checking slot capacity and the shared cooldown.</summary>
    /// <param name="bus">Address space supplying projectile tables, collision state, and movement data.</param>
    /// <param name="level">Room collision grid used to test the muzzle on the firing frame.</param>
    /// <param name="samus">Shooter state supplying equipment, direction, and position.</param>
    /// <param name="controllerNewInput">Fresh-button word used when selecting uncharged auto-fire cooldown.</param>
    /// <param name="sharedProjectiles">Shared projectile cooldown and bomb-explosion state.</param>
    /// <param name="roomPlms">Optional sink for door or shootable-block reactions caused by the muzzle.</param>
    /// <param name="charged">Selects charged projectile data and cooldown behavior when <see langword="true"/>.</param>
    /// <returns>The allocated projectile slot and routed sound, or no slot and no sound when admission fails.</returns>
    private (int? Slot, ushort Sound) TryFireBeam(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms,
        bool charged)
    {
        const ushort shoot = (ushort)SnesButton.X;

        // `$90:B823/$90:B986` converge on this allocation gate for every ordinary and
        // charged beam combination. Without Charge Beam, callers attempt a shot every held
        // frame and cooldown `$0CCC` supplies the native rate limit; charged releases reach
        // the same gate once.
        if (ProjectileCounter >= SlotCount ||
            (sharedProjectiles.CooldownTimer & 0x00ff) != 0)
        {
            return (null, 0);
        }

        // `$90:AC39` writes one before allocating storage. A corrupt counter/free-slot
        // disagreement is therefore intentionally observable rather than silently repaired.
        sharedProjectiles.SetSharedCooldown(1);
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));

        int slotIndex = Array.FindIndex(_slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
        {
            // Consistent state cannot arrive here. Match the producer's defensive rollback
            // so a debugger mutation does not leave the count permanently above five.
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            return (null, 0);
        }

        // The fire routines overwrite only the words they initialize. Words such as the
        // subpixels keep the slot's previous occupant until velocity setup, so the left
        // muzzle check's -1 speed ($90:BDA7) can carry back into the same whole pixel.
        SamusProjectileSlot slot = _slots[slotIndex];

        byte direction = samus.PoseTransitionShotDirection != 0
            ? unchecked((byte)samus.PoseTransitionShotDirection)
            : samus.ReadShotDirection(bus);

        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            return (null, 0);
        }

        slot.Direction = direction;
        InitializePosition(bus, samus, slot);
        slot.TrailTimer = 4;
        slot.Type = SamusProjectileTypeWord.CreateBeam(samus.EquippedBeams, charged);
        ProjectileInvincibilityTimer = 10;

        // The four low bits are a direct ROM table index, not four independent animation
        // layers composed by the host. In particular, Spazer's familiar three streaks live
        // in one bank-$93 spritemap selected by one data pointer and occupy one projectile
        // slot. This is why no synthetic child projectiles are created here.
        int beamType = slot.PackedType.BeamCombinationIndex;

        // `$93:8000` indexes a data-table pointer by beam type, stores damage, chooses the
        // direction-specific list, samples its initial radii, and arms a one-frame timer.
        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            (charged
                ? SamusProjectileRomData.Beams.ChargedDataPointers
                : SamusProjectileRomData.Beams.UnchargedDataPointers) + beamType * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + direction * 2)));
        slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = 1;

        // $90:B8ED checks Charge equipment and both input-edge latches before
        // selecting ordinary cooldown. Only uncharged auto-fire without those
        // conditions uses the longer held-fire delay.
        byte cooldown = charged
            ? SamusProjectileCooldownDefinitions.ReadByte(
                SamusProjectileRomData.Beams.UnchargedCooldowns +
                SamusProjectileRomData.Beams.ChargedRowOffset + beamType)
            : samus.EquippedBeams.HasAny(SamusBeamFlags.Charge) || (controllerNewInput & shoot) != 0
                ? SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + beamType)
                : SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.AutoFireCooldowns + beamType);
        sharedProjectiles.SetSharedCooldown(cooldown);

        ushort sound = SamusProjectileSoundRoutingDefinitions.Resolve(charged, beamType);
        if (charged)
            ChargedShotGlowTimer = 4;

        // FireUnchargedBeam and FireChargedBeam explicitly zero both speed words and call
        // CheckBeamCollByDir/WaveBeam_CheckColl before installing the pre-instruction and
        // initial speed. This is what lets a muzzle already overlapping a door cap trigger
        // its bank-$94 shot reaction instead of moving its leading edge beyond the cap.
        bool initialImpact = RunInitialBeamCollision(
            bus,
            level,
            slot,
            roomPlms,
            waveBeam: (beamType & 1) != 0,
            sharedProjectiles.PowerBombExplosion);
        if (!initialImpact)
        {
            // The cartridge installs the callback only after muzzle collision. Do not
            // infer it from Wave bits: the table is the dispatcher, including overreads.
            SamusBeamCallbackDefinition callback =
                SamusBeamCallbackDefinitions.Resolve(charged, beamType);
            slot.PreInstruction = callback.Translated ?? throw new NotSupportedException(
                $"Beam callback $90:{callback.NativePointer:X4} is not translated.");
            InitializePowerBeamVelocity(bus, slot);
        }
        return (slotIndex, sound);
    }

    /// <summary>Allocates the Hyper beam projectile, applies its fixed damage and presentation state, and checks its initial collision.</summary>
    /// <param name="bus">Address space supplying projectile tables and Samus movement data.</param>
    /// <param name="level">Room collision grid used to test the muzzle before movement begins.</param>
    /// <param name="samus">Shooter state supplying position and shot direction.</param>
    /// <param name="sharedProjectiles">Shared slot cooldown and power-bomb explosion state.</param>
    /// <param name="roomPlms">Optional sink for door or shootable-block reactions from the initial impact.</param>
    /// <returns>The allocated projectile slot and sound request, or no slot when capacity, cooldown, or direction rejects firing.</returns>
    private (int? Slot, ushort Sound) TryFireHyperBeam(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        RoomPlmSystem? roomPlms)
    {
        // `$90:AC39` is shared with normal beams: five counted ordinary slots and a nonzero
        // low cooldown byte reject firing before any slot fields are touched.
        if (ProjectileCounter >= SlotCount ||
            (sharedProjectiles.CooldownTimer & 0x00ff) != 0)
        {
            return (null, 0);
        }

        sharedProjectiles.SetSharedCooldown(1);
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));
        int slotIndex = Array.FindIndex(_slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            return (null, 0);
        }

        // As for ordinary beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slots[slotIndex];
        byte direction = samus.ReadShotDirection(bus);
        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            return (null, 0);
        }
        slot.Direction = direction;

        InitializePosition(bus, samus, slot);
        ProjectileInvincibilityTimer = 10;

        // The literal `$9018` is charged Plasma for bank-$93 data/art purposes even though
        // acquisition equips `$1009` Wave+Plasma tiles/palette. InitializeProjectile first
        // reads charged table entry eight; `$90:BD21` then deliberately replaces its damage
        // with 1000 before the first instruction record executes.
        slot.Type = 0x9018;
        const int hyperBeamType = 8;
        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.Beams.ChargedDataPointers + hyperBeamType * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + slot.PackedDirection.DirectionIndex * 2)));
        slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = 1;
        slot.Damage = 1000;

        // Hyper uses WaveBeam_CheckColl at this same producer seam. It publishes door and
        // shootable-block reactions but, like every Wave family, ignores solid carry and
        // survives to receive its normal movement state.
        _ = RunInitialBeamCollision(
            bus,
            level,
            slot,
            roomPlms,
            waveBeam: true,
            sharedProjectiles.PowerBombExplosion);
        slot.PreInstruction = SamusProjectilePreInstruction.HyperBeam;
        InitializePowerBeamVelocity(bus, slot);

        // These are literal stores at `$90:BD29-$BD52`. `$8014` is not an ordinary four-call
        // glow timer: its sign bit identifies Hyper's palette phase to the later consumer.
        sharedProjectiles.SetSharedCooldown(21);
        ChargedShotGlowTimer = 0x8014;
        _flareFrames[0] = 29;
        _flareFrames[1] = 5;
        _flareFrames[2] = 5;
        _flareTimers[0] = _flareTimers[1] = _flareTimers[2] = 3;
        FlareCounter = 0x8000;

        ushort sound = SamusProjectileSoundRoutingDefinitions.Resolve(
            charged: true, hyperBeamType);
        return (slotIndex, sound);
    }

    /// <summary>Attempts to fire a missile or Super Missile from a fresh or retained Shoot edge, consuming ammo on allocation.</summary>
    /// <param name="bus">Address space used to read Samus movement type and projectile data.</param>
    /// <param name="samus">Selected weapon, ammunition, direction, and position used to initialize the shot.</param>
    /// <param name="controllerNewInput">Current fresh-button word.</param>
    /// <param name="controllerPreviousNewInput">Previous filtered fresh-button word retained for the native delayed edge.</param>
    /// <param name="sharedProjectiles">Shared projectile cooldown and allocation state.</param>
    /// <returns>The allocated slot and sound request, or no slot and sound when input or admission conditions reject firing.</returns>
    internal (int? Slot, ushort Sound) TryFireMissile(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput,
        ushort controllerPreviousNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const ushort shoot = (ushort)SnesButton.X;

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
        int admissionLimit = isSuperMissile ? SamusProjectileRomData.NonBeam.SuperMissileAdmissionLimit : SlotCount;
        if (ProjectileCounter >= admissionLimit ||
            (sharedProjectiles.CooldownTimer & 0x00ff) != 0)
        {
            return (null, 0);
        }

        sharedProjectiles.SetSharedCooldown(1);
        if (ammo == 0)
            return (null, 0);

        int slotIndex = Array.FindIndex(_slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
            return (null, 0);

        // As for beams, the slot keeps its previous occupant's uninitialized words.
        SamusProjectileSlot slot = _slots[slotIndex];
        byte direction = samus.ReadShotDirection(bus);
        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
            return (null, 0);
        slot.Direction = direction;

        InitializePosition(bus, samus, slot);
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));
        ProjectileInvincibilityTimer = 20;
        if (isSuperMissile)
            samus.SuperMissiles = unchecked((ushort)(samus.SuperMissiles - 1));
        else
            samus.Missiles = unchecked((ushort)(samus.Missiles - 1));
        slot.TrailTimer = 4;
        slot.Type = isSuperMissile ? (ushort)0x8200 : (ushort)0x8100;
        slot.Variable = 0;

        // The producer first calls the generic velocity initializer with base speed zero.
        // Missile_Func1 replaces that zero with `$0100` during this same frame's alpha pass;
        // keeping both calls makes the one-frame ignition transition directly inspectable.
        InitializeDirectionalVelocity(bus, slot, baseSpeed: 0);

        ushort dataPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.NonBeam.DataPointers + samus.SelectedHudItem * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = SamusProjectileSelectionDefinitions.ReadWord(
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + slot.PackedDirection.DirectionIndex * 2)));
        slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = 1;
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

    /// <summary>Places a projectile at the direction-specific muzzle offset selected by Samus's pose and movement type.</summary>
    /// <param name="bus">Address space used to resolve Samus's movement type.</param>
    /// <param name="samus">Shooter pose and position supplying the origin and pose-specific Y adjustment.</param>
    /// <param name="slot">Projectile whose direction selects the muzzle offset and whose position is initialized.</param>
    private static void InitializePosition(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusProjectileSlot slot)
    {
        byte poseYOffset = SamusPoseProjectileOriginDefinitions.ReadYOffset(samus.Pose);
        SamusMovementType movementType = samus.ReadMovementType(bus);

        // `$90:BA94` uses the running/moonwalk origin table for movement type one and for
        // the two diagonally-up moonwalk poses $75/$76. Every other pose uses the default.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseIds.MoonwalkAimUpLeftPose or SamusPoseIds.MoonwalkAimUpRightPose;
        var origin = SamusProjectileOriginDefinitions.Read(runningOrigin, slot.Direction);
        slot.XPosition = unchecked((ushort)(samus.XPosition + origin.X));
        slot.YPosition = unchecked((ushort)(samus.YPosition + origin.Y - poseYOffset));
    }

    /// <summary>Selects the native speed row for the projectile's beam combination and direction, then applies directional inheritance.</summary>
    /// <param name="bus">Mutable SNES memory used to read Samus's inherited velocity.</param>
    /// <param name="slot">Beam projectile whose packed type and direction select its base speed and final velocity.</param>
    private static void InitializePowerBeamVelocity(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        bool diagonal = direction is 1 or 3 or 6 or 8;
        int rowOffset = slot.PackedType.BeamCombinationIndex * SamusProjectileRomData.Beams.InitialSpeedRowBytes;
        short speed = unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
            (diagonal
                ? SamusProjectileRomData.Beams.DiagonalSpeeds
                : SamusProjectileRomData.Beams.HorizontalVerticalSpeeds) + rowOffset));

        InitializeDirectionalVelocity(bus, slot, speed);
    }

    /// <summary>Clears projectile subpositions and combines its base speed with Samus's direction-dependent inherited velocity.</summary>
    /// <param name="bus">Mutable SNES memory containing Samus's live velocity words.</param>
    /// <param name="slot">Projectile whose direction determines inherited velocity components.</param>
    /// <param name="baseSpeed">Projectile-family speed added to the inherited velocity.</param>
    private static void InitializeDirectionalVelocity(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        short baseSpeed)
    {
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        ISnesMutableMemory memory = bus as ISnesMutableMemory ??
            throw new InvalidOperationException("Projectile inheritance requires live WRAM.");
        (slot.XVelocity, slot.YVelocity) = SamusProjectileInheritance.ReadVelocity(memory, slot.Direction, baseSpeed);
    }

}
