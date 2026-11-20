namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Where a door's opening scroll carries the camera, Samus and the background, in each of the four directions.
/// </summary>
/// <remarks>
/// The camera, Samus and the background take these positions as they are, with nothing between one step and the next.
/// Each scroll update moves them along the travel direction only.
/// </remarks>
internal sealed class DoorOpeningScrollState
{

    /// <summary>Captures the directional setup positions, Samus's step, and the exact destination positions.</summary>
    private DoorOpeningScrollState(DoorOpeningScrollSetup setup)
    {
        Direction = setup.Direction;
        SamusScrollStep = setup.SamusScrollStep;
        Scroll = UpdateCountdown.For(setup.ScrollUpdates);
        CameraPosition = setup.CameraPosition;
        BackgroundPosition = setup.BackgroundPosition;
        SamusPosition = setup.SamusPosition;
        FinalCameraPosition = setup.FinalCameraPosition;
    }

    /// <summary>Travel axis and sign selected by the destination door definition.</summary>
    public DoorDirection Direction { get; }

    /// <summary>Samus's displacement for one scroll update, along the travel axis.</summary>
    public FixedVector<RoomPixels> SamusScrollStep { get; }

    /// <summary>
    /// The scroll's updates, counted once each update it advances: the scroll can snap to its destination once it is over,
    /// and its age delays an upward door's layer recalculation.
    /// </summary>
    public UpdateCountdown Scroll { get; }

    /// <summary>The camera's position as the scroll carries it.</summary>
    public Position16<RoomPixels> CameraPosition { get; private set; }

    /// <summary>The background's position as the scroll carries it.</summary>
    public Position16<BackgroundPixels> BackgroundPosition { get; private set; }

    /// <summary>Samus's position, fractions of a pixel included, as the scroll carries her.</summary>
    public FixedPosition<RoomPixels> SamusPosition { get; private set; }

    /// <summary>Exact camera endpoint published after the scroll's final update.</summary>
    public Position16<RoomPixels> FinalCameraPosition { get; }

    /// <summary>
    /// Builds the setup position and the scroll's trajectory from a door and its destination camera and background
    /// positions; the background's endpoint is reached through directional steps (<see cref="DoorOpeningScrollDefinitions.LayerStepPixels"/>).
    /// </summary>
    public static DoorOpeningScrollState Create(
        DoorDefinition door,
        FixedPosition<RoomPixels> sourceSamus,
        Position16<RoomPixels> finalCamera,
        Position16<BackgroundPixels> finalBackground)
    {
        DoorDirection direction = door.Orientation.Direction;
        FixedPosition<RoomPixels> samus = RebaseSamus(door, ApplySetupMovement(door, sourceSamus));

        Offset16<BackgroundPixels> backgroundSetup = Ensure.Listed(DoorOpeningScrollDefinitions.SetupBackgroundOffsets, direction);

        return new DoorOpeningScrollState(new DoorOpeningScrollSetup { Direction = direction, SamusScrollStep = GetSamusStep(door), ScrollUpdates = MovingScrollUpdates(direction), CameraPosition = GetSetupCamera(door), BackgroundPosition = finalBackground + backgroundSetup, SamusPosition = samus, FinalCameraPosition = finalCamera });
    }

    /// <summary>The scroll updates that remain after setup (<see cref="DoorOpeningScrollDefinitions.MovingScrollUpdates"/>).</summary>
    internal static int MovingScrollUpdates(DoorDirection direction) =>
        Ensure.Listed(DoorOpeningScrollDefinitions.MovingScrollUpdates, direction);

    /// <summary>The room-pixel origin of the door's destination screen.</summary>
    internal static Position16<RoomPixels> GetDestination(DoorDefinition door) =>
        door.DestinationScreen.ToRoomPixels();

    /// <summary>Where the camera stands after the scroll setup's first update.</summary>
    internal static Position16<RoomPixels> GetSetupCamera(DoorDefinition door) =>
        GetDestination(door) + Ensure.Listed(DoorOpeningScrollDefinitions.SetupCameraOffsets, door.Orientation.Direction);

    /// <summary>
    /// The door setup places Samus at the setup camera plus her place within the screen she stood in, on both axes,
    /// preserving her fractions and deferring final nudges.
    /// </summary>
    internal static FixedPosition<RoomPixels> RebaseSamus(DoorDefinition door, FixedPosition<RoomPixels> setup)
    {
        var withinScreen = new Offset16<RoomPixels>(RoomGridConversions.OffsetWithinScreen(setup.X.Whole), RoomGridConversions.OffsetWithinScreen(setup.Y.Whole));
        return setup.WithWhole(GetSetupCamera(door) + withinScreen);
    }

    /// <summary>One scroll update's step; downward setup alone omits this displacement.</summary>
    internal static FixedPosition<RoomPixels> AdvanceSamus(DoorDefinition door, FixedPosition<RoomPixels> samus) =>
        samus.Advance(GetSamusStep(door));

    /// <summary>
    /// Samus's displacement for one scroll update, to a fraction of a pixel: the door's speed (or the
    /// axis default), added along right/down and subtracted along left/up.
    /// </summary>
    internal static FixedVector<RoomPixels> GetSamusStep(DoorDefinition door)
    {
        SignedSixteenSixteen step = door.SamusSpeed.Step(door.Orientation.Direction).ToSixteenSixteenDelta();
        SignedSixteenSixteen back = -step;
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
    /// Applies setup's first directional step before the door setup replaces the
    /// whole position. Downward setup readies a row without moving Samus.
    /// </summary>
    internal static FixedPosition<RoomPixels> ApplySetupMovement(
        DoorDefinition door, FixedPosition<RoomPixels> source) =>
        door.Orientation.Direction == DoorDirection.Down ? source : AdvanceSamus(door, source);

    /// <summary>The signed layer step of one scroll update along the travel axis.</summary>
    internal static Offset16<TSpace> LayerStep<TSpace>(DoorDirection direction)
        where TSpace : ISpatialSpace => direction switch
        {
            DoorDirection.Right => new(DoorOpeningScrollDefinitions.LayerStepPixels, 0),
            DoorDirection.Left => new(-DoorOpeningScrollDefinitions.LayerStepPixels, 0),
            DoorDirection.Down => new(0, DoorOpeningScrollDefinitions.LayerStepPixels),
            DoorDirection.Up => new(0, -DoorOpeningScrollDefinitions.LayerStepPixels),
            _ => throw new InvalidOperationException($"Undefined door direction {direction}."),
        };

    /// <summary>Runs one scroll update and reports whether it is the one that finishes the scroll.</summary>
    public bool Advance()
    {
        if (Scroll.IsOver)
            return true;

        CameraPosition += LayerStep<RoomPixels>(Direction);
        BackgroundPosition += LayerStep<BackgroundPixels>(Direction);
        SamusPosition = SamusPosition.Advance(SamusScrollStep);

        return Scroll.Count();
    }

    /// <summary>
    /// Places Samus on the elevator as the door loader does: whole X, and Y
    /// with a zero fraction. Later scroll updates move Samus on from this position.
    /// </summary>
    public void PlaceSamusOnElevator(Position16<RoomPixels> position) =>
        SamusPosition = new FixedPosition<RoomPixels>(
            SamusPosition.X.WithWhole(position.X),
            FixedCoordinate.FromWhole(position.Y));

    /// <summary>
    /// Snaps the camera to the door's destination after the final directional step has
    /// produced its last background request.
    /// </summary>
    public void SnapCameraToDestination()
    {
        if (Scroll.IsRunning)
            throw new InvalidOperationException(
                "A door-opening trajectory cannot snap before its final scroll update.");

        // The final directional step recalculates the layer scrolls first; only then is the
        // camera replaced with the exact door destination. The background reaches its endpoint
        // through the final directional step and is not rewritten by the snap.
        CameraPosition = FinalCameraPosition;
    }
}
