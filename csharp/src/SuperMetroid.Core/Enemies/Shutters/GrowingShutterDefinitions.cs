namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>How a growing shutter's sections extend (#2156).</summary>
internal static partial class GrowingShutterDefinitions
{
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
