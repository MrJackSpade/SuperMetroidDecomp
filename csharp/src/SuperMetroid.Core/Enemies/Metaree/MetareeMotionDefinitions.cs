namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>When a Metaree dives, how it steers and how long it burrows (#2156).</summary>
internal static partial class MetareeMotionDefinitions
{
    /// <summary>How close horizontally Samus comes, in pixels and exclusive, for a Metaree to dive, whichever side she is on vertically.</summary>
    internal const int ActivationDistance = 72;

    /// <summary>How far a diving Metaree steers toward Samus each airborne update, in whole pixels.</summary>
    internal const int DiveSteer = 2;

    /// <summary>How long a Metaree that has struck the ground burrows before it deletes itself, in updates.</summary>
    internal const int BurrowLifetime = 21;

    /// <summary>The burrow countdown at which a Metaree bursts into particles.</summary>
    internal const int BurstCountdown = 8;

    /// <summary>How far a burrowing Metaree sinks into the ground each update, in whole pixels (#2375).</summary>
    internal const int BurrowSpeed = 1;
}
