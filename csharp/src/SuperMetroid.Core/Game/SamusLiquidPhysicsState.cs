using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The room-FX words and derived liquid state consulted by Samus's bank-$90/$91 physics.
/// </summary>
/// <remarks>
/// Super Metroid does not have one universal "underwater" flag. Different native routines
/// compare either Samus's top, bottom, or bottom-minus-one boundary against the live FX
/// surface, and <c>$0AD2</c> remembers the medium selected by the animation/room-load pass.
/// Keeping the literal source words here lets each translated caller reproduce its own
/// comparison instead of sharing a convenient but incorrect host-side boolean.
/// </remarks>
public sealed class SamusLiquidPhysicsState
{
    private readonly List<SamusSoundRequest> _soundRequests = [];
    private readonly Bank80SystemState _standaloneRandom = new();

    /// <summary>No liquid physics are active at the sampled Samus boundary.</summary>
    public const ushort Air = 0;

    /// <summary>Water physics, the native value stored at WRAM <c>$0AD2</c>.</summary>
    public const ushort Water = 1;

    /// <summary>Lava/acid physics, the native value stored at WRAM <c>$0AD2</c>.</summary>
    public const ushort LavaAcid = 2;

    /// <summary>
    /// Exclusive FX dispatcher identity stored at WRAM <c>$196E</c>. Values two and four
    /// are lava/acid, six is water, and zero is the no-FX handler.
    /// </summary>
    public RoomFxType FxType { get; set; }

    /// <summary>
    /// General FX surface Y at WRAM <c>$195E</c>. A negative 16-bit value tells movement
    /// routines to consult <see cref="LavaAcidYPosition"/> instead of the water surface.
    /// </summary>
    public ushort FxYPosition { get; set; } = ushort.MaxValue;

    /// <summary>Lava/acid surface Y at WRAM <c>$1962</c>; negative means absent.</summary>
    public ushort LavaAcidYPosition { get; set; } = ushort.MaxValue;

    /// <summary>
    /// Liquid-options word at WRAM <c>$197E</c>. Bit two disables water interaction even
    /// when the geometric surface comparison says Samus is below it.
    /// </summary>
    public ushort LiquidOptions { get; set; }

    /// <summary>
    /// Remembered native medium at WRAM <c>$0AD2</c>. This is intentionally stateful:
    /// Space Jump reads it, and the animation handlers use changes to detect entry/exit.
    /// </summary>
    public ushort LiquidPhysicsType { get; private set; }

    /// <summary>Typed view of the remembered native medium.</summary>
    public SamusLiquidMedium LiquidMedium => (SamusLiquidMedium)LiquidPhysicsType;

    /// <summary>The four native water/lava/footstep particle slots and their OAM renderer.</summary>
    public SamusAtmosphericEffectsState AtmosphericEffects { get; } = new();

    /// <summary>
    /// Sound-library requests emitted by the current Samus animation call. A host audio
    /// backend may consume these without moving sound policy out of the translated routines.
    /// </summary>
    public IReadOnlyList<SamusSoundRequest> SoundRequests => _soundRequests;

    /// <summary>
    /// Starts one native Samus-handler sound-publication window. Runtime calls this before
    /// movement because landing audio is emitted by collision at <c>$91:F046</c>, before
    /// <c>AnimateSamus</c>. Standalone animation calls retain their convenient default of
    /// beginning a window themselves.
    /// </summary>
    public void BeginFrameSoundRequests() => _soundRequests.Clear();

    /// <summary>
    /// Publishes a sound selected by a movement routine into the same per-frame Samus queue
    /// used by liquid entry, footsteps, and landing impact.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general audio backend. Bank-$90 movement owns a handful
    /// of exact queue calls—most notably underwater Space Jump's library-one sound $2F—and
    /// the runtime already begins one shared publication window before movement.
    /// </remarks>
    public void QueueMovementSound(SoundEffectId soundEffect, byte maximumQueued) =>
        QueueSound(soundEffect, maximumQueued);

    /// <summary>WRAM <c>$0A4E</c>, the fractional half of pending periodic damage.</summary>
    public ushort PeriodicSubDamage { get; private set; }

    /// <summary>WRAM <c>$0A50</c>, the whole-energy half of pending periodic damage.</summary>
    public ushort PeriodicDamage { get; private set; }

    /// <summary>Logical room pair consumed by area/room-specific footstep graphics.</summary>
    public RoomIdentity RoomIdentity { get; set; }

    /// <summary>Validated retail-area projection used by area-only consumers.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;

    /// <summary>Per-area room-index projection used by native room tables.</summary>
    public byte RoomIndex => RoomIdentity.RoomIndex;

    /// <summary>Nonzero cinematic-function state suppresses ordinary footstep audio.</summary>
    public bool CinematicFunctionActive { get; set; }

    /// <summary>Nonzero boss ID suppresses ordinary footstep audio.</summary>
    public ushort BossId { get; set; }

    /// <summary>Configures the exact room-FX words for an ordinary water surface.</summary>
    public void ConfigureWater(ushort surfaceY, ushort liquidOptions = 0)
    {
        FxType = RoomFxType.Water;
        FxYPosition = surfaceY;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = liquidOptions;
    }

    /// <summary>Configures the exact room-FX words for a lava (type two) or acid (type four) surface.</summary>
    public void ConfigureLavaAcid(ushort surfaceY, bool acid = false)
    {
        FxType = acid ? RoomFxType.Acid : RoomFxType.Lava;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = surfaceY;
        LiquidOptions = 0;
    }

    /// <summary>Restores the no-FX sentinel state used by dry rooms.</summary>
    public void Clear()
    {
        FxType = RoomFxType.None;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = 0;
        LiquidPhysicsType = Air;
        PeriodicSubDamage = 0;
        PeriodicDamage = 0;
        _soundRequests.Clear();
        AtmosphericEffects.Clear();
    }

    /// <summary>
    /// Reproduces <c>SetLiquidPhysicsType</c> at <c>$90:8E0F</c>. Unlike movement-table
    /// selection, this room-load helper dispatches on FX type and does not exempt Gravity
    /// Suit; animation later suppresses the suit's delay while retaining the medium word.
    /// </summary>
    public void InitializeRememberedMedium(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ushort bottom = samus.Kinematics.BottomBoundary;
        LiquidPhysicsType = FxType switch
        {
            RoomFxType.Lava or RoomFxType.Acid
                when IsBelowSurface(LavaAcidYPosition, bottom) => LavaAcid,
            RoomFxType.Water when WaterAffectsBoundary(bottom) => Water,
            _ => Air,
        };
    }

    /// <summary>
    /// Selects air/water/lava physics for routines that test Samus's bottom boundary and
    /// bypass all liquid behavior when Gravity Suit bit <c>$0020</c> is equipped.
    /// </summary>
    public ushort DetermineMovementMedium(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return Air;
        return DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary);
    }

    /// <summary>
    /// True when the spin routine's top-boundary test at <c>$90:A449-$A469</c> finds Samus
    /// fully under water/lava. The caller performs the Gravity-Suit palette-bit exemption.
    /// </summary>
    public bool IsTopBoundarySubmerged(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.TopBoundary) != Air;
    }

    /// <summary>
    /// True when Samus's full bottom collision boundary is below an active water or
    /// lava/acid surface, without applying the Gravity Suit exemption.
    /// </summary>
    /// <remarks>
    /// This is the exact environmental question asked by the Samus palette handler at
    /// <c>$91:D9BA-$D9D8</c>. It deliberately differs from
    /// <see cref="DetermineMovementMedium"/>: the native palette routine tests the Gravity
    /// Suit bit first, then performs the raw bottom-boundary liquid test. Keeping those two
    /// decisions separate lets the caller reproduce both the submerged Power/Varia early
    /// return and Gravity Suit's unconditional palette-animation bypass. Water option bit
    /// two still disables water, while a negative general-FX surface falls through to the
    /// independent lava/acid surface exactly as the cartridge does.
    /// </remarks>
    public bool IsBottomBoundarySubmerged(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) != Air;
    }

    /// <summary>
    /// Reproduces the liquid-flag update at <c>$9B:C4BE-$C4EA</c>, which runs after the
    /// current grapple function and therefore affects the following grapple frame.
    /// </summary>
    /// <remarks>
    /// This test is intentionally not the ordinary movement-medium selector. Native grapple
    /// checks the suit-palette Gravity bit, requires a nonzero FX type, and compares only the
    /// general FX Y word against Samus's bottom. It does not consult liquid-options bit two
    /// and never falls back to the lava/acid Y word. Those quirks are retained rather than
    /// replacing the cartridge's single flag with a friendlier host notion of “submerged.”
    /// </remarks>
    public bool DetermineGrappleSubmersion(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        return !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            FxType != RoomFxType.None &&
            IsBelowSurface(FxYPosition, samus.Kinematics.BottomBoundary);
    }

    /// <summary>
    /// Returns the extra animation delay used by <c>$91:FB08</c> while installing a changed
    /// pose. That routine samples <c>Y + radius - 1</c>, not the normal bottom boundary.
    /// </summary>
    public ushort DeterminePoseChangeAnimationBuffer(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return samus.XSpeedDivisor;

        ushort bottomMinusOne = unchecked((ushort)(
            samus.Kinematics.YPosition + samus.Kinematics.YRadius - 1));
        return DetermineRawMediumAtBoundary(bottomMinusOne) switch
        {
            Water => 3,
            LavaAcid => 2,
            _ => samus.XSpeedDivisor,
        };
    }

    /// <summary>
    /// Runs <c>Samus_Animate</c>'s complete bank-$90 FX dispatch before the generic animation
    /// timer: liquid delay, medium changes, four particle producers, sound requests, and
    /// lava/acid fixed-point damage accumulation.
    /// </summary>
    public void PrepareAnimationFrame(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort nmiFrameCounter = 0,
        Bank80SystemState? system = null,
        bool beginSoundRequestFrame = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Direct verifier/debugger calls begin a publication window here. The full runtime
        // begins it before movement and passes false, preserving collision sounds queued by
        // `$91:F046` before this bank-$90 animation phase. Visual slots and damage words are
        // persistent WRAM and therefore are never cleared merely because a frame began.
        if (beginSoundRequestFrame)
            BeginFrameSoundRequests();

        SamusMovementType movementType = samus.ReadMovementType(bus);
        TrySpawnRunningFootsteps(bus, samus, movementType);

        ushort bottom = samus.Kinematics.BottomBoundary;
        ushort top = samus.Kinematics.TopBoundary;
        RoomFxType fxKind = FxType;
        bool gravitySuit = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);

        if (fxKind == RoomFxType.Water && WaterAffectsBoundary(bottom))
        {
            // `$90:80B8` publishes delay three before testing the remembered medium. A
            // transition into water queues sound $0D and creates either a diving splash or
            // two grounded splashes; every submerged call then gets the bubble opportunity.
            bool enteredWater = LiquidMedium != SamusLiquidMedium.Water;
            LiquidPhysicsType = Water;
            samus.AnimationFrameBuffer = 3;
            if (enteredWater)
            {
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0d), maximumQueued: 6);
                SpawnWaterSplash(bus, samus, movementType, bottom);
            }

            TrySpawnAirBubbles(
                samus,
                top,
                nmiFrameCounter,
                system ?? _standaloneRandom);

            // Standing forward `$00`, backward `$9B`, and every Gravity-Suit pose clear the
            // buffer in Spawn_AirBubbles after all particle/audio side effects have run.
            if (samus.Pose is SamusPoseIds.ForwardFacingPowerSuitPose or
                SamusPoseIds.ForwardFacingSuitedPose || gravitySuit)
                samus.AnimationFrameBuffer = 0;
            return;
        }

        if (fxKind is RoomFxType.Lava or RoomFxType.Acid &&
            IsBelowSurface(LavaAcidYPosition, bottom))
        {
            if (fxKind == RoomFxType.Lava && samus.HorizontalSpeed.SpeedBoostCounter != 0)
            {
                // Lava alone executes `$90:81C9-$81D5` before the Gravity-Suit branch.
                // Cancel_SpeedBoosting owns the momentum/counter/palette/echo transition;
                // the two extra-run words are then cleared explicitly by the FX handler.
                // Acid deliberately skips all three writes.
                samus.HorizontalSpeed.CancelRunningMomentum(samus.ReadPoseXDirection(bus));
                samus.HorizontalSpeed.ExtraRunSpeed = 0;
                samus.HorizontalSpeed.ExtraRunSubspeed = 0;
            }

            if (fxKind == RoomFxType.Lava && gravitySuit)
            {
                // Lava's Gravity-Suit branch returns before damage and before surface spray.
                // Acid intentionally has no equivalent early exit and still hurts at quarter
                // strength when HandlePeriodicDamage runs later in the same Samus handler.
                LiquidPhysicsType = LavaAcid;
                samus.AnimationFrameBuffer = 0;
                return;
            }

            AccumulateLiquidDamage(
                bus,
                fxKind == RoomFxType.Lava
                    ? SamusMovementRomData.Environment.LavaSubdamagePerFrame
                    : SamusMovementRomData.Environment.AcidSubdamagePerFrame,
                fxKind == RoomFxType.Lava
                    ? SamusMovementRomData.Environment.LavaDamagePerFrame
                    : SamusMovementRomData.Environment.AcidDamagePerFrame);
            if ((nmiFrameCounter & 7) == 0 && samus.Health >= 0x0047)
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x2d), maximumQueued: 3);

            // Both lava and acid now share `$90:824C`'s delay-two submerged path.
            LiquidPhysicsType = LavaAcid;
            samus.AnimationFrameBuffer = 2;
            TrySpawnLavaSurfaceSpray(samus, top, nmiFrameCounter);
            if (samus.Pose is SamusPoseIds.ForwardFacingPowerSuitPose or
                SamusPoseIds.ForwardFacingSuitedPose || gravitySuit)
                samus.AnimationFrameBuffer = 0;
            return;
        }

        // `$90:8078` publishes the X-speed divisor and clears the remembered medium. Only
        // water (bit zero set) owns an exit splash; lava/acid simply clears `$0AD2`.
        samus.AnimationFrameBuffer = samus.XSpeedDivisor;
        bool exitedWater = (LiquidPhysicsType & 1) != 0;
        LiquidPhysicsType = Air;
        if (exitedWater)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0e), maximumQueued: 6);
            if (!gravitySuit && movementType is
                SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x30), maximumQueued: 6);
            SpawnWaterSplash(bus, samus, movementType, bottom);
            TrySpawnAirBubbles(
                samus,
                top,
                nmiFrameCounter,
                system ?? _standaloneRandom);
        }
    }

    /// <summary>
    /// Ports <c>HandleLandingSoundEffectsAndGraphics</c> and all area handlers at
    /// <c>$91:F046-$F1D2</c>. Call this after downward collision has placed Samus against the
    /// surface but before animation and pose-change cleanup erase the impact velocity/radius.
    /// </summary>
    public void HandleLandingSoundEffectsAndGraphics(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusMovementType previousMovementType,
        byte previousPose,
        ushort impactYSpeed,
        ushort impactYSubspeed)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // `$91:F046-$F074` ends the spin-loop sound separately from the impact. The test is
        // on PREVIOUS movement type and pose because landing selection has not run yet.
        // Cinematics suppress both the ordinary-spin `$32` and Screw Attack `$34` requests.
        if (!CinematicFunctionActive && previousMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, previousPose is SamusPoseIds.ScrewAttackRightPose or
                    SamusPoseIds.ScrewAttackLeftPose ? (byte)0x34 : (byte)0x32), maximumQueued: 6);
        }

        // A truly stationary grounding probe returns before impact audio AND graphics. Any
        // nonzero fractional descent is a soft landing; whole speed five or greater is hard.
        // Native Y speed is an unsigned magnitude, so CMP #$0005/BPL is equivalent here.
        if (impactYSpeed == 0 && impactYSubspeed == 0)
            return;

        if (!CinematicFunctionActive)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, impactYSpeed >= 5 ? (byte)0x04 : (byte)0x05), maximumQueued: 6);
        }

        HandleLandingGraphics(bus, samus);
    }

    private void HandleLandingGraphics(ISnesAddressSpace bus, SamusState samus)
    {
        // `$91:F0AA` is an eight-entry area jump table. Invalid area bytes would execute
        // unrelated bank-$91 data on hardware; fail loudly instead of manufacturing output.
        switch (AreaIndex)
        {
            case AreaId.Crateria:
                HandleCrateriaLandingGraphics(bus, samus);
                return;

            case AreaId.Brinstar:
                // Retail code's apparent missing RTS is real: room eight branches directly
                // to dust, while every other Brinstar room falls through Tourian's room set.
                if (RoomIdentity == RoomIdentities.BrinstarDirectLandingDust ||
                    RoomIdentities.UsesTourianStyleLandingDust(RoomIdentity))
                    SpawnLandingPairUnlessSubmerged(samus, type: 6);
                else
                    DeleteLandingPair();
                return;

            case AreaId.Norfair:
            case AreaId.WreckedShip:
                SpawnLandingPairUnlessSubmerged(samus, type: 6);
                return;

            case AreaId.Maridia:
                SpawnLandingPairUnlessSubmerged(samus, type: 1);
                return;

            case AreaId.Tourian:
                if (RoomIdentities.UsesTourianStyleLandingDust(RoomIdentity))
                    SpawnLandingPairUnlessSubmerged(samus, type: 6);
                else
                    DeleteLandingPair();
                return;

            case AreaId.Ceres:
                DeleteLandingPair();
                return;

            default:
                throw new InvalidDataException($"Landing graphics area {(byte)AreaIndex} is outside the seven retail areas.");
        }
    }

    private void HandleCrateriaLandingGraphics(ISnesAddressSpace bus, SamusState samus)
    {
        // Crateria's cinematic path deletes both landing-owned slots before consulting room
        // data. This is distinct from the sound suppression already applied by the caller.
        if (CinematicFunctionActive)
        {
            DeleteLandingPair();
            return;
        }

        // Room `$1C` (space-pirate shaft) bypasses the 16-byte classification table and
        // always chooses the dry-dust handler, still subject to live liquid submersion.
        if (RoomIdentity == RoomIdentities.CrateriaSpacePirateShaft)
        {
            SpawnLandingPairUnlessSubmerged(samus, type: 6);
            return;
        }

        if (RoomIndex >= 0x10)
        {
            DeleteLandingPair();
            return;
        }

        // Read the literal inline flags at `$91:F0F3`, rather than maintaining a second C#
        // room list. BIT priority is 1 (Landing Site), 2 (Wrecked Ship entrance), then 4
        // (wet-footstep rooms), even if a modified/private image combines those bits.
        byte roomFlags = bus.ReadByte(
            SamusMovementRomData.Environment.RoomAtmosphericEffectFlags + RoomIndex);
        if ((roomFlags & 1) != 0)
        {
            // Landing Site creates splashes only for FX type `$000A`; its normal scrolling-
            // sky type deletes the pair. This exact comparison is not a generic water test.
            if (FxType == RoomFxType.Rain)
                SpawnLandingPairUnlessSubmerged(samus, type: 1);
            else
                DeleteLandingPair();
            return;
        }

        if ((roomFlags & 2) != 0)
        {
            // Above Y=$03B0 the entrance is dry and deletes; at/below it, use wet splashes.
            if (samus.YPosition >= 0x03b0)
                SpawnLandingPairUnlessSubmerged(samus, type: 1);
            else
                DeleteLandingPair();
            return;
        }

        if ((roomFlags & 4) != 0)
        {
            SpawnLandingPairUnlessSubmerged(samus, type: 1);
            return;
        }

        DeleteLandingPair();
    }

    private void SpawnLandingPairUnlessSubmerged(SamusState samus, byte type)
    {
        ushort bottom = samus.Kinematics.BottomBoundary;

        // Both `$91:F116` and `$91:F166` suppress their particles when Samus's current
        // bottom is genuinely below active water or lava/acid. `DetermineRawMedium...`
        // preserves the signed surface sentinels and water option-bit-two exemption used by
        // those routines. Importantly, suppression RETURNS and leaves old slot data intact.
        if (DetermineRawMediumAtBoundary(bottom) != Air)
            return;

        ushort rightOffset = type == 1 ? (ushort)4 : (ushort)8;
        ushort leftSeparation = type == 1 ? (ushort)7 : (ushort)16;
        ushort firstX = unchecked((ushort)(samus.XPosition + rightOffset));
        ushort secondX = unchecked((ushort)(firstX - leftSeparation));
        ushort y = type == 1 ? unchecked((ushort)(bottom - 4)) : bottom;

        // Landing owns WRAM slots two and three (byte offsets +4/+6), not the running-foot
        // slots zero/one. Both begin immediately on frame zero with timer three.
        AtmosphericEffects.SetSlot(2, type, 0, 3, firstX, y);
        AtmosphericEffects.SetSlot(3, type, 0, 3, secondX, y);
    }

    private void DeleteLandingPair()
    {
        // Native STZ writes only the packed frame/type words; timers and coordinates survive.
        AtmosphericEffects.ClearFrameAndType(2);
        AtmosphericEffects.ClearFrameAndType(3);
    }

    /// <summary>
    /// Ports <c>HandlePeriodicDamageToSamus</c> at <c>$90:E9CE</c>. The producer above adds
    /// raw fixed-point damage; this consumer applies Varia's half or Gravity's quarter,
    /// subtracts with the original borrow chain, clamps fatal underflow, and clears the pair.
    /// </summary>
    public void ApplyPeriodicDamage(SamusState samus, bool timeIsFrozen)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (timeIsFrozen)
        {
            PeriodicSubDamage = 0;
            PeriodicDamage = 0;
            return;
        }

        ushort appliedSubDamage = PeriodicSubDamage;
        ushort appliedDamage = PeriodicDamage;
        int divisorShift = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? 1
                : 0;
        if (divisorShift != 0)
        {
            // Native reads the overlapping word `$0A4D`: fractional high byte followed by
            // whole-damage low byte. Shifting that 8.8 quantity and splitting it back is not
            // equivalent to independently shifting both 16-bit words when a carry crosses.
            ushort overlappingDamage = unchecked((ushort)(
                (PeriodicSubDamage >> 8) | ((PeriodicDamage & 0x00ff) << 8)));
            overlappingDamage = unchecked((ushort)(overlappingDamage >> divisorShift));
            appliedSubDamage = unchecked((ushort)((overlappingDamage & 0x00ff) << 8));
            appliedDamage = unchecked((ushort)(overlappingDamage >> 8));
        }

        // `$90:EA11` deliberately jumps to the native crash handler if the signed whole
        // word is negative. Reachable environmental rates never do this; throwing retains
        // the corruption/overflow guard instead of converting it into plausible damage.
        if (unchecked((short)appliedDamage) < 0)
            throw new InvalidDataException("Periodic whole damage overflowed into the signed-negative range.");

        int subunitResult = samus.SubunitHealth - appliedSubDamage;
        samus.SubunitHealth = unchecked((ushort)subunitResult);
        int healthResult = samus.Health - appliedDamage - (subunitResult < 0 ? 1 : 0);
        ushort wrappedHealth = unchecked((ushort)healthResult);
        if (unchecked((short)wrappedHealth) < 0)
        {
            samus.SubunitHealth = 0;
            samus.Health = 0;
        }
        else
        {
            samus.Health = wrappedHealth;
        }

        PeriodicSubDamage = 0;
        PeriodicDamage = 0;
    }

    /// <summary>
    /// Adds one native 16.16 periodic-damage contribution to WRAM `$0A4E.$0A50`.
    /// </summary>
    /// <remarks>
    /// Liquid damage reads its operands from ROM, while block and enemy reactions supply
    /// literal values. They all use the same 65816 <c>CLC/ADC/STA; LDA/ADC/STA</c> carry
    /// chain, so keeping that operation here prevents each producer from quietly dropping
    /// a fractional overflow into the whole-energy word.
    /// </remarks>
    public void AccumulatePeriodicDamage(ushort subDamage, ushort wholeDamage)
    {
        uint fractional = (uint)PeriodicSubDamage + subDamage;
        PeriodicSubDamage = unchecked((ushort)fractional);
        PeriodicDamage = unchecked((ushort)(
            PeriodicDamage + wholeDamage + (fractional >> 16)));
    }

    private void SpawnWaterSplash(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusMovementType movementType,
        ushort bottom)
    {
        bool groundedSplash = bus.ReadByte(
            SamusMovementRomData.Environment.WaterSplashTypes + (byte)movementType) != 0;
        if (!groundedSplash)
        {
            AtmosphericEffects.SetSlot(
                0, type: 3, animationFrame: 0, animationTimer: 2,
                samus.XPosition, FxYPosition);
            return;
        }

        AtmosphericEffects.SetSlot(
            0, type: 1, animationFrame: 0, animationTimer: 3,
            unchecked((ushort)(samus.XPosition + 4)),
            unchecked((ushort)(bottom - 4)));
        AtmosphericEffects.SetSlot(
            1, type: 1, animationFrame: 0, animationTimer: 3,
            unchecked((ushort)(samus.XPosition - 3)),
            unchecked((ushort)(bottom - 4)));
    }

    private void TrySpawnAirBubbles(
        SamusState samus,
        ushort top,
        ushort nmiFrameCounter,
        Bank80SystemState system)
    {
        ushort mouthY = unchecked((ushort)(top - 0x18));
        if (unchecked((short)(mouthY - FxYPosition)) < 0 ||
            (nmiFrameCounter & 0x007f) != 0 ||
            AtmosphericEffects.Slots[2].FrameAndType != 0)
        {
            return;
        }

        AtmosphericEffects.SetSlot(
            2, type: 5, animationFrame: 0, animationTimer: 3,
            samus.XPosition,
            unchecked((ushort)(top + 6)));
        ushort random = system.NextRandom();
        QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, (byte)((random & 1) != 0 ? 0x0f : 0x11)), maximumQueued: 6);
    }

    private void TrySpawnLavaSurfaceSpray(
        SamusState samus,
        ushort top,
        ushort nmiFrameCounter)
    {
        if (unchecked((short)(top - LavaAcidYPosition)) >= 0 ||
            (AtmosphericEffects.Slots[0].FrameAndType & 0x0400) != 0)
        {
            return;
        }

        ushort[] xPositions =
        [
            unchecked((ushort)(samus.XPosition + 6)),
            samus.XPosition,
            samus.XPosition,
            unchecked((ushort)(samus.XPosition - 6)),
        ];
        ushort[] timers = [3, 0x8002, 0x8002, 3];
        for (int slot = 0; slot < SamusAtmosphericEffectsState.SlotCount; slot++)
        {
            AtmosphericEffects.SetSlot(
                slot, type: 4, animationFrame: 0, timers[slot],
                xPositions[slot], LavaAcidYPosition);
        }

        if ((nmiFrameCounter & 1) == 0)
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x10), maximumQueued: 6);
    }

    private void TrySpawnRunningFootsteps(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusMovementType movementType)
    {
        if (movementType != SamusMovementType.Running ||
            samus.AnimationFrameTimer != 1 ||
            bus.ReadByte(
                SamusMovementRomData.Environment.RunningFootstepFrames + samus.AnimationFrame) == 0)
        {
            return;
        }

        bool useWetFootsteps = AreaIndex == AreaId.Maridia;
        if (AreaIndex == AreaId.Crateria)
        {
            if (CinematicFunctionActive)
            {
                useWetFootsteps = false;
            }
            else if (RoomIndex < 0x10)
            {
                byte specialType = bus.ReadByte(
                    SamusMovementRomData.Environment.CrateriaFootstepTypes + RoomIndex);
                // The three BIT branches have strict priority. Retail records contain one
                // flag apiece, but retaining priority also reproduces corrupted/debug data.
                if ((specialType & 1) != 0)
                    useWetFootsteps = FxType == RoomFxType.Rain;
                else if ((specialType & 2) != 0)
                    useWetFootsteps = samus.YPosition >= 0x03b0;
                else if ((specialType & 4) != 0)
                    useWetFootsteps = true;
            }
        }

        if (useWetFootsteps)
            SpawnFootstepPair(bus, samus, type: 1);
        else if ((samus.HorizontalSpeed.SpeedBoostCounter & 0xff00) == 0x0400 &&
                 DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) == Air)
            SpawnFootstepPair(bus, samus, type: 7);

        // Graphics are independent of the ordinary step sound. Cinematics, bosses, an
        // active special palette, and boost stage four each suppress library-three sound $06.
        if (!CinematicFunctionActive &&
            BossId == 0 &&
            samus.HorizontalSpeed.SpecialPaletteTimer == 0 &&
            (samus.HorizontalSpeed.SpeedBoostCounter & 0x0400) == 0)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x06), maximumQueued: 6);
        }
    }

    private void SpawnFootstepPair(ISnesAddressSpace bus, SamusState samus, byte type)
    {
        bool directionIsFour = samus.IsFacingLeft(bus);
        ushort firstX = directionIsFour
            ? unchecked((ushort)(samus.XPosition - 12))
            : unchecked((ushort)(samus.XPosition + 12));
        ushort secondX = directionIsFour
            ? unchecked((ushort)(samus.XPosition + 8))
            : unchecked((ushort)(samus.XPosition - 8));
        ushort y = unchecked((ushort)(samus.YPosition + 16));
        AtmosphericEffects.SetSlot(0, type, 0, 0x8002, firstX, y);
        AtmosphericEffects.SetSlot(1, type, 0, 3, secondX, y);
    }

    private void AccumulateLiquidDamage(
        ISnesAddressSpace bus,
        int subDamageAddress,
        int damageAddress)
    {
        AccumulatePeriodicDamage(
            ReadWord(bus, subDamageAddress),
            ReadWord(bus, damageAddress));
    }

    private void QueueSound(SoundEffectId soundEffect, byte maximumQueued) =>
        _soundRequests.Add(new SamusSoundRequest(soundEffect, maximumQueued));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    /// <summary>
    /// Native movement routines distinguish water from lava by the sign of `$195E`, then
    /// perform signed 16-bit subtraction against the selected surface. Retain that exact
    /// ordering so sentinel and wraparound behavior stay debugger-visible.
    /// </summary>
    private ushort DetermineRawMediumAtBoundary(ushort boundary)
    {
        if (unchecked((short)FxYPosition) >= 0)
            return WaterAffectsBoundary(boundary) ? Water : Air;
        return IsBelowSurface(LavaAcidYPosition, boundary) ? LavaAcid : Air;
    }

    private bool WaterAffectsBoundary(ushort boundary) =>
        (LiquidOptions & 0x0004) == 0 && IsBelowSurface(FxYPosition, boundary);

    private static bool IsBelowSurface(ushort surface, ushort boundary) =>
        unchecked((short)surface) >= 0 &&
        unchecked((short)(surface - boundary)) < 0;
}
