namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>How long a Wrecked Ship Spark's actions last past its placement's base time (#2182).</summary>
internal static partial class SparkTimingDefinitions
{
    /// <summary>How much longer than its base action time an intermittent Spark stays out, in updates.</summary>
    internal const int ExtraTimeOut = 8;
}
