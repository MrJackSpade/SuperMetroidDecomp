using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Movement;

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
    /// <summary>$8002: a negative atmospheric timer, so the slot neither moves nor draws while it holds.</summary>
    private const ushort BreathBubbleHeldTimer = SamusAtmosphericEffectsState.DelayedStartMarker | 2;

    /// <summary>Audio requests published by movement, liquid effects, and landing during one Samus handler frame.</summary>
    private readonly List<SamusSoundRequest> _soundRequests = [];
    /// <summary>Transient Power Bomb state that suppresses selected sounds without entering saved Samus state.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? _audioPowerBomb;
    /// <summary>Fallback RNG for standalone animation calls that have no shared bank-$80 owner.</summary>
    private readonly Bank80SystemState _standaloneRandom = new();

    /// <summary>
    /// Exclusive FX dispatcher identity stored at WRAM <c>$196E</c>. Values two and four
    /// are lava/acid, six is water, and zero is the no-FX handler.
    /// </summary>
    public RoomFxType FxType { get; set; }

    /// <summary>
    /// General FX surface Y at WRAM <c>$195E</c>. A negative 16-bit value tells movement
    /// routines to consult <see cref="LavaAcidYPosition"/> instead of the water surface.
    /// </summary>
    public int FxYPosition
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = ushort.MaxValue;

    /// <summary>Lava/acid surface Y at WRAM <c>$1962</c>; negative means absent.</summary>
    public int LavaAcidYPosition
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = ushort.MaxValue;

    /// <summary>
    /// The room's liquid options (WRAM <c>$197E</c>). The water-physics option disables water
    /// interaction even when the geometric surface comparison says Samus is below it.
    /// </summary>
    public RoomLiquidOptions LiquidOptions { get; set; }

    /// <summary>
    /// Remembered native medium at WRAM <c>$0AD2</c>. This is intentionally stateful:
    /// Space Jump reads it, and the animation handlers use changes to detect entry/exit.
    /// </summary>
    public SamusLiquidMedium LiquidPhysicsType { get; internal set; }

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
    public void BeginFrameSoundRequests(SamusPowerBombExplosionState? powerBomb = null)
    {
        _soundRequests.Clear();
        // One live owner spans movement, animation and post-draw calls. Capture its
        // value on each request because a bomb may activate between those stages.
        _audioPowerBomb = powerBomb;
    }

    /// <summary>
    /// Publishes a sound selected by a movement routine into the same per-frame Samus queue
    /// used by liquid entry, footsteps, and landing impact.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general audio backend. Bank-$90 movement owns a handful
    /// of exact queue calls—most notably underwater Space Jump's library-one sound $2F—and
    /// the runtime already begins one shared publication window before movement.
    /// </remarks>
    public void QueueMovementSound(SoundEffectId soundEffect, int maximumQueued) =>
        QueueSound(soundEffect, maximumQueued);

    /// <summary>WRAM <c>$0A4E</c>, the fractional half of pending periodic damage, 0 through 65535.</summary>
    public int PeriodicSubDamage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>WRAM <c>$0A50</c>, the whole-energy half of pending periodic damage, 0 through 65535.</summary>
    public int PeriodicDamage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Logical room pair consumed by area/room-specific footstep graphics.</summary>
    public RoomIdentity RoomIdentity { get; set; }

    /// <summary>Validated retail-area projection used by area-only consumers.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;

    /// <summary>Nonzero cinematic-function state suppresses ordinary footstep audio.</summary>
    public bool CinematicFunctionActive { get; set; }

    /// <summary>
    /// Whether the room's boss identity is set; a boss room suppresses ordinary footstep audio.
    /// No producer in the port sets it, so footsteps play in boss rooms.
    /// </summary>
    public bool BossPresent { get; set; }

    /// <summary>Configures water words while preserving the ordinary or statue room's native FX identity.</summary>
    public void ConfigureWater(int surfaceY, RoomLiquidOptions liquidOptions = RoomLiquidOptions.None, RoomFxType type = RoomFxType.Water)
    {
        if (!RoomFxTypes.UsesWater(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "A water configuration requires a water FX dispatcher.");
        FxType = type;
        FxYPosition = surfaceY;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = liquidOptions;
    }

    /// <summary>Configures the exact room-FX words for a lava (type two) or acid (type four) surface.</summary>
    public void ConfigureLavaAcid(int surfaceY, bool acid = false)
    {
        FxType = acid ? RoomFxType.Acid : RoomFxType.Lava;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = surfaceY;
        LiquidOptions = RoomLiquidOptions.None;
    }

    /// <summary>
    /// Selects a non-liquid room FX while restoring the two negative liquid sentinels.
    /// Rain and other atmospheric owners still need their cartridge type in <c>$196E</c>,
    /// so clearing the entire state would discard observable animation behavior.
    /// </summary>
    public void ConfigureNonLiquidRoomFx(RoomFxType type)
    {
        if (type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Use the liquid-specific configuration method.");
        Clear();
        FxType = type;
    }

    /// <summary>Restores the no-FX sentinel state used by dry rooms.</summary>
    public void Clear()
    {
        FxType = RoomFxType.None;
        FxYPosition = ushort.MaxValue;
        LavaAcidYPosition = ushort.MaxValue;
        LiquidOptions = RoomLiquidOptions.None;
        LiquidPhysicsType = SamusLiquidMedium.Air;
        PeriodicSubDamage = 0;
        PeriodicDamage = 0;
        _soundRequests.Clear();
        AtmosphericEffects.Clear();
    }

    /// <summary>
    /// Selects air/water/lava physics for routines that test Samus's bottom boundary and
    /// bypass all liquid behavior when Gravity Suit bit <c>$0020</c> is equipped.
    /// </summary>
    public SamusLiquidMedium DetermineMovementMedium(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return SamusLiquidMedium.Air;
        // Samus_GetBottom_R18 samples the current pose definition, even before the next
        // movement pass publishes its radius to the live collision state.
        ushort bottom = unchecked((ushort)(samus.Position.Y.Whole + SamusPoseCollisionDefinitions.ReadVerticalRadius(samus.Pose) - 1));
        return DetermineRawMediumAtBoundary(bottom);
    }

    /// <summary>
    /// True when the spin routine's top-boundary test at <c>$90:A449-$A469</c> finds Samus
    /// fully under water/lava. The caller performs the Gravity-Suit palette-bit exemption.
    /// </summary>
    public bool IsTopBoundarySubmerged(SamusState samus)
    {
        Ensure.NotNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.TopBoundary.Y) != SamusLiquidMedium.Air;
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
        Ensure.NotNull(samus);
        return DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) != SamusLiquidMedium.Air;
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
        Ensure.NotNull(samus);
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
        Ensure.NotNull(samus);
        if (samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return checked((ushort)samus.XSpeedDivisor);

        ushort bottomMinusOne = unchecked((ushort)(
            samus.Position.Y.Whole + SamusPoseCollisionDefinitions.ReadVerticalRadius(samus.Pose) - 1));
        SamusLiquidMedium medium = DetermineRawMediumAtBoundary(bottomMinusOne);
        return medium switch
        {
            SamusLiquidMedium.Water => 3,
            SamusLiquidMedium.LavaOrAcid => 2,
            SamusLiquidMedium.Air => checked((ushort)samus.XSpeedDivisor),
            _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
        };
    }

    /// <summary>
    /// Runs <c>Samus_Animate</c>'s complete bank-$90 FX dispatch before the generic animation
    /// timer: liquid delay, medium changes, four particle producers, sound requests, and
    /// lava/acid fixed-point damage accumulation.
    /// </summary>
    public void PrepareAnimationFrame(
        SamusState samus,
        int nmiFrameCounter = 0,
        Bank80SystemState? system = null,
        bool beginSoundRequestFrame = true)
    {
        Ensure.NotNull(samus);

        // Direct verifier/debugger calls begin a publication window here. The full runtime
        // begins it before movement and passes false, preserving collision sounds queued by
        // `$91:F046` before this bank-$90 animation phase. Visual slots and damage words are
        // persistent WRAM and therefore are never cleared merely because a frame began.
        if (beginSoundRequestFrame)
            BeginFrameSoundRequests();

        SamusMovementType movementType = samus.PoseMetadata.ReadMovementType();
        TrySpawnRunningFootsteps(samus, movementType);

        // Samus_Animate's FX handlers call $90:EC3E, whose occupied bottom
        // pixel is Y + pose radius - 1. The exclusive collision boundary would
        // enter liquid one frame early and can prematurely cancel lava momentum.
        ushort bottom = samus.Kinematics.BottomPixel.Y;
        ushort top = samus.Kinematics.TopBoundary.Y;
        RoomFxType fxKind = FxType;
        bool gravitySuit = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);

        if (RoomFxTypes.UsesWater(fxKind) && WaterAffectsBoundary(bottom))
        {
            // `$90:80B8` publishes delay three before testing the remembered medium. A
            // transition into water queues sound $0D and creates either a diving splash or
            // two grounded splashes; every submerged call then gets the bubble opportunity.
            bool enteredWater = LiquidPhysicsType != SamusLiquidMedium.Water;
            LiquidPhysicsType = SamusLiquidMedium.Water;
            samus.AnimationFrameBuffer = 3;
            if (enteredWater)
            {
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 13), maximumQueued: 6);
                SpawnWaterSplash(samus, movementType, bottom);
            }

            TrySpawnAirBubbles(
                samus,
                top,
                nmiFrameCounter,
                system ?? _standaloneRandom);

            // Standing forward `$00`, backward `$9B`, and every Gravity-Suit pose clear the
            // buffer in Spawn_AirBubbles after all particle/audio side effects have run.
            if (samus.Pose is SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose || gravitySuit)
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
                samus.HorizontalSpeed.CancelRunningMomentum(samus.PoseMetadata.ReadFacingDirection());
                samus.HorizontalSpeed.ExtraRunSpeed = 0;
                samus.HorizontalSpeed.ExtraRunSubspeed = 0;
            }

            if (fxKind == RoomFxType.Lava && gravitySuit)
            {
                // Lava's Gravity-Suit branch returns before damage and before surface spray.
                // Acid intentionally has no equivalent early exit and still hurts at quarter
                // strength when HandlePeriodicDamage runs later in the same Samus handler.
                LiquidPhysicsType = SamusLiquidMedium.LavaOrAcid;
                samus.AnimationFrameBuffer = 0;
                return;
            }

            SamusLiquidDamageRate damageRate = fxKind == RoomFxType.Lava
                ? SamusLiquidDamageDefinitions.Lava
                : SamusLiquidDamageDefinitions.Acid;
            AccumulatePeriodicDamage(checked((ushort)damageRate.SubDamage), checked((ushort)damageRate.WholeDamage));
            if ((nmiFrameCounter & 7) == 0 && samus.Health >= 71)
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 45), maximumQueued: 3);

            // Both lava and acid now share `$90:824C`'s delay-two submerged path.
            LiquidPhysicsType = SamusLiquidMedium.LavaOrAcid;
            samus.AnimationFrameBuffer = 2;
            TrySpawnLavaSurfaceSpray(samus, top, nmiFrameCounter);
            if (samus.Pose is SamusPoseId.ForwardFacingPowerSuitPose or
                SamusPoseId.ForwardFacingSuitedPose || gravitySuit)
                samus.AnimationFrameBuffer = 0;
            return;
        }

        // `$90:8078` publishes the X-speed divisor and clears the remembered medium. Only
        // water (bit zero set) owns an exit splash; lava/acid simply clears `$0AD2`.
        samus.AnimationFrameBuffer = checked((ushort)samus.XSpeedDivisor);
        bool exitedWater = LiquidPhysicsType == SamusLiquidMedium.Water;
        LiquidPhysicsType = SamusLiquidMedium.Air;
        if (exitedWater)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 14), maximumQueued: 6);
            if (!gravitySuit && movementType is
                SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
                QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 48), maximumQueued: 6);
            SpawnWaterSplash(samus, movementType, bottom);
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
        SamusState samus,
        SamusMovementType previousMovementType,
        SamusPoseId previousPose,
        int impactYSpeed,
        int impactYSubspeed)
    {
        Ensure.NotNull(samus);
        Ensure.BetweenInclusive(0, ushort.MaxValue, impactYSpeed);
        Ensure.BetweenInclusive(0, ushort.MaxValue, impactYSubspeed);

        // `$91:F046-$F074` ends the spin-loop sound separately from the impact. The test is
        // on PREVIOUS movement type and pose because landing selection has not run yet.
        // Cinematics suppress both the ordinary-spin `$32` and Screw Attack `$34` requests.
        if (!CinematicFunctionActive && previousMovementType is
            SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
        {
            QueueSound(previousPose is SamusPoseId.ScrewAttackRightPose or
                    SamusPoseId.ScrewAttackLeftPose
                        ? SoundEffectLibrary1Sounds.StopScrewAttack
                        : SoundEffectLibrary1Sounds.StopSpinJump,
                maximumQueued: 6);
        }

        // A truly stationary grounding probe returns before impact audio AND graphics. Any
        // nonzero fractional descent is a soft landing; whole speed five or greater is hard.
        // Native Y speed is an unsigned magnitude, so CMP #$0005/BPL is equivalent here.
        if (impactYSpeed == 0 && impactYSubspeed == 0)
            return;

        if (!CinematicFunctionActive)
        {
            QueueSound(impactYSpeed >= 5
                    ? SoundEffectLibrary3Sounds.HardLanding
                    : SoundEffectLibrary3Sounds.SoftLanding,
                maximumQueued: 6);
        }

        HandleLandingGraphics(samus);
    }

    /// <summary>Selects landing dust or splash by area and room while leaving impact sound policy to the caller.</summary>
    private void HandleLandingGraphics(SamusState samus)
    {
        // `$91:F0AA` is an eight-entry area jump table. Invalid area bytes would execute
        // unrelated bank-$91 data on hardware; fail loudly instead of manufacturing output.
        switch (AreaIndex)
        {
            case AreaId.Crateria:
                HandleCrateriaLandingGraphics(samus);
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

    /// <summary>Applies Crateria's cinematic, rain, entrance-depth, and room-index landing particle branches.</summary>
    private void HandleCrateriaLandingGraphics(SamusState samus)
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

        if (RoomIdentity.RoomIndex >= 16)
        {
            DeleteLandingPair();
            return;
        }

        // The cartridge duplicates this room policy for running footsteps and landing
        // effects. Preserve its BIT priority even though retail rows contain one bit each.
        CrateriaAtmosphericEffectFlags roomFlags =
            SamusAtmosphericEffectDefinitions.ForCrateriaRoom(RoomIdentity.RoomIndex);
        if ((roomFlags & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
        {
            // Landing Site creates splashes only for FX type `$000A`; its normal scrolling-
            // sky type deletes the pair. This exact comparison is not a generic water test.
            if (FxType == RoomFxType.Rain)
                SpawnLandingPairUnlessSubmerged(samus, type: 1);
            else
                DeleteLandingPair();
            return;
        }

        if ((roomFlags & CrateriaAtmosphericEffectFlags.WreckedShipEntrance) != 0)
        {
            // Above Y=$03B0 the entrance is dry and deletes; at/below it, use wet splashes.
            if (samus.Position.Y.Whole >= 944)
                SpawnLandingPairUnlessSubmerged(samus, type: 1);
            else
                DeleteLandingPair();
            return;
        }

        if ((roomFlags & CrateriaAtmosphericEffectFlags.WetFootsteps) != 0)
        {
            SpawnLandingPairUnlessSubmerged(samus, type: 1);
            return;
        }

        DeleteLandingPair();
    }

    /// <summary>Writes landing-owned particle slots two and three when Samus's last occupied pixel is above active liquid.</summary>
    private void SpawnLandingPairUnlessSubmerged(SamusState samus, byte type)
    {
        // GetBottom_R18 uses the current pose definition and includes the last
        // occupied pixel; the live collision radius can still belong to the old pose.
        ushort bottom = unchecked((ushort)(samus.Position.Y.Whole +
            SamusPoseCollisionDefinitions.ReadVerticalRadius(samus.Pose) - 1));

        // Both `$91:F116` and `$91:F166` suppress their particles when Samus's current
        // bottom is genuinely below active water or lava/acid. `DetermineRawMedium...`
        // preserves the signed surface sentinels and water option-bit-two exemption used by
        // those routines. Importantly, suppression RETURNS and leaves old slot data intact.
        if (DetermineRawMediumAtBoundary(bottom) != SamusLiquidMedium.Air)
            return;

        ushort rightOffset = type == 1 ? (ushort)4 : (ushort)8;
        ushort leftSeparation = type == 1 ? (ushort)7 : (ushort)16;
        ushort firstX = unchecked((ushort)(samus.Position.X.Whole + rightOffset));
        ushort secondX = unchecked((ushort)(firstX - leftSeparation));
        ushort y = type == 1 ? unchecked((ushort)(bottom - 4)) : bottom;

        // Landing owns WRAM slots two and three (byte offsets +4/+6), not the running-foot
        // slots zero/one. Both begin immediately on frame zero with timer three.
        AtmosphericEffects.SetSlot(2, type, 0, 3, new(firstX, y));
        AtmosphericEffects.SetSlot(3, type, 0, 3, new(secondX, y));
    }

    /// <summary>Clears only the packed frame/type words of the two landing slots, preserving their timers and positions.</summary>
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
        Ensure.NotNull(samus);
        if (timeIsFrozen)
        {
            PeriodicSubDamage = 0;
            PeriodicDamage = 0;
            return;
        }

        ushort appliedSubDamage = checked((ushort)PeriodicSubDamage);
        ushort appliedDamage = checked((ushort)PeriodicDamage);
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
                SnesLittleEndian.Word(SnesLittleEndian.HighByte(appliedSubDamage), SnesLittleEndian.LowByte(appliedDamage))));
            overlappingDamage = unchecked((ushort)(overlappingDamage >> divisorShift));
            appliedSubDamage = SnesLittleEndian.Word(0, SnesLittleEndian.LowByte(overlappingDamage));
            appliedDamage = unchecked((ushort)(SnesLittleEndian.HighByte(overlappingDamage)));
        }

        // `$90:EA11` deliberately jumps to the native crash handler if the signed whole
        // word is negative. Reachable environmental rates never do this; throwing retains
        // the corruption/overflow guard instead of converting it into plausible damage.
        if (unchecked((short)appliedDamage) < 0)
            throw new InvalidDataException("Periodic whole damage overflowed into the signed-negative range.");

        samus.TakeFractionalEnergyDamage(appliedSubDamage, appliedDamage);

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
            PeriodicDamage + wholeDamage + SnesCarry.OutOfWord(fractional)));
    }

    /// <summary>Creates either the diving surface particle or paired shallow splashes selected by movement type.</summary>
    internal void SpawnWaterSplash(
        SamusState samus,
        SamusMovementType movementType,
        ushort bottom)
    {
        if (SamusAtmosphericEffectDefinitions.WaterSplashFor(movementType) ==
            WaterSplashKind.Diving)
        {
            AtmosphericEffects.SetSlot(
                0, type: 3, animationFrame: 0, animationTimer: 2,
                new(samus.Position.X.Whole, checked((ushort)FxYPosition)));
            return;
        }

        AtmosphericEffects.SetSlot(
            0, type: 1, animationFrame: 0, animationTimer: 3,
            new(unchecked((ushort)(samus.Position.X.Whole + 4)), unchecked((ushort)(bottom - 4))));
        AtmosphericEffects.SetSlot(
            1, type: 1, animationFrame: 0, animationTimer: 3,
            new(unchecked((ushort)(samus.Position.X.Whole - 3)), unchecked((ushort)(bottom - 4))));
    }

    /// <summary>Creates a bubble every 128 accepted NMIs only when Samus's mouth is underwater and particle slot two is free.</summary>
    private void TrySpawnAirBubbles(
        SamusState samus,
        ushort top,
        int nmiFrameCounter,
        Bank80SystemState system)
    {
        ushort mouthY = unchecked((ushort)(top - 24));
        if (unchecked((short)(mouthY - FxYPosition)) < 0 ||
            (nmiFrameCounter & 127) != 0 ||
            AtmosphericEffects.Slots[2].FrameAndType != 0)
        {
            return;
        }

        AtmosphericEffects.SetSlot(
            2, type: 5, animationFrame: 0, animationTimer: 3,
            new(samus.Position.X.Whole, unchecked((ushort)(top + 6))));
        ushort random = system.NextRandom();
        QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, (byte)((random & 1) != 0 ? 15 : 17)), maximumQueued: 6);
    }

    /// <summary>Starts the four-slot lava spray while Samus's top is above the surface and the previous spray has cleared.</summary>
    private void TrySpawnLavaSurfaceSpray(
        SamusState samus,
        ushort top,
        int nmiFrameCounter)
    {
        if (unchecked((short)(top - LavaAcidYPosition)) >= 0 ||
            (AtmosphericEffects.Slots[0].FrameAndType & 1 << 10) != 0)
        {
            return;
        }

        ushort[] xPositions =
        [
            unchecked((ushort)(samus.Position.X.Whole + 6)),
            samus.Position.X.Whole,
            samus.Position.X.Whole,
            unchecked((ushort)(samus.Position.X.Whole - 6)),
        ];
        ushort[] timers = [3, BreathBubbleHeldTimer, BreathBubbleHeldTimer, 3];
        for (int slot = 0; slot < SamusAtmosphericEffectsState.SlotCount; slot++)
        {
            AtmosphericEffects.SetSlot(
                slot, type: 4, animationFrame: 0, timers[slot],
                new(xPositions[slot], checked((ushort)LavaAcidYPosition)));
        }

        if ((nmiFrameCounter & 1) == 0)
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 16), maximumQueued: 6);
    }

    /// <summary>Emits room-dependent wet or boosted footstep art at running contact frames and applies native sound suppression.</summary>
    private void TrySpawnRunningFootsteps(
        SamusState samus,
        SamusMovementType movementType)
    {
        if (movementType != SamusMovementType.Running ||
            samus.AnimationFrameTimer.RawValue != 1 ||
            !SamusAtmosphericEffectDefinitions.IsRunningFootContact(samus.AnimationFrame))
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
            else if (RoomIdentity.RoomIndex < 16)
            {
                CrateriaAtmosphericEffectFlags specialType =
                    SamusAtmosphericEffectDefinitions.ForCrateriaRoom(RoomIdentity.RoomIndex);
                // The three BIT branches have strict priority. Retail records contain one
                // flag apiece, but retaining priority also reproduces corrupted/debug data.
                if ((specialType & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                    useWetFootsteps = FxType == RoomFxType.Rain;
                else if ((specialType & CrateriaAtmosphericEffectFlags.WreckedShipEntrance) != 0)
                    useWetFootsteps = samus.Position.Y.Whole >= 944;
                else if ((specialType & CrateriaAtmosphericEffectFlags.WetFootsteps) != 0)
                    useWetFootsteps = true;
            }
        }

        if (useWetFootsteps)
            SpawnFootstepPair(samus, type: 1);
        else if (samus.HorizontalSpeed.IsFullSpeedBoost &&
                 DetermineRawMediumAtBoundary(samus.Kinematics.BottomBoundary) == SamusLiquidMedium.Air)
            SpawnFootstepPair(samus, type: 7);

        // Graphics are independent of the ordinary step sound. Cinematics, bosses, an
        // a stored shinespark, and boost stage four each suppress library-three sound $06.
        // This is $0A68, not $0AD0: the latter is already nonzero during boost buildup.
        if (!CinematicFunctionActive &&
            !BossPresent &&
            samus.InputSnapshot.SharedShineTimer == 0 &&
            (samus.HorizontalSpeed.SpeedBoostCounter & 1 << 10) == 0)
        {
            QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 6), maximumQueued: 6);
        }
    }

    /// <summary>Places the two running-footstep particles on opposite sides of Samus according to facing.</summary>
    private void SpawnFootstepPair(SamusState samus, byte type)
    {
        bool directionIsFour = samus.PoseMetadata.IsFacingLeft();
        ushort firstX = directionIsFour
            ? unchecked((ushort)(samus.Position.X.Whole - 12))
            : unchecked((ushort)(samus.Position.X.Whole + 12));
        ushort secondX = directionIsFour
            ? unchecked((ushort)(samus.Position.X.Whole + 8))
            : unchecked((ushort)(samus.Position.X.Whole - 8));
        ushort y = unchecked((ushort)(samus.Position.Y.Whole + 16));
        AtmosphericEffects.SetSlot(0, type, 0, BreathBubbleHeldTimer, new(firstX, y));
        AtmosphericEffects.SetSlot(1, type, 0, 3, new(secondX, y));
    }

    /// <summary>Publishes a bounded sound request marked for suppression while a Power Bomb is active.</summary>
    private void QueueSound(SoundEffectId soundEffect, int maximumQueued) =>
        _soundRequests.Add(new SamusSoundRequest(soundEffect, maximumQueued,
            SoundSuppressed: _audioPowerBomb?.IsActive == true));

    /// <summary>
    /// Native movement routines distinguish water from lava by the sign of `$195E`, then
    /// perform signed 16-bit subtraction against the selected surface. Retain that exact
    /// ordering so sentinel and wraparound behavior stay debugger-visible.
    /// </summary>
    private SamusLiquidMedium DetermineRawMediumAtBoundary(ushort boundary)
    {
        if (unchecked((short)FxYPosition) >= 0)
            return WaterAffectsBoundary(boundary) ? SamusLiquidMedium.Water : SamusLiquidMedium.Air;
        return IsBelowSurface(LavaAcidYPosition, boundary) ? SamusLiquidMedium.LavaOrAcid : SamusLiquidMedium.Air;
    }

    /// <summary>Applies the room's water-disable option before comparing a boundary with the water surface.</summary>
    internal bool WaterAffectsBoundary(ushort boundary) =>
        (LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) == 0 &&
        IsBelowSurface(FxYPosition, boundary);

    /// <summary>Uses native signed 16-bit subtraction and rejects a negative absent-surface sentinel.</summary>
    internal static bool IsBelowSurface(int surface, ushort boundary) =>
        unchecked((short)surface) >= 0 &&
        unchecked((short)(surface - boundary)) < 0;

    /// <summary>Ports $91:FA76's pose-entry dust writes, shared by ordinary and grapple wall jumps.</summary>
    internal void SpawnWallJumpDust(SamusState samus)
    {
        // Get_Samus_Bottom_Boundary ($90:EC3E) uses the last occupied pixel of the current
        // pose's definition radius, not live $0B00, which keeps the spin body's radius
        // through the wall-jump frame. Liquid suppression leaves all previous slot words
        // intact, even when Gravity Suit makes movement itself behave as though the room
        // were dry.
        ushort bottom = unchecked((ushort)(samus.Position.Y.Whole + SamusPoseMetadata.ReadPoseYRadius(samus.Pose) - 1));
        if (DetermineRawMediumAtBoundary(bottom) != SamusLiquidMedium.Air)
            return;
        int offset = samus.PoseMetadata.IsFacingRight()
            ? -SamusWallJumpDustDefinitions.HorizontalOffset
            : SamusWallJumpDustDefinitions.HorizontalOffset;
        AtmosphericEffects.SetSlot(SamusWallJumpDustDefinitions.Slot,
            SamusWallJumpDustDefinitions.Type, 0, SamusWallJumpDustDefinitions.InitialTimer,
            new(unchecked((ushort)(samus.Position.X.Whole + offset)), bottom));
    }
}
