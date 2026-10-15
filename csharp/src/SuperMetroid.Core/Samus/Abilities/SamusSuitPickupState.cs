using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

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
/// The cartridge writes the light beam into a 256-word WH0/WH1 table at WRAM <c>$7E:9E00</c>.
/// Nothing in gameplay reads that table, so this owner publishes the beam's shape, progress
/// and fixed color as <c>Flash</c> instead, and the renderer rasterizes it.
/// </para>
/// </remarks>
public sealed class SamusSuitPickupState
{
    /// <summary>Holds the setup sound request until the enemy audio owner can publish it once.</summary>
    private bool _transformationSoundPending;
    /// <summary>Native queue guard captured at suit setup, before deferred host publication.</summary>
    public bool TransformationSoundSuppressed { get; private set; }

    /// <summary>Consumes the one-shot Max6 transformation request emitted by native suit setup.</summary>
    public bool ConsumeTransformationSoundRequest()
    {
        bool pending = _transformationSoundPending;
        _transformationSoundPending = false;
        return pending;
    }

    /// <summary>Whether the post-message transformation currently owns Samus input.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// HDMA-handler calls left before the instruction list installs the pre-instruction.
    /// The handler runs an object's pre-instruction before its instructions; the list's
    /// first call ends on a one-frame table entry and its second installs the
    /// pre-instruction and sleeps ($91:D5A2/$D67A), so the first stage runs on the third.
    /// </summary>
    private int _preInstructionInstallCallsRemaining;

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

    /// <summary>Light-beam outline and progress produced by the most recent stage.</summary>
    public SamusSuitPickupBeam Beam { get; private set; }

    /// <summary>
    /// Executes <c>VariaSuitPickup</c> or <c>GravitySuitPickup</c> after its message closes.
    /// </summary>
    public void Begin(
        ISnesAddressSpace bus,
        SamusState samus,
        Position16<RoomPixels> layer1,
        SamusSuitPickupKind kind,
        bool soundSuppressed = false)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(samus);
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
        _preInstructionInstallCallsRemaining = 2;
        PublishSharedScratch(samus);
        LightBeamWideningSpeed = SamusSpecialSequenceRomData.SuitPickup.InitialWideningSpeed;
        Beam = SamusSuitPickupBeam.None;

        // `$91:D4F7-$D53A/$D5CD-$D610` cancels every independent speed component before
        // changing pose. CancelRunningMomentum also preserves the native departing-echo
        // side effect and publishes the immediate palette restore only when boost was live.
        samus.HorizontalSpeed.CancelRunningMomentum(samus.PoseMetadata.ReadFacingDirection(bus));
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.AccelerationMode = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.MorphBallBounceState = SamusBallBounceState.None;

        // The first pose tests the OTHER suit bit. Acquiring Varia while Gravity is already
        // equipped (or vice versa) therefore starts in the suited front pose; an ordinary
        // first suit starts in power-suit pose zero until stage three performs the reveal.
        bool otherSuitAlreadyEquipped = kind == SamusSuitPickupKind.Varia
            ? samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit);
        samus.Pose = otherSuitAlreadyEquipped
            ? SamusPoseId.ForwardFacingSuitedPose
            : SamusPoseId.ForwardFacingPowerSuitPose;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        // Entry's Samus command $15 publishes history before locking gameplay.
        samus.CommitPoseHistory(bus);
        samus.InputLocked = true;

        // Native writes whole positions but leaves fractional words untouched. These are
        // fixed screen coordinates, not a room-specific item-location approximation.
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(layer1.X + 120)), unchecked((ushort)(layer1.Y + 136))));
        // $91:D580/$D58D store the same words as the previous position, so this frame's
        // scrolling sees no movement.
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        samus.WritePreviousYPosition(samus.Position.Y.Whole);
        IsActive = true;
        TransformationSoundSuppressed = soundSuppressed;
        _transformationSoundPending = true;
    }

    /// <summary>Runs one installed bank-$88 HDMA pre-instruction.</summary>
    public void Step(ISnesAddressSpace bus, SamusState samus, SnesCgram cgram)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(samus);
        Ensure.NotNull(cgram);
        if (!IsActive)
            return;
        if (_preInstructionInstallCallsRemaining != 0)
        {
            _preInstructionInstallCallsRemaining--;
            return;
        }

        switch (Substate)
        {
            case 0:
                StepLightBeamAppears();
                break;
            case 1:
                StepUniformWidening();
                break;
            case 2:
                StepCurvedWidening();
                break;
            case 3:
                RevealSuit(bus, samus, cgram);
                break;
            case 4:
                StepLightBeamShrinks();
                break;
            case 5:
                StepLightBeamDissipates();
                break;
            case 6:
                Finish(samus);
                break;
            default:
                throw new InvalidDataException($"Unknown suit-pickup substate {Substate}.");
        }
        PublishSharedScratch(samus);
    }

    /// <summary>Publishes suit writes to the other owners of native $0DEC/$0DEE.</summary>
    private void PublishSharedScratch(SamusState samus)
    {
        samus.Shinespark.ApplySuitPickupScratch(Substate, LightBeamPosition);
        // Suit setup and each HDMA phase write the same word Flash later reads as
        // its ammo-decrement counter. Suspending Flash's movement does not give it
        // a private copy of that word; Draygon's escape counter aliases it too.
        samus.CrystalFlash.SetSharedAmmoCounter(Substate);
        samus.DraygonGrabbed.SetSharedEscapeCounter(Substate);
    }

    /// <summary>Extends the narrow beam inward from both screen edges until 128 rows are covered.</summary>
    private void StepLightBeamAppears()
    {
        LightBeamPosition = unchecked((ushort)(LightBeamPosition + 8));
        // The narrow beam covers this many scanlines from both the top and bottom edges.
        int scanlineCount = unchecked((short)LightBeamPosition);
        Beam = new(SamusSuitPickupBeamShape.GrowingFromEdges,
            SnesLittleEndian.LowByte(SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints),
            SnesLittleEndian.HighByte(SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints),
            scanlineCount);

        if (unchecked((short)(LightBeamPosition - 128)) >= 0)
        {
            Substate++;
            LightBeamPosition = SamusSpecialSequenceRomData.SuitPickup.NarrowBeamEndpoints;
        }
    }

    /// <summary>Moves the beam's left and right endpoints apart uniformly before curved widening begins.</summary>
    private void StepUniformWidening()
    {
        byte amount = unchecked((byte)(SnesEightEight.WholeOf(LightBeamWideningSpeed)));
        byte left = unchecked((byte)LightBeamPosition);
        byte right = SnesLittleEndian.HighByte(LightBeamPosition);
        left = unchecked((byte)(left - amount));
        right = unchecked((byte)(right + amount));
        LightBeamPosition = unchecked((ushort)(SnesLittleEndian.Word(left, right)));
        Beam = new(SamusSuitPickupBeamShape.Column, left, right, 0);

        if (unchecked((sbyte)(left - 97)) < 0)
        {
            Substate++;
            LightBeamPosition = SamusSpecialSequenceRomData.SuitPickup.CurvedWideningStart;
        }
    }

    /// <summary>Applies the mirrored ROM curve to each row as the beam widens and fixed color approaches white.</summary>
    private void StepCurvedWidening()
    {
        AdvanceColorTowardWhite();

        byte amount = unchecked((byte)(SnesEightEight.WholeOf(LightBeamWideningSpeed)));
        byte oldLeft = unchecked((byte)LightBeamPosition);
        byte left = unchecked((byte)(oldLeft - amount));
        byte right = SnesLittleEndian.HighByte(LightBeamPosition);
        // The native BEQ shares the negative-result clamp. Exact zero must also
        // force the right endpoint to 255 or the reveal is delayed by a frame.
        if (left == 0 || unchecked((sbyte)(oldLeft - amount)) < 0)
        {
            left = 0;
            right = byte.MaxValue;
        }
        else
        {
            int widenedRight = right + amount;
            right = widenedRight > byte.MaxValue ? byte.MaxValue : unchecked((byte)widenedRight);
        }
        LightBeamPosition = unchecked((ushort)(SnesLittleEndian.Word(left, right)));
        // `$88:E13E-$E19C` widens this column by the authored curve on each scanline.
        Beam = new(SamusSuitPickupBeamShape.CurvedColumn, left, right, 0);

        LightBeamWideningSpeed = unchecked((ushort)(
            LightBeamWideningSpeed + SamusSpecialSequenceRomData.SuitPickup.WideningAcceleration));
        if (LightBeamPosition == SamusSpecialSequenceRomData.SuitPickup.FullScreenEndpoints)
        {
            Substate++;
            LightBeamWideningSpeed >>= 1;
            LightBeamPosition = 0;
        }
    }

    /// <summary>Installs the equipment bit, suited front pose, and palette at the native third-stage handoff.</summary>
    private void RevealSuit(ISnesAddressSpace bus, SamusState samus, SnesCgram cgram)
    {
        ushort mask = Kind == SamusSuitPickupKind.Varia
            ? (ushort)SamusEquipmentFlags.VariaSuit
            : (ushort)SamusEquipmentFlags.GravitySuit;
        samus.EquippedItems |= mask;
        samus.CollectedItems |= mask;
        samus.Pose = SamusPoseId.ForwardFacingSuitedPose;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        // The reveal phase publishes another transition, even if the other suit
        // already selected this same front-facing pose at entry.
        samus.CommitPoseHistory(bus);
        samus.Presentation.LoadSuitPalette(bus, cgram);
        Substate++;
    }

    /// <summary>Wipes beam rows from both edges while decelerating and restoring the suit-specific fixed color.</summary>
    private void StepLightBeamShrinks()
    {
        if (Kind == SamusSuitPickupKind.Gravity)
            AdvanceColorTowardBlue();
        else
            AdvanceColorTowardOrange();

        LightBeamPosition = unchecked((ushort)(
            LightBeamPosition + (SnesEightEight.WholeOf(LightBeamWideningSpeed))));
        // The final curved column is wiped by this many scanlines from the top. Native's
        // lower do/while starts at position minus one, so the bottom wipe is one shorter.
        int scanlineCount = unchecked((short)LightBeamPosition);
        Beam = Beam with { Shape = SamusSuitPickupBeamShape.ClosingCurvedColumn, Progress = scanlineCount };

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

    /// <summary>Closes the single remaining center scanline until the native threshold ends the effect.</summary>
    private void StepLightBeamDissipates()
    {
        byte left = unchecked((byte)(LightBeamPosition + 8));
        byte right = unchecked((byte)((SnesLittleEndian.HighByte(LightBeamPosition)) - 8));
        LightBeamPosition = unchecked((ushort)(SnesLittleEndian.Word(left, right)));
        Beam = new(SamusSuitPickupBeamShape.CenterLine, left, right, 0);
        if (unchecked((sbyte)(left - 112)) >= 0)
            Substate++;
    }

    /// <summary>Clears the window and fixed-color effect, then releases Samus input after the last HDMA stage.</summary>
    private void Finish(SamusState samus)
    {
        FixedColorRed = SamusPaletteRomData.SuitPickup.ResetRed;
        FixedColorGreen = SamusPaletteRomData.SuitPickup.ResetGreen;
        FixedColorBlue = SamusPaletteRomData.SuitPickup.ResetBlue;
        Beam = SamusSuitPickupBeam.None;
        Substate = 0;
        LightBeamPosition = 0;
        FixedColorRed = 0;
        FixedColorGreen = 0;
        FixedColorBlue = 0;
        samus.InputLocked = false;
        IsActive = false;
    }

    /// <summary>Raises each COLDATA component by two toward the white reveal color.</summary>
    private void AdvanceColorTowardWhite()
    {
        FixedColorRed = AddTwoAndClamp(FixedColorRed, SamusPaletteRomData.SuitPickup.WhiteRed);
        FixedColorGreen = AddTwoAndClamp(FixedColorGreen, SamusPaletteRomData.SuitPickup.WhiteGreen);
        FixedColorBlue = AddTwoAndClamp(FixedColorBlue, SamusPaletteRomData.SuitPickup.WhiteBlue);
    }

    /// <summary>Returns the Varia beam's fixed color toward its orange target during the shrinking stage.</summary>
    private void AdvanceColorTowardOrange() => StepColorDownToward(
        SamusPaletteRomData.SuitPickup.WhiteRed,
        SamusPaletteRomData.SuitPickup.VariaOrangeGreen,
        SamusPaletteRomData.SuitPickup.VariaOrangeBlue);

    /// <summary>Returns the Gravity beam's fixed color toward its blue target during the shrinking stage.</summary>
    private void AdvanceColorTowardBlue() => StepColorDownToward(
        SamusPaletteRomData.SuitPickup.InitialRed,
        SamusPaletteRomData.SuitPickup.GravityGreen,
        SamusPaletteRomData.SuitPickup.GravityBlue);

    /// <summary>Decrements each fixed-color component by one until it reaches its target.</summary>
    private void StepColorDownToward(byte red, byte green, byte blue)
    {
        if (FixedColorRed != red) FixedColorRed = NativeWordCounter.DecrementByte(FixedColorRed);
        if (FixedColorGreen != green) FixedColorGreen = NativeWordCounter.DecrementByte(FixedColorGreen);
        if (FixedColorBlue != blue) FixedColorBlue = NativeWordCounter.DecrementByte(FixedColorBlue);
    }

    /// <summary>Advances a raw fixed-color component by two without overshooting its stage target.</summary>
    private static byte AddTwoAndClamp(byte value, byte target)
    {
        if (value == target)
            return value;
        int advanced = value + 2;
        return unchecked((byte)Math.Min(advanced, target));
    }
}
