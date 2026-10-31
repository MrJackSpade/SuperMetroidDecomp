namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Pure coordinate owner for a door's opening scroll in each of the four directions.
/// </summary>
/// <remarks>
/// Keeping the arithmetic independent of room loading makes every directional trace
/// exhaustively testable. The runtime merely publishes these values to the camera, Samus and
/// the background scroll; it never substitutes interpolation. Each scroll update moves only
/// the travel axis, so whole-value steps carry a zero component on the other.
/// </remarks>
internal sealed class DoorOpeningScrollState
{
    /// <summary>Pixels the camera and layer two move per scroll update.</summary>
    private const short LayerStepPixels = 4;

    /// <summary>Captures the directional setup positions, Samus's step, and the exact destination positions.</summary>
    private DoorOpeningScrollState(DoorOpeningScrollSetup setup)
    {
        Direction = setup.Direction;
        SamusScrollStep = setup.SamusScrollStep;
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

    /// <summary>Samus's signed 16.16 displacement for one scroll update, along the travel axis.</summary>
    public FixedVector<RoomPixels> SamusScrollStep { get; }

    /// <summary>Scroll updates still required before the scroll can snap to its destination.</summary>
    public int RemainingFrames { get; private set; }

    /// <summary>The camera's position as the scroll carries it.</summary>
    public Position16<RoomPixels> CameraPosition { get; private set; }

    /// <summary>The second background layer's position as the scroll carries it.</summary>
    public Position16<BackgroundPixels> Layer2Position { get; private set; }

    /// <summary>Samus's 16.16 position as the scroll carries her.</summary>
    public FixedPosition<RoomPixels> SamusPosition { get; private set; }

    /// <summary>Exact camera endpoint published after the scroll's final update.</summary>
    public Position16<RoomPixels> FinalCameraPosition { get; }

    /// <summary>Layer-two endpoint reached through directional four-pixel steps.</summary>
    public Position16<BackgroundPixels> FinalLayer2Position { get; }

    /// <summary>Deferred Samus endpoint retained by the door loader for the destination handoff.</summary>
    public FixedPosition<RoomPixels> FinalSamusPosition { get; }

    /// <summary>Whether the just-advanced update recomputes the layer scrolls; upward doors delay their first frames.</summary>
    public bool RecalculatesScrollAfterAdvance { get; private set; } = true;

    /// <summary>Builds the setup position and the scroll's trajectory from a door and its destination coordinates.</summary>
    public static DoorOpeningScrollState Create(
        DoorDefinition door,
        FixedPosition<RoomPixels> sourceSamus,
        Position16<RoomPixels> finalCamera,
        Position16<BackgroundPixels> finalLayer2,
        FixedPosition<RoomPixels> finalSamus)
    {
        DoorDirection direction = door.Orientation.Direction;
        FixedPosition<RoomPixels> samus = RebaseSamus(door, ApplySetupMovement(door, sourceSamus));

        // Layer two starts as far before its endpoint as the setup camera does before the
        // destination, except upward doors: their layer two begins $DC (not $FB) below it.
        Offset16<BackgroundPixels> layer2Setup = direction switch
        {
            DoorDirection.Right => new(-252, 0),
            DoorDirection.Left => new(252, 0),
            DoorDirection.Down => new(0, -224), // Down frame zero only stages the off-screen row.
            DoorDirection.Up => new(0, 220),
            _ => throw new ArgumentOutOfRangeException(nameof(door), direction, "Undefined door direction."),
        };

        return new DoorOpeningScrollState(new DoorOpeningScrollSetup { Direction = direction, SamusScrollStep = GetSamusStep(door), RemainingFrames = MovingScrollUpdates(direction), CameraPosition = GetSetupCamera(door), Layer2Position = finalLayer2 + layer2Setup, SamusPosition = samus, FinalCameraPosition = finalCamera, FinalLayer2Position = finalLayer2, FinalSamusPosition = finalSamus });
    }

    /// <summary>
    /// The scroll updates that remain after setup: a right or left setup takes the first step
    /// before placement (63 remain), down's first update only stages a row (56 remain), and an
    /// upward door's tile correction leaves its first moving step to setup (55 remain).
    /// </summary>
    internal static int MovingScrollUpdates(DoorDirection direction) => direction switch
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

    /// <summary>One scroll update's step; downward setup alone omits this displacement.</summary>
    internal static FixedPosition<RoomPixels> AdvanceSamus(DoorDefinition door, FixedPosition<RoomPixels> samus) =>
        samus.Advance(GetSamusStep(door));

    /// <summary>
    /// Samus's fixed-point displacement for one scroll update: the door's speed (or the
    /// axis default), added along right/down and subtracted along left/up, wrapping at
    /// 32 bits.
    /// </summary>
    internal static FixedVector<RoomPixels> GetSamusStep(DoorDefinition door)
    {
        int magnitude = SixteenSixteen.FromEightEight(door.SamusSpeed.StepSubpixels(door.Orientation.Direction));
        var step = SignedSixteenSixteen.FromRaw(magnitude);
        var back = SignedSixteenSixteen.FromRaw(-magnitude);
        return door.Orientation.Direction switch
        {
            DoorDirection.Right => new(step, SignedSixteenSixteen.Zero),
            DoorDirection.Left => new(back, SignedSixteenSixteen.Zero),
            DoorDirection.Down => new(SignedSixteenSixteen.Zero, step),
            DoorDirection.Up => new(SignedSixteenSixteen.Zero, back),
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

    /// <summary>The signed four-pixel layer step of one scroll update along the travel axis.</summary>
    private static Offset16<TSpace> LayerStep<TSpace>(DoorDirection direction)
        where TSpace : ISpatialSpace => direction switch
        {
            DoorDirection.Right => new(LayerStepPixels, 0),
            DoorDirection.Left => new(-LayerStepPixels, 0),
            DoorDirection.Down => new(0, LayerStepPixels),
            DoorDirection.Up => new(0, -LayerStepPixels),
            _ => throw new InvalidOperationException($"Undefined door direction {direction}."),
        };

    /// <summary>Runs one scroll update and reports whether it is the one that finishes the scroll.</summary>
    public bool Advance()
    {
        if (RemainingFrames <= 0)
            return true;

        int frameCounter = Direction == DoorDirection.Up ? 57 - RemainingFrames : 0;
        CameraPosition += LayerStep<RoomPixels>(Direction);
        Layer2Position += LayerStep<BackgroundPixels>(Direction);
        SamusPosition = SamusPosition.Advance(SamusScrollStep);

        RemainingFrames--;
        RecalculatesScrollAfterAdvance = Direction != DoorDirection.Up || frameCounter >= 5;
        return RemainingFrames == 0;
    }

    /// <summary>
    /// Applies the door loader's <c>PlaceSamusOnElevator</c> ($A3:9612): whole X, and Y
    /// with a zero fraction. Later scroll updates move Samus on from this position.
    /// </summary>
    public void PlaceSamusOnElevator(Position16<RoomPixels> position) =>
        SamusPosition = new FixedPosition<RoomPixels>(
            SamusPosition.X.WithWhole(position.X),
            new FixedCoordinate(position.Y, 0));

    /// <summary>
    /// Snaps the camera to the door's destination after the final directional step has
    /// produced its last background request.
    /// </summary>
    public void SnapLayerOneToDestination()
    {
        if (RemainingFrames != 0)
            throw new InvalidOperationException(
                "A door-opening trajectory cannot snap before its final scroll update.");

        // The final directional step recalculates the layer scrolls first; only then is the
        // camera replaced with the exact door destination. Layer two reaches its endpoint
        // through the final directional step and is not rewritten by the snap.
        CameraPosition = FinalCameraPosition;
    }
}
