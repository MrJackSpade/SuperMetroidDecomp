namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A picture of a slot's helmet as it turns to face the player and shines (#2202, #2359).</summary>
internal enum FileSelectHelmetFrame
{
    /// <summary>Turned away: every helmet's resting picture.</summary>
    TurnedAway,
    /// <summary>Turning to face the player.</summary>
    Turning,
    /// <summary>The shine at the visor's left edge.</summary>
    ShineLeft,
    /// <summary>The shine left of the visor's middle.</summary>
    ShineLeftOfMiddle,
    /// <summary>The shine right of the visor's middle.</summary>
    ShineRightOfMiddle,
    /// <summary>The shine at the visor's right edge.</summary>
    ShineRight,
    /// <summary>A tall sparkle as the shine leaves.</summary>
    SparkleTall,
    /// <summary>The wide sparkle the turn settles on.</summary>
    SparkleWide,
}
