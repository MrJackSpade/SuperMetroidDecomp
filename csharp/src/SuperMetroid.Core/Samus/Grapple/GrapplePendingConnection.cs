using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The prospective pose a connecting grapple publishes. The grapple sets
/// its phase, angle, and rope geometry immediately, while Samus's pose handling applies this pose
/// later only if no interruption that outranks it (an animation's transition pose, a hit, a bomb jump, a shinespark
/// pose or X-ray's activation) changed her pose that frame.
/// </summary>
/// <param name="Pose">The prospective connected pose.</param>
/// <param name="Kind">The connection's kind (#2560): swinging, or locked in place.</param>
/// <param name="Previous">The previous whole-pixel Samus position retained for movement resolution.</param>
public readonly record struct GrapplePendingConnection(
    SamusPoseId Pose,
    GrappleConnectionKind Kind,
    Position16<RoomPixels> Previous);
