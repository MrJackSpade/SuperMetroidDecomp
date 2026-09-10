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
        LoadNormalSuitPalette(bus, cgram, samus.EquippedItems);
    }

    private void CancelCharge()
    {
        FlareCounter = 0;
        PreviousBeamChargeCounter = 0;
        ClearFlareAnimationState();
    }

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

        // Retail owns twelve low-nibble beam combinations: power through ice+wave+plasma.
        // Spazer and plasma are mutually exclusive in normal inventory state, which is why
        // indices `$C-$F` have no tile, palette, projectile-data, sound, or dispatch entry.
        SamusBeamLoadoutWord beamLoadout = samus.EquippedBeams;
        int beamType = beamLoadout.CombinationIndex;
        if ((uint)beamType >= 12)
            return (null, 0, 0);

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
        if (firing.Sound != 0)
            return (firing.Slot, firing.Sound, MaximumQueued: 15);

        return PreviousBeamChargeCounter >=
            SamusProjectileRomData.Beams.ChargeSoundStartCounter
            ? (null, SoundEffectLibrary1Sounds.CancelAll.Value, MaximumQueued: 15)
            : (null, 0, 0);
    }

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

        SamusProjectileSlot slot = _slots[slotIndex];
        slot.ClearFields();

        byte direction = samus.PoseTransitionShotDirection != 0
            ? unchecked((byte)samus.PoseTransitionShotDirection)
            : ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);

        if (!new SamusProjectileDirectionWord(direction).IsValidInitialDirection)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            slot.ClearFields();
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
        ushort dataPointer = ReadWord(
            bus,
            (charged
                ? SamusProjectileRomData.Beams.ChargedDataPointers
                : SamusProjectileRomData.Beams.UnchargedDataPointers) + beamType * 2);
        slot.Damage = ReadWord(bus, SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + direction * 2)));
        slot.XRadius = bus.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = bus.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = 1;

        // A fresh press takes the ordinary table path. Held auto-fire without a new edge
        // uses $19 instead, preserving the native distinction even though both read ROM.
        byte cooldown = charged
            ? bus.ReadByte(
                SamusProjectileRomData.Beams.UnchargedCooldowns +
                SamusProjectileRomData.Beams.CooldownCancelRowOffset + beamType)
            : (controllerNewInput & shoot) != 0
                ? bus.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + beamType)
                : bus.ReadByte(SamusProjectileRomData.Beams.AutoFireCooldowns + beamType);
        sharedProjectiles.SetSharedCooldown(cooldown);

        ushort sound = ReadWord(
            bus,
            (charged
                ? SamusProjectileRomData.Beams.ChargedSounds
                : SamusProjectileRomData.Beams.UnchargedSounds) + beamType * 2);
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
            waveBeam: (beamType & 1) != 0);
        if (!initialImpact)
        {
            // The cartridge installs the callback only after muzzle collision. Do not
            // infer it from Wave bits: the table is the dispatcher, including overreads.
            ushort callback = ReadWord(bus, (charged
                ? SamusBeamPreInstructionCodes.ChargedTable
                : SamusBeamPreInstructionCodes.UnchargedTable) + slot.PackedType.BeamCombinationIndex * sizeof(ushort));
            slot.PreInstruction = callback switch
            {
                SamusBeamPreInstructionCodes.NoWave => SamusProjectilePreInstruction.NoWaveBeam,
                SamusBeamPreInstructionCodes.WaveThreeFrameTrail => SamusProjectilePreInstruction.WaveBeamThreeFrameTrail,
                SamusBeamPreInstructionCodes.WaveFourFrameTrail => SamusProjectilePreInstruction.WaveBeamFourFrameTrail,
                _ => throw new NotSupportedException($"Beam callback $90:{callback:X4} is not translated."),
            };
            InitializePowerBeamVelocity(bus, slot);
        }
        return (slotIndex, sound);
    }

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

        SamusProjectileSlot slot = _slots[slotIndex];
        slot.ClearFields();
        slot.Direction = ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);
        if (!slot.PackedDirection.IsValidInitialDirection)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            slot.ClearFields();
            return (null, 0);
        }

        InitializePosition(bus, samus, slot);
        ProjectileInvincibilityTimer = 10;

        // The literal `$9018` is charged Plasma for bank-$93 data/art purposes even though
        // acquisition equips `$1009` Wave+Plasma tiles/palette. InitializeProjectile first
        // reads charged table entry eight; `$90:BD21` then deliberately replaces its damage
        // with 1000 before the first instruction record executes.
        slot.Type = 0x9018;
        const int hyperBeamType = 8;
        ushort dataPointer = ReadWord(
            bus,
            SamusProjectileRomData.Beams.ChargedDataPointers + hyperBeamType * 2);
        slot.Damage = ReadWord(bus, SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + slot.PackedDirection.DirectionIndex * 2)));
        slot.XRadius = bus.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = bus.ReadByte(
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
            waveBeam: true);
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

        ushort sound = ReadWord(
            bus,
            SamusProjectileRomData.Beams.ChargedSounds + hyperBeamType * 2);
        return (slotIndex, sound);
    }

    private (int? Slot, ushort Sound) TryFireMissile(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput,
        ushort controllerPreviousNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const ushort shoot = (ushort)SnesButton.X;

        // `$90:BE65-$BE72` accepts either the current new-press word or the previous filtered
        // drawing word. Ordinary live input normally makes the second word zero; bank-$91
        // demo playback intentionally republishes its prior edge there. Cooldown still owns
        // admission, so the delayed copy cannot manufacture desktop auto-fire.
        if (((controllerNewInput | controllerPreviousNewInput) & shoot) == 0)
            return (null, 0);

        // `$90:AC5A` increments the common counter before the ammo/free-slot checks later in
        // `$BE62`; every failing branch rolls it back. Testing the stable preconditions first
        // yields the same externally visible state without temporarily corrupting debugger
        // watches between C# statements.
        bool isSuperMissile = samus.SelectedHudItem == 2;
        ushort ammo = isSuperMissile ? samus.SuperMissiles : samus.Missiles;
        if (ProjectileCounter >= SlotCount ||
            (sharedProjectiles.CooldownTimer & 0x00ff) != 0 ||
            ammo == 0)
        {
            return (null, 0);
        }

        int slotIndex = Array.FindIndex(_slots, candidate => candidate.Damage == 0);
        if (slotIndex < 0)
            return (null, 0);

        SamusProjectileSlot slot = _slots[slotIndex];
        slot.ClearFields();
        slot.Direction = ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);
        if (!slot.PackedDirection.IsValidInitialDirection)
            return (null, 0);

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
        InitializeDirectionalVelocity(slot, baseSpeed: 0);

        ushort dataPointer = ReadWord(
            bus,
            SamusProjectileRomData.NonBeam.DataPointers + samus.SelectedHudItem * 2);
        slot.Damage = ReadWord(bus, SamusProjectileRomData.Banks.Projectile | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            SamusProjectileRomData.Banks.Projectile |
                unchecked((ushort)(dataPointer + 2 + slot.PackedDirection.DirectionIndex * 2)));
        slot.XRadius = bus.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 4))));
        slot.YRadius = bus.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.ProjectileNumber,
                unchecked((ushort)(slot.InstructionPointer + 5))));
        slot.InstructionTimer = 1;
        slot.PreInstruction = isSuperMissile
            ? SamusProjectilePreInstruction.SuperMissile
            : SamusProjectilePreInstruction.Missile;

        // Retail constants embedded beside the bank-$90 producer: missile sound library-one
        // effect three and ten-frame shared cooldown. Empty ammo auto-deselects the HUD item.
        sharedProjectiles.SetSharedCooldown(isSuperMissile ? (ushort)20 : (ushort)10);
        if ((isSuperMissile ? samus.SuperMissiles : samus.Missiles) == 0)
            samus.SelectedHudItem = 0;
        return (slotIndex, isSuperMissile ? (ushort)4 : (ushort)3);
    }

    private static void InitializePosition(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusProjectileSlot slot)
    {
        int directionOffset = (slot.Direction & 0x0f) * 2;
        byte poseYOffset = ReadPoseByte(bus, samus.Pose, PoseYOffsetOffset);
        SamusMovementType movementType = samus.ReadMovementType(bus);

        // `$90:BA94` uses the running/moonwalk origin table for movement type one and for
        // the two diagonally-up moonwalk poses $75/$76. Every other pose uses the default.
        bool runningOrigin = movementType == SamusMovementType.Running ||
            samus.Pose is SamusPoseIds.MoonwalkAimUpLeftPose or SamusPoseIds.MoonwalkAimUpRightPose;
        int xTable = runningOrigin
            ? SamusProjectileRomData.Origins.RunningX
            : SamusProjectileRomData.Origins.DefaultX;
        int yTable = runningOrigin
            ? SamusProjectileRomData.Origins.RunningY
            : SamusProjectileRomData.Origins.DefaultY;

        short xOffset = unchecked((short)ReadWord(bus, xTable + directionOffset));
        short yOffset = unchecked((short)ReadWord(bus, yTable + directionOffset));
        slot.XPosition = unchecked((ushort)(samus.XPosition + xOffset));
        slot.YPosition = unchecked((ushort)(samus.YPosition + yOffset - poseYOffset));
    }

    private static void InitializePowerBeamVelocity(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        bool diagonal = direction is 1 or 3 or 6 or 8;
        int rowOffset = slot.PackedType.BeamCombinationIndex * SamusProjectileRomData.Beams.InitialSpeedRowBytes;
        short speed = unchecked((short)ReadWord(
            bus,
            (diagonal
                ? SamusProjectileRomData.Beams.DiagonalSpeeds
                : SamusProjectileRomData.Beams.HorizontalVerticalSpeeds) + rowOffset));

        InitializeDirectionalVelocity(slot, speed);
    }

    private static void InitializeDirectionalVelocity(
        SamusProjectileSlot slot,
        short baseSpeed)
    {
        byte direction = unchecked((byte)(slot.Direction & 0x0f));

        // Projectile inheritance in `$90:B1F3` samples the PREVIOUS frame's four signed
        // displacement words. The translated movement owners reset those words at the end
        // of beta and do not yet expose their byte-overlap garbage. Zero is therefore the
        // exact initialized/debug-standing value, not a fabricated running multiplier.
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        slot.XVelocity = direction switch
        {
            1 or 2 or 3 => baseSpeed,
            6 or 7 or 8 => unchecked((short)-baseSpeed),
            0 or 4 or 5 or 9 => 0,
            _ => throw new InvalidDataException(
                $"Projectile velocity initialization received invalid direction ${direction:X2}."),
        };
        slot.YVelocity = direction switch
        {
            0 or 1 or 8 or 9 => unchecked((short)-baseSpeed),
            3 or 4 or 5 or 6 => baseSpeed,
            2 or 7 => 0,
            _ => throw new InvalidDataException(
                $"Projectile velocity initialization received invalid direction ${direction:X2}."),
        };
    }

}
