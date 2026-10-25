namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Pure coordinate owner for <c>$80:AD30-$80:AFC0</c>'s four IRQ door trajectories.
/// </summary>
/// <remarks>
/// Keeping the arithmetic independent of room loading makes every directional trace
/// exhaustively testable. The runtime merely publishes these values to Camera, Samus, and
/// the background-scroll mirrors; it never substitutes host interpolation. Each IRQ call
/// moves only the travel axis, so whole-value steps carry a zero component on the other.
/// </remarks>
internal sealed class DoorOpeningScrollState
{
    /// <summary>Pixels the camera and layer two move per IRQ call.</summary>
    private const short LayerStepPixels = 4;

    /// <summary>Captures the directional setup positions, IRQ step, and exact destination snapshots.</summary>
    private DoorOpeningScrollState(DoorOpeningScrollSetup setup)
    {
        Direction = setup.Direction;
        SamusIrqStep = setup.SamusIrqStep;
        RemainingFrames = setup.RemainingFrames;
        CameraPosition = setup.CameraPosition;
        Layer2Position = setup.Layer2Position;
        SamusPosition = setup.SamusPosition;
        FinalCameraPosition = setup.FinalCameraPosition;
        FinalLayer2Position = setup.FinalLayer2Position;
        FinalSamusPosition = setup.FinalSamusPosition;
    }

    /// <summary>Travel axis and sign selected by the destination door header.</summary>
    public DoorDirection Direction { get; }

    /// <summary>Samus's signed 16.16 displacement for one moving IRQ call, along the travel axis.</summary>
    public FixedVector<RoomPixels> SamusIrqStep { get; }

    /// <summary>Moving IRQ calls still required before the trajectory can snap to its destination.</summary>
    public int RemainingFrames { get; private set; }

    /// <summary>The layer-one words <c>$0911/$0915</c> owned by the IRQ.</summary>
    public Position16<RoomPixels> CameraPosition { get; private set; }

    /// <summary>The layer-two words <c>$0917/$0919</c> owned by the IRQ.</summary>
    public Position16<Bg2Pixels> Layer2Position { get; private set; }

    /// <summary>Samus's 16.16 position as the IRQ carries her.</summary>
    public FixedPosition<RoomPixels> SamusPosition { get; private set; }

    /// <summary>Exact layer-one endpoint published after the final directional IRQ call.</summary>
    public Position16<RoomPixels> FinalCameraPosition { get; }

    /// <summary>Layer-two endpoint reached through directional four-pixel steps.</summary>
    public Position16<Bg2Pixels> FinalLayer2Position { get; }

    /// <summary>Deferred Samus endpoint retained by the door loader for the destination handoff.</summary>
    public FixedPosition<RoomPixels> FinalSamusPosition { get; }

    /// <summary>Whether the just-advanced IRQ recomputes the scroll registers; upward doors delay their first frames.</summary>
    public bool RecalculatesScrollAfterAdvance { get; private set; } = true;

    /// <summary>Builds the native setup position and IRQ trajectory from a door header and destination coordinates.</summary>
    public static DoorOpeningScrollState Create(
        DoorDefinition door,
        FixedPosition<RoomPixels> sourceSamus,
        Position16<RoomPixels> finalCamera,
        Position16<Bg2Pixels> finalLayer2,
        FixedPosition<RoomPixels> finalSamus)
    {
        DoorDirection direction = door.Orientation.Direction;
        FixedPosition<RoomPixels> samus = RebaseSamus(door, ApplySetupMovement(door, sourceSamus));

        // Layer two starts as far before its endpoint as the setup camera does before the
        // destination, except upward doors: their layer two begins $DC (not $FB) below it.
        Offset16<Bg2Pixels> layer2Setup = direction switch
        {
            DoorDirection.Right => new(-252, 0),
            DoorDirection.Left => new(252, 0),
            DoorDirection.Down => new(0, -224), // Down frame zero only stages the off-screen row.
            DoorDirection.Up => new(0, 220),
            _ => throw new ArgumentOutOfRangeException(nameof(door), direction, "Undefined door direction."),
        };

        return new DoorOpeningScrollState(new DoorOpeningScrollSetup { Direction = direction, SamusIrqStep = GetSamusStep(door), RemainingFrames = MovingIrqCalls(direction), CameraPosition = GetSetupCamera(door), Layer2Position = finalLayer2 + layer2Setup, SamusPosition = samus, FinalCameraPosition = finalCamera, FinalLayer2Position = finalLayer2, FinalSamusPosition = finalSamus });
    }

    /// <summary>
    /// The IRQ calls that remain after setup: right/left setup calls DoorTransition once
    /// before placement (63 remain), down's frame zero only stages a row (56 remain), and
    /// FixDoorsMovingUp leaves counter one for setup's first moving call (55 remain).
    /// </summary>
    internal static int MovingIrqCalls(DoorDirection direction) => direction switch
    {
        DoorDirection.Right or DoorDirection.Left => 63,
        DoorDirection.Down => 56,
        DoorDirection.Up => 55,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined door direction."),
    };

    /// <summary>The room-pixel origin of the door's destination screen.</summary>
    internal static Position16<RoomPixels> GetDestination(DoorDefinition door) =>
        door.DestinationScreen.ToRoomPixels();

    /// <summary>Layer-one origin after the directional setup's initial call.</summary>
    internal static Position16<RoomPixels> GetSetupCamera(DoorDefinition door) =>
        GetDestination(door) + door.Orientation.Direction switch
        {
            DoorDirection.Right => new Offset16<RoomPixels>(-252, 0),
            DoorDirection.Left => new Offset16<RoomPixels>(252, 0),
            DoorDirection.Down => new Offset16<RoomPixels>(0, -224),
            DoorDirection.Up => new Offset16<RoomPixels>(0, 251),
            _ => throw new ArgumentOutOfRangeException(nameof(door), door.Orientation.Direction, "Undefined door direction."),
        };

    /// <summary>
    /// <c>$82:E3C0</c> replaces both whole coordinates with layer one plus the
    /// low position byte, preserving the fractions and deferring final nudges.
    /// </summary>
    internal static FixedPosition<RoomPixels> RebaseSamus(DoorDefinition door, FixedPosition<RoomPixels> setup)
    {
        var withinScreen = new Offset16<RoomPixels>((byte)setup.X.Whole, (byte)setup.Y.Whole);
        return setup.WithWhole(GetSetupCamera(door) + withinScreen);
    }

    /// <summary>One moving IRQ call; downward setup alone omits this displacement.</summary>
    internal static FixedPosition<RoomPixels> AdvanceSamus(DoorDefinition door, FixedPosition<RoomPixels> samus) =>
        samus.Advance(GetSamusStep(door));

    /// <summary>
    /// Samus's fixed-point displacement for one native door IRQ call: the door's distance
    /// (or the native default) times $100 subpixels, added along right/down and subtracted
    /// along left/up with the native 32-bit wrap.
    /// </summary>
    internal static FixedVector<RoomPixels> GetSamusStep(DoorDefinition door)
    {
        int distance = unchecked((short)door.SamusDistance);
        if (distance < 0)
            distance = door.Orientation.IsVertical ? 384 : 200;
        int magnitude = SnesSixteenSixteen.FromEightEight(distance);
        var step = SnesSignedSixteenSixteen.FromRaw(magnitude);
        var back = SnesSignedSixteenSixteen.FromRaw(-magnitude);
        return door.Orientation.Direction switch
        {
            DoorDirection.Right => new(step, SnesSignedSixteenSixteen.Zero),
            DoorDirection.Left => new(back, SnesSignedSixteenSixteen.Zero),
            DoorDirection.Down => new(SnesSignedSixteenSixteen.Zero, step),
            DoorDirection.Up => new(SnesSignedSixteenSixteen.Zero, back),
            _ => throw new ArgumentOutOfRangeException(nameof(door), door.Orientation.Direction, "Undefined door direction."),
        };
    }

    /// <summary>
    /// Applies setup's first directional call before <c>$82:E3C0</c> replaces the
    /// whole position words. Downward setup stages a row without moving Samus.
    /// </summary>
    internal static FixedPosition<RoomPixels> ApplySetupMovement(
        DoorDefinition door, FixedPosition<RoomPixels> source) =>
        door.Orientation.Direction == DoorDirection.Down ? source : AdvanceSamus(door, source);

    /// <summary>The signed four-pixel layer step of one IRQ call along the travel axis.</summary>
    private static Offset16<TSpace> LayerStep<TSpace>(DoorDirection direction)
        where TSpace : ISpatialSpace => direction switch
        {
            DoorDirection.Right => new(LayerStepPixels, 0),
            DoorDirection.Left => new(-LayerStepPixels, 0),
            DoorDirection.Down => new(0, LayerStepPixels),
            DoorDirection.Up => new(0, -LayerStepPixels),
            _ => throw new InvalidOperationException($"Undefined door direction {direction}."),
        };

    /// <summary>Runs one IRQ call and reports the frame that sets completion bit $8000.</summary>
    public bool Advance()
    {
        if (RemainingFrames <= 0)
            return true;

        int frameCounter = Direction == DoorDirection.Up ? 57 - RemainingFrames : 0;
        CameraPosition += LayerStep<RoomPixels>(Direction);
        Layer2Position += LayerStep<Bg2Pixels>(Direction);
        SamusPosition = SamusPosition.Advance(SamusIrqStep);

        RemainingFrames--;
        RecalculatesScrollAfterAdvance = Direction != DoorDirection.Up || frameCounter >= 5;
        return RemainingFrames == 0;
    }

    /// <summary>
    /// Applies the door loader's <c>PlaceSamusOnElevator</c> ($A3:9612): whole X, and Y
    /// with a zero fraction. Later IRQ calls move Samus on from this position.
    /// </summary>
    public void PlaceSamusOnElevator(Position16<RoomPixels> position) =>
        SamusPosition = new FixedPosition<RoomPixels>(
            SamusPosition.X.WithWhole(position.X),
            new SnesFixedPosition(position.Y, 0));

    /// <summary>
    /// Applies <c>Irq_FollowDoorTransition</c>'s layer-one destination snap after the
    /// directional routine has produced its final background-stream request.
    /// </summary>
    public void SnapLayerOneToDestination()
    {
        if (RemainingFrames != 0)
            throw new InvalidOperationException(
                "A door-opening trajectory cannot snap before its final IRQ call.");

        // The cartridge calls $80:A3A0 from DoorTransition_* before control returns to
        // Irq_FollowDoorTransition. Only then does the wrapper replace $0911/$0915 with
        // the exact door destination. Layer two reaches its endpoint through the final
        // directional step and is not rewritten by the wrapper.
        CameraPosition = FinalCameraPosition;
    }
}
