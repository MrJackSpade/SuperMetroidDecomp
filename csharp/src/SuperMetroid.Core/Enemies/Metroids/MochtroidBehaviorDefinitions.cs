namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How a Mochtroid flies, holds on to Samus and drains her (#2156).</summary>
internal static class MochtroidBehaviorDefinitions
{
    /// <summary>How many updates of contact an attached Mochtroid counts between its drains of Samus's health.</summary>
    internal const int AttachedDrainPeriod = 80;

    /// <summary>The fastest a free Mochtroid flies on each axis, in whole pixels an update.</summary>
    internal const int FastestFlight = 3;

    /// <summary>How fast an attached Mochtroid moves toward Samus on each axis: one pixel an update.</summary>
    internal static SignedSixteenSixteen AttachedSpeed { get; } = SignedSixteenSixteen.FromWholePixels(1);
}
