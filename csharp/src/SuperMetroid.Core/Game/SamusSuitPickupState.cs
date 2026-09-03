using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The shared bank-$88/$91 Varia- and Gravity-suit pickup transformation.
/// </summary>
/// <remarks>
/// <para>
/// The item PLM grants the equipment bit and displays bank $85's message first. Only after
/// that synchronous message returns does <c>$91:D4E4/$91:D5BA</c> center and lock Samus,
/// while one bank-$88 HDMA pre-instruction advances this seven-stage light-beam sequence
/// once per gameplay frame. This owner deliberately begins after the message rather than
/// hiding those two independently observable lifetimes behind one host animation.
/// </para>
/// <para>
/// <see cref="WindowTable"/> is the literal 256-word WH0/WH1 table produced at WRAM
/// <c>$7E:9E00</c>. Each word stores the inclusive left endpoint in its low byte and right
/// endpoint in its high byte. Keeping the actual table makes the transformation both
/// debugger-friendly and renderable without reverse-engineering its geometry twice.
/// </para>
/// </remarks>
public sealed class SamusSuitPickupState
{
    private readonly ushort[] _windowTable =
        new ushort[SamusSpecialSequenceRomData.SuitPickup.WindowScanlineCount];

    /// <summary>Whether the post-message transformation currently owns Samus input.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The suit whose native stage-three function is installed.</summary>
    public SamusSuitPickupKind Kind { get; private set; }

    /// <summary>WRAM <c>$0A72</c>, selecting one of the seven bank-$88 functions.</summary>
    public byte Substate { get; private set; }

    /// <summary>
    /// Overloaded WRAM light-beam word. Depending on the stage its bytes are horizontal
    /// endpoints or its low word is a top/bottom scanline count.
    /// </summary>
    public ushort LightBeamPosition { get; private set; }

    /// <summary>Unsigned 8.8 widening speed used by stages one, two, and four.</summary>
    public ushort LightBeamWideningSpeed { get; private set; }

    /// <summary>Raw COLDATA red byte, including its component-enable bit.</summary>
    public byte FixedColorRed { get; private set; }

    /// <summary>Raw COLDATA green byte, including its component-enable bit.</summary>
    public byte FixedColorGreen { get; private set; }

    /// <summary>Raw COLDATA blue byte, including its component-enable bit.</summary>
    public byte FixedColorBlue { get; private set; }

    /// <summary>Most recently produced inclusive WH0/WH1 endpoint pair for each scanline.</summary>
    public ReadOnlySpan<ushort> WindowTable => _windowTable;

    /// <summary>
    /// Executes <c>VariaSuitPickup</c> or <c>GravitySuitPickup</c> after its message closes.
    /// </summary>
    public void Begin(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        SamusSuitPickupKind kind)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (IsActive)
            throw new InvalidOperationException("A suit pickup transformation is already active.");

        Kind = kind;
        FixedColorRed = SamusPaletteRomData.SuitPickup.InitialRed;
        FixedColorGreen = kind == SamusSuitPickupKind.Varia
            ? SamusPaletteRomData.SuitPickup.VariaGreen
            : SamusPaletteRomData.SuitPickup.GravityGreen;
        FixedColorBlue = kind == SamusSuitPickupKind.Varia
            ? SamusPaletteRomData.SuitPickup.VariaBlue
            : SamusPaletteRomData.SuitPickup.GravityBlue;
        Substate = 0;
        LightBeamPosition = 0;
        LightBeamWideningSpeed = SamusSpecialSequenceRomData.SuitPickup.InitialWideningSpeed;
        Array.Fill(_windowTable, SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints);

        // `$91:D4F7-$D53A/$D5CD-$D610` cancels every independent speed component before
        // changing pose. CancelRunningMomentum also preserves the native departing-echo
        // side effect and publishes the immediate palette restore only when boost was live.
        samus.HorizontalSpeed.CancelRunningMomentum(samus.ReadPoseXDirection(bus));
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.AccelerationMode = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 0;
        samus.MorphBallBounceState = 0;

        // The first pose tests the OTHER suit bit. Acquiring Varia while Gravity is already
        // equipped (or vice versa) therefore starts in the suited front pose; an ordinary
        // first suit starts in power-suit pose zero until stage three performs the reveal.
        bool otherSuitAlreadyEquipped = kind == SamusSuitPickupKind.Varia
            ? samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit);
        samus.Pose = otherSuitAlreadyEquipped
            ? SamusPoseIds.ForwardFacingSuitedPose
            : SamusPoseIds.ForwardFacingPowerSuitPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.InputLocked = true;

        // Native writes whole positions but leaves fractional words untouched. These are
        // fixed screen coordinates, not a room-specific item-location approximation.
        samus.XPosition = unchecked((ushort)(layer1X + 120));
        samus.YPosition = unchecked((ushort)(layer1Y + 136));
        IsActive = true;
    }

    /// <summary>Runs one installed bank-$88 HDMA pre-instruction.</summary>
    public void Step(ISnesAddressSpace bus, SamusState samus, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(cgram);
        if (!IsActive)
            return;

        switch (Substate)
        {
            case 0:
                StepLightBeamAppears();
                return;
            case 1:
                StepUniformWidening();
                return;
            case 2:
                StepCurvedWidening(bus);
                return;
            case 3:
                RevealSuit(bus, samus, cgram);
                return;
            case 4:
                StepLightBeamShrinks();
                return;
            case 5:
                StepLightBeamDissipates();
                return;
            case 6:
                Finish(samus);
                return;
            default:
                throw new InvalidDataException($"Unknown suit-pickup substate {Substate}.");
        }
    }

    private void StepLightBeamAppears()
    {
        LightBeamPosition = unchecked((ushort)(LightBeamPosition + 8));
        int scanlineCount = unchecked((short)LightBeamPosition);
        for (int i = 0; i < scanlineCount; i++)
        {
            _windowTable[i] = SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints;
            _windowTable[_windowTable.Length - 1 - i] =
                SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints;
        }

        if (unchecked((short)(LightBeamPosition - 128)) >= 0)
        {
            Substate++;
            LightBeamPosition = SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints;
        }
    }

    private void StepUniformWidening()
    {
        byte amount = unchecked((byte)(LightBeamWideningSpeed >> 8));
        byte left = unchecked((byte)LightBeamPosition);
        byte right = unchecked((byte)(LightBeamPosition >> 8));
        left = unchecked((byte)(left - amount));
        right = unchecked((byte)(right + amount));
        LightBeamPosition = unchecked((ushort)(left | (right << 8)));
        Array.Fill(_windowTable, LightBeamPosition);

        if (unchecked((sbyte)(left - 97)) < 0)
        {
            Substate++;
            LightBeamPosition = SamusSpecialSequenceRomData.SuitPickup.CurvedWideningStart;
        }
    }

    private void StepCurvedWidening(ISnesAddressSpace bus)
    {
        AdvanceColorTowardWhite();

        byte amount = unchecked((byte)(LightBeamWideningSpeed >> 8));
        byte oldLeft = unchecked((byte)LightBeamPosition);
        byte left = unchecked((byte)(oldLeft - amount));
        byte right = unchecked((byte)(LightBeamPosition >> 8));
        if (unchecked((sbyte)(oldLeft - amount)) < 0)
        {
            left = 0;
            right = 0xff;
        }
        else
        {
            int widenedRight = right + amount;
            right = widenedRight > byte.MaxValue ? byte.MaxValue : unchecked((byte)widenedRight);
        }
        LightBeamPosition = unchecked((ushort)(left | (right << 8)));

        // `$88:E13E-$E19C` walks the 128-byte ROM curve forward for the top half, then
        // backward for the bottom half. The signed left clamp and unsigned right carry
        // saturation are intentionally asymmetric because the 65816 routine is too.
        for (int scanline = 0; scanline < _windowTable.Length; scanline++)
        {
            int curveIndex = scanline < _windowTable.Length / 2
                ? scanline
                : _windowTable.Length - 1 - scanline;
            byte curve = bus.ReadByte(
                SamusSpecialSequenceRomData.SuitPickup.BeamCurve + curveIndex);
            int candidateLeft = left - curve;
            byte curvedLeft = unchecked((sbyte)candidateLeft) < 0
                ? (byte)0
                : unchecked((byte)candidateLeft);
            int candidateRight = right + curve;
            byte curvedRight = candidateRight > byte.MaxValue
                ? byte.MaxValue
                : unchecked((byte)candidateRight);
            _windowTable[scanline] = unchecked((ushort)(curvedLeft | (curvedRight << 8)));
        }

        LightBeamWideningSpeed = unchecked((ushort)(
            LightBeamWideningSpeed + SamusSpecialSequenceRomData.SuitPickup.WideningAcceleration));
        if (LightBeamPosition == SamusSpecialSequenceRomData.SuitPickup.FullScreenEndpoints)
        {
            Substate++;
            LightBeamWideningSpeed >>= 1;
            LightBeamPosition = 0;
        }
    }

    private void RevealSuit(ISnesAddressSpace bus, SamusState samus, SnesCgram cgram)
    {
        ushort mask = Kind == SamusSuitPickupKind.Varia
            ? (ushort)SamusEquipmentFlags.VariaSuit
            : (ushort)SamusEquipmentFlags.GravitySuit;
        samus.EquippedItems |= mask;
        samus.CollectedItems |= mask;
        samus.Pose = SamusPoseIds.ForwardFacingSuitedPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.LoadSuitPalette(bus, cgram);
        Substate++;
    }

    private void StepLightBeamShrinks()
    {
        if (Kind == SamusSuitPickupKind.Gravity)
            AdvanceColorTowardBlue();
        else
            AdvanceColorTowardOrange();

        LightBeamPosition = unchecked((ushort)(
            LightBeamPosition + (LightBeamWideningSpeed >> 8)));
        int scanlineCount = unchecked((short)LightBeamPosition);
        for (int i = 0; i < scanlineCount; i++)
        {
            _windowTable[i] = SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints;
            _windowTable[_windowTable.Length - 1 - i] =
                SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints;
        }

        LightBeamWideningSpeed = unchecked((ushort)(
            LightBeamWideningSpeed - SamusSpecialSequenceRomData.SuitPickup.ShrinkingDeceleration));
        if (unchecked((short)(
            LightBeamWideningSpeed - SamusSpecialSequenceRomData.SuitPickup.MinimumShrinkingSpeed)) < 0)
        {
            LightBeamWideningSpeed =
                SamusSpecialSequenceRomData.SuitPickup.MinimumShrinkingSpeed;
        }
        if (unchecked((short)(LightBeamPosition - 128)) >= 0)
        {
            Substate++;
            LightBeamPosition = SamusSpecialSequenceRomData.SuitPickup.DissipationStart;
        }
    }

    private void StepLightBeamDissipates()
    {
        byte left = unchecked((byte)(LightBeamPosition + 8));
        byte right = unchecked((byte)((LightBeamPosition >> 8) - 8));
        LightBeamPosition = unchecked((ushort)(left | (right << 8)));
        _windowTable[128] = LightBeamPosition;
        if (unchecked((sbyte)(left - 112)) >= 0)
            Substate++;
    }

    private void Finish(SamusState samus)
    {
        FixedColorRed = SamusPaletteRomData.SuitPickup.ResetRed;
        FixedColorGreen = SamusPaletteRomData.SuitPickup.ResetGreen;
        FixedColorBlue = SamusPaletteRomData.SuitPickup.ResetBlue;
        _windowTable[0] = SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints;
        Substate = 0;
        LightBeamPosition = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        samus.InputLocked = false;
        IsActive = false;
    }

    private void AdvanceColorTowardWhite()
    {
        FixedColorRed = AddTwoAndClamp(FixedColorRed, SamusPaletteRomData.SuitPickup.WhiteRed);
        FixedColorGreen = AddTwoAndClamp(FixedColorGreen, SamusPaletteRomData.SuitPickup.WhiteGreen);
        FixedColorBlue = AddTwoAndClamp(FixedColorBlue, SamusPaletteRomData.SuitPickup.WhiteBlue);
    }

    private void AdvanceColorTowardOrange()
    {
        if (FixedColorRed != SamusPaletteRomData.SuitPickup.WhiteRed) FixedColorRed--;
        if (FixedColorGreen != SamusPaletteRomData.SuitPickup.VariaOrangeGreen) FixedColorGreen--;
        if (FixedColorBlue != SamusPaletteRomData.SuitPickup.VariaOrangeBlue) FixedColorBlue--;
    }

    private void AdvanceColorTowardBlue()
    {
        if (FixedColorRed != SamusPaletteRomData.SuitPickup.InitialRed) FixedColorRed--;
        if (FixedColorGreen != SamusPaletteRomData.SuitPickup.GravityGreen) FixedColorGreen--;
        if (FixedColorBlue != SamusPaletteRomData.SuitPickup.GravityBlue) FixedColorBlue--;
    }

    private static byte AddTwoAndClamp(byte value, byte target)
    {
        if (value == target)
            return value;
        int advanced = value + 2;
        return unchecked((byte)Math.Min(advanced, target));
    }
}

/// <summary>The two native stage-three/stage-six variants of the shared transformation.</summary>
public enum SamusSuitPickupKind : byte
{
    Varia,
    Gravity,
}
