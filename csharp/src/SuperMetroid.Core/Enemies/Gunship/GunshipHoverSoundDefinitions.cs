namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's engine hum (#2043): how soon it first sounds and how often it repeats.</summary>
internal static class GunshipHoverSoundDefinitions
{
    /// <summary>The updates before the landed ship's first hover sound, counted from its placement, the one it sounds on included.</summary>
    internal const int FirstDelay = 71;

    /// <summary>
    /// The updates before the ship descending after Ceres first sounds, the one it sounds on included: it sounds on its
    /// first update (#2373).
    /// </summary>
    internal const int FirstDelayAfterCeres = 1;

    /// <summary>The updates between the ship's hover sounds.</summary>
    internal const int Interval = 70;
}
