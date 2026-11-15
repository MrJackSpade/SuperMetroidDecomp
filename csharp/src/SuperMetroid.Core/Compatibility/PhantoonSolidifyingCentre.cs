namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The single pixel at the centre of Phantoon's body that absorbs a shot or a bomb on the update his body turns solid
/// (#1937). When his eye starts to track Samus, his body stops ignoring her at once but shows its eye-hitbox look only from
/// the update after; on the update between, his body still shows the invulnerable look, whose only hitbox is this point.
/// A beam, missile or bomb striking that exact pixel then is spent, a Super Missile shaking the room, though nothing on him
/// is hit; Samus's touch there does nothing.
/// </summary>
/// <remarks>
/// Consulted by Phantoon's shot resolver and the enemies' bomb pass through the hitboxes of his invulnerable look, which
/// they read only while his body does not ignore Samus. Evidence: the invulnerable look's hitbox list in his frame data is
/// one zero-sized rectangle whose touch and shot routines return at once; his eye-tracking cue clears the body's ignoring
/// property and installs the eye-hitbox animation for the next update, and his projectile collision runs before his body's
/// animation on each update. His eye, tentacles and mouth always ignore Samus, so the same point on their looks is never read.
/// </remarks>
[CompatibilityService("a shot or a bomb at the centre of Phantoon's body is spent on the update his body turns solid")]
public static class PhantoonSolidifyingCentre
{
    /// <summary>
    /// The point, relative to his body's origin, that his invulnerable look's one hitbox covers: it absorbs a shot or a
    /// bomb and ignores Samus's touch.
    /// </summary>
    public static Offset16<RoomPixels> Centre { get; } = Offset16<RoomPixels>.Zero;
}
