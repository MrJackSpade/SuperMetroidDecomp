namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a Samus projectile's animation stands (#1600): the segment it plays, and how many of that segment's beats it has
/// played, from none, before its first, to all of them, at the segment's end where its play continues or ends. The
/// import decodes the glitch paths' landings and overwrites by these places.
/// </summary>
/// <param name="Segment">The segment the animation plays.</param>
/// <param name="Beat">The segment's beats played so far: its next beat, or its beat count at its end.</param>
internal readonly record struct SamusProjectileSegmentBeat(SamusProjectileSegment Segment, int Beat);
