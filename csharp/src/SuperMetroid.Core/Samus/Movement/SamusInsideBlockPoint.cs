namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The body point whose block an inside-block reaction is running for.</summary>
public enum SamusInsideBlockPoint
{
    /// <summary>The first sample point: one pixel above the bottom boundary.</summary>
    Bottom,

    /// <summary>The second sample point: Samus's Y position.</summary>
    Center,

    /// <summary>The third sample point: Y position minus the Y radius.</summary>
    Top,
}
