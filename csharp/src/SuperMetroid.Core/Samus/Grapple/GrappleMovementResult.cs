using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>One observable grapple update result.</summary>
/// <param name="Phase">The phase selected after the grapple update.</param>
/// <param name="Fired">Whether the update created a newly firing beam.</param>
/// <param name="OwnsMovement">Whether grapple continues to own Samus movement.</param>
/// <param name="WallJumpStarted">Whether release began the independent wall jump.</param>
/// <param name="CameraPrevious">Optional clamped previous Samus position published to camera tracking by the update.</param>
/// <param name="PendingDropPose">Optional pose requested after a dropped connection.</param>
/// <param name="PendingConnection">Optional deferred pose and geometry for a new connection.</param>
/// <param name="PendingReleasePose">Optional pose requested after releasing a swing.</param>
[IndependentFlag("independent outcomes of one grapple step")]
public readonly record struct GrappleMovementResult(
    GrapplePhase Phase,
    bool Fired = false,
    bool OwnsMovement = true,
    bool WallJumpStarted = false,
    Position16<RoomPixels>? CameraPrevious = null,
    SamusPoseId? PendingDropPose = null,
    GrapplePendingConnection? PendingConnection = null,
    SamusPoseId? PendingReleasePose = null);
