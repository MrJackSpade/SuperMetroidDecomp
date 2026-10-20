using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Bank-$88's two cooperating power-bomb HDMA objects, represented as one semantic owner.
/// </summary>
/// <remarks>
/// The cartridge uses one HDMA object to advance the explosion instruction list and a
/// second object to configure the color-math window. The second object has no independent
/// lifetime: it sleeps until the first object disables both channels. Keeping one host
/// object therefore exposes the meaningful state without inventing a second animation.
/// Every phase transition and every 8.8 fixed-point radius update below comes directly
/// from $88:8ACE/$8B80 and their pre-instructions at $88:8DE9-$91A8.
/// </remarks>
public sealed class SamusPowerBombExplosionState
{
    /// <summary>Current host-owned explosion colors; excluded from debugger-state serialization.</summary>
    [field: NonSerialized]
    public PowerBombFixedColorCatalog? PresentationColors { get; set; }

    /// <summary>The HDMA explosion status, 0 through 65535; the top bit set is the normal active state.</summary>
    [NativeWord("HDMA explosion status word: zero is no explosion and the top bit set is the active state")]
    public int Status
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The status bit set while the bank-$88 animation runs.</summary>
    private const int ActiveStatus = 1 << 15;

    /// <summary>Room-pixel centre copied from the projectile when its fuse reaches zero.</summary>
    public Position16<RoomPixels> Position { get; private set; }

    /// <summary>WRAM $0CEC, the pre-explosion flash's unsigned 8.8 radius.</summary>
    public PowerBombRadius PreExplosionRadius { get; private set; }

    /// <summary>WRAM $0CEA, the damaging explosion's unsigned 8.8 radius.</summary>
    public PowerBombRadius ExplosionRadius { get; internal set; }

    /// <summary>
    /// The per-frame radius change, deliberately shared by the pre-explosion and explosion phases. Cleanup leaves
    /// it in place; the next pre-explosion setup replaces it.
    /// </summary>
    public PowerBombRadius RadiusSpeed { get; private set; }

    /// <summary>The next authored window shape a shape phase consumes, or null when no shape stream is active.</summary>
    public PowerBombShapeCursor? Shape { get; private set; }

    /// <summary>
    /// The window shape the current rendered frame consumed before the cursor advanced, or null when the frame used none.
    /// </summary>
    public PowerBombShapeCursor? RenderedShape { get; private set; }

    /// <summary>Current semantic phase of the native sleeping instruction list.</summary>
    public PowerBombExplosionPhase Phase { get; private set; }

    /// <summary>
    /// Phase whose HDMA table was produced during the current logic frame.
    /// </summary>
    /// <remarks>
    /// A pre-instruction can advance <see cref="Phase"/> after generating its final table.
    /// Keeping the consumed phase separate prevents the host renderer from displaying the
    /// next phase one frame early at the yellow/white transition boundaries.
    /// </remarks>
    public PowerBombExplosionPhase RenderedPhase { get; internal set; }

    /// <summary>Pre-explosion radius consumed before the native end-of-frame update.</summary>
    public PowerBombRadius RenderedPreExplosionRadius { get; internal set; }

    /// <summary>Explosion radius consumed before the native end-of-frame update.</summary>
    public PowerBombRadius RenderedExplosionRadius { get; internal set; }

    /// <summary>Current fixed-color red component written through COLDATA, 0 through 255.</summary>
    public int FixedColorRed
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Current fixed-color green component written through COLDATA, 0 through 255.</summary>
    public int FixedColorGreen
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Current fixed-color blue component written through COLDATA, 0 through 255.</summary>
    public int FixedColorBlue
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>True from placement until cleanup releases the one-at-a-time Power Bomb lock.</summary>
    public bool IsArmed { get; private set; }

    /// <summary>True while the bank-$88 color-math animation is running.</summary>
    public bool IsActive => Status >= ActiveStatus;

    /// <summary>HDMA fade timer; its zero initialization underflows to permit the first afterglow step.</summary>
    private NativeCountdown _afterglowTimer;

    /// <summary>
    /// Afterglow steps left, 32 when the final white-explosion frame primes them; each step consumes one, and the
    /// step that reaches zero ends the explosion instead of fading the colors. It is only stepped while positive.
    /// </summary>
    private int _afterglowStepsRemaining;

    /// <summary>
    /// Native Crystal Flash completion count, kept separate from editable COLDATA so
    /// rebinding colors cannot change the HDMA cleanup frame after restoration.
    /// </summary>
    private int _crystalFlashAfterglowStepsRemaining;

    /// <summary>
    /// Implements $90:BF9D's early power-bomb lock before a projectile slot is initialized.
    /// </summary>
    public void Arm()
    {
        IsArmed = true;
    }

    /// <summary>
    /// Implements $88:8AA4 after $90:C157 has copied the projectile's world position.
    /// </summary>
    public void Spawn(Position16<RoomPixels> position)
    {
        Position = position;
        Status = ActiveStatus;

        // Allocation occurs after this frame's HDMA pass. The NEXT pass consumes the
        // instruction list and installs the radius pre-instruction; that pre-instruction
        // cannot run until the pass after setup. Executing setup here skips one frame.
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        // $88:8AA4 allocates the objects without clearing $0CF0. The next HDMA
        // setup owns its replacement, even when the previous explosion has ended.
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.PendingPreExplosionSetup;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        _afterglowTimer = NativeCountdown.Start(0);
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
    }

    /// <summary>
    /// Implements `$90:D6AE-$D6C5` and both setup calls at `$88:A2E4/$A309` after
    /// Crystal Flash completes its ten-step rise.
    /// </summary>
    public void BeginCrystalFlash(Position16<RoomPixels> position)
    {
        // `$90:D6AE` releases the one-at-a-time Power Bomb lock before spawning the two
        // Crystal Flash HDMA objects. Their animation still reuses the same status,
        // position, radius, speed, fixed-color, and window-table WRAM words.
        IsArmed = false;
        Position = position;
        Status = ActiveStatus;
        PreExplosionRadius = PowerBombRadius.FromRaw(SamusSpecialSequenceRomData.PowerBomb.InitialRadius);
        ExplosionRadius = PowerBombRadius.FromRaw(SamusSpecialSequenceRomData.PowerBomb.InitialRadius);
        RadiusSpeed = PowerBombRadius.Zero;
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.CrystalFlashExplosion;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        _afterglowTimer = NativeCountdown.Start(0);
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
    }

    /// <summary>
    /// Runs one invocation of the active bank-$88 HDMA pre-instruction.
    /// </summary>
    /// <returns>True only on the frame that $88:8B4E completes cleanup.</returns>
    public bool StepFrame()
    {
        if (!IsActive)
            return false;

        // Every bank-$88 pre-instruction first builds this frame's window and only then
        // mutates its radius/pointer or advances the sleeping instruction list. Record
        // that input phase here so post-logic desktop composition sees the generated
        // frame rather than the state prepared for the following one.
        RenderedPhase = Phase;

        switch (Phase)
        {
            case PowerBombExplosionPhase.PendingPreExplosionSetup:
                PreExplosionRadius = PowerBombRadius.FromRaw(SamusSpecialSequenceRomData.PowerBomb.InitialRadius);
                RadiusSpeed = PowerBombRadius.FromRaw(SamusSpecialSequenceRomData.PowerBomb.InitialPreExplosionSpeed);
                Phase = PowerBombExplosionPhase.PreExplosionWhite;
                RenderedPhase = PowerBombExplosionPhase.Inactive;
                return false;

            case PowerBombExplosionPhase.PreExplosionWhite:
                StepPreExplosionWhite();
                return false;

            case PowerBombExplosionPhase.PreExplosionYellow:
                StepPreExplosionYellow();
                return false;

            case PowerBombExplosionPhase.ExplosionYellow:
                StepExplosionYellow();
                return false;

            case PowerBombExplosionPhase.ExplosionWhite:
                StepExplosionWhite();
                return false;

            case PowerBombExplosionPhase.Afterglow:
                return StepAfterglow();

            case PowerBombExplosionPhase.CrystalFlashExplosion:
                StepCrystalFlashExplosion();
                return false;

            case PowerBombExplosionPhase.CrystalFlashAfterglow:
                return StepCrystalFlashAfterglow();

            case PowerBombExplosionPhase.Inactive:
            default:
                throw new InvalidOperationException($"Active power-bomb status has invalid phase {Phase}.");
        }
    }

    /// <summary>
    /// Releases the armed flag after cleanup when Crystal Flash did not take ownership.
    /// </summary>
    public void ReleaseFlag() => IsArmed = false;

    /// <summary>Clears every bank-$88/WRAM field during room/runtime reset.</summary>
    public void Reset()
    {
        IsArmed = false;
        Status = 0;
        Position = Position16<RoomPixels>.Zero;
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        RadiusSpeed = PowerBombRadius.Zero;
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        _afterglowTimer = NativeCountdown.Start(0);
        _afterglowStepsRemaining = 0;
        _crystalFlashAfterglowStepsRemaining = 0;
    }

    /// <summary>
    /// Retains the old runtime debugger seam used to force X-ray's status precondition.
    /// A forced status does not fabricate an animation phase or armed projectile.
    /// </summary>
    internal void SetStatusForDebugging(int value)
    {
        Status = value;
        if (value == 0 && Phase != PowerBombExplosionPhase.Inactive)
            Phase = PowerBombExplosionPhase.Inactive;
    }

    /// <summary>Displays the current white flash radius, then slows expansion until the yellow shape list takes over.</summary>
    private void StepPreExplosionWhite()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        ReadFixedColor(
            PowerBombFixedColorSequence.PreExplosion,
            (PreExplosionRadius.WholePixels >> 3) & 15);

        // $88:90DF uses wrapping 16-bit addition. It subtracts acceleration only while
        // the newly computed radius is still below $9200.
        PreExplosionRadius = AdvanceRadiusWrapping(PreExplosionRadius);
        if (PreExplosionRadius.RawValue < SamusSpecialSequenceRomData.PowerBomb.PreExplosionWhiteLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingSubtract(SamusSpecialSequenceRomData.PowerBomb.PreExplosionAcceleration);
            return;
        }

        // Advancing past Sleep executes CallFar($8B32), installs pre-instruction $91A8,
        // and sleeps again during this same HDMA-object-handler pass.
        Shape = new PowerBombShapeCursor(PowerBombShapeStream.Yellow, 0);
        Phase = PowerBombExplosionPhase.PreExplosionYellow;
    }

    /// <summary>
    /// Consumes the cursor's shape: returns the shape this frame's window is built from and moves the cursor to the
    /// next shape of its stream.
    /// </summary>
    /// <param name="finishedShapes">Whether the consumed shape was the stream's last.</param>
    private PowerBombShapeCursor ConsumeShape(out bool finishedShapes)
    {
        PowerBombShapeCursor shape = Shape ?? throw new InvalidOperationException(
            $"Power Bomb phase {Phase} consumes a window shape with no shape stream active.");
        PowerBombShapeCursor next = shape with { Index = shape.Index + 1 };
        finishedShapes = next.Index == PowerBombShapeCursor.CountOf(shape.Stream);
        Shape = next;
        return shape;
    }

    /// <summary>Advances a radius by the shared speed with the cartridge's sixteen-bit wraparound ($88:90DF).</summary>
    private PowerBombRadius AdvanceRadiusWrapping(PowerBombRadius radius) => radius.WrappingAdd(RadiusSpeed);

    /// <summary>
    /// Advances a radius by the shared speed unless the sum carries past 65535; the carry test runs before either
    /// word changes, so a carrying frame leaves the radius (and the caller's speed update) frozen.
    /// </summary>
    /// <returns>True with the advanced radius when the sum fits.</returns>
    private bool TryAdvanceRadiusWithoutCarry(PowerBombRadius radius, out PowerBombRadius advanced) =>
        radius.TryAddWithoutCarry(RadiusSpeed, out advanced);

    /// <summary>Consumes each yellow shape before advancing its pointer and primes the damaging radius at the last shape.</summary>
    private void StepPreExplosionYellow()
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        ReadFixedColor(
            PowerBombFixedColorSequence.PreExplosion,
            (PreExplosionRadius.WholePixels >> 3) & 15);

        // $91A8 builds this frame's two 192-byte window tables before incrementing the
        // source pointer. Preserve that consumed pointer for a host compositor running
        // after the logic frame has completed.
        RenderedShape = ConsumeShape(out bool finishedShapes);

        // The carry test is performed on the sum before either state word is changed.
        // If it would exceed $FFFF, both radius and speed remain frozen for this frame.
        if (TryAdvanceRadiusWithoutCarry(PreExplosionRadius, out PowerBombRadius nextRadius))
        {
            PreExplosionRadius = nextRadius;
            RadiusSpeed = RadiusSpeed.WrappingSubtract(SamusSpecialSequenceRomData.PowerBomb.PreExplosionAcceleration);
        }

        if (!finishedShapes)
            return;

        // CallFar($8B39) initializes the actual damaging radius before the next frame's
        // ExplosionYellow pre-instruction. PreExplosionRadius remains debugger-visible.
        ExplosionRadius = PowerBombRadius.FromRaw(SamusSpecialSequenceRomData.PowerBomb.InitialRadius);
        RadiusSpeed = PowerBombRadius.Zero;
        Phase = PowerBombExplosionPhase.ExplosionYellow;
    }

    /// <summary>Expands the damaging yellow radius with acceleration, then selects white window shapes.</summary>
    private void StepExplosionYellow()
    {
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            PowerBombFixedColorSequence.Explosion,
            ExplosionRadius.WholePixels >> 3);

        ExplosionRadius = AdvanceRadiusWrapping(ExplosionRadius);
        if (ExplosionRadius.RawValue < SamusSpecialSequenceRomData.PowerBomb.ExplosionYellowLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingAdd(SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration);
            return;
        }

        // The next instruction calls $8B47, selecting the first of seventeen expanding
        // white shape tables, then installs $8EB2 and sleeps.
        Shape = new PowerBombShapeCursor(PowerBombShapeStream.White, 0);
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.ExplosionWhite;
    }

    /// <summary>Consumes seventeen white shapes and primes the 32-step fixed-color afterglow.</summary>
    private void StepExplosionWhite()
    {
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            PowerBombFixedColorSequence.Explosion,
            ExplosionRadius.WholePixels >> 3);

        RenderedShape = ConsumeShape(out bool finishedShapes);

        if (TryAdvanceRadiusWithoutCarry(ExplosionRadius, out PowerBombRadius nextRadius))
        {
            ExplosionRadius = nextRadius;
            RadiusSpeed = RadiusSpeed.WrappingAdd(SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration);
        }

        if (!finishedShapes)
            return;

        // $88:8EB2 primes D=32 and timer=0. The list then installs the afterglow
        // pre-instruction; it performs the first decrement on the following frame.
        _afterglowStepsRemaining = 32;
        _afterglowTimer = NativeCountdown.Start(0);
        Phase = PowerBombExplosionPhase.Afterglow;
    }

    /// <summary>Fades the three fixed-color components on timer underflow and releases HDMA after the last step.</summary>
    private bool StepAfterglow()
    {
        _afterglowTimer = _afterglowTimer.Decrement().Remaining;
        if (!_afterglowTimer.IsNegative)
            return false;

        // CMP D--,#1 advances the instruction list without one final color decrement.
        ConsumeAfterglowStep();
        if (_afterglowStepsRemaining == 0)
        {
            Status = 0;
            PreExplosionRadius = PowerBombRadius.Zero;
            ExplosionRadius = PowerBombRadius.Zero;
            // $88:8B4E does not clear the shared radius-speed word. Retain it
            // until the next pre-explosion setup, as on the cartridge.
            Shape = null;
            RenderedShape = null;
            Phase = PowerBombExplosionPhase.Inactive;
            RenderedPhase = PowerBombExplosionPhase.Inactive;
            RenderedPreExplosionRadius = PowerBombRadius.Zero;
            RenderedExplosionRadius = PowerBombRadius.Zero;
            return true;
        }

        if (FixedColorRed != 0)
            FixedColorRed = NativeWordCounter.DecrementByte(FixedColorRed);
        if (FixedColorGreen != 0)
            FixedColorGreen = NativeWordCounter.DecrementByte(FixedColorGreen);
        if (FixedColorBlue != 0)
            FixedColorBlue = NativeWordCounter.DecrementByte(FixedColorBlue);

        ReloadAfterglowTimerLowByte();
        return false;
    }

    /// <summary>Uses the explosion curve to expand Crystal Flash only to its smaller radius limit.</summary>
    private void StepCrystalFlashExplosion()
    {
        // `$88:A552` is a deliberately shorter clone of the yellow Power Bomb expansion.
        // It uses the same curve and fixed-color table, but stops at radius `$20.00`.
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            PowerBombFixedColorSequence.Explosion,
            ExplosionRadius.WholePixels >> 3);

        ExplosionRadius = AdvanceRadiusWrapping(ExplosionRadius);
        if (ExplosionRadius.RawValue < SamusSpecialSequenceRomData.PowerBomb.CrystalFlashRadiusLimit)
        {
            RadiusSpeed = RadiusSpeed.WrappingAdd(SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration);
            return;
        }

        // The sleeping list clears the HDMA timer before installing `$88:A35D`. As with
        // the ordinary afterglow, zero underflows on its first active pre-instruction.
        _afterglowTimer = NativeCountdown.Start(0);
        _crystalFlashAfterglowStepsRemaining =
            SamusSpecialSequenceRomData.PowerBomb.CrystalFlashAfterglowSteps(checked((ushort)RenderedExplosionRadius.RawValue));
        Phase = PowerBombExplosionPhase.CrystalFlashAfterglow;
    }

    /// <summary>Fades Crystal Flash for its compiled lifetime, then clears the shared Power Bomb state.</summary>
    private bool StepCrystalFlashAfterglow()
    {
        _afterglowTimer = _afterglowTimer.Decrement().Remaining;
        if (!_afterglowTimer.IsNegative)
            return false;

        // `$88:A35D` wakes once the stock components reach zero. The compiled operand
        // tracks the same number of decrements; editable colors only affect the fade
        // displayed inside the retained final window. The byte reload still preserves
        // the underflowed high byte, so every following call remains signed-negative.
        if (_crystalFlashAfterglowStepsRemaining != 0)
        {
            _crystalFlashAfterglowStepsRemaining--;
            if (FixedColorRed != 0)
                FixedColorRed = NativeWordCounter.DecrementByte(FixedColorRed);
            if (FixedColorGreen != 0)
                FixedColorGreen = NativeWordCounter.DecrementByte(FixedColorGreen);
            if (FixedColorBlue != 0)
                FixedColorBlue = NativeWordCounter.DecrementByte(FixedColorBlue);
            ReloadAfterglowTimerLowByte();
            return false;
        }

        // The awakened list immediately calls `$88:A317`, disables both HDMA channels,
        // and clears every shared Power Bomb word. Flag is already zero from `$90:D6AE`,
        // but assigning it again mirrors the cleanup routine and makes reset atomic.
        IsArmed = false;
        Status = 0;
        PreExplosionRadius = PowerBombRadius.Zero;
        ExplosionRadius = PowerBombRadius.Zero;
        RadiusSpeed = PowerBombRadius.Zero;
        Shape = null;
        RenderedShape = null;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = PowerBombRadius.Zero;
        RenderedExplosionRadius = PowerBombRadius.Zero;
        // Replacement colors may be brighter than the stock lifetime operand. Once
        // the HDMA window is released, do not retain their unused display remainder.
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        return true;
    }

    /// <summary>
    /// Consumes one afterglow step. The afterglow phase is entered with 32 steps and ends on the step that reaches
    /// zero, so a step is never taken from zero; one would mean the phase and its count disagree.
    /// </summary>
    private void ConsumeAfterglowStep()
    {
        if (_afterglowStepsRemaining <= 0)
            throw new InvalidOperationException(
                $"Power Bomb afterglow stepped with {_afterglowStepsRemaining} steps remaining; the phase ends when the count reaches zero.");
        _afterglowStepsRemaining--;
    }

    /// <summary>Reloads only the low timer byte, retaining the underflowed high byte as native 8-bit STA does.</summary>
    private void ReloadAfterglowTimerLowByte()
    {
        // Both $88:8BD5 and $88:A3A2 execute STA with an eight-bit accumulator.
        // DEC turned zero into FFFF; retaining FF is observable, not unused scratch.
        _afterglowTimer = NativeCountdown.FromRaw((ushort)((SnesLittleEndian.HighByteInPlace(_afterglowTimer.RawValue)) |
            SamusSpecialSequenceRomData.PowerBomb.AfterglowTimerReload));
    }

    /// <summary>Loads the installed fixed-color triplet for a sequence and radius-derived index.</summary>
    private void ReadFixedColor(PowerBombFixedColorSequence sequence, int colorIndex)
    {
        (FixedColorRed, FixedColorGreen, FixedColorBlue) =
            (PresentationColors ?? throw new InvalidOperationException(
                "Power Bomb color requires installed fixed-color definitions."))
            .Resolve(sequence, colorIndex);
    }
}
