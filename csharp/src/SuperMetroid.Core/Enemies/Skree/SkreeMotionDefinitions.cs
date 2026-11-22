namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>How a Skree winds up, dives and burrows (#2076).</summary>
internal static partial class SkreeMotionDefinitions
{
    /// <summary>How close Samus comes horizontally, in pixels and exclusive, before an idling Skree winds up.</summary>
    internal const int WindUpDistance = 48;

    /// <summary>How far a diving Skree falls each update, and how far below it it looks for the floor, in pixels.</summary>
    internal const int DiveStep = 6;

    /// <summary>How far a diving Skree drifts toward Samus each update, in pixels (#2182).</summary>
    internal const int DiveDrift = 1;

    /// <summary>How far a burrowing Skree sinks into the floor each update, in pixels (#2182).</summary>
    internal const int BurrowSink = 1;

    /// <summary>How long a Skree burrows into the floor before it is gone, in updates.</summary>
    internal const int BurrowDuration = 21;

    /// <summary>The burrow updates left at which the Skree throws up its debris.</summary>
    internal const int DebrisBurstAt = 8;
}
