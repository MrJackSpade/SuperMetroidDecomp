using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The shared Varia- and Gravity-suit pickup transformation.
/// </summary>
/// <remarks>
/// <para>
/// The item room object grants the equipment bit and displays message first. Only after
/// that synchronous message returns does the suit pickup center and lock Samus,
/// while the light beam's per-line display effect advances this seven-stage sequence
/// once per gameplay frame. This owner deliberately begins after the message rather than
/// hiding those two independently observable lifetimes behind one host animation.
/// </para>
/// <para>
/// Nothing in gameplay reads how the light beam is drawn, so this owner publishes the beam's
/// shape, progress and colour as <c>Flash</c>, and the display capture describes it as
/// window shapes the backend draws (#1468).
/// </para>
/// <para>
/// The pickup leaves its stage and its light beam's progress at its setup and after every stage call
/// (<see cref="LeaveProgress"/>): both in the shinespark's upward acceleration (<see cref="SuitPickupSparkCarryover"/>),
/// and the stage on the count Crystal Flash's ammunition drain and Draygon's escape share
/// (<see cref="SharedEscapeAndDrainCount"/>).
/// </para>
/// </remarks>
public sealed class SamusSuitPickupState
{
    /// <summary>Whether the post-message transformation currently owns Samus input.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Effect updates left before the light beam's stage step is installed.
    /// The handler runs an object's frame behaviour before its program; the program's
    /// first call ends on a one-frame table entry and its second installs the
    /// frame behaviour and sleeps, so the first stage runs on the third.
    /// </summary>
    private int _frameBehaviorInstallCallsRemaining;

    /// <summary>The suit whose stage-three function is installed.</summary>
    public SamusSuitPickupKind Kind { get; private set; }

    /// <summary>The light-beam stage the next frame-behaviour call runs; it returns to the first stage when the transformation finishes.</summary>
    public SamusSuitPickupStage Stage { get; private set; }

    /// <summary>
    /// Screen rows the beam has covered from each screen edge while it appears, or wiped while it shrinks: zero at
    /// setup and again from the end of the curved widening until the shrinking stage starts, and always fewer than the
    /// beam's <see cref="SuitPickupBeamDefinitions.BeamLines"/>: each stage stops once it reaches 128, one update's
    /// growth past it at most.
    /// </summary>
    private int _lightBeamLines;

    /// <summary>
    /// The beam's inclusive left screen column while it is a column or centre line (the uniform, curved,
    /// dissipating and finishing stages); only X is meaningful. It always lies across the screen's width
    /// (<see cref="ScreenColumn"/>): the widening stops its edges at the screen's sides, and the dissipation carries them
    /// round the row (<see cref="SuitPickupClosingLineWrap"/>).
    /// </summary>
    private Position16<ScreenPixels> _lightBeamLeft;

    /// <summary>The beam's inclusive right screen column in the same stages as <see cref="_lightBeamLeft"/>; only X is meaningful.</summary>
    private Position16<ScreenPixels> _lightBeamRight;

    /// <summary>
    /// The speed, in screen pixels per update, at which the beam's edges move apart or its wipe advances. It starts at
    /// one pixel, accelerates during the curved widening, is halved at the reveal and decelerates to a one-pixel minimum
    /// while the beam shrinks. It stays positive and far below the sign of its 8.8 form: the curved widening moves the
    /// left edge from column 108 by at least one pixel an update, so it accelerates for at most 108 updates and the speed
    /// stays below 256 + 108 * 96 = 10624 256ths of a pixel.
    /// </summary>
    private SignedEightEight _lightBeamSpeed;

    /// <summary>The colour the light beam adds inside its window.</summary>
    public RgbColor BeamColor { get; private set; }

    /// <summary>Light-beam outline and progress produced by the most recent stage.</summary>
    public SamusSuitPickupBeam Beam { get; private set; }

    /// <summary>
    /// Runs the Varia or Gravity suit pickup after its message closes.
    /// </summary>
    public void Begin(
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        SamusSuitPickupKind kind)
    {
        Ensure.NotNull(samus);
        if (IsActive)
            throw new InvalidOperationException("A suit pickup transformation is already active.");

        Kind = kind;
        BeamColor = kind == SamusSuitPickupKind.Varia
            ? new FadeLevelColour(SamusPaletteDefinitions.SuitPickup.InitialRed, SamusPaletteDefinitions.SuitPickup.VariaGreen, SamusPaletteDefinitions.SuitPickup.VariaBlue).ToColour()
            : new FadeLevelColour(SamusPaletteDefinitions.SuitPickup.InitialRed, SamusPaletteDefinitions.SuitPickup.GravityGreen, SamusPaletteDefinitions.SuitPickup.GravityBlue).ToColour();
        Stage = SamusSuitPickupStage.LightBeamAppears;
        _lightBeamLines = 0;
        _lightBeamLeft = Position16<ScreenPixels>.Zero;
        _lightBeamRight = Position16<ScreenPixels>.Zero;
        _frameBehaviorInstallCallsRemaining = 2;
        LeaveProgress(samus);
        _lightBeamSpeed = SuitPickupBeamDefinitions.InitialWideningSpeed;
        Beam = SamusSuitPickupBeam.None;

        // The suit pickup cancels every independent speed component before
        // changing pose. CancelRunningMomentum also preserves the departing-echo
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
        // Entry's Samus lock command publishes history before locking gameplay.
        samus.CommitPoseHistory();
        samus.Control.LockForSuitPickup();

        // The game writes whole positions but leaves the fractions untouched. These are
        // fixed screen coordinates, not a room-specific item-location approximation.
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(cameraPosition.X + 120), CoordinateAxis.Wrap(cameraPosition.Y + 136)));
        // The pickup stores the same position as the previous position, so this frame's
        // scrolling sees no movement.
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        samus.WritePreviousYPosition(samus.Position.Y.Whole);
        IsActive = true;
        // The setup queues the transformation sound itself, at normal priority, as it runs.
        samus.AudioRequests.QueueSound(EnemySound.SuitAcquired, SoundRequestPriority.Normal);
    }

    /// <summary>Runs one update of the installed light-beam stage step.</summary>
    public void Step(SamusState samus, SamusColours colours)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(colours);
        if (!IsActive)
            return;
        if (_frameBehaviorInstallCallsRemaining != 0)
        {
            _frameBehaviorInstallCallsRemaining--;
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
        LeaveProgress(samus);
    }

    /// <summary>
    /// Leaves the current stage and the light beam's progress for a later shinespark windup and the stage for Crystal
    /// Flash's drain and Draygon's escape: the lines grown or wiped while the beam appears, during the reveal and while it
    /// shrinks; its edges while it is a column or centre line. A Crystal Flash drain the pickup suspended counts on from
    /// the stage.
    /// </summary>
    private void LeaveProgress(SamusState samus)
    {
        int stage = StageOrder(Stage);
        switch (Stage)
        {
            case SamusSuitPickupStage.LightBeamAppears:
            case SamusSuitPickupStage.RevealSuit:
            case SamusSuitPickupStage.LightBeamShrinks:
                samus.SparkCarryover.LeaveLines(stage, _lightBeamLines);
                break;
            case SamusSuitPickupStage.UniformWidening:
            case SamusSuitPickupStage.CurvedWidening:
            case SamusSuitPickupStage.LightBeamDissipates:
            case SamusSuitPickupStage.Finish:
                samus.SparkCarryover.LeaveEdges(stage, _lightBeamLeft.X, _lightBeamRight.X);
                break;
            default:
                throw new InvalidDataException($"Unknown suit-pickup stage {Stage}.");
        }
        samus.EscapeAndDrainCount.LeaveSuitPickupStage(stage);
    }

    /// <summary>The place of <paramref name="stage"/> in the stage order, 0 through 6.</summary>
    private static int StageOrder(SamusSuitPickupStage stage) => stage switch
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

    /// <summary>Extends the narrow beam inward from both screen edges until 128 rows are covered.</summary>
    private void StepLightBeamAppears()
    {
        // The count grows from zero by eight to exactly 128, so the game's signed
        // comparison with 128 is this ordinary one.
        _lightBeamLines += 8;
        Beam = new(SamusSuitPickupBeamShape.GrowingFromEdges,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Left,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Right,
            _lightBeamLines);

        if (_lightBeamLines >= 128)
        {
            Stage = SamusSuitPickupStage.UniformWidening;
            SetLightBeamEdges(SuitPickupBeamDefinitions.NarrowBeamEdges);
        }
    }

    /// <summary>Moves the beam's left and right endpoints apart uniformly before curved widening begins.</summary>
    private void StepUniformWidening()
    {
        // The widening speed stays a few pixels per update, so the edges stay inside the screen row: the left one
        // stops below 97 long before it would pass zero.
        int amount = _lightBeamSpeed.WholePixels;
        int left = _lightBeamLeft.X - amount;
        int right = _lightBeamRight.X + amount;
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.Column, _lightBeamLeft, _lightBeamRight, 0);

        if (left < 97)
        {
            Stage = SamusSuitPickupStage.CurvedWidening;
            SetLightBeamEdges(SuitPickupBeamDefinitions.CurvedWideningStart);
        }
    }

    /// <summary>Applies the mirrored authored curve to each row as the beam widens and its colour approaches white.</summary>
    private void StepCurvedWidening()
    {
        AdvanceColorTowardWhite();

        int amount = _lightBeamSpeed.WholePixels;
        int left = _lightBeamLeft.X - amount;
        int right = _lightBeamRight.X;
        // The zero test shares the negative-result clamp. Exact zero must also
        // force the right endpoint to the screen's rightmost column or the reveal is delayed by a frame.
        if (left <= 0)
        {
            left = 0;
            right = SuitPickupBeamDefinitions.RightmostColumn;
        }
        else
        {
            int widenedRight = right + amount;
            right = Math.Min(widenedRight, SuitPickupBeamDefinitions.RightmostColumn);
        }
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        // Stage two widens this column by the authored curve on each row.
        Beam = new(SamusSuitPickupBeamShape.CurvedColumn, _lightBeamLeft, _lightBeamRight, 0);

        _lightBeamSpeed += SuitPickupBeamDefinitions.WideningAcceleration;
        if (SuitPickupBeamEdges.Columns(left, right) == SuitPickupBeamDefinitions.FullScreenWindow)
        {
            Stage = SamusSuitPickupStage.RevealSuit;
            // The speed halves, truncating its lowest fraction bit.
            _lightBeamSpeed = _lightBeamSpeed.Halved();
            _lightBeamLines = 0;
        }
    }

    /// <summary>Installs the equipment bit, suited front pose, and palette at the third-stage handoff.</summary>
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
        SamusNormalSuitPalette.Load(colours, samus.EquippedItems);
        Stage = SamusSuitPickupStage.LightBeamShrinks;
    }

    /// <summary>Wipes beam rows from both edges while decelerating and restoring the beam's suit-specific colour.</summary>
    private void StepLightBeamShrinks()
    {
        if (Kind == SamusSuitPickupKind.Gravity)
            AdvanceColorTowardBlue();
        else
            AdvanceColorTowardOrange();

        // The wipe grows from zero by the speed's whole pixels and stops once it reaches 128, so it
        // stays far inside the range where the game's signed comparison is this ordinary one.
        _lightBeamLines += _lightBeamSpeed.WholePixels;
        // The final curved column is wiped by this many rows from the top. The game's
        // lower do/while starts at position minus one, so the bottom wipe is one shorter.
        Beam = Beam with { Shape = SamusSuitPickupBeamShape.ClosingCurvedColumn, Progress = _lightBeamLines };

        // The speed never falls below its minimum, so one deceleration stays far above zero.
        SignedEightEight decelerated = _lightBeamSpeed - SuitPickupBeamDefinitions.ShrinkingDeceleration;
        _lightBeamSpeed = decelerated < SuitPickupBeamDefinitions.MinimumShrinkingSpeed
            ? SuitPickupBeamDefinitions.MinimumShrinkingSpeed
            : decelerated;
        if (_lightBeamLines >= 128)
        {
            Stage = SamusSuitPickupStage.LightBeamDissipates;
            SetLightBeamEdges(SuitPickupBeamDefinitions.DissipationStart);
        }
    }

    /// <summary>Closes the single remaining center row until the threshold ends the effect.</summary>
    private void StepLightBeamDissipates()
    {
        int left = SuitPickupClosingLineWrap.NarrowedLeft(_lightBeamLeft.X);
        int right = SuitPickupClosingLineWrap.NarrowedRight(_lightBeamRight.X);
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.CenterLine, _lightBeamLeft, _lightBeamRight, 0);
        if (left >= 112)
            Stage = SamusSuitPickupStage.Finish;
    }

    /// <summary>Clears the light beam's window and colour, then releases Samus input after the last light-beam stage.</summary>
    private void Finish(SamusState samus)
    {
        BeamColor = new FadeLevelColour(SamusPaletteDefinitions.SuitPickup.ResetRed, SamusPaletteDefinitions.SuitPickup.ResetGreen,
            SamusPaletteDefinitions.SuitPickup.ResetBlue).ToColour();
        Beam = SamusSuitPickupBeam.None;
        Stage = SamusSuitPickupStage.LightBeamAppears;
        _lightBeamLines = 0;
        _lightBeamLeft = Position16<ScreenPixels>.Zero;
        _lightBeamRight = Position16<ScreenPixels>.Zero;
        BeamColor = RgbColor.Black;
        samus.Control.Unlock();
        IsActive = false;
    }

    /// <summary>Starts a column stage at the authored edge pair.</summary>
    private void SetLightBeamEdges(SuitPickupBeamEdges edges)
    {
        _lightBeamLeft = edges.Left;
        _lightBeamRight = edges.Right;
    }

    /// <summary>
    /// The screen position of window column <paramref name="column"/>, which lies across the screen's width; only X is
    /// meaningful.
    /// </summary>
    private static Position16<ScreenPixels> ScreenColumn(int column) =>
        Position16<ScreenPixels>.Of(Ensure.BetweenInclusive(0, SuitPickupBeamDefinitions.RightmostColumn, column), 0);

    /// <summary>Raises each channel by two toward the white reveal colour.</summary>
    private void AdvanceColorTowardWhite() =>
        BeamColor = new FadeLevelColour(AddTwoAndClamp(BeamColor.FadeLevels.Red, SamusPaletteDefinitions.SuitPickup.WhiteRed),
            AddTwoAndClamp(BeamColor.FadeLevels.Green, SamusPaletteDefinitions.SuitPickup.WhiteGreen),
            AddTwoAndClamp(BeamColor.FadeLevels.Blue, SamusPaletteDefinitions.SuitPickup.WhiteBlue)).ToColour();

    /// <summary>Returns the Varia beam's colour toward its orange target during the shrinking stage.</summary>
    private void AdvanceColorTowardOrange() => StepColorDownToward(
        SamusPaletteDefinitions.SuitPickup.WhiteRed,
        SamusPaletteDefinitions.SuitPickup.VariaOrangeGreen,
        SamusPaletteDefinitions.SuitPickup.VariaOrangeBlue);

    /// <summary>Returns the Gravity beam's colour toward its blue target during the shrinking stage.</summary>
    private void AdvanceColorTowardBlue() => StepColorDownToward(
        SamusPaletteDefinitions.SuitPickup.InitialRed,
        SamusPaletteDefinitions.SuitPickup.GravityGreen,
        SamusPaletteDefinitions.SuitPickup.GravityBlue);

    /// <summary>Lowers each channel by one until it reaches its target.</summary>
    private void StepColorDownToward(int red, int green, int blue) =>
        BeamColor = new FadeLevelColour(BeamColor.FadeLevels.Red == red ? red : BeamColor.FadeLevels.Red - 1, BeamColor.FadeLevels.Green == green ? green : BeamColor.FadeLevels.Green - 1,
            BeamColor.FadeLevels.Blue == blue ? blue : BeamColor.FadeLevels.Blue - 1).ToColour();

    /// <summary>Advances a channel by two without overshooting its stage target.</summary>
    private static int AddTwoAndClamp(int value, int target)
    {
        if (value == target)
            return value;
        return Math.Min(value + 2, target);
    }
}
