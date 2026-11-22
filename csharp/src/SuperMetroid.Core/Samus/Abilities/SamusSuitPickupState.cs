using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The shared Varia- and Gravity-suit pickup transformation.
/// </summary>
/// <remarks>
/// <para>
/// The item room object grants the suit and displays its message first. Only after
/// that synchronous message returns does the suit pickup center and lock Samus,
/// while the light beam advances this seven-stage sequence
/// once per gameplay frame. This owner deliberately begins after the message rather than
/// hiding those two independently observable lifetimes behind one combined animation.
/// </para>
/// <para>
/// Nothing in gameplay reads how the light beam is drawn, so this owner publishes the beam's
/// shape, progress and colour as <c>Flash</c>, and the display capture describes it as
/// window shapes the backend draws (#1468).
/// </para>
/// <para>
/// The pickup leaves its stage and its light beam's progress at its setup and after every stage update
/// (<see cref="LeaveProgress"/>): both in the shinespark's upward acceleration (<see cref="SuitPickupSparkCarryover{TStage}"/>),
/// and the stage on the count Crystal Flash's ammunition drain and Draygon's escape share
/// (<see cref="SharedEscapeAndDrainCount{TStage}"/>).
/// </para>
/// </remarks>
public sealed partial class SamusSuitPickupState
{
    /// <summary>Whether the post-message transformation currently owns Samus input: it runs a light-beam stage (#2560).</summary>
    public bool IsActive => runningStage is not null;

    /// <summary>
    /// The wait before the light beam's first stage runs: the light beam starts on the pickup's third update
    /// (<see cref="SuitPickupBeamDefinitions.UpdatesBeforeLightBeam"/>).
    /// </summary>
    private readonly UpdateCountdown _lightBeamWait = UpdateCountdown.Ended();

    /// <summary>The suit the pickup equips and reveals.</summary>
    public SamusSuitPickupKind Kind { get; private set; }

    /// <summary>The light-beam stage the next update runs, or none while no transformation runs (#2560).</summary>
    private SamusSuitPickupStage? runningStage;

    /// <summary>
    /// The light-beam stage the next update runs, which the light beam leaves behind; a finished transformation stands at
    /// the first stage again, as its finish resets it, and leaves that.
    /// </summary>
    internal SamusSuitPickupStage Stage
    {
        get => runningStage ?? SamusSuitPickupStage.LightBeamAppears;
        set => runningStage = value;
    }

    /// <summary>
    /// Screen rows the beam has covered from each screen edge while it appears, or wiped while it shrinks: zero at
    /// setup and again from the end of the curved widening until the shrinking stage starts, and always fewer than the
    /// beam's <see cref="SuitPickupBeamDefinitions.BeamLines"/>: each stage stops once it reaches <see cref="SuitPickupBeamDefinitions.CoveredLines"/>, one update's
    /// growth past it at most.
    /// </summary>
    private int _lightBeamLines;

    /// <summary>
    /// The beam's inclusive left screen column while it is a column or centre line (the uniform, curved,
    /// dissipating and finishing stages). It always lies across the screen's width
    /// (<see cref="ScreenColumn"/>): the widening stops its edges at the screen's sides, and the dissipation carries them
    /// round the row (<see cref="SuitPickupClosingLineWrap"/>).
    /// </summary>
    private int _lightBeamLeft;

    /// <summary>The beam's inclusive right screen column in the same stages as <see cref="_lightBeamLeft"/>.</summary>
    private int _lightBeamRight;

    /// <summary>
    /// The speed, in screen pixels per update, at which the beam's edges move apart or its wipe advances. It starts at
    /// one pixel, accelerates during the curved widening, is halved at the reveal and decelerates to a one-pixel minimum
    /// while the beam shrinks. It stays positive and small: the curved widening moves the left edge from its starting
    /// column by at least one pixel an update, so it accelerates for no more updates than that column lies from the
    /// screen's edge, and the speed stays below one pixel plus that much acceleration.
    /// </summary>
    private SignedEightEight _lightBeamSpeed;

    /// <summary>The colour the light beam adds where it shines.</summary>
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
        // The pickup locks Samus until it ends, so no second suit starts one while it runs.
        Kind = kind;
        BeamColor = (kind == SamusSuitPickupKind.Varia
            ? SamusShadeDefinitions.SuitPickup.VariaBeam
            : SamusShadeDefinitions.SuitPickup.GravityBeam).ToColour();
        Stage = SamusSuitPickupStage.LightBeamAppears;
        _lightBeamLines = 0;
        _lightBeamLeft = 0;
        _lightBeamRight = 0;
        _lightBeamWait.Restart(SuitPickupBeamDefinitions.UpdatesBeforeLightBeam);
        LeaveProgress(samus);
        _lightBeamSpeed = SuitPickupBeamDefinitions.InitialWideningSpeed;
        Beam = SamusSuitPickupBeam.None;

        // The suit pickup cancels every independent speed component before
        // changing pose. CancelRunningMomentum also preserves the departing-echo
        // side effect and publishes the immediate shade restore only when boost was live.
        samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
        samus.MorphBallBounceState = null;

        // The first pose tests whether the other suit is equipped. Acquiring Varia while Gravity is already
        // equipped (or vice versa) therefore starts in the suited front pose; an ordinary
        // first suit starts in the power suit's front pose until the reveal.
        bool otherSuitAlreadyEquipped = kind == SamusSuitPickupKind.Varia
            ? samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit);
        samus.ChangePose(otherSuitAlreadyEquipped
            ? SamusPoseId.ForwardFacingSuitedPose
            : SamusPoseId.ForwardFacingPowerSuitPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        // Entry's suit-pickup lock publishes history before locking gameplay.
        samus.CommitPoseHistory();
        samus.Control.LockForSuitPickup();

        // The game writes whole positions but leaves the fractions untouched. These are
        // fixed screen coordinates, not a room-specific item-location approximation. The camera lies within the room,
        // so a place a fixed distance into its screen does too.
        samus.Position = samus.Position.WithWhole(new(cameraPosition.X + SamusSuitPickupDefinitions.ScreenX, cameraPosition.Y + SamusSuitPickupDefinitions.ScreenY));
        // The pickup stores the same position as the previous position, so this frame's
        // scrolling sees no movement.
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        samus.WritePreviousYPosition(samus.Position.Y.Whole);
        // The setup queues the transformation sound itself, at normal priority, as it runs.
        samus.AudioRequests.QueueSound(EnemySound.SuitAcquired, SoundRequestPriority.Normal);
    }

    /// <summary>What <c>Step</c> does for each <see cref="SamusSuitPickupStage"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<SamusSuitPickupStage, Action<SamusState, SamusColours>> StepForSamusSuitPickupStage => field ??= new(new Dictionary<SamusSuitPickupStage, Action<SamusState, SamusColours>>
    {
        [SamusSuitPickupStage.LightBeamAppears] = (samus, colours) => StepLightBeamAppears(),
        [SamusSuitPickupStage.UniformWidening] = (samus, colours) => StepUniformWidening(),
        [SamusSuitPickupStage.CurvedWidening] = (samus, colours) => StepCurvedWidening(),
        [SamusSuitPickupStage.RevealSuit] = (samus, colours) => RevealSuit(samus, colours),
        [SamusSuitPickupStage.LightBeamShrinks] = (samus, colours) => StepLightBeamShrinks(),
        [SamusSuitPickupStage.LightBeamDissipates] = (samus, colours) => StepLightBeamDissipates(),
        [SamusSuitPickupStage.Finish] = (samus, colours) => Finish(samus),
    });

    /// <summary>Runs one update of the light beam's current stage.</summary>
    public void Step(SamusState samus, SamusColours colours)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(colours);
        if (!IsActive)
            return;
        if (_lightBeamWait.IsRunning)
        {
            _lightBeamWait.Count();
            return;
        }

        StepForSamusSuitPickupStage[Stage](samus, colours);
        LeaveProgress(samus);
    }

    /// <summary>What <see cref="LeaveProgress"/> does for each <see cref="SamusSuitPickupStage"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<SamusSuitPickupStage, Action<SamusState>> LeaveProgressForSamusSuitPickupStage => field ??= new(new Dictionary<SamusSuitPickupStage, Action<SamusState>>
    {
        [SamusSuitPickupStage.LightBeamAppears] = samus => samus.SparkCarryover.LeaveLines(Stage, _lightBeamLines),
        [SamusSuitPickupStage.RevealSuit] = samus => samus.SparkCarryover.LeaveLines(Stage, _lightBeamLines),
        [SamusSuitPickupStage.LightBeamShrinks] = samus => samus.SparkCarryover.LeaveLines(Stage, _lightBeamLines),
        [SamusSuitPickupStage.UniformWidening] = samus => samus.SparkCarryover.LeaveEdges(Stage, _lightBeamLeft, _lightBeamRight),
        [SamusSuitPickupStage.CurvedWidening] = samus => samus.SparkCarryover.LeaveEdges(Stage, _lightBeamLeft, _lightBeamRight),
        [SamusSuitPickupStage.LightBeamDissipates] = samus => samus.SparkCarryover.LeaveEdges(Stage, _lightBeamLeft, _lightBeamRight),
        [SamusSuitPickupStage.Finish] = samus => samus.SparkCarryover.LeaveEdges(Stage, _lightBeamLeft, _lightBeamRight),
    });

    /// <summary>
    /// Leaves the current stage and the light beam's progress for a later shinespark windup and the stage for Crystal
    /// Flash's drain and Draygon's escape: the lines grown or wiped while the beam appears, during the reveal and while it
    /// shrinks; its edges while it is a column or centre line. A Crystal Flash drain the pickup suspended counts on from
    /// the stage.
    /// </summary>
    private void LeaveProgress(SamusState samus)
    {
        LeaveProgressForSamusSuitPickupStage[Stage](samus);
        samus.EscapeAndDrainCount.LeaveSuitPickupStage(Stage);
    }

    /// <summary>Extends the narrow beam inward from both screen edges until half its rows are covered.</summary>
    private void StepLightBeamAppears()
    {
        // The count grows from zero in even steps until it covers exactly half the rows.
        _lightBeamLines += SuitPickupBeamDefinitions.AppearingLinesPerUpdate;
        Beam = new(SamusSuitPickupBeamShape.GrowingFromEdges,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Left,
            SuitPickupBeamDefinitions.NarrowBeamEdges.Right,
            _lightBeamLines);

        if (_lightBeamLines >= SuitPickupBeamDefinitions.CoveredLines)
        {
            Stage = SamusSuitPickupStage.UniformWidening;
            SetLightBeamEdges(SuitPickupBeamDefinitions.NarrowBeamEdges);
        }
    }

    /// <summary>Moves the beam's left and right endpoints apart uniformly before curved widening begins.</summary>
    private void StepUniformWidening()
    {
        // The widening speed stays a few pixels per update, so the edges stay inside the screen row: the left one
        // stops long before it would pass zero.
        int amount = _lightBeamSpeed.WholePixels;
        int left = _lightBeamLeft - amount;
        int right = _lightBeamRight + amount;
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.Column, _lightBeamLeft, _lightBeamRight, 0);

        if (left < SuitPickupBeamDefinitions.UniformWideningEndColumn)
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
        int left = _lightBeamLeft - amount;
        int right = _lightBeamRight;
        // Once the left edge reaches the screen's side, the beam spans the full width.
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
        // The curved widening widens this column by the authored curve on each row.
        Beam = new(SamusSuitPickupBeamShape.CurvedColumn, _lightBeamLeft, _lightBeamRight, 0);

        _lightBeamSpeed += SuitPickupBeamDefinitions.WideningAcceleration;
        if (new SuitPickupBeamEdges(left, right) == SuitPickupBeamDefinitions.FullScreenEdges)
        {
            Stage = SamusSuitPickupStage.RevealSuit;
            // The speed halves, rounding down.
            _lightBeamSpeed = _lightBeamSpeed.Halved();
            _lightBeamLines = 0;
        }
    }

    /// <summary>Equips the suit and shows the suited front pose and shade as the beam starts to close.</summary>
    private void RevealSuit(SamusState samus, SamusColours colours)
    {
        SamusEquipmentFlags mask = Kind == SamusSuitPickupKind.Varia
            ? SamusEquipmentFlags.VariaSuit
            : SamusEquipmentFlags.GravitySuit;
        samus.EquippedItems |= mask;
        samus.CollectedItems |= mask;
        samus.ChangePose(SamusPoseId.ForwardFacingSuitedPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        // The reveal phase publishes another transition, even if the other suit
        // already selected this same front-facing pose at entry.
        samus.CommitPoseHistory();
        SamusSuitShade.Show(colours, samus.EquippedItems);
        Stage = SamusSuitPickupStage.LightBeamShrinks;
    }

    /// <summary>Wipes beam rows from both edges while decelerating and restoring the beam's suit-specific colour.</summary>
    private void StepLightBeamShrinks()
    {
        if (Kind == SamusSuitPickupKind.Gravity)
            AdvanceColorTowardBlue();
        else
            AdvanceColorTowardOrange();

        // The wipe grows from zero by the speed's whole pixels and stops once it reaches the covered lines.
        _lightBeamLines += _lightBeamSpeed.WholePixels;
        // The final curved column is wiped by this many rows from the top and one row fewer from the bottom.
        Beam = Beam with { Shape = SamusSuitPickupBeamShape.ClosingCurvedColumn, Progress = _lightBeamLines };

        // The speed never falls below its minimum, so one deceleration stays far above zero.
        SignedEightEight decelerated = _lightBeamSpeed - SuitPickupBeamDefinitions.ShrinkingDeceleration;
        _lightBeamSpeed = decelerated < SuitPickupBeamDefinitions.MinimumShrinkingSpeed
            ? SuitPickupBeamDefinitions.MinimumShrinkingSpeed
            : decelerated;
        if (_lightBeamLines >= SuitPickupBeamDefinitions.CoveredLines)
        {
            Stage = SamusSuitPickupStage.LightBeamDissipates;
            SetLightBeamEdges(SuitPickupBeamDefinitions.DissipationStart);
        }
    }

    /// <summary>Closes the single remaining center row until the threshold ends the effect.</summary>
    private void StepLightBeamDissipates()
    {
        int left = SuitPickupClosingLineWrap.NarrowedLeft(_lightBeamLeft);
        int right = SuitPickupClosingLineWrap.NarrowedRight(_lightBeamRight);
        _lightBeamLeft = ScreenColumn(left);
        _lightBeamRight = ScreenColumn(right);
        Beam = new(SamusSuitPickupBeamShape.CenterLine, _lightBeamLeft, _lightBeamRight, 0);
        if (left >= SuitPickupBeamDefinitions.DissipatedColumn)
            Stage = SamusSuitPickupStage.Finish;
    }

    /// <summary>Clears the light beam's outline and colour, then releases Samus input after the last light-beam stage.</summary>
    private void Finish(SamusState samus)
    {
        Beam = SamusSuitPickupBeam.None;
        runningStage = null;
        _lightBeamLines = 0;
        _lightBeamLeft = 0;
        _lightBeamRight = 0;
        BeamColor = RgbColor.Black;
        samus.Control.Unlock();
    }

    /// <summary>Starts a column stage at the authored edge pair.</summary>
    private void SetLightBeamEdges(SuitPickupBeamEdges edges)
    {
        _lightBeamLeft = edges.Left;
        _lightBeamRight = edges.Right;
    }

    /// <summary>
    /// The screen column an edge at <paramref name="column"/> stands at: the game keeps each edge as one byte of its
    /// window, so an edge lies across the screen's 256-pixel row, and a column past either side would wrap round it as
    /// that byte does. The stages' arithmetic keeps every edge inside the row (#2553).
    /// </summary>
    private static int ScreenColumn(int column) =>
        ((column % ScreenGeometry.ScreenWidthPixels) + ScreenGeometry.ScreenWidthPixels) % ScreenGeometry.ScreenWidthPixels;

    /// <summary>Raises each channel toward the white reveal colour, stopping at white's level.</summary>
    private void AdvanceColorTowardWhite()
    {
        FadeLevelColour white = SamusShadeDefinitions.SuitPickup.White;
        BeamColor = BeamColor.FadeLevels.Map((channel, level) =>
            Math.Min(level + SamusShadeDefinitions.SuitPickup.WhiteningLevelsPerUpdate, white[channel])).ToColour();
    }

    /// <summary>Returns the Varia beam's colour toward its orange target during the shrinking stage.</summary>
    private void AdvanceColorTowardOrange() => StepColorDownToward(SamusShadeDefinitions.SuitPickup.VariaOrange);

    /// <summary>Returns the Gravity beam's colour toward its own colour during the shrinking stage.</summary>
    private void AdvanceColorTowardBlue() => StepColorDownToward(SamusShadeDefinitions.SuitPickup.GravityBeam);

    /// <summary>Lowers each channel toward <paramref name="target"/> until it reaches the target's level.</summary>
    private void StepColorDownToward(FadeLevelColour target) =>
        BeamColor = BeamColor.FadeLevels.Map((channel, level) =>
            level == target[channel] ? level : level - SamusShadeDefinitions.SuitPickup.SettlingLevelsPerUpdate).ToColour();
}
