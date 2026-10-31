namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How a Boulder rolls once it falls (#1472).</summary>
public enum BoulderRoll
{
    /// <summary>It rolls right.</summary>
    Right,

    /// <summary>It rolls left.</summary>
    Left,

    /// <summary>It rolls left and breaks on its first impact.</summary>
    LeftBreakingOnImpact,
}
