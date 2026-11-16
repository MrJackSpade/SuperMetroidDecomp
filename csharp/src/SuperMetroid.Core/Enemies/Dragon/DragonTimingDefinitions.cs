namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>How long a Dragon rises, sinks and waits, and how many fireballs one attack spits (#2279).</summary>
internal static class DragonTimingDefinitions
{
    /// <summary>The count a Dragon's rise or sink starts its timer at.</summary>
    internal const int RiseOrSinkCount = 48;

    /// <summary>The fireballs one attack spits.</summary>
    internal const int ShotsPerAttack = 3;

    /// <summary>How long a Dragon waits after its attack before it sinks, in updates.</summary>
    internal const int WaitBeforeSink = 96;

    /// <summary>The count a Dragon's wait beneath the lava before its next rise starts its timer at.</summary>
    internal const int WaitBeforeRiseCount = 128;
}
