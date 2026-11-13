using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The prospective pose a connecting grapple publishes. The grapple installs
/// the phase, angle, and rope geometry immediately, while Samus's pose handling applies this pose
/// later only if a higher-priority super-special transition did not win the frame.
/// </summary>
/// <param name="Pose">The prospective connected pose.</param>
/// <param name="Swinging">Whether the connection starts in swinging rather than locked mode.</param>
/// <param name="Previous">The previous whole-pixel Samus position retained for movement resolution.</param>
public readonly record struct GrapplePendingConnection(
    SamusPoseId Pose,
    bool Swinging,
    Position16<RoomPixels> Previous);
