namespace SuperMetroid.Core.Game;

/// <summary>
/// The prospective pose published by <c>HandleConnectingGrapple</c>. Bank $9B installs
/// the function, angle, and rope geometry immediately, while bank $91 applies this pose
/// later only if a higher-priority super-special transition did not win the frame.
/// </summary>
/// <param name="Pose">The prospective connected pose.</param>
/// <param name="Swinging">Whether the connection starts in swinging rather than locked mode.</param>
/// <param name="Previous">The previous whole-pixel Samus position retained for movement resolution.</param>
public readonly record struct GrapplePendingConnection(
    SamusPoseId Pose,
    bool Swinging,
    Position16<RoomPixels> Previous);
