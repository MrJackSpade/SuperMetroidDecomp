namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the camera moves, and how places are measured from it, at the room's top and left edges (#2568): a camera step
/// that carries the view above or left of the room's origin leaves it counted back from the far end of the room's
/// coordinates, where the camera's next clamp reads it as lying before the room and returns it to the edge; the camera's
/// comparisons with its ideal place and its scroll boundaries are taken by the sign of their difference, the short way;
/// and a place's distance from the camera, which decides whether an enemy, a projectile or a shinespark echo is on
/// screen and where a boss's body and line scrolls are drawn from, is the short-way difference too, so an actor past the
/// room's top or left edge lies a short way off the view rather than far beyond its right or bottom.
/// </summary>
/// <remarks>
/// Consulted by the camera's tracking, its scroll-zone handling and its grapple slow scroll on every update that moves
/// it, after Samus's movement; by the background's scroll as it follows the foreground; and by every view and
/// processing-window test, deletion test and screen place that measures from the camera, in the update of the actor
/// tested. Evidence: the original's scroll-zone checks (<c>$80:A528</c>, <c>$80:A641</c>, <c>$80:A6BB</c>,
/// <c>$80:A731</c>, <c>$80:A893</c>, <c>$80:A936</c>) clear the layer's position only when its word reads negative and
/// compare each proposed position with a scroll boundary by a sixteen-bit subtraction's sign; the enemy and projectile
/// view tests subtract the layer's position from the actor's as words; the Ridley full-movie and 100% recorded movies
/// replay the camera's place on every update.
/// </remarks>
[CompatibilityService("the camera carried above or left of the room's origin reads as lying before the room and is clamped back, and places are measured from the camera the short way across the ends of the room's coordinates")]
public static class CameraPastRoomEdge
{
    /// <summary>
    /// The camera coordinate a step lands on: a step past the room's top or left continues from the far end of the room's
    /// coordinates, 0 through 65535.
    /// </summary>
    /// <param name="coordinate">The coordinate computed, which may lie before the room or past the coordinates' far end.</param>
    public static int At(int coordinate) => CoordinateAxis.Wrap(coordinate);

    /// <summary>
    /// How far <paramref name="place"/> lies past <paramref name="camera"/> on one axis, -32768 through 32767 pixels:
    /// negative above or left of the view's top-left corner, taken the short way across the ends of the room's
    /// coordinates.
    /// </summary>
    /// <param name="place">The place measured, which may be computed past either end of the room's coordinates.</param>
    /// <param name="camera">The camera's coordinate, or a place the camera is compared with.</param>
    public static int FromCamera(int place, int camera) => CoordinateAxis.Difference(place, camera);

    /// <summary>
    /// How far <paramref name="camera"/> lies past <paramref name="place"/> on one axis, -32768 through 32767 pixels: the
    /// pixel of a body placed at <paramref name="place"/> that the view's top-left corner shows, negative while the body
    /// lies right of or below the corner, taken the short way as <see cref="FromCamera"/> takes it.
    /// </summary>
    /// <param name="camera">The camera's coordinate.</param>
    /// <param name="place">The coordinate the body is placed against.</param>
    public static int CameraPast(int camera, int place) => CoordinateAxis.Difference(camera, place);

    /// <summary>
    /// Whether <paramref name="coordinate"/> lies above or left of <paramref name="reference"/>, the sign of their
    /// short-way difference (<see cref="FromCamera"/>).
    /// </summary>
    /// <param name="coordinate">The coordinate tested, which may be computed past either end of the room's coordinates.</param>
    /// <param name="reference">The coordinate it is compared with, which may be computed past either end of the room's coordinates.</param>
    public static bool LiesBefore(int coordinate, int reference) => CoordinateAxis.Precedes(coordinate, reference);

    /// <summary>Whether a camera or tracked coordinate lies above or left of the room's origin: it reads below zero.</summary>
    /// <param name="coordinate">The coordinate, 0 through 65535.</param>
    public static bool LiesBeforeRoom(int coordinate) => CoordinateAxis.AsSigned(coordinate) < 0;

    /// <summary>
    /// A scroll coordinate read as the signed scroll it stands for, -32768 through 32767: a scroll counted back from the far
    /// end of the coordinates reads as that many pixels above or left of the origin.
    /// </summary>
    /// <param name="coordinate">The scroll coordinate computed, which may lie past either end of the coordinates.</param>
    public static int Signed(int coordinate) => CoordinateAxis.AsSigned(coordinate);

}
