namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Schema generations of the installed Ridley/Baby RGB5 palette document.</summary>
public enum CeresRidleyColorVersion
{
    /// <summary>Legacy schema version one, lacking Baby and alarm rows; loading requires a stock catalog to provide both families.</summary>
    PreBaby = 1,
    /// <summary>Legacy schema version two, containing Baby rows but lacking alarm rows; loading requires a stock catalog for the alarm family.</summary>
    PreAlarm = 2,
    /// <summary>Legacy schema version three, complete but holding the Baby's colours as four ordered steps rather than by shade.</summary>
    BabySteps = 3,
    /// <summary>Current schema version four, requiring the complete startup, fade, health, retreat, alarm and Baby palette families, the Baby's colours named by shade.</summary>
    Current = 4,
}
