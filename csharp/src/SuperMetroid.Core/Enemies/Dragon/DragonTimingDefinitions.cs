namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>How long a Dragon rises, sinks and waits, and how many fireballs one attack spits (#2279).</summary>
internal static partial class DragonTimingDefinitions
{
    /// <summary>How long a Dragon rises or sinks, in updates, a pixel each, the last the one its rise or sink ends on.</summary>
    internal const int RiseOrSinkDuration = 49;

    /// <summary>The fireballs one attack spits.</summary>
    internal const int ShotsPerAttack = 3;

    /// <summary>How long a Dragon waits after its attack before it sinks, in updates.</summary>
    internal const int WaitBeforeSink = 96;

    /// <summary>
    /// How long a Dragon waits beneath the lava before its next rise, in updates, the last the one it turns to Samus on.
    /// A Dragon placed in the room starts with its wait already run out and rises at once.
    /// </summary>
    internal const int WaitBeforeRise = 129;
}
