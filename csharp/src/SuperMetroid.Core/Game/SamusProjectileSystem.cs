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
public sealed partial class SamusProjectileSystem
{
    /// <summary>Native ordinary-projectile capacity; bombs occupy the other five slots.</summary>
    public const int SlotCount = 5;

    /// <summary>
    /// WRAM allocates byte indices <c>$00-$22</c>, inclusive, for projectile trails.
    /// Because every parallel array is word-indexed, that is eighteen independent slots.
    /// </summary>
    public const int TrailSlotCount = 18;

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
    private const int NormalSuitPalettePointers = 0x91d727;
    private const int BeamChargePalettePointers = 0x91d7d5;
    private const int PseudoScrewPalettePointers = 0x91d7ff;
    private const int HyperBeamShotPalettePointers = 0x91d829;
    private const int SamusPaletteCgramIndex = 192;
    private const int UnchargedBeamDataPointers = 0x9383c1;
    private const int ChargedBeamDataPointers = 0x9383d9;
    // `$93:83FF` is only the pointer-table entry that names the beam-explosion DATA
    // record (`$8679`). KillProjectileInner does not install that address. Its assembly
    // reads the instruction-list pointer stored two bytes into the data record, at
    // `$93:867B`. Treating `$83FF` itself as the list made the interpreter consume data
    // tables as eight-byte animation records: the first bogus "spritemap" was the real
    // list pointer, producing unrelated OBJ fragments, and no delete opcode was reached.
    private const int BeamExplosionInstructionPointerAddress = 0x93867b;
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
    /// WRAM <c>$0B18</c>. Ordinary charged shots use values four through zero for three
    /// white frames and a fourth-call suit restore. Hyper Beam uses `$8014` through `$8000`
    /// for ten ROM palettes held two calls each before the same restore path.
    /// </summary>
    public ushort ChargedShotGlowTimer { get; private set; }

    /// <summary>The most recent `$91:D743` charged-shot palette sub-handler result.</summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; private set; }

    /// <summary>Nested `$91:D83F` result when inactive charging falls through to visor handling.</summary>
    public SamusVisorPaletteStepResult LastVisorPaletteStep { get; private set; }

    /// <summary>
    /// WRAM <c>$0B62</c>, a byte offset into the active six-word charge-palette list.
    /// Native values are exactly 0, 2, 4, 6, 8, and 10; the sixth call wraps to zero.
    /// </summary>
    public ushort SamusChargePaletteIndex { get; private set; }

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
    /// Newly allocated projectile before its first pre-instruction, or null when this frame
    /// did not fire. This deliberately survives same-frame collision/window deletion.
    /// </summary>
    public SamusProjectileSpawnSnapshot? LastFiredProjectileSnapshot { get; private set; }

    /// <summary>
    /// Executes <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D for the equipped beam.
    /// </summary>
    /// <remarks>
    /// The 256 tile bytes are DMAed from bank $9A to VRAM word $6300. Sixteen palette
    /// words are copied from bank $90 into sprite palette six, CGRAM colors $E0-$EF.
    /// Projectile spritemaps already reference those hardware locations; omitting this
    /// room/equipment-time upload produces valid OAM whose pixels are all transparent.
    /// </remarks>
    public static void LoadBeamTilesAndPalette(
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
    public static void QueueBeamTilesAndLoadPalette(
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
    /// Runs <c>HandleBeamChargePalettes</c> at <c>$91:D743-$D7D4</c> against the
    /// modeled Samus CGRAM palette.
    /// </summary>
    /// <remarks>
    /// This belongs to the Samus palette handler, not charge-flare drawing. On hardware the
    /// main thread edits palette-buffer colors 192-207 and a later NMI uploads them. The
    /// software PPU exposes CGRAM directly, so the same words are written at this phase.
    /// The runtime still executes special Speed Booster/Screw/Shinespark/Crystal Flash/X-ray
    /// handlers afterward, preserving `$91:D708-$D721`'s ability to replace this result.
    /// </remarks>
    public SamusBeamChargePaletteStepResult UpdateBeamChargePalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus,
        ushort layerBlendingDefaultConfig = 0x0002)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(samus);
        LastVisorPaletteStep = default;

        ushort timerBefore = ChargedShotGlowTimer;
        if (timerBefore == 0)
        {
            // `$91:D748-$D793` admits the continuously cycling charge palette only while
            // grapple's function pointer is exactly inactive and the flare counter has
            // reached 60. The 65816 BMI is a signed subtraction test, retained literally
            // so Hyper's `$8000` sentinel follows cartridge behavior too.
            bool chargePaletteActive =
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                FlareCounter != 0 &&
                unchecked((short)(FlareCounter - 0x003c)) >= 0;
            if (chargePaletteActive)
            {
                bool pseudoScrew = samus.HorizontalSpeed.ContactDamageIndex == 4;
                int pointerTable = pseudoScrew
                    ? PseudoScrewPalettePointers
                    : BeamChargePalettePointers;
                ushort suitOffset = GetSuitPaletteOffset(samus.EquippedItems);

                // The first lookup selects one of the three suit-specific six-word lists
                // in bank $91. The second produces a bank-$9B, sixteen-color palette.
                // `$0B62` is kept as a byte offset because that is what the native ADC uses.
                ushort listPointer = ReadWord(bus, pointerTable + suitOffset);
                int listEntryAddress = 0x910000 |
                    unchecked((ushort)(listPointer + SamusChargePaletteIndex));
                ushort palettePointer = ReadWord(bus, listEntryAddress);
                cgram.LoadFromBus(
                    bus,
                    0x9b0000 | palettePointer,
                    colorCount: 16,
                    destinationIndex: SamusPaletteCgramIndex);

                int paletteIndex = SamusChargePaletteIndex / 2;
                SamusChargePaletteIndex = SamusChargePaletteIndex >= 10
                    ? (ushort)0
                    : unchecked((ushort)(SamusChargePaletteIndex + 2));
                LastBeamChargePaletteStep = new(
                    pseudoScrew
                        ? SamusBeamChargePaletteAction.PseudoScrewCycle
                        : SamusBeamChargePaletteAction.ChargeCycle,
                    timerBefore,
                    ChargedShotGlowTimer,
                    palettePointer,
                    HyperPaletteIndex: null,
                    ChargePaletteIndex: paletteIndex);
                return LastBeamChargePaletteStep;
            }

            // `$91:D7B0` resets the sequence whenever charge is below threshold or grapple
            // owns its function pointer. This guarantees the next eligible call starts at
            // palette zero rather than resuming a partially completed cycle.
            SamusChargePaletteIndex = 0;
            LastVisorPaletteStep = samus.VisorPalette.Update(
                bus,
                cgram,
                samus.Xray.SpecialPaletteType,
                layerBlendingDefaultConfig);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.Inactive,
                TimerBefore: 0,
                TimerAfter: 0,
                PalettePointer: 0,
                HyperPaletteIndex: null,
                ChargePaletteIndex: null);
            return LastBeamChargePaletteStep;
        }

        if (samus.HyperBeam == 0)
        {
            // `$91:D799` decrements before its BEQ. Starting at four therefore produces
            // white on timers 3, 2, and 1. Timer 1 -> 0 takes carry-set `$D7AE`, causing
            // outer `$D717` to restore the complete suit palette on the fourth call.
            ChargedShotGlowTimer = unchecked((ushort)(ChargedShotGlowTimer - 1));
            if (ChargedShotGlowTimer == 0)
            {
                ushort normalPointer = LoadNormalSuitPalette(bus, cgram, samus.EquippedItems);
                LastBeamChargePaletteStep = new(
                    SamusBeamChargePaletteAction.RestoredNormalSuit,
                    timerBefore,
                    ChargedShotGlowTimer,
                    normalPointer,
                    HyperPaletteIndex: null);
                return LastBeamChargePaletteStep;
            }

            // Native starts at `Palettes_SpriteP4C1 + $1C` and walks backward by words.
            // That is exactly colors 15..1: transparent color zero is intentionally left
            // untouched while every visible Samus color becomes BGR555 `$03FF`.
            for (int color = 1; color < 16; color++)
                cgram.SetColor(SamusPaletteCgramIndex + color, 0x03ff);

            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.OrdinaryWhite,
                timerBefore,
                ChargedShotGlowTimer,
                PalettePointer: 0,
                HyperPaletteIndex: null);
            return LastBeamChargePaletteStep;
        }

        // Hyper's sign bit selects this branch but is not part of the table index. Odd
        // values are pure hold calls; even low-five-bit values 20..2 select palette 0..9.
        // Each selected ROM palette consequently remains visible for exactly two calls.
        if ((ChargedShotGlowTimer & 1) != 0)
        {
            ChargedShotGlowTimer = unchecked((ushort)(ChargedShotGlowTimer - 1));
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.HyperHold,
                timerBefore,
                ChargedShotGlowTimer,
                PalettePointer: 0,
                HyperPaletteIndex: null);
            return LastBeamChargePaletteStep;
        }

        ushort tableOffset = unchecked((ushort)(ChargedShotGlowTimer & 0x001e));
        if (tableOffset == 0)
        {
            // `$91:D7C1` branches before DEC at `$D7CB`. Native explicitly zeroes the
            // already-`$8000` timer and returns carry so `$D717` performs the suit copy.
            ChargedShotGlowTimer = 0;
            ushort normalPointer = LoadNormalSuitPalette(bus, cgram, samus.EquippedItems);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.RestoredNormalSuit,
                timerBefore,
                ChargedShotGlowTimer,
                normalPointer,
                HyperPaletteIndex: null);
            return LastBeamChargePaletteStep;
        }

        ushort hyperPointer = ReadWord(bus, HyperBeamShotPalettePointers + tableOffset);
        cgram.LoadFromBus(
            bus,
            0x9b0000 | hyperPointer,
            colorCount: 16,
            destinationIndex: SamusPaletteCgramIndex);
        int hyperPaletteIndex = (0x14 - tableOffset) / 2;
        ChargedShotGlowTimer = unchecked((ushort)(ChargedShotGlowTimer - 1));
        LastBeamChargePaletteStep = new(
            SamusBeamChargePaletteAction.HyperPalette,
            timerBefore,
            ChargedShotGlowTimer,
            hyperPointer,
            hyperPaletteIndex);
        return LastBeamChargePaletteStep;
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
        bool projectileProducerEnabled = true,
        RoomPlmSystem? roomPlms = null,
        ushort controllerPreviousNewInput = 0)
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
                    level,
                    samus,
                    controllerInput,
                    controllerNewInput,
                    sharedProjectiles,
                    roomPlms);
            }
            else if (samus.SelectedHudItem is 1 or 2)
            {
                (firedSlot, queuedSound) = TryFireMissile(
                    bus,
                    samus,
                    controllerNewInput,
                    controllerPreviousNewInput,
                    sharedProjectiles);
            }
        }

        // FireUnchargedBeam/FireChargedBeam perform one zero-speed block dispatch before
        // installing ordinary movement. A muzzle born against a solid door can therefore
        // already be an explosion before the descending alpha-pass loop begins below.
        bool collisionStartedExplosion = firedSlot is { } initialCollisionSlot &&
            _slots[initialCollisionSlot].PackedType.IsFamily(
                SamusProjectileFamily.BeamExplosion);
        bool projectileDeleted = false;

        LastFiredProjectileSnapshot = firedSlot is { } newSlot
            ? new SamusProjectileSpawnSnapshot(
                newSlot,
                _slots[newSlot].Direction,
                _slots[newSlot].XPosition,
                _slots[newSlot].YPosition,
                _slots[newSlot].XVelocity,
                _slots[newSlot].YVelocity)
            : null;

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
                    layer1Y,
                    roomPlms);
            }
            else if (slot.PreInstruction is
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail or
                SamusProjectilePreInstruction.WaveBeamFourFrameTrail)
            {
                RunWaveBeamPreInstruction(bus, level, slot, layer1X, layer1Y, roomPlms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.HyperBeam)
            {
                RunHyperBeamPreInstruction(bus, level, slot, layer1X, layer1Y, roomPlms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.Missile)
            {
                collisionStartedExplosion |= RunMissilePreInstruction(
                    bus,
                    level,
                    slot,
                    layer1X,
                    layer1Y,
                    sharedProjectiles,
                    roomPlms);
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
                    sharedProjectiles,
                    roomPlms);
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
            queuedSound == 0
                ? null
                : SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, queuedSound),
            queuedSound == 0
                ? (byte)0
                : samus.SelectedHudItem is 0 or 3
                    ? (byte)15 // QueueSfx1_Max15 in the power-beam producer.
                    : (byte)6, // QueueSfx1_Max6 in the missile/super-missile producer.
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
        ushort layer1Y,
        SamusMode7Transform? mode7Transform = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(samus);

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

                DrawFlareComponent(
                    bus, oam, samus, layer1X, layer1Y, component, mode7Transform);
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
            DrawFlareComponent(
                bus, oam, samus, layer1X, layer1Y, component, mode7Transform);
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
            if (!slot.IsActive ||
                slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb)
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
            SamusProjectileFamily family = slot.PackedType.Family;
            if (!slot.IsActive || family is not (
                    SamusProjectileFamily.BeamExplosion or
                    SamusProjectileFamily.MissileExplosion))
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
        LastFiredProjectileSnapshot = null;
        LastBeamChargePaletteStep = default;
        LastVisorPaletteStep = default;
        SamusChargePaletteIndex = 0;
    }

}
