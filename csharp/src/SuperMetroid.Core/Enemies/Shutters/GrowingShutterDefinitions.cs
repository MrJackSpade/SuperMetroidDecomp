namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>How a growing shutter's sections extend (#2156).</summary>
internal static partial class GrowingShutterDefinitions
{
    /// <summary>
    /// The growth sections in the order they grow, each by the artwork the shutter shows while it grows, a quarter, a half,
    /// three quarters and the whole of its length (#2553), and how many sections grew before it.
    /// </summary>
    internal static readonly GrowingShutterSection[] Sections =
    [
        new(ShutterSegment.QuarterGrown, GrownBefore: 0),
        new(ShutterSegment.HalfGrown, GrownBefore: 1),
        new(ShutterSegment.ThreeQuartersGrown, GrownBefore: 2),
        new(ShutterSegment.FullyGrown, GrownBefore: 3),
    ];

    /// <summary>How far each growing section advances before its final placement, in pixels.</summary>
    internal const int SectionLength = 16;

    /// <summary>How far inside its full-size end an intermediate section stops, in pixels.</summary>
    internal const int IntermediateInset = 7;

    /// <summary>
    /// The vertical radius each section gives the shutter, in pixels (#2182): the first section's alone, and each grown
    /// section adds as much again.
    /// </summary>
    internal const int SectionRadius = 8;
}
