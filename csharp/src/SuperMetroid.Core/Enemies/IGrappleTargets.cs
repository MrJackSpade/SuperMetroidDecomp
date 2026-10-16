using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Enemies;

/// <summary>Resolves the grapple beam's endpoint against the room's enemies.</summary>
internal interface IGrappleTargets
{
    /// <summary>The enemy collision at <paramref name="endpoint"/>.</summary>
    GrappleEnemyCollision ResolveGrappleEndpoint(Position16<RoomPixels> endpoint);
}
