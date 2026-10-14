namespace SuperMetroid.Core.Game;

/// <summary>
/// The Zebetites' health-tier programs, in bank $A6 order: five for the large barrier, then five for the halves of
/// a split pair. Each shows its tier's pose once and sleeps.
/// </summary>
public enum ZebetiteProgram
{
    /// <summary><c>InstList_Big_HealthGreaterThanEqualTo800</c> (<c>$A6:FDCC</c>): large barrier, health 800 or more.</summary>
    LargeAtLeast800,
    /// <summary>Large barrier, health 600 to 799 (<c>$A6:FDD2</c>).</summary>
    LargeAtLeast600,
    /// <summary>Large barrier, health 400 to 599 (<c>$A6:FDD8</c>).</summary>
    LargeAtLeast400,
    /// <summary>Large barrier, health 200 to 399 (<c>$A6:FDDE</c>).</summary>
    LargeAtLeast200,
    /// <summary>Large barrier, health below 200 (<c>$A6:FDE4</c>).</summary>
    LargeBelow200,
    /// <summary>Split pair, health 800 or more (<c>$A6:FDEA</c>).</summary>
    SplitAtLeast800,
    /// <summary>Split pair, health 600 to 799 (<c>$A6:FDF0</c>).</summary>
    SplitAtLeast600,
    /// <summary>Split pair, health 400 to 599 (<c>$A6:FDF6</c>).</summary>
    SplitAtLeast400,
    /// <summary>Split pair, health 200 to 399 (<c>$A6:FDFC</c>).</summary>
    SplitAtLeast200,
    /// <summary>Split pair, health below 200 (<c>$A6:FE02</c>).</summary>
    SplitBelow200,
}
