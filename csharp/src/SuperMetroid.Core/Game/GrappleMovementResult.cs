namespace SuperMetroid.Core.Game;

/// <summary>One observable bank-$9B grapple-function result.</summary>
/// <param name="Phase">The phase selected after the grapple update.</param>
/// <param name="Fired">Whether the update created a newly firing beam.</param>
/// <param name="OwnsMovement">Whether grapple continues to own Samus movement.</param>
/// <param name="TerrainCollided">Whether the body sweep collided with terrain.</param>
/// <param name="CollisionDistanceFromFeet">Native body-sweep countdown at the collision point.</param>
/// <param name="RopeLengthBlocked">Whether collision prevented the requested rope-length change.</param>
/// <param name="WallJumpStarted">Whether release began the independent wall-jump handler.</param>
/// <param name="CameraPrevious">Optional clamped previous Samus position published to camera tracking by the update.</param>
/// <param name="PendingDropPose">Optional pose requested after a dropped connection.</param>
/// <param name="PendingConnection">Optional deferred pose and geometry for a new connection.</param>
/// <param name="PendingReleasePose">Optional pose requested after releasing a swing.</param>
public readonly record struct GrappleMovementResult(
    GrapplePhase Phase,
    bool Fired = false,
    bool OwnsMovement = true,
    bool TerrainCollided = false,
    int CollisionDistanceFromFeet = 0,
    bool RopeLengthBlocked = false,
    bool WallJumpStarted = false,
    Position16<RoomPixels>? CameraPrevious = null,
    SamusPoseId? PendingDropPose = null,
    GrapplePendingConnection? PendingConnection = null,
    SamusPoseId? PendingReleasePose = null);
