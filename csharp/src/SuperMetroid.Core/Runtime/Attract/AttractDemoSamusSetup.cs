namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>How a demo scene stages Samus once its room has loaded.</summary>
public enum AttractDemoSamusSetup
{
    /// <summary>Front-facing Landing Site arrival.</summary>
    LandingSite,
    /// <summary>Left-facing grounded morph ball.</summary>
    MorphLeft,
    /// <summary>Low health, standing left.</summary>
    LowHealthLeft,
    /// <summary>Standing left.</summary>
    StandingLeft,
    /// <summary>Falling left.</summary>
    FallingLeft,
    /// <summary>Standing right.</summary>
    StandingRight,
    /// <summary>Immediate diagonal-right shinespark.</summary>
    DiagonalShinespark,
    /// <summary>Immediate horizontal-left shinespark.</summary>
    HorizontalShinespark,
}
