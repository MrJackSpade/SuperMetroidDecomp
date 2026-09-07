using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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
    public ushort RaiseTimer { get; private set; }

    /// <summary>WRAM <c>$0DEA</c>: zero missiles, one supers, two power bombs.</summary>
    public ushort AmmoDecrementIndex { get; private set; }

    /// <summary>WRAM <c>$0DEC</c>, reloaded to ten between the three ammunition families.</summary>
    public ushort AmmoDecrementTimer { get; private set; }

    /// <summary>WRAM <c>$0DF0</c>, captured after the complete 20-pixel rise.</summary>
    public ushort RaisedYPosition { get; private set; }

    /// <summary>WRAM <c>$0DF2</c>, seeded to one for bank-$91's palette program.</summary>
    public ushort CrystalPaletteTimer { get; private set; }

    /// <summary>WRAM <c>$0ACC</c>; seven dispatches the cartridge's Crystal Flash palette.</summary>
    public ushort SpecialPaletteType { get; private set; }

    /// <summary>Typed view of the native special-palette handler index.</summary>
    public SamusSpecialPaletteType SpecialPaletteKind =>
        (SamusSpecialPaletteType)SpecialPaletteType;

    /// <summary>WRAM <c>$0ACE</c>, initialized to the first Crystal Flash palette record.</summary>
    public ushort SpecialPaletteFrame { get; private set; }

    /// <summary>WRAM <c>$0A68</c>; <c>$FFFF</c> requests beam-palette restoration at finish.</summary>
    public ushort SpecialPaletteTimer { get; private set; }

    /// <summary>
    /// WRAM <c>$0AD0</c>, a byte offset into the ten interleaved pointer/timer records at
    /// `$91:DC00`.
    /// </summary>
    public ushort CommonPaletteTimer { get; private set; }

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
    /// <param name="skipInputCheck">
    /// True only for the title-demo route where game state is at least <c>$28</c>. Ordinary
    /// gameplay must supply the exact chord; extra held buttons make native initiation fail.
    /// </param>
    public bool TryBegin(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput,
        ushort shotBinding = (ushort)SnesButton.X,
        bool skipInputCheck = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // `$90:D5AD-$D5B9` uses CMP, not a bit test. This deliberately rejects any extra
        // held button, including Jump, Dash, Select, or the opposite D-pad directions.
        SnesButton heldButtons = SnesButtons.FromRaw(controllerInput, "Crystal Flash input");
        SnesButton shotButton = SnesButtons.FromRaw(shotBinding, "Crystal Flash Shot binding");
        SnesButton requiredInput = RequiredInputWithoutShot | shotButton;
        if (!skipInputCheck && heldButtons != requiredInput)
            return false;

        // The remaining preconditions are literal WRAM comparisons. Reserve energy must
        // be empty even when the reserve mode is manual; possession of a reserve tank alone
        // does not matter. Each current ammo count—not its maximum—must supply ten shots.
        if (samus.Kinematics.YSpeed != 0 || samus.Kinematics.YSubspeed != 0 ||
            !SignedLessThan(samus.Health, 0x0033) || samus.ReserveEnergy != 0 ||
            SignedLessThan(samus.Missiles, 10) ||
            SignedLessThan(samus.SuperMissiles, 10) ||
            SignedLessThan(samus.PowerBombs, 10))
        {
            return false;
        }

        // `$90:D5ED-$D600` considers direction byte four left and every other value right.
        // Read that byte from the source pose before replacing it with Crystal Flash art.
        bool facingLeft = samus.IsFacingLeft(bus);
        samus.Pose = facingLeft
            ? SamusPoseIds.CrystalFlashLeftPose
            : SamusPoseIds.CrystalFlashRightPose;
        samus.RefreshCollisionRadii(bus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.Special)
            throw new InvalidDataException("ROM pose $D3/$D4 no longer has movement type $1B.");
        samus.InitializeAnimation(bus, initialFrame: 0);

        // The native routine reuses shinespark words but installs a distinct pointer. The
        // C# states stay separate so a debugger cannot mistake Crystal Flash for a spark.
        RaiseTimer = 9;
        AmmoDecrementIndex = 0;
        AmmoDecrementTimer = 10;
        RaisedYPosition = 0;
        CrystalPaletteTimer = 1;
        SpecialPaletteType = (ushort)SamusSpecialPaletteType.CrystalFlash;
        SpecialPaletteFrame = 0;
        SpecialPaletteTimer = 1;
        // `$90:D5A2` clears shared `$0ACE` but does not write shared `$0AD0`. The latter
        // is also Speed Booster's palette timer, so preserve the live aliased word rather
        // than silently inventing the usual zero initialization.
        samus.HorizontalSpeed.SpecialPaletteFrame = 0;
        CommonPaletteTimer = samus.HorizontalSpeed.SpecialPaletteTimer;
        BubbleHdmaRequested = false;
        ActivationSoundRequested = false;
        samus.KnockbackTimer = 0;
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        Phase = CrystalFlashPhase.Raising;
        return true;
    }

    /// <summary>Executes one active handler call in the native beta-movement position.</summary>
    public CrystalFlashMovementResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (Phase == CrystalFlashPhase.Inactive)
            throw new InvalidOperationException("Crystal Flash has no installed movement handler.");

        CrystalFlashPhase phaseAtStart = Phase;
        ushort healthAtStart = samus.Health;
        ushort missilesAtStart = samus.Missiles;
        ushort supersAtStart = samus.SuperMissiles;
        ushort powerBombsAtStart = samus.PowerBombs;

        switch (Phase)
        {
            case CrystalFlashPhase.Raising:
                StepRaising(samus);
                break;

            case CrystalFlashPhase.DrainingAmmo:
                StepAmmoDrain(samus, nmiFrameCounter);
                break;

            case CrystalFlashPhase.Finishing:
                StepFinish(bus, samus);
                break;

            default:
                throw new InvalidOperationException($"Unknown Crystal Flash phase {Phase}.");
        }

        return new CrystalFlashMovementResult(
            phaseAtStart,
            Phase,
            samus.Health != healthAtStart,
            samus.Missiles != missilesAtStart ||
                samus.SuperMissiles != supersAtStart ||
                samus.PowerBombs != powerBombsAtStart,
            BubbleHdmaRequested,
            Phase == CrystalFlashPhase.Inactive);
    }

    private void StepRaising(SamusState samus)
    {
        // `$90:D678-$D67D` changes only the whole-pixel word. The subposition survives.
        samus.YPosition = unchecked((ushort)(samus.YPosition - 2));

        // Native initializes nine, then uses DEC/BPL. Values 8..0 return; the tenth call
        // underflows to $FFFF and installs the main handler after completing its Y move.
        NativeWordCounterStep raiseTimer = NativeWordCounter.Decrement(RaiseTimer);
        RaiseTimer = raiseTimer.Value;
        if (raiseTimer.IsNonNegative)
            return;

        // The delay program is already on its raise loop, but the movement handler forces
        // frame six and timer three so main art begins on this precise transition frame.
        samus.SetAnimationFrameFromSpecialHandler(frame: 6, timer: 3);
        RaisedYPosition = samus.YPosition;
        Phase = CrystalFlashPhase.DrainingAmmo;
        BubbleHdmaRequested = true;
        ActivationSoundRequested = true;
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
    }

    private void StepAmmoDrain(SamusState samus, ushort nmiFrameCounter)
    {
        // All three pointed routines return immediately on seven of every eight accepted
        // NMI frames. Main-loop frames do not own an independent Crystal Flash cadence.
        if ((nmiFrameCounter & 7) != 0)
            return;

        switch (AmmoDecrementIndex)
        {
            case 0:
                samus.Missiles = unchecked((ushort)(samus.Missiles - 1));
                break;
            case 1:
                samus.SuperMissiles = unchecked((ushort)(samus.SuperMissiles - 1));
                break;
            case 2:
                samus.PowerBombs = unchecked((ushort)(samus.PowerBombs - 1));
                break;
            default:
                throw new InvalidOperationException(
                    $"Crystal Flash ammo pointer index ${AmmoDecrementIndex:X4} is invalid.");
        }

        SamusEnergyRestoration.Restore(samus, 50);
        NativeWordCounterStep ammoTimer = NativeWordCounter.Decrement(AmmoDecrementTimer);
        AmmoDecrementTimer = ammoTimer.Value;
        bool timerExpired = ammoTimer.IsZeroOrNegative;
        if (!timerExpired)
            return;

        if (AmmoDecrementIndex < 2)
        {
            AmmoDecrementTimer = 10;
            AmmoDecrementIndex = unchecked((ushort)(AmmoDecrementIndex + 1));
            return;
        }

        // `$90:D742-$D757` switches to finish art directly; it does not wait for the main
        // four-frame animation loop to reach a command byte.
        Phase = CrystalFlashPhase.Finishing;
        samus.SetAnimationFrameFromSpecialHandler(frame: 12, timer: 3);
    }

    private void StepFinish(ISnesAddressSpace bus, SamusState samus)
    {
        // Retail usually enters with equality because RaisedYPosition was captured at the
        // end of the rise. Preserve the real defensive branch: an external producer that
        // displaced Samus upward/downward makes the whole Y word walk downward one per call.
        if (samus.YPosition != RaisedYPosition)
            samus.YPosition = unchecked((ushort)(samus.YPosition + 1));

        // The ROM delay list ends in `$FD,$01/$02`; pose transition occurs after movement.
        // Therefore cleanup naturally happens on the following beta pass, once type zero is
        // observable here, and that frame performs no ordinary standing movement.
        if (samus.ReadMovementKind(bus) != SamusMovementType.Standing)
            return;

        SpecialPaletteTimer = 0xffff;
        samus.KnockbackTimer = 0;
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
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(samus);
        if (SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash)
            return false;

        if ((SpecialPaletteTimer & 0x8000) != 0)
        {
            // `$91:DBEB` calls `$90:ACC2`, which masks away Charge Beam and uses the
            // resulting low twelve bits as an index into the retail beam-palette table.
            int beamType = new SamusBeamLoadoutWord(samus.EquippedBeams).NativeConfigurationIndex;
            if ((uint)beamType >= SamusPaletteRomData.CrystalFlash.BeamPaletteCount)
                throw new ArgumentOutOfRangeException(nameof(samus), "Equipped beam combination is outside the retail table.");
            ushort beamPalette = ReadWord(
                bus,
                SamusPaletteRomData.CrystalFlash.BeamPalettePointers +
                    beamType * sizeof(ushort));
            cgram.LoadFromBus(
                bus,
                SamusPaletteRomData.CrystalFlash.BeamPaletteBank | beamPalette,
                colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
                destinationIndex: SamusPaletteRomData.CrystalFlash.BodyCgramStart);

            SpecialPaletteType = (ushort)SamusSpecialPaletteType.None;
            SpecialPaletteFrame = 0;
            CommonPaletteTimer = 0;
            SpecialPaletteTimer = 0;
            CrystalPaletteTimer = 0;
            PublishSharedPaletteWords(samus);
            return true;
        }

        // DEC followed by BEQ/BPL treats both zero and a signed underflow as expiry.
        NativeWordCounterStep specialPaletteTimer =
            NativeWordCounter.Decrement(SpecialPaletteTimer);
        SpecialPaletteTimer = specialPaletteTimer.Value;
        if (specialPaletteTimer.IsZeroOrNegative)
        {
            SpecialPaletteTimer = 5;
            ushort bubblePalette = ReadWord(
                bus,
                SamusPaletteRomData.CrystalFlash.BubblePointers + SpecialPaletteFrame);
            cgram.LoadFromBus(
                bus,
                SamusPaletteRomData.Banks.Palette | bubblePalette,
                colorCount: SamusPaletteRomData.CrystalFlash.BubbleColorCount,
                destinationIndex: SamusPaletteRomData.CrystalFlash.BubbleCgramStart);

            ushort nextBubbleFrame = unchecked((ushort)(SpecialPaletteFrame + 2));
            SpecialPaletteFrame = nextBubbleFrame <
                SamusPaletteRomData.CrystalFlash.BubblePaletteCount * sizeof(ushort)
                    ? nextBubbleFrame
                    : (ushort)0;
        }

        NativeWordCounterStep crystalPaletteTimer =
            NativeWordCounter.Decrement(CrystalPaletteTimer);
        CrystalPaletteTimer = crystalPaletteTimer.Value;
        if (crystalPaletteTimer.IsZeroOrNegative)
        {
            int recordAddress = SamusPaletteRomData.CrystalFlash.BodyRecords +
                CommonPaletteTimer;
            ushort bodyPalette = ReadWord(bus, recordAddress);
            CrystalPaletteTimer = ReadWord(bus, recordAddress + 2);
            cgram.LoadFromBus(
                bus,
                SamusPaletteRomData.Banks.Palette | bodyPalette,
                colorCount: SamusPaletteRomData.CrystalFlash.BodyColorCount,
                destinationIndex: SamusPaletteRomData.CrystalFlash.BodyCgramStart);

            ushort nextRecord = unchecked((ushort)(
                CommonPaletteTimer + SamusPaletteRomData.CrystalFlash.BodyRecordByteCount));
            CommonPaletteTimer = nextRecord <
                SamusPaletteRomData.CrystalFlash.BodyRecordCount *
                    SamusPaletteRomData.CrystalFlash.BodyRecordByteCount
                        ? nextRecord
                        : (ushort)0;
        }

        // Carry is set for every active call, including frames on which neither timer
        // expires. The outer palette dispatcher must therefore suppress its normal copy.
        PublishSharedPaletteWords(samus);
        return true;
    }

    private static bool SignedLessThan(ushort left, ushort right) =>
        unchecked((short)(left - right)) < 0;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));

    private void PublishSharedPaletteWords(SamusState samus)
    {
        // `$0ACE/$0AD0` are not Crystal-Flash-private storage. Speed Booster, Screw Attack,
        // X-ray, and several other special handlers reuse the same two physical words.
        // Their handlers are mutually exclusive, but exposing the alias keeps breakpoint
        // inspection and the next owner's initial state faithful to WRAM.
        samus.HorizontalSpeed.SpecialPaletteFrame = SpecialPaletteFrame;
        samus.HorizontalSpeed.SpecialPaletteTimer = CommonPaletteTimer;
    }
}

/// <summary>Named substitutes for Crystal Flash's three bank-$90 handler addresses.</summary>
public enum CrystalFlashPhase
{
    Inactive,
    Raising,
    DrainingAmmo,
    Finishing,
}

/// <summary>One-frame debugger witness from the translated Crystal Flash handler.</summary>
public readonly record struct CrystalFlashMovementResult(
    CrystalFlashPhase PhaseAtStart,
    CrystalFlashPhase PhaseAfterStep,
    bool RestoredEnergy,
    bool ConsumedAmmo,
    bool BubbleHdmaRequested,
    bool Completed);
