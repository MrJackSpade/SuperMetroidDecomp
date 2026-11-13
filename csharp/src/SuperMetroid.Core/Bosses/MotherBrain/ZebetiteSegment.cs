namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Zebetites' health-tier programs, order: five for the large barrier, then five for the halves of
/// a split pair. Each shows its tier's pose once and sleeps.
/// </summary>
public enum ZebetiteSegment
{
    /// <summary>Large barrier, health 800 or more.</summary>
    LargeAtLeast800,
    /// <summary>Large barrier, health 600 to 799.</summary>
    LargeAtLeast600,
    /// <summary>Large barrier, health 400 to 599.</summary>
    LargeAtLeast400,
    /// <summary>Large barrier, health 200 to 399.</summary>
    LargeAtLeast200,
    /// <summary>Large barrier, health below 200.</summary>
    LargeBelow200,
    /// <summary>Split pair, health 800 or more.</summary>
    SplitAtLeast800,
    /// <summary>Split pair, health 600 to 799.</summary>
    SplitAtLeast600,
    /// <summary>Split pair, health 400 to 599.</summary>
    SplitAtLeast400,
    /// <summary>Split pair, health 200 to 399.</summary>
    SplitAtLeast200,
    /// <summary>Split pair, health below 200.</summary>
    SplitBelow200,
}
