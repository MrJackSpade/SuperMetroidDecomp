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

    /// <summary>
    /// WRAM allocates byte indices <c>$00-$22</c>, inclusive, for projectile trails.
    /// Because every parallel array is word-indexed, that is eighteen independent slots.
    /// </summary>
    public const int TrailSlotCount = 18;

    /// <summary>Live, uncharged beam flag written by <c>Fire_Uncharge_Beam</c>.</summary>
    public const ushort LiveUnchargedBeamFlag = 0x8000;

    /// <summary>Beam-explosion family installed by <c>Kill_Projectile</c>.</summary>
    public const ushort BeamExplosionFamily = 0x0700;

    /// <summary>Missile/super-missile explosion family installed by <c>Kill_Projectile</c>.</summary>
    public const ushort MissileExplosionFamily = 0x0800;

    private const int PoseDefinitions = 0x91b629;
    private const int PoseDirectionOffset = 3;
    private const int PoseYOffsetOffset = 4;
    private const int ProjectileXDefault = 0x90c204;
    private const int ProjectileYDefault = 0x90c218;
    private const int ProjectileXRunning = 0x90c22c;
    private const int ProjectileYRunning = 0x90c240;
    private const int UnchargedCooldowns = 0x90c254;
    private const int BeamAutoFireCooldowns = 0x90c283;
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
    private const int MissileExplosionInstructionPointerAddress = 0x93867f;
    private const int SuperMissileExplosionInstructionPointerAddress = 0x938693;
    private const int NonBeamProjectileDataPointers = 0x9383f1;
    private const int SuperMissileLinkDataPointers = 0x93842b;
    private const int MissileAccelerations = 0x90c303;
    private const int SuperMissileAccelerations = 0x90c32b;
    private const int NonSquareSlopeDefinitions = 0x948b2b;
    private const int SquareSlopeDefinitions = 0x948e54;
    private const int TrailLeftInstructionPointers = 0x90b5bb;
    private const int TrailRightInstructionPointers = 0x90b609;
    private const int UnchargedTrailOffsetFamilies = 0x9ba4b3;
    private const int ChargedTrailOffsetFamilies = 0x9ba4cb;
    private const int SpazerSbaTrailOffsetFamilies = 0x9ba4e3;
    private const ushort MoveLeftTrailDown = 0xb525;
    private const ushort MoveRightTrailDown = 0xb587;
    private const ushort MoveLeftTrailUp = 0xb5b3;
    private const ushort ProjectileInstructionDelete = 0x822f;
    private const ushort ProjectileInstructionGoto = 0x8239;

    private readonly SamusProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount).Select(index => new SamusProjectileSlot(index)).ToArray();
    private readonly SamusProjectileTrailSlot[] _trailSlots =
        Enumerable.Range(0, TrailSlotCount).Select(index => new SamusProjectileTrailSlot(index)).ToArray();

    /// <summary>The slots in native low-to-high byte-index order: $00, $02, ... $08.</summary>
    public IReadOnlyList<SamusProjectileSlot> Slots => _slots;

    /// <summary>
    /// The independent trail pool in native low-to-high byte-index order. Allocation scans
    /// this collection backward, matching <c>$90:B679-$B683</c> rather than using a queue.
    /// </summary>
    public IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => _trailSlots;

    /// <summary>Debugger-friendly count of slots whose left stream still owns the slot.</summary>
    public int ActiveTrailCount => _trailSlots.Count(slot => slot.IsActive);

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

    /// <summary>
    /// WRAM <c>$0BD0</c>; firing a missile writes 20 before the enemy-collision consumer.
    /// That consumer is not translated yet, so the producer-owned value remains inspectable.
    /// </summary>
    public ushort ProjectileInvincibilityTimer { get; private set; }

    /// <summary>Global quake words written by a super-missile impact at `$93:8125-$812E`.</summary>
    public ushort EarthquakeType { get; private set; }
    public ushort EarthquakeTimer { get; private set; }

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

        // Humanoid projectile dispatch is selected by the live HUD item. Indices zero/three
        // share `$90:B80D`; index one reaches `$90:BE62`'s missile producer. Ball poses still
        // dispatch to the companion bomb owner, and the debug grapple route disables this
        // producer explicitly while it owns Shoot.
        if (projectileProducerEnabled && !SamusState.IsStableBallPose(samus.Pose))
        {
            if (samus.SelectedHudItem is 0 or 3)
            {
                (firedSlot, queuedSound) = HandleBeamInput(
                    bus,
                    samus,
                    controllerInput,
                    controllerNewInput,
                    sharedProjectiles);
            }
            else if (samus.SelectedHudItem is 1 or 2)
            {
                (firedSlot, queuedSound) = TryFireMissile(
                    bus,
                    samus,
                    controllerNewInput,
                    sharedProjectiles);
            }
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
            else if (slot.PreInstruction is
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail or
                SamusProjectilePreInstruction.WaveBeamFourFrameTrail)
            {
                RunWaveBeamPreInstruction(bus, slot, layer1X, layer1Y);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.HyperBeam)
            {
                RunHyperBeamPreInstruction(bus, slot, layer1X, layer1Y);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.Missile)
            {
                collisionStartedExplosion |= RunMissilePreInstruction(
                    bus,
                    level,
                    slot,
                    layer1X,
                    layer1Y,
                    sharedProjectiles);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SuperMissile)
            {
                collisionStartedExplosion |= RunSuperMissilePreInstruction(
                    bus,
                    level,
                    samus,
                    slot,
                    layer1X,
                    layer1Y,
                    sharedProjectiles);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SuperMissileLink)
            {
                RunSuperMissileLinkPreInstruction(slot);
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

        if (samus.HyperBeam != 0)
        {
            // Hyper Beam replaces the ordinary count-up flare with `$90:BAFC`'s descending
            // three-component program. Native iteration is fast spark -> slow spark -> main
            // flare (WRAM offsets four, two, zero), so preserve that reverse OAM order.
            for (int component = 2; component >= 0; component--)
            {
                _flareTimers[component] = unchecked((ushort)(_flareTimers[component] - 1));
                if (_flareTimers[component] == 0 || (_flareTimers[component] & 0x8000) != 0)
                {
                    bool finalFrame = _flareFrames[component] == 1;
                    _flareFrames[component] = unchecked((ushort)(_flareFrames[component] - 1));
                    if (finalFrame)
                    {
                        // The fast-spark component at native offset four owns the global
                        // completion signal. Other zero frames merely stop rearming.
                        if (component == 2)
                            FlareCounter = 0;
                    }
                    else
                    {
                        _flareTimers[component] = 3;
                    }
                }

                DrawFlareComponent(bus, oam, samus, layer1X, layer1Y, component);
            }
            return;
        }

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
            // mask `$0F10`. Only an ordinary uncharged beam reaches the flicker branches;
            // a charged beam must remain visible on every NMI parity.
            if ((slot.Type & 0x0f10) == 0)
            {
                // Native X is the even byte index `$00,$02,...,$08`, so native `X & 2`
                // is exactly the parity of this host slot index. Spazer/plasma combinations
                // (low mask `$0C`) use NMI bit one and the opposite phase from power/ice/
                // wave, which use NMI bit zero. Keeping these branches separate is what
                // prevents the wider beam art from disappearing on the wrong video field.
                bool oddNativeSlot = (slotIndex & 1) != 0;
                bool skip = (slot.Type & 0x000c) != 0
                    ? oddNativeSlot != ((nmiFrameCounter & 2) != 0)
                    : oddNativeSlot == ((nmiFrameCounter & 1) != 0);
                if (skip)
                    continue;
            }

            DrawSlot(bus, oam, slot, layer1X, layer1Y, horizontalMargin: 64);
        }
    }

    /// <summary>
    /// Runs and draws <c>$90:B6A9</c>'s eighteen projectile-trail slots after the ordinary
    /// projectile pass. Each side owns its own timer, instruction pointer, position, and OBJ.
    /// </summary>
    /// <param name="timeIsFrozen">
    /// WRAM <c>$0A78</c>. Frozen trails retain their current record and are still drawn;
    /// their timers and embedded position commands do not advance.
    /// </param>
    public void HandleTrailsAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        bool timeIsFrozen)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        // Native Y starts at byte index $22, so the most recently preferred allocation slot
        // is also handled first. Preserve that order because overlapping 8x8 OBJs reveal it.
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileTrailSlot slot = _trailSlots[slotIndex];
            HandleTrailSideAndDraw(bus, oam, slot.Left, layer1X, layer1Y, timeIsFrozen, isLeft: true);
            HandleTrailSideAndDraw(bus, oam, slot.Right, layer1X, layer1Y, timeIsFrozen, isLeft: false);
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
            ushort family = unchecked((ushort)(slot.Type & 0x0f00));
            if (!slot.IsActive || family is not (BeamExplosionFamily or MissileExplosionFamily))
                continue;

            DrawSlot(bus, oam, slot, layer1X, layer1Y, horizontalMargin: 48);
        }
    }

    /// <summary>Clears all five ordinary slots and the separately maintained counter.</summary>
    public void Reset()
    {
        foreach (SamusProjectileSlot slot in _slots)
            slot.ClearFields();
        foreach (SamusProjectileTrailSlot trail in _trailSlots)
            trail.ClearFields();
        ProjectileCounter = 0;
        FlareCounter = 0;
        PreviousBeamChargeCounter = 0;
        ChargedShotGlowTimer = 0;
        ProjectileInvincibilityTimer = 0;
        EarthquakeType = 0;
        EarthquakeTimer = 0;
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

        // `$90:B80D` gives the Hyper flag priority over Charge equipment. It still uses
        // held Shoot rather than a new edge, with its own 21-frame cooldown preventing a
        // desktop-held button from allocating more frequently than the cartridge.
        if (samus.HyperBeam != 0)
            return (controllerInput & shoot) != 0
                ? TryFireHyperBeam(bus, samus, sharedProjectiles)
                : (null, 0);

        // Native `$90:B80D` can also consume charge through the one-frame `$0CFA`
        // "projectile direction changed by pose" bridge. The translated pose pipeline does
        // not publish that overlapped WRAM word yet, so standing/running/air poses with a
        // stable shot direction follow the exact path below; firing during the transitional
        // direction-change frame remains an explicit producer seam rather than a guess.

        // Retail owns twelve low-nibble beam combinations: power through ice+wave+plasma.
        // Spazer and plasma are mutually exclusive in normal inventory state, which is why
        // indices `$C-$F` have no tile, palette, projectile-data, sound, or dispatch entry.
        int beamType = samus.EquippedBeams & 0x000f;
        if ((uint)beamType >= 12)
            return (null, 0);

        bool chargeEquipped = (samus.EquippedBeams & 0x1000) != 0;
        bool held = (controllerInput & shoot) != 0;
        if (!chargeEquipped)
        {
            FlareCounter = 0;
            ClearFlareAnimationState();
            return held
                ? TryFireBeam(bus, samus, controllerNewInput, sharedProjectiles, charged: false)
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
                    return TryFireBeam(
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
        return TryFireBeam(
            bus,
            samus,
            controllerNewInput,
            sharedProjectiles,
            charged: releaseCharged);
    }

    private (int? Slot, ushort Sound) TryFireBeam(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles,
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
        ProjectileInvincibilityTimer = 10;

        // The four low bits are a direct ROM table index, not four independent animation
        // layers composed by the host. In particular, Spazer's familiar three streaks live
        // in one bank-$93 spritemap selected by one data pointer and occupy one projectile
        // slot. This is why no synthetic child projectiles are created here.
        int beamType = slot.Type & 0x000f;

        // `$93:8000` indexes a data-table pointer by beam type, stores damage, chooses the
        // direction-specific list, samples its initial radii, and arms a one-frame timer.
        ushort dataPointer = ReadWord(
            bus,
            (charged ? ChargedBeamDataPointers : UnchargedBeamDataPointers) + beamType * 2);
        slot.Damage = ReadWord(bus, 0x930000 | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            0x930000 | unchecked((ushort)(dataPointer + 2 + direction * 2)));
        slot.XRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 5)));
        slot.InstructionTimer = 1;
        // `$90:B887` gives uncharged power-wave and ice-wave a three-frame trail reload.
        // All other uncharged wave combinations, and every charged wave combination, use
        // the common four-frame wave pre-instruction. Beam words without bit zero retain
        // the ordinary terrain-stopping collision path regardless of ice/spazer/plasma.
        slot.PreInstruction = (beamType & 1) == 0
            ? SamusProjectilePreInstruction.NoWaveBeam
            : !charged && beamType < 4
                ? SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;

        InitializePowerBeamVelocity(bus, slot);

        // A fresh press takes the ordinary table path. Held auto-fire without a new edge
        // uses $19 instead, preserving the native distinction even though both read ROM.
        byte cooldown = charged
            ? bus.ReadByte(UnchargedCooldowns + 0x10 + beamType)
            : (controllerNewInput & shoot) != 0
                ? bus.ReadByte(UnchargedCooldowns + beamType)
                : bus.ReadByte(BeamAutoFireCooldowns + beamType);
        sharedProjectiles.SetSharedCooldown(cooldown);

        ushort sound = ReadWord(
            bus,
            (charged ? ChargedSounds : UnchargedSounds) + beamType * 2);
        if (charged)
            ChargedShotGlowTimer = 4;
        return (slotIndex, sound);
    }

    private (int? Slot, ushort Sound) TryFireHyperBeam(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles)
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
        if ((slot.Direction & 0x00f0) != 0 || (slot.Direction & 0x000f) > 9)
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
        ushort dataPointer = ReadWord(bus, ChargedBeamDataPointers + hyperBeamType * 2);
        slot.Damage = ReadWord(bus, 0x930000 | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            0x930000 | unchecked((ushort)(dataPointer + 2 + (slot.Direction & 0x000f) * 2)));
        slot.XRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 5)));
        slot.InstructionTimer = 1;
        slot.Damage = 1000;
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

        ushort sound = ReadWord(bus, ChargedSounds + hyperBeamType * 2);
        return (slotIndex, sound);
    }

    private (int? Slot, ushort Sound) TryFireMissile(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput,
        SamusBombProjectileSystem sharedProjectiles)
    {
        const ushort shoot = (ushort)SnesButton.X;

        // `$90:BE65-$BE72` accepts either the current new-press word or the previous filtered
        // drawing word. The latter controller-filter seam is not published by this runtime;
        // a real fresh press is therefore the only admitted producer stimulus, and holding X
        // cannot silently become desktop auto-fire.
        if ((controllerNewInput & shoot) == 0)
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
        if ((slot.Direction & 0x00f0) != 0 || (slot.Direction & 0x000f) > 9)
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
            NonBeamProjectileDataPointers + samus.SelectedHudItem * 2);
        slot.Damage = ReadWord(bus, 0x930000 | dataPointer);
        slot.InstructionPointer = ReadWord(
            bus,
            0x930000 | unchecked((ushort)(dataPointer + 2 + (slot.Direction & 0x000f) * 2)));
        slot.XRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 4)));
        slot.YRadius = bus.ReadByte(0x930000 | unchecked((ushort)(slot.InstructionPointer + 5)));
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
            _ => 0,
        };
        slot.YVelocity = direction switch
        {
            0 or 1 or 8 or 9 => unchecked((short)-baseSpeed),
            3 or 4 or 5 or 6 => baseSpeed,
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

        // `$90:AF00` allocates a persistent trail before applying acceleration or movement.
        // Thus the detached 8x8 OBJ records the beam's old world position and visibly falls
        // behind it. Failure to preserve this order makes the trail sit inside the projectile.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }

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

    private void RunWaveBeamPreInstruction(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return;
        }

        // `$90:B0C3` and `$90:B0E4` differ only in the value reloaded after a trail timer
        // expires. The producer always starts at four; an uncharged low-family wave becomes
        // three only after its first emission. Spawning precedes acceleration and movement,
        // so the trail samples the old world position just like the no-wave family.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = slot.PreInstruction ==
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                    ? (ushort)3
                    : (ushort)4;
            SpawnTrail(bus, slot);
        }

        RunWaveBeamShared(bus, slot, layer1X, layer1Y);
    }

    private void RunHyperBeamPreInstruction(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return;
        }

        // `$90:B159` falls directly into the shared Wave movement. Hyper deliberately has
        // no projectile-trail timer or SpawnProjectileTrail call; its long beam spritemap
        // and the separately counting muzzle flare provide the complete native presentation.
        RunWaveBeamShared(bus, slot, layer1X, layer1Y);
    }

    private void RunWaveBeamShared(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        int direction = slot.Direction & 0x000f;
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + direction * 2))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + direction * 2))));

        // `$94:A352/$A3E4` advance the same 16.16 positions and scan every block touched by
        // the projectile radii, but deliberately return carry clear unconditionally. That is
        // the defining Wave Beam behavior: block reactions may run, yet terrain never kills
        // or clips the shot. Shootable-block PLM production is still owned by the incomplete
        // general shot-reaction dispatcher; until that owner is connected, this method keeps
        // the native pass-through motion without fabricating terrain mutations.
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            (slot.XPosition, slot.XSubposition) = AddVelocity(
                slot.XPosition,
                slot.XSubposition,
                slot.XVelocity);
        }
        if (direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            (slot.YPosition, slot.YSubposition) = AddVelocity(
                slot.YPosition,
                slot.YSubposition,
                slot.YVelocity);
        }

        short screenX = unchecked((short)(slot.XPosition - layer1X));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        if (screenX < -64 || screenX >= 320 || screenY < -64 || screenY >= 320)
            ClearProjectile(slot);
    }

    private bool RunMissilePreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        SamusBombProjectileSystem sharedProjectiles)
    {
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            return false;
        }

        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 4;
            SpawnTrail(bus, slot);
        }

        int direction = slot.Direction & 0x000f;

        // Missile pre-instruction `$90:AF8F-$AFA0` first applies the same small directional
        // acceleration as beams. On the ignition frame Missile_Func1 below replaces velocity,
        // so this addition is intentionally overwritten; subsequent frames retain it.
        slot.XVelocity = unchecked((short)(slot.XVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationX + direction * 2))));
        slot.YVelocity = unchecked((short)(slot.YVelocity +
            unchecked((short)ReadWord(bus, ProjectileAccelerationY + direction * 2))));

        if ((slot.Variable & 0xff00) == 0)
        {
            // `$90:C301` is the literal `$0100` ignition increment. Crossing into a nonzero
            // high byte re-runs `$90:B1F3` with that word as base 8.8 speed. A normal missile
            // crosses on its first alpha pass and therefore begins at exactly one pixel/frame.
            slot.Variable = unchecked((ushort)(slot.Variable + 0x0100));
            if ((slot.Variable & 0xff00) != 0)
                InitializeDirectionalVelocity(slot, unchecked((short)slot.Variable));
        }
        else
        {
            int acceleration = MissileAccelerations + direction * 4;
            slot.XVelocity = unchecked((short)(slot.XVelocity +
                unchecked((short)ReadWord(bus, acceleration))));
            slot.YVelocity = unchecked((short)(slot.YVelocity +
                unchecked((short)ReadWord(bus, acceleration + 2))));
        }

        bool collided = direction switch
        {
            0 or 4 or 5 or 9 => MoveMissileVertically(bus, level, slot),
            2 or 7 => MoveMissileHorizontally(bus, level, slot),
            1 or 3 or 6 or 8 =>
                MoveMissileHorizontally(bus, level, slot) ||
                MoveMissileVertically(bus, level, slot),
            _ => false,
        };
        if (collided)
        {
            KillMissile(bus, slot, sharedProjectiles);
            return true;
        }

        short screenX = unchecked((short)(slot.XPosition - layer1X));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        if (screenX < -64 || screenX >= 320 || screenY < -64 || screenY >= 320)
            ClearProjectile(slot);
        return false;
    }

    private bool RunSuperMissilePreInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        SamusBombProjectileSystem sharedProjectiles)
    {
        if ((slot.Direction & 0x00f0) != 0)
        {
            ClearProjectile(slot);
            ClearAllSuperMissileLinks();
            return false;
        }

        // `$90:AFF3` begins with the producer's value four but reloads two after every
        // expiry. Super Missiles therefore emit twice as frequently as ordinary missiles;
        // both families still select the same `$B5A1` art through pointer entries $20/$21.
        slot.TrailTimer = unchecked((ushort)(slot.TrailTimer - 1));
        if (slot.TrailTimer == 0)
        {
            slot.TrailTimer = 2;
            SpawnTrail(bus, slot);
        }

        int direction = slot.Direction & 0x000f;
        if ((slot.Variable & 0xff00) == 0)
        {
            // The shared missile accelerator crosses `$0100` on its first alpha pass. Only
            // the Super family immediately allocates `$90:BF46`'s invisible collision link;
            // the link's native byte index is retained in the variable's low byte.
            slot.Variable = unchecked((ushort)(slot.Variable + 0x0100));
            if ((slot.Variable & 0xff00) != 0)
            {
                InitializeDirectionalVelocity(slot, unchecked((short)slot.Variable));
                SpawnSuperMissileLink(bus, samus, slot);
            }
        }
        else
        {
            int acceleration = SuperMissileAccelerations + direction * 4;
            slot.XVelocity = unchecked((short)(slot.XVelocity +
                unchecked((short)ReadWord(bus, acceleration))));
            slot.YVelocity = unchecked((short)(slot.YVelocity +
                unchecked((short)ReadWord(bus, acceleration + 2))));
        }

        bool collided = false;
        if (direction is 2 or 7 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileHorizontally(bus, level, slot);
            if (collided)
                KillMissile(bus, slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(bus, level, slot, vertical: false, sharedProjectiles);
        }
        if (!collided && direction is 0 or 4 or 5 or 9 or 1 or 3 or 6 or 8)
        {
            collided = MoveMissileVertically(bus, level, slot);
            if (collided)
                KillMissile(bus, slot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(bus, level, slot, vertical: true, sharedProjectiles);
        }

        short screenX = unchecked((short)(slot.XPosition - layer1X));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        if (screenX < -64 || screenX >= 320 || screenY < -64 || screenY >= 320)
        {
            ClearProjectile(slot);
            ClearAllSuperMissileLinks();
        }
        return collided;
    }

    private void SpawnSuperMissileLink(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusProjectileSlot owner)
    {
        SamusProjectileSlot? link = _slots.FirstOrDefault(candidate => candidate.Damage == 0);
        if (link is null)
            return;

        link.ClearFields();
        link.Type = 0x8200;
        link.Direction = owner.Direction;
        link.XPosition = owner.XPosition;
        link.YPosition = owner.YPosition;

        // `$90:BF78` deliberately calls the ordinary muzzle initializer again after copying
        // the owner's coordinates. At ignition both positions are equivalent; retaining the
        // call matters for moving/transition poses whose cartridge origin tables can change.
        InitializePosition(bus, samus, link);
        ushort dataPointer = ReadWord(bus, SuperMissileLinkDataPointers + 4);
        link.Damage = ReadWord(bus, 0x930000 | dataPointer);
        link.InstructionPointer = ReadWord(bus, 0x930000 | unchecked((ushort)(dataPointer + 2)));
        link.InstructionTimer = 1;
        link.PreInstruction = SamusProjectilePreInstruction.SuperMissileLink;

        owner.Variable = unchecked((ushort)((owner.Variable & 0xff00) + link.NativeByteIndex));
        ProjectileCounter = unchecked((ushort)(ProjectileCounter + 1));
    }

    private void RunSuperMissileLinkPreInstruction(SamusProjectileSlot link)
    {
        // `$90:B075` leaves an ordinary link completely stationary. A high direction nibble
        // is a deletion signal and clears every `$x200` family slot, including its owner.
        if ((link.Direction & 0x00f0) == 0)
            return;
        ClearProjectile(link);
        ClearAllSuperMissileLinks();
    }

    private void UpdateSuperMissileLinkAxis(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot owner,
        bool vertical,
        SamusBombProjectileSystem sharedProjectiles)
    {
        if ((owner.Variable & 0xff00) == 0)
            return;

        int linkIndex = (owner.Variable & 0x00ff) >> 1;
        if ((uint)linkIndex >= (uint)_slots.Length)
            return;
        SamusProjectileSlot link = _slots[linkIndex];
        if (!link.IsActive)
            return;

        // A primary collision has already converted the owner to `$0800`. Both slow and fast
        // native branches then clear the invisible link rather than allowing a second quake.
        if ((owner.Type & 0x0f00) == MissileExplosionFamily)
        {
            ClearProjectile(link);
            return;
        }

        short velocity = vertical ? owner.YVelocity : owner.XVelocity;
        int wholeMagnitude = (Math.Abs((int)velocity) & 0xff00) >> 8;
        ushort ownerPosition = vertical ? owner.YPosition : owner.XPosition;
        ushort linkPosition = ownerPosition;
        if (wholeMagnitude >= 11)
        {
            int offset = wholeMagnitude - 10;
            linkPosition = unchecked((ushort)(ownerPosition + (velocity < 0 ? offset : -offset)));
        }

        if (vertical)
            link.YPosition = linkPosition;
        else
            link.XPosition = linkPosition;

        // At <11 px/frame the link only follows the main center. At higher speeds it samples
        // exactly ten pixels beyond the previous position to close the point-collision gap.
        if (wholeMagnitude >= 11 && MissilePointReaction(
                bus,
                level,
                link,
                horizontalMovement: !vertical))
            KillMissile(bus, link, sharedProjectiles);
    }

    private void ClearAllSuperMissileLinks()
    {
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot candidate = _slots[slotIndex];
            if ((candidate.Type & 0x0fff) == 0x0200)
                ClearProjectile(candidate);
        }
    }

    private static bool MoveMissileHorizontally(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot)
    {
        (slot.XPosition, slot.XSubposition) = AddVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity);

        // `$94:A46F` tests the projectile center, not its radius-spanning leading edge. Room
        // width is stored in 256-pixel screens; coordinates in/past the first out-of-room
        // high byte skip reaction and are left for the later 64-pixel viewport deletion.
        int roomWidthInScreens = (level.WidthInBlocks + 15) >> 4;
        if ((slot.XPosition >> 8) >= roomWidthInScreens)
            return false;
        return MissilePointReaction(bus, level, slot, horizontalMovement: true);
    }

    private static bool MoveMissileVertically(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot)
    {
        (slot.YPosition, slot.YSubposition) = AddVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity);

        int roomHeightInScreens = (level.HeightInBlocks + 15) >> 4;
        if ((slot.YPosition >> 8) >= roomHeightInScreens)
            return false;
        return MissilePointReaction(bus, level, slot, horizontalMovement: false);
    }

    private static bool MissilePointReaction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusProjectileSlot slot,
        bool horizontalMovement)
    {
        int blockX = slot.XPosition >> 4;
        int blockY = slot.YPosition >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
        return block.CollisionType switch
        {
            // The point reaction dispatch treats these categories as transparent air.
            0 or 2 or 3 or 6 => false,

            // These categories return carry immediately and therefore kill the missile.
            8 or 9 or 10 or 11 or 14 => true,

            // `$94:A147/$A15E` divide slope BTS values into the five square definitions and
            // the remaining 27 pixel-height definitions. Both paths use the missile center;
            // the direction only changes which half of a square definition is sampled.
            1 => MissileSlopePointReaction(bus, block, slot, horizontalMovement),

            // Block families below have extension walks or bank-$84 PLM side effects.
            // Throwing at the exact contacted block prevents a shootable/bombable tile from
            // being guessed into plain air or plain solid.
            _ => throw new NotSupportedException(
                $"Missile point reaction for collision type ${block.CollisionType:X1}, " +
                $"BTS ${block.Behavior:X2}, block index {block.Index} is not translated."),
        };
    }

    private static bool MissileSlopePointReaction(
        ISnesAddressSpace bus,
        RoomCollisionBlock block,
        SamusProjectileSlot slot,
        bool horizontalMovement)
    {
        int slopeShape = block.Behavior & 0x1f;
        if (slopeShape >= 5)
        {
            // `$94:A58F` mirrors the projectile's within-block coordinate before indexing
            // the cartridge table. BTS bit 6 flips X and bit 7 flips Y. A table height at or
            // above the mirrored Y point (the original uses signed `height - y <= 0`) is
            // solid. Values can reach 20 for overhanging shapes, so do not mask to a nibble.
            int xInBlock = slot.XPosition & 0x000f;
            if ((block.Behavior & 0x40) != 0)
                xInBlock ^= 0x000f;

            int yInBlock = slot.YPosition & 0x000f;
            if ((block.Behavior & 0x80) != 0)
                yInBlock ^= 0x000f;

            int height = bus.ReadByte(
                NonSquareSlopeDefinitions + slopeShape * 16 + xInBlock) & 0x1f;
            return height <= yInBlock;
        }

        // `$94:A66A/$A71A` treat each square slope as four 8x8 quadrants. The top two BTS
        // bits select the base flip, while the projectile half along the movement axis and
        // then the perpendicular axis select the exact quadrant. Each ROM byte is either
        // `$00` (air) or `$80` (solid).
        int quadrant = slopeShape * 4 + (block.Behavior >> 6);
        if (horizontalMovement)
        {
            quadrant ^= (slot.XPosition & 8) >> 3;
            if ((slot.YPosition & 8) != 0)
                quadrant ^= 2;
        }
        else
        {
            quadrant ^= (slot.YPosition & 8) >> 2;
            if ((slot.XPosition & 8) != 0)
                quadrant ^= 1;
        }

        return bus.ReadByte(SquareSlopeDefinitions + quadrant) != 0;
    }

    private void SpawnTrail(ISnesAddressSpace bus, SamusProjectileSlot projectile)
    {
        int pointerIndex;
        if ((projectile.Type & 0x0f00) == 0)
        {
            // Beam indices retain charge/SBA bits in the low six bits. Charged plain power
            // therefore selects entry $10, not ordinary-power entry zero.
            pointerIndex = projectile.Type & 0x003f;
        }
        else
        {
            // Missiles and supers map families $01/$02 to table entries $20/$21. Other
            // projectile families return carry set and do not consume a trail slot.
            int family = (projectile.Type >> 8) & 0x000f;
            if (family >= 3)
                return;
            pointerIndex = family + 0x001f;
        }

        SamusProjectileTrailSlot? trail = null;
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            // The original tests only the left timer. A right stream can still be alive in
            // malformed/debug-edited state and will be overwritten when the left sentinel is
            // clear; retain that asymmetric allocation contract rather than being "safer".
            if (_trailSlots[slotIndex].Left.InstructionTimer == 0)
            {
                trail = _trailSlots[slotIndex];
                break;
            }
        }
        if (trail is null)
            return;

        trail.Left.InstructionTimer = 1;
        trail.Right.InstructionTimer = 1;
        trail.Left.InstructionPointer = ReadWord(bus, TrailLeftInstructionPointers + pointerIndex * 2);
        trail.Right.InstructionPointer = ReadWord(bus, TrailRightInstructionPointers + pointerIndex * 2);

        // `$93:81D1` returns the animation field that is current at this exact pre-instruction
        // instant. When the timer is one and the upcoming word is a normal record, that means
        // the upcoming field; otherwise it means the record eight bytes behind the pointer.
        ushort animationFrame = GetTrailAnimationFrame(bus, projectile);
        int direction = projectile.Direction & 0x000f;
        int familyTable = (projectile.Type & 0x0020) != 0
            ? SpazerSbaTrailOffsetFamilies
            : (projectile.Type & 0x0010) != 0
                ? ChargedTrailOffsetFamilies
                : UnchargedTrailOffsetFamilies;
        ushort directionTable = ReadWord(
            bus,
            familyTable + (projectile.Type & 0x000f) * 2);
        ushort offsetList = ReadWord(
            bus,
            0x9b0000 | unchecked((ushort)(directionTable + direction * 2)));
        int offsets = 0x9b0000 | unchecked((ushort)(offsetList + animationFrame * 4));

        // All four bytes are signed offsets. The final minus four converts a beam-centered
        // point to the upper-left origin of the raw 8x8 trail OBJ, exactly as `$9B:A3CC`.
        trail.Left.XPosition = AddSignedOffset(projectile.XPosition, bus.ReadByte(offsets), -4);
        trail.Left.YPosition = AddSignedOffset(projectile.YPosition, bus.ReadByte(offsets + 1), -4);
        trail.Right.XPosition = AddSignedOffset(projectile.XPosition, bus.ReadByte(offsets + 2), -4);
        trail.Right.YPosition = AddSignedOffset(projectile.YPosition, bus.ReadByte(offsets + 3), -4);
    }

    private static ushort GetTrailAnimationFrame(
        ISnesAddressSpace bus,
        SamusProjectileSlot projectile)
    {
        ushort pointer = projectile.InstructionPointer;
        ushort upcomingWord = ReadWord(bus, 0x930000 | pointer);
        int recordDelta = projectile.InstructionTimer == 1 && (upcomingWord & 0x8000) == 0
            ? 0
            : -8;
        ushort frameAddress = unchecked((ushort)(pointer + recordDelta + 6));
        return ReadWord(bus, 0x930000 | frameAddress);
    }

    private static ushort AddSignedOffset(ushort origin, byte encodedOffset, int constant) =>
        unchecked((ushort)(origin + unchecked((sbyte)encodedOffset) + constant));

    private static void HandleTrailSideAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileTrailSide side,
        ushort layer1X,
        ushort layer1Y,
        bool timeIsFrozen,
        bool isLeft)
    {
        if (side.InstructionTimer == 0)
            return;

        if (!timeIsFrozen)
        {
            side.InstructionTimer = unchecked((ushort)(side.InstructionTimer - 1));
            if (side.InstructionTimer == 0)
            {
                ushort pointer = side.InstructionPointer;
                while (true)
                {
                    ushort instructionOrTimer = ReadWord(bus, 0x900000 | pointer);
                    if ((instructionOrTimer & 0x8000) == 0)
                    {
                        side.InstructionTimer = instructionOrTimer;
                        if (instructionOrTimer == 0)
                            return;

                        side.TileNumberAttributes = ReadWord(
                            bus,
                            0x900000 | unchecked((ushort)(pointer + 2)));
                        side.InstructionPointer = unchecked((ushort)(pointer + 4));
                        break;
                    }

                    // Bank $90 stores executable instruction addresses inline. Each handler
                    // returns to the parser with X already advanced past its one-word opcode.
                    pointer = unchecked((ushort)(pointer + 2));
                    switch (instructionOrTimer)
                    {
                        case MoveLeftTrailDown when isLeft:
                        case MoveRightTrailDown when !isLeft:
                            side.YPosition = unchecked((ushort)(side.YPosition + 1));
                            break;
                        case MoveLeftTrailUp when isLeft:
                            side.YPosition = unchecked((ushort)(side.YPosition - 1));
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unsupported {(isLeft ? "left" : "right")} projectile-trail " +
                                $"instruction ${instructionOrTimer:X4} at $90:{unchecked((ushort)(pointer - 2)):X4}.");
                    }
                }
            }
        }

        // `$90:B6F4/$B703` require both complete 16-bit camera-relative coordinates to have
        // a zero high byte. Unlike the generic spritemap path, negative or 256+ coordinates
        // are skipped rather than wrapped or parked.
        ushort screenX = unchecked((ushort)(side.XPosition - layer1X));
        ushort screenY = unchecked((ushort)(side.YPosition - layer1Y));
        if ((screenX & 0xff00) != 0 || (screenY & 0xff00) != 0)
            return;

        oam.AddProjectileTrailSprite(
            unchecked((byte)screenX),
            unchecked((byte)screenY),
            side.TileNumberAttributes);
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

    private void KillMissile(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        // The shared `$90:AE3A` leading-edge correction runs for beams and missiles alike.
        // Missiles use point collision while travelling, but their explosion is deliberately
        // anchored one current animation radius farther in the fired direction.
        byte direction = unchecked((byte)(slot.Direction & 0x0f));
        if (direction is 1 or 2 or 3)
            slot.XPosition = unchecked((ushort)(slot.XPosition + slot.XRadius));
        else if (direction is 6 or 7 or 8)
            slot.XPosition = unchecked((ushort)(slot.XPosition - slot.XRadius));

        if (direction is 0 or 1 or 8 or 9)
            slot.YPosition = unchecked((ushort)(slot.YPosition - slot.YRadius));
        else if (direction is 3 or 4 or 5 or 6)
            slot.YPosition = unchecked((ushort)(slot.YPosition + slot.YRadius));

        // `$93:80CF` queues library-two sound seven, converts non-beams to family `$0800`,
        // selects `$86:7F`, and leaves the slot counted until its delete opcode. Sound-library
        // two has no public frame-result channel yet; every stateful effect is retained here.
        bool wasSuperMissile = (slot.Type & 0x0200) != 0;
        slot.Type = unchecked((ushort)((slot.Type & 0xf0ff) | MissileExplosionFamily));
        slot.InstructionPointer = ReadWord(
            bus,
            wasSuperMissile
                ? SuperMissileExplosionInstructionPointerAddress
                : MissileExplosionInstructionPointerAddress);
        slot.InstructionTimer = 1;
        slot.Damage = 8;
        slot.PreInstruction = SamusProjectilePreInstruction.None;

        if (wasSuperMissile)
        {
            // `$93:8125-$812E` is presentation state, but it is authored by the projectile
            // impact itself: quake type $14 for thirty frames. The screen-offset consumer is
            // still separate, so expose the exact words for debugger watches and integration.
            EarthquakeType = 20;
            EarthquakeTimer = 30;
        }

        // Only cooldowns 21+ are shortened to 20. A normal missile begins at ten, so ordinary
        // wall impact does not extend or replace its remaining fire delay.
        if (sharedProjectiles.CooldownTimer >= 21)
            sharedProjectiles.SetSharedCooldown(20);
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
    /// <summary>WRAM <c>$0C7C</c>; missile ignition/acceleration state in the high byte.</summary>
    public ushort Variable { get; internal set; }
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
        Variable = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }
}

/// <summary>Semantic identities for the bank-$90 function pointers stored per slot.</summary>
public enum SamusProjectilePreInstruction : byte
{
    None,
    NoWaveBeam,
    WaveBeamThreeFrameTrail,
    WaveBeamFourFrameTrail,
    HyperBeam,
    Missile,
    SuperMissile,
    SuperMissileLink,
}

/// <summary>
/// One of the eighteen independent projectile-trail allocations. The left timer is the
/// native free-slot sentinel even though both sides otherwise animate independently.
/// </summary>
public sealed class SamusProjectileTrailSlot
{
    internal SamusProjectileTrailSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    public int SlotIndex { get; }
    public int NativeByteIndex => SlotIndex * 2;
    public SamusProjectileTrailSide Left { get; }
    public SamusProjectileTrailSide Right { get; }
    public bool IsActive => Left.InstructionTimer != 0;

    internal void ClearFields()
    {
        Left.ClearFields();
        Right.ClearFields();
    }
}

/// <summary>One side of a two-stream bank-$90 projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort TileNumberAttributes { get; internal set; }

    internal void ClearFields()
    {
        XPosition = 0;
        YPosition = 0;
        InstructionTimer = 0;
        InstructionPointer = 0;
        TileNumberAttributes = 0;
    }
}

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
public readonly record struct SamusProjectileFrameResult(
    int? FiredSlot,
    ushort QueuedSoundEffect,
    bool CollisionStartedExplosion,
    bool ProjectileDeleted);
