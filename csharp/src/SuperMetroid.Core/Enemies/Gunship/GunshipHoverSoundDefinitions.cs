namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The landed gunship's engine hum (#2043): how soon it first sounds and how often it repeats.</summary>
internal static class GunshipHoverSoundDefinitions
{
    /// <summary>The updates before the landed ship's first hover sound, counted from its placement.</summary>
    internal const int FirstDelay = 71;

    /// <summary>The updates between the landed ship's hover sounds.</summary>
    internal const int Interval = 70;
}
