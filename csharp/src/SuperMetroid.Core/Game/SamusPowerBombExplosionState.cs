using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    /// <summary>WRAM $0CEA. Negative means an armed/executing power bomb.</summary>
    public ushort Flag { get; private set; }

    /// <summary>WRAM $0CE2. $8000 is the normal active explosion state.</summary>
    public ushort Status { get; private set; }

    /// <summary>World X copied from the projectile when its fuse reaches zero.</summary>
    public ushort XPosition { get; private set; }

    /// <summary>World Y copied from the projectile when its fuse reaches zero.</summary>
    public ushort YPosition { get; private set; }

    /// <summary>WRAM $0CEC, the pre-explosion flash's unsigned 8.8 radius.</summary>
    public ushort PreExplosionRadius { get; private set; }

    /// <summary>WRAM $0CEE, the damaging explosion's unsigned 8.8 radius.</summary>
    public ushort ExplosionRadius { get; private set; }

    /// <summary>
    /// WRAM $0CF0, deliberately shared by the pre-explosion and explosion phases.
    /// </summary>
    public ushort RadiusSpeed { get; private set; }

    /// <summary>Current ROM-authored 192-byte ellipse shape table, when one is active.</summary>
    public ushort ShapeDefinitionPointer { get; private set; }

    /// <summary>
    /// Shape table consumed by the current rendered frame before the native pointer advanced.
    /// </summary>
    public ushort RenderedShapeDefinitionPointer { get; private set; }

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
    public PowerBombExplosionPhase RenderedPhase { get; private set; }

    /// <summary>Pre-explosion radius consumed before the native end-of-frame update.</summary>
    public ushort RenderedPreExplosionRadius { get; private set; }

    /// <summary>Explosion radius consumed before the native end-of-frame update.</summary>
    public ushort RenderedExplosionRadius { get; private set; }

    /// <summary>Current fixed-color red component written through COLDATA.</summary>
    public byte FixedColorRed { get; private set; }

    /// <summary>Current fixed-color green component written through COLDATA.</summary>
    public byte FixedColorGreen { get; private set; }

    /// <summary>Current fixed-color blue component written through COLDATA.</summary>
    public byte FixedColorBlue { get; private set; }

    /// <summary>True from placement until cleanup releases WRAM $0CEA.</summary>
    public bool IsArmed => (Flag & 0x8000) != 0;

    /// <summary>True while the bank-$88 color-math animation is running.</summary>
    public bool IsActive => (Status & 0x8000) != 0;

    // $19D8 is initialized to zero by HDMA allocation. Stage five performs a wrapping
    // decrement, so zero immediately underflows and permits its first fade step.
    private ushort _afterglowTimer;

    // $19B4's low byte is initialized to 32 by the last white-explosion frame.
    private byte _afterglowStepsRemaining;

    /// <summary>
    /// Implements $90:BF9D's early power-bomb lock before a projectile slot is initialized.
    /// </summary>
    public void Arm()
    {
        Flag = 0xffff;
    }

    /// <summary>
    /// Implements $88:8AA4 after $90:C157 has copied the projectile's world position.
    /// </summary>
    public void Spawn(ushort xPosition, ushort yPosition)
    {
        XPosition = xPosition;
        YPosition = yPosition;
        Status = 0x8000;

        // The first object executes CallFar($88:8B14), installs pre-instruction $90DF,
        // and sleeps. HDMA processing precedes Samus/projectile processing in the native
        // frame, so the pre-instruction itself does not run until the following frame.
        PreExplosionRadius = SamusSpecialSequenceRomData.PowerBomb.InitialRadius;
        ExplosionRadius = 0;
        RadiusSpeed = SamusSpecialSequenceRomData.PowerBomb.InitialPreExplosionSpeed;
        ShapeDefinitionPointer = 0;
        RenderedShapeDefinitionPointer = 0;
        Phase = PowerBombExplosionPhase.PreExplosionWhite;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = 0;
        RenderedExplosionRadius = 0;
        _afterglowTimer = 0;
        _afterglowStepsRemaining = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
    }

    /// <summary>
    /// Implements `$90:D6AE-$D6C5` and both setup calls at `$88:A2E4/$A309` after
    /// Crystal Flash completes its ten-step rise.
    /// </summary>
    public void BeginCrystalFlash(ushort xPosition, ushort yPosition)
    {
        // `$90:D6AE` releases the one-at-a-time Power Bomb lock before spawning the two
        // Crystal Flash HDMA objects. Their animation still reuses the same status,
        // position, radius, speed, fixed-color, and window-table WRAM words.
        Flag = 0;
        XPosition = xPosition;
        YPosition = yPosition;
        Status = 0x8000;
        PreExplosionRadius = SamusSpecialSequenceRomData.PowerBomb.InitialRadius;
        ExplosionRadius = SamusSpecialSequenceRomData.PowerBomb.InitialRadius;
        RadiusSpeed = 0;
        ShapeDefinitionPointer = 0;
        RenderedShapeDefinitionPointer = 0;
        Phase = PowerBombExplosionPhase.CrystalFlashExplosion;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = 0;
        RenderedExplosionRadius = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        _afterglowTimer = 0;
        _afterglowStepsRemaining = 0;
    }

    /// <summary>
    /// Runs one invocation of the active bank-$88 HDMA pre-instruction.
    /// </summary>
    /// <returns>True only on the frame that $88:8B4E completes cleanup.</returns>
    public bool StepFrame(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsActive)
            return false;

        // Every bank-$88 pre-instruction first builds this frame's window and only then
        // mutates its radius/pointer or advances the sleeping instruction list. Record
        // that input phase here so post-logic desktop composition sees the generated
        // frame rather than the state prepared for the following one.
        RenderedPhase = Phase;

        switch (Phase)
        {
            case PowerBombExplosionPhase.PreExplosionWhite:
                StepPreExplosionWhite(bus);
                return false;

            case PowerBombExplosionPhase.PreExplosionYellow:
                StepPreExplosionYellow(bus);
                return false;

            case PowerBombExplosionPhase.ExplosionYellow:
                StepExplosionYellow(bus);
                return false;

            case PowerBombExplosionPhase.ExplosionWhite:
                StepExplosionWhite(bus);
                return false;

            case PowerBombExplosionPhase.Afterglow:
                return StepAfterglow();

            case PowerBombExplosionPhase.CrystalFlashExplosion:
                StepCrystalFlashExplosion(bus);
                return false;

            case PowerBombExplosionPhase.CrystalFlashAfterglow:
                return StepCrystalFlashAfterglow();

            default:
                throw new InvalidOperationException($"Active power-bomb status has invalid phase {Phase}.");
        }
    }

    /// <summary>
    /// Releases the armed flag after cleanup when Crystal Flash did not take ownership.
    /// </summary>
    public void ReleaseFlag() => Flag = 0;

    /// <summary>Clears every bank-$88/WRAM field during room/runtime reset.</summary>
    public void Reset()
    {
        Flag = 0;
        Status = 0;
        XPosition = 0;
        YPosition = 0;
        PreExplosionRadius = 0;
        ExplosionRadius = 0;
        RadiusSpeed = 0;
        ShapeDefinitionPointer = 0;
        RenderedShapeDefinitionPointer = 0;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = 0;
        RenderedExplosionRadius = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        _afterglowTimer = 0;
        _afterglowStepsRemaining = 0;
    }

    /// <summary>
    /// Retains the old runtime debugger seam used to force X-ray's status precondition.
    /// A forced status does not fabricate an animation phase or armed projectile.
    /// </summary>
    internal void SetStatusForDebugging(ushort value)
    {
        Status = value;
        if (value == 0 && Phase != PowerBombExplosionPhase.Inactive)
            Phase = PowerBombExplosionPhase.Inactive;
    }

    private void StepPreExplosionWhite(ISnesAddressSpace bus)
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        ReadFixedColor(
            bus,
            SamusPaletteRomData.PowerBomb.PreExplosionColors,
            ((PreExplosionRadius >> 8) >> 3) & 0x0f);

        // $88:90DF uses wrapping 16-bit addition. It subtracts acceleration only while
        // the newly computed radius is still below $9200.
        PreExplosionRadius = unchecked((ushort)(PreExplosionRadius + RadiusSpeed));
        if (PreExplosionRadius < SamusSpecialSequenceRomData.PowerBomb.PreExplosionWhiteLimit)
        {
            RadiusSpeed = unchecked((ushort)(
                RadiusSpeed - SamusSpecialSequenceRomData.PowerBomb.PreExplosionAcceleration));
            return;
        }

        // Advancing past Sleep executes CallFar($8B32), installs pre-instruction $91A8,
        // and sleeps again during this same HDMA-object-handler pass.
        ShapeDefinitionPointer = SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape;
        Phase = PowerBombExplosionPhase.PreExplosionYellow;
    }

    private void StepPreExplosionYellow(ISnesAddressSpace bus)
    {
        RenderedPreExplosionRadius = PreExplosionRadius;
        ReadFixedColor(
            bus,
            SamusPaletteRomData.PowerBomb.PreExplosionColors,
            ((PreExplosionRadius >> 8) >> 3) & 0x0f);

        // $91A8 builds this frame's two 192-byte window tables before incrementing the
        // source pointer. Preserve that consumed pointer for a host compositor running
        // after the logic frame has completed.
        RenderedShapeDefinitionPointer = ShapeDefinitionPointer;
        ShapeDefinitionPointer = unchecked((ushort)(
            ShapeDefinitionPointer + SamusSpecialSequenceRomData.PowerBomb.ShapeStride));
        bool finishedShapes =
            ShapeDefinitionPointer == SamusSpecialSequenceRomData.PowerBomb.YellowShapeEnd;

        // The carry test is performed on the sum before either state word is changed.
        // If it would exceed $FFFF, both radius and speed remain frozen for this frame.
        int nextRadius = PreExplosionRadius + RadiusSpeed;
        if (nextRadius < 0x10000)
        {
            PreExplosionRadius = (ushort)nextRadius;
            RadiusSpeed = unchecked((ushort)(
                RadiusSpeed - SamusSpecialSequenceRomData.PowerBomb.PreExplosionAcceleration));
        }

        if (!finishedShapes)
            return;

        // CallFar($8B39) initializes the actual damaging radius before the next frame's
        // ExplosionYellow pre-instruction. PreExplosionRadius remains debugger-visible.
        ExplosionRadius = SamusSpecialSequenceRomData.PowerBomb.InitialRadius;
        RadiusSpeed = 0;
        Phase = PowerBombExplosionPhase.ExplosionYellow;
    }

    private void StepExplosionYellow(ISnesAddressSpace bus)
    {
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            bus,
            SamusPaletteRomData.PowerBomb.ExplosionColors,
            (ExplosionRadius >> 8) >> 3);

        ExplosionRadius = unchecked((ushort)(ExplosionRadius + RadiusSpeed));
        if (ExplosionRadius < SamusSpecialSequenceRomData.PowerBomb.ExplosionYellowLimit)
        {
            RadiusSpeed = unchecked((ushort)(
                RadiusSpeed + SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration));
            return;
        }

        // The next instruction calls $8B47, selecting the first of seventeen expanding
        // white shape tables, then installs $8EB2 and sleeps.
        ShapeDefinitionPointer = SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape;
        RenderedShapeDefinitionPointer = 0;
        Phase = PowerBombExplosionPhase.ExplosionWhite;
    }

    private void StepExplosionWhite(ISnesAddressSpace bus)
    {
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            bus,
            SamusPaletteRomData.PowerBomb.ExplosionColors,
            (ExplosionRadius >> 8) >> 3);

        RenderedShapeDefinitionPointer = ShapeDefinitionPointer;
        ShapeDefinitionPointer = unchecked((ushort)(
            ShapeDefinitionPointer + SamusSpecialSequenceRomData.PowerBomb.ShapeStride));
        bool finishedShapes =
            ShapeDefinitionPointer == SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd;

        int nextRadius = ExplosionRadius + RadiusSpeed;
        if (nextRadius < 0x10000)
        {
            ExplosionRadius = (ushort)nextRadius;
            RadiusSpeed = unchecked((ushort)(
                RadiusSpeed + SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration));
        }

        if (!finishedShapes)
            return;

        // $88:8EB2 primes D=32 and timer=0. The list then installs the afterglow
        // pre-instruction; it performs the first decrement on the following frame.
        _afterglowStepsRemaining = 32;
        _afterglowTimer = 0;
        Phase = PowerBombExplosionPhase.Afterglow;
    }

    private bool StepAfterglow()
    {
        _afterglowTimer = unchecked((ushort)(_afterglowTimer - 1));
        if ((_afterglowTimer & 0x8000) == 0)
            return false;

        // CMP D--,#1 advances the instruction list without one final color decrement.
        if (_afterglowStepsRemaining-- == 1)
        {
            Status = 0;
            PreExplosionRadius = 0;
            ExplosionRadius = 0;
            RadiusSpeed = 0;
            ShapeDefinitionPointer = 0;
            RenderedShapeDefinitionPointer = 0;
            Phase = PowerBombExplosionPhase.Inactive;
            RenderedPhase = PowerBombExplosionPhase.Inactive;
            RenderedPreExplosionRadius = 0;
            RenderedExplosionRadius = 0;
            return true;
        }

        if (FixedColorRed != 0)
            FixedColorRed--;
        if (FixedColorGreen != 0)
            FixedColorGreen--;
        if (FixedColorBlue != 0)
            FixedColorBlue--;

        ReloadAfterglowTimerLowByte();
        return false;
    }

    private void StepCrystalFlashExplosion(ISnesAddressSpace bus)
    {
        // `$88:A552` is a deliberately shorter clone of the yellow Power Bomb expansion.
        // It uses the same curve and fixed-color table, but stops at radius `$20.00`.
        RenderedExplosionRadius = ExplosionRadius;
        ReadFixedColor(
            bus,
            SamusPaletteRomData.PowerBomb.ExplosionColors,
            (ExplosionRadius >> 8) >> 3);

        ExplosionRadius = unchecked((ushort)(ExplosionRadius + RadiusSpeed));
        if (ExplosionRadius < SamusSpecialSequenceRomData.PowerBomb.CrystalFlashRadiusLimit)
        {
            RadiusSpeed = unchecked((ushort)(
                RadiusSpeed + SamusSpecialSequenceRomData.PowerBomb.ExplosionAcceleration));
            return;
        }

        // The sleeping list clears the HDMA timer before installing `$88:A35D`. As with
        // the ordinary afterglow, zero underflows on its first active pre-instruction.
        _afterglowTimer = 0;
        Phase = PowerBombExplosionPhase.CrystalFlashAfterglow;
    }

    private bool StepCrystalFlashAfterglow()
    {
        _afterglowTimer = unchecked((ushort)(_afterglowTimer - 1));
        if ((_afterglowTimer & 0x8000) == 0)
            return false;

        // Unlike ordinary Power Bomb stage five, `$88:A35D` uses the three current color
        // components as its completion criterion. This retains the final radius window
        // while each nonzero component fades. The byte reload retains the underflowed
        // high byte, so the signed timer remains negative on the next call.
        if ((FixedColorRed | FixedColorGreen | FixedColorBlue) != 0)
        {
            if (FixedColorRed != 0)
                FixedColorRed--;
            if (FixedColorGreen != 0)
                FixedColorGreen--;
            if (FixedColorBlue != 0)
                FixedColorBlue--;
            ReloadAfterglowTimerLowByte();
            return false;
        }

        // The awakened list immediately calls `$88:A317`, disables both HDMA channels,
        // and clears every shared Power Bomb word. Flag is already zero from `$90:D6AE`,
        // but assigning it again mirrors the cleanup routine and makes reset atomic.
        Flag = 0;
        Status = 0;
        PreExplosionRadius = 0;
        ExplosionRadius = 0;
        RadiusSpeed = 0;
        ShapeDefinitionPointer = 0;
        RenderedShapeDefinitionPointer = 0;
        Phase = PowerBombExplosionPhase.Inactive;
        RenderedPhase = PowerBombExplosionPhase.Inactive;
        RenderedPreExplosionRadius = 0;
        RenderedExplosionRadius = 0;
        return true;
    }

    private void ReloadAfterglowTimerLowByte()
    {
        // Both $88:8BD5 and $88:A3A2 execute STA with an eight-bit accumulator.
        // DEC turned zero into FFFF; retaining FF is observable, not unused scratch.
        _afterglowTimer = (ushort)((_afterglowTimer & 0xff00) |
            SamusSpecialSequenceRomData.PowerBomb.AfterglowTimerReload);
    }

    private void ReadFixedColor(ISnesAddressSpace bus, int tableAddress, int colorIndex)
    {
        int address = tableAddress + colorIndex * SamusPaletteRomData.PowerBomb.BytesPerColor;
        FixedColorRed = (byte)(bus.ReadByte(address) & SamusPaletteRomData.PowerBomb.ComponentMask);
        FixedColorGreen = (byte)(bus.ReadByte(address + 1) & SamusPaletteRomData.PowerBomb.ComponentMask);
        FixedColorBlue = (byte)(bus.ReadByte(address + 2) & SamusPaletteRomData.PowerBomb.ComponentMask);
    }
}

/// <summary>The five sleeping-list phases beginning at ROM $88:8ACE.</summary>
public enum PowerBombExplosionPhase
{
    Inactive,
    PreExplosionWhite,
    PreExplosionYellow,
    ExplosionYellow,
    ExplosionWhite,
    Afterglow,
    CrystalFlashExplosion,
    CrystalFlashAfterglow,
}
