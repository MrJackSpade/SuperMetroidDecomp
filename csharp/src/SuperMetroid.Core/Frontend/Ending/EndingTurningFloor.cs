namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The view of the ending's turning floor, which the escape, the explosion and the planet escape share (#2541): where it
/// stands, how far it is zoomed and turned. Each scene sets it up at its start and moves it by its own operations; nothing
/// else writes it.
/// </summary>
internal sealed partial class EndingTurningFloor
{
    /// <summary>Where the floor stands: it moves across the screen by fractions of a pixel and up and down by whole pixels.</summary>
    internal FixedVector<FloorPixels> Position { get; set; }

    /// <summary>
    /// The floor's zoom, initially its own size. Every scene starts it at an authored zoom and moves it a little each
    /// update toward a bound it stops at, so it stays positive and small.
    /// </summary>
    internal FloorZoom Zoom { get; set; } = FloorZoom.OwnSize;

    /// <summary>How far the floor is turned.</summary>
    internal RotationAngle Angle { get; private set; }

    /// <summary>
    /// Sets the floor up for a scene: at the whole pixels <paramref name="place"/>, keeping the fraction of a pixel its
    /// horizontal place had, zoomed to <paramref name="zoom"/> and turned to <paramref name="angle"/>.
    /// </summary>
    internal void SetUp(Offset16<FloorPixels> place, FloorZoom zoom, RotationAngle angle)
    {
        Position = new(Position.X.WithWholePixels(place.X), SignedSixteenSixteen.FromWholePixels(place.Y));
        Zoom = zoom;
        Angle = angle;
    }

    /// <summary>Turns the floor by <paramref name="steps"/> steps of a turn.</summary>
    internal void Turn(int steps) => Angle = Angle.TurnedBySteps(steps);

    /// <summary>Widens the floor's zoom by <paramref name="step"/>.</summary>
    internal void Widen(FloorZoomStep step) => Zoom = Zoom.Widened(step);

    /// <summary>Narrows the floor's zoom by <paramref name="step"/>.</summary>
    internal void Narrow(FloorZoomStep step) => Zoom = Zoom.Narrowed(step);

    /// <summary>Moves the floor across by <paramref name="displacement"/>.</summary>
    internal void MoveAcross(SignedSixteenSixteen displacement) => Position = Position.WithX(Position.X + displacement);
}
