using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The shared bank-$88/$91 Varia- and Gravity-suit pickup transformation.
/// </summary>
/// <remarks>
/// <para>
/// The item PLM grants the equipment bit and displays bank $85's message first. Only after
/// that synchronous message returns does <c>$91:D4E4/$91:D5BA</c> center and lock Samus,
/// while the light beam's per-line display effect advances this seven-stage sequence
/// once per gameplay frame. This owner deliberately begins after the message rather than
/// hiding those two independently observable lifetimes behind one host animation.
/// </para>
/// <para>
/// Nothing in gameplay reads how the light beam is drawn, so this owner publishes the beam's
/// shape, progress and fixed color as <c>Flash</c>, and the display capture describes it as
/// window shapes the backend draws (#1468).
/// </para>
/// <para>
/// The stage and the light beam's progress share two scratch words with the shinespark's
/// vertical acceleration, Crystal Flash's ammunition timer and Draygon's escape counter.
/// <see cref="PublishSharedScratch"/> writes the exact values those owners read after every stage call.
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
    /// Effect updates left before the light beam's stage step is installed.
    /// The handler runs an object's pre-instruction before its instructions; the list's
    /// first call ends on a one-frame table entry and its second installs the
    /// pre-instruction and sleeps ($91:D5A2/$D67A), so the first stage runs on the third.
    /// </summary>
    private int _preInstructionInstallCallsRemaining;

    /// <summary>The suit whose native stage-three function is installed.</summary>
    public SamusSuitPickupKind Kind { get; private set; }

    /// <summary>The light-beam stage the next pre-instruction call runs; it returns to the first stage when the transformation finishes.</summary>
    public SamusSuitPickupStage Stage { get; private set; }

    /// <summary>
    /// Scanlines the beam has covered from each screen edge while it appears, or wiped while it shrinks: zero at
    /// setup and again from the end of the curved widening until the shrinking stage starts, and never above 255.
    /// </summary>
    private int _lightBeamScanlines;

    /// <summary>
    /// The beam's inclusive left screen column while it is a column or centre line (the uniform, curved,
    /// dissipating and finishing stages); only X is meaningful. Its arithmetic wraps at the eight-bit window column.
    /// </summary>
    private Position16<ScreenPixels> _lightBeamLeft;

    /// <summary>The beam's inclusive right screen column in the same stages as <see cref="_lightBeamLeft"/>; only X is meaningful.</summary>
    private Position16<ScreenPixels> _lightBeamRight;

    /// <summary>
    /// The unsigned 8.8 speed, in screen pixels per update, at which the beam's edges move apart or its wipe
    /// advances; only X is meaningful. It starts at one pixel, accelerates during the curved widening, is halved at
    /// the reveal and decelerates to a one-pixel minimum while the beam shrinks.
    /// </summary>
    public EightEightVector<ScreenPixels> LightBeamWideningVelocity { get; private set; }

    /// <summary>The colour the light beam adds inside its window.</summary>
    public RgbColor FixedColor { get; private set; }

    /// <summary>Light-beam outline and progress produced by the most recent stage.</summary>
    public SamusSuitPickupBeam Beam { get; private set; }

    /// <summary>
    /// Executes <c>VariaSuitPickup</c> or <c>GravitySuitPickup</c> after its message closes.
    /// </summary>
    public void Begin(
        SamusState samus,
        Position16<RoomPixels> layer1,
        SamusSuitPickupKind kind,
        bool soundSuppressed = false)
    {
        Ensure.NotNull(samus);
        if (IsActive)
            throw new InvalidOperationException("A suit pickup transformation is already active.");

        Kind = kind;
        FixedColor = kind == SamusSuitPickupKind.Varia
            ? new RgbColor(SamusPaletteDefinitions.SuitPickup.InitialRed, SamusPaletteDefinitions.SuitPickup.VariaGreen, SamusPaletteDefinitions.SuitPickup.VariaBlue)
            : new RgbColor(SamusPaletteDefinitions.SuitPickup.InitialRed, SamusPaletteDefinitions.SuitPickup.GravityGreen, SamusPaletteDefinitions.SuitPickup.GravityBlue);
        Stage = SamusSuitPickupStage.LightBeamAppears;
        _lightBeamScanlines = 0;
        _lightBeamLeft = Position16<ScreenPixels>.Zero;
        _lightBeamRight = Position16<ScreenPixels>.Zero;
        _preInstructionInstallCallsRemaining = 2;
        PublishSharedScratch(samus);
        LightBeamWideningVelocity = WideningVelocity(SuitPickupBeamDefinitions.InitialWideningSpeed);
        Beam = SamusSuitPickupBeam.None;

        // `$91:D4F7-$D53A/$D5CD-$D610` cancels every independent speed component before
        // changing pose. CancelRunningMomentum also preserves the native departing-echo
        // side effect and publishes the immediate palette restore only when boost was live.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.Kinematics.YSpeed = default;
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
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Entry's Samus command $15 publishes history before locking gameplay.
        samus.CommitPoseHistory();
        samus.Control.LockForSuitPickup();

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

    /// <summary>Runs one update of the installed light-beam stage step.</summary>
    public void Step(SamusState samus, SamusColours colours)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(colours);
        if (!IsActive)
            return;
        if (_preInstructionInstallCallsRemaining != 0)
        {
            _preInstructionInstallCallsRemaining--;
            return;
        }

        switch (Stage)
        {
            case SamusSuitPickupStage.LightBeamAppears:
                StepLightBeamAppears();
                break;
            case SamusSuitPickupStage.UniformWidening:
                StepUniformWidening();
                break;
            case SamusSuitPickupStage.CurvedWidening:
                StepCurvedWidening();
                break;
            case SamusSuitPickupStage.RevealSuit:
                RevealSuit(samus, colours);
                break;
            case SamusSuitPickupStage.LightBeamShrinks:
                StepLightBeamShrinks();
                break;
            case SamusSuitPickupStage.LightBeamDissipates:
                StepLightBeamDissipates();
                break;
            case SamusSuitPickupStage.Finish:
                Finish(samus);
                break;
            default:
                throw new InvalidDataException($"Unknown suit-pickup stage {Stage}.");
        }
        PublishSharedScratch(samus);
    }

    /// <summary>
    /// Publishes suit writes to the other owners of native $0DEC/$0DEE: the stage number, and the light beam's
    /// progress word (its scanline count, or its left column in the low byte and right column in the high byte).
    /// </summary>
    private void PublishSharedScratch(SamusState samus)
    {
        int stageNumber = SharedScratchStageNumber(Stage);
        samus.Shinespark.ApplySuitPickupScratch(stageNumber, SharedScratchLightBeamWord());
        // Suit setup and each light-beam stage write the same word Flash later reads as
        // its ammo-decrement counter. Suspending Flash's movement does not give it
        // a private copy of that word; Draygon's escape counter aliases it too.
        samus.CrystalFlash.SetSharedAmmoCounter(stageNumber);
        samus.DraygonGrabbed.SetSharedEscapeCounter(stageNumber);
    }

    /// <summary>The number, 0 through 6 in stage order, the shared scratch word holds for <paramref name="stage"/>.</summary>
    private static ushort SharedScratchStageNumber(SamusSuitPickupStage stage) => stage switch
    {
        SamusSuitPickupStage.LightBeamAppears => 0,
        SamusSuitPickupStage.UniformWidening => 1,
        SamusSuitPickupStage.CurvedWidening => 2,
        SamusSuitPickupStage.RevealSuit => 3,
        SamusSuitPickupStage.LightBeamShrinks => 4,
        SamusSuitPickupStage.LightBeamDissipates => 5,
        SamusSuitPickupStage.Finish => 6,
        _ => throw new InvalidDataException($"Unknown suit-pickup stage {stage}."),
    };

    /// <summary>
    /// The light beam's progress the shared scratch holds for a later shinespark (<see cref="SuitPickupSparkCarryover"/>):
    /// the lines grown or wiped while the beam appears, during the reveal and while it shrinks; its edges while it is a
    /// column or centre line.
    /// </summary>
    private int SharedScratchLightBeamWord() => Stage switch
    {
        SamusSuitPickupStage.LightBeamAppears or SamusSuitPickupStage.RevealSuit or
            SamusSuitPickupStage.LightBeamShrinks => SuitPickupSparkCarryover.OfLines(_lightBeamScanlines),
        SamusSuitPickupStage.UniformWidening or SamusSuitPickupStage.CurvedWidening or
            SamusSuitPickupStage.LightBeamDissipates or SamusSuitPickupStage.Finish =>
            SuitPickupSparkCarryover.OfEdges(_lightBeamLeft.X, _lightBeamRight.X),
        _ => throw new InvalidDataException($"Unknown suit-pickup stage {Stage}."),
    };

    /// <summary>Extends the narrow beam inward from both screen edges until 128 rows are covered.</summary>
    private void StepLightBeamAppears()
    {
        // The count grows from zero by eight to exactly 128, so the cartridge's signed
        // comparison with 128 is this ordinary one.
        _lightBeamScanlines += 8;
        Beam = new(SamusSuitPickupBeamShape.GrowingFromEdges,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Left,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Right,
            _lightBeamScanlines);

        if (_lightBeamScanlines >= 128)
        {
            Stage = SamusSuitPickupStage.UniformWidening;
            SetLightBeamEdges(SuitPickupBeamDefinitions.NarrowBeamEdges);
        }
    }

    /// <summary>Moves the beam's left and right endpoints apart uniformly before curved widening begins.</summary>
    private void StepUniformWidening()
    {
        int amount = unchecked((byte)EightEight.WholeOf(LightBeamWideningVelocity.X.RawValue));
        // Window columns are eight bits wide: both edges wrap within the screen row.
        int left = unchecked((byte)(checked((byte)_lightBeamLeft.X) - amount));
        int right = unchecked((byte)(checked((byte)_lightBeamRight.X) + amount));
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.Column, _lightBeamLeft, _lightBeamRight, 0);

        if (unchecked((sbyte)(left - 97)) < 0)
        {
            Stage = SamusSuitPickupStage.CurvedWidening;
            SetLightBeamEdges(SuitPickupBeamDefinitions.CurvedWideningStart);
        }
    }

    /// <summary>Applies the mirrored ROM curve to each row as the beam widens and fixed color approaches white.</summary>
    private void StepCurvedWidening()
    {
        AdvanceColorTowardWhite();

        int amount = unchecked((byte)EightEight.WholeOf(LightBeamWideningVelocity.X.RawValue));
        int oldLeft = checked((byte)_lightBeamLeft.X);
        int left = unchecked((byte)(oldLeft - amount));
        int right = checked((byte)_lightBeamRight.X);
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
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        // `$88:E13E-$E19C` widens this column by the authored curve on each scanline.
        Beam = new(SamusSuitPickupBeamShape.CurvedColumn, _lightBeamLeft, _lightBeamRight, 0);

        LightBeamWideningVelocity += WideningVelocity(SuitPickupBeamDefinitions.WideningAcceleration);
        if (SuitPickupBeamEdges.Columns(left, right) == SuitPickupBeamDefinitions.FullScreenWindow)
        {
            Stage = SamusSuitPickupStage.RevealSuit;
            // The unsigned speed halves, truncating its lowest fraction bit.
            LightBeamWideningVelocity = WideningVelocity(unchecked((ushort)(LightBeamWideningVelocity.X.RawValue / 2)));
            _lightBeamScanlines = 0;
        }
    }

    /// <summary>Installs the equipment bit, suited front pose, and palette at the native third-stage handoff.</summary>
    private void RevealSuit(SamusState samus, SamusColours colours)
    {
        SamusEquipmentFlags mask = Kind == SamusSuitPickupKind.Varia
            ? SamusEquipmentFlags.VariaSuit
            : SamusEquipmentFlags.GravitySuit;
        samus.EquippedItems |= mask;
        samus.CollectedItems |= mask;
        samus.Pose = SamusPoseId.ForwardFacingSuitedPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // The reveal phase publishes another transition, even if the other suit
        // already selected this same front-facing pose at entry.
        samus.CommitPoseHistory();
        samus.Presentation.LoadSuitPalette(samus, colours);
        Stage = SamusSuitPickupStage.LightBeamShrinks;
    }

    /// <summary>Wipes beam rows from both edges while decelerating and restoring the suit-specific fixed color.</summary>
    private void StepLightBeamShrinks()
    {
        if (Kind == SamusSuitPickupKind.Gravity)
            AdvanceColorTowardBlue();
        else
            AdvanceColorTowardOrange();

        // The wipe grows from zero by the speed's whole pixels and stops once it reaches 128, so it
        // stays far inside the range where the cartridge's signed comparison is this ordinary one.
        _lightBeamScanlines += EightEight.WholeOf(LightBeamWideningVelocity.X.RawValue);
        // The final curved column is wiped by this many scanlines from the top. Native's
        // lower do/while starts at position minus one, so the bottom wipe is one shorter.
        Beam = Beam with { Shape = SamusSuitPickupBeamShape.ClosingCurvedColumn, Progress = _lightBeamScanlines };

        int decelerated = unchecked((ushort)(
            LightBeamWideningVelocity.X.RawValue - SuitPickupBeamDefinitions.ShrinkingDeceleration));
        LightBeamWideningVelocity = WideningVelocity(decelerated);
        if (unchecked((short)(decelerated - SuitPickupBeamDefinitions.MinimumShrinkingSpeed)) < 0)
            LightBeamWideningVelocity = WideningVelocity(SuitPickupBeamDefinitions.MinimumShrinkingSpeed);
        if (_lightBeamScanlines >= 128)
        {
            Stage = SamusSuitPickupStage.LightBeamDissipates;
            SetLightBeamEdges(SuitPickupBeamDefinitions.DissipationStart);
        }
    }

    /// <summary>Closes the single remaining center scanline until the native threshold ends the effect.</summary>
    private void StepLightBeamDissipates()
    {
        // Window columns are eight bits wide: the starting left column 255 wraps to 7 on the first call.
        int left = unchecked((byte)(checked((byte)_lightBeamLeft.X) + 8));
        int right = unchecked((byte)(checked((byte)_lightBeamRight.X) - 8));
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.CenterLine, _lightBeamLeft, _lightBeamRight, 0);
        if (unchecked((sbyte)(left - 112)) >= 0)
            Stage = SamusSuitPickupStage.Finish;
    }

    /// <summary>Clears the window and fixed-color effect, then releases Samus input after the last light-beam stage.</summary>
    private void Finish(SamusState samus)
    {
        FixedColor = new RgbColor(SamusPaletteDefinitions.SuitPickup.ResetRed, SamusPaletteDefinitions.SuitPickup.ResetGreen,
            SamusPaletteDefinitions.SuitPickup.ResetBlue);
        Beam = SamusSuitPickupBeam.None;
        Stage = SamusSuitPickupStage.LightBeamAppears;
        _lightBeamScanlines = 0;
        _lightBeamLeft = Position16<ScreenPixels>.Zero;
        _lightBeamRight = Position16<ScreenPixels>.Zero;
        FixedColor = RgbColor.Black;
        samus.Control.Unlock();
        IsActive = false;
    }

    /// <summary>Starts a column stage at the authored edge pair.</summary>
    private void SetLightBeamEdges(SuitPickupBeamEdges edges)
    {
        _lightBeamLeft = edges.Left;
        _lightBeamRight = edges.Right;
    }

    /// <summary>The screen position of window column <paramref name="column"/>; only X is meaningful.</summary>
    private static Position16<ScreenPixels> ScreenColumn(int column) => Position16<ScreenPixels>.Of(column, 0);

    /// <summary>A horizontal unsigned 8.8 speed from its raw word; the vertical axis is unused.</summary>
    private static EightEightVector<ScreenPixels> WideningVelocity(int speed) =>
        new(new SignedEightEight(checked((ushort)speed)), new SignedEightEight(0));

    /// <summary>Raises each channel by two toward the white reveal colour.</summary>
    private void AdvanceColorTowardWhite() =>
        FixedColor = new RgbColor(AddTwoAndClamp(FixedColor.Red, SamusPaletteDefinitions.SuitPickup.WhiteRed),
            AddTwoAndClamp(FixedColor.Green, SamusPaletteDefinitions.SuitPickup.WhiteGreen),
            AddTwoAndClamp(FixedColor.Blue, SamusPaletteDefinitions.SuitPickup.WhiteBlue));

    /// <summary>Returns the Varia beam's fixed color toward its orange target during the shrinking stage.</summary>
    private void AdvanceColorTowardOrange() => StepColorDownToward(
        SamusPaletteDefinitions.SuitPickup.WhiteRed,
        SamusPaletteDefinitions.SuitPickup.VariaOrangeGreen,
        SamusPaletteDefinitions.SuitPickup.VariaOrangeBlue);

    /// <summary>Returns the Gravity beam's fixed color toward its blue target during the shrinking stage.</summary>
    private void AdvanceColorTowardBlue() => StepColorDownToward(
        SamusPaletteDefinitions.SuitPickup.InitialRed,
        SamusPaletteDefinitions.SuitPickup.GravityGreen,
        SamusPaletteDefinitions.SuitPickup.GravityBlue);

    /// <summary>Lowers each channel by one until it reaches its target.</summary>
    private void StepColorDownToward(int red, int green, int blue) =>
        FixedColor = new RgbColor(FixedColor.Red == red ? red : FixedColor.Red - 1, FixedColor.Green == green ? green : FixedColor.Green - 1,
            FixedColor.Blue == blue ? blue : FixedColor.Blue - 1);

    /// <summary>Advances a channel by two without overshooting its stage target.</summary>
    private static int AddTwoAndClamp(int value, int target)
    {
        if (value == target)
            return value;
        return Math.Min(value + 2, target);
    }
}
