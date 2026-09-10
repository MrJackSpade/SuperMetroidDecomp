using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-word state for stored Speed Booster shine and Samus's shinespark handler.
/// </summary>
/// <remarks>
/// This is a transcription of the bank-$90/$91 state machine, not a desktop “dash”
/// controller. Timers wrap as 16-bit words, movement retains split 16.16 values, and every
/// palette is selected through the user's ROM. Solid-enemy collision remains an explicit
/// boundary; block collision uses the translated bank-$94 dispatcher.
/// </remarks>
public sealed class SamusShinesparkState
{
    /// <summary>WRAM <c>$0A68</c>: zero, stored-shine handler one, or spark handler six.</summary>
    public ushort PaletteType { get; private set; }

    /// <summary>WRAM <c>$0A6A</c>: 180 stored, 60 windup, and 15 during active motion.</summary>
    public ushort ShineTimer { get; private set; }

    /// <summary>WRAM <c>$0ACE</c>, retained as native even byte offsets 0/2/4/….</summary>
    public ushort PaletteFrameOffset { get; private set; }

    /// <summary>WRAM <c>$0A64</c>, the shared 30-frame windup/crash countdown.</summary>
    public ushort StartStopTimer { get; private set; }

    /// <summary>
    /// Whole word of vertical spark acceleration. Native aliases it to <c>substate</c> and
    /// seeds it to 7.0000 at <c>$90:CFFA</c>.
    /// </summary>
    public ushort VerticalAccelerationSpeed { get; private set; }

    /// <summary>Fractional word paired with <see cref="VerticalAccelerationSpeed"/>.</summary>
    public ushort VerticalAccelerationSubspeed { get; private set; }

    /// <summary>Debugger-readable replacement for the installed bank-$90 handler pointer.</summary>
    public ShinesparkPhase Phase { get; private set; }

    /// <summary>Set at stored timer 170, matching SFX queue three `$0C`.</summary>
    public bool StoredShineWarningSoundRequested { get; set; }

    /// <summary>Set when direction selection requests SFX queue three `$0F`.</summary>
    public bool LaunchSoundRequested { get; set; }

    /// <summary>Set when energy or collision installs the crash handler.</summary>
    public bool CrashSoundRequested { get; set; }

    /// <summary>Consumes the stored-shine warning's <c>QueueSfx3_Max9($0C)</c> call.</summary>
    public bool ConsumeStoredShineWarningSoundRequest()
    {
        bool requested = StoredShineWarningSoundRequested;
        StoredShineWarningSoundRequested = false;
        return requested;
    }

    /// <summary>Consumes directional launch's <c>QueueSfx3_Max9($0F)</c> call.</summary>
    public bool ConsumeLaunchSoundRequest()
    {
        bool requested = LaunchSoundRequested;
        LaunchSoundRequested = false;
        return requested;
    }

    /// <summary>
    /// Consumes crash setup. Retail publishes two calls at this point:
    /// <c>QueueSfx1_Max6($35)</c> and <c>QueueSfx3_Max6($10)</c>.
    /// </summary>
    public bool ConsumeCrashSoundRequest()
    {
        bool requested = CrashSoundRequested;
        CrashSoundRequested = false;
        return requested;
    }

    /// <summary>High byte of native `$0AAE` while the two crash echoes orbit Samus.</summary>
    public byte CrashSubphase { get; private set; }

    /// <summary>Low byte of native `$0AAE`, expanded/separated/contracted in steps of four.</summary>
    public byte CrashRadius { get; private set; }

    /// <summary>First byte-angle formerly stored in speed-echo X speed slot zero.</summary>
    public SnesAngle FirstCrashEchoAngle { get; private set; }

    /// <summary>Second byte-angle, exactly 128 degrees-of-256 opposite the first.</summary>
    public SnesAngle SecondCrashEchoAngle { get; private set; }

    /// <summary>
    /// WRAM $0ABC: crash angular travel and released projectile-slot-three echo Y share
    /// one word. Native crash entry retains it; only an actual echo write/clear replaces it.
    /// </summary>
    public ushort CrashAngularTravel
    {
        get => _firstReleasedCrashEcho.YPosition;
        private set => _firstReleasedCrashEcho.YPosition = value;
    }

    /// <summary>
    /// Host-readable echo drawing words associated with native projectile slot three.
    /// Active is the drawing enable, not ownership: a combo can overwrite the projectile
    /// without clearing these words or allowing the old echo handler to keep running.
    /// </summary>
    public ShinesparkReleasedEcho FirstReleasedCrashEcho => _firstReleasedCrashEcho.Snapshot;

    /// <summary>
    /// Host-readable snapshot of native projectile slot four. Native draws this slot before
    /// slot three, even though it is initialized second.
    /// </summary>
    public ShinesparkReleasedEcho SecondReleasedCrashEcho => _secondReleasedCrashEcho.Snapshot;

    /// <summary>Number of enabled departing echo drawings, independent of slot ownership.</summary>
    public int ReleasedCrashEchoCount =>
        (_firstReleasedCrashEcho.Active ? 1 : 0) +
        (_secondReleasedCrashEcho.Active ? 1 : 0);

    /// <summary>
    /// Most recent viewport deletion, retained only as a debugger witness after native slot
    /// storage has been zeroed. It does not participate in movement or drawing.
    /// </summary>
    public ShinesparkReleasedEchoClear? LastReleasedCrashEchoClear { get; private set; }

    private readonly ReleasedEchoSlot _firstReleasedCrashEcho = new();
    private readonly ReleasedEchoSlot _secondReleasedCrashEcho = new();

    /// <summary>
    /// Samus_HandleTransitionsB_5 ($91:EEA6) clears $0A6A and replaces the shared
    /// special-palette handler with X-Ray. Relinquish this owner's palette state so
    /// it cannot keep ticking or repaint over the newly installed visor palette.
    /// </summary>
    internal void RelinquishPaletteToXray()
    {
        ShineTimer = 0;
        PaletteType = 0;
        PaletteFrameOffset = 0;
        if (Phase == ShinesparkPhase.Stored)
            Phase = ShinesparkPhase.Inactive;
    }

    /// <summary>
    /// ResetProjectileData ($90:AD22) clears the complete speed-echo arrays and index.
    /// Keep the movement-handler phase and its unrelated timers unchanged.
    /// </summary>
    internal void ResetProjectileEchoStorage()
    {
        _firstReleasedCrashEcho.Clear();
        _secondReleasedCrashEcho.Clear();
        FirstCrashEchoAngle = SecondCrashEchoAngle = SnesAngle.Zero;
        CrashSubphase = CrashRadius = 0;
        LastReleasedCrashEchoClear = null;
    }

    /// <summary>
    /// Ports <c>Samus_CrouchTrans</c> at <c>$91:F7B0</c>. The signed comparison accepts
    /// stage four and any larger high byte exactly as the 65816 does.
    /// </summary>
    public bool TryStoreFromSpeedBooster(ushort speedBoostCounter)
    {
        if (unchecked((short)((speedBoostCounter & 0xff00) -
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)) < 0)
            return false;

        ShineTimer = 180;
        PaletteType = 1;
        PaletteFrameOffset = 0;
        Phase = ShinesparkPhase.Stored;
        StoredShineWarningSoundRequested = false;
        return true;
    }

    /// <summary>
    /// Ports <c>Projectile_Func7_Shinespark</c> at <c>$90:CFFA</c> after bank $91 has
    /// selected windup pose `$C7/$C8`.
    /// </summary>
    public void BeginWindup(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase != ShinesparkPhase.Stored || ShineTimer == 0)
            throw new InvalidOperationException("Shinespark windup requires a live stored shine.");

        InitializeWindup(samus);
    }

    /// <summary>DemoSetFunc_5/6 call the native windup initializer without a stored shine.</summary>
    internal void BeginDemoLaunch(ISnesAddressSpace bus, SamusState samus, byte targetPose)
    {
        InitializeWindup(samus);
        BeginDirectionalLaunch(bus, samus, targetPose);
    }

    private void InitializeWindup(SamusState samus)
    {

        // These are literal native writes. Stage four remains published during windup even
        // though base speed is zero and the extra component becomes exactly 8.0000.
        samus.Kinematics.YDirection = 1;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        samus.HorizontalSpeed.SpeedBoostCounter =
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter;
        samus.HorizontalSpeed.ExtraRunSpeed = 8;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.BombJumpDirection = 0;

        VerticalAccelerationSpeed = 7;
        VerticalAccelerationSubspeed = 0;
        StartStopTimer = 30;
        ShineTimer = 60;
        PaletteType = 6;
        PaletteFrameOffset = 0;
        Phase = ShinesparkPhase.Windup;
        samus.HurtFlashCounter = 0;
        LaunchSoundRequested = false;
        CrashSoundRequested = false;
    }

    /// <summary>
    /// Installs the handler selected by <c>SamusFunc_F468_Shinespark</c> at
    /// <c>$91:F80F</c> for poses `$C9-$CE`.
    /// </summary>
    public void BeginDirectionalLaunch(ISnesAddressSpace bus, SamusState samus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase != ShinesparkPhase.Windup)
            throw new InvalidOperationException("Directional shinespark launch requires windup.");

        Phase = targetPose switch
        {
            SamusPoseIds.ShinesparkHorizontalRightPose or
            SamusPoseIds.ShinesparkHorizontalLeftPose => ShinesparkPhase.Horizontal,
            SamusPoseIds.ShinesparkVerticalRightPose or
            SamusPoseIds.ShinesparkVerticalLeftPose => ShinesparkPhase.Vertical,
            SamusPoseIds.ShinesparkDiagonalRightPose or
            SamusPoseIds.ShinesparkDiagonalLeftPose => ShinesparkPhase.Diagonal,
            // `$91:F80F` is reached only from the six table-selected `$C9-$CE` records.
            // Any other byte is a caller contract violation, not another untranslated arm.
            _ => throw new ArgumentOutOfRangeException(
                nameof(targetPose),
                $"Pose ${targetPose:X2} is not one of the six directional shinespark poses."),
        };

        samus.Pose = targetPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        samus.HorizontalSpeed.ResetSpeedEchoPositionsForShinespark();
        LaunchSoundRequested = true;
    }

    /// <summary>Runs one installed special-handler frame from <c>$90:D068-$D2B9</c>.</summary>
    public ShinesparkMovementResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        ushort projectileCounter = 0,
        RoomPlmSystem? plms = null,
        bool playerInvincibilityEnabled = false,
        SamusProjectileSystem? projectiles = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);

        ShinesparkPhase phaseAtStart = Phase;
        if (Phase == ShinesparkPhase.Windup)
        {
            // DEC/BEQ/BMI at `$90:D068`: exact zero and signed underflow both launch up.
            NativeWordCounterStep timer = NativeWordCounter.Decrement(StartStopTimer);
            StartStopTimer = timer.Value;
            bool timedOut = timer.IsZeroOrNegative;
            if (timedOut)
            {
                byte verticalPose = samus.IsFacingLeft(bus)
                    ? SamusPoseIds.ShinesparkVerticalLeftPose
                    : SamusPoseIds.ShinesparkVerticalRightPose;
                BeginDirectionalLaunch(bus, samus, verticalPose);
            }

            return new ShinesparkMovementResult(
                phaseAtStart, null, null, timedOut, false, false, false);
        }

        if (Phase is not (ShinesparkPhase.Horizontal or
            ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal))
        {
            if (Phase == ShinesparkPhase.Crash)
                return StepCrashOrbit(bus, samus, phaseAtStart);
            if (Phase == ShinesparkPhase.CrashEchoCircle)
                return StepCrashEchoCircle(phaseAtStart);
            if (Phase == ShinesparkPhase.CrashFinish)
                return FinishCrash(bus, samus, phaseAtStart,
                    projectiles?.ProjectileCounter ?? projectileCounter, projectiles);

            // Stored/inactive states are not installed special movement handlers. Returning
            // an empty snapshot is defensive; runtime normally never dispatches them here.
            return new ShinesparkMovementResult(
                phaseAtStart, null, null, false, false, false, false);
        }

        samus.HorizontalSpeed.ContactDamageIndex = 2;
        samus.HurtFlashCounter = 8;

        // Each active handler samples echoes before motion. Runtime suppresses its ordinary
        // post-motion producer while this special handler owns the frame.
        samus.HorizontalSpeed.CaptureSpeedEchoPosition(
            nmiFrameCounter,
            samus.XPosition,
            samus.YPosition);

        BlockMoveResult? horizontal = null;
        BlockMoveResult? vertical = null;
        if (Phase is ShinesparkPhase.Horizontal or ShinesparkPhase.Diagonal)
            horizontal = MoveX(bus, level, samus, plms);
        if (Phase is ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal)
            vertical = MoveY(bus, level, samus, nmiFrameCounter, plms);

        bool collided = horizontal is { Collided: true } || vertical is { Collided: true };
        // The host cheat bypasses only the energy exit. Terrain/enemy collision still
        // terminates all three launch directions through the ordinary crash handler.
        bool hasNativeEnergy = unchecked((short)(samus.Health -
            SamusSpecialSequenceRomData.Shinespark.MinimumSustainingEnergy)) >= 0;
        bool lowEnergy = !playerInvincibilityEnabled && !hasNativeEnergy;
        if (collided || lowEnergy)
            BeginCrash(bus, samus);

        // At exactly 30 energy, this frame still moves and drains to 29. The following
        // frame moves once more before EndSuperJump observes that 29 is below the threshold.
        // Invincible playtesting retains energy loss without requiring refills: below
        // the native cutoff, continue draining to one, never decrementing zero or one.
        bool drained = playerInvincibilityEnabled
            ? samus.Health > 1
            : hasNativeEnergy;
        if (drained)
            samus.Health = unchecked((ushort)(samus.Health - 1));

        return new ShinesparkMovementResult(
            phaseAtStart, horizontal, vertical, false, collided, lowEnergy, drained);
    }

    /// <summary>
    /// Runs palette handler one or six from <c>$91:D6F7</c>, including normal restoration
    /// when its signed countdown expires.
    /// </summary>
    public bool UpdatePalette(ISnesAddressSpace bus, SnesCgram cgram, ushort equippedItems)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        if (PaletteType is not (1 or 6))
            return false;

        if (PaletteType == 1 && ShineTimer == 170)
            StoredShineWarningSoundRequested = true;

        NativeWordCounterStep timer = NativeWordCounter.Decrement(ShineTimer);
        ShineTimer = timer.Value;
        bool expired = timer.IsZeroOrNegative;
        if (expired)
        {
            PaletteType = 0;
            PaletteFrameOffset = 0;
            if (Phase == ShinesparkPhase.Stored)
                Phase = ShinesparkPhase.Inactive;
            LoadNormalSuitPalette(bus, cgram, equippedItems);
            return true;
        }

        ushort suitOffset = equippedItems.GetSuitPaletteTableOffset();
        int tableAddress = PaletteType == 1
            ? SamusPaletteRomData.FullBodyCycles.StoredShineLists
            : SamusPaletteRomData.FullBodyCycles.ActiveShinesparkLists;
        ushort listPointer = ReadWord(bus, tableAddress + suitOffset);
        ushort palettePointer = ReadWord(
            bus,
            SamusPaletteRomData.Banks.Movement |
                unchecked((ushort)(listPointer + PaletteFrameOffset)));
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | palettePointer,
            colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
            destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);

        ushort exclusiveLimit = PaletteType == 1 ? (ushort)12 : (ushort)8;
        PaletteFrameOffset = unchecked((ushort)(PaletteFrameOffset + 2));
        if (PaletteFrameOffset >= exclusiveLimit)
            PaletteFrameOffset = 0;
        return true;
    }

    private void BeginCrash(ISnesAddressSpace bus, SamusState samus)
    {
        // `$90:D2BA-$D345` kills translational velocity before installing the crash handler.
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 0;
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.SpeedBoostCounter = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        samus.HurtFlashCounter = 0;
        bool facingLeft = samus.IsFacingLeft(bus);
        FirstCrashEchoAngle = SnesAngle.FromTableIndex(facingLeft ? (byte)32 : (byte)224);
        SecondCrashEchoAngle = SnesAngle.FromTableIndex(facingLeft ? (byte)160 : (byte)96);
        CrashAngularDelta = facingLeft ? (short)4 : (short)-4;
        _firstReleasedCrashEcho.DisableDrawing();
        CrashSubphase = 0;
        CrashRadius = 0;
        // The native entry does not initialize the aliased angular-travel/echo-Y
        // word. A preceding echo or crash can therefore shorten this orbit.
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            encodedIndex: 0,
            samus.XPosition,
            samus.XPosition,
            samus.YPosition,
            samus.YPosition);
        Phase = ShinesparkPhase.Crash;
        CrashSoundRequested = true;
    }

    /// <summary>
    /// WRAM $0AB4: crash angular delta aliases departing slot-three echo X. Preserve the
    /// complete signed word so any still-owned echo write affects the native consumer.
    /// </summary>
    private short CrashAngularDelta
    {
        get => unchecked((short)_firstReleasedCrashEcho.XPosition);
        set => _firstReleasedCrashEcho.XPosition = unchecked((ushort)value);
    }

    /// <summary>Ports `$90:D346-$D3F2`, including the three overloaded-index substates.</summary>
    private ShinesparkMovementResult StepCrashOrbit(
        ISnesAddressSpace bus,
        SamusState samus,
        ShinesparkPhase phaseAtStart)
    {
        ShineTimer = 15;
        switch (CrashSubphase)
        {
            case 0:
            {
                // `$90:D383` tests the old radius against twelve, after calculating +4.
                // The transition frame therefore publishes radius sixteen and subphase one.
                byte oldRadius = CrashRadius;
                CrashRadius = unchecked((byte)(CrashRadius + 4));
                if (oldRadius >= 12)
                    CrashSubphase = 1;
                break;
            }

            case 1:
                FirstCrashEchoAngle = FirstCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                SecondCrashEchoAngle = SecondCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                CrashAngularTravel = unchecked((ushort)(CrashAngularTravel + 4));
                if (unchecked((short)(CrashAngularTravel - 128)) >= 0)
                    CrashSubphase = 2;
                break;

            case 2:
                CrashRadius = unchecked((byte)(CrashRadius - 4));
                if (CrashRadius == 0)
                {
                    Phase = ShinesparkPhase.CrashEchoCircle;
                    StartStopTimer = 30;
                    CrashSubphase = 0;
                    FirstCrashEchoAngle = SnesAngle.Zero;
                    SecondCrashEchoAngle = SnesAngle.Zero;
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Native shinespark crash subphase {CrashSubphase} is outside 0..2.");
        }

        // Projectile_SinLookup consumes a byte angle/radius and returns wrapped 16-bit
        // offsets. Both additions below deliberately retain that unsigned WRAM wrapping.
        (ushort firstOffsetX, ushort firstOffsetY) =
            ProjectileSinLookup(bus, FirstCrashEchoAngle, CrashRadius);
        (ushort secondOffsetX, ushort secondOffsetY) =
            ProjectileSinLookup(bus, SecondCrashEchoAngle, CrashRadius);
        ushort encodedIndex = unchecked((ushort)((CrashSubphase << 8) | CrashRadius));
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            encodedIndex,
            unchecked((ushort)(samus.XPosition + firstOffsetX)),
            unchecked((ushort)(samus.XPosition + secondOffsetX)),
            unchecked((ushort)(samus.YPosition + firstOffsetY)),
            unchecked((ushort)(samus.YPosition + secondOffsetY)));

        return new ShinesparkMovementResult(
            phaseAtStart, null, null, false, false, false, false);
    }

    /// <summary>Ports the 30-frame center echo hold at `$90:D3F3`.</summary>
    private ShinesparkMovementResult StepCrashEchoCircle(ShinesparkPhase phaseAtStart)
    {
        ShineTimer = 15;
        NativeWordCounterStep timer = NativeWordCounter.Decrement(StartStopTimer);
        StartStopTimer = timer.Value;
        if (timer.IsZeroOrNegative)
            Phase = ShinesparkPhase.CrashFinish;
        return new ShinesparkMovementResult(
            phaseAtStart, null, null, false, false, false, false);
    }

    /// <summary>
    /// Ports `$90:D40D-$D4D1`: convert the crash copies into fixed projectile slots three
    /// and four when capacity permits, restore standing, and expire palette handler six.
    /// </summary>
    private ShinesparkMovementResult FinishCrash(
        ISnesAddressSpace bus,
        SamusState samus,
        ShinesparkPhase phaseAtStart,
        ushort projectileCounter,
        SamusProjectileSystem? projectiles)
    {
        // `$90:D40D-$D481` reads the *crash* pose before publishing standing `$01/$02`.
        // The six pose pairs `$C9-$CE` select two literal byte angles. Keeping this table
        // adjacent to the consumer makes the subtraction by `$C9` visible instead of
        // disguising it as a facing-direction approximation.
        ReadOnlySpan<SnesAngle> departureAngles =
        [
            SnesAngle.Zero, SnesAngle.HalfTurn, // $C9: horizontal right
            SnesAngle.Zero, SnesAngle.HalfTurn, // $CA: horizontal left
            SnesAngle.QuarterTurn, SnesAngle.ThreeQuarterTurn, // $CB: vertical right
            SnesAngle.QuarterTurn, SnesAngle.ThreeQuarterTurn, // $CC: vertical left
            SnesAngle.FromTableIndex(0xe0), SnesAngle.FromTableIndex(0x60), // $CD: diagonal right
            SnesAngle.FromTableIndex(0x20), SnesAngle.FromTableIndex(0xa0), // $CE: diagonal left
        ];
        int angleIndex = (samus.Pose - SamusPoseIds.ShinesparkHorizontalRightPose) * 2;
        if ((uint)angleIndex >= (uint)departureAngles.Length)
        {
            throw new InvalidOperationException(
                $"Shinespark crash finish requires pose $C9-$CE, not ${samus.Pose:X2}.");
        }

        samus.HorizontalSpeed.ResetSpeedEchoPositionsForShinespark();

        // Native reserves fixed projectile slots three and four. Its projectile counter
        // admits both while below four, only slot four at exactly four, and neither at five
        // or above. The runtime supplies the ordinary projectile count, not bomb count.
        // Capacity failure does not clear existing echo words or slots. In particular,
        // retaining slot three's Y also retains the next crash's angular-travel seed.
        LastReleasedCrashEchoClear = null;
        if (unchecked((short)(projectileCounter - 5)) < 0)
        {
            if (unchecked((short)(projectileCounter - 4)) < 0)
            {
                _firstReleasedCrashEcho.Initialize(
                    departureAngles[angleIndex],
                    samus.XPosition,
                    samus.YPosition);
                projectiles?.InitializeShinesparkEcho(bus, 3, departureAngles[angleIndex]);
            }

            _secondReleasedCrashEcho.Initialize(
                departureAngles[angleIndex + 1],
                samus.XPosition,
                samus.YPosition);
            projectiles?.InitializeShinesparkEcho(bus, 4, departureAngles[angleIndex + 1]);
        }

        ShineTimer = 1;
        VerticalAccelerationSpeed = 0;
        VerticalAccelerationSubspeed = 0;
        byte standingPose = samus.IsFacingLeft(bus)
            ? SamusPoseIds.FacingLeftNormalPose
            : SamusPoseIds.FacingRightNormalPose;
        samus.Pose = standingPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        Phase = ShinesparkPhase.Inactive;
        return new ShinesparkMovementResult(
            phaseAtStart, null, null, false, false, false, false,
            CrashSequenceFinished: true);
    }

    /// <summary>
    /// Runs <c>ProjPreInstr_SpeedEcho</c> at <c>$90:D4D2</c> for the two fixed crash-echo
    /// projectile slots. Call this during alpha projectile processing, before Samus moves.
    /// </summary>
    public void StepReleasedCrashEchoProjectiles(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        StepReleasedCrashEcho(
            bus, samus, layer1X, layer1Y, nativeSlot: 3, _firstReleasedCrashEcho);
        StepReleasedCrashEcho(
            bus, samus, layer1X, layer1Y, nativeSlot: 4, _secondReleasedCrashEcho);
    }

    /// <summary>
    /// The projectile radius/angle belong to its current slot owner. Drawing enable and
    /// coordinates are separate native words and may survive replacement of that owner.
    /// </summary>
    internal bool StepOwnedReleasedCrashEcho(ISnesAddressSpace bus, SamusState samus,
        ushort layer1X, ushort layer1Y, SamusProjectileSlot projectile)
    {
        ReleasedEchoSlot echo = projectile.SlotIndex switch
        {
            3 => _firstReleasedCrashEcho,
            4 => _secondReleasedCrashEcho,
            _ => throw new InvalidOperationException("Crash echo requires fixed slot three or four.")
        };
        echo.Radius = unchecked((ushort)projectile.XVelocity);
        echo.Angle = SnesAngle.FromTableIndex(unchecked((byte)projectile.Variable));
        if (!StepReleasedCrashEcho(bus, samus, layer1X, layer1Y,
            (byte)projectile.SlotIndex, echo, projectileOwnsSlot: true)) return false;
        projectile.XVelocity = unchecked((short)echo.Radius);
        projectile.XPosition = echo.XPosition;
        projectile.YPosition = echo.YPosition;
        return true;
    }

    private bool StepReleasedCrashEcho(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        byte nativeSlot,
        ReleasedEchoSlot slot,
        bool projectileOwnsSlot = false)
    {
        if (!projectileOwnsSlot && !slot.Active)
            return false;

        // `$90:D4D2` increments the complete word but passes only its low byte as radius.
        // The point is recalculated from the *current* Samus position on every frame; these
        // are expanding radial copies, not ordinary projectiles with independent velocity.
        slot.Radius = unchecked((ushort)(slot.Radius + 8));
        (ushort offsetX, ushort offsetY) =
            ProjectileSinLookup(bus, slot.Angle, unchecked((byte)slot.Radius));
        slot.XPosition = unchecked((ushort)(samus.XPosition + offsetX));

        // Native performs the X viewport rejection before calculating/publishing Y. The
        // cleared result is identical either way, but retaining the branch order makes a
        // debugger trace line up with `$90:D4F2-$D51E` instruction for instruction.
        short screenX = unchecked((short)(slot.XPosition - layer1X));
        if (screenX < 0 || screenX >= 256)
        {
            LastReleasedCrashEchoClear = new(
                nativeSlot, slot.Angle, slot.Radius,
                slot.XPosition, slot.YPosition, screenX, null, ShinesparkEchoClearAxis.X);
            slot.Clear();
            return false;
        }

        slot.YPosition = unchecked((ushort)(samus.YPosition + offsetY));
        short screenY = unchecked((short)(slot.YPosition - layer1Y));
        if (screenY < 0 || screenY >= 256)
        {
            LastReleasedCrashEchoClear = new(
                nativeSlot, slot.Angle, slot.Radius,
                slot.XPosition, slot.YPosition, screenX, screenY, ShinesparkEchoClearAxis.Y);
            slot.Clear();
            return false;
        }
        return true;
    }

    /// <summary>Exact byte-split multiplication used by `$90:CC39/$90:CC8A`.</summary>
    private static (ushort X, ushort Y) ProjectileSinLookup(
        ISnesAddressSpace bus,
        SnesAngle angle,
        byte radius)
    {
        ushort x = LookupSignedComponent(bus, angle, radius);
        ushort y = LookupSignedComponent(
            bus,
            angle.AddRaw(-SnesAngle.QuarterTurn.RawValue),
            radius);
        return (x, y);
    }

    private static ushort LookupSignedComponent(
        ISnesAddressSpace bus,
        SnesAngle angle,
        byte radius)
    {
        bool negative = angle.RawValue >= SnesAngle.HalfTurn.RawValue;
        SnesAngle positiveAngle = negative
            ? angle.AddRaw(SnesAngle.HalfTurn.RawValue)
            : angle;
        // The complete signed sine/cosine table begins at `$A0:B3C3`. Native routine
        // `$90:CC8A` deliberately biases its long pointer by 64 words, so index zero in
        // this positive-half lookup is the table's entry 64 at `$A0:B443`. Multiplying
        // that signed 8.8 entry by the byte radius and shifting produces the same whole-
        // pixel component as the native pair of byte multiplies.
        ushort tableWord = ReadWord(
            bus,
            SamusSpecialSequenceRomData.Shinespark.PositiveSineTable +
                positiveAngle.SineTableByteOffset);
        ushort magnitude = unchecked((ushort)(((uint)tableWord * radius) >> 8));
        return negative ? unchecked((ushort)-magnitude) : magnitude;
    }

    private BlockMoveResult MoveX(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        RoomPlmSystem? plms)
    {
        ShineTimer = 15;
        uint accelerated = unchecked(Compose(
            samus.HorizontalSpeed.ExtraRunSpeed,
            samus.HorizontalSpeed.ExtraRunSubspeed) + Compose(
                samus.Kinematics.YAcceleration,
                samus.Kinematics.YSubacceleration));
        samus.HorizontalSpeed.ExtraRunSpeed = unchecked((ushort)(accelerated >> 16));
        samus.HorizontalSpeed.ExtraRunSubspeed = unchecked((ushort)accelerated);
        if (unchecked((short)(samus.HorizontalSpeed.ExtraRunSpeed - 15)) >= 0)
        {
            samus.HorizontalSpeed.ExtraRunSpeed = 15;
            samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        }

        int displacement = samus.IsFacingLeft(bus)
            ? samus.HorizontalSpeed.CalculateLeftDisplacement(
                baseSpeed: 0,
                samus.Kinematics.ExtraXFixed)
            : samus.HorizontalSpeed.CalculateRightDisplacement(
                baseSpeed: 0,
                samus.Kinematics.ExtraXFixed);
        return SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            displacement,
            canBreakBombBlocks: true,
            plms: plms);
    }

    private BlockMoveResult MoveY(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms)
    {
        ShineTimer = 15;
        uint acceleration = unchecked(Compose(
            VerticalAccelerationSpeed,
            VerticalAccelerationSubspeed) + Compose(
                samus.Kinematics.YAcceleration,
                samus.Kinematics.YSubacceleration));
        VerticalAccelerationSpeed = unchecked((ushort)(acceleration >> 16));
        VerticalAccelerationSubspeed = unchecked((ushort)acceleration);

        uint speed = unchecked(Compose(
            samus.Kinematics.YSpeed,
            samus.Kinematics.YSubspeed) + acceleration);
        samus.Kinematics.YSpeed = unchecked((ushort)(speed >> 16));
        samus.Kinematics.YSubspeed = unchecked((ushort)speed);

        // Samus_ClampSpeedHi preserves the fractional word while replacing only the whole
        // word at or above fourteen. Negation then converts the magnitude to an upward move.
        if (unchecked((short)(samus.Kinematics.YSpeed - 14)) >= 0)
            samus.Kinematics.YSpeed = 14;
        int upwardDisplacement = unchecked(-(int)Compose(
            samus.Kinematics.YSpeed,
            samus.Kinematics.YSubspeed));
        upwardDisplacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            samus.Kinematics,
            upwardDisplacement);

        // `$90:D261-$D29F` temporarily negates the signed result, clamps only a positive
        // upward magnitude at 15 whole pixels while preserving its fraction, then negates
        // it back before the room move. If external Y is strong enough to reverse motion
        // downward, the temporary value is negative and deliberately bypasses this clamp.
        int collisionMagnitude = unchecked(-upwardDisplacement);
        short collisionMagnitudeWhole = unchecked((short)(collisionMagnitude >> 16));
        if (unchecked((short)(collisionMagnitudeWhole - 15)) >= 0)
        {
            collisionMagnitude = unchecked((int)(
                (15u << 16) | (ushort)collisionMagnitude));
            upwardDisplacement = unchecked(-collisionMagnitude);
        }
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            upwardDisplacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            canBreakBombBlocks: true,
            plms: plms);
    }

    private static uint Compose(ushort high, ushort low) => ((uint)high << 16) | low;

    private static void LoadNormalSuitPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort equippedItems)
    {
        ushort palette = ReadWord(
            bus,
            SamusPaletteRomData.Common.NormalSuitPointers +
                equippedItems.GetSuitPaletteTableOffset());
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | palette,
            colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
            destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    /// <summary>
    /// Mutable storage corresponding to one of native fixed projectile slots three/four.
    /// It is deliberately private so callers cannot manufacture a live visual projectile.
    /// </summary>
    private sealed class ReleasedEchoSlot
    {
        public bool Active { get; private set; }
        public SnesAngle Angle { get; set; }
        public ushort Radius { get; set; }
        public ushort XPosition { get; set; }
        public ushort YPosition { get; set; }

        public ShinesparkReleasedEcho Snapshot => new(
            Active, Angle, Radius, XPosition, YPosition);

        // Crash entry changes only the drawing enable. It does not clear coordinates,
        // projectile ownership, or the aliased angular-travel word.
        public void DisableDrawing() => Active = false;

        public void Initialize(SnesAngle angle, ushort xPosition, ushort yPosition)
        {
            Active = true;
            Angle = angle;
            // Crash finish clears projectile X velocity (the departing radius). The
            // separate speed-echo drawing field is 64; it is not this projectile word.
            Radius = 0;
            XPosition = xPosition;
            YPosition = yPosition;
        }

        public void Clear()
        {
            Active = false;
            Angle = SnesAngle.Zero;
            Radius = 0;
            XPosition = 0;
            YPosition = 0;
        }
    }
}

/// <summary>Host names for the installed shinespark movement-handler pointers.</summary>
public enum ShinesparkPhase
{
    Inactive,
    Stored,
    Windup,
    Horizontal,
    Vertical,
    Diagonal,
    Crash,
    CrashEchoCircle,
    CrashFinish,
}

/// <summary>Debugger snapshot of one windup or active shinespark handler call.</summary>
public readonly record struct ShinesparkMovementResult(
    ShinesparkPhase PhaseAtStart,
    BlockMoveResult? Horizontal,
    BlockMoveResult? Vertical,
    bool WindupTimedOut,
    bool EndedByCollision,
    bool EndedByLowEnergy,
    bool EnergyDrained,
    bool CrashSequenceFinished = false);

/// <summary>Immutable debugger view of one departing crash-echo projectile.</summary>
public readonly record struct ShinesparkReleasedEcho(
    bool Active,
    SnesAngle Angle,
    ushort Radius,
    ushort XPosition,
    ushort YPosition);

/// <summary>Debugger-only witness captured immediately before an off-camera echo clears.</summary>
public readonly record struct ShinesparkReleasedEchoClear(
    byte NativeSlot,
    SnesAngle Angle,
    ushort Radius,
    ushort XPosition,
    ushort YPosition,
    short ScreenX,
    short? ScreenY,
    ShinesparkEchoClearAxis Axis);

/// <summary>Which ordered viewport comparison rejected a departing echo.</summary>
public enum ShinesparkEchoClearAxis
{
    X,
    Y,
}
