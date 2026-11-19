namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where an actor stands when the game places or moves it above or left of the room (#2568): its place is counted back
/// from the far end of the room's coordinates, 80 pixels left of the room standing at 65456, and the tests that turn,
/// park or delete an actor read such a place as lying before the room (a signed reading below zero) while every
/// comparison with a place inside the room measures it the short way (<see cref="DistanceAcrossRoomEdge"/>). Draygon
/// parks 80 pixels left of and above his arena between passes, Phantoon's swoop holds him as far as 64 pixels left of his
/// room, the gunship climbs out past the top of Crateria's landing site, Mother Brain's Baby Metroid leaves 128 pixels left
/// of her room, the Tourian statue's particles and the room's projectiles move past the top or left before they are
/// deleted there, and Samus's beam combos circle her past the room's edges; each keeps its place this way, and its
/// next test reads it as before the room.
/// </summary>
/// <remarks>
/// Consulted wherever an actor's place is set or moved by an amount that can carry it past the room's top or left edge,
/// in the update of the actor that owns the place, and wherever its off-room test reads that place back in the same or a
/// later update. Evidence: the original keeps every actor's place as an unclamped sixteen-bit word and tests leaving the
/// room by the word's sign; Draygon's leftward goop pass (<c>$A5:8DB2</c>) ends once his X reads negative and below
/// <c>$FFB0</c>, and then stores <c>$FFB0</c> as both his X and his Y; the Draygon, Phantoon and Mother Brain recorded
/// movies replay those passes and exits on every update.
/// </remarks>
[CompatibilityService("an actor placed or moved above or left of the room stands at a place counted back from the far end of the room's coordinates, which its off-room tests read as lying before the room")]
public static class OffRoomPlaces
{
    /// <summary>
    /// The place an actor stands at when it is placed at <paramref name="coordinate"/>, which may lie above or left of the
    /// room (below zero) or past the far end of the room's coordinates: 0 through 65535, a place before the room counted
    /// back from 65535.
    /// </summary>
    /// <param name="coordinate">The place computed, in room pixels, which may lie before the room.</param>
    public static int Place(int coordinate) => CoordinateAxis.Wrap(coordinate);

    /// <summary>
    /// The place an actor stands at when the room places it at <paramref name="signedPlace"/>, a place that may lie above
    /// or left of the room, each axis as <see cref="Place"/> keeps it.
    /// </summary>
    /// <typeparam name="TSpace">The place's coordinate space.</typeparam>
    /// <param name="signedPlace">The place, negative above or left of the room's origin.</param>
    public static Position16<TSpace> Placed<TSpace>(Offset16<TSpace> signedPlace)
        where TSpace : ISpatialSpace => Position16<TSpace>.Wrapped(signedPlace.X, signedPlace.Y);

    /// <summary>
    /// <paramref name="position"/> with its whole horizontal place moved to <paramref name="whole"/>, which may lie left of
    /// the room, as <see cref="Place"/> keeps it; the fraction is kept.
    /// </summary>
    /// <typeparam name="TSpace">The place's coordinate space.</typeparam>
    /// <param name="position">The actor's place.</param>
    /// <param name="whole">The whole horizontal place computed.</param>
    public static FixedPosition<TSpace> WithWholeX<TSpace>(FixedPosition<TSpace> position, int whole)
        where TSpace : ISpatialSpace => position.WithWrappedWholeX(whole);

    /// <summary>
    /// <paramref name="position"/> with its whole vertical place moved to <paramref name="whole"/>, which may lie above the
    /// room, as <see cref="Place"/> keeps it; the fraction is kept.
    /// </summary>
    /// <typeparam name="TSpace">The place's coordinate space.</typeparam>
    /// <param name="position">The actor's place.</param>
    /// <param name="whole">The whole vertical place computed.</param>
    public static FixedPosition<TSpace> WithWholeY<TSpace>(FixedPosition<TSpace> position, int whole)
        where TSpace : ISpatialSpace => position.WithWrappedWholeY(whole);

    /// <summary><paramref name="coordinate"/> with its whole pixels moved to <paramref name="whole"/>, as <see cref="Place"/> keeps it; the fraction is kept.</summary>
    /// <param name="coordinate">One axis of the actor's place.</param>
    /// <param name="whole">The whole place computed, which may lie before the room.</param>
    public static FixedCoordinate WithWhole(FixedCoordinate coordinate, int whole) => coordinate.WithWrappedWhole(whole);

    /// <summary>Whether an actor at <paramref name="place"/> lies above or left of the room: its place reads below zero.</summary>
    /// <param name="place">One axis of the actor's place, 0 through 65535.</param>
    public static bool LiesBeforeRoom(int place) => CoordinateAxis.AsSigned(place) < 0;

    /// <summary>
    /// <paramref name="place"/> read as the signed place it stands for: a place counted back from the far end of the room's
    /// coordinates reads as that many pixels above or left of the room, -32768 through 32767.
    /// </summary>
    /// <param name="place">One axis of the actor's place, 0 through 65535.</param>
    public static int Signed(int place) => CoordinateAxis.AsSigned(place);

    /// <summary><paramref name="place"/> read as the signed place it stands for, each axis as <see cref="Signed(int)"/> reads it.</summary>
    /// <typeparam name="TSpace">The place's coordinate space.</typeparam>
    /// <param name="place">The actor's place.</param>
    public static Offset16<TSpace> Signed<TSpace>(Position16<TSpace> place)
        where TSpace : ISpatialSpace => new(CoordinateAxis.AsSigned(place.X), CoordinateAxis.AsSigned(place.Y));
}
