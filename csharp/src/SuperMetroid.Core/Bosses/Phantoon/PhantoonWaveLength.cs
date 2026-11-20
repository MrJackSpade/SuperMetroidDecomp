namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How many screen lines one whole cycle of Phantoon's wave spans.</summary>
public enum PhantoonWaveLength
{
    /// <summary>The short wave the introduction ripples with (<see cref="PhantoonWaveDefinitions.ShortPeriod"/>).</summary>
    Short,
    /// <summary>The long wave the final death ripples with (<see cref="PhantoonWaveDefinitions.LongPeriod"/>).</summary>
    Long,
}
