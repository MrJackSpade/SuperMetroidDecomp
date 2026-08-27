using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five ordinary Samus-projectile slots at WRAM byte indices <c>$00-$08</c>.
/// </summary>
/// <remarks>
/// This class is deliberately a field-for-field translation rather than a desktop particle
/// effect. Firing comes from bank $90, damage and animation records come from bank $93,
/// and terrain movement comes from bank $94. Keeping those seams visible makes every value
/// inspectable in a C# debugger and prevents a convenient host sprite from replacing the
/// actual cartridge projectile lifecycle.
///
/// The implemented producer covers ordinary and charged plain power beams. The shared slot
/// model and bank-$93 interpreter are intentionally general enough for the remaining beam
/// and missile families, but unsupported producers are rejected before allocating a slot.
/// </remarks>
public sealed class SamusProjectileSystem
{
    /// <summary>Native ordinary-projectile capacity; bombs occupy the other five slots.</summary>
    public const int SlotCount = 5;

    /// <summary>Live, uncharged beam flag written by <c>Fire_Uncharge_Beam</c>.</summary>
    public const ushort LiveUnchargedBeamFlag = 0x8000;

    /// <summary>Beam-explosion family installed by <c>Kill_Projectile</c>.</summary>
    public const ushort BeamExplosionFamily = 0x0700;

    private const int PoseDefinitions = 0x91b629;
    private const int PoseDirectionOffset = 3;
    private const int PoseYOffsetOffset = 4;
    private const int ProjectileXDefault = 0x90c204;
    private const int ProjectileYDefault = 0x90c218;
    private const int ProjectileXRunning = 0x90c22c;
    private const int ProjectileYRunning = 0x90c240;
    private const int UnchargedCooldowns = 0x90c254;
    private const int UnchargedSounds = 0x90c28f;
    private const int ChargedSounds = 0x90c2a7;
    private const int BeamSpeedsHorizontalVertical = 0x90c2d1;
    private const int BeamSpeedsDiagonal = 0x90c2d3;
    private const int ProjectileAccelerationX = 0x90c353;
    private const int ProjectileAccelerationY = 0x90c367;
    private const int BeamTilePointers = 0x90c3b1;
    private const int BeamPalettePointers = 0x90c3c9;
    private const int UnchargedBeamDataPointers = 0x9383c1;
    private const int ChargedBeamDataPointers = 0x9383d9;
    private const int BeamExplosionInstructionPointerAddress = 0x9383ff;
    private const ushort ProjectileInstructionDelete = 0x822f;
    private const ushort ProjectileInstructionGoto = 0x8239;

    private readonly SamusProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount).Select(index => new SamusProjectileSlot(index)).ToArray();

    /// <summary>The slots in native low-to-high byte-index order: $00, $02, ... $08.</summary>
    public IReadOnlyList<SamusProjectileSlot> Slots => _slots;

    /// <summary>WRAM <c>$0CCE</c>; maintained separately from free-slot scans by the ROM.</summary>
    public ushort ProjectileCounter { get; private set; }

    /// <summary>WRAM <c>$0CD0</c>; 60 frames arms a charged shot, 120 is the SBA clamp.</summary>
    public ushort FlareCounter { get; private set; }

    /// <summary>WRAM <c>$0DC2</c>; sampled before the current charge-input update.</summary>
    public ushort PreviousBeamChargeCounter { get; private set; }

    /// <summary>
    /// WRAM <c>$0B18</c>; four calls of post-shot glow state. The palette consumer is a
    /// separate, still-untranslated presentation routine, so this value is debugger-visible
    /// and cadence-correct but does not yet recolor Samus.
    /// </summary>
    public ushort ChargedShotGlowTimer { get; private set; }

    private readonly ushort[] _flareFrames = new ushort[3];
    private readonly ushort[] _flareTimers = new ushort[3];

    /// <summary>The last alpha-pass result, retained for debugger watches and verification.</summary>
    public SamusProjectileFrameResult LastFrameResult { get; private set; }

    /// <summary>
    /// Executes <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D for the equipped beam.
    /// </summary>
    /// <remarks>
    /// The 256 tile bytes are DMAed from bank $9A to VRAM word $6300. Sixteen palette
    /// words are copied from bank $90 into sprite palette six, CGRAM colors $E0-$EF.
    /// Projectile spritemaps already reference those hardware locations; omitting this
    /// room/equipment-time upload produces valid OAM whose pixels are all transparent.
    /// </remarks>
    public void LoadBeamTilesAndPalette(
        ISnesAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        ushort equippedBeams)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);

        int beamType = equippedBeams & 0x0fff;
        if ((uint)beamType >= 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(equippedBeams),
                "Retail beam tile/palette tables contain twelve low-bit combinations.");
        }

        ushort tilePointer = ReadWord(bus, BeamTilePointers + beamType * 2);
        vram.ExecuteQueuedWrite(
            bus,
            0x9a0000 | tilePointer,
            sizeInBytes: 0x0100,
            encodedDestination: 0x6300);

        ushort palettePointer = ReadWord(bus, BeamPalettePointers + beamType * 2);
        cgram.LoadFromBus(
            bus,
            0x900000 | palettePointer,
            colorCount: 16,
            destinationIndex: 0xe0);
    }

    /// <summary>
    /// Queues the tile half of $90:AC8D after the room's standard-sprite upload and applies
    /// the matching final palette to the modeled CGRAM buffer.
    /// </summary>
    public void QueueBeamTilesAndLoadPalette(
        ISnesAddressSpace bus,
        VramWriteQueue writes,
        SnesCgram cgram,
        ushort equippedBeams)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(writes);
        ArgumentNullException.ThrowIfNull(cgram);

        int beamType = equippedBeams & 0x0fff;
        if ((uint)beamType >= 12)
            throw new ArgumentOutOfRangeException(nameof(equippedBeams));

        ushort tilePointer = ReadWord(bus, BeamTilePointers + beamType * 2);
        writes.Enqueue(
            sizeInBytes: 0x0100,
            sourceAddress: 0x9a0000 | tilePointer,
            encodedVramDestination: 0x6300);

        ushort palettePointer = ReadWord(bus, BeamPalettePointers + beamType * 2);
        cgram.LoadFromBus(
            bus,
            0x900000 | palettePointer,
            colorCount: 16,
            destinationIndex: 0xe0);
    }

    /// <summary>
    /// Runs the power-beam HUD producer and then handles all allocated ordinary slots.
    /// </summary>
    /// <param name="sharedProjectiles">
    /// Bomb-slot owner carrying shared WRAM cooldown <c>$0CCC</c>. The runtime steps that
    /// clock once, immediately before calling this method; this method may replace it after
    /// a successful shot just as bank $90 does.
    /// </param>
    public SamusProjectileFrameResult StepFrame(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        ushort layer1X,
        ushort layer1Y,
        SamusBombProjectileSystem sharedProjectiles,
        bool projectileProducerEnabled = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);

        int? firedSlot = null;
        ushort queuedSound = 0;

        // HUD indices zero and three share `$90:B80D`. A humanoid Samus may therefore fire
        // her beam with either Nothing or Power Bombs selected. Ball poses dispatch to the
        // separate bomb producer, and the debug grapple selector replaces this producer.
        if (projectileProducerEnabled &&
            samus.SelectedHudItem is 0 or 3 &&
            !SamusState.IsStableBallPose(samus.Pose))
        {
            (firedSlot, queuedSound) = HandleBeamInput(
                bus,
                samus,
                controllerInput,
                controllerNewInput,
                sharedProjectiles);
        }

        bool collisionStartedExplosion = false;
        bool projectileDeleted = false;

        // `$90:AECE` walks the complete ten-slot arrays from byte index $12 down to zero.
        // Bombs were handled by the companion class first; preserve the ordinary half's
        // descending $08->$00 order here because instruction deletion changes the counter.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = _slots[slotIndex];
            if (slot.InstructionPointer == 0)
                continue;

            if (slot.PreInstruction == SamusProjectilePreInstruction.NoWaveBeam)
            {
                collisionStartedExplosion |= RunNoWaveBeamPreInstruction(
                    bus,
                    level,
                    slot,
                    layer1X,
                    layer1Y);
            }

            // Kill_Projectile replaces rather than clears a live beam. Consequently the
            // explosion's first bank-$93 record is selected in this same handler pass.
            if (slot.InstructionPointer != 0)
                projectileDeleted |= RunProjectileInstructionHandler(bus, slot);
        }

        LastFrameResult = new SamusProjectileFrameResult(
            firedSlot,
            queuedSound,
            collisionStartedExplosion,
            projectileDeleted);
        return LastFrameResult;
    }

    /// <summary>
    /// Runs and draws <c>$90:BAFC</c>'s three charge-flare animation components before Samus.
    /// </summary>
    public void HandleChargeFlareAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(samus);

        if (ChargedShotGlowTimer != 0)
            ChargedShotGlowTimer--;
        if (FlareCounter == 0)
            return;

        // Calls 1..14 initialize/retain the hidden animation state. Call 15 begins drawing
        // the central flare; calls 30+ additionally draw both orbiting spark components.
        if (FlareCounter == 1)
        {
            _flareFrames[0] = _flareFrames[1] = _flareFrames[2] = 0;
            _flareTimers[0] = 3;
            _flareTimers[1] = 5;
            _flareTimers[2] = 4;
        }
        if (FlareCounter < 15)
            return;

        int componentCount = FlareCounter < 30 ? 1 : 3;
        for (int component = 0; component < componentCount; component++)
        {
            AdvanceFlareComponent(bus, component);
            DrawFlareComponent(bus, oam, samus, layer1X, layer1Y, component);
        }
    }

    /// <summary>Draws `$93:8254`'s live ordinary-projectile subset after Samus.</summary>
    public void DrawLiveProjectiles(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive || (slot.Type & 0x0f00) >= 0x0300)
                continue;

            // `$93:8268` first exempts every charged/projectile-family word selected by
            // mask `$0F10`. Only an ordinary uncharged power/ice/wave shot reaches the
            // alternating frame/slot test at `$826D-$828A`; a charged power shot must remain
            // visible on both parities. Slot index parity here equals bit one of native X.
            if ((slot.Type & 0x0f10) == 0)
            {
                bool oddNativeSlot = (slotIndex & 1) != 0;
                bool oddFrame = (nmiFrameCounter & 1) != 0;
                if (oddNativeSlot == oddFrame)
                    continue;
            }

            DrawSlot(bus, oam, slot, layer1X, layer1Y, horizontalMargin: 64);
        }
    }

    /// <summary>
    /// Draws beam explosions in bank-$A0's earlier bomb/explosion phase.
    /// </summary>
    public void DrawExplosions(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive || (slot.Type & 0x0f00) != BeamExplosionFamily)
                continue;

            DrawSlot(bus, oam, slot, layer1X, layer1Y, horizontalMargin: 48);
        }
    }

    /// <summary>Clears all five ordinary slots and the separately maintained counter.</summary>
    public void Reset()
    {
        foreach (SamusProjectileSlot slot in _slots)
            slot.ClearFields();
        ProjectileCounter = 0;
        FlareCounter = 0;
        PreviousBeamChargeCounter = 0;
        ChargedShotGlowTimer = 0;
        Array.Clear(_flareFrames);
        Array.Clear(_flareTimers);
        LastFrameResult = default;
    }

    private (int? Slot, ushort Sound) HandleBeamInput(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const ushort shoot = (ushort)SnesButton.X;
        PreviousBeamChargeCounter = FlareCounter;

        // Native `$90:B80D` can also consume charge through the one-frame `$0CFA`
        // "projectile direction changed by pose" bridge. The translated pose pipeline does
        // not publish that overlapped WRAM word yet, so standing/running/air poses with a
        // stable shot direction follow the exact path below; firing during the transitional
        // direction-change frame remains an explicit producer seam rather than a guess.

        // This slice admits power plus the independent Charge bit. Ice/wave/spazer/plasma
        // select different collision pre-instructions and must not leak through the no-wave
        // path until those handlers are translated.
        if ((samus.EquippedBeams & 0x000f) != 0)
            return (null, 0);

        bool chargeEquipped = (samus.EquippedBeams & 0x1000) != 0;
        bool held = (controllerInput & shoot) != 0;
        if (!chargeEquipped)
        {
            FlareCounter = 0;
            ClearFlareAnimationState();
            return held
                ? TryFirePowerBeam(bus, samus, controllerNewInput, sharedProjectiles, charged: false)
                : (null, 0);
        }

        if (held)
        {
            // `$90:B843` increments through 120 and fires one ordinary shot on the first
            // held frame. Charge Beam therefore changes sustained-fire semantics rather
            // than suppressing the familiar initial power shot.
            if (FlareCounter < 120)
            {
                FlareCounter++;
                if (FlareCounter == 1)
                {
                    ClearFlareAnimationState();
                    return TryFirePowerBeam(
                        bus,
                        samus,
                        controllerNewInput,
                        sharedProjectiles,
                        charged: false);
                }
            }
            return (null, 0);
        }

        if (FlareCounter == 0)
            return (null, 0);

        bool releaseCharged = FlareCounter >= 60;
        FlareCounter = 0;
        ClearFlareAnimationState();
        return TryFirePowerBeam(
            bus,
            samus,
            controllerNewInput,
            sharedProjectiles,
            charged: releaseCharged);
    }

    private (int? Slot, ushort Sound) TryFirePowerBeam(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
        bool charged)
    {
        const ushort shoot = (ushort)SnesButton.X;

        // `$90:B823/$90:B986` converge on this allocation gate for ordinary and charged
        // power shots. Without Charge Beam, callers attempt an ordinary shot every held
        // frame and cooldown `$0CCC` supplies the native rate limit; charged releases reach
        // the same gate once. Other beam combinations remain excluded until their distinct
        // wave/spazer/plasma pre-instructions are translated.
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

        byte poseDirection = ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);
        byte direction = poseDirection;

        // `$91:F5CF-$F5E6` publishes the OLD pose's direction when a fresh Shot press causes
        // a gun-pose transition. `$90:BA5F` consumes its low byte before the prospective pose
        // is installed. Reconstructing that one-frame bridge here avoids a shot lag and does
        // not guess from host-facing artwork.
        if ((controllerNewInput & shoot) != 0)
            direction = unchecked((byte)(poseDirection & 0x0f));

        if ((direction & 0xf0) != 0 || (direction & 0x0f) > 9)
        {
            ProjectileCounter = unchecked((ushort)(ProjectileCounter - 1));
            slot.ClearFields();
            return (null, 0);
        }

        slot.Direction = direction;
        InitializePosition(bus, samus, slot);
        slot.TrailTimer = 4;
        slot.Type = charged
            ? unchecked((ushort)((samus.EquippedBeams & 0x100f) | 0x8010))
            : unchecked((ushort)(samus.EquippedBeams | LiveUnchargedBeamFlag));

        // `$93:8000` indexes a data-table pointer by beam type, stores damage, chooses the
        // direction-specific list, samples its initial radii, and arms a one-frame timer.
        ushort dataPointer = ReadWord(
            bus,
            charged ? ChargedBeamDataPointers : UnchargedBeamDataPointers);
        slot.Damage = ReadWord(bus, 0x930000 | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            0x930000 | unchecked((ushort)(dataPointer + 2 + direction * 2)));
        slot.XRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 5)));
        slot.InstructionTimer = 1;
        slot.PreInstruction = SamusProjectilePreInstruction.NoWaveBeam;

        InitializePowerBeamVelocity(bus, slot);

        // A fresh press takes the ordinary table path. Held auto-fire without a new edge
        // uses $19 instead, preserving the native distinction even though both read ROM.
        byte cooldown = charged
            ? bus.ReadByte(UnchargedCooldowns + 0x10)
            : (controllerNewInput & shoot) != 0
                ? bus.ReadByte(UnchargedCooldowns)
                : bus.ReadByte(0x90c283);
        sharedProjectiles.SetSharedCooldown(cooldown);

        ushort sound = ReadWord(bus, charged ? ChargedSounds : UnchargedSounds);
        if (charged)
            ChargedShotGlowTimer = 4;
        return (slotIndex, sound);
    }

    private static void InitializePosition(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusProjectileSlot slot)
    {
        int directionOffset = (slot.Direction & 0x0f) * 2;
        byte poseYOffset = ReadPoseByte(bus, samus.Pose, PoseYOffsetOffset);
        byte movementType = samus.ReadMovementType(bus);

        // `$90:BA94` uses the running/moonwalk origin table for movement type one and for
        // the two diagonally-up moonwalk poses $75/$76. Every other pose uses the default.
        bool runningOrigin = movementType == 1 ||
            samus.Pose is SamusState.MoonwalkAimUpLeftPose or SamusState.MoonwalkAimUpRightPose;
        int xTable = runningOrigin ? ProjectileXRunning : ProjectileXDefault;
        int yTable = runningOrigin ? ProjectileYRunning : ProjectileYDefault;

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
        short speed = unchecked((short)ReadWord(
            bus,
            diagonal ? BeamSpeedsDiagonal : BeamSpeedsHorizontalVertical));

        // Projectile inheritance in `$90:B1F3` samples the PREVIOUS frame's four signed
        // displacement words. The translated movement owners reset those words at the end
        // of beta and do not yet expose their byte-overlap garbage. Zero is therefore the
        // exact initialized/debug-standing value, not a fabricated running multiplier.
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        slot.XVelocity = direction switch
        {
            1 or 2 or 3 => speed,
            6 or 7 or 8 => unchecked((short)-speed),
            _ => 0,
        };
        slot.YVelocity = direction switch
        {
            0 or 1 or 8 or 9 => unchecked((short)-speed),
            3 or 4 or 5 or 6 => speed,
            _ => 0,
        };
    }

    private bool RunNoWaveBeamPreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return false;
        }

        // Trails are allocated every fourth frame at $90:AF00. Their two independent
        // bank-$90 instruction streams are the next projectile-animation slice; retain the
        // exact timer now so adding their visible slots cannot shift live-beam timing later.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
            slot.TrailTimer = 4;

        int directionOffset = (slot.Direction & 0x0f) * 2;
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + directionOffset))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + directionOffset))));

        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        bool collided = direction switch
        {
            0 or 4 or 5 or 9 => MoveVertically(level, slot),
            2 or 7 => MoveHorizontally(level, slot),
            1 or 3 or 6 or 8 =>
                MoveHorizontally(level, slot) || MoveVertically(level, slot),
            _ => false,
        };

        if (collided)
        {
            KillBeam(bus, slot);
            return true;
        }

        short screenX = unchecked((short)(slot.XPosition - layer1X));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        if (screenX < -64 || screenX >= 320 || screenY < -64 || screenY >= 320)
            ClearProjectile(slot);

        return false;
    }

    private static bool MoveHorizontally(RoomLevelData level, SamusProjectileSlot slot)
    {
        (slot.XPosition, slot.XSubposition) = AddVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity);

        int topBlock = unchecked((ushort)(slot.YPosition - slot.YRadius)) >> 4;
        int bottomBlock = unchecked((ushort)(slot.YPosition + slot.YRadius - 1)) >> 4;
        int targetX = slot.XVelocity < 0
            ? unchecked((ushort)(slot.XPosition - slot.XRadius))
            : unchecked((ushort)(slot.XPosition + slot.XRadius - 1));
        int blockX = targetX >> 4;

        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)topBlock >= (uint)level.HeightInBlocks ||
            (uint)bottomBlock >= (uint)level.HeightInBlocks)
        {
            return false;
        }

        // The native target-collision counter starts at span-1 and goes negative only when
        // every crossed block returns carry set. A tall beam grazing air beside one solid
        // block therefore keeps moving; this is not an ordinary "any overlap" AABB test.
        for (int blockY = topBlock; blockY <= bottomBlock; blockY++)
        {
            if (!IsUnconditionallySolidShotBlock(level.GetCollisionBlock(blockX, blockY)))
                return false;
        }
        return true;
    }

    private static bool MoveVertically(RoomLevelData level, SamusProjectileSlot slot)
    {
        (slot.YPosition, slot.YSubposition) = AddVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity);

        int leftBlock = unchecked((ushort)(slot.XPosition - slot.XRadius)) >> 4;
        int rightBlock = unchecked((ushort)(slot.XPosition + slot.XRadius - 1)) >> 4;
        int targetY = slot.YVelocity < 0
            ? unchecked((ushort)(slot.YPosition - slot.YRadius))
            : unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int blockY = targetY >> 4;

        if ((uint)blockY >= (uint)level.HeightInBlocks ||
            (uint)leftBlock >= (uint)level.WidthInBlocks ||
            (uint)rightBlock >= (uint)level.WidthInBlocks)
        {
            return false;
        }

        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
        {
            if (!IsUnconditionallySolidShotBlock(level.GetCollisionBlock(blockX, blockY)))
                return false;
        }
        return true;
    }

    private static bool IsUnconditionallySolidShotBlock(RoomCollisionBlock block)
    {
        // `$94:A175/$A195` return carry for 8/B/E directly. Door ($9), spike ($A),
        // shootable ($C), and bombable ($F) blocks also collide after their actor/PLM
        // reactions; this initial power-beam slice has no Landing Site instance requiring
        // those producers, but retaining their collision result prevents tunneling.
        return block.CollisionType is >= 8 and <= 15;
    }

    private static (ushort Position, ushort Subposition) AddVelocity(
        ushort position,
        ushort subposition,
        short velocity)
    {
        // Bank $94 overlaps DP $12/$13 so an 8.8 velocity becomes a signed 16.16 delta by
        // shifting it left eight. Performing the addition as one wrapped 32-bit quantity is
        // exactly equivalent to its low-word ADC followed by sign-word ADC.
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = unchecked((uint)(fixedPosition + ((int)velocity << 8)));
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private static void KillBeam(ISnesAddressSpace bus, SamusProjectileSlot slot)
    {
        // `$90:AE3A` moves the explosion anchor to the leading edge before bank $93 swaps
        // the instruction list. Diagonals adjust both axes in their respective signs.
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        if (direction is 1 or 2 or 3)
            slot.XPosition = unchecked((ushort)(slot.XPosition + slot.XRadius));
        else if (direction is 6 or 7 or 8)
            slot.XPosition = unchecked((ushort)(slot.XPosition - slot.XRadius));

        if (direction is 0 or 1 or 8 or 9)
            slot.YPosition = unchecked((ushort)(slot.YPosition - slot.YRadius));
        else if (direction is 3 or 4 or 5 or 6)
            slot.YPosition = unchecked((ushort)(slot.YPosition + slot.YRadius));

        slot.Type = unchecked((ushort)((slot.Type & 0xf0ff) | BeamExplosionFamily));
        slot.InstructionPointer = ReadWord(bus, BeamExplosionInstructionPointerAddress);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;
    }

    private bool RunProjectileInstructionHandler(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (slot.InstructionTimer != 0)
            return false;

        ushort pointer = slot.InstructionPointer;
        while (true)
        {
            ushort instructionOrTimer = ReadWord(bus, 0x930000 | pointer);
            if ((instructionOrTimer & 0x8000) == 0)
            {
                slot.InstructionTimer = instructionOrTimer;
                slot.SpritemapPointer = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 2));
                slot.XRadius = bus.ReadByte(0x930000 | AddWithinBank(pointer, 4));
                slot.YRadius = bus.ReadByte(0x930000 | AddWithinBank(pointer, 5));
                slot.AnimationFrame = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 6));
                slot.InstructionPointer = unchecked((ushort)(pointer + 8));
                return false;
            }

            if (instructionOrTimer == ProjectileInstructionDelete)
            {
                ClearProjectile(slot);
                return true;
            }

            if (instructionOrTimer == ProjectileInstructionGoto)
            {
                pointer = ReadWord(bus, 0x930000 | AddWithinBank(pointer, 2));
                continue;
            }

            throw new InvalidOperationException(
                $"Unsupported bank-$93 projectile instruction ${instructionOrTimer:X4} " +
                $"at $93:{pointer:X4}.");
        }
    }

    private void ClearProjectile(SamusProjectileSlot slot)
    {
        slot.ClearFields();
        ProjectileCounter = ProjectileCounter == 0
            ? (ushort)0
            : unchecked((ushort)(ProjectileCounter - 1));
    }

    private static void DrawSlot(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        int horizontalMargin)
    {
        short screenX = unchecked((short)(slot.XPosition - layer1X));
        ushort screenY = unchecked((ushort)(slot.YPosition - layer1Y));
        if (screenX < -horizontalMargin || screenX >= 256 + horizontalMargin ||
            (screenY & 0xff00) != 0)
        {
            return;
        }

        oam.AddProjectileSpritemap(
            bus,
            slot.SpritemapPointer,
            unchecked((ushort)screenX),
            screenY);
    }

    private void AdvanceFlareComponent(ISnesAddressSpace bus, int component)
    {
        // The assembly advances only when 16-bit DEC crosses zero into `$FFFF`. A timer
        // value of zero therefore survives one visible call; testing equality here would
        // make every ROM-authored delay one frame too short.
        _flareTimers[component] = unchecked((ushort)(_flareTimers[component] - 1));
        if ((_flareTimers[component] & 0x8000) == 0)
            return;

        ushort frame = unchecked((ushort)(_flareFrames[component] + 1));
        ushort delayList = ReadWord(bus, 0x90c481 + component * 2);
        byte delay = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame)));
        if (delay == 0xff)
        {
            frame = 0;
            delay = bus.ReadByte(0x900000 | delayList);
        }
        else if (delay == 0xfe)
        {
            byte rewind = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame + 1)));
            frame = unchecked((ushort)(frame - rewind));
            delay = bus.ReadByte(0x900000 | unchecked((ushort)(delayList + frame)));
        }

        _flareFrames[component] = frame;
        _flareTimers[component] = delay;
    }

    private void DrawFlareComponent(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        int component)
    {
        byte direction = ReadPoseByte(bus, samus.Pose, PoseDirectionOffset);
        if (direction is 0xff or 0x10 || (direction & 0xf0) != 0)
            return;

        int directionOffset = (direction & 0x0f) * 2;
        bool running = samus.ReadMovementType(bus) == 1;
        int xTable = running ? 0x90c1dc : 0x90c1a8;
        int yTable = running ? 0x90c1f0 : 0x90c1c2;
        short xOffset = unchecked((short)ReadWord(bus, xTable + directionOffset));
        short yOffset = unchecked((short)ReadWord(bus, yTable + directionOffset));
        byte poseYOffset = ReadPoseByte(bus, samus.Pose, PoseYOffsetOffset);
        ushort screenX = unchecked((ushort)(samus.XPosition + xOffset - layer1X));
        ushort screenY = unchecked((ushort)(samus.YPosition + yOffset - poseYOffset - layer1Y));

        // `$90:BC98` clips only by the origin's Y high byte. The shared bank-$81 loader
        // deliberately allows individual entries to wrap, matching charge sparks near an
        // edge instead of applying the generic on-screen-origin parking rule.
        if ((screenY & 0xff00) != 0)
            return;

        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        ushort indexOffset = unchecked((ushort)(facingLeft
            ? component switch { 0 => 0, 1 => 0x2a, _ => 0x30 }
            : component switch { 0 => 0, 1 => 0x1e, _ => 0x24 }));
        ushort tableIndex = unchecked((ushort)(indexOffset + _flareFrames[component]));
        oam.AddFlareSpritemap(bus, tableIndex, screenX, screenY);
    }

    private void ClearFlareAnimationState()
    {
        Array.Clear(_flareFrames);
        Array.Clear(_flareTimers);
    }

    private static byte ReadPoseByte(ISnesAddressSpace bus, byte pose, int fieldOffset) =>
        bus.ReadByte(PoseDefinitions + pose * 8 + fieldOffset);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8)));

    private static int AddWithinBank(int address, int byteCount) =>
        (address & 0xff0000) | ((address + byteCount) & 0xffff);
}

/// <summary>One debugger-readable ordinary slot corresponding to a native even byte index.</summary>
public sealed class SamusProjectileSlot
{
    internal SamusProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Host slot 0..4; native byte index is this value times two.</summary>
    public int SlotIndex { get; }

    /// <summary>Native byte index used by the parallel WRAM arrays.</summary>
    public int NativeByteIndex => SlotIndex * 2;

    /// <summary>Damage doubles as the producer's free-slot sentinel.</summary>
    public ushort Damage { get; internal set; }

    /// <summary>Projectile family/beam flags from WRAM <c>$0C18</c>.</summary>
    public ushort Type { get; internal set; }

    /// <summary>Low nibble is direction 0..9; upper bits are lifecycle flags.</summary>
    public ushort Direction { get; internal set; }

    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public short XVelocity { get; internal set; }
    public short YVelocity { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort AnimationFrame { get; internal set; }
    public ushort TrailTimer { get; internal set; }
    public SamusProjectilePreInstruction PreInstruction { get; internal set; }

    /// <summary>Bank-$93 considers a nonzero instruction pointer allocated and drawable.</summary>
    public bool IsActive => InstructionPointer != 0;

    internal void ClearFields()
    {
        Damage = 0;
        Type = 0;
        Direction = 0;
        XPosition = 0;
        YPosition = 0;
        XSubposition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
        AnimationFrame = 0;
        TrailTimer = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }
}

/// <summary>Semantic identities for the bank-$90 function pointers stored per slot.</summary>
public enum SamusProjectilePreInstruction : byte
{
    None,
    NoWaveBeam,
}

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
public readonly record struct SamusProjectileFrameResult(
    int? FiredSlot,
    ushort QueuedSoundEffect,
    bool CollisionStartedExplosion,
    bool ProjectileDeleted);
