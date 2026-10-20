using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Literal, debugger-visible translation of Crystal Flash's three installed movement
/// handlers at <c>$90:D678-$90:D792</c>.
/// </summary>
/// <remarks>
/// Crystal Flash is unusual even among Samus's scripted movement states. Bank <c>$88</c>
/// attempts it only when the power-bomb explosion centred on Samus finishes; successful
/// initiation then replaces both the normal pose-input handler and the normal movement
/// handler. Keeping that handler pointer as <see cref="Phase"/> prevents pose `$D3/$D4`
/// from accidentally inheriting invented type-$1B physics.
/// </remarks>
public sealed class SamusCrystalFlashState
{
    /// <summary>The exact normal-game controller chord: Down + L + R + the Shot binding.</summary>
    public const SnesButton RequiredInputWithoutShot = SnesButtons.CrystalFlashWithoutShot;

    /// <summary>Debugger-readable equivalent of the active bank-$90 movement-handler pointer.</summary>
    public CrystalFlashPhase Phase { get; private set; }

    /// <summary>WRAM <c>$0A64</c>, initialized to nine for the ten two-pixel raise frames.</summary>
    public NativeCountdown RaiseTimer { get; private set; }

    /// <summary>
    /// The ammunition the drain is consuming: missiles at activation, then super missiles and power bombs as each
    /// ten-decrement run expires. No other owner shares this selector.
    /// </summary>
    public CrystalFlashAmmoKind DrainingAmmo { get; private set; }

    /// <summary>WRAM <c>$0DEC</c>, reloaded to ten between the three ammunition families.</summary>
    public NativeCountdown AmmoDecrementTimer { get; private set; }

    /// <summary>Mirrors another owner's write, 0 through 65535, to the shared ammo-countdown word; other values are rejected.</summary>
    internal void SetSharedAmmoCounter(int value) =>
        AmmoDecrementTimer = NativeCountdown.FromRaw(checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value)));

    /// <summary>Replaces the movement pointer without clearing the live Flash palette/timer.</summary>
    internal void RelinquishMovementHandler() => Phase = CrystalFlashPhase.Inactive;

    /// <summary>Another special palette handler replaces $0ACC and its live $0A68 timer.</summary>
    internal void RelinquishPaletteHandler()
    {
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        SpecialPaletteTimer = NativeCountdown.Start(0);
    }

    /// <summary>
    /// Samus's room position captured after the complete 20-pixel rise. Only the Y axis is meaningful: the finish
    /// handler compares Samus's whole Y with it through the wrapped spatial difference and walks her down one
    /// pixel per call while they differ.
    /// </summary>
    public Position16<RoomPixels> RaisedPosition { get; private set; }

    /// <summary>WRAM <c>$0DF2</c>, seeded to one for bank-$91's palette program.</summary>
    public NativeCountdown CrystalPaletteTimer { get; private set; }

    /// <summary>
    /// The special-palette handler this owner holds: Crystal Flash from activation until the finish restores the
    /// beam palette, or none once another handler replaces it.
    /// </summary>
    public SamusSpecialPaletteType SpecialPaletteKind { get; internal set; }

    /// <summary>
    /// Byte offset of the current bubble colour set, 0 through twice the bubble-set count minus two, in steps of
    /// two. Each bubble-timer expiry advances it by two and wraps it to zero after the last set; it is mirrored
    /// into the shared special-palette frame the next special-palette owner inherits.
    /// </summary>
    public int SpecialPaletteFrame { get; private set; }

    /// <summary>WRAM <c>$0A68</c>; <c>$FFFF</c> requests beam-palette restoration at finish.</summary>
    public NativeCountdown SpecialPaletteTimer { get; internal set; }

    /// <summary>
    /// Byte offset of the current body-palette record, four bytes per record. Activation inherits the value the
    /// shared special-palette timer holds (Speed Booster's), so it is any value from 0 through 65535 until the
    /// first body-timer expiry validates it as one of the ten record offsets; afterwards each expiry advances it
    /// by one record and wraps it to zero after the tenth. It is mirrored into that shared timer every call.
    /// </summary>
    public int BodyPaletteRecordOffset { get; private set; }

    /// <summary>Set when `$90:D6C5` would spawn the two bank-$88 window HDMA objects.</summary>
    public bool BubbleHdmaRequested { get; private set; }

    /// <summary>Set when `$90:D6AA` queues sound-library-three effect one.</summary>
    public bool ActivationSoundRequested { get; private set; }

    /// <summary>
    /// Consumes the one <c>QueueSfx3_Max15($01)</c> call emitted when the raising phase
    /// becomes the main Crystal Flash phase. The flag is debugger-visible, while this
    /// method gives the frontend the native one-shot semantics.
    /// </summary>
    public bool ConsumeActivationSoundRequest()
    {
        bool requested = ActivationSoundRequested;
        ActivationSoundRequested = false;
        return requested;
    }

    /// <summary>
    /// Applies every test and initialization write in <c>CrystalFlash</c> at
    /// <c>$90:D5A2</c>.
    /// </summary>
    /// <param name="samus">Live actor whose admission state is checked and whose pose, handler ownership, shared timers, and hit state are updated on success.</param>
    /// <param name="controllerInput">Raw held-button word; ordinary admission requires exact equality to Down, L, R, and the Shot binding.</param>
    /// <param name="shotBinding">Raw controller mask for Shot, defaulting to X; non-controller bits are rejected.</param>
    /// <param name="skipInputCheck">
    /// True only for the title-demo route where game state is at least <c>$28</c>. Ordinary
    /// gameplay must supply the exact chord; extra held buttons make native initiation fail.
    /// </param>
    /// <returns>True after installing the raising handler; false when the chord or native speed, health, reserve, or ammunition preconditions reject activation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A controller word contains non-controller bits or the Crystal Flash pose no longer has native movement type $1B.</exception>
    public bool TryBegin(
        SamusState samus,
        SnesButton controllerInput,
        SnesButton shotBinding = SnesButton.X,
        bool skipInputCheck = false)
    {
        Ensure.NotNull(samus);

        // `$90:D5AD-$D5B9` uses CMP, not a bit test. This deliberately rejects any extra
        // held button, including Jump, Dash, Select, or the opposite D-pad directions.
        SnesButton heldButtons = controllerInput;
        SnesButton shotButton = shotBinding;
        SnesButton requiredInput = RequiredInputWithoutShot | shotButton;
        if (!skipInputCheck && heldButtons != requiredInput)
            return false;

        // The remaining preconditions are literal WRAM comparisons. Reserve energy must
        // be empty even when the reserve mode is manual; possession of a reserve tank alone
        // does not matter. Each current ammo count—not its maximum—must supply ten shots.
        if (samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0 ||
            samus.Health >= 51 || samus.ReserveEnergy != 0 ||
            samus.Missiles < 10 ||
            samus.SuperMissiles < 10 ||
            samus.PowerBombs < 10)
        {
            return false;
        }

        // `$90:D5ED-$D600` considers direction byte four left and every other value right.
        // Read that byte from the source pose before replacing it with Crystal Flash art.
        bool facingLeft = samus.PoseMetadata.IsFacingLeft();
        samus.Pose = facingLeft
            ? SamusPoseId.CrystalFlashLeftPose
            : SamusPoseId.CrystalFlashRightPose;
        samus.PoseMetadata.RefreshCollisionRadii();
        if (samus.PoseMetadata.ReadMovementKind() != SamusMovementType.Special)
            throw new InvalidDataException("ROM pose $D3/$D4 no longer has movement type $1B.");
        samus.Presentation.InitializeAnimation(initialFrame: 0);
        // Successful HDMA-owned activation publishes history directly, before
        // installing its movement handler. Rejected activation must not shift it.
        samus.CommitPoseHistory();

        // The native routine reuses shinespark words but installs a distinct pointer. The
        // C# states stay separate so a debugger cannot mistake Crystal Flash for a spark.
        samus.Shinespark.RelinquishToCrystalFlash();
        // Reserve Mode leaves X-Ray's HDMA object alive but Crystal Flash still replaces
        // the shared special-palette handler. Keep the greyout object; release only its
        // palette words so handler seven is the sole cartridge owner.
        samus.Xray.RelinquishPaletteHandler();
        samus.DraygonGrabbed.RelinquishMovementHandler();
        // Flash admission can occur after the Super Metroid has already installed drained
        // crouching art. `$90:D5A2` then replaces that pose/movement owner; if the host kept
        // its synthetic Drained phase alive, it would re-lock Samus after Flash completed
        // instead of reproducing the documented adjacent timing that frees the stun.
        samus.Drained.RelinquishMovementHandler();
        samus.InputSnapshot.CrystalFlashPoseInputLocked = true;
        RaiseTimer = NativeCountdown.Start(9);
        DrainingAmmo = CrystalFlashAmmoKind.Missiles;
        AmmoDecrementTimer = NativeCountdown.Start(10);
        samus.DraygonGrabbed.SetSharedEscapeCounter(AmmoDecrementTimer.RawValue);
        RaisedPosition = Position16<RoomPixels>.Zero;
        CrystalPaletteTimer = NativeCountdown.Start(1);
        SpecialPaletteKind = SamusSpecialPaletteType.CrystalFlash;
        SpecialPaletteFrame = 0;
        SpecialPaletteTimer = NativeCountdown.Start(1);
        // `$90:D5A2` clears shared `$0ACE` but does not write shared `$0AD0`. The latter
        // is also Speed Booster's palette timer, so preserve the live aliased word rather
        // than silently inventing the usual zero initialization.
        samus.HorizontalSpeed.SpecialPaletteFrame = 0;
        BodyPaletteRecordOffset = samus.HorizontalSpeed.SpecialPaletteTimer.RawValue;
        BubbleHdmaRequested = false;
        ActivationSoundRequested = false;
        // Activation discards existing hit immunity; Crystal Flash is not a damage shield.
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        samus.KnockbackTimer = NativeCountdown.Start(0);
        samus.KnockbackDirection = Movement.SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        Phase = CrystalFlashPhase.Raising;
        return true;
    }

    /// <summary>Executes one active handler call in the native beta-movement position.</summary>
    /// <param name="samus">Live actor receiving movement, ammunition/energy, animation, and handler-completion writes.</param>
    /// <param name="nmiFrameCounter">Native accepted-NMI counter; ammunition drains only when its low three bits are zero.</param>
    /// <returns>A value snapshot of the entry phase and retained bubble-HDMA request, even when this call changes phase.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No Flash movement handler is installed, or the phase/ammunition-family state is unsupported.</exception>
    public CrystalFlashMovementResult Step(
        SamusState samus,
        int nmiFrameCounter)
    {
        Ensure.NotNull(samus);
        if (Phase == CrystalFlashPhase.Inactive)
            throw new InvalidOperationException("Crystal Flash has no installed movement handler.");

        CrystalFlashPhase phaseAtStart = Phase;

        switch (Phase)
        {
            case CrystalFlashPhase.Raising:
                StepRaising(samus);
                break;

            case CrystalFlashPhase.DrainingAmmo:
                StepAmmoDrain(samus, nmiFrameCounter);
                // The native wrapper clears both words after calling the ammo handler,
                // including its seven no-drain frames and the final phase-changing call.
                samus.InvincibilityTimer = NativeCountdown.Start(0);
                samus.KnockbackTimer = NativeCountdown.Start(0);
                break;

            case CrystalFlashPhase.Finishing:
                StepFinish(samus);
                break;

            case CrystalFlashPhase.Inactive:
            default:
                throw new InvalidOperationException($"Unknown Crystal Flash phase {Phase}.");
        }

        return new CrystalFlashMovementResult(
            phaseAtStart,
            BubbleHdmaRequested);
    }

    /// <summary>Raises Samus two whole pixels for ten calls, then starts the drain phase and requests bubble HDMA.</summary>
    private void StepRaising(SamusState samus)
    {
        // `$90:D678-$D67D` changes only the whole-pixel word. The subposition survives.
        samus.Position -= new Offset16<RoomPixels>(0, 2);

        // Native initializes nine, then uses DEC/BPL. Values 8..0 return; the tenth call
        // underflows to $FFFF and installs the main handler after completing its Y move.
        NativeWordCounterStep raiseTimer = RaiseTimer.Decrement();
        RaiseTimer = raiseTimer.Remaining;
        if (raiseTimer.IsNonNegative)
            return;

        // The delay program is already on its raise loop, but the movement handler forces
        // frame six and timer three so main art begins on this precise transition frame.
        samus.Presentation.SetAnimationFrameFromSpecialHandler(frame: 6, timer: 3);
        RaisedPosition = Position16<RoomPixels>.Zero.WithY(samus.Position.Y.Whole);
        Phase = CrystalFlashPhase.DrainingAmmo;
        BubbleHdmaRequested = true;
        ActivationSoundRequested = true;
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        samus.KnockbackTimer = NativeCountdown.Start(0);
        samus.KnockbackActive = false;
    }

    /// <summary>
    /// Consumes missiles, supers, then power bombs on each eighth NMI and restores 50 energy per decrement. Each
    /// decrement is unchecked, so a kind the activation check let through with fewer than ten drains below zero.
    /// </summary>
    private void StepAmmoDrain(SamusState samus, int nmiFrameCounter)
    {
        // All three pointed routines return immediately on seven of every eight accepted
        // NMI frames. Main-loop frames do not own an independent Crystal Flash cadence.
        if ((nmiFrameCounter & 7) != 0)
            return;

        switch (DrainingAmmo)
        {
            case CrystalFlashAmmoKind.Missiles:
                samus.Missiles = SamusQuantity.DecrementWord(samus.Missiles);
                break;
            case CrystalFlashAmmoKind.SuperMissiles:
                samus.SuperMissiles = SamusQuantity.DecrementWord(samus.SuperMissiles);
                break;
            case CrystalFlashAmmoKind.PowerBombs:
                samus.PowerBombs = SamusQuantity.DecrementWord(samus.PowerBombs);
                break;
            default:
                throw new InvalidOperationException(
                    $"Crystal Flash ammunition kind {DrainingAmmo} is undefined.");
        }

        SamusEnergyRestoration.Restore(samus, 50);
        NativeWordCounterStep ammoTimer = AmmoDecrementTimer.Decrement();
        AmmoDecrementTimer = ammoTimer.Remaining;
        samus.DraygonGrabbed.SetSharedEscapeCounter(AmmoDecrementTimer.RawValue);
        bool timerExpired = ammoTimer.IsZeroOrNegative;
        if (!timerExpired)
            return;

        if (DrainingAmmo != CrystalFlashAmmoKind.PowerBombs)
        {
            AmmoDecrementTimer = NativeCountdown.Start(10);
            samus.DraygonGrabbed.SetSharedEscapeCounter(AmmoDecrementTimer.RawValue);
            DrainingAmmo = DrainingAmmo == CrystalFlashAmmoKind.Missiles
                ? CrystalFlashAmmoKind.SuperMissiles
                : CrystalFlashAmmoKind.PowerBombs;
            return;
        }

        // `$90:D742-$D757` switches to finish art directly; it does not wait for the main
        // four-frame animation loop to reach a command byte.
        Phase = CrystalFlashPhase.Finishing;
        samus.Presentation.SetAnimationFrameFromSpecialHandler(frame: 12, timer: 3);
    }

    /// <summary>Waits for standing pose after the finish art, then releases input locks and requests palette restoration.</summary>
    private void StepFinish(SamusState samus)
    {
        // Retail usually enters with equality because RaisedPosition was captured at the
        // end of the rise. Preserve the real defensive branch: an external producer that
        // displaced Samus upward/downward makes the whole Y word walk downward one per call.
        if ((samus.Position.Whole - RaisedPosition).Y != 0)
            samus.Position += new Offset16<RoomPixels>(0, 1);

        // The ROM delay list ends in `$FD,$01/$02`; pose transition occurs after movement.
        // Therefore cleanup naturally happens on the following beta pass, once type zero is
        // observable here, and that frame performs no ordinary standing movement.
        if (samus.PoseMetadata.ReadMovementKind() != SamusMovementType.Standing)
            return;

        SpecialPaletteTimer = NativeCountdown.Start(ushort.MaxValue);
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // Normal-input completion clears the same hit timers as the native return path.
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        samus.KnockbackTimer = NativeCountdown.Start(0);
        samus.KnockbackActive = false;
        Phase = CrystalFlashPhase.Inactive;
    }

    /// <summary>
    /// Translates special Samus palette handler seven at `$91:DB93-$91:DBFF`.
    /// </summary>
    /// <remarks>
    /// Sprite palette six is split deliberately: the ten body colors cycle through a
    /// ten-record pointer/timer program, while the upper six bubble colors cycle through
    /// an independent six-pointer table every five handler calls. Both sources are bank-$9B
    /// ROM data; completion restores the complete bank-$90 beam palette selected by the
    /// currently equipped beam combination.
    /// </remarks>
    public bool UpdatePalette(
        IPaletteColors cgram,
        SamusState samus,
        SuperMetroid.Core.Samus.Artwork.BeamPaletteCatalog? palettes = null,
        SuperMetroid.Core.Samus.Artwork.CrystalFlashColorCatalog? colors = null)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(samus);
        if (SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash)
            return false;

        if (SpecialPaletteTimer.IsNegative)
        {
            // `$91:DBEB` calls `$90:ACC2`, which masks away Charge Beam and uses the
            // resulting low twelve bits as an index into the retail beam-palette table.
            if (new SamusBeamLoadoutWord(samus.EquippedBeams).Combination is not SamusBeamCombination beamType ||
                !beamType.IsRetail)
                throw new ArgumentOutOfRangeException(nameof(samus), "Equipped beam combination is outside the retail table.");
            (palettes ?? throw new InvalidOperationException(
                "Crystal Flash requires installed beam palettes.")).LoadTo(cgram, beamType);

            SpecialPaletteKind = SamusSpecialPaletteType.None;
            SpecialPaletteFrame = 0;
            BodyPaletteRecordOffset = 0;
            SpecialPaletteTimer = NativeCountdown.Start(0);
            CrystalPaletteTimer = NativeCountdown.Start(0);
            PublishSharedPaletteWords(samus);
            return true;
        }

        // DEC followed by BEQ/BPL treats both zero and a signed underflow as expiry.
        NativeWordCounterStep specialPaletteTimer =
            SpecialPaletteTimer.Decrement();
        SpecialPaletteTimer = specialPaletteTimer.Remaining;
        if (specialPaletteTimer.IsZeroOrNegative)
        {
            SpecialPaletteTimer = NativeCountdown.Start(5);
            if (colors is null || (SpecialPaletteFrame & 1) != 0 ||
                SpecialPaletteFrame >= SamusPaletteRomData.CrystalFlash.BubblePaletteCount * sizeof(ushort))
                throw new InvalidDataException(
                    $"Crystal Flash bubble frame {SpecialPaletteFrame} has no installed color.");
            colors.ApplyBubble(cgram, SpecialPaletteFrame / sizeof(ushort));

            // The frame was validated below the table end above, so this advance stays far below any wrap.
            int nextBubbleFrame = SpecialPaletteFrame + sizeof(ushort);
            SpecialPaletteFrame = nextBubbleFrame <
                SamusPaletteRomData.CrystalFlash.BubblePaletteCount * sizeof(ushort)
                    ? nextBubbleFrame
                    : 0;
        }

        NativeWordCounterStep crystalPaletteTimer =
            CrystalPaletteTimer.Decrement();
        CrystalPaletteTimer = crystalPaletteTimer.Remaining;
        if (crystalPaletteTimer.IsZeroOrNegative)
        {
            if (colors is null ||
                BodyPaletteRecordOffset % SamusPaletteRomData.CrystalFlash.BodyRecordByteCount != 0 ||
                BodyPaletteRecordOffset >= SamusPaletteRomData.CrystalFlash.BodyRecordCount *
                    SamusPaletteRomData.CrystalFlash.BodyRecordByteCount)
                throw new InvalidDataException(
                    $"Crystal Flash body frame {BodyPaletteRecordOffset} has no installed color.");
            CrystalPaletteTimer =
                NativeCountdown.FromRaw(CrystalFlashPaletteTimingDefinitions.DurationForByteOffset(
                    BodyPaletteRecordOffset));
            colors.ApplyBody(cgram,
                BodyPaletteRecordOffset / SamusPaletteRomData.CrystalFlash.BodyRecordByteCount);

            // The offset was validated as one of the ten records above, so this advance cannot wrap.
            int nextRecord = BodyPaletteRecordOffset + SamusPaletteRomData.CrystalFlash.BodyRecordByteCount;
            BodyPaletteRecordOffset = nextRecord <
                SamusPaletteRomData.CrystalFlash.BodyRecordCount *
                    SamusPaletteRomData.CrystalFlash.BodyRecordByteCount
                        ? nextRecord
                        : 0;
        }

        // Carry is set for every active call, including frames on which neither timer
        // expires. The outer palette dispatcher must therefore suppress its normal copy.
        PublishSharedPaletteWords(samus);
        return true;
    }


    /// <summary>Mirrors Flash's frame and timer into WRAM words shared with other special palettes.</summary>
    private void PublishSharedPaletteWords(SamusState samus)
    {
        // `$0ACE/$0AD0` are not Crystal-Flash-private storage. Speed Booster, Screw Attack,
        // X-ray, and several other special handlers reuse the same two physical words.
        // Their handlers are mutually exclusive, but exposing the alias keeps breakpoint
        // inspection and the next owner's initial state faithful to WRAM.
        samus.HorizontalSpeed.SpecialPaletteFrame = checked((ushort)SpecialPaletteFrame);
        samus.HorizontalSpeed.SpecialPaletteTimer = NativeCountdown.FromRaw(checked((ushort)BodyPaletteRecordOffset));
    }
}
