using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How fast Ridley's wings beat for the speed he flies at, and the pose each Ridley's wing beat starts in (#2232, #2329).</summary>
internal static partial class RidleyWingDefinitions
{
    /// <summary>
    /// The countdown each update takes off the wing beat for Ridley's speed, by the whole quarter pixels of his faster axis,
    /// slowest first; the last serves every faster speed. Flight that does not climb beats at half the selected pace.
    /// </summary>
    internal static readonly int[] BeatPaces = [12, 14, 16, 18, 28, 32, 40, 48];

    /// <summary>The wing countdown each beat starts from: the beat steps once the countdown runs below zero.</summary>
    internal const int BeatCountdown = 32;

    /// <summary>The wing countdown as Ridley takes off, before his speed paces the beat.</summary>
    internal const int TakeOffCountdown = 8;

    /// <summary>The pose Ceres Ridley's wings show first: fully lowered.</summary>
    internal static RidleyWingPose CeresFirstPose => RidleySupplementalVisualDefinitions.WingsDown;

    /// <summary>The pose Lower Norfair Ridley's wings show first: fully raised.</summary>
    internal static RidleyWingPose NorfairFirstPose => RidleySupplementalVisualDefinitions.WingsRaised;
}
