namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a Work Robot must stand for its laser to request the laser sound (#2186): its body must reach into the
/// camera's columns, its bottom edge must reach the camera's top or below it, and its top edge must lie less than a
/// screen's height below the top of the room, wherever the camera stands vertically. A robot standing lower in its room
/// fires silently even while it is on screen, and one in the room's top screen's height is heard while the camera looks
/// at it from below. The request enters the enemies' sound queue as any normal-priority sound does, so a request made
/// or withheld here changes which later sounds that queue admits and when it empties, which the door transition's wait
/// for empty sound queues reads.
/// </summary>
/// <remarks>
/// Consulted once each time a robot fires a laser, after the laser is placed at the muzzle, during the robot's main
/// update in the enemy pass; a static rule with no state. Evidence: the lasers' common setup (<c>$86:D35B</c>) tests the
/// robot's left and right edges against the camera's left edge and a screen's width past it, its bottom edge against
/// the camera's top, and whether its top edge lies a screen's height below the room's top by the sign of the difference
/// alone, with no comparison against the camera's top (<c>$86:D3B1</c>), and queues the sound
/// through the six-request limit (<c>$86:D3B9</c>); the sound queues' admission (<see cref="SoundQueueAdmission"/>) is what
/// carries the request into gameplay. The Work Robot laser sound range check compares every edge class with the
/// original's word arithmetic and fires the production laser from a robot on screen above and below the room's top
/// screen.
/// </remarks>
[CompatibilityService("a Work Robot's laser requests its sound only while the robot reaches into the camera's columns, reaches down to the camera's top and has its top edge less than a screen's height below the room's top, so a robot lower in its room fires silently")]
internal static class WorkRobotLaserSoundRange
{
    /// <summary>
    /// Whether a laser fired by a robot at <paramref name="robot"/>, reaching <paramref name="radius"/> from it on each
    /// axis, requests its sound while the camera stands at <paramref name="camera"/>.
    /// </summary>
    /// <param name="robot">The robot's position in whole room pixels.</param>
    /// <param name="radius">How far the robot's body reaches from its position on each axis.</param>
    /// <param name="camera">The camera's top-left corner in whole room pixels.</param>
    internal static bool RequestsSound(Position16<RoomPixels> robot, Extent16<RoomPixels> radius, Position16<RoomPixels> camera) =>
        CoordinateAxis.AsSigned(robot.X + radius.X - camera.X) >= 0 &&
        CoordinateAxis.Difference(robot.X - radius.X - (ScreenGeometry.ScreenWidthPixels + 1), camera.X) < 0 &&
        CoordinateAxis.AsSigned(robot.Y + radius.Y - camera.Y) >= 0 &&
        CoordinateAxis.AsSigned(robot.Y - radius.Y - ScreenGeometry.ScreenHeightPixels) < 0;
}
