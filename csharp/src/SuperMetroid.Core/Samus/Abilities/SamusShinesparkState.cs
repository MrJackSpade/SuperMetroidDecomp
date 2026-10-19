using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Abilities;

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
    /// <summary>Live Power Bomb state consulted when a spark sound request is produced.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? _audioPowerBomb;
    /// <summary>Live charge-flare owner canceled when spark windup begins.</summary>
    [NonSerialized] private SamusProjectileSystem? _projectiles;
    /// <summary>One-frame library-one sound request from canceling a sufficiently charged flare.</summary>
    [NonSerialized] private bool _chargeCancellationSoundRequested;

    /// <summary>
    /// Rebinds the live projectile and sound owners before movement/palette production,
    /// including restored states. These references are routing, not cartridge state.
    /// </summary>
    internal void BindProjectileOwners(
        SamusProjectileSystem projectiles,
        SamusPowerBombExplosionState powerBomb)
    {
        _projectiles = projectiles;
        _audioPowerBomb = powerBomb;
    }

    /// <summary>Native queue guard captured at the stored-shine warning call, not at publication.</summary>
    public bool StoredShineWarningSoundSuppressed { get; private set; }
    /// <summary>Native queue guard captured at directional launch.</summary>
    public bool LaunchSoundSuppressed { get; private set; }
    /// <summary>Native queue guard shared by the two adjacent crash-entry sound calls.</summary>
    public bool CrashSoundSuppressed { get; private set; }
    /// <summary>Special-palette handler (WRAM <c>$0ACC</c>) this owner installs: none, stored shine, or shinesparking.</summary>
    public SamusSpecialPaletteType PaletteType { get; private set; }

    /// <summary>WRAM <c>$0A6A</c>: 180 stored, 60 windup, and 15 during active motion.</summary>
    public NativeCountdown ShineTimer { get; private set; }

    /// <summary>
    /// Byte offset of the next shine palette phase, an even value below twelve for the stored shine and below eight
    /// while sparking: each palette call advances it by two and returns it to zero at that limit.
    /// </summary>
    public int PaletteFrameOffset { get; private set; }

    /// <summary>WRAM <c>$0A64</c>, the shared 30-frame windup/crash countdown.</summary>
    public NativeCountdown StartStopTimer { get; internal set; }

    /// <summary>
    /// The upward spark's 16.16 acceleration in pixels per frame per frame: 7.0 when windup begins, raised by
    /// Samus's vertical acceleration on every vertical or diagonal spark call, and cleared when the crash finishes.
    /// Its storage is shared with the suit pickup's scratch, which can replace it (see
    /// <see cref="ApplySuitPickupScratch"/>); the adds wrap at 32 bits exactly as the cartridge's word pair does.
    /// </summary>
    public SnesSignedSixteenSixteen VerticalAcceleration { get; private set; }

    /// <summary>
    /// Mirrors suit HDMA writes to shared WRAM $0DEC/$0DEE (substate/light-beam
    /// position), which $90:D1FF later reads as the spark acceleration pair.
    /// A suspended windup must not retain its pre-pickup 7.0000 initializer.
    /// </summary>
    /// <param name="substate">The suit pickup's stage number, which becomes the acceleration's whole word.</param>
    /// <param name="lightBeamPosition">The suit pickup's light-beam word, which becomes the acceleration's fraction.</param>
    internal void ApplySuitPickupScratch(ushort substate, ushort lightBeamPosition) =>
        VerticalAcceleration = SnesSignedSixteenSixteen.FromParts(unchecked((short)substate), lightBeamPosition);

    /// <summary>Debugger-readable replacement for the installed bank-$90 handler pointer.</summary>
    public ShinesparkPhase Phase { get; internal set; }

    /// <summary>
    /// Relinquishes the movement-handler pointer when bank $90 installs normal movement.
    /// It does not clear the independently owned palette timer, boost counter, or echoes;
    /// Draygon's unconditional release at $90:E2DE relies on that distinction.
    /// </summary>
    internal void RelinquishMovementHandler() => Phase = ShinesparkPhase.Inactive;

    /// <summary>
    /// Whether the spark's routine is installed in the single movement-handler word: $90:CFFA
    /// installs windup, and launch and crash replace it in turn until the crash finishes.
    /// </summary>
    public bool OwnsMovementHandler => Phase is
        ShinesparkPhase.Windup or ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or
        ShinesparkPhase.Diagonal or ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle or
        ShinesparkPhase.CrashFinish;

    /// <summary>
    /// CrystalFlash ($90:D5A2) replaces both the movement pointer and shared special
    /// palette words. The new owner holds those words; do not resume this old owner
    /// after Flash finishes. Extra run speed and the boost counter are not cleared.
    /// </summary>
    internal void RelinquishToCrystalFlash()
    {
        Phase = ShinesparkPhase.Inactive;
        ShineTimer = NativeCountdown.Start(0);
        PaletteType = SamusSpecialPaletteType.None;
        PaletteFrameOffset = 0;
    }

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

    /// <summary>
    /// Consumes <c>Projectile_Func7_Shinespark</c>'s library-one <c>$02</c> request,
    /// emitted only when windup cancels a charge whose counter reached sixteen.
    /// </summary>
    public bool ConsumeChargeCancellationSoundRequest()
    {
        bool requested = _chargeCancellationSoundRequested;
        _chargeCancellationSoundRequested = false;
        return requested;
    }

    /// <summary>Producer-time Power Bomb guard for the pending charge-cancellation call.</summary>
    [field: NonSerialized]
    public bool ChargeCancellationSoundSuppressed { get; private set; }

    /// <summary>
    /// The stage of the crash echoes' orbit: expanding at crash entry, orbiting once the radius reaches sixteen,
    /// and contracting once the angular travel reaches 128.
    /// </summary>
    public ShinesparkCrashOrbitStage CrashOrbitStage { get; private set; }

    /// <summary>
    /// Distance of the two crash echoes from Samus in pixels, 0 through 16: zero at crash entry, raised by four per
    /// expanding call until it reaches sixteen and lowered by four per contracting call until it reaches zero.
    /// </summary>
    public int CrashRadius { get; private set; }

    /// <summary>First byte-angle formerly stored in speed-echo X speed slot zero.</summary>
    public SnesAngle FirstCrashEchoAngle { get; private set; }

    /// <summary>Second byte-angle, exactly 128 degrees-of-256 opposite the first.</summary>
    public SnesAngle SecondCrashEchoAngle { get; private set; }

    /// <summary>
    /// The crash echoes' angular travel, 0 through <see cref="LargestCrashAngularTravel"/>. Its storage is the
    /// released slot-three echo's Y coordinate: crash entry does not reset it, so a preceding echo or crash leaves a
    /// value the orbit starts from. It advances only through <see cref="AdvanceCrashAngularTravel"/>, which wraps
    /// past the largest value to zero, and the orbit ends on the sign of its distance from 128.
    /// </summary>
    public int CrashAngularTravel
    {
        get => _firstReleasedCrashEcho.Position.Y;
        private set => _firstReleasedCrashEcho.Position = _firstReleasedCrashEcho.Position.WithY(checked((ushort)value));
    }

    /// <summary>The largest angular travel the shared echo coordinate holds; one more step wraps past it to zero.</summary>
    internal const int LargestCrashAngularTravel = ushort.MaxValue;

    /// <summary>Angular travel added on each orbiting call.</summary>
    private const int CrashAngularTravelStep = 4;

    /// <summary>
    /// Adds one orbiting call's travel, wrapping past <see cref="LargestCrashAngularTravel"/> to zero as the shared
    /// echo coordinate does. A seeded travel near the largest value therefore restarts from zero and orbits longer.
    /// </summary>
    private void AdvanceCrashAngularTravel()
    {
        int advanced = CrashAngularTravel + CrashAngularTravelStep;
        CrashAngularTravel = advanced > LargestCrashAngularTravel ? advanced - (LargestCrashAngularTravel + 1) : advanced;
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

    /// <summary>Fixed slot-three echo storage whose position aliases crash angular words.</summary>
    internal readonly ReleasedEchoSlot _firstReleasedCrashEcho = new();
    /// <summary>Fixed slot-four echo storage drawn before the slot-three copy.</summary>
    internal readonly ReleasedEchoSlot _secondReleasedCrashEcho = new();

    /// <summary>
    /// Samus_HandleTransitionsB_5 ($91:EEA6) clears $0A6A and replaces the shared
    /// special-palette handler with X-Ray. Relinquish this owner's palette state so
    /// it cannot keep ticking or repaint over the newly installed visor palette.
    /// </summary>
    internal void RelinquishPaletteToXray()
    {
        ShineTimer = NativeCountdown.Start(0);
        PaletteType = SamusSpecialPaletteType.None;
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
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
    }

    /// <summary>
    /// Ports <c>Samus_CrouchTrans</c> at <c>$91:F7B0</c>. The signed comparison accepts
    /// stage four and any larger high byte exactly as the 65816 does.
    /// </summary>
    public bool TryStoreFromSpeedBooster(ushort speedBoostCounter)
    {
        if (unchecked((short)((SnesLittleEndian.HighByteInPlace(speedBoostCounter)) -
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)) < 0)
            return false;

        ShineTimer = NativeCountdown.Start(180);
        PaletteType = SamusSpecialPaletteType.SpeedBoosterShine;
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
        Ensure.NotNull(samus);
        if (samus.InputSnapshot.SharedShineTimer == 0)
            throw new InvalidOperationException("Shinespark windup requires a live stored shine.");

        InitializeWindup(samus);
    }

    /// <summary>DemoSetFunc_5/6 call the native windup initializer without a stored shine.</summary>
    internal void BeginDemoLaunch(SamusState samus, SamusPoseId targetPose)
    {
        InitializeWindup(samus);
        BeginDirectionalLaunch(samus, targetPose);
    }

    /// <summary>Cancels beam charge and installs the native 30-frame spark windup, palette and boost state.</summary>
    private void InitializeWindup(SamusState samus)
    {
        // Projectile_Func7_Shinespark checks $0CD0 before clearing it. A charge that has
        // reached the draw-time sound threshold first queues library-one $02; shorter
        // charges are still torn down without a sound call. The live projectile owner
        // contains the flare layers and previous-counter word, while Samus mirrors $0CD0
        // for movement decisions. Keep both views synchronized at this native seam.
        int cancelledCharge = samus.ProjectileFlareCounter;
        if (cancelledCharge != 0)
        {
            _projectiles?.Firing.CancelChargeForShinespark();
            samus.ProjectileFlareCounter = 0;
            if (cancelledCharge >= SamusProjectileRomData.Beams.ChargeSoundStartCounter)
            {
                _chargeCancellationSoundRequested = true;
                ChargeCancellationSoundSuppressed = _audioPowerBomb?.IsActive == true;
            }
        }

        // $90:CFFA replaces the movement and palette pointers. Flash's live
        // $0A68 can admit this launch even after its movement was interrupted.
        samus.CrystalFlash.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishPaletteHandler();
        // A Reserve-Mode X-Ray HDMA object survives, but directional spark setup replaces
        // the same shared special-palette dispatcher with handler six.
        samus.Xray.RelinquishPaletteHandler();

        // These are literal native writes. Stage four remains published during windup even
        // though base speed is zero and the extra component becomes exactly 8.0000.
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        samus.Kinematics.YSpeed = default;
        samus.KnockbackDirection = Movement.SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        samus.HorizontalSpeed.SpeedBoostCounter =
            SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter;
        samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(8, 0);
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.BombJumpDirection = 0;

        VerticalAcceleration = SnesSignedSixteenSixteen.FromParts(7, 0);
        StartStopTimer = NativeCountdown.Start(30);
        ShineTimer = NativeCountdown.Start(60);
        PaletteType = SamusSpecialPaletteType.Shinesparking;
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
    public void BeginDirectionalLaunch(SamusState samus, SamusPoseId targetPose,
        bool deferPoseChange = false)
    {
        Ensure.NotNull(samus);
        // `$91:FACA` installs the launch handler over whatever handler is current. A windup
        // whose handler normal movement replaced (a knockback ending in `$C7/$C8`) keeps its
        // pose and the windup's speeds, and a direction still launches from it.
        Phase = targetPose switch
        {
            SamusPoseId.ShinesparkHorizontalRightPose or
            SamusPoseId.ShinesparkHorizontalLeftPose => ShinesparkPhase.Horizontal,
            SamusPoseId.ShinesparkVerticalRightPose or
            SamusPoseId.ShinesparkVerticalLeftPose => ShinesparkPhase.Vertical,
            SamusPoseId.ShinesparkDiagonalRightPose or
            SamusPoseId.ShinesparkDiagonalLeftPose => ShinesparkPhase.Diagonal,
            // `$91:F80F` is reached only from the six table-selected `$C9-$CE` records.
            // Any other byte is a caller contract violation, not another untranslated arm.
            _ => throw new ArgumentOutOfRangeException(
                nameof(targetPose),
                $"Pose ${(int)targetPose:X2} is not one of the six directional shinespark poses."),
        };

        samus.InputSnapshot.ShinesparkPoseInputLocked = true;
        samus.InputSnapshot.AutoJumpInputPending = false;
        if (!deferPoseChange)
            ApplyDirectionalLaunchPose(samus, targetPose);
        samus.HorizontalSpeed.ResetSpeedEchoPositionsForShinespark();
        LaunchSoundRequested = true;
        LaunchSoundSuppressed = _audioPowerBomb?.IsActive == true;
    }

    /// <summary>Commits the interrupted launch pose after the current frame's animation.</summary>
    internal static void ApplyDirectionalLaunchPose(SamusState samus, SamusPoseId pose)
    {
        samus.Pose = pose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>Runs one installed special-handler frame from <c>$90:D068-$D2B9</c>.</summary>
    /// <param name="terrain">The room terrain Samus's block collision probes: the address space, the level data, and the PLMs that react to touched blocks (#633).</param>
    /// <param name="samus">Live player state; movement, energy, contact damage, pose state, and echo coordinates may change during the call.</param>
    /// <param name="frame">What one shinespark step reads from the frame: clocks, projectiles, invincibility, timeout policy and the previous checkpoint (#633).</param>
    public ShinesparkMovementResult Step(SamusTerrain terrain, SamusState samus, ShinesparkFrame frame)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);

        ShinesparkPhase phaseAtStart = Phase;
        if (Phase == ShinesparkPhase.Windup)
        {
            // DEC/BEQ/BMI at `$90:D068`: exact zero and signed underflow both launch up.
            NativeWordCounterStep timer = StartStopTimer.Decrement();
            StartStopTimer = timer.Remaining;
            bool timedOut = timer.IsZeroOrNegative;
            SamusPoseId? pendingLaunchPose = null;
            if (timedOut)
            {
                SamusPoseId verticalPose = samus.PoseMetadata.IsFacingLeft()
                    ? SamusPoseId.ShinesparkVerticalLeftPose
                    : SamusPoseId.ShinesparkVerticalRightPose;
                BeginDirectionalLaunch(samus, verticalPose, frame.DeferTimeoutPoseChange);
                if (frame.DeferTimeoutPoseChange)
                    pendingLaunchPose = verticalPose;
            }

            return new ShinesparkMovementResult(
timedOut,                 PendingLaunchPose: pendingLaunchPose);
        }

        if (Phase is not (ShinesparkPhase.Horizontal or
            ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal))
        {
            if (Phase == ShinesparkPhase.Crash)
                return StepCrashOrbit(samus);
            if (Phase == ShinesparkPhase.CrashEchoCircle)
                return StepCrashEchoCircle();
            if (Phase == ShinesparkPhase.CrashFinish)
                return FinishCrash(samus, frame.Projectiles?.SlotTable.ProjectileCounter ?? frame.ProjectileCounter, frame.Projectiles);


            // Stored/inactive states are not installed special movement handlers. Returning
            // an empty snapshot is defensive; runtime normally never dispatches them here.
            return new ShinesparkMovementResult(
false);
        }

        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.Shinespark;
        samus.HurtFlashCounter = 8;

        // Each active handler samples echoes before motion. Runtime suppresses its ordinary
        // post-motion producer while this special handler owns the frame.
        samus.HorizontalSpeed.CaptureSpeedEchoPosition(checked((ushort)frame.GameTimeFrames), samus.Position.Whole);

        BlockMoveResult? horizontal = null;
        BlockMoveResult? vertical = null;
        // Each axis ends by capping how far the camera's previous-position word trails
        // Samus. Only a camera reads that word; fixtures without one pass no checkpoint.
        if (Phase is ShinesparkPhase.Horizontal or ShinesparkPhase.Diagonal)
        {
            horizontal = MoveX(terrain.Level, samus, terrain.Plms);
            if (frame.PreviousCheckpoint is { } checkpoint)
                CapPreviousX(samus, checkpoint);
        }
        if (Phase is ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal)
        {
            vertical = MoveY(terrain.Level, samus, frame.NmiFrameCounter, terrain.Plms);
            if (frame.PreviousCheckpoint is { } checkpoint)
                CapPreviousY(samus, checkpoint);
        }

        // Native X and Y movement share one collision flag. Diagonal movement runs Y
        // last, so a clear vertical probe replaces an earlier wall hit and allows the
        // spark to slide upward. Combining both results would terminate a retail spark
        // early; use the last executed axis, including when its result is clear.
        bool collided = vertical?.Collided ?? horizontal?.Collided ?? false;
        // The host cheat bypasses only the energy exit. Terrain/enemy collision still
        // terminates all three launch directions through the ordinary crash handler.
        bool hasNativeEnergy = samus.Health >= SamusSpecialSequenceRomData.Shinespark.MinimumSustainingEnergy;
        bool lowEnergy = !frame.PlayerInvincibilityEnabled && !hasNativeEnergy;
        if (collided || lowEnergy)
            BeginCrash(samus);

        // At exactly 30 energy, this frame still moves and drains to 29. The following
        // frame moves once more before EndSuperJump observes that 29 is below the threshold.
        // Invincible playtesting retains energy loss without requiring refills: below
        // the native cutoff, continue draining to one, never decrementing zero or one.
        bool drained = frame.PlayerInvincibilityEnabled
            ? samus.Health > 1
            : hasNativeEnergy;
        if (drained)
            samus.Health = SamusQuantity.DecrementPositive(samus.Health);

        return new ShinesparkMovementResult(
false);
    }

    /// <summary>
    /// Runs palette handler one or six from <c>$91:D6F7</c>, including normal restoration
    /// when its signed countdown expires.
    /// </summary>
    public bool UpdatePalette(SnesCgram cgram, SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? suitColors = null,
        SamusFullBodyCycleColorCatalog? cycleColors = null)
    {
        Ensure.NotNull(cgram);
        if (PaletteType is not (SamusSpecialPaletteType.SpeedBoosterShine or SamusSpecialPaletteType.Shinesparking))
            return false;

        if (PaletteType == SamusSpecialPaletteType.SpeedBoosterShine && ShineTimer.RawValue == 170)
        {
            StoredShineWarningSoundRequested = true;
            StoredShineWarningSoundSuppressed = _audioPowerBomb?.IsActive == true;
        }

        NativeWordCounterStep timer = ShineTimer.Decrement();
        ShineTimer = timer.Remaining;
        bool expired = timer.IsZeroOrNegative;
        if (expired)
        {
            PaletteType = SamusSpecialPaletteType.None;
            PaletteFrameOffset = 0;
            if (Phase == ShinesparkPhase.Stored)
                Phase = ShinesparkPhase.Inactive;
            SamusNormalSuitPalette.Load(cgram, equippedItems, suitColors);
            return true;
        }

        ushort suitOffset = equippedItems.GetSuitPaletteTableOffset();
        ushort phaseOffset = checked((ushort)PaletteFrameOffset);
        bool catalogued = PaletteType == SamusSpecialPaletteType.SpeedBoosterShine
            ? SamusPaletteRomData.FullBodyCycles.TryStoredShinePalettePointer(
                suitOffset, phaseOffset, out ushort palettePointer)
            : SamusPaletteRomData.FullBodyCycles.TryActiveShinesparkPalettePointer(
                suitOffset, phaseOffset, out palettePointer);
        if (!catalogued)
            throw new InvalidDataException(
                $"Shinespark palette type {(int)PaletteType} offset {PaletteFrameOffset} is not compiled.");
        (cycleColors ?? throw new InvalidOperationException(
            "Shinespark palette requires installed color assets.")).Apply(cgram, palettePointer);

        int exclusiveLimit = PaletteType == SamusSpecialPaletteType.SpeedBoosterShine ? 12 : 8;
        // The offset is always below its limit before this advance, so it never approaches a wrap.
        PaletteFrameOffset += 2;
        if (PaletteFrameOffset >= exclusiveLimit)
            PaletteFrameOffset = 0;
        return true;
    }

    /// <summary>Stops translational spark motion, seeds paired orbit angles and installs the crash handler without clearing aliased angular travel.</summary>
    private void BeginCrash(SamusState samus)
    {
        // `$90:D2BA-$D345` kills translational velocity before installing the crash handler.
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.SpeedBoostCounter = 0;
        // EndSuperJump retains this frame's contact damage. The shared frame
        // prologue clears it before the newly installed crash handler runs next.
        samus.HurtFlashCounter = 0;
        bool facingLeft = samus.PoseMetadata.IsFacingLeft();
        FirstCrashEchoAngle = SnesAngle.FromTableIndex(facingLeft ? (byte)32 : (byte)224);
        SecondCrashEchoAngle = SnesAngle.FromTableIndex(facingLeft ? (byte)160 : (byte)96);
        CrashAngularDelta = facingLeft ? (short)4 : (short)-4;
        _firstReleasedCrashEcho.DisableDrawing();
        CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
        CrashRadius = 0;
        // The native entry does not initialize the aliased angular-travel/echo-Y
        // word. A preceding echo or crash can therefore shorten this orbit.
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            encodedIndex: 0,
            samus.Position.Whole,
            samus.Position.Whole);
        Phase = ShinesparkPhase.Crash;
        CrashSoundRequested = true;
        CrashSoundSuppressed = _audioPowerBomb?.IsActive == true;
    }

    /// <summary>
    /// WRAM $0AB4: crash angular delta aliases departing slot-three echo X. Preserve the
    /// complete signed word so any still-owned echo write affects the native consumer.
    /// </summary>
    private short CrashAngularDelta
    {
        get => unchecked((short)_firstReleasedCrashEcho.Position.X);
        set => _firstReleasedCrashEcho.Position = _firstReleasedCrashEcho.Position.WithX(unchecked((ushort)value));
    }

    /// <summary>Ports `$90:D346-$D3F2`, including the three overloaded-index substates.</summary>
    private ShinesparkMovementResult StepCrashOrbit(
        SamusState samus)
    {
        ShineTimer = NativeCountdown.Start(15);
        switch (CrashOrbitStage)
        {
            case ShinesparkCrashOrbitStage.Expanding:
            {
                // `$90:D383` tests the old radius against twelve, after calculating +4.
                // The transition frame therefore publishes radius sixteen and the orbiting stage.
                int oldRadius = CrashRadius;
                CrashRadius += 4;
                if (oldRadius >= 12)
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Orbiting;
                break;
            }

            case ShinesparkCrashOrbitStage.Orbiting:
                FirstCrashEchoAngle = FirstCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                SecondCrashEchoAngle = SecondCrashEchoAngle.AddTableUnits(CrashAngularDelta);
                AdvanceCrashAngularTravel();
                // The sign of the sixteen-bit distance from 128 decides, so a travel that wrapped to
                // zero, or one seeded far above 128, keeps orbiting until it climbs back to 128.
                if (unchecked((short)(CrashAngularTravel - 128)) >= 0)
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Contracting;
                break;

            case ShinesparkCrashOrbitStage.Contracting:
                CrashRadius -= 4;
                if (CrashRadius == 0)
                {
                    Phase = ShinesparkPhase.CrashEchoCircle;
                    StartStopTimer = NativeCountdown.Start(30);
                    CrashOrbitStage = ShinesparkCrashOrbitStage.Expanding;
                    FirstCrashEchoAngle = SnesAngle.Zero;
                    SecondCrashEchoAngle = SnesAngle.Zero;
                }
                break;

            default:
                throw new InvalidOperationException(
                    $"Shinespark crash orbit stage {CrashOrbitStage} is undefined.");
        }

        // Projectile_SinLookup consumes a byte angle/radius and returns wrapped 16-bit
        // offsets. Both additions below deliberately retain that unsigned WRAM wrapping.
        byte orbitRadius = checked((byte)CrashRadius);
        Offset16<RoomPixels> firstOffset = ProjectileSinLookup(FirstCrashEchoAngle, orbitRadius);
        Offset16<RoomPixels> secondOffset = ProjectileSinLookup(SecondCrashEchoAngle, orbitRadius);
        samus.HorizontalSpeed.SetShinesparkCrashEchoState(
            CrashEchoIndexWord(),
            samus.Position.Whole + firstOffset,
            samus.Position.Whole + secondOffset);

        return new ShinesparkMovementResult(
false);
    }

    /// <summary>
    /// The speed-echo index word the crash shares with the speed echoes: the orbit radius in its low byte and the
    /// orbit stage number, 0 through 2, in its high byte. The echo drawer and the next boost read that word.
    /// </summary>
    private ushort CrashEchoIndexWord()
    {
        byte stageNumber = CrashOrbitStage switch
        {
            ShinesparkCrashOrbitStage.Expanding => (byte)0,
            ShinesparkCrashOrbitStage.Orbiting => (byte)1,
            ShinesparkCrashOrbitStage.Contracting => (byte)2,
            _ => throw new InvalidOperationException($"Shinespark crash orbit stage {CrashOrbitStage} is undefined."),
        };
        return SnesLittleEndian.Word(checked((byte)CrashRadius), stageNumber);
    }

    /// <summary>Ports the 30-frame center echo hold at `$90:D3F3`.</summary>
    private ShinesparkMovementResult StepCrashEchoCircle()
    {
        ShineTimer = NativeCountdown.Start(15);
        NativeWordCounterStep timer = StartStopTimer.Decrement();
        StartStopTimer = timer.Remaining;
        if (timer.IsZeroOrNegative)
            Phase = ShinesparkPhase.CrashFinish;
        return new ShinesparkMovementResult(
false);
    }

    /// <summary>
    /// Ports `$90:D40D-$D4D1`: convert the crash copies into fixed projectile slots three
    /// and four when capacity permits, restore standing, and expire palette handler six.
    /// </summary>
    internal ShinesparkMovementResult FinishCrash(
        SamusState samus,
        int projectileCounter,
        SamusProjectileSystem? projectiles)
    {
        // Resolve the crash axis before publishing the standing pose.
        var departureAngles = SamusShinesparkProjectileRomData.DepartureAngles(samus.Pose);
        samus.HorizontalSpeed.ResetSpeedEchoPositionsForShinespark();

        // Native reserves fixed projectile slots three and four. Its projectile counter
        // admits both while below four, only slot four at exactly four, and neither at five
        // or above. The runtime supplies the ordinary projectile count, not bomb count.
        // Capacity failure does not clear existing echo words or slots. In particular,
        // retaining slot three's Y also retains the next crash's angular-travel seed.
        if (unchecked((short)(projectileCounter - 5)) < 0)
        {
            if (unchecked((short)(projectileCounter - 4)) < 0)
            {
                _firstReleasedCrashEcho.Initialize(departureAngles.First, samus.Position.Whole);
                projectiles?.Shinespark.InitializeShinesparkEcho(3, departureAngles.First);
            }

            _secondReleasedCrashEcho.Initialize(departureAngles.Second, samus.Position.Whole);
            projectiles?.Shinespark.InitializeShinesparkEcho(4, departureAngles.Second);
        }

        ShineTimer = NativeCountdown.Start(1);
        VerticalAcceleration = SnesSignedSixteenSixteen.Zero;
        Phase = ShinesparkPhase.Inactive;
        return new ShinesparkMovementResult(
false,             CrashSequenceFinished: true);
    }

    /// <summary>Commits the crash's transitional standing pose after this frame's animation.</summary>
    public static void ApplyCrashFinishPose(SamusState samus)
    {
        // Transitional command two restores the normal input handler as well as beta. That
        // replaces a drained pose's movement word too: a Samus drained mid-crash is free.
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.Drained.RelinquishMovementHandler();
        SamusPoseId standingPose = samus.PoseMetadata.IsFacingLeft()
            ? SamusPoseId.FacingLeftNormalPose
            : SamusPoseId.FacingRightNormalPose;
        ushort previousRadius = samus.Kinematics.Radius.Y;
        samus.Pose = standingPose;
        // The native transitional-pose handler restores normal movement and
        // aligns the standing body's bottom with the old crash body. Alpha alone
        // publishes the new live radius on the following frame.
        samus.AlignBottomAfterPoseChange(previousRadius, SamusPoseMetadata.ReadPoseYRadius(standingPose));
        samus.Presentation.InitializeAnimation(initialFrame: 0);
    }

    /// <summary>
    /// The projectile radius/angle belong to its current slot owner. Drawing enable and
    /// coordinates are separate native words and may survive replacement of that owner.
    /// </summary>
    internal bool StepOwnedReleasedCrashEcho(SamusState samus,
        Position16<RoomPixels> layer1, SamusProjectileSlot projectile)
    {
        ReleasedEchoSlot echo = projectile.SlotIndex switch
        {
            3 => _firstReleasedCrashEcho,
            4 => _secondReleasedCrashEcho,
            _ => throw new InvalidOperationException("Crash echo requires fixed slot three or four.")
        };
        echo.Radius = unchecked((ushort)projectile.Velocity.X.SignedRawValue);
        echo.Angle = SnesAngle.FromTableIndex(unchecked((byte)projectile.Variable));
        if (!StepReleasedCrashEcho(samus, layer1,
            echo, projectileOwnsSlot: true)) return false;
        projectile.Velocity = projectile.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)echo.Radius)));
        projectile.Position = projectile.Position.WithWhole(echo.Position);
        return true;
    }

    /// <summary>Expands an echo around live Samus position and clears its drawing words when either viewport axis rejects it.</summary>
    internal static bool StepReleasedCrashEcho(
        SamusState samus,
        Position16<RoomPixels> layer1,
        ReleasedEchoSlot slot,
        bool projectileOwnsSlot = false)
    {
        if (!projectileOwnsSlot && !slot.Active)
            return false;

        // `$90:D4D2` increments the complete word but passes only its low byte as radius.
        // The point is recalculated from the *current* Samus position on every frame; these
        // are expanding radial copies, not ordinary projectiles with independent velocity.
        slot.ExpandRadius();
        Offset16<RoomPixels> offset = ProjectileSinLookup(slot.Angle, unchecked((byte)slot.Radius));
        Position16<RoomPixels> point = samus.Position.Whole + offset;
        slot.Position = slot.Position.WithX(point.X);

        // Native performs the X viewport rejection before calculating/publishing Y. The
        // cleared result is identical either way, but retaining the branch order makes a
        // debugger trace line up with `$90:D4F2-$D51E` instruction for instruction.
        short screenX = unchecked((short)(slot.Position.X - layer1.X));
        if (screenX is < 0 or >= 256)
        {
            slot.Clear();
            return false;
        }

        slot.Position = slot.Position.WithY(point.Y);
        short screenY = unchecked((short)(slot.Position.Y - layer1.Y));
        if (screenY is < 0 or >= 256)
        {
            slot.Clear();
            return false;
        }
        return true;
    }

    /// <summary>Exact byte-split multiplication used by `$90:CC39/$90:CC8A`.</summary>
    internal static Offset16<RoomPixels> ProjectileSinLookup(
        SnesAngle angle,
        byte radius)
    {
        ushort x = LookupSignedComponent(angle, radius);
        ushort y = LookupSignedComponent(
            angle.AddRaw(-SnesAngle.QuarterTurn.RawValue),
            radius);
        return new(unchecked((short)x), unchecked((short)y));
    }

    /// <summary>Multiplies a positive sine half-wave and restores its sign after truncation, matching the projectile table routine.</summary>
    private static ushort LookupSignedComponent(
        SnesAngle angle,
        byte radius)
    {
        // Native multiplies the positive half-wave, discards the fraction, then
        // restores sign. Shifting a negative product would round differently.
        return EnemyTrigonometryTables.MultiplySignedSine(radius, angle.TableIndex);
    }

    /// <summary>
    /// Ports <c>$90:D1D6-$D1FB</c>: after horizontal spark movement, a previous X more than
    /// 15 pixels behind Samus is pulled to 15 behind, in either direction.
    /// </summary>
    private static void CapPreviousX(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        short delta = unchecked((short)(samus.Position.X.Whole - samus.PeekPreviousPositionWords(checkpoint).X));
        if (delta >= 0)
        {
            if (delta >= 16)
                samus.WritePreviousXPosition(unchecked((ushort)(samus.Position.X.Whole - 15)));
        }
        else if (delta < -15)
        {
            samus.WritePreviousXPosition(unchecked((ushort)(samus.Position.X.Whole + 15)));
        }
    }

    /// <summary>
    /// Ports <c>$90:D2A3-$D2B6</c>: after vertical spark movement, a previous Y more than 14
    /// pixels below Samus is pulled to 14 below.
    /// </summary>
    private static void CapPreviousY(SamusState samus, FixedPosition<RoomPixels> checkpoint)
    {
        ushort delta = unchecked((ushort)(samus.Position.Y.Whole - samus.PeekPreviousPositionWords(checkpoint).Y));
        // CMP #$FFF2 / BPL: the sign of delta - $FFF2, which is delta + 14 in sixteen bits.
        if (unchecked((short)(delta + 14)) < 0)
            samus.WritePreviousYPosition(unchecked((ushort)(samus.Position.Y.Whole + 14)));
    }

    /// <summary>Accelerates horizontal spark speed up to fifteen pixels per frame and probes breakable room blocks.</summary>
    private BlockMoveResult MoveX(

        RoomLevelData level,
        SamusState samus,
        RoomPlmSystem? plms)
    {
        ShineTimer = NativeCountdown.Start(15);
        uint accelerated = unchecked(SnesSixteenSixteen.Compose(
            samus.HorizontalSpeed.ExtraRunSpeed.Whole,
            samus.HorizontalSpeed.ExtraRunSpeed.Fraction) + SnesSixteenSixteen.Compose(
                samus.Kinematics.YAcceleration.Whole,
                samus.Kinematics.YAcceleration.Fraction));
        samus.HorizontalSpeed.ExtraRunSpeed = SnesUnsignedSixteenSixteen.FromRaw(accelerated);
        if (unchecked((short)(samus.HorizontalSpeed.ExtraRunSpeed.Whole - 15)) >= 0)
        {
            samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(15, 0);
        }

        var displacement = SamusHorizontalDisplacement.Toward(samus.PoseMetadata.IsFacingLeft(), samus, baseSpeed: 0);
        return SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = displacement.Displacement, CanBreakBombBlocks = true, CollisionMovementDirection = displacement.CollisionDirection });
    }

    /// <summary>Accumulates upward spark acceleration while capping only the temporary collision displacement and probing solid enemies first.</summary>
    private BlockMoveResult MoveY(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms)
    {
        ShineTimer = NativeCountdown.Start(15);
        VerticalAcceleration += SnesSignedSixteenSixteen.FromRaw(unchecked((int)SnesSixteenSixteen.Compose(
            samus.Kinematics.YAcceleration.Whole,
            samus.Kinematics.YAcceleration.Fraction)));
        uint acceleration = unchecked((uint)VerticalAcceleration.RawValue);

        uint speed = unchecked(SnesSixteenSixteen.Compose(
            samus.Kinematics.YSpeed.Whole,
            samus.Kinematics.YSpeed.Fraction) + acceleration);
        samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(unchecked((ushort)(SnesSixteenSixteen.WholeOf(speed))), unchecked((ushort)speed));

        // The native clamp operates on a temporary displacement, not stored velocity.
        // Keep accumulating the wrapped speed words even after movement reaches its cap.
        uint displacementSpeed = unchecked((short)(samus.Kinematics.YSpeed.Whole - 14)) >= 0
            ? SnesSixteenSixteen.Compose(14, samus.Kinematics.YSpeed.Fraction)
            : speed;
        int upwardDisplacement = unchecked(-(int)displacementSpeed);
        upwardDisplacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            samus.Kinematics,
            upwardDisplacement);

        // `$90:D261-$D29F` temporarily negates the signed result, clamps only a positive
        // upward magnitude at 15 whole pixels while preserving its fraction, then negates
        // it back before the room move. If external Y is strong enough to reverse motion
        // downward, the temporary value is negative and deliberately bypasses this clamp.
        int collisionMagnitude = unchecked(-upwardDisplacement);
        short collisionMagnitudeWhole = unchecked((short)SnesSixteenSixteen.WholeOf(collisionMagnitude));
        if (unchecked((short)(collisionMagnitudeWhole - 15)) >= 0)
        {
            collisionMagnitude = unchecked((int)(
                (15u << 16) | (ushort)collisionMagnitude));
            upwardDisplacement = unchecked(-collisionMagnitude);
        }
        // Shinespark's native solid-enemy branch sets collision and returns without
        // adding the clipped distance to Y. The ordinary upward mover instead advances
        // to the enemy boundary; using it here shifts the entire crash/recovery anchor.
        SolidEnemyCollisionResult enemy = SamusSolidEnemyCollision.Probe(
            samus.Kinematics, samus.Kinematics.InteractiveEnemies, SamusCollisionDirection.Up,
            SnesFixedPosition.WholeOf(collisionMagnitude), unchecked((ushort)collisionMagnitude));
        samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirection.Up, enemy.EnemyIndex);
        if (enemy.Collided)
            return new BlockMoveResult(0, true, EnemyCollision: enemy);

        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = upwardDisplacement, ScanLeftToRight = (nmiFrameCounter & 1) == 0, CanBreakBombBlocks = true, IncludeSolidEnemies = false });
    }

    /// <summary>
    /// Mutable storage corresponding to one of native fixed projectile slots three/four.
    /// It is deliberately private so callers cannot manufacture a live visual projectile.
    /// </summary>
    internal sealed class ReleasedEchoSlot
    {
        /// <summary>Drawing enable, independent of the slot's current projectile owner.</summary>
        public bool Active { get; private set; }
        /// <summary>Byte-angle used to place the departing echo around Samus.</summary>
        public SnesAngle Angle { get; set; }
        /// <summary>
        /// Expanding radius in pixels, 0 through <see cref="LargestRadius"/>, retained in the owning projectile's X
        /// velocity word: zero when the echo departs and raised only by <see cref="ExpandRadius"/>. The sine lookup
        /// places the echo with the radius taken modulo 256.
        /// </summary>
        public int Radius { get; set; }

        /// <summary>The largest radius the projectile velocity word holds; one more expansion wraps past it to zero.</summary>
        internal const int LargestRadius = ushort.MaxValue;

        /// <summary>Pixels the radius grows on each departing call.</summary>
        private const int RadiusStep = 8;

        /// <summary>Grows the radius by one departing call's step, wrapping past <see cref="LargestRadius"/> to zero.</summary>
        public void ExpandRadius()
        {
            int expanded = Radius + RadiusStep;
            Radius = expanded > LargestRadius ? expanded - (LargestRadius + 1) : expanded;
        }
        /// <summary>Last rendered room position, including the slot-three crash aliases.</summary>
        public Position16<RoomPixels> Position { get; set; }

        /// <summary>Copies only the drawing enable and position for debugger consumers.</summary>
        public ShinesparkReleasedEcho Snapshot => new(Active, Position);

        /// <summary>Clears drawing enable while retaining coordinates, projectile ownership and aliased crash travel.</summary>
        public void DisableDrawing() => Active = false;

        /// <summary>Activates a departing echo at its crash position with zero projectile radius.</summary>
        public void Initialize(SnesAngle angle, Position16<RoomPixels> position)
        {
            Active = true;
            Angle = angle;
            // Crash finish clears projectile X velocity (the departing radius). The
            // separate speed-echo drawing field is 64; it is not this projectile word.
            Radius = 0;
            Position = position;
        }

        /// <summary>Zeroes the native drawing and position words after a viewport rejection.</summary>
        public void Clear()
        {
            Active = false;
            Angle = SnesAngle.Zero;
            Radius = 0;
            Position = Position16<RoomPixels>.Zero;
        }
    }
}
