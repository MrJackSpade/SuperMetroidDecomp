namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The velocity an explosion that follows an actor leaves in its enemy projectile place (#1547): its offset from the
/// actor, each axis's pixels read as 256ths of a pixel an update. The explosion itself never moves by it; it keeps its
/// offset as its own state. A projectile that later takes the place and does not set its velocity starts from this one,
/// in the same room or, through <see cref="RoomHandOver"/>, in the next.
/// </summary>
/// <remarks>
/// Written once per explosion, when Mother Brain's death explosion or her rainbow-beam impact is spawned, before its
/// first placement. Evidence: both initializers store the requested offset in the projectile's velocity words, which
/// their pre-instructions add to the followed actor's position each update; a projectile's deletion and the next spawn into its place leave
/// the velocity words alone, so the next projectile there starts from them (<see cref="EnemyProjectileHandOver"/>), in that room or the next
/// (<see cref="RoomHandOver"/>).
/// </remarks>
[CompatibilityService("an explosion that follows an actor leaves its offset from that actor as its projectile place's velocity, which the place's next occupant inherits")]
public static class FollowingExplosionLeftVelocity
{
    /// <summary>The velocity the explosion leaves in its place: <paramref name="offset"/>'s pixels as 256ths of a pixel an update.</summary>
    /// <param name="offset">The explosion's offset from the actor it follows.</param>
    public static EightEightVector<RoomPixels> Of(Offset16<RoomPixels> offset) =>
        EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(offset.X, offset.Y);
}
