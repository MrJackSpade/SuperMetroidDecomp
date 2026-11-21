namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One section a growing shutter grows (#2553).</summary>
/// <param name="Artwork">The artwork the shutter shows while the section grows.</param>
/// <param name="GrownBefore">How many sections grew before it, which place its origin and give its radius.</param>
internal sealed record GrowingShutterSection(ShutterSegment Artwork, int GrownBefore);
