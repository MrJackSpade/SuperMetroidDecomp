namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Where Samus wakes and steers an Etecoon, and the room columns its teaching routes turn at (#2279).</summary>
internal static class EtecoonRouteDefinitions
{
    /// <summary>The teaching hops an Etecoon counts before the count empties (#2369).</summary>
    internal const int TeachingHopsPerLesson = 4;

    /// <summary>The vertical distance, in pixels, Samus must come strictly within to wake a dormant Etecoon.</summary>
    internal const int ActivationYDistance = 128;

    /// <summary>
    /// The distance, in pixels on each axis, Samus must come strictly within for a hopping Etecoon to look at her, and
    /// vertically for a waiting one to start its long run.
    /// </summary>
    internal const int NearbyDistance = 64;

    /// <summary>
    /// The band, in pixels on each axis, within which Samus counts as straight beside or straight above or below an Etecoon
    /// rather than diagonal to it.
    /// </summary>
    internal const int DirectionBand = 32;

    /// <summary>The horizontal distance, in pixels, Samus must come strictly within for a waiting Etecoon to start its long run.</summary>
    internal const int LongRunTriggerDistance = 48;

    /// <summary>The room column at which the leftward route to the teaching spot stops and starts hopping.</summary>
    internal const int FirstTeachingX = 537;

    /// <summary>
    /// The room column at which the rightward route to the teaching spot stops, and from which the long and return runs jump.
    /// </summary>
    internal const int RunStartX = 600;

    /// <summary>The room column at which the long jump turns back into a run.</summary>
    internal const int LongJumpMidpointX = 680;

    /// <summary>The room column at which the long route jumps to land.</summary>
    internal const int LongJumpEndX = 840;

    /// <summary>The room column short of which a teach-at-once Etecoon waits for Samus before another teaching hop.</summary>
    internal const int LongRunMaximumX = 832;

    /// <summary>The room column from which on Samus hears the Etecoons' hops and wall jumps.</summary>
    internal const int AudibleFromSamusX = 256;
}
